using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TombIDE.Shared.SharedClasses
{
	public static class XmlHandling
	{
		/// <summary>
		/// Returns a list made up of projects taken from each .trproj file path stored in TombIDEProjects.xml.
		/// </summary>
		public static List<Project> GetProjectsFromXml()
		{
			string xmlPath = Path.Combine(DefaultPaths.GetConfigsPath(), "TombIDEProjects.xml");

			if (File.Exists(xmlPath))
			{
				List<string> projectFilePaths = (List<string>)ReadXmlFile(xmlPath, typeof(List<string>));
				// TombIDEProjects.xml only stores .trproj file paths

				List<Project> projectList = new List<Project>();

				foreach (string path in projectFilePaths)
				{
					if (File.Exists(path))
						projectList.Add(Project.FromFile(path));
				}

				return projectList;
			}
			else // TombIDEProjects.xml doesn't exist
			{
				// Create a new (empty) .xml file
				SaveXmlFile(xmlPath, typeof(List<string>), new List<string>());
				return new List<Project>();
			}
		}

		public static List<Plugin> GetPluginsFromXml()
		{
			string xmlPath = Path.Combine(DefaultPaths.GetConfigsPath(), "TombIDEPlugins.xml");

			if (File.Exists(xmlPath))
			{
				List<Plugin> pluginList = (List<Plugin>)ReadXmlFile(xmlPath, typeof(List<Plugin>));

				List<Plugin> validPlugins = new List<Plugin>();

				foreach (Plugin plugin in pluginList)
				{
					if (File.Exists(plugin.InternalDllPath))
						validPlugins.Add(plugin);
				}

				UpdatePluginsXml(validPlugins);

				return validPlugins;
			}
			else // TombIDEPlugins.xml doesn't exist
			{
				// Create a new (empty) .xml file
				SaveXmlFile(xmlPath, typeof(List<Plugin>), new List<Plugin>());
				return new List<Plugin>();
			}
		}

		/// <summary>
		/// Updates TombIDEProjects.xml with the projects' .trproj file paths.
		/// </summary>
		public static void UpdateProjectsXml(List<Project> projects)
		{
			List<string> projectFilePaths = new List<string>();

			foreach (Project project in projects)
				projectFilePaths.Add(project.GetTrprojFilePath());

			string xmlPath = Path.Combine(DefaultPaths.GetConfigsPath(), "TombIDEProjects.xml");
			SaveXmlFile(xmlPath, typeof(List<string>), projectFilePaths);
		}

		public static void UpdatePluginsXml(List<Plugin> pluginList)
		{
			string xmlPath = Path.Combine(DefaultPaths.GetConfigsPath(), "TombIDEPlugins.xml");
			SaveXmlFile(xmlPath, typeof(List<Plugin>), pluginList);
		}

		public static object ReadXmlFile(string path, Type type)
		{
			using (StreamReader reader = new StreamReader(path))
			{
				XmlSerializer serializer = new XmlSerializer(type);
				return serializer.Deserialize(reader);
			}
		}

		public static void SaveXmlFile(string path, Type type, object content)
		{
			using (StreamWriter writer = new StreamWriter(path))
			{
				XmlSerializer serializer = new XmlSerializer(type);
				serializer.Serialize(writer, content);
			}
		}
	}
}
