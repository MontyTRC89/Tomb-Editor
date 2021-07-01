using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class SettingsIcon : UserControl
	{
		private IDE _ide;

		#region Initialization

		public SettingsIcon()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			radioButton_Dark.Checked = !_ide.IDEConfiguration.LightModePreviewEnabled;
			radioButton_Light.Checked = _ide.IDEConfiguration.LightModePreviewEnabled;

			if (_ide.Project.ProjectPath.ToLower() == _ide.Project.EnginePath.ToLower())
			{
				label_Unavailable.Visible = true;

				button_Change.Enabled = false;
				button_Reset.Enabled = false;
			}
			else
				UpdateIcons();
		}

		#endregion Initialization

		#region Events

		private void radioButton_Dark_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton_Dark.Checked)
			{
				panel_PreviewBackground.BackColor = Color.FromArgb(48, 48, 48);

				_ide.IDEConfiguration.LightModePreviewEnabled = false;
				_ide.IDEConfiguration.Save();
			}
		}

		private void radioButton_Light_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton_Light.Checked)
			{
				panel_PreviewBackground.BackColor = Color.White;

				_ide.IDEConfiguration.LightModePreviewEnabled = true;
				_ide.IDEConfiguration.Save();
			}
		}

		private void button_Change_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Choose the .ico file you want to inject into your game's .exe file";
				dialog.Filter = "Icon Files|*.ico";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					ApplyIconToExe(dialog.FileName);
			}
		}

		private void button_Reset_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to restore the default icon?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				string icoFilePath = string.Empty;

				if (_ide.Project.GameVersion == TRVersion.Game.TR4 || _ide.Project.GameVersion == TRVersion.Game.TRNG)
				{
					if (_ide.Project.GameVersion == TRVersion.Game.TRNG && File.Exists(Path.Combine(_ide.Project.EnginePath, "flep.exe")))
						icoFilePath = Path.Combine(TemplatePaths.GetDefaultTemplatesPath(_ide.Project.GameVersion), "FLEP.ico");
					else
						icoFilePath = Path.Combine(TemplatePaths.GetDefaultTemplatesPath(_ide.Project.GameVersion), _ide.Project.GameVersion + ".ico");
				}
				else if (_ide.Project.GameVersion == TRVersion.Game.TombEngine)
					icoFilePath = Path.Combine(TemplatePaths.GetDefaultTemplatesPath(_ide.Project.GameVersion), _ide.Project.GameVersion + ".ico");

				ApplyIconToExe(icoFilePath);
			}
		}

		#endregion Events

		#region Methods

		private void ApplyIconToExe(string iconPath)
		{
			try
			{
				IconInjector.InjectIcon(_ide.Project.LaunchFilePath, iconPath);
				UpdateIcons();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateIcons() // This method is trash I know, but I couldn't find a better one
		{
			// Create the temporary .exe file
			string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".exe");
			File.Copy(_ide.Project.LaunchFilePath, tempFilePath);

			Bitmap ico_256 = ImageHandling.CropBitmapWhitespace(IconExtractor.ExtractIcon(tempFilePath, IconSize.Jumbo).ToBitmap());

			// Windows doesn't seem to have a name for 128x128 px icons, therefore we must resize the Jumbo one
			Bitmap ico_128 = ImageHandling.ResizeImage(ico_256, 128, 128) as Bitmap;

			Bitmap ico_48 = ImageHandling.CropBitmapWhitespace(IconExtractor.ExtractIcon(tempFilePath, IconSize.ExtraLarge).ToBitmap());
			Bitmap ico_16 = IconExtractor.ExtractIcon(tempFilePath, IconSize.Small).ToBitmap();

			if (ico_256.Width == ico_48.Width && ico_256.Height == ico_48.Height)
			{
				panel_256.BorderStyle = BorderStyle.FixedSingle;
				panel_128.BorderStyle = BorderStyle.FixedSingle;

				panel_256.BackgroundImage = ico_48;
				panel_128.BackgroundImage = ico_48;
				panel_48.BackgroundImage = ico_48;
				panel_16.BackgroundImage = ico_16;
			}
			else
			{
				panel_256.BorderStyle = BorderStyle.None;
				panel_128.BorderStyle = BorderStyle.None;

				panel_256.BackgroundImage = ico_256;
				panel_128.BackgroundImage = ico_128;
				panel_48.BackgroundImage = ico_48;
				panel_16.BackgroundImage = ico_16;
			}

			// Now delete the temporary .exe file
			File.Delete(tempFilePath);
		}

		#endregion Methods
	}
}
