using SharpDX.Direct3D;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    internal class WadToolClass : IDisposable
    {
        public Wad2 DestinationWad { get; set; }
        public Wad2 SourceWad { get; set; }

        public GraphicsDevice Device { get; set; }
        public Dictionary<string, Texture2D> Textures { get; } = new Dictionary<string, Texture2D>();
        public Dictionary<string, Effect> Effects { get; } = new Dictionary<string, Effect>();
        public SpriteFont Font { get; set; }

        public Configuration Configuration { get { return _configuration; } }

        private Configuration _configuration;
        private static WadToolClass _instance;

        public static WadToolClass Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WadToolClass();
                    return _instance;
                }
                else
                {
                    return _instance;
                }
            }
        }

        public void Initialize()
        {
            Device = GraphicsDevice.New(DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.None, FeatureLevel.Level_10_0);

            string resourcePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            // Load effects
            IEnumerable<string> effectFiles = Directory.EnumerateFiles(resourcePath + "\\Editor", "*.fx");
            foreach (string fileName in effectFiles)
            {
                string effectName = Path.GetFileNameWithoutExtension(fileName);
                Effects.Add(effectName, LoadEffect(fileName));
            }

            // Load BasicEffect
            BasicEffect bEffect = new BasicEffect(Device);
            Effects.Add("Toolkit.BasicEffect", bEffect);

            // Load images
            IEnumerable<string> textureFiles = Directory.EnumerateFiles(resourcePath + "\\Editor", "*.png");
            foreach (string fileName in textureFiles)
            {
                string textureName = Path.GetFileNameWithoutExtension(fileName);
                Textures.Add(textureName, TombLib.Graphics.TextureLoad.Load(Device, fileName));
            }

            // Load default font
            SpriteFontData fontData = SpriteFontData.Load("Editor\\Font.bin");
            fontData.DefaultCharacter = '\n'; // Don't crash on uncommon Unicode values
            Font = SpriteFont.New(Device, fontData);

            // Load configuration
            _configuration = Configuration.LoadOrUseDefault();

            // Load items catalog
            TrCatalog.LoadCatalog("Editor\\TRCatalog.xml");
        }

        private Effect LoadEffect(string fileName)
        {
            EffectCompilerResult result = EffectCompiler.CompileFromFile(fileName);

            if (result.HasErrors)
            {
                string errors = "";
                foreach (var err in result.Logger.Messages)
                    errors += err + Environment.NewLine;
                throw new Exception("Could not compile effect '" + fileName + "'" + Environment.NewLine + errors);
            }

            return new Effect(Device, result.EffectData);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
