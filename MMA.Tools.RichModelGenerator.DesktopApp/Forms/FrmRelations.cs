using MMA.Tools.RichModelGenerator.DesktopApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp
{
    public partial class FrmRelations : Form
    {
        private readonly BindingSource source = new BindingSource();
        public FrmRelations()
        {
            InitializeComponent();
            LoadData();
            
        }

        private void LoadData()
        {
            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            var tables1 = frm.Tables.Select(t => new { t.Name }).ToList();
            var tables2 = frm.Tables.Select(t => new { t.Name }).ToList();
            cmbParent.DataSource = tables1;
            cmbParent.ValueMember = "Name";
            cmbParent.DisplayMember = "Name";

            cmbChiled.DataSource = tables2;
            cmbChiled.ValueMember = "Name";
            cmbChiled.DisplayMember = "Name";

            
            source.DataSource = frm.Relations;
            dataGridView1.DataSource = source;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            frm.Relations.Add(new Relation
            {
                ParentName = cmbParent.SelectedValue.ToString(),
                RelationType = "1:M",
                ChiledName = cmbChiled.SelectedValue.ToString(),
                ForeignKey=$"{cmbParent.SelectedValue}Id"
            });

            source.ResetBindings(false);
        }
    }
}
