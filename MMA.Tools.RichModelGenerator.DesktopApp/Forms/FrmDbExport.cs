﻿using System;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp.Forms
{
    public partial class FrmDbExport : Form
    {
        public FrmDbExport()
        {
            InitializeComponent();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtConnectionString.Text))
            {
                return;
            }

            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            frm.ConnectionString = txtConnectionString.Text;
            Close();
        }

        private void BtnConnBuilder_Click(object sender, EventArgs e)
        {
            _ = new FrmConnection().ShowDialog();
        }
    }
}
