namespace MMA.Tools.RichModelGenerator.DesktopApp.Models
{
    public class DatabaseScheme
    {
        public string schema_name { get; set; }
        public string table_name { get; set; }
        public string column_id { get; set; }
        public string column_name { get; set; }
        public string data_type { get; set; }
        public string max_length { get; set; }
        public string precision { get; set; }
        public bool is_nullable { get; set; }

    }

    public class RelationsScheme
    {
        public string CurrentTable { get; set; }
        public string ForeignKey { get; set; }
        public string RelatedTableName { get; set; }
    }
}
