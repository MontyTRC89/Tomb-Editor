using System.Collections.Generic;

namespace TombLib.GeometryIO
{
    public static class IOSettingsPresets
    {
        public static List<IOGeometrySettingsPreset> SettingsPresets { get; private set; }

        static IOSettingsPresets()
        {
            SettingsPresets = new List<IOGeometrySettingsPreset>();

            // Generic
            SettingsPresets.Add(new IOGeometrySettingsPreset
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
            SettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Blender OBJ",
                Settings = new IOGeometrySettings
                {
                    Scale = 1024.0f,
                    FlipZ = true,
                    FlipUV_V = true,
                    InvertFaces = true,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });

            SettingsPresets.Add(new IOGeometrySettingsPreset
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

            SettingsPresets.Add(new IOGeometrySettingsPreset
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

            // Metasequoia
            SettingsPresets.Add(new IOGeometrySettingsPreset
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

            SettingsPresets.Add(new IOGeometrySettingsPreset
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

            SettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Metasequoia MQO",
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

            // 3ds Max
            SettingsPresets.Add(new IOGeometrySettingsPreset
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

            SettingsPresets.Add(new IOGeometrySettingsPreset
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
        }
    }
}
