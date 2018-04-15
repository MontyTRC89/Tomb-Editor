using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class WadRenderer : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public GraphicsDevice GraphicsDevice { get; }
        private Dictionary<WadMoveable, AnimatedModel> Moveables { get; } = new Dictionary<WadMoveable, AnimatedModel>();
        private Dictionary<WadStatic, StaticModel> Statics { get; } = new Dictionary<WadStatic, StaticModel>();

        public Texture2D Texture { get; private set; } = null;
        private RectPacker TexturePacker { get; set; }
        private Dictionary<WadTexture, VectorInt2> PackedTextures { get; set; }
        private bool _compactTexture;

        public WadRenderer(GraphicsDevice graphicsDevice, bool compactTexture = false)
        {
            GraphicsDevice = graphicsDevice;
            _compactTexture = compactTexture;
            CreateTexturePacker();
        }

        private void CreateTexturePacker()
        {
            PackedTextures = new Dictionary<WadTexture, VectorInt2>();
            if (_compactTexture)
                TexturePacker = new RectPackerTree(new VectorInt2(TextureAtlasSize, TextureAtlasSize));
            else
                TexturePacker = new RectPackerSimpleStack(new VectorInt2(TextureAtlasSize, TextureAtlasSize));
        }

        // Size of the atlas
        // DX10 requires minimum 8K textures support for hardware certification so we should be safe with this
        public const int TextureAtlasSize = 4096;

        public void Dispose()
        {
            Texture?.Dispose();
            Texture = null;
            CreateTexturePacker();

            foreach (var obj in Moveables.Values)
                obj.Dispose();
            Moveables.Clear();

            foreach (var obj in Statics.Values)
                obj.Dispose();
            Statics.Clear();
        }

        private void ReclaimTextureSpace<T, U>(Model<T, U> model) where U : struct
        {
            // TODO Some mechanism to reclaim texture space without rebuilding the atlas would be good.
            // Currently texture space is only freed when the atlas fills up completely and must be rebuilt.
        }

        public AnimatedModel GetMoveable(WadMoveable moveable, bool maybeRebuildAll = true)
        {
            // Check if the data is already loaded
            // If yes attempt to use that one
            AnimatedModel model;
            if (Moveables.TryGetValue(moveable, out model))
            {
                if (model.Version >= moveable.Version)
                    return model;
                ReclaimTextureSpace(model);
                model.Dispose();
                Moveables.Remove(moveable);
            }

            // The data is either new or has changed, unfortunately we need to reload it
            try
            {
                model = AnimatedModel.FromWadMoveable(GraphicsDevice, moveable, AllocateTexture);
            }
            catch (TextureAtlasFullException exc)
            {
                logger.Info(exc.Message);

                if (maybeRebuildAll)
                {
                    logger.Info("Starting to rebuild the entire atlas.");
                    Dispose();
                    return GetMoveable(moveable, false);
                }
            }
            Moveables.Add(moveable, model);
            return model;
        }

        public StaticModel GetStatic(WadStatic @static, bool maybeRebuildAll = true)
        {
            // Check if the data is already loaded
            // If yes attempt to use that one
            StaticModel model;
            if (Statics.TryGetValue(@static, out model))
            {
                if (model.Version >= @static.Version)
                    return model;
                ReclaimTextureSpace(model);
                model.Dispose();
                Statics.Remove(@static);
            }

            // The data is either new or has changed, unfortunately we need to reload it
            try
            {
                model = new StaticModel(GraphicsDevice);
                model.Meshes.Add(ObjectMesh.FromWad2(GraphicsDevice, @static.Mesh, AllocateTexture));
                model.UpdateBuffers();
            }
            catch (TextureAtlasFullException exc)
            {
                logger.Info(exc.Message);

                if (maybeRebuildAll)
                {
                    logger.Info("Starting to rebuild the entire atlas.");
                    Dispose();
                    return GetStatic(@static, false);
                }
            }

            Statics.Add(@static, model);
            return model;
        }

        private VectorInt2 AllocateTexture(WadTexture texture)
        {
            // Check if the texture is already loaded
            {
                VectorInt2 position;
                if (PackedTextures.TryGetValue(texture, out position))
                    return position;
            }

            // Allocate texture
            {
                VectorInt2? position = TexturePacker.TryAdd(texture.Image.Size);
                if (position == null)
                    throw new TextureAtlasFullException();

                // Upload texture
                if (Texture == null)
                    Texture = Texture2D.New(GraphicsDevice, TextureAtlasSize, TextureAtlasSize, SharpDX.DXGI.Format.B8G8R8A8_UNorm, TextureFlags.ShaderResource, 1, SharpDX.Direct3D11.ResourceUsage.Default);
                TextureLoad.Update(GraphicsDevice, Texture, texture.Image, position.Value);
                return position.Value;
            }
        }

        private void PreloadReplaceTextures(IEnumerable<WadMoveable> newMoveables, IEnumerable<WadStatic> newStatics)
        {
            Dispose();

            // Collect textures
            var textures = new HashSet<WadTexture>();
            foreach (var moveable in newMoveables)
                foreach (var mesh in moveable.Meshes)
                    foreach (WadPolygon polygon in mesh.Polys)
                        textures.Add((WadTexture)polygon.Texture.Texture);
            foreach (var stat in newStatics)
                foreach (WadPolygon polygon in stat.Mesh.Polys)
                    textures.Add((WadTexture)polygon.Texture.Texture);

            // Order textures for packing
            var packedTextures = new List<WadTexture>(textures);
            packedTextures.Sort(new ComparerWadTextures());

            // Pack the textures in a single atlas
            CreateTexturePacker();
            foreach (var texture in packedTextures)
            {
                VectorInt2? positionInAtlas = TexturePacker.TryAdd(texture.Image.Size);
                if (positionInAtlas == null)
                    throw new TextureAtlasFullException();
                PackedTextures.Add(texture, positionInAtlas.Value);
            }

            // Create texture atlas
            var tempBitmap = ImageC.CreateNew(TextureAtlasSize, TextureAtlasSize);
            foreach (var texture in PackedTextures)
                tempBitmap.CopyFrom(texture.Value.X, texture.Value.Y, texture.Key.Image);
            Texture = TextureLoad.Load(GraphicsDevice, tempBitmap, SharpDX.Direct3D11.ResourceUsage.Default);
        }

        // This method is optional. By sorting the wad textures it may result ahead of time
        // we may have better texture space efficency.
        public void PreloadReplaceWad(IEnumerable<WadMoveable> newMoveables, IEnumerable<WadStatic> newStatics)
        {
            Dispose();
            try
            {
                PreloadReplaceTextures(newMoveables, newStatics);
            }
            catch (TextureAtlasFullException)
            {
                logger.Error("Unable to preload wad because the texture atlas became too full!");
                return;
            }

            // Create moveable models
            Parallel.ForEach(newMoveables, moveable =>
            {
                var model = AnimatedModel.FromWadMoveable(GraphicsDevice, moveable, AllocateTexture);
                lock (Moveables)
                    Moveables.Add(moveable, model);
            });

            // Create static meshes
            Parallel.ForEach(newStatics, @static =>
            {
                var model = new StaticModel(GraphicsDevice);
                model.Meshes.Add(ObjectMesh.FromWad2(GraphicsDevice, @static.Mesh, AllocateTexture));
                model.UpdateBuffers();
                lock (Statics)
                    Statics.Add(@static, model);
            });
        }

        public class TextureAtlasFullException : Exception
        {
            public TextureAtlasFullException()
                : base("Texture atlas is full. Unable to add a wad texture..")
            { }
        }
    }
}
