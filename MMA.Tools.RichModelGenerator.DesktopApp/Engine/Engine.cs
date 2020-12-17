using MMA.Tools.RichModelGenerator.DesktopApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp
{
    public class Engine
    {
        public static void Start(List<TableDesigner> tablesSchemes, List<Relation> relations, string solutionName)
        {

            var tables = GetTables(tablesSchemes, relations);

            var entities = tables.Select(t => ClassGenerate(t, solutionName)).ToList();
            var models = tables.Select(t => ModelsGenerate(t, solutionName)).ToList();
            var validators = tables.Select(t => ValidatorGenerate(t, solutionName)).ToList();
            var configurations = tables.Select(t => EntityConfigurationGenerate(t, solutionName)).ToList();
            var dbcontext = DbContextGenerate(tables);

            SaveFiles(solutionName, entities, models, validators, configurations, dbcontext);

        }

        public static List<Table> GetTables(List<TableDesigner> tablesSchemes, List<Relation> relations)
        {
            List<TableRelation> tableRelations(string tableName) =>
               relations.Where(r => r.ParentName == tableName || r.ChiledName == tableName)
               .Select(r => new TableRelation
               {
                   IsCollection = (r.ParentName == tableName),
                   RelatedTableName = r.ParentName == tableName ? r.ChiledName : r.ParentName
               }).ToList();

            List<Column> tableColumns(DataGridViewRowCollection rows) =>
                rows.Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => new Column
                {
                    Name = r.Cells[0].Value.ToString(),
                    DataType = r.Cells[1].Value.ToString(),
                    IsNullable = r.Cells[2].Value != null && bool.Parse(r.Cells[2].Value.ToString())
                }).ToList();

            var tables = tablesSchemes.Select(t => new Table
            {
                Name = t.Name,
                Columns = tableColumns(t.Rows),
                TableRelations = relations.Any()?tableRelations(t.Name): new List<TableRelation>()
            }).ToList();

            return tables;
        }
               

        private static void SaveFiles(string solutionName, List<FileModel> entities, List<FileModel> models, List<FileModel> validators,List<FileModel> configurations, string dbcontext)
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select folder to save files",
                RootFolder = Environment.SpecialFolder.Desktop,
                ShowNewFolderButton = true
            };

            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            CreateFolders(dialog.SelectedPath, solutionName);
            void saveEntity(FileModel f)
            {

                using var writer = new StreamWriter($"{dialog.SelectedPath}\\{solutionName}\\Databae\\Tables\\{f.FileName}.cs");
                writer.Write(f.Contents);
            }
            entities.ForEach(f => saveEntity(f));

            void saveModels(FileModel f)
            {

                using var writer = new StreamWriter($"{dialog.SelectedPath}\\{solutionName}\\Models\\{f.FileName}.cs");
                writer.Write(f.Contents);
            }
            models.ForEach(f => saveModels(f));

            void saveValidators(FileModel f)
            {

                using var writer = new StreamWriter($"{dialog.SelectedPath}\\{solutionName}\\Validations\\{f.FileName}.cs");
                writer.Write(f.Contents);
            }
            validators.ForEach(f => saveValidators(f));

            void saveConfigurations(FileModel f)
            {

                using var writer = new StreamWriter($"{dialog.SelectedPath}\\{solutionName}\\EntityFramworkCore\\EntityConfigurations\\{ f.FileName}.cs");
                writer.Write(f.Contents);
            }
            configurations.ForEach(f => saveConfigurations(f));



            using var writer = new StreamWriter($"{dialog.SelectedPath}\\{solutionName}\\ApplicationDbContex.cs");
            writer.Write(dbcontext);

            Process.Start("explorer.exe", $"{dialog.SelectedPath}\\{solutionName}");
        }

        private static FileModel ClassGenerate(Table table, string solutionName)
        {
            StringBuilder builder = new StringBuilder(Templates.CLASS_TEMPLATE);
            builder.Replace("@SolutionName@", solutionName)
                .Replace("@ClassName@", table.Name)
                .Replace("@ValidatorType@", $"{table.Name}Validator")
                .Replace("@PrivateConst@", BuildPrivateConst(table))
                .Replace("@PublicConst@", BuildPublicConst(table))
                .Replace("@Update@", BuildUpdate(table))
                .Replace("@ChildFunctions@", BuildChildFunctions(table))
                .Replace("@Props@", BuildProps(table, true))
                .Replace("@NavigationProps@", BuildNavigationProps(table))
                .Replace("@ForeignKeys@", BuildForeignKeys(table, true));

            return new FileModel
            {
                FileName = table.Name,
                Contents = builder.ToString()
            };
        }

        private static FileModel ModelsGenerate(Table table, string solutionName)
        {
            StringBuilder builder = new StringBuilder(Templates.MODELS_TEMPLATE);
            builder.Replace("@SolutionName@", solutionName)
                .Replace("@ClassName@", table.Name)
                .Replace("@Props@", BuildProps(table, false))
                .Replace("@NavigationProps@", BuildModelsNavigationProps(table))
                .Replace("@ForeignKeys@", BuildForeignKeys(table, false));

            return new FileModel
            {
                FileName = $"{table.Name}Models",
                Contents = builder.ToString()
            };
        }

        private static FileModel ValidatorGenerate(Table table, string solutionName)
        {
            StringBuilder builder = new StringBuilder(Templates.VALIDATOR_TEMPLATE);
            builder.Replace("@SolutionName@", solutionName)
                .Replace("@ClassName@", table.Name);

            return new FileModel
            {
                FileName = $"{table.Name}Validator",
                Contents = builder.ToString()
            };
        }
        private static FileModel EntityConfigurationGenerate(Table table, string solutionName)
        {
            string relationsConfig(TableRelation r) =>
                Templates.RELATION_CONFIG_TEMPLATE
                .Replace("@Children@", r.SetRelatedTableName)
                .Replace("@Parent@", table.Name)
                .Replace("@ForeignKey@", $"{table.Name}Id");

            List<string> buildRelationsConfig(List<TableRelation> relations) =>
                relations.Where(r => r.IsCollection)
                .Select(r => relationsConfig(r))
                .ToList();

            StringBuilder builder = new StringBuilder(Templates.ENTITY_CONFIGURATIONS_TEMPLATE);
            builder.Replace("@SolutionName@", solutionName)
                .Replace("@ClassName@", table.Name)
                 .Replace("@ClassNames@", table.SetName)
                .Replace("@RelationsConfig@", string.Join("\n", buildRelationsConfig(table.TableRelations))); ;

            return new FileModel
            {
                FileName = $"{table.Name}EntityConfiguration",
                Contents = builder.ToString()
            };
        }

        private static (string,string) TableDbContext(Table table)
        {
           
            StringBuilder builder = new StringBuilder(Templates.DBCONTEXT_SET_TEMPLATE);
            builder.Replace("@ClassName@", table.Name)
                .Replace("@ClassNames@", table.SetName);

            StringBuilder configBuilder = new StringBuilder(Templates.DBCONTEXT_OnModelCreating_TEMPLATE);
            configBuilder.Replace("@ClassName@", table.Name);

            return (builder.ToString(), configBuilder.ToString());
        }
        private static string DbContextGenerate(List<Table> tables)
        {
            var codes = tables.Select(t => TableDbContext(t)).ToList();
            StringBuilder res = new StringBuilder("// Put on ApplicationDbContext\n\n");
            StringBuilder configs = new StringBuilder("// Put on OnModelCreating\n\n");
            codes.ForEach(t =>
            {
                res.Append(t.Item1);
                res.Append("\n");
                configs.Append(t.Item2);
                configs.Append("\n");
            });

            res.Append("\n\n//===========================================");
            res.Append(configs.ToString());
            return res.ToString();
        }

        private static string BuildNavigationProps(Table table)
        {
            string collectionProp(TableRelation r) => $"public virtual ICollection<{r.RelatedTableName}> {r.SetRelatedTableName} {{ get; private set; }}";
            string singleProp(TableRelation r) => $"public virtual {r.RelatedTableName} {r.RelatedTableName} {{ get; private set; }}";

            var props = table.TableRelations
                .Select(r => r.IsCollection ? collectionProp(r) : singleProp(r))
                .ToList();

            return string.Join("\n", props);
        }

        private static string BuildProps(Table table, bool isPrivateSet)
        {
            var modifier = isPrivateSet ? "private " : "";
            string nullable(Column c) => c.IsNullable ? "?" : "";
            var props = table.Columns
                .Select(c => $"public {c.DataType}{nullable(c)} {c.Name} {{get; {modifier}set;}}")
                .ToList();
            return string.Join("\n", props);
        }

        private static string BuildChildFunctions(Table table)
        {
            var functions = table.TableRelations
                .Where(r => r.IsCollection)
                .Select(r => Templates.CHILED_FUNCTIONS_TEMPLATE
                .Replace("@ClassName@", table.Name)
                .Replace("@RelatedName@", r.RelatedTableName)
                .Replace("@SetRelatedName@", r.SetRelatedTableName)
                .Replace("@_SetRelatedName@", r.SetRelatedTableName.ToLower()))
                .ToList();
            return string.Join("\n", functions);
        }

        private static string BuildUpdate(Table table)
        {
            var assignments = table.Columns
                .Select(c => $"{c.Name} = model.{c.Name};")
                .ToList();
            return string.Join("\n", assignments);
        }

        private static string BuildPublicConst(Table table)
        {
            var assignments = table.Columns
                .Select(c => $"{c.Name} = model.{c.Name};")
                .ToList();
            return string.Join("\n", assignments);
        }

        private static string BuildPrivateConst(Table table)
        {
            var initChildren = table.TableRelations
                .Where(r => r.IsCollection)
                .Select(r => $"{r.SetRelatedTableName} = new HashSet<{r.RelatedTableName}>();")
                .ToList();
            return string.Join("\n", initChildren);
        }

        private static string BuildModelsNavigationProps(Table table)
        {
            string collectionProp(TableRelation r) => $"public List<{r.RelatedTableName}ReadModel> {r.SetRelatedTableName} {{ get; set; }}";

            string singleProp(TableRelation r) => $"public  {r.RelatedTableName}ReadModel {r.RelatedTableName} {{ get; set; }}";

            var props = table.TableRelations
                .Select(r => r.IsCollection ? collectionProp(r) : singleProp(r))
                .ToList();

            return string.Join("\n", props);
        }

        private static string BuildForeignKeys(Table table, bool isPrivateSet)
        {
            var modifier = isPrivateSet ? "private " : "";
            var keys = table.TableRelations
                .Where(r => !r.IsCollection)
                .Select(r => $"public long {r.ForeignKey} {{get; {modifier}set;}}");
            return string.Join("\n", keys);
        }

        private static void CreateFolders(string root, string solutionName)
        {
            if (!Directory.Exists($"{root}\\{solutionName}"))
            {
                Directory.CreateDirectory($"{root}\\{solutionName}");
            }

            if (!Directory.Exists($"{root}\\{solutionName}\\Validations"))
            {
                Directory.CreateDirectory($"{root}\\{solutionName}\\Validations");
            }

            if (!Directory.Exists($"{root}\\{solutionName}\\Models"))
            {
                Directory.CreateDirectory($"{root}\\{solutionName}\\Models");
            }

            if (!Directory.Exists($"{root}\\{solutionName}\\Databae\\Tables"))
            {
                Directory.CreateDirectory($"{root}\\{solutionName}\\Databae\\Tables");
            }

            if (!Directory.Exists($"{root}\\{solutionName}\\EntityFramworkCore\\EntityConfigurations"))
            {
                Directory.CreateDirectory($"{root}\\{solutionName}\\EntityFramworkCore\\EntityConfigurations");
            }
        }
    }
}
