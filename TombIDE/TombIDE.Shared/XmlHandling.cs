using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TombLib.Projects;

namespace TombIDE.Shared
{
	public class XmlHandling
	{
		/// <summary>
		/// Returns a list made up of Projects taken from each .trproj file stored in TombIDEProjects.xml.
		/// </summary>
		public static List<Project> GetProjectsFromXml()
		{
			try
			{
				List<string> projectFilePaths = (List<string>)ReadXmlFile("TombIDEProjects.xml", typeof(List<string>));
				// TombIDEProjects.xml only stores .trproj file paths

				List<Project> projectList = new List<Project>();

				foreach (string path in projectFilePaths)
				{
					if (File.Exists(path))
						projectList.Add(ReadTRPROJ(path));
				}

				return projectList;
			}
			catch (FileNotFoundException) // TombIDEProjects.xml doesn't exist
			{
				// Create a new (empty) .xml file
				SaveXmlFile("TombIDEProjects.xml", typeof(List<string>), new List<string>());
				return new List<Project>();
			}
		}

		/// <summary>
		/// Updates TombIDEProjects.xml with the Projects' .trproj file paths.
		/// </summary>
		public static void UpdateProjectsXml(List<Project> projects)
		{
			List<string> projectFilePaths = new List<string>();

			foreach (Project proj in projects)
				projectFilePaths.Add(proj.GetTRPROJFilePath());

			SaveXmlFile("TombIDEProjects.xml", typeof(List<string>), projectFilePaths);
		}

		public static Project ReadTRPROJ(string path)
		{
			Project project = (Project)ReadXmlFile(path, typeof(Project));
			project.DecodeProjectPaths(path);

			return project;
		}

		public static void SaveTRPROJ(Project project)
		{
			Project projectCopy = project.Clone();
			projectCopy.EncodeProjectPaths();

			SaveXmlFile(project.GetTRPROJFilePath(), typeof(Project), projectCopy);
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
