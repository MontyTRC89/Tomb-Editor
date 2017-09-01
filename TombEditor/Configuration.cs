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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public float RenderingItem_NavigationSpeedMouseWheelZoom { get; set; } = 6.0f;
        public float RenderingItem_NavigationSpeedMouseZoom { get; set; } = 200.0f;
        public float RenderingItem_NavigationSpeedMouseTranslate { get; set; } = 200.0f;
        public float RenderingItem_NavigationSpeedMouseRotate { get; set; } = 4.0f;

        public int Rendering3D_DrawRoomsMaxDepth { get; set; } = 6;
        public float Rendering3D_NavigationSpeedKeyRotate { get; set; } = 0.17f;
        public float Rendering3D_NavigationSpeedKeyZoom { get; set; } = 3000.0f;
        public float Rendering3D_NavigationSpeedMouseWheelZoom { get; set; } = 25.0f;
        public float Rendering3D_NavigationSpeedMouseZoom { get; set; } = 46000.0f;
        public float Rendering3D_NavigationSpeedMouseTranslate { get; set; } = 22000.0f;
        public float Rendering3D_NavigationSpeedMouseRotate { get; set; } = 2.2f;
        public float Rendering3D_LineWidth { get; set; } = 10.0f;

        public float Map2D_NavigationSpeedMouseWheelZoom { get; set; } = 0.001f;
        public float Map2D_NavigationSpeedMouseZoom { get; set; } = 5.0f;
        public float Map2D_NavigationSpeedKeyZoom { get; set; } = 0.17f;
        public float Map2D_NavigationSpeedKeyMove { get; set; } = 107.0f;

        public float TextureMap_NavigationSpeedMouseWheelZoom { get; set; } = 0.0015f;
        public float TextureMap_NavigationSpeedMouseZoom { get; set; } = 5.0f;
        public float TextureMap_TextureAreaToViewRelativeSize { get; set; } = 0.32f;


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
                logger.Info(exc, "Unable to save configuration to \"" + GetDefaultPath() + "\"");
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
                logger.Info(exc, "Unable to load configuration from \"" + GetDefaultPath() + "\"");
                return new Configuration();
            }
        }
    }
}
