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

    public partial class Wad2 : IDisposable
    {
        public Dictionary<Hash, WadTexture> Textures { get; private set; }
        public Dictionary<Hash, WadMesh> Meshes { get; private set; }
        public SortedDictionary<uint, WadMoveable> Moveables { get; private set; }
        public SortedDictionary<uint, WadStatic> Statics { get; private set; }
        public SortedDictionary<ushort, WadSoundInfo> SoundInfo { get; private set; }
        public List<WadSpriteSequence> SpriteSequences { get; private set; }
        public Dictionary<Hash, WadTexture> SpriteTextures { get; private set; }
        public string FileName { get; set; }

        // Data for rendering
        public GraphicsDevice GraphicsDevice { get; set; }
        public Texture2D DirectXTexture { get; private set; }
        public SortedDictionary<uint, SkinnedModel> DirectXMoveables { get; } = new SortedDictionary<uint, SkinnedModel>();
        public SortedDictionary<uint, StaticModel> DirectXStatics { get; } = new SortedDictionary<uint, StaticModel>();

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
            Textures = new Dictionary<Hash, WadTexture>();
            Meshes = new Dictionary<Hash, WadMesh>();
            Moveables = new SortedDictionary<uint, WadMoveable>();
            Statics = new SortedDictionary<uint, WadStatic>();
            SoundInfo = new SortedDictionary<ushort, WadSoundInfo>();
            SpriteSequences = new List<WadSpriteSequence>();
            SpriteTextures = new Dictionary<Hash, WadTexture>();
        }

        public Wad2(GraphicsDevice device) : this()
        {
            GraphicsDevice = device;
        }

        public short[] FixedSounds
        {
            get
            {
                return new short[] {0, 2, 6, 7, 8, 9, 10, 17, 19, 27, 30, 31, 33, 35, 36, 37, 49, 60,
                                    68, 79, 105, 106, 107, 108, 109,
                                    110, 111, 113, 114, 115, 116, 118, 121, 148, 149, 150, 163, 182, 183, 185, 186,
                                    199, 235, 270, 288, 290, 291, 292, 293, 293, 325, 339, 340, 344, 347, 351, 368};
            }
        }

        public List<WadTexture> RebuildTextureAtlas()
        {
            if (DirectXTexture != null) DirectXTexture.Dispose();

            // Pack the textures in a single atlas
            List<WadTexture> packedTextures = new List<WadTexture>();

            for (int i = 0; i < Textures.Count; i++)
            {
                packedTextures.Add(Textures.ElementAt(i).Value);
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

            return packedTextures;
        }

        public void PrepareDataForDirectX()
        {
            Dispose();

            // Rebuild the texture atlas and covert it to a DirectX texture
            var packedTextures = RebuildTextureAtlas();
            
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
