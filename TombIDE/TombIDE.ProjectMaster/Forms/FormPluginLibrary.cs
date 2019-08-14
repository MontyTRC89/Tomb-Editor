using DarkUI.Controls;
using DarkUI.Forms;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Projects;
using TombLib.Utils;

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
				Title = "Select the archive of the plugin you want to install",
				Filter = "All Supported Files|*.zip;*.rar;*.7z|ZIP Archives|*.zip|RAR Archives|*.rar|7ZIP Archives|*.7z"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				string filePath = dialog.FileName;

				try
				{
					if (!Directory.Exists("Plugins"))
						Directory.CreateDirectory("Plugins");

					foreach (string directory in Directory.GetDirectories("Plugins"))
					{
						if (Path.GetFileName(directory) == Path.GetFileNameWithoutExtension(filePath))
							throw new ArgumentException("Plugin already installed.");
					}

					using (IArchive archive = ArchiveFactory.Open(filePath))
					{
						if (!IsValidPluginArchive(archive))
							throw new ArgumentException("Selected archive doesn't contain a valid plugin DLL file.");

						string extractionPath = Path.Combine("Plugins", Path.GetFileNameWithoutExtension(filePath));

						foreach (IArchiveEntry entry in archive.Entries)
						{
							if (!entry.IsDirectory)
								entry.WriteToDirectory(extractionPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
						}

						if (Directory.GetFileSystemEntries(extractionPath).Length == 1)
						{
							string nextFolderPath = Path.Combine(extractionPath, Path.GetFileName(Directory.GetFileSystemEntries(extractionPath).First()));
							string cachedFolderPath = nextFolderPath;

							while (Directory.GetFileSystemEntries(nextFolderPath).Length == 1)
							{
								string nextEntry = Path.Combine(nextFolderPath, Path.GetFileName(Directory.GetFileSystemEntries(nextFolderPath).First()));

								if ((File.GetAttributes(nextEntry) & FileAttributes.Directory) == FileAttributes.Directory)
									nextFolderPath = nextEntry;
								else if (!Path.GetFileName(nextEntry).ToLower().StartsWith("plugin_") || !Path.GetFileName(nextEntry).ToLower().EndsWith(".dll"))
								{
									Directory.Delete(extractionPath, true);
									throw new ArgumentException("Selected archive doesn't contain a valid plugin DLL file.");
								}
							}

							if (!IsValidPluginFolder(nextFolderPath))
							{
								Directory.Delete(extractionPath, true);
								throw new ArgumentException("Selected archive doesn't contain a valid plugin DLL file.");
							}

							foreach (string file in Directory.GetFiles(nextFolderPath))
								File.Move(file, Path.Combine(extractionPath, Path.GetFileName(file)));

							foreach (string directory in Directory.GetDirectories(nextFolderPath))
								Directory.Move(directory, Path.Combine(extractionPath, Path.GetFileName(directory)));

							Directory.Delete(cachedFolderPath, true);
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

						string dllFilePath = Directory.GetFiles(extractionPath, "plugin_*.dll").First();

						Plugin plugin = new Plugin
						{
							Name = pluginName,
							InternalDllPath = dllFilePath
						};

						_ide.AvailablePlugins.Add(plugin);
						XmlHandling.UpdatePluginsXml(_ide.AvailablePlugins);
					}
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				UpdateAvailablePluginsTreeView();
			}
		}

		private bool IsValidPluginArchive(IArchive archive)
		{
			foreach (IArchiveEntry entry in archive.Entries)
			{
				if (Path.GetFileName(entry.Key).ToLower().StartsWith("plugin_") && Path.GetFileName(entry.Key).ToLower().EndsWith(".dll"))
					return true;
			}

			return false;
		}

		private bool IsValidPluginFolder(string path)
		{
			return Directory.GetFiles(path, "plugin_*.dll", SearchOption.AllDirectories).Length > 0;
		}

		private void button_Install_Click(object sender, EventArgs e)
		{
			foreach (DarkTreeNode node in treeView_AvailablePlugins.SelectedNodes)
			{
				try
				{
					string pluginFolderPath = Path.GetDirectoryName(((Plugin)node.Tag).InternalDllPath);
					string dllFilePath = Directory.GetFiles(pluginFolderPath, "plugin_*.dll").First();

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

		private void button_Download_Click(object sender, EventArgs e) =>
			Process.Start("https://www.tombraiderforums.com/showpost.php?p=7636390");

		private void dllFileWatcher_Deleted(object sender, FileSystemEventArgs e) =>
			_ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());

		private void pluginFolderWatcher_Deleted(object sender, FileSystemEventArgs e) =>
			_ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());

		private void Button_OpenFolder_Click(object sender, EventArgs e)
		{
			BrowseFolderDialog dialog = new BrowseFolderDialog
			{
				Title = "Select the folder of the plugin you want to install"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				string folderPath = dialog.Folder;

				try
				{
					if (!Directory.Exists("Plugins"))
						Directory.CreateDirectory("Plugins");

					foreach (string directory in Directory.GetDirectories("Plugins"))
					{
						if (Path.GetFileName(directory) == Path.GetFileName(folderPath))
							throw new ArgumentException("Plugin already installed.");
					}

					if (!IsValidPluginFolder(folderPath))
						throw new ArgumentException("Selected folder doesn't contain a valid plugin DLL file.");

					string extractionPath = Path.Combine("Plugins", Path.GetFileName(folderPath));

					if (Directory.GetFileSystemEntries(extractionPath).Length == 1)
					{
						string nextFolderPath = Path.Combine(extractionPath, Path.GetFileName(Directory.GetFileSystemEntries(extractionPath).First()));
						string cachedFolderPath = nextFolderPath;

						while (Directory.GetFileSystemEntries(nextFolderPath).Length == 1)
						{
							string nextEntry = Path.Combine(nextFolderPath, Path.GetFileName(Directory.GetFileSystemEntries(nextFolderPath).First()));

							if ((File.GetAttributes(nextEntry) & FileAttributes.Directory) == FileAttributes.Directory)
								nextFolderPath = nextEntry;
							else if (!Path.GetFileName(nextEntry).ToLower().StartsWith("plugin_") || !Path.GetFileName(nextEntry).ToLower().EndsWith(".dll"))
							{
								Directory.Delete(extractionPath, true);
								throw new ArgumentException("Selected folder doesn't contain a valid plugin DLL file.");
							}
						}

						if (!IsValidPluginFolder(nextFolderPath))
						{
							Directory.Delete(extractionPath, true);
							throw new ArgumentException("Selected archive doesn't contain a valid plugin DLL file.");
						}

						foreach (string file in Directory.GetFiles(nextFolderPath))
							File.Move(file, Path.Combine(extractionPath, Path.GetFileName(file)));

						foreach (string directory in Directory.GetDirectories(nextFolderPath))
							Directory.Move(directory, Path.Combine(extractionPath, Path.GetFileName(directory)));

						Directory.Delete(cachedFolderPath, true);
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

					string dllFilePath = Directory.GetFiles(extractionPath, "plugin_*.dll").First();

					Plugin plugin = new Plugin
					{
						Name = pluginName,
						InternalDllPath = dllFilePath
					};

					_ide.AvailablePlugins.Add(plugin);
					XmlHandling.UpdatePluginsXml(_ide.AvailablePlugins);
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				UpdateAvailablePluginsTreeView();
			}
		}
	}
}
