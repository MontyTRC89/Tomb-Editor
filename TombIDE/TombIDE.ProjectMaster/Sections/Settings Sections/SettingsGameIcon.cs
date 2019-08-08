using DarkUI.Forms;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;

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
			OpenFileDialog dialog = new OpenFileDialog
			{
				Title = "Choose the .ico file you want to inject into your game's .exe file",
				Filter = "Icon Files|*.ico"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
				ApplyIconToExe(dialog.FileName);
		}

		private void button_Reset_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to restore the default icon?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				ApplyIconToExe(Path.Combine("Templates", _ide.Project.GameVersion + ".ico"));
		}

		#endregion Events

		#region Methods

		private void ApplyIconToExe(string iconPath)
		{
			try
			{
				string exeFilePath = Path.Combine(_ide.Project.ProjectPath, _ide.Project.GetExeFileName());
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
			string exeFilePath = Path.Combine(_ide.Project.ProjectPath, _ide.Project.GetExeFileName());

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
