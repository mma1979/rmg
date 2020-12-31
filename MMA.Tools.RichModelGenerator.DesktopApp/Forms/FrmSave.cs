﻿using MMA.Tools.RichModelGenerator.DesktopApp.Engines;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp.Forms
{
    public partial class FrmSave : Form
    {
        public FrmSave()
        {
            InitializeComponent();
            btnSave.Enabled = txtName.Text.Length >= 3;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            FrmMain frm = Application.OpenForms["FrmMain"] as FrmMain;
            if (frm.TableDesigners.Any())
            {
                Engine.Start(frm.TableDesigners, frm.Relations, txtName.Text);
                Hide();
            }
        }

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            btnSave.Enabled = txtName.Text.Length >= 3;
        }
    }
}
