using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster
{
	public partial class SettingsSplashScreen : UserControl
	{
		private IDE _ide;

		#region Initialization

		public SettingsSplashScreen()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			UpdatePreview();
		}

		#endregion Initialization

		#region Events

		private void button_Preview_Click(object sender, EventArgs e)
		{
			using (FormSplashPreview form = new FormSplashPreview(_ide.Project.EnginePath))
				form.ShowDialog(this);
		}

		private void button_Change_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = "Bitmap Files|*.bmp";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					ReplaceImage(dialog.FileName);
			}
		}

		private void button_Remove_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to remove the splash screen image?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				try
				{
					string splashImagePath = Path.Combine(_ide.Project.EnginePath, "splash.bmp");

					if (File.Exists(splashImagePath))
						File.Delete(splashImagePath);

					UpdatePreview();
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		#endregion Events

		#region Methods

		private void ReplaceImage(string imagePath)
		{
			try
			{
				File.Copy(imagePath, Path.Combine(_ide.Project.EnginePath, "splash.bmp"), true);
				UpdatePreview();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdatePreview()
		{
			try
			{
				string splashImagePath = Path.Combine(_ide.Project.EnginePath, "splash.bmp");

				if (File.Exists(splashImagePath))
				{
					using (Image image = Image.FromFile(Path.Combine(_ide.Project.EnginePath, "splash.bmp")))
					{
						panel_Preview.BackgroundImage = ImageHandling.ResizeKeepAspect(image, panel_Preview.Width, panel_Preview.Height);
						label_Blank.Visible = false;
					}
				}
				else
					label_Blank.Visible = true;
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion Methods
	}
}
