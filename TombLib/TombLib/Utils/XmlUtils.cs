using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TombLib.Utils
{
	public static class XmlUtils
	{
		private static readonly XmlWriterSettings ReadableOutputSettings = new XmlWriterSettings()
		{
			Indent = true,
			IndentChars = "\t"
		};

		public static bool IsXmlDocument(string filePath, out XmlDocument document)
		{
			document = new XmlDocument();

			try
			{
				document.Load(filePath);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#region Normal Read

		public static object ReadXmlFile(string filePath, Type type)
			=> ReadXmlFile(filePath, type, null);

		public static object ReadXmlFile(Stream stream, Type type)
			=> ReadXmlFile(stream, type, null);

		public static object ReadXmlFile(string filePath, Type type, XmlReaderSettings settings)
		{
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				return ReadXmlFile(stream, type, settings);
		}

		public static object ReadXmlFile(Stream stream, Type type, XmlReaderSettings settings)
		{
			using (var reader = XmlReader.Create(stream, settings))
			{
				var serializer = new XmlSerializer(type);
				return serializer.Deserialize(reader);
			}
		}

		#endregion Normal Read

		#region Generic Read

		public static T ReadXmlFile<T>(string filePath)
			=> (T)ReadXmlFile(filePath, typeof(T));

		public static T ReadXmlFile<T>(Stream stream)
			=> (T)ReadXmlFile(stream, typeof(T));

		public static T ReadXmlFile<T>(string filePath, XmlReaderSettings settings)
			=> (T)ReadXmlFile(filePath, typeof(T), settings);

		public static T ReadXmlFile<T>(Stream stream, XmlReaderSettings settings)
			=> (T)ReadXmlFile(stream, typeof(T), settings);

		#endregion Generic Read

		#region Normal Write

		public static void WriteXmlFile(string filePath, Type type, object content)
			=> WriteXmlFile(filePath, type, content, ReadableOutputSettings);

		public static void WriteXmlFile(Stream stream, Type type, object content)
			=> WriteXmlFile(stream, type, content, ReadableOutputSettings);

		public static void WriteXmlFile(string filePath, Type type, object content, XmlWriterSettings settings)
		{
			using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				WriteXmlFile(stream, type, content, settings);
		}

		public static void WriteXmlFile(Stream stream, Type type, object content, XmlWriterSettings settings)
		{
			using (var writer = XmlWriter.Create(stream, settings))
			{
				var serializer = new XmlSerializer(type);
				serializer.Serialize(writer, content);
			}
		}

		#endregion Normal Write

		#region Generic Write

		public static void WriteXmlFile<T>(string filePath, T content)
			=> WriteXmlFile(filePath, typeof(T), content);

		public static void WriteXmlFile<T>(Stream stream, T content)
			=> WriteXmlFile(stream, typeof(T), content);

		public static void WriteXmlFile<T>(string filePath, T content, XmlWriterSettings settings)
			=> WriteXmlFile(filePath, typeof(T), content, settings);

		public static void WriteXmlFile<T>(Stream stream, T content, XmlWriterSettings settings)
			=> WriteXmlFile(stream, typeof(T), content, settings);

		#endregion Generic Write
	}
}
