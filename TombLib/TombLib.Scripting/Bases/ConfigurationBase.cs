using System.IO;
using TombLib.Utils;

namespace TombLib.Scripting.Bases
{
	public abstract class ConfigurationBase
	{
		public abstract string DefaultPath { get; }

		#region Loading

		public T Load<T>(Stream stream) where T : ConfigurationBase, new()
			=> XmlUtils.ReadXmlFile<T>(stream);

		public T Load<T>(string filePath) where T : ConfigurationBase, new()
		{
			if (!File.Exists(filePath))
				XmlUtils.WriteXmlFile(filePath, new T());

			return XmlUtils.ReadXmlFile<T>(filePath);
		}

		public T Load<T>() where T : ConfigurationBase, new()
			=> Load<T>(DefaultPath);

		#endregion Loading

		#region Saving

		public void Save(Stream stream)
			=> XmlUtils.WriteXmlFile(stream, GetType(), this);

		public void Save(string path)
		{
			string directoryName = Path.GetDirectoryName(path);

			if (!Directory.Exists(directoryName))
				Directory.CreateDirectory(directoryName);

			XmlUtils.WriteXmlFile(path, GetType(), this);
		}

		public void Save()
			=> Save(DefaultPath);

		#endregion Saving
	}
}
