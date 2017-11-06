using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO
{
    public static class IOSettingsPresets
    {
        public static List<IOGeometrySettingsPreset> SettingsPresets { get; private set; }

        static IOSettingsPresets()
        {
            SettingsPresets = new List<IOGeometrySettingsPreset>();

            // Blender
            SettingsPresets.Add(new IOGeometrySettingsPreset
            {
                Name = "Blender OBJ",
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
                    Scale = 1.0f,
                    FlipZ = true,
                    FlipUV_V = false,
                    PremultiplyUV = true,
                    WrapUV = true
                }
            });
        }
    }
}
