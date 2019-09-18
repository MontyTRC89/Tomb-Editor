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

namespace WadTool
{
    public partial class FormStartupHelp : DarkForm
    {
        public FormStartupHelp()
        {
            InitializeComponent();

            richTextBox.BackColor = BackColor;
            richTextBox.ForeColor = ForeColor;
            richTextBox.Rtf = Properties.Resources.StartUpText;

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butIgnore_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Ignore;
            Close();
        }
    }
}
