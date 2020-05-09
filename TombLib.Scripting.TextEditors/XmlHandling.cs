using System.IO;
using System.Xml.Serialization;

namespace TombLib.Scripting.TextEditors
{
	internal class XmlHandling
	{
		public static T ReadXmlFile<T>(string path)
		{
			using (StreamReader reader = new StreamReader(path))
				return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
		}

		public static void SaveXmlFile<T>(string path, T content)
		{
			using (StreamWriter writer = new StreamWriter(path))
				new XmlSerializer(typeof(T)).Serialize(writer, content);
		}
	}
}
