using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Forms;

namespace TombEditor
{
    public partial class FormAbout : DarkForm
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void btnLink_Click(object sender, EventArgs e)
        {
            var control = sender as LinkLabel;

            try
            {
                System.Diagnostics.Process.Start("http://www." + control.Text + "/");
                control.LinkVisited = false;
            }
            catch { }
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            versionLabel.Text = "Version " + Application.ProductVersion;
            versionLabel.Location = new Point(ClientSize.Width - versionLabel.Width, versionLabel.Location.Y);
        }
    }
}
