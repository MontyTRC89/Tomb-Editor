using System;
using System.Windows.Forms;
using DarkUI.Forms;

namespace TombLib.Forms
{
    public partial class FormInputBox : DarkForm
    {
        public FormInputBox(string Title = "Input", string Message = "Enter text:", string StartValue = "")
        {
            InitializeComponent();

            Text = Title;
            labelMessage.Text = Message;
            Result = StartValue;
        }

        public string Result
        {
            get { return tbInput.Text; }
            set { tbInput.Text = value; }
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
