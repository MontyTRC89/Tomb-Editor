using DarkUI.Controls;
using DarkUI.Forms;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Projects;
using TombLib.Utils;

namespace TombIDE.ProjectMaster
{
	public partial class FormPluginLibrary : DarkForm
	{
		private IDE _ide;

		#region Initialization

		public FormPluginLibrary(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			InitializeComponent();

			UpdateAvailablePluginsTreeView();
			UpdateInstalledPluginsTreeView();
		}

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.PluginListsUpdatedEvent)
			{
				UpdateAvailablePluginsTreeView();
				UpdateInstalledPluginsTreeView();
			}
		}

		#endregion Initialization

		#region Events

		private void button_OpenArchive_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the archive of the plugin you want to install";
				dialog.Filter = "All Supported Files|*.zip;*.rar;*.7z|ZIP Archives|*.zip|RAR Archives|*.rar|7-Zip Archives|*.7z";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					string filePath = dialog.FileName;
					string pluginsFolderPath = Path.Combine(SharedMethods.GetProgramDirectory(), "Plugins");

					try
					{
						if (!Directory.Exists(pluginsFolderPath))
							Directory.CreateDirectory(pluginsFolderPath);

						foreach (string directory in Directory.GetDirectories(pluginsFolderPath))
						{
							if (Path.GetFileName(directory) == Path.GetFileNameWithoutExtension(filePath))
								throw new ArgumentException("Plugin already installed.");
						}

						using (IArchive archive = ArchiveFactory.Open(filePath))
						{
							if (!IsValidPluginArchive(archive))
								throw new ArgumentException("Selected archive doesn't contain a valid plugin DLL file.");

							string extractionPath = Path.Combine(pluginsFolderPath, Path.GetFileNameWithoutExtension(filePath));

							foreach (IArchiveEntry entry in archive.Entries)
							{
								if (!entry.IsDirectory)
									entry.WriteToDirectory(extractionPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
							}

							ValidatePluginFolder(extractionPath, true);

							Plugin plugin = Plugin.InstallPluginFolder(extractionPath);

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
		}

		private void button_OpenFolder_Click(object sender, EventArgs e)
		{
			using (BrowseFolderDialog dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Select the folder of the plugin you want to install";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					string folderPath = dialog.Folder;
					string pluginsFolderPath = Path.Combine(SharedMethods.GetProgramDirectory(), "Plugins");

					try
					{
						if (!Directory.Exists(pluginsFolderPath))
							Directory.CreateDirectory(pluginsFolderPath);

						foreach (string directory in Directory.GetDirectories(pluginsFolderPath))
						{
							if (Path.GetFileName(directory) == Path.GetFileName(folderPath))
								throw new ArgumentException("Plugin already installed.");
						}

						if (!IsValidPluginFolder(folderPath))
							throw new ArgumentException("Selected folder doesn't contain a valid plugin DLL file.");

						string extractionPath = Path.Combine(pluginsFolderPath, Path.GetFileName(folderPath));

						ValidatePluginFolder(extractionPath, true);

						Plugin plugin = Plugin.InstallPluginFolder(extractionPath);

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

		private void button_Download_Click(object sender, EventArgs e) =>
			Process.Start("https://www.tombraiderforums.com/showpost.php?p=7636390");

		#endregion Events

		private void ValidatePluginFolder(string pluginFolderPath, bool extractedFromArchive = false)
		{
			if (Directory.GetFileSystemEntries(pluginFolderPath).Length == 1)
			{
				string nextFolderPath = Path.Combine(pluginFolderPath, Path.GetFileName(Directory.GetFileSystemEntries(pluginFolderPath).First()));
				string cachedFolderPath = nextFolderPath;

				while (Directory.GetFileSystemEntries(nextFolderPath).Length == 1)
				{
					string nextEntry = Path.Combine(nextFolderPath, Path.GetFileName(Directory.GetFileSystemEntries(nextFolderPath).First()));

					if ((File.GetAttributes(nextEntry) & FileAttributes.Directory) == FileAttributes.Directory)
						nextFolderPath = nextEntry;
					else if (!Path.GetFileName(nextEntry).ToLower().StartsWith("plugin_") || !Path.GetFileName(nextEntry).ToLower().EndsWith(".dll"))
					{
						Directory.Delete(pluginFolderPath, true);
						throw new ArgumentException(string.Format("Selected {0} doesn't contain a valid plugin DLL file.",
							extractedFromArchive ? "archive" : "folder"));
					}
				}

				if (!IsValidPluginFolder(nextFolderPath))
				{
					Directory.Delete(pluginFolderPath, true);
					throw new ArgumentException(string.Format("Selected {0} doesn't contain a valid plugin DLL file.",
						extractedFromArchive ? "archive" : "folder"));
				}

				foreach (string file in Directory.GetFiles(nextFolderPath))
					File.Move(file, Path.Combine(pluginFolderPath, Path.GetFileName(file)));

				foreach (string directory in Directory.GetDirectories(nextFolderPath))
					Directory.Move(directory, Path.Combine(pluginFolderPath, Path.GetFileName(directory)));

				Directory.Delete(cachedFolderPath, true);
			}

			// Check for DLL duplicates
			foreach (Plugin plugin in _ide.AvailablePlugins)
			{
				string existingDLLFileName = Path.GetFileName(plugin.InternalDllPath.ToLower());
				string installedDLLFileName = Path.GetFileName(Directory.GetFiles(pluginFolderPath, "plugin_*.dll").First().ToLower());

				if (existingDLLFileName == installedDLLFileName)
				{
					Directory.Delete(pluginFolderPath, true);
					throw new ArgumentException(string.Format("Selected {0} has the same DLL file as the\n" +
						"\"{1}\" plugin.", extractedFromArchive ? "archive" : "folder", plugin.Name));
				}
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

		private void button_Delete_Click(object sender, EventArgs e)
		{
		}

		private void button_OpenInExplorer_Click(object sender, EventArgs e)
		{
		}

		private void button_OpenInExplorer_Click_1(object sender, EventArgs e)
		{
		}
	}
}
