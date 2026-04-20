using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TombLib.Rendering;

namespace TombLib.Graphics
{
    public class DeviceManager
    {
        // to be removed
        public static DeviceManager DefaultDeviceManager = new DeviceManager();

        //public RenderingDevice Device;
        public RenderingDevice Device;
        public GraphicsDevice ___LegacyDevice { get; set; }
        public Dictionary<string, Effect> ___LegacyEffects { get; } = new Dictionary<string, Effect>();
        public SpriteFont ___LegacyFont { get; set; }

        public DeviceManager()
        {
            Device = new Rendering.DirectX11.Dx11RenderingDevice();

            // Recreate legacy environment via COM pointer sharing.
            // The Vortice device and SharpDX Toolkit share the same underlying D3D11 device.
            {
                var vorticeDevice = ((Rendering.DirectX11.Dx11RenderingDevice)Device).Device;
                IntPtr nativePtr = vorticeDevice.NativePointer;
                Marshal.AddRef(nativePtr);
                var sharpDxDevice = new SharpDX.Direct3D11.Device(nativePtr);

                ___LegacyDevice = GraphicsDevice.New(sharpDxDevice);
                LevelData.ImportedGeometry.Device = ___LegacyDevice;

                // Load legacy effects
                string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location) + "\\Rendering\\Legacy";

                if (!Directory.Exists(dir))
                {
                    MessageBox.Show("Shader files are missing. Please reinstall Tomb Editor.", "Error", MessageBoxButtons.OK);
                    throw new FileNotFoundException();
                }

                IEnumerable<string> effectFiles = Directory.EnumerateFiles(dir, "*.fx");
                foreach (string fileName in effectFiles)
                {
                    string effectName = Path.GetFileNameWithoutExtension(fileName);
                    EffectCompilerResult effect = EffectCompiler.CompileFromFile(fileName);
                    if (effect.HasErrors)
                    {
                        string errors = "";
                        foreach (var err in effect.Logger.Messages)
                            errors += err + Environment.NewLine;
                        throw new Exception("Could not compile effect '" + fileName + "'" + Environment.NewLine + errors);
                    }
                    ___LegacyEffects.Add(effectName, new Effect(___LegacyDevice, effect.EffectData));
                }

                // Load legacy font
                SpriteFontData fontData = SpriteFontData.Load(ResourcesC.ResourcesC.font);
                fontData.DefaultCharacter = '\n'; // Don't crash on uncommon Unicode values
                ___LegacyFont = SpriteFont.New(___LegacyDevice, fontData);
            }
        }
    }
}
