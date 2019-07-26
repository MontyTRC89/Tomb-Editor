using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TombEditor.Forms
{
    public partial class FormSelectRoomByTags : DarkUI.Forms.DarkForm
    {
        public FormSelectRoomByTags()
        {
            InitializeComponent();
        }

        private void ButOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void ButCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
