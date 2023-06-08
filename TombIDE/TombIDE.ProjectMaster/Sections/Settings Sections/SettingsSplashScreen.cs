using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

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

			if (_ide.Project.DirectoryPath.Equals(_ide.Project.GetEngineRootDirectoryPath(), StringComparison.OrdinalIgnoreCase))
			{
				label_NotSupported.Visible = true;

				button_Preview.Enabled = false;
				button_Change.Enabled = false;
				button_Remove.Enabled = false;
			}
			else
				UpdatePreview();
		}

		#endregion Initialization

		#region Events

		private void button_Preview_Click(object sender, EventArgs e)
		{
			string launcherExecutable = _ide.Project.GetLauncherFilePath();
			string originalFileName = FileVersionInfo.GetVersionInfo(launcherExecutable).OriginalFilename;

			if (!originalFileName.Equals("launch.exe", StringComparison.OrdinalIgnoreCase))
			{
				DarkMessageBox.Show(this, "Project not supported.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var startInfo = new ProcessStartInfo
			{
				FileName = launcherExecutable,
				WorkingDirectory = Path.GetDirectoryName(launcherExecutable),
				Arguments = "-p"
			};

			Process.Start(startInfo);
		}

		private void button_Change_Click(object sender, EventArgs e)
		{
			using var dialog = new OpenFileDialog();
			dialog.Filter = "Bitmap Files|*.bmp";

			if (dialog.ShowDialog(this) == DialogResult.OK)
				ReplaceImage(dialog.FileName);
		}

		private void button_Remove_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to remove the splash screen image?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				try
				{
					string splashImagePath = Path.Combine(_ide.Project.GetEngineRootDirectoryPath(), "splash.bmp");

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
				using var image = Image.FromFile(imagePath);

				if ((image.Width == 1024 && image.Height == 512) ||
					(image.Width == 768 && image.Height == 384) ||
					(image.Width == 512 && image.Height == 256))
				{
					File.Copy(imagePath, Path.Combine(_ide.Project.GetEngineRootDirectoryPath(), "splash.bmp"), true);
					UpdatePreview();
				}
				else
					throw new ArgumentException("Wrong image size.");
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
				string engineDirectory = _ide.Project.GetEngineRootDirectoryPath();
				string splashImagePath = Path.Combine(engineDirectory, "splash.bmp");

				if (File.Exists(splashImagePath))
				{
					using var image = Image.FromFile(Path.Combine(engineDirectory, "splash.bmp"));

					if ((image.Width == 1024 && image.Height == 512) ||
						(image.Width == 768 && image.Height == 384) ||
						(image.Width == 512 && image.Height == 256))
					{
						panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 460, 230);
						label_Blank.Visible = false;
					}
					else
						label_Blank.Visible = true;
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
