using SharpDX;
using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace WadTool
{
    // Just add properties to this class to add now configuration options.
    // They will be loaded and saved automatically.
    public class Configuration
    {
        [XmlIgnore]
        public string FilePath { get; set; } = null;

        public float RenderingItem_NavigationSpeedMouseWheelZoom { get; set; } = 6.0f;
        public float RenderingItem_NavigationSpeedMouseZoom { get; set; } = 300.0f;
        public float RenderingItem_NavigationSpeedMouseTranslate { get; set; } = 200.0f;
        public float RenderingItem_NavigationSpeedMouseRotate { get; set; } = 4.0f;
        public float RenderingItem_FieldOfView { get; set; } = 50.0f;
            
        public float Gizmo_Size { get; set; } = 1536.0f;
        public float Gizmo_TranslationConeSize { get; set; } = 220.0f;
        public float Gizmo_CenterCubeSize { get; set; } = 128.0f;
        public float Gizmo_ScaleCubeSize { get; set; } = 128.0f;
        public float Gizmo_LineThickness { get; set; } = 45.0f;

        public string Sounds_Tr2MainSfxPath { get; set; } = "Sounds\\TR2\\Main.sfx";
        public string Sounds_Tr3MainSfxPath { get; set; } = "Sounds\\TR3\\Main.sfx";

        public Vector4 Rendering3D_BackgroundColor { get; set; } = new Vector4(0.65f, 0.65f, 0.65f, 1.0f);

        public static string GetDefaultPath()
        {
            return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "WadToolConfiguration.xml");
        }

        public void Save(Stream stream)
        {
            new XmlSerializer(typeof(Configuration)).Serialize(stream, this);
        }

        public void Save(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                Save(stream);
            FilePath = path;
        }

        public void Save()
        {
            Save(FilePath);
        }

        public void SaveTry()
        {
            if (!string.IsNullOrEmpty(FilePath))
                try
                {
                    Save();
                }
                catch (Exception)
                {
                    //logger.Info(exc, "Unable to save configuration to \"" + GetDefaultPath() + "\"");
                }
        }

        public static Configuration Load(Stream stream)
        {
            return (Configuration)(new XmlSerializer(typeof(Configuration)).Deserialize(stream));
        }

        public static Configuration Load(string filePath)
        {
            Configuration result;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                result = Load(stream);
            result.FilePath = filePath;
            return result;
        }

        public static Configuration Load()
        {
            return Load(GetDefaultPath());
        }

        public static Configuration LoadOrUseDefault(/*ICollection<LogEventInfo> log = null*/)
        {
            try
            {
                return Load();
            }
            catch (Exception)
            {
                //log?.Add(new LogEventInfo(LogLevel.Info, logger.Name, null, "Unable to load configuration from \"" + GetDefaultPath() + "\"", null, exc));
                return new Configuration { FilePath = GetDefaultPath() };
            }
        }
    }
}
