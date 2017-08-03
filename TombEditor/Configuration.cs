using NLog;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace TombEditor
{
    // Just add properties to this class to add now configuration options.
    // They will be loaded and saved automatically.
    public class Configuration
    {
        public int DrawRoomsMaxDepth { get; set; } = 6;


        
        public static string GetDefaultPath()
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + "/TombEditorConfiguration.xml";
        }

        public void Save(Stream stream)
        {
            new XmlSerializer(typeof(Configuration)).Serialize(stream, this);
        }

        public void Save(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                Save(stream);
        }

        public void Save()
        {
            Save(GetDefaultPath());
        }

        public void SaveTry()
        {
            try
            {
                Save();
            }
            catch (Exception exc)
            {
                LogManager.GetCurrentClassLogger().Log(LogLevel.Info, exc,
                    "Unable to save configuration to \"" + GetDefaultPath() + "\"");
            }
        }

        public static Configuration Load(Stream stream)
        {
            return (Configuration)(new XmlSerializer(typeof(Configuration)).Deserialize(stream));
        }

        public static Configuration Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Load(stream);
        }

        public static Configuration Load()
        {
            return Load(GetDefaultPath());
        }
        
        public static Configuration LoadOrUseDefault()
        {
            try
            {
                return Load();
            }
            catch (Exception exc)
            {
                LogManager.GetCurrentClassLogger().Log(LogLevel.Info, exc, 
                    "Unable to load configuration from \"" + GetDefaultPath() + "\"");
                return new Configuration();
            }
        }
    }
}
