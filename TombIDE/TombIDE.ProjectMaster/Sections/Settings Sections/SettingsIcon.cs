using DarkUI.Forms;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
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

			radioButton_Dark.Checked = !_ide.Configuration.LightModePreviewEnabled;
			radioButton_Light.Checked = _ide.Configuration.LightModePreviewEnabled;

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

				_ide.Configuration.LightModePreviewEnabled = false;
				_ide.Configuration.Save();
			}
		}

		private void radioButton_Light_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton_Light.Checked)
			{
				panel_PreviewBackground.BackColor = Color.White;

				_ide.Configuration.LightModePreviewEnabled = true;
				_ide.Configuration.Save();
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
						icoFilePath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB4\Defaults", "FLEP.ico");
					else
						icoFilePath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB4\Defaults", _ide.Project.GameVersion + ".ico");
				}
				else if (_ide.Project.GameVersion == TRVersion.Game.TR5Main)
					icoFilePath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB5\Defaults", _ide.Project.GameVersion + ".ico");

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
				UpdateWindowsIconCache();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateIcons() // This method is trash I know, but I couldn't find a better one
		{
			// Generate a random string to create a temporary .exe file.
			// We will extract the icon from the .exe copy because Windows is caching icons which doesn't allow us to easily extract
			// icons larger than 32x32 px.
			Random random = new Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			string randomString = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());

			// Create the temporary .exe file
			string tempFilePath = _ide.Project.LaunchFilePath + "." + randomString + ".exe";
			File.Copy(_ide.Project.LaunchFilePath, tempFilePath);

			Bitmap ico_256 = ImageHandling.CropBitmapWhitespace(IconExtractor.GetIconFrom(tempFilePath, IconSize.Jumbo, false).ToBitmap());

			// Windows doesn't seem to have a name for 128x128 px icons, therefore we must resize the Jumbo one
			Bitmap ico_128 = (Bitmap)ImageHandling.ResizeImage(IconExtractor.GetIconFrom(tempFilePath, IconSize.Jumbo, false).ToBitmap(), 128, 128); ;

			Bitmap ico_48 = ImageHandling.CropBitmapWhitespace(IconExtractor.GetIconFrom(tempFilePath, IconSize.ExtraLarge, false).ToBitmap());
			Bitmap ico_16 = IconExtractor.GetIconFrom(tempFilePath, IconSize.Small, false).ToBitmap();

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

		private void UpdateWindowsIconCache()
		{
			ProcessStartInfo info = new ProcessStartInfo
			{
				FileName = @"C:\Windows\SysNative\ie4uinit.exe",
				Arguments = IsWindows10() ? "-show" : "-ClearIconCache"
			};

			Process.Start(info);
		}

		private bool IsWindows10()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
			string productName = (string)key.GetValue("ProductName");
			return productName.StartsWith("Windows 10");
		}

		#endregion Methods
	}
}
