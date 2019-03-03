using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace ScriptEditor
{
	public class Configuration
	{
		public string LastProjectName { get; set; }
		public string CachedNGCenterPath { get; set; }

		public string FontFamily { get; set; } = "Consolas";
		public float FontSize { get; set; } = 12F;

		// General options

		public bool Autocomplete { get; set; } = true;
		public bool AutoCloseBrackets { get; set; } = true;
		public bool ShowSpaces { get; set; } = false;
		public bool WordWrap { get; set; } = false;

		// Tidy options

		public bool Tidy_PreEqualSpace { get; set; } = false;
		public bool Tidy_PostEqualSpace { get; set; } = true;
		public bool Tidy_PreCommaSpace { get; set; } = false;
		public bool Tidy_PostCommaSpace { get; set; } = true;
		public bool Tidy_ReduceSpaces { get; set; } = false;
		public bool Tidy_ReindentOnSave { get; set; } = false;

		// View options

		public bool View_ShowToolStrip { get; set; } = true;
		public bool View_ShowObjBrowser { get; set; } = true;
		public bool View_ShowProjExplorer { get; set; } = true;
		public bool View_ShowInfoBox { get; set; } = true;
		public bool View_ShowStatusStrip { get; set; } = true;
		public bool View_ShowLineNumbers { get; set; } = true;
		public bool View_ShowToolTips { get; set; } = true;

		// Script syntax colors

		public string ScriptColors_Comment { get; set; } = ColorTranslator.ToHtml(Color.Green);
		public string ScriptColors_Section { get; set; } = ColorTranslator.ToHtml(Color.SteelBlue);
		public string ScriptColors_NewCommand { get; set; } = ColorTranslator.ToHtml(Color.SpringGreen);
		public string ScriptColors_OldCommand { get; set; } = ColorTranslator.ToHtml(Color.MediumAquamarine);
		public string ScriptColors_UnknownCommand { get; set; } = ColorTranslator.ToHtml(Color.Red);
		public string ScriptColors_Value { get; set; } = ColorTranslator.ToHtml(Color.LightSalmon);
		public string ScriptColors_Reference { get; set; } = ColorTranslator.ToHtml(Color.Orchid);

		public static string GetDefaultPath()
		{
			return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ScriptEditorConfiguration.xml");
		}

		public void Save(Stream stream)
		{
			try
			{
				new XmlSerializer(typeof(Configuration)).Serialize(stream, this);
			}
			catch (IOException)
			{
				using (XmlWriter writer = XmlWriter.Create(stream))
					new XmlSerializer(typeof(Configuration)).Serialize(writer, new Configuration());

				Save(stream);
			}
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
				using (XmlWriter writer = XmlWriter.Create(path))
					new XmlSerializer(typeof(Configuration)).Serialize(writer, new Configuration());

				Save(path);
			}
		}

		public void Save()
		{
			Save(GetDefaultPath());
		}

		public static Configuration Load(Stream stream)
		{
			try
			{
				using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = false }))
					return (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(reader);
			}
			catch (IOException)
			{
				using (XmlWriter writer = XmlWriter.Create(stream))
					new XmlSerializer(typeof(Configuration)).Serialize(writer, new Configuration());

				return Load(stream);
			}
		}

		public static Configuration Load(string filePath)
		{
			try
			{
				using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					return Load(stream);
			}
			catch (IOException)
			{
				using (XmlWriter writer = XmlWriter.Create(filePath))
					new XmlSerializer(typeof(Configuration)).Serialize(writer, new Configuration());

				return Load(filePath);
			}
		}

		public static Configuration Load()
		{
			return Load(GetDefaultPath());
		}
	}
}
