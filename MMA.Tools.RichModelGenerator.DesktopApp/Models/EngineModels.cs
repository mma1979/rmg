using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMA.Tools.RichModelGenerator.DesktopApp
{
    public class Table
    {
        public string Name { get; set; }
        public string SetName
        {
            get
            {
                return Name.ToLower().EndsWith("y") ?
                    $"{Name.TrimEnd('y', 'Y')}ies" :
                        Name.ToLower().EndsWith("s") ?
                        $"{Name}es" :
                        $"{Name}s";
            }
        }
        public string IdType { get; set; }

        public List<Column> Columns { get; set; }
        public List<TableRelation> TableRelations { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
    }

    public class TableRelation
    {
        public string RelatedTableName { get; set; }
        public string SetRelatedTableName
        {
            get
            {
                if (!IsCollection)
                {
                    return RelatedTableName;
                }
                return RelatedTableName.ToLower().EndsWith("y") ?
                    $"{RelatedTableName.TrimEnd('y', 'Y')}ies" :
                        RelatedTableName.ToLower().EndsWith("s") ?
                        $"{RelatedTableName}es":
                        $"{RelatedTableName}s";
            }
        }
        public string ForeignKey
        {
            get
            {
                return IsCollection ? "" : $"{RelatedTableName}Id";
            }
        }
        public bool IsCollection { get; set; }
    }
}
