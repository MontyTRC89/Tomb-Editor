using DarkUI.Forms;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class SettingsStartupImage : UserControl
	{
		private IDE _ide;

		#region Initialization

		public SettingsStartupImage()
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

		private void button_Change_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = "Bitmap Files|*.bmp";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					ReplaceImage(dialog.FileName);
			}
		}

		private void button_UseBlank_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to apply a blank image?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				try
				{
					using (Bitmap bitmap = new Bitmap(1, 1))
					{
						bitmap.SetPixel(0, 0, Color.Black);
						bitmap.Save(Path.Combine(_ide.Project.ProjectPath, "load.bmp"), ImageFormat.Bmp);
					}

					UpdatePreview();
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void button_Reset_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to restore the default image?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				string imageFilePath = string.Empty;

				if (_ide.Project.GameVersion == GameVersion.TR4 || _ide.Project.GameVersion == GameVersion.TRNG)
					imageFilePath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB4\Defaults", "load.bmp");
				else if (_ide.Project.GameVersion == GameVersion.TR5 || _ide.Project.GameVersion == GameVersion.TR5Main)
					imageFilePath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB5\Defaults", "load.bmp");

				ReplaceImage(imageFilePath);
			}
		}

		#endregion Events

		#region Methods

		private void ReplaceImage(string imagePath)
		{
			try
			{
				File.Copy(imagePath, Path.Combine(_ide.Project.ProjectPath, "load.bmp"), true);
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
				using (Image image = Image.FromFile(Path.Combine(_ide.Project.ProjectPath, "load.bmp")))
				{
					panel_Preview.BackgroundImage = ImageHandling.ResizeKeepAspect(image, panel_Preview.Width, panel_Preview.Height);
					label_Blank.Visible = image.Width == 1 && image.Height == 1;
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion Methods
	}
}
