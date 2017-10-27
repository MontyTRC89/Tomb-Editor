using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using TombLib.Utils;
using TombLib.Graphics;
using SharpDX.Toolkit.Graphics;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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
        public TombRaiderVersion Version { get; private set; }

        public Dictionary<Hash, WadTexture> Textures { get; private set; }
        public Dictionary<Hash, WadMesh> Meshes { get; private set; }
        public SortedDictionary<uint, WadMoveable> Moveables { get; private set; }
        public SortedDictionary<uint, WadStatic> Statics { get; private set; }
        public SortedDictionary<ushort, WadSoundInfo> SoundInfo { get; private set; }
        public Dictionary<Hash, WadSample> Samples { get; private set; }
        public List<WadSpriteSequence> SpriteSequences { get; private set; }
        public Dictionary<Hash, WadSprite> SpriteTextures { get; private set; }
        public string FileName { get; set; }
        public int SoundMapSize { get; set; }

        // Data for rendering
        public GraphicsDevice GraphicsDevice { get; set; }
        public Texture2D DirectXTexture { get; private set; }
        public SortedDictionary<uint, SkinnedModel> DirectXMoveables { get; } = new SortedDictionary<uint, SkinnedModel>();
        public SortedDictionary<uint, StaticModel> DirectXStatics { get; } = new SortedDictionary<uint, StaticModel>();
        public List<WadTexture> PackedTextures { get; set; } = new List<WadTexture>();

        // Size of the atlas
        public const int TextureAtlasSize = 2048;

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

        public Wad2(TombRaiderVersion version)
        {
            Version = version;
            Textures = new Dictionary<Hash, WadTexture>();
            Meshes = new Dictionary<Hash, WadMesh>();
            Moveables = new SortedDictionary<uint, WadMoveable>();
            Statics = new SortedDictionary<uint, WadStatic>();
            SoundInfo = new SortedDictionary<ushort, WadSoundInfo>();
            SpriteSequences = new List<WadSpriteSequence>();
            SpriteTextures = new Dictionary<Hash, WadSprite>();
            Samples = new Dictionary<Hash, WadSample>();
        }

        public void RebuildTextureAtlas()
        {
            if (DirectXTexture != null) DirectXTexture.Dispose();

            // Pack the textures in a single atlas
            PackedTextures = new List<WadTexture>();

            for (int i = 0; i < Textures.Count; i++)
            {
                PackedTextures.Add(Textures.ElementAt(i).Value);
            }

            PackedTextures.Sort(new ComparerWadTextures());

            var packer = new RectPackerSimpleStack(TextureAtlasSize, TextureAtlasSize);

            foreach (var texture in PackedTextures)
            {
                var point = packer.TryAdd(texture.Width, texture.Height);
                texture.PositionInPackedTexture = new Vector2(point.Value.X, point.Value.Y);
            }

            // Copy the page in a temp bitmap. 
            // I generate a texture atlas, putting all texture pages inside 2048x2048 pixel textures.
            var tempBitmap = ImageC.CreateNew(TextureAtlasSize, TextureAtlasSize);

            foreach (var texture in PackedTextures)
            {
                int startX = (int)texture.PositionInPackedTexture.X;
                int startY = (int)texture.PositionInPackedTexture.Y;
                tempBitmap.CopyFrom(startX, startY, texture.Image);
            }

            // Create the DirectX texture atlas
            DirectXTexture = TextureLoad.Load(GraphicsDevice, tempBitmap);
        }

        public void PrepareDataForDirectX()
        {
            Dispose();

            // Rebuild the texture atlas and covert it to a DirectX texture
            RebuildTextureAtlas();

            var tempMoveables = new ConcurrentDictionary<uint, SkinnedModel>();
            var tempStatics = new ConcurrentDictionary<uint, StaticModel>();

            // Create movable models
            Parallel.For(0, Moveables.Count, i =>
              {
                  var mov = Moveables.ElementAt(i).Value;
                  tempMoveables.TryAdd(mov.ObjectID, SkinnedModel.FromWad2(GraphicsDevice, this, mov, PackedTextures));
              });

            // Create static meshes
            Parallel.For(0, Statics.Count, i =>
              {
                  var staticMesh = Statics.ElementAt(i).Value;
                  tempStatics.TryAdd(staticMesh.ObjectID, StaticModel.FromWad2(GraphicsDevice, this, staticMesh, PackedTextures));
              });

            foreach (var mov in tempMoveables)
                DirectXMoveables.Add(mov.Key, mov.Value);

            foreach (var stat in tempStatics)
                DirectXStatics.Add(stat.Key, stat.Value);

            // Prepare sprites
            for (int i = 0; i < SpriteTextures.Count; i++)
            {
                var sprite = SpriteTextures.ElementAt(i).Value;
                sprite.DirectXTexture = TextureLoad.Load(GraphicsDevice, sprite.Image);
            }
        }
    }
}
