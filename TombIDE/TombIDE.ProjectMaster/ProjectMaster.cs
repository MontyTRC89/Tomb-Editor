using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;
using TombLib.Projects;

namespace TombIDE.ProjectMaster
{
	public partial class ProjectMaster : UserControl
	{
		private IDE _ide;

		public ProjectMaster()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			// Initialize the watchers
			prj2FileWatcher.Path = _ide.Project.LevelsPath;
			levelFolderWatcher.Path = _ide.Project.LevelsPath;

			string pluginsFolderPath = Path.Combine(SharedMethods.GetProgramDirectory(), "Plugins");

			if (!Directory.Exists(pluginsFolderPath))
				Directory.CreateDirectory(pluginsFolderPath);

			projectDLLFileWatcher.Path = _ide.Project.ProjectPath;
			internalDLLFileWatcher.Path = pluginsFolderPath;
			internalPluginFolderWatcher.Path = pluginsFolderPath;

			// Initialize the sections
			section_LevelList.Initialize(_ide);
			section_LevelProperties.Initialize(_ide);
			section_ProjectInfo.Initialize(_ide);
			section_PluginList.Initialize(_ide);

			// Collapse the "Plugins" section if the engine doesn't support plugins
			if (_ide.Project.GameVersion == GameVersion.TR4 || _ide.Project.GameVersion == GameVersion.TR5)
				splitContainer_Info.Panel2Collapsed = true;

			CheckPlugins();
		}

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.RequestedPluginListRefreshEvent)
				CheckPlugins();
		}

		// Deleting .prj2 files is critical, so watch out
		private void prj2FileWatcher_Deleted(object sender, FileSystemEventArgs e) => _ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());
		private void levelFolderWatcher_Deleted(object sender, FileSystemEventArgs e) => _ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());

		// Plugin watchers
		private void projectDLLFileWatcher_Changed(object sender, FileSystemEventArgs e) => CheckPlugins();
		private void projectDLLFileWatcher_Renamed(object sender, RenamedEventArgs e) => CheckPlugins();

		private void internalDLLFileWatcher_Changed(object sender, FileSystemEventArgs e) => CheckPlugins();
		private void internalDLLFileWatcher_Renamed(object sender, RenamedEventArgs e) => CheckPlugins();

		private void internalPluginFolderWatcher_Changed(object sender, FileSystemEventArgs e) => CheckPlugins();
		private void internalPluginFolderWatcher_Renamed(object sender, RenamedEventArgs e) => CheckPlugins();

		private void CheckPlugins()
		{
			UpdateInternalPluginList();
			LookForUndefinedPlugins();

			_ide.AvailablePlugins.Sort(delegate (Plugin p1, Plugin p2) { return p1.Name.CompareTo(p2.Name); });

			UpdateProjectPluginList();

			_ide.Project.InstalledPlugins.Sort(delegate (Plugin p1, Plugin p2) { return p1.Name.CompareTo(p2.Name); });

			XmlHandling.UpdatePluginsXml(_ide.AvailablePlugins);
			XmlHandling.SaveTRPROJ(_ide.Project);

			_ide.RaiseEvent(new IDE.PluginListsUpdatedEvent());
		}

		/// <summary>
		/// Removes invalid plugins from the AvailablePlugins list.
		/// <para>Also handles .script reference files.</para>
		/// </summary>
		private void UpdateInternalPluginList()
		{
			List<Plugin> validPlugins = new List<Plugin>();
			List<Plugin> invalidPlugins = new List<Plugin>();

			foreach (Plugin plugin in _ide.AvailablePlugins)
			{
				if (File.Exists(plugin.InternalDllPath))
					validPlugins.Add(plugin);
				else
					invalidPlugins.Add(plugin);
			}

			// Copy missing .script reference files into the NGC folder

			List<Plugin> validPluginsLoopList = new List<Plugin>();
			validPluginsLoopList.AddRange(validPlugins);

			foreach (Plugin plugin in validPluginsLoopList)
			{
				List<string> invalidReferenceFiles = new List<string>();

				foreach (string referenceFile in plugin.ReferenceFilePaths)
				{
					if (!File.Exists(referenceFile))
					{
						try
						{
							string internalReferenceFilePath = Path.Combine(Path.GetDirectoryName(plugin.InternalDllPath), Path.GetFileName(referenceFile));
							File.Copy(internalReferenceFilePath, referenceFile);
						}
						catch (Exception ex)
						{
							DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							invalidReferenceFiles.Add(referenceFile);
						}
					}
				}

				if (invalidReferenceFiles.Count > 0)
				{
					validPlugins.Remove(plugin);

					foreach (string invalidFile in invalidReferenceFiles)
						plugin.ReferenceFilePaths.Remove(invalidFile);

					validPlugins.Add(plugin);
				}
			}

			// Delete .script files of invalid plugins from the NGC folder
			foreach (Plugin plugin in invalidPlugins)
			{
				foreach (string referenceFile in plugin.ReferenceFilePaths)
				{
					if (File.Exists(referenceFile))
						File.Delete(referenceFile);
				}
			}

			_ide.AvailablePlugins.Clear();
			_ide.AvailablePlugins.AddRange(validPlugins);
		}

		/// <summary>
		/// Looks for potential plugin folders inside TombIDE's internal /Plugins/ folder.
		/// <para>If a plugin folder is valid, it's added to the AvailablePlugins list.</para>
		/// </summary>
		private void LookForUndefinedPlugins()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(SharedMethods.GetProgramDirectory(), "Plugins"));

			foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
			{
				if (!IsValidPluginFolder(directory.FullName))
					continue;

				if (IsPluginFolderAlreadyDefined(directory.FullName))
					continue;

				if (IsDLLFileADuplicate(directory.FullName))
					continue;

				Plugin plugin = Plugin.InstallPluginFolder(directory.FullName);
				_ide.AvailablePlugins.Add(plugin);
			}
		}

		private void UpdateProjectPluginList()
		{
			List<Plugin> projectPlugins = new List<Plugin>();

			foreach (string pluginFile in Directory.GetFiles(_ide.Project.ProjectPath, "plugin_*.dll", SearchOption.TopDirectoryOnly))
			{
				bool pluginFound = false;

				foreach (Plugin availablePlugin in _ide.AvailablePlugins)
				{
					if (Path.GetFileName(availablePlugin.InternalDllPath.ToLower()) == Path.GetFileName(pluginFile.ToLower()))
					{
						projectPlugins.Add(availablePlugin);
						pluginFound = true;
						break;
					}
				}

				if (!pluginFound)
				{
					Plugin plugin = new Plugin
					{
						Name = Path.GetFileName(pluginFile)
					};

					projectPlugins.Add(plugin);
				}
			}

			_ide.Project.InstalledPlugins = projectPlugins;
		}

		private bool IsValidPluginFolder(string path)
		{
			return Directory.GetFiles(path, "plugin_*.dll", SearchOption.TopDirectoryOnly).Length > 0;
		}

		private bool IsPluginFolderAlreadyDefined(string path)
		{
			foreach (Plugin plugin in _ide.AvailablePlugins)
			{
				if (Path.GetDirectoryName(plugin.InternalDllPath) == path)
					return true;
			}

			return false;
		}

		private bool IsDLLFileADuplicate(string pluginFolderPath)
		{
			foreach (Plugin plugin in _ide.AvailablePlugins)
			{
				string existingDLLFileName = Path.GetFileName(plugin.InternalDllPath.ToLower());
				string installedDLLFileName = Path.GetFileName(Directory.GetFiles(pluginFolderPath, "plugin_*.dll").First().ToLower());

				if (existingDLLFileName == installedDLLFileName)
					return true;
			}

			return false;
		}
	}
}
