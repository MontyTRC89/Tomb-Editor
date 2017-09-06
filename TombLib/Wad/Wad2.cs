using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using TombLib.Utils;
using TombLib.Graphics;
using SharpDX.Toolkit.Graphics;
using System.Drawing;

namespace TombLib.Wad
{
    public class ComparerWadTextures : IComparer<WadTexture>
    {
        public int Compare(WadTexture x, WadTexture y)
        {
            if (x == null || y == null)
                return 0;

            return -x.Height.CompareTo(y.Height);
        }
    }

    public class Wad2 : IDisposable
    {
        // Textures
        private Dictionary<Hash, WadTexture> _textures;

        // Meshes
        private Dictionary<Hash, WadMesh> _meshes;

        // Objects
        private Dictionary<uint, WadMoveable> _moveables;
        private Dictionary<uint, WadStatic> _staticMeshes;

        // Sounds
        private Dictionary<ushort, WadSoundInfo> _soundInfos;

        // Sprites
        private List<WadSpriteSequence> _spriteSequences;
        private Dictionary<Hash, WadTexture> _spriteTextures;

        // Data for rendering
        public GraphicsDevice GraphicsDevice { get; set; }
        public Texture2D DirectXTexture { get; private set; }
        public Dictionary<uint, SkinnedModel> DirectXMoveables { get; } = new Dictionary<uint, SkinnedModel>();
        public Dictionary<uint, StaticModel> DirectXStatics { get; } = new Dictionary<uint, StaticModel>();

        // Size of the atlas
        public const int TextureAtlasSize = 2048;

        // TODO: to remove
        public TR4Wad OriginalWad;

        public void Dispose()
        {
            DirectXTexture?.Dispose();
            DirectXTexture = null;

            foreach (var obj in DirectXMoveables.Values)
                obj.Dispose();
            DirectXMoveables.Clear();

            foreach (var obj in DirectXStatics.Values)
                obj.Dispose();
            DirectXStatics.Clear();
        }

        public Wad2()
        {
            _textures = new Dictionary<Hash, WadTexture>();
            _meshes = new Dictionary<Hash, WadMesh>();
            _moveables = new Dictionary<uint, WadMoveable>();
            _staticMeshes = new Dictionary<uint, WadStatic>();
            _soundInfos = new Dictionary<ushort, WadSoundInfo>();
            _spriteSequences = new List<WadSpriteSequence>();
            _spriteTextures = new Dictionary<Hash, WadTexture>();
        }

        public Wad2(GraphicsDevice device) : this()
        {
            GraphicsDevice = device;
        }

        public Dictionary<Hash, WadTexture> Textures { get { return _textures; } }
        public Dictionary<Hash, WadMesh> Meshes { get { return _meshes; } }
        public Dictionary<uint, WadMoveable> Moveables { get { return _moveables; } }
        public Dictionary<uint, WadStatic> Statics { get { return _staticMeshes; } }
        public Dictionary<ushort, WadSoundInfo> SoundInfo { get { return _soundInfos; } }
        public List<WadSpriteSequence> SpriteSequences { get { return _spriteSequences; } }
        public Dictionary<Hash, WadTexture> SpriteTextures { get { return _spriteTextures; } }

        public void PrepareDataForDirectX()
        {
            Dispose();

            // Pack the textures in a single atlas
            List<WadTexture> packedTextures = new List<WadTexture>();

            for (int i = 0; i < _textures.Count; i++)
            {
                packedTextures.Add(_textures.ElementAt(i).Value);
            }

            packedTextures.Sort(new ComparerWadTextures());

            RectPackerSimpleStack packer = new RectPackerSimpleStack(TextureAtlasSize, TextureAtlasSize);

            foreach (var texture in packedTextures)
            {
                var point = packer.TryAdd(texture.Width, texture.Height);
                texture.PositionInPackedTexture = new Vector2(point.Value.X, point.Value.Y);
            }

            // Copy the page in a temp bitmap. 
            // I generate a texture atlas, putting all texture pages inside 2048x2048 pixel textures.
            var tempBitmap = ImageC.CreateNew(TextureAtlasSize, TextureAtlasSize);

            foreach (var texture in packedTextures)
            {
                int startX = (int)texture.PositionInPackedTexture.X;
                int startY = (int)texture.PositionInPackedTexture.Y;

                for (int y = 0; y < texture.Height; y++)
                {
                    for (int x = 0; x < texture.Width; x++)
                    {
                        var color = texture.Image.GetPixel(x, y);
                        tempBitmap.SetPixel(startX + x, startY + y, color);
                    }
                }
            }

            // Create the DirectX texture atlas
            DirectXTexture = TextureLoad.Load(GraphicsDevice, tempBitmap);
            
            // Create movable models
            for (int i = 0; i < Moveables.Count; i++)
            {
                WadMoveable mov = Moveables.ElementAt(i).Value;
                DirectXMoveables.Add(mov.ObjectID, SkinnedModel.FromWad2(GraphicsDevice, this, mov, packedTextures));
            }

            // Create static meshes
            for (int i = 0; i < Statics.Count; i++)
            {
                WadStatic staticMesh = Statics.ElementAt(i).Value;
                DirectXStatics.Add(staticMesh.ObjectID, StaticModel.FromWad2(GraphicsDevice, this, staticMesh, packedTextures));
            }
        }

        // Lets remove this methode once we use UVs internally in the Wad representation.
        // Until then this converts the deprecated format to UVs that the new texture manager in the *.tr4 export understands.
        public TextureArea GetTextureArea(WadPolygon poly)
        {
            TextureArea result = new TextureArea();
            result.Texture = poly.Texture;
            result.BlendMode = poly.Transparent ? BlendMode.Additive : BlendMode.Normal;
            result.DoubleSided = false; // TODO isn't this flag also available in wads?

            result.TexCoord0 = poly.UV[0];
            result.TexCoord1 = poly.UV[1];
            result.TexCoord2 = poly.UV[2];
            if (poly.Shape == WadPolygonShape.Rectangle) result.TexCoord3 = poly.UV[3];

            return result;
        }
    }
}
