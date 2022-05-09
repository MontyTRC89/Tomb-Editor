using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Forms
{
	public partial class FormGameArchive : DarkForm
	{
		private IDE _ide;
		private FormPleaseWait _waitForm = new FormPleaseWait();

		public FormGameArchive(IDE ide)
		{
			InitializeComponent();

			_ide = ide;
		}

		private string _fileName;

		private void button_Generate_Click(object sender, EventArgs e)
		{
			using (var dialog = new SaveFileDialog())
			{
				dialog.InitialDirectory = _ide.Project.ProjectPath;
				dialog.FileName = $"{_ide.Project.Name.Replace(' ', '_')}.zip";
				dialog.Filter = "Zip Archive (*.zip)|*.zip";
				dialog.DefaultExt = "zip";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					_fileName = dialog.FileName;
					_waitForm.Show(this);
					timer.Start();
				}
			}
		}

		public void GenerateArchive(string filePath, string readmeText)
		{
			string[] importantFolders = new string[]
			{
				Path.Combine(_ide.Project.EnginePath, "audio"),
				Path.Combine(_ide.Project.EnginePath, "data"),
				Path.Combine(_ide.Project.EnginePath, "pix")
			};

			string[] importantFiles = new string[]
			{
				 Path.Combine(_ide.Project.EnginePath, "load.bmp"),
				 Path.Combine(_ide.Project.EnginePath, "splash.bmp"),
				 Path.Combine(_ide.Project.EnginePath, "patches.bin"),
				 Path.Combine(_ide.Project.EnginePath, "tomb4.exe")
			};

			var allImportantFiles = new List<string>();
			allImportantFiles.AddRange(importantFiles);
			allImportantFiles.AddRange(Directory.GetFiles(_ide.Project.EnginePath, "*.dll", SearchOption.TopDirectoryOnly));
			allImportantFiles.AddRange(Directory.GetFiles(_ide.Project.EnginePath, "*.dat", SearchOption.TopDirectoryOnly));

			string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

			if (!Directory.Exists(tempDirectory))
				Directory.CreateDirectory(tempDirectory);

			string targetTempEngineDirectory = Path.Combine(tempDirectory, "Engine");

			foreach (string folder in importantFolders)
			{
				if (!Directory.Exists(folder))
					continue;

				string pathPart = folder.Remove(0, _ide.Project.EnginePath.Length);
				string targetPath = Path.Combine(targetTempEngineDirectory, pathPart.Trim('\\'));

				SharedMethods.CopyFilesRecursively(folder, targetPath);
			}

			foreach (string file in allImportantFiles)
			{
				if (!File.Exists(file))
					continue;

				string pathPart = file.Remove(0, _ide.Project.EnginePath.Length);
				string targetPath = Path.Combine(targetTempEngineDirectory, pathPart.Trim('\\'));

				File.Copy(file, targetPath);
			}

			// Copy launch.exe
			if (File.Exists(_ide.Project.LaunchFilePath))
				File.Copy(_ide.Project.LaunchFilePath, Path.Combine(tempDirectory, Path.GetFileName(_ide.Project.LaunchFilePath)), true);

			if (!string.IsNullOrWhiteSpace(readmeText))
				File.WriteAllText(Path.Combine(tempDirectory, "readme.txt"), readmeText);

			if (File.Exists(filePath))
				File.Delete(filePath);

			ZipFile.CreateFromDirectory(tempDirectory, filePath);

			if (Directory.Exists(tempDirectory))
				Directory.Delete(tempDirectory, true);
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			timer.Stop();

			GenerateArchive(_fileName, richTextBox.Text);
			_waitForm.Close();

			DarkMessageBox.Show(this, "Archive creation completed.", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);

			DialogResult = DialogResult.OK;
		}
	}
}
