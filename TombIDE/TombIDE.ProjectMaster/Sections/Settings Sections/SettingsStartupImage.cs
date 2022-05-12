using DarkUI.Forms;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
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

			radioButton_Wide.Checked = !_ide.IDEConfiguration.StandardAspectRatioPreviewEnabled;
			radioButton_Standard.Checked = _ide.IDEConfiguration.StandardAspectRatioPreviewEnabled;

			UpdatePreview();
		}

		#endregion Initialization

		#region Events

		private void radioButton_Wide_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton_Wide.Checked)
			{
				try
				{
					using (Image image = Image.FromFile(Path.Combine(_ide.Project.EnginePath, "load.bmp")))
						panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 426, 240);

					_ide.IDEConfiguration.StandardAspectRatioPreviewEnabled = false;
					_ide.IDEConfiguration.Save();
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void radioButton_Standard_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton_Standard.Checked)
			{
				try
				{
					using (Image image = Image.FromFile(Path.Combine(_ide.Project.EnginePath, "load.bmp")))
						panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 320, 240);

					_ide.IDEConfiguration.StandardAspectRatioPreviewEnabled = true;
					_ide.IDEConfiguration.Save();
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
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
						bitmap.Save(Path.Combine(_ide.Project.EnginePath, "load.bmp"), ImageFormat.Bmp);
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
				string imageFilePath = Path.Combine(TemplatePaths.GetDefaultTemplatesPath(_ide.Project.GameVersion), "load.bmp");
				ReplaceImage(imageFilePath);
			}
		}

		#endregion Events

		#region Methods

		private void ReplaceImage(string imagePath)
		{
			try
			{
				File.Copy(imagePath, Path.Combine(_ide.Project.EnginePath, "load.bmp"), true);
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
				using (Image image = Image.FromFile(Path.Combine(_ide.Project.EnginePath, "load.bmp")))
				{
					if (radioButton_Wide.Checked)
						panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 426, 240);
					else if (radioButton_Standard.Checked)
						panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 320, 240);

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
