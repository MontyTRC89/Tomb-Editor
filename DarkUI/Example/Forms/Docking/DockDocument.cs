﻿using System.Drawing;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Docking;
using DarkUI.Forms;

namespace Example.Forms.Docking
{
    public partial class DockDocument : DarkDocument
    {
        #region Constructor Region

        public DockDocument()
        {
            InitializeComponent();

            // Workaround to stop the textbox from highlight all text.
            txtDocument.SelectionStart = txtDocument.Text.Length;

            // Build dummy dropdown data
            cmbOptions.Items.Add(new DarkDropdownItem("25%"));
            cmbOptions.Items.Add(new DarkDropdownItem("50%"));
            cmbOptions.Items.Add(new DarkDropdownItem("100%"));
            cmbOptions.Items.Add(new DarkDropdownItem("200%"));
            cmbOptions.Items.Add(new DarkDropdownItem("300%"));
            cmbOptions.Items.Add(new DarkDropdownItem("400%"));
        }

        public DockDocument(string text, Image icon)
            : this()
        {
            DockText = text;
            Icon = icon;
        }

        #endregion

        #region Event Handler Region

        public override void Close()
        {
            var result = DarkMessageBox.Show(this, @"You will lose any unsaved changes. Continue?", @"Close document", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
                return;

            base.Close();
        }

        #endregion
    }
}
