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
	public partial class SettingsGameIcon : UserControl
	{
		private IDE _ide;

		#region Initialization

		public SettingsGameIcon()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			radioButton_Dark.Checked = !_ide.Configuration.LightModePreviewEnabled;
			radioButton_Light.Checked = _ide.Configuration.LightModePreviewEnabled;

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

				if (_ide.Project.GameVersion == GameVersion.TR4 || _ide.Project.GameVersion == GameVersion.TRNG)
				{
					if (_ide.Project.GameVersion == GameVersion.TRNG && File.Exists(Path.Combine(_ide.Project.ProjectPath, "flep.exe")))
						icoFilePath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB4\Defaults", "FLEP.ico");
					else
						icoFilePath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB4\Defaults", _ide.Project.GameVersion + ".ico");
				}
				else if (_ide.Project.GameVersion == GameVersion.TR5 || _ide.Project.GameVersion == GameVersion.TR5Main)
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
				string launchFile = Path.Combine(_ide.Project.ProjectPath, "launch.exe");
				string gameFile = Path.Combine(_ide.Project.ProjectPath, _ide.Project.GetExeFileName());

				string exeFilePath = File.Exists(launchFile) ? launchFile : gameFile;

				IconInjector.InjectIcon(exeFilePath, iconPath);

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
			string launchFile = Path.Combine(_ide.Project.ProjectPath, "launch.exe");
			string gameFile = Path.Combine(_ide.Project.ProjectPath, _ide.Project.GetExeFileName());

			string exeFilePath = File.Exists(launchFile) ? launchFile : gameFile;

			// Generate a random string to create a temporary .exe file.
			// We will extract the icon from the .exe copy because Windows is caching icons which doesn't allow us to easily extract
			// icons larger than 32x32 px.
			Random random = new Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			string randomString = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());

			// Create the temporary .exe file
			string tempFilePath = exeFilePath + "." + randomString + ".exe";
			File.Copy(exeFilePath, tempFilePath);

			panel_256.BackgroundImage = IconExtractor.GetIconFrom(tempFilePath, IconSize.Jumbo, false).ToBitmap();

			// Windows doesn't seem to have a name for 128x128 px icons, therefore we must resize the Jumbo one
			panel_128.BackgroundImage = ImageHandling.ResizeImage(IconExtractor.GetIconFrom(tempFilePath, IconSize.Jumbo, false).ToBitmap(), 128, 128);

			panel_48.BackgroundImage = IconExtractor.GetIconFrom(tempFilePath, IconSize.ExtraLarge, false).ToBitmap();
			panel_16.BackgroundImage = IconExtractor.GetIconFrom(tempFilePath, IconSize.Small, false).ToBitmap();

			// Now delete the temporary .exe file
			File.Delete(tempFilePath);
		}

		private void UpdateWindowsIconCache()
		{
			ProcessStartInfo info = new ProcessStartInfo
			{
				FileName = @"C:\Windows\SysNative\ie4uinit.exe"
			};

			if (IsWindows10())
				info.Arguments = "-show";
			else
				info.Arguments = "-ClearIconCache";

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
