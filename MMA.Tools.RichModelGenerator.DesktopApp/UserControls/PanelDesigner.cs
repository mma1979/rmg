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
        private Button btnRemove;

        private void InitializeComponent(string name = "table1", int x = 31, int y = 35, string idType = "long")
        {


            lblName = new Label();
            table = new TableDesigner(name, x, y, idType);
            btnRemove = new Button();
            // 
            // lblName
            // 
            lblName.AutoSize = false;
            lblName.Location = new System.Drawing.Point(0, 0);
            lblName.Name = "lblName";
            lblName.Text = name;
            //lblName.Dock = DockStyle.Top;
            lblName.Font = new System.Drawing.Font("Trebuchet MS", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblName.TabIndex = 0;
            lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblName.BorderStyle = BorderStyle.FixedSingle;
            lblName.Size = new System.Drawing.Size(265, 25);

            // 
            // btnRemove
            // 
            btnRemove.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
            btnRemove.FlatAppearance.BorderSize = 1;
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnRemove.Location = new System.Drawing.Point(265, 0);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new System.Drawing.Size(25, 25);
            btnRemove.TabIndex = 2;
            btnRemove.Text = "x";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Tag = name;
            btnRemove.Click += BtnRemove_Click;



            table.Dock = DockStyle.Bottom;

            Size = new System.Drawing.Size(290, 225);
            Location = new System.Drawing.Point(x, y);
            Name = name;

            Controls.AddRange(new Control[] { lblName, btnRemove, table });
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var tableName = btn.Tag.ToString();
            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            var table = frm.TableDesigners.First(t => t.Name == tableName);
            frm.TableDesigners.Remove(table);
            var t = frm.Tables.First(t => t.Name == tableName);
            frm.Tables.Remove(t);
            frm.Controls.RemoveByKey(tableName);

            frm.Relations.RemoveAll(r => r.ChiledName == tableName || r.ParentName == tableName);

            frm.TableDesigners.ForEach(t => frm.Controls.RemoveByKey(t.Name));
            var columns = Screen.PrimaryScreen.WorkingArea.Width / 300;
            var rows = Screen.PrimaryScreen.WorkingArea.Width / 300;

            var _tables = frm.Tables.Select((t, i) =>
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

                frm.TableDesigners.Add(panel.table);
                return panel;

            }).ToList();

            frm.Controls.AddRange(_tables.ToArray());
        }

        public PanelDesigner(string name = "table1", int x = 31, int y = 35, string idType = "long")
        {
            InitializeComponent(name,x,y,idType);
        }
    }
}
