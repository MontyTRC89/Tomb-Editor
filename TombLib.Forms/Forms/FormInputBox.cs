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
    public partial class FormInputBox : DarkUI.Forms.DarkForm
    {
        public string Result { get; set; }

        public FormInputBox(string title, string message, string input = "")
        {
            InitializeComponent();

            Text = title;
            labelMessage.Text = message;
            tbInput.Text = input;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            Result = tbInput.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
