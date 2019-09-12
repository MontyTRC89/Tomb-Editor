using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombLib.Forms
{
    public partial class AnimationImportDialog : DarkForm
    {
        public int AnimationToImport => cmbSelectAnimation.SelectedIndex; 

        public AnimationImportDialog(List<string> animations)
        {
            InitializeComponent();
            animations.ForEach(item => cmbSelectAnimation.Items.Add(item));

            if (cmbSelectAnimation.Items.Count > 0)
                cmbSelectAnimation.SelectedIndex = 0;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
