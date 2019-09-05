using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TombIDE.ProjectMaster
{
	public partial class FormSplashPreview : Form
	{
		public FormSplashPreview(string engineDirectory)
		{
			InitializeComponent();

			label.ForeColor = Color.FromArgb(64, 64, 64);

			string splashImagePath = Path.Combine(engineDirectory, "splash.bmp");

			if (File.Exists(splashImagePath))
			{
				Image splashImage = Image.FromFile(splashImagePath);

				if ((splashImage.Width == 1024 && splashImage.Height == 512)
					|| (splashImage.Width == 768 && splashImage.Height == 384)
					|| (splashImage.Width == 512 && splashImage.Height == 256))
				{
					Width = splashImage.Width;
					Height = splashImage.Height + 60;

					panel.BackgroundImage = splashImage;
				}
				else
				{
					splashImage.Dispose();
					HideSplashImage();
				}
			}
			else
				HideSplashImage();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			timer_Animation.Start();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Escape)
				DialogResult = DialogResult.OK;
		}

		protected override void OnDeactivate(EventArgs e)
		{
			if (panel.BackgroundImage != null)
				panel.BackgroundImage.Dispose();

			base.OnDeactivate(e);
		}

		private void Label_Click(object sender, EventArgs e) => DialogResult = DialogResult.OK;

		private void Timer_Animation_Tick(object sender, EventArgs e)
		{
			label.ForeColor = Color.FromArgb(label.ForeColor.R + 8, label.ForeColor.G + 8, label.ForeColor.B + 8);

			if (label.ForeColor.R >= 224 || label.ForeColor.G >= 224 || label.ForeColor.B >= 224)
				timer_Animation.Stop();
		}

		private void HideSplashImage()
		{
			panel.Dispose();
			label.Dock = DockStyle.Fill;

			Width = 420;
			Height = 60;
		}
	}
}
