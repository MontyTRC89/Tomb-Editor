using System.Collections.Generic;

namespace TombLib.GeometryIO
{
    public static class IOSettingsPresets
    {
        public static List<IOGeometrySettingsPreset> GeometryImportSettingsPresets { get; private set; }
        public static List<IOGeometrySettingsPreset> GeometryExportSettingsPresets { get; private set; }
        public static List<IOGeometrySettingsPreset> AnimationSettingsPresets { get; private set; }

        static IOSettingsPresets()
        {
            GeometryExportSettingsPresets = new List<IOGeometrySettingsPreset>();

            // Metasequoia
            GeometryExportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Blender OBJ",
                Settings = new IOGeometrySettings
                {
                    Export = true,
                    Scale = 128.0f,
                    FlipUV_V = true
                }
            });

            GeometryExportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Metasequoia MQO scale 1",
                Settings = new IOGeometrySettings
                {
                    Export = true,
                    Scale = 1024.0f,
                    FlipUV_V = true
                }
            });

            GeometryExportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Metasequoia MQO scale 1024",
                Settings = new IOGeometrySettings
                {
                    Export = true,
                    Scale = 1.0f,
                    FlipUV_V = true
                }
            });

            GeometryImportSettingsPresets = new List<IOGeometrySettingsPreset>();

            // Metasequoia
            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Metasequoia MQO Scale 1",
                Settings = new IOGeometrySettings
                {
                    Scale = 1.0f,
                    FlipZ = true,
                    InvertFaces = false,
                    FlipUV_V = false,
                    PremultiplyUV = true,
                    WrapUV = true,
                    UseVertexColor = true
                }
            });

            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Metasequoia MQO Scale 1024",
                Settings = new IOGeometrySettings
                {
                    Scale = 1024.0f,
                    FlipZ = true,
                    InvertFaces = false,
                    FlipUV_V = false,
                    PremultiplyUV = true,
                    WrapUV = true,
                    UseVertexColor = true
                }
            });

            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Metasequoia MQO Scale 1/1024",
                Settings = new IOGeometrySettings
                {
                    Scale = 1.0f / 1024.0f,
                    FlipZ = true,
                    InvertFaces = false,
                    FlipUV_V = false,
                    PremultiplyUV = true,
                    WrapUV = true,
                    UseVertexColor = true
                }
            });

            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Metasequoia OBJ",
                Settings = new IOGeometrySettings
                {
                    Scale = 1024.0f,
                    FlipZ = true,
                    InvertFaces = true,
                    FlipUV_V = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Metasequoia PLY",
                Settings = new IOGeometrySettings
                {
                    Scale = 1024.0f,
                    SwapYZ = true,
                    FlipZ = false,
                    FlipUV_V = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            // Generic
            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Generic OBJ",
                Settings = new IOGeometrySettings
                {
                    Scale = 1.0f,
                    FlipZ = true,
                    FlipUV_V = true,
                    InvertFaces = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            // Blender
            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Blender OBJ",
                Settings = new IOGeometrySettings
                {
                    Scale = 128.0f,
                    FlipZ = true,
                    FlipUV_V = true,
                    InvertFaces = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Blender DAE",
                Settings = new IOGeometrySettings
                {
                    Scale = 1024.0f,
                    FlipZ = true,
                    FlipUV_V = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Blender PLY",
                Settings = new IOGeometrySettings
                {
                    Scale = 1024.0f,
                    SwapYZ = true,
                    FlipZ = false,
                    FlipUV_V = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            // 3ds Max
            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "3ds Max FBX",
                Settings = new IOGeometrySettings
                {
                    Scale = 1.0f,
                    FlipZ = true,
                    FlipUV_V = true,
                    InvertFaces = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "3ds Max OBJ",
                Settings = new IOGeometrySettings
                {
                    Scale = 1.0f,
                    FlipZ = true, 
                    FlipUV_V = true,
                    InvertFaces = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            GeometryImportSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "TRViewer 3DS",
                Settings = new IOGeometrySettings
                {
                    Scale = 1.0f,
                    FlipZ = false,
                    SwapYZ = true,
                    FlipUV_V = true,
                    InvertFaces = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            AnimationSettingsPresets = new List<IOGeometrySettingsPreset>();

            // 3dsmax COLLADA
            AnimationSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "3dsmax COLLADA",
                Settings = new IOGeometrySettings
                {
                    ProcessAnimations = true,
                    ProcessGeometry = false,
                    SwapAnimTranslationYZ = true
                }
            });

            // 3dsmax Filmbox (FBX)
            AnimationSettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "3dsmax Filmbox (FBX)",
                Settings = new IOGeometrySettings
                {
                    ProcessAnimations = true,
                    ProcessGeometry = false,
                    SwapAnimTranslationXZ = true
                }
            });
        }
    }
}
