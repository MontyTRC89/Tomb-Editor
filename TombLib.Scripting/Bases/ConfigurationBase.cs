using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TombLib.Scripting.Bases
{
	public abstract class ConfigurationBase
	{
		public abstract string DefaultPath { get; }

		#region Loading

		public T Load<T>(Stream stream) where T : ConfigurationBase
			=> new XmlSerializer(GetType()).Deserialize(stream) as T;

		public T Load<T>(string filePath) where T : ConfigurationBase
		{
			try
			{
				using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					return Load<T>(stream);
			}
			catch (IOException)
			{
				if (!Directory.Exists(Path.GetDirectoryName(filePath)))
					Directory.CreateDirectory(Path.GetDirectoryName(filePath));

				using (var writer = XmlWriter.Create(filePath))
					new XmlSerializer(GetType()).Serialize(writer, this);

				return Load<T>(filePath);
			}
		}

		public T Load<T>() where T : ConfigurationBase
			=> Load<T>(DefaultPath);

		#endregion Loading

		#region Saving

		public void Save(Stream stream)
			=> new XmlSerializer(GetType()).Serialize(stream, this);

		public void Save(string path)
		{
			try
			{
				using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
					Save(stream);
			}
			catch (IOException)
			{
				if (!Directory.Exists(Path.GetDirectoryName(path)))
					Directory.CreateDirectory(Path.GetDirectoryName(path));

				using (var writer = XmlWriter.Create(path))
					new XmlSerializer(GetType()).Serialize(writer, this);

				Save(path);
			}
		}

		public void Save()
			=> Save(DefaultPath);

		#endregion Saving
	}
}
