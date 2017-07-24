using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TombEditor
{
    public partial class FormSplash : Form
    {
        public FormSplash()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void FormSplash_Load(object sender, EventArgs e)
        {
            this.Show();
            this.TopMost = true;

            timer1.Start();

            FormMain form = new FormMain();
            form.Show();

        }
    }
}
