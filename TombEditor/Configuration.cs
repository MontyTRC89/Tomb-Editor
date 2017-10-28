using NLog;
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

namespace TombEditor
{
    // Just add properties to this class to add now configuration options.
    // They will be loaded and saved automatically.
    public class Configuration
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [XmlIgnore]
        public string FilePath { get; set; } = null;

        [XmlIgnore]
        public LogLevel Log_MinLevel { get; set; } = LogLevel.Debug;
        [XmlElement(nameof(Log_MinLevel))]
        public string Log_MinLevelSerialized
        {
            get { return Log_MinLevel.Name; }
            set { Log_MinLevel = LogLevel.FromString(value); }
        }
        public bool Log_WriteToFile { get; set; } = true;
        public int Log_ArchiveN { get; set; } = 0;

        public float RenderingItem_NavigationSpeedMouseWheelZoom { get; set; } = 6.0f;
        public float RenderingItem_NavigationSpeedMouseZoom { get; set; } = 300.0f;
        public float RenderingItem_NavigationSpeedMouseTranslate { get; set; } = 200.0f;
        public float RenderingItem_NavigationSpeedMouseRotate { get; set; } = 4.0f;
        public float RenderingItem_FieldOfView { get; set; } = 50.0f;

        public int Rendering3D_DrawRoomsMaxDepth { get; set; } = 6;
        public float Rendering3D_NavigationSpeedKeyRotate { get; set; } = 0.17f;
        public float Rendering3D_NavigationSpeedKeyZoom { get; set; } = 3000.0f;
        public float Rendering3D_NavigationSpeedMouseWheelZoom { get; set; } = 30.0f;
        public float Rendering3D_NavigationSpeedMouseZoom { get; set; } = 72000.0f;
        public float Rendering3D_NavigationSpeedMouseTranslate { get; set; } = 22000.0f;
        public float Rendering3D_NavigationSpeedMouseRotate { get; set; } = 3.0f;
        public bool Rendering3D_InvertMouseZoom { get; set; } = false;
        public float Rendering3D_LineWidth { get; set; } = 10.0f;
        public float Rendering3D_FieldOfView { get; set; } = 50.0f;
        public Vector4 Rendering3D_BackgroundColor { get; set; } = new Vector4(0.65f, 0.65f, 0.65f, 1.0f);
        public Vector4 Rendering3D_BackgroundColorFlipRoom { get; set; } = new Vector4(0.13f, 0.13f, 0.13f, 1.0f);
        public Vector4 Rendering3D_TextColor { get; set; } = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        public float Map2D_NavigationMinZoom { get; set; } = 0.04f;
        public float Map2D_NavigationMaxZoom { get; set; } = 500.0f;
        public float Map2D_NavigationSpeedMouseWheelZoom { get; set; } = 0.001f;
        public float Map2D_NavigationSpeedMouseZoom { get; set; } = 7.5f;
        public float Map2D_NavigationSpeedKeyZoom { get; set; } = 0.17f;
        public float Map2D_NavigationSpeedKeyMove { get; set; } = 107.0f;

        public float TextureMap_NavigationMinZoom { get; set; } = 0.02f;
        public float TextureMap_NavigationMaxZoom { get; set; } = 2000.0f;
        public float TextureMap_NavigationSpeedMouseWheelZoom { get; set; } = 0.0015f;
        public float TextureMap_NavigationSpeedMouseZoom { get; set; } = 7.5f;
        public float TextureMap_NavigationSpeedKeyZoom { get; set; } = 0.17f;
        public float TextureMap_NavigationSpeedKeyMove { get; set; } = 107.0f;
        public float TextureMap_TextureAreaToViewRelativeSize { get; set; } = 0.32f;
        public float TextureMap_DefaultTileSelectionSize { get; set; } = 64.0f;
        public bool TextureMap_UseAdvancedTexturingByDefault { get; set; } = false;

        public float Gizmo_Size { get; set; } = 1536.0f;
        public float Gizmo_TranslationConeSize { get; set; } = 220.0f;
        public float Gizmo_CenterCubeSize { get; set; } = 128.0f;
        public float Gizmo_ScaleCubeSize { get; set; } = 128.0f;
        public float Gizmo_LineThickness { get; set; } = 45.0f;

        public Point Window_Position { get; set; } = new Point(32, 32);
        public Size Window_Size { get; set; } = Window_SizeDefault;
        public bool Window_Maximized { get; set; } = true;
        public DockPanelState Window_Layout { get; set; } = Window_LayoutDefault;

        public static readonly Size Window_SizeDefault = new Size(1212, 763);
        public static readonly DockPanelState Window_LayoutDefault = new DockPanelState
        {
            Regions = new List<DockRegionState>
            {
                new DockRegionState
                {
                    Area = DarkDockArea.Document,
                    Size = new Size(0, 0),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "MainView" },
                            VisibleContent = "MainView",
                            Order = 0,
                            Size = new Size(0 ,0)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Left,
                    Size = new Size(286, 893),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "SectorOptions" },
                            VisibleContent = "SectorOptions",
                            Order = 0,
                            Size = new Size(284,280)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "RoomOptions" },
                            VisibleContent = "RoomOptions",
                            Order = 1,
                            Size = new Size(284,192)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "ObjectBrowser" },
                            VisibleContent = "ObjectBrowser",
                            Order = 2,
                            Size = new Size(284,259)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "TriggerList", "ObjectList" },
                            VisibleContent = "TriggerList",
                            Order = 3,
                            Size = new Size(284,174)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Right,
                    Size = new Size(286, 0),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "TexturePanel" },
                            VisibleContent = "TexturePanel",
                            Order = 0,
                            Size = new Size(284,700)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Bottom,
                    Size = new Size(1007, 128),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "Lighting" },
                            VisibleContent = "Lighting",
                            Order = 0,
                            Size = new Size(432,128)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "Palette" },
                            VisibleContent = "Palette",
                            Order = 1,
                            Size = new Size(645,128)
                        }
                    }
                }
            }
        };

        public static string GetDefaultPath()
        {
            return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "TombEditorConfiguration.xml");
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
                catch (Exception exc)
                {
                    logger.Info(exc, "Unable to save configuration to \"" + GetDefaultPath() + "\"");
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

        public static Configuration LoadOrUseDefault(ICollection<LogEventInfo> log = null)
        {
            try
            {
                return Load();
            }
            catch (Exception exc)
            {
                log?.Add(new LogEventInfo(LogLevel.Info, logger.Name, null, "Unable to load configuration from \"" + GetDefaultPath() + "\"", null, exc));
                return new Configuration { FilePath = GetDefaultPath() };
            }
        }
    }
}
