using System;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Config;
using DarkUI.Forms;

namespace TombLib.Forms
{
    public partial class FormAbout : DarkForm
    {
        public FormAbout(Image image)
        {
            Text = "About " + Application.ProductName;

            InitializeComponent();

            pictureBox.BackgroundImage = image;
            pictureBox.Paint += PictureBox_Paint;
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            var font = new Font(Font.FontFamily, 10.0f);
            using (var b = new SolidBrush(Colors.LightText))
                e.Graphics.DrawString("Version " + Application.ProductVersion, font, b, pictureBox.Width, pictureBox.Height, new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far });
        }

        private void btnLink_Click(object sender, EventArgs e)
        {
            var control = (LinkLabel)sender;

            try
            {
                System.Diagnostics.Process.Start("http://www." + control.Text + "/");
                control.LinkVisited = false;
            }
            catch
            {
                // ignored
            }
        }
    }
}
