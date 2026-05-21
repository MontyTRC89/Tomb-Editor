using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TombLib.Rendering;

namespace TombLib.Graphics
{
    public class DeviceManager
    {
        // Lazy-initialized so the static DX11 device + DXGI factory aren't
        // created unless someone actually touches the legacy renderer. The
        // V2 path (Vulkan / V2-DX11) can avoid this entirely. On Intel UHD
        // this is critical: the Intel Vulkan WSI rejects swapchain creation
        // (ErrorNativeWindowInUseKhr) in any process that has previously
        // initialised a DXGI adapter — eager static init guarantees that
        // condition.
        private static DeviceManager? _default;
        private static readonly object _defaultLock = new object();
        public static DeviceManager DefaultDeviceManager
        {
            get
            {
                if (_default != null) return _default;
                lock (_defaultLock) { return _default ??= new DeviceManager(); }
            }
        }
        public static bool IsDefaultInitialized => _default != null;

        public RenderingDevice Device;
        public GraphicsDevice ___LegacyDevice { get; set; }
        public Dictionary<string, Effect> ___LegacyEffects { get; } = new Dictionary<string, Effect>();
        public SpriteFont ___LegacyFont { get; set; }

        public DeviceManager()
        {
            Device = new Rendering.DirectX11.Dx11RenderingDevice();

            // Recreate legacy environment
            {
                ___LegacyDevice = GraphicsDevice.New(((Rendering.DirectX11.Dx11RenderingDevice)Device).Device);
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
