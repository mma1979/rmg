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
        List<TableDesigner> tables;
        public List<TableDesigner> Tables
        {
            get
            {
                tables = tables ?? new List<TableDesigner>();
                return tables;
            }
        }
        public bool OK { get; set; }
        public string TableName { get; set; }

        public List<Relation> Relations { get; set; }
        public FrmMain()
        {
            InitializeComponent();
            Relations = new List<Relation>();

        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tables.Count >= 15)
            {
                MessageBox.Show("Max number of Tables");
                return;
            }

            var frmName = new FrmTableName();
            _ = frmName.ShowDialog();

            if (!OK)
            {
                return;
            }
            var columns = Screen.PrimaryScreen.WorkingArea.Width / 300;
            var rows = Screen.PrimaryScreen.WorkingArea.Width / 300;


            var c = Tables.Count % columns;
            var r = Tables.Count / rows;


            var x = c * 310;
            var y = r * 235 + 35;


            var table = new PanelDesigner(TableName, x, y);
            Controls.Add(table);
            Tables.Add(table.Controls[TableName] as TableDesigner);

        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tables.ForEach(t => Controls.RemoveByKey(t.Name));
            tables = new List<TableDesigner>();
            Relations = new List<Relation>();
            OK = false;
            TableName = string.Empty;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AddRelationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Tables.Any())
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
            if (!tables.Any())
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

            if(result == DialogResult.OK)
            {
                var model = new ExportImportModel
                {
                    Tables = Engine.GetTables(Tables, Relations),
                    Relations = Relations
                };

                using var writer = new StreamWriter(dialog.FileName);
                writer.Write(JsonConvert.SerializeObject(model));

                MessageBox.Show("Export is Finished");
            }
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tables.ForEach(t => Controls.RemoveByKey(t.Name));
            tables = new List<TableDesigner>();
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

                var _tables = model.Tables.Select(t => new TableDesigner(t.Name)
                {
                    DataSource = t.Columns
                }).ToList();

                tables = _tables;
                Controls.AddRange(_tables.ToArray());

                Relations = model.Relations;

            }

           
        }
    }
}
