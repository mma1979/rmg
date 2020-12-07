using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp
{
    public class PanelDesigner : Panel
    {
        public TableDesigner table;
        private Label lblName;


        private void InitializeComponent(string name = "table1", int x = 31, int y = 35)
        {
            
           

            lblName = new System.Windows.Forms.Label();
            table = new TableDesigner(name, x, y);
            // 
            // lblName
            // 
            lblName.AutoSize = false;
            lblName.Location = new System.Drawing.Point(0, 0);
            lblName.Name = "lblName";
            lblName.TabIndex = 0;
            lblName.Text = name;
            lblName.Dock = DockStyle.Top;
            lblName.Font = new System.Drawing.Font("Trebuchet MS", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblName.TabIndex = 0;
            lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;


            table.Dock = DockStyle.Bottom;

            Size = new System.Drawing.Size(290, 225);
            Location = new System.Drawing.Point(x, y);
            Name = name;

            Controls.AddRange(new Control[] { lblName, table });
        }

        public PanelDesigner(string name = "table1", int x = 31, int y = 35)
        {
            InitializeComponent(name,x,y);
        }
    }
}
