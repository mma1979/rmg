using MMA.Tools.RichModelGenerator.DesktopApp.Engines;
using MMA.Tools.RichModelGenerator.DesktopApp.Forms;
using MMA.Tools.RichModelGenerator.DesktopApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp
{
    public partial class FrmMain : Form
    {
        List<TableDesigner> tableDesigners;
        public List<TableDesigner> TableDesigners
        {
            get
            {
                tableDesigners ??= new List<TableDesigner>();
                return tableDesigners;
            }
        }

        List<Table> tables;
        public List<Table> Tables
        {
            get
            {
                tables ??= new List<Table>();
                return tables;
            }
        }

        public bool OK { get; set; }
        public string TableName { get; set; }
        public string IdType { get; set; }
        public string ConnectionString { get; set; }

        public List<Relation> Relations { get; set; }
        public FrmMain()
        {
            InitializeComponent();
            Relations = new List<Relation>();
            tables = new List<Table>();

        }

        public void AddTable(string tableName, string idType)
        {
            TableDesigners.ForEach(t => Controls.RemoveByKey(t.Name));
            tables = Engine.GetTables(TableDesigners, Relations);
            tables.Add(new Table
            {
                Name = tableName,
                IdType = idType,
                Columns = new List<Column>(),
                TableRelations=new List<TableRelation>()
            });

            var columns = Screen.PrimaryScreen.WorkingArea.Width / 300;
            var rows = Screen.PrimaryScreen.WorkingArea.Width / 300;

            var _tables = Tables.Select((t, i) =>
            {

                var c = i % columns;
                var r = i / rows;


                var x = c * 310;
                var y = r * 235 + 35;


                var panel = new PanelDesigner(t.Name, x, y, t.IdType);
                t.Columns.ForEach(c =>
                {
                    panel.table.Rows.Add(c.Name, c.DataType, c.IsNullable);
                });

                tableDesigners.Add(panel.table);
                return panel;

            }).ToList();

            Controls.AddRange(_tables.ToArray());

        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frmName = new FrmTableName();
            _ = frmName.ShowDialog();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TableDesigners.ForEach(t => Controls.RemoveByKey(t.Name));
            tableDesigners = new List<TableDesigner>();
            Relations = new List<Relation>();
            tables = new List<Table>();
            OK = false;
            TableName = string.Empty;
            IdType = "long";
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AddRelationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TableDesigners.Any())
            {
                return;
            }
            var frm = new FrmRelations();
            _ = frm.ShowDialog();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ = new FrmSave().ShowDialog();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ = new FrmAboutBox().ShowDialog();
        }

        private void ShortcutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ = new FrmKeys().ShowDialog();
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!tableDesigners.Any())
            {
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Export ...",
                InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString(),
                AddExtension = true,
                DefaultExt = "rmg",
                Filter = "Rich Model Generator File (*.rmg) | *.rmg"
            };

            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                var model = new ExportImportModel
                {
                    Tables = Engine.GetTables(TableDesigners, Relations),
                    Relations = Relations
                };

                using var writer = new StreamWriter(dialog.FileName);
                writer.Write(JsonConvert.SerializeObject(model));

                MessageBox.Show("Export is Finished");
            }
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TableDesigners.ForEach(t => Controls.RemoveByKey(t.Name));
            tableDesigners = new List<TableDesigner>();
            Relations = new List<Relation>();
            OK = false;
            TableName = string.Empty;

            var dialog = new OpenFileDialog
            {
                Title = "Import ...",
                InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString(),
                DefaultExt = "rmg",
                Filter = "Rich Model Generator File (*.rmg) | *.rmg"
            };


            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                using var reader = new StreamReader(dialog.FileName);
                var model = JsonConvert.DeserializeObject<ExportImportModel>(reader.ReadToEnd());

                var columns = Screen.PrimaryScreen.WorkingArea.Width / 300;
                var rows = Screen.PrimaryScreen.WorkingArea.Width / 300;

                var _tables = model.Tables.Select((t, i) =>
                {

                    var c = i % columns;
                    var r = i / rows;


                    var x = c * 310;
                    var y = r * 235 + 35;


                    var panel = new PanelDesigner(t.Name, x, y, t.IdType);
                    t.Columns.ForEach(c =>
                    {
                        panel.table.Rows.Add(c.Name, c.DataType, c.IsNullable);
                    });

                    tableDesigners.Add(panel.table);
                    return panel;

                }).ToList();
                tables = model.Tables;

                Controls.AddRange(_tables.ToArray());

                Relations = model.Relations;

            }


        }

        private void DbExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new FrmDbExport();
            var res = frm.ShowDialog();

            if (string.IsNullOrEmpty(ConnectionString))
            {
                return;

            }

            TableDesigners.ForEach(t => Controls.RemoveByKey(t.Name));
            tableDesigners = new List<TableDesigner>();
            Relations = new List<Relation>();
            OK = false;
            TableName = string.Empty;

            var database = new DatabaseEngine(ConnectionString);
            var tablesScheme = database.ReadTablesScheme();
            var relationsScheme = database.ReadRelationsScheme();

            var tablesList = tablesScheme.GroupBy(t => t.table_name)
                .Select(t => database.GetTable(t.ToList(), relationsScheme))
                .ToList();


            var columns = Screen.PrimaryScreen.WorkingArea.Width / 300;
            var rows = Screen.PrimaryScreen.WorkingArea.Width / 300;

            var _tables = tablesList.Select((t, i) =>
            {

                var c = i % columns;
                var r = i / rows;


                var x = c * 310;
                var y = r * 235 + 35;


                var panel = new PanelDesigner(t.Name, x, y, t.IdType);
                t.Columns.ForEach(c =>
                {
                    panel.table.Rows.Add(c.Name, c.DataType, c.IsNullable);
                });

                tableDesigners.Add(panel.table);
                return panel;

            }).ToList();

            Controls.AddRange(_tables.ToArray());
            tables = tablesList;
            Relations = database.GetRelations(relationsScheme);
        }


    }
}
