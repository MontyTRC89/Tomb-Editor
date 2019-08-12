using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.IO;
using System.IO.Compression;
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
				Filter = "All Supported Files|*.zip;*.rar"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				string filePath = dialog.FileName;

				try
				{
					if (!Directory.Exists("Plugins"))
						Directory.CreateDirectory("Plugins");

					if (Path.GetExtension(filePath) == ".zip")
						ZipFile.ExtractToDirectory(filePath, Path.Combine("Plugins", Path.GetFileNameWithoutExtension(filePath)));
					else if (Path.GetExtension(filePath) == ".rar")
					{
						// bruh...
					}
				}
				catch { }

				UpdateTreeView();
			}
		}

		private void UpdateTreeView()
		{
			treeView.Nodes.Clear();

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

				treeView.Nodes.Add(node);
			}
		}
	}
}
