using DarkUI.Controls;
using DarkUI.Forms;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TombIDE.ProjectMaster
{
	public partial class FormPluginLibrary : DarkForm
	{
		public FormPluginLibrary()
		{
			InitializeComponent();

			UpdateTreeView();
		}

		private void button_OpenArchive_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Title = "Select a .ZIP / .GZIP / .LZIP / .BZIP2 / .TAR / .RAR / .XZ File",
				Filter = "All Supported Files|*.zip;*.gzip;*.lzip;*.bzip2;*.tar;*.rar;*.xz"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				string filePath = dialog.FileName;

				try
				{
					if (!Directory.Exists("Plugins"))
						Directory.CreateDirectory("Plugins");

					using (Stream stream = File.OpenRead(filePath))
					{
						using (IReader reader = ReaderFactory.Open(stream))
						{
							while (reader.MoveToNextEntry())
							{
								if (!reader.Entry.IsDirectory)
								{
									reader.WriteEntryToDirectory(Path.Combine("Plugins", Path.GetFileNameWithoutExtension(filePath)),
										new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				UpdateTreeView();
			}
		}

		private void UpdateTreeView()
		{
			treeView_AvailablePlugins.Nodes.Clear();

			DirectoryInfo directoryInfo = new DirectoryInfo("Plugins");

			foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
			{
				string pluginName = directory.Name;

				foreach (FileInfo file in directory.GetFiles())
				{
					if (Path.GetExtension(file.Name) == ".btn")
					{
						string[] fileContent = File.ReadAllLines(file.FullName, Encoding.GetEncoding(1252));

						foreach (string line in fileContent)
						{
							if (line.StartsWith("NAME#"))
							{
								pluginName = line.Replace("NAME#", string.Empty).Trim();
								break;
							}
						}
					}
				}

				DarkTreeNode node = new DarkTreeNode(pluginName)
				{
					Tag = directory.FullName
				};

				treeView_AvailablePlugins.Nodes.Add(node);
			}
		}
	}
}
