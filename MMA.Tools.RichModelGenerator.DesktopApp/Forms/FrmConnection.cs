using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp.Forms
{
    public partial class FrmConnection : Form
    {

        private const string WIN_AUTH = "Data Source={0};Initial Catalog={1};Integrated Security=True";
        private const string SQL_AUTH = "Data Source={0};Initial Catalog={1};User ID={2};Password={3}";

        public FrmConnection()
        {
            InitializeComponent();
            btnOK.Enabled = comDatabase.Text != string.Empty &&
                txtServer.Text != string.Empty &&
                (!panelSecurity.Enabled || (panelSecurity.Enabled && txtUsername.Text != string.Empty && txtPassword.Text != string.Empty));
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            var connectionString = panelSecurity.Enabled ? 
                string.Format(SQL_AUTH, txtServer.Text, comDatabase.Text,txtUsername.Text,txtPassword.Text) : 
                string.Format(WIN_AUTH, txtServer.Text, comDatabase.Text);

            FrmDbExport frm = Application.OpenForms["FrmDbExport"] as FrmDbExport;
            frm.txtConnectionString.Text = connectionString;
            Close();
        }

        private void ComDatabase_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = comDatabase.Text != string.Empty &&
                txtServer.Text != string.Empty &&
                (!panelSecurity.Enabled || (panelSecurity.Enabled && txtUsername.Text != string.Empty && txtPassword.Text != string.Empty));
        }

        private void TxtServer_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = comDatabase.Text != string.Empty &&
               txtServer.Text != string.Empty &&
               (!panelSecurity.Enabled || (panelSecurity.Enabled && txtUsername.Text != string.Empty && txtPassword.Text != string.Empty));
        }

        private void ComSecurity_TextChanged(object sender, EventArgs e)
        {
            panelSecurity.Enabled = comSecurity.Text != "Windows Authentication";
            btnOK.Enabled = comDatabase.Text != string.Empty &&
               txtServer.Text != string.Empty &&
               (!panelSecurity.Enabled || (panelSecurity.Enabled && txtUsername.Text != string.Empty && txtPassword.Text != string.Empty));
        }

        private void TxtUsername_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = comDatabase.Text != string.Empty &&
              txtServer.Text != string.Empty &&
              (!panelSecurity.Enabled || (panelSecurity.Enabled && txtUsername.Text != string.Empty && txtPassword.Text != string.Empty));
        }

        private void TxtPassword_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = comDatabase.Text != string.Empty &&
              txtServer.Text != string.Empty &&
              (!panelSecurity.Enabled || (panelSecurity.Enabled && txtUsername.Text != string.Empty && txtPassword.Text != string.Empty));
        }

        private void ComDatabase_DropDown(object sender, EventArgs e)
        {
            try
            {
                var connectionString = panelSecurity.Enabled ?
                string.Format(SQL_AUTH, txtServer.Text, "master", txtUsername.Text, txtPassword.Text) :
                string.Format(WIN_AUTH, txtServer.Text, "master");

                using var ad = new SqlDataAdapter("select database_id, [name] from sys.databases order by [name]", connectionString);
                using var dt = new DataTable();
                ad.Fill(dt);

                comDatabase.DataSource = dt.DefaultView;
                comDatabase.DisplayMember = "name";
                comDatabase.ValueMember = "name";
            }
            catch { }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                var connectionString = panelSecurity.Enabled ?
               string.Format(SQL_AUTH, txtServer.Text, comDatabase.Text, txtUsername.Text, txtPassword.Text) :
               string.Format(WIN_AUTH, txtServer.Text, comDatabase.Text);

                using var conn = new SqlConnection(connectionString);
                conn.Open();
                MessageBox.Show("Connection successed", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                conn.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
