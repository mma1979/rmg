using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMA.Tools.RichModelGenerator.DesktopApp.Models
{
    public class Relation
    {
        public string ParentName { get; set; }
        public string RelationType { get; set; }
        public string ChiledName { get; set; }
        public string ForeignKey { get; set; }
    }
}
