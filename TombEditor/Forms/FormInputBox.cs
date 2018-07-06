using System;
using System.Windows.Forms;
using DarkUI.Forms;

namespace TombEditor.Forms
{
    public partial class FormInputBox : DarkForm
    {
        public FormInputBox()
        {
            InitializeComponent();
        }

        public string Title
        {
            set { Text = value; }
        }

        public string Message
        {
            set { labelMessage.Text = value; }
        }

        public string Value
        {
            get { return tbValue.Text; }
            set { tbValue.Text = value; }
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
