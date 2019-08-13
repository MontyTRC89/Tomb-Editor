using DarkUI.Controls;
using DarkUI.Forms;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Projects;

namespace TombIDE.ProjectMaster
{
	public partial class FormPluginLibrary : DarkForm
	{
		private IDE _ide;

		public FormPluginLibrary(IDE ide)
		{
			_ide = ide;

			InitializeComponent();

			UpdateAvailablePluginsTreeView();
			UpdateInstalledPluginsTreeView();
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
							foreach (string directory in Directory.GetDirectories("Plugins"))
							{
								if (Path.GetFileName(directory) == Path.GetFileNameWithoutExtension(filePath))
									throw new ArgumentException("Plugin already installed.");
							}

							string extractionPath = Path.Combine("Plugins", Path.GetFileNameWithoutExtension(filePath));

							while (reader.MoveToNextEntry())
							{
								if (!reader.Entry.IsDirectory)
									reader.WriteEntryToDirectory(extractionPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
							}

							foreach (string file in Directory.GetFiles(extractionPath, "*.script"))
								File.Copy(file, Path.Combine("NGC", Path.GetFileName(file)), true);

							string pluginName = Path.GetFileName(extractionPath);

							if (Directory.GetFiles(extractionPath, "*.btn").Length > 0)
							{
								string btnFilePath = Directory.GetFiles(extractionPath, "*.btn").First();
								string[] btnFileContent = File.ReadAllLines(btnFilePath, Encoding.GetEncoding(1252));

								foreach (string line in btnFileContent)
								{
									if (line.StartsWith("NAME#"))
									{
										pluginName = line.Replace("NAME#", string.Empty).Trim();
										break;
									}
								}
							}

							string dllFilePath = Directory.GetFiles(extractionPath, "*.dll").First();

							Plugin plugin = new Plugin
							{
								Name = pluginName,
								InternalDllPath = dllFilePath
							};

							_ide.AvailablePlugins.Add(plugin);
							XmlHandling.SaveXmlFile("TombIDEPlugins.xml", typeof(List<Plugin>), _ide.AvailablePlugins);
						}
					}
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				UpdateAvailablePluginsTreeView();
			}
		}

		private void button_Install_Click(object sender, EventArgs e)
		{
			foreach (DarkTreeNode node in treeView_AvailablePlugins.SelectedNodes)
			{
				try
				{
					string pluginFolderPath = Path.GetDirectoryName(((Plugin)node.Tag).InternalDllPath);
					string dllFilePath = Directory.GetFiles(pluginFolderPath, "*.dll").First();

					File.Copy(dllFilePath, Path.Combine(_ide.Project.ProjectPath, Path.GetFileName(dllFilePath)), true);

					_ide.Project.InstalledPlugins.Add((Plugin)node.Tag);
					XmlHandling.SaveTRPROJ(_ide.Project);
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			UpdateAvailablePluginsTreeView();
			UpdateInstalledPluginsTreeView();
		}

		private void button_Uninstall_Click(object sender, EventArgs e)
		{
			foreach (DarkTreeNode node in treeView_Installed.SelectedNodes)
			{
				try
				{
					string pluginFolderPath = Path.GetDirectoryName(((Plugin)node.Tag).InternalDllPath);
					string dllFilePath = Directory.GetFiles(pluginFolderPath, "*.dll").First();

					string dllProjectPath = Path.Combine(_ide.Project.ProjectPath, Path.GetFileName(dllFilePath));

					if (File.Exists(dllProjectPath))
						File.Delete(dllProjectPath);

					_ide.Project.InstalledPlugins.Remove((Plugin)node.Tag);
					XmlHandling.SaveTRPROJ(_ide.Project);
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			UpdateAvailablePluginsTreeView();
			UpdateInstalledPluginsTreeView();
		}

		private void UpdateAvailablePluginsTreeView()
		{
			treeView_AvailablePlugins.Nodes.Clear();

			foreach (Plugin availablePlugin in _ide.AvailablePlugins)
			{
				bool isPluginInstalled = false;

				foreach (Plugin installedPlugin in _ide.Project.InstalledPlugins)
				{
					if (availablePlugin.InternalDllPath == installedPlugin.InternalDllPath)
					{
						isPluginInstalled = true;
						break;
					}
				}

				if (isPluginInstalled)
					continue;

				DarkTreeNode node = new DarkTreeNode(availablePlugin.Name)
				{
					Tag = availablePlugin
				};

				treeView_AvailablePlugins.Nodes.Add(node);
			}

			treeView_AvailablePlugins.Invalidate();
		}

		private void UpdateInstalledPluginsTreeView()
		{
			treeView_Installed.Nodes.Clear();

			foreach (Plugin plugin in _ide.Project.InstalledPlugins)
			{
				DarkTreeNode node = new DarkTreeNode(plugin.Name)
				{
					Tag = plugin
				};

				treeView_Installed.Nodes.Add(node);
			}

			treeView_Installed.Invalidate();
		}
	}
}
