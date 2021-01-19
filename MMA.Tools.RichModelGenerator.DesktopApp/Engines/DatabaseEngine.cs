using MMA.Tools.RichModelGenerator.DesktopApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MMA.Tools.RichModelGenerator.DesktopApp.Engines
{
    public class DatabaseEngine
    {
        private string ConnectionString { get; set; }
        public DatabaseEngine(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public List<DatabaseScheme> ReadTablesScheme()
        {
            using var ad = new SqlDataAdapter(@"select schema_name(tab.schema_id) as schema_name,
                        tab.name as table_name, 
                        col.column_id,
                        col.name as column_name, 
                        t.name as data_type,    
                        col.max_length,
                        col.precision,
                        col.is_nullable
                    from sys.tables as tab
                        inner join sys.columns as col
                            on tab.object_id = col.object_id
                        left join sys.types as t
                        on col.user_type_id = t.user_type_id
                   where tab.name not in ( '__EFMigrationsHistory','sysdiagrams') and col.name not in ('CreatedBy','CreatedDate','ModifiedBy','ModifiedDate','IsActive')
                    order by schema_name,
                        table_name, 
                        column_id;", ConnectionString);

            using var dt = new DataTable();
            ad.Fill(dt);

            return dt.Rows.Cast<DataRow>()
                .Select(r => new DatabaseScheme
                {
                    schema_name = r["schema_name"].ToString(),
                    table_name = r["table_name"].ToString(),
                    column_id = r["column_id"].ToString(),
                    column_name = r["column_name"].ToString(),
                    data_type = r["data_type"].ToString(),
                    max_length = r["max_length"].ToString(),
                    precision = r["precision"].ToString(),
                    is_nullable = bool.Parse(r["is_nullable"].ToString())
                }).ToList();

        }

        public List<RelationsScheme> ReadRelationsScheme()
        {
            using var ad = new SqlDataAdapter(@"SELECT
                    --fk.name 'FK Name',
                    tp.name 'RelatedTableName',
                    cp.name 'ForeignKey', --cp.column_id,
                    tr.name 'CurrentTable'
                    --cr.name, cr.column_id
                FROM 
                    sys.foreign_keys fk
                INNER JOIN 
                    sys.tables tp ON fk.parent_object_id = tp.object_id
                INNER JOIN 
                    sys.tables tr ON fk.referenced_object_id = tr.object_id
                INNER JOIN 
                    sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                INNER JOIN 
                    sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
                INNER JOIN 
                    sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
                ORDER BY
                    tp.name, cp.column_id", ConnectionString);

            using var dt = new DataTable();
            ad.Fill(dt);

            return dt.Rows.Cast<DataRow>()
                .Select(r => new RelationsScheme
                {
                    CurrentTable = r["CurrentTable"].ToString(),
                    ForeignKey = r["ForeignKey"].ToString(),
                    RelatedTableName = r["RelatedTableName"].ToString()
                }).ToList();

        }

        public Table GetTable(List<DatabaseScheme> scheme, List<RelationsScheme> relations)
        {
            var tableName = scheme.First().table_name;
            var idType = MapDataType(scheme.FirstOrDefault(s => s.column_name.ToLower() == "id")?.data_type);
            var columns = scheme.Where(s => s.column_name.ToLower() != "id")
                .Select(c => new Column
                {
                    Name = c.column_name,
                    DataType = MapDataType(c.data_type),
                    IsNullable = true
                }).ToList();
            var tableRelations = relations
                .Where(r => r.CurrentTable == tableName || r.RelatedTableName == tableName)
                .Select(r => new TableRelation
                {
                    IsCollection = (r.CurrentTable == tableName),
                    RelatedTableName = MapTableName(r.CurrentTable == tableName ? r.RelatedTableName : r.CurrentTable)
                }).ToList();

            return new Table
            {
                Name = MapTableName(tableName),
                IdType = idType,
                Columns = columns,
                TableRelations = tableRelations
            };
        }

        public List<Relation> GetRelations(List<RelationsScheme> schemes)
        {
            return schemes.Select(r => new Relation
            {
                ParentName = MapTableName(r.CurrentTable),
                ChiledName = MapTableName(r.RelatedTableName),
                ForeignKey = r.ForeignKey,
                RelationType = "1:M"
            }).ToList();
        }

        public string MapDataType(string dbType)
        {
            return dbType switch {
                "int"=> "int",
                "bigint" => "long",
                "nvarchar"=>"string",
                "ntext" => "string",
                "bit" => "bool",
                "datetimeoffset"=> "DateTime",
                "datetime"=>"DateTime",
                "datetime2" => "DateTime",
                "decimal"=> "decimal",
                "uniqueidentifier"=>"Guid",
                _=> "string"

            };
        }

        public string MapTableName(string dbTableName)
        {
            return dbTableName.EndsWith("ies")?
                dbTableName.Replace("ies","y") : dbTableName.EndsWith("ses")?
                dbTableName.Replace("ses","s") : dbTableName.TrimEnd('s');
        }
    }
}
