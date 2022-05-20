using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace TombIDE.Shared.SharedClasses
{
	public static class XmlHandling
	{
		/// <summary>
		/// Returns a list made up of projects taken from each .trproj file path stored in TombIDEProjects.xml.
		/// </summary>
		public static IEnumerable<Project> GetProjectsFromXml()
		{
			string xmlPath = Path.Combine(DefaultPaths.ConfigsDirectory, "TombIDEProjects.xml");

			try
			{
				List<string> projectFilePaths = ReadXmlFile<List<string>>(xmlPath);
				// TombIDEProjects.xml only stores .trproj file paths

				return projectFilePaths
					.Where(path => File.Exists(path))
					.Select(path => Project.FromFile(path));
			}
			catch
			{
				// Create a new (empty) .xml file
				SaveXmlFile(xmlPath, new List<string>());
				return new List<Project>();
			}
		}

		/// <summary>
		/// Updates TombIDEProjects.xml with the projects' .trproj file paths.
		/// </summary>
		public static void UpdateProjectsXml(List<Project> projects)
		{
			IEnumerable<string> projectFilePaths = projects
				.Select(project => project.GetTrprojFilePath());

			string xmlPath = Path.Combine(DefaultPaths.ConfigsDirectory, "TombIDEProjects.xml");
			SaveXmlFile(xmlPath, projectFilePaths.ToList());
		}

		public static T ReadXmlFile<T>(string path)
		{
			using (var reader = new StreamReader(path))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(reader);
			}
		}

		public static void SaveXmlFile<T>(string path, T content)
		{
			using (var writer = new StreamWriter(path))
			{
				var serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(writer, content);
			}
		}
	}
}
