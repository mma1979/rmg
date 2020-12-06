using System;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp
{
    public partial class FrmTableName : Form
    {
      
        public FrmTableName()
        {
            InitializeComponent();
            btnOK.Enabled = txtName.Text.Length >= 3;
           
        }

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = txtName.Text.Length >= 3;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            frm.OK = true;
            frm.TableName = txtName.Text;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            frm.OK = false;
            Close();
        }
    }
}
