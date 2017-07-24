using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormInputBox : DarkForm
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Value { get; set; }

        public FormInputBox()
        {
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormSink_Load(object sender, EventArgs e)
        {
            this.Text = Title;
            labelMessage.Text = Message;
            tbValue.Text = Value;

            tbValue.Focus();
            tbValue.Select();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            Value = tbValue.Text;

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
