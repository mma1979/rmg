using System.Collections.Generic;

namespace MMA.Tools.RichModelGenerator.DesktopApp.Models
{
    public class TableComparer : IEqualityComparer<Table>
    {
        public bool Equals(Table tbl1, Table tbl2)
        {
            return tbl1.Name == tbl2.Name;
        }

        public int GetHashCode(Table tbl)
        {
            return tbl.GetHashCode();
        }
    }
}
