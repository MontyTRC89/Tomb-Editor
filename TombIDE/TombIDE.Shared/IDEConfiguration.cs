using DarkUI.Docking;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TombIDE.Shared
{
	public class IDEConfiguration
	{
		/* TombIDE */

		public bool Start_OpenMaximized { get; set; } = false;
		public Size Start_WindowSize { get; set; } = new Size(512, 512);
		public bool IDE_OpenMaximized { get; set; } = false;
		public Size IDE_WindowSize { get; set; } = new Size(1070, 640);

		public string RememberedProject { get; set; }

		public List<string> PinnedProgramPaths { get; set; }

		/* ProjectMaster */

		public bool ViewFileNames { get; set; } = true;
		public bool ViewFullFolderPaths { get; set; } = false;
		public bool LightModePreviewEnabled { get; set; } = false;
		public bool StandardAspectRatioPreviewEnabled { get; set; } = false;
		public bool PluginsPanelHidden { get; set; } = false;

		/// <summary>
		/// This can only be changed by directly modifying the .xml file
		/// </summary>
		public string ExternalLevelPrefix { get; set; } = "(Ext.) ";

		/* ScriptEditor */

		public bool UseNewIncludeMethod { get; set; } = true;
		public bool ShowCompilerLogsAfterBuild { get; set; } = true;
		public bool ReindentOnSave { get; set; } = false;

		public bool InfoBox_AlwaysOnTop { get; set; } = true;
		public bool InfoBox_CloseTabsOnClose { get; set; } = false;

		public DockPanelState CS_DockPanelState { get; set; } = DefaultLayouts.ClassicScriptLayout;
		public DockPanelState GFL_DockPanelState { get; set; } = DefaultLayouts.GameFlowScriptLayout;
		public DockPanelState T1M_DockPanelState { get; set; } = DefaultLayouts.Tomb1MainLayout;

		public static string GetDefaultPath()
		{
			return Path.Combine(DefaultPaths.ConfigsDirectory, "TombIDEConfiguration.xml");
		}

		public void Save(Stream stream)
		{
			new XmlSerializer(typeof(IDEConfiguration)).Serialize(stream, this);
		}

		public void Save(string path)
		{
			try
			{
				using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
					Save(stream);
			}
			catch (IOException)
			{
				if (!Directory.Exists(DefaultPaths.ConfigsDirectory))
					Directory.CreateDirectory(DefaultPaths.ConfigsDirectory);

				using (XmlWriter writer = XmlWriter.Create(path))
					new XmlSerializer(typeof(IDEConfiguration)).Serialize(writer, new IDEConfiguration());

				Save(path);
			}
		}

		public void Save()
		{
			Save(GetDefaultPath());
		}

		public static IDEConfiguration Load(Stream stream)
		{
			using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = false }))
				return (IDEConfiguration)new XmlSerializer(typeof(IDEConfiguration)).Deserialize(reader);
		}

		public static IDEConfiguration Load(string filePath)
		{
			try
			{
				using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					return Load(stream);
			}
			catch (IOException)
			{
				if (!Directory.Exists(DefaultPaths.ConfigsDirectory))
					Directory.CreateDirectory(DefaultPaths.ConfigsDirectory);

				using (XmlWriter writer = XmlWriter.Create(filePath))
					new XmlSerializer(typeof(IDEConfiguration)).Serialize(writer, new IDEConfiguration());

				return Load(filePath);
			}
		}

		public static IDEConfiguration Load()
		{
			return Load(GetDefaultPath());
		}
	}
}
