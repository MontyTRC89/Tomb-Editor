using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TombLib.Scripting.Helpers;

namespace TombLib.Scripting.Configuration
{
	public class LuaConfiguration
	{
		public double FontSize { get; set; } = 16d;
		public string FontFamily { get; set; } = "Consolas";

		public int UndoStackSize { get; set; } = 256;

		public bool AutocompleteEnabled { get; set; } = true;
		public bool LiveErrorUnderlining { get; set; } = true;
		public bool AutoCloseParentheses { get; set; } = true;
		public bool AutoCloseBraces { get; set; } = true;
		public bool AutoCloseBrackets { get; set; } = true;
		public bool AutoCloseQuotes { get; set; } = true;
		public bool WordWrapping { get; set; } = false;

		public bool ShowLineNumbers { get; set; } = true;
		public bool ShowVisualSpaces { get; set; } = false;
		public bool ShowVisualTabs { get; set; } = true;
		public bool ShowToolTips { get; set; } = true;

		public static string GetDefaultPath()
		{
			return Path.Combine(PathHelper.GetTextEditorConfigsPath(), "LuaConfiguration.xml");
		}

		public void Save(Stream stream)
		{
			new XmlSerializer(typeof(LuaConfiguration)).Serialize(stream, this);
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
				if (!Directory.Exists(PathHelper.GetTextEditorConfigsPath()))
					Directory.CreateDirectory(PathHelper.GetTextEditorConfigsPath());

				using (XmlWriter writer = XmlWriter.Create(path))
					new XmlSerializer(typeof(LuaConfiguration)).Serialize(writer, new LuaConfiguration());

				Save(path);
			}
		}

		public void Save()
		{
			Save(GetDefaultPath());
		}

		public static LuaConfiguration Load(Stream stream)
		{
			using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = false }))
				return (LuaConfiguration)new XmlSerializer(typeof(LuaConfiguration)).Deserialize(reader);
		}

		public static LuaConfiguration Load(string filePath)
		{
			try
			{
				using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					return Load(stream);
			}
			catch (IOException)
			{
				if (!Directory.Exists(PathHelper.GetTextEditorConfigsPath()))
					Directory.CreateDirectory(PathHelper.GetTextEditorConfigsPath());

				using (XmlWriter writer = XmlWriter.Create(filePath))
					new XmlSerializer(typeof(LuaConfiguration)).Serialize(writer, new LuaConfiguration());

				return Load(filePath);
			}
		}

		public static LuaConfiguration Load()
		{
			return Load(GetDefaultPath());
		}
	}
}
