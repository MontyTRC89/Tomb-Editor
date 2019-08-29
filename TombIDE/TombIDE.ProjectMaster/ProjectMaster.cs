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

		#region Initialization

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

			string pluginsFolderPath = Path.Combine(SharedMethods.GetProgramDirectory(), "TRNG Plugins");

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
			if (_ide.Project.GameVersion == GameVersion.TR4)
				splitContainer_Info.Panel2Collapsed = true;

			CheckPlugins();
		}

		#endregion Initialization

		#region Events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.RequestedPluginListRefreshEvent)
				CheckPlugins();
		}

		#endregion Events

		#region Watchers

		// Deleting .prj2 files is critical, so watch out
		private void prj2FileWatcher_Deleted(object sender, FileSystemEventArgs e) => _ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());
		private void levelFolderWatcher_Deleted(object sender, FileSystemEventArgs e) => _ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());

		// Plugin watchers
		private void projectDLLFileWatcher_Changed(object sender, FileSystemEventArgs e) => CheckPlugins();
		private void projectDLLFileWatcher_Renamed(object sender, RenamedEventArgs e) => CheckPlugins();

		private void internalDLLFileWatcher_Deleted(object sender, FileSystemEventArgs e) => CheckPlugins();
		private void internalDLLFileWatcher_Renamed(object sender, RenamedEventArgs e) => CheckPlugins();

		private void internalPluginFolderWatcher_Deleted(object sender, FileSystemEventArgs e) => CheckPlugins();
		private void internalPluginFolderWatcher_Renamed(object sender, RenamedEventArgs e) => CheckPlugins();

		#endregion Watchers

		#region Plugin handling

		/// <summary>
		/// Updates all plugin lists.
		/// </summary>
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

			DeleteReferenceFilesOfInvalidPlugins(invalidPlugins);

			_ide.AvailablePlugins.Clear();
			_ide.AvailablePlugins.AddRange(validPlugins);

			CopyMissingReferenceFiles();
		}

		/// <summary>
		/// Copies missing .script files into the /NGC/ folder.
		/// </summary>
		private void CopyMissingReferenceFiles()
		{
			foreach (Plugin plugin in _ide.AvailablePlugins)
			{
				string scriptFilePath = Path.Combine(
					Path.GetDirectoryName(plugin.InternalDllPath), Path.GetFileNameWithoutExtension(plugin.InternalDllPath) + ".script");

				if (File.Exists(scriptFilePath))
				{
					string destPath = Path.Combine(SharedMethods.GetProgramDirectory(), "NGC", Path.GetFileName(scriptFilePath));
					File.Copy(scriptFilePath, destPath, true);
				}
			}
		}

		/// <summary>
		/// Deletes .script files of invalid plugins from the /NGC/ folder.
		/// </summary>
		private void DeleteReferenceFilesOfInvalidPlugins(List<Plugin> invalidPlugins)
		{
			foreach (Plugin plugin in invalidPlugins)
			{
				string internalScriptFilePath = Path.Combine(
					SharedMethods.GetProgramDirectory(), "NGC", Path.GetFileNameWithoutExtension(plugin.InternalDllPath) + ".script");

				if (File.Exists(internalScriptFilePath))
					File.Delete(internalScriptFilePath);
			}
		}

		/// <summary>
		/// Looks for potential plugin folders inside TombIDE's internal /Plugins/ folder.
		/// <para>If a plugin folder is valid, it's added to the AvailablePlugins list.</para>
		/// </summary>
		private void LookForUndefinedPlugins()
		{
			foreach (string directory in Directory.GetDirectories(Path.Combine(SharedMethods.GetProgramDirectory(), "TRNG Plugins")))
			{
				if (!IsValidPluginFolder(directory))
					continue;

				if (IsPluginFolderAlreadyDefined(directory))
					continue;

				if (IsDLLFileADuplicate(directory))
					continue;

				Plugin plugin = Plugin.InstallPluginFolder(directory);
				_ide.AvailablePlugins.Add(plugin);
			}
		}

		private void UpdateProjectPluginList()
		{
			List<Plugin> projectPlugins = new List<Plugin>();

			foreach (string pluginFile in Directory.GetFiles(_ide.Project.ProjectPath, "plugin_*.dll", SearchOption.TopDirectoryOnly))
			{
				// Check if the plugin is available in TombIDE
				Plugin plugin = _ide.AvailablePlugins.Find(x => Path.GetFileName(x.InternalDllPath).ToLower() == Path.GetFileName(pluginFile).ToLower());

				if (plugin != null)
					projectPlugins.Add(plugin);
				else // The plugin's DLL file is unknown for TombIDE
				{
					plugin = new Plugin
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
			return _ide.AvailablePlugins.Exists(x => Path.GetDirectoryName(x.InternalDllPath).ToLower() == path.ToLower());
		}

		private bool IsDLLFileADuplicate(string pluginFolderPath)
		{
			string dllFileName = Path.GetFileName(Directory.GetFiles(pluginFolderPath, "plugin_*.dll").First());
			return _ide.AvailablePlugins.Exists(x => Path.GetFileName(x.InternalDllPath).ToLower() == dllFileName.ToLower());
		}

		#endregion Plugin handling
	}
}
