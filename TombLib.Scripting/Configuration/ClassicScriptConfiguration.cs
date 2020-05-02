using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TombLib.Scripting.Helpers;

namespace TombLib.Scripting.Configuration
{
	public class ClassicScriptConfiguration
	{
		public double FontSize { get; set; } = 16d;
		public string FontFamily { get; set; } = "Consolas";

		public int UndoStackSize { get; set; } = 256;

		public bool AutocompleteEnabled { get; set; } = true;
		public bool LiveErrorUnderlining { get; set; } = true;
		public bool AutoCloseBrackets { get; set; } = true;
		public bool AutoCloseQuotes { get; set; } = true;
		public bool WordWrapping { get; set; } = false;

		public bool ShowLineNumbers { get; set; } = true;
		public bool ShowSectionSeparators { get; set; } = true;
		public bool ShowVisualSpaces { get; set; } = false;
		public bool ShowVisualTabs { get; set; } = true;
		public bool ShowDefinitionToolTips { get; set; } = true;

		public bool Tidy_PreEqualSpace { get; set; } = false;
		public bool Tidy_PostEqualSpace { get; set; } = true;
		public bool Tidy_PreCommaSpace { get; set; } = false;
		public bool Tidy_PostCommaSpace { get; set; } = true;
		public bool Tidy_ReduceSpaces { get; set; } = true;

		public string Colors_Sections { get; set; } = ColorTranslator.ToHtml(Color.SteelBlue);
		public string Colors_Values { get; set; } = ColorTranslator.ToHtml(Color.LightSalmon);
		public string Colors_References { get; set; } = ColorTranslator.ToHtml(Color.Orchid);
		public string Colors_StandardCommands { get; set; } = ColorTranslator.ToHtml(Color.MediumAquamarine);
		public string Colors_NewCommands { get; set; } = ColorTranslator.ToHtml(Color.SpringGreen);
		public string Colors_Comments { get; set; } = ColorTranslator.ToHtml(Color.Green);
		public string Colors_UnknownCommands { get; set; } = ColorTranslator.ToHtml(Color.Red);

		public static string GetDefaultPath()
		{
			return Path.Combine(PathHelper.GetTextEditorConfigsPath(), "ClassicScriptConfiguration.xml");
		}

		public void Save(Stream stream)
		{
			new XmlSerializer(typeof(ClassicScriptConfiguration)).Serialize(stream, this);
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
					new XmlSerializer(typeof(ClassicScriptConfiguration)).Serialize(writer, new ClassicScriptConfiguration());

				Save(path);
			}
		}

		public void Save()
		{
			Save(GetDefaultPath());
		}

		public static ClassicScriptConfiguration Load(Stream stream)
		{
			using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = false }))
				return (ClassicScriptConfiguration)new XmlSerializer(typeof(ClassicScriptConfiguration)).Deserialize(reader);
		}

		public static ClassicScriptConfiguration Load(string filePath)
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
					new XmlSerializer(typeof(ClassicScriptConfiguration)).Serialize(writer, new ClassicScriptConfiguration());

				return Load(filePath);
			}
		}

		public static ClassicScriptConfiguration Load()
		{
			return Load(GetDefaultPath());
		}
	}
}
