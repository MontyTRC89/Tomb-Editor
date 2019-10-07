﻿using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;

namespace WadTool
{
    public enum VariableType
    {
        EditorDirectory
    }

    // Just add properties to this class to add now configuration options.
    // They will be loaded and saved automatically.
    public class Configuration
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

        public bool Tool_MakeEmptyWadAtStartup { get; set; } = false;

        public int AnimationEditor_UndoDepth { get; set; } = 30;
        public bool AnimationEditor_RewindAfterChainPlayback { get; set; } = true;

        public float RenderingItem_NavigationSpeedMouseWheelZoom { get; set; } = 6.0f;
        public float RenderingItem_NavigationSpeedMouseZoom { get; set; } = 800.0f;
        public float RenderingItem_NavigationSpeedMouseTranslate { get; set; } = 1500.0f;
        public float RenderingItem_NavigationSpeedMouseRotate { get; set; } = 4.0f;
        public float RenderingItem_FieldOfView { get; set; } = 50.0f;
        public bool RenderingItem_ShowDebugInfo { get; set; } = false;
        public bool RenderingItem_Antialias { get; set; } = false;
        public Vector4 RenderingItem_BackgroundColor { get; set; } = new Vector4(0.65f, 0.65f, 0.65f, 1.0f);

        public float GizmoStatic_Size { get; set; } = 1536.0f;
        public float GizmoStatic_TranslationConeSize { get; set; } = 220.0f;
        public float GizmoStatic_CenterCubeSize { get; set; } = 128.0f;
        public float GizmoStatic_ScaleCubeSize { get; set; } = 128.0f;
        public float GizmoStatic_LineThickness { get; set; } = 45.0f;

        public float GizmoSkeleton_Size { get; set; } = 512.0f;
        public float GizmoSkeleton_TranslationConeSize { get; set; } = 110.0f;
        public float GizmoSkeleton_CenterCubeSize { get; set; } = 64.0f;
        public float GizmoSkeleton_ScaleCubeSize { get; set; } = 64.0f;
        public float GizmoSkeleton_LineThickness { get; set; } = 16.0f;

        public float GizmoAnimationEditor_Size { get; set; } = 128.0f;
        public float GizmoAnimationEditor_TranslationConeSize { get; set; } = 48.0f;
        public float GizmoAnimationEditor_CenterCubeSize { get; set; } = 24.0f;
        public float GizmoAnimationEditor_ScaleCubeSize { get; set; } = 24.0f;
        public float GizmoAnimationEditor_LineThickness { get; set; } = 6.0f;

        public bool StartUpHelp_Show { get; set; } = false;

        public const string VariableBegin = "$(";
        public const string VariableEnd = ")";
        public static readonly char Dir = Path.DirectorySeparatorChar;

        public string GetVariable(VariableType variableType)
        {
            switch (variableType)
            {
                case VariableType.EditorDirectory:
                    return Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                default:
                    throw new ArgumentException();
            }
        }

        public string ParseVariables(string path, params VariableType[] excluded)
        {
            int startIndex = 0;
            do
            {
                // Find variable
                startIndex = path.IndexOf(VariableBegin, startIndex);
                if (startIndex == -1)
                    break;
                int afterStartIndex = startIndex + VariableBegin.Length;
                int endIndex = path.IndexOf(VariableEnd, afterStartIndex);
                if (endIndex == -1)
                    break;
                string variableName = path.Substring(afterStartIndex, endIndex - afterStartIndex);

                // Parse variable
                VariableType variableType;
                if (!Enum.TryParse(variableName, out variableType) ||
                    excluded.Contains(variableType))
                {
                    startIndex = endIndex + VariableEnd.Length;
                    continue;
                }
                string variableContent = GetVariable(variableType);
                path = path.Remove(startIndex, endIndex + VariableEnd.Length - startIndex);
                path = path.Insert(startIndex, variableContent);
                startIndex += variableContent.Length;
            } while (true);

            return path;
        }

        public static string GetDefaultPath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WadToolConfiguration.xml");
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
            if (!string.IsNullOrEmpty(GetDefaultPath()))
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
            return (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(stream);
        }

        public static Configuration Load(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Load(stream);
        }

        public static Configuration Load()
        {
            return Load(GetDefaultPath());
        }

        public static Configuration LoadOrUseDefault(ICollection<LogEventInfo> log = null)
        {
            string path = GetDefaultPath();
            if (!File.Exists(path))
            {
                log?.Add(new LogEventInfo(LogLevel.Info, logger.Name, null, "Unable to load configuration from \"" + path + "\"", null, new FileNotFoundException("File not found", path)));
                return new Configuration();
            }

            try
            {
                return Load();
            }
            catch (Exception exc)
            {
                log?.Add(new LogEventInfo(LogLevel.Info, logger.Name, null, "Unable to load configuration from \"" + path + "\"", null, exc));
                return new Configuration();
            }
        }
    }
}
