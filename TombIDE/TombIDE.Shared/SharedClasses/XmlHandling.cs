using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Utils;

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
				List<string> projectFilePaths = XmlUtils.ReadXmlFile<List<string>>(xmlPath);
				// TombIDEProjects.xml only stores .trproj file paths

				return projectFilePaths
					.Where(path => File.Exists(path))
					.Select(path => Project.FromFile(path));
			}
			catch
			{
				// Create a new (empty) .xml file
				XmlUtils.WriteXmlFile(xmlPath, new List<string>());
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
			XmlUtils.WriteXmlFile(xmlPath, projectFilePaths.ToList());
		}
	}
}
