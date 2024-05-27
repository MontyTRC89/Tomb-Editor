using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class WadRenderer : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public GraphicsDevice GraphicsDevice { get; }
        public Texture2D Texture { get; private set; } = null;

        private IDictionary<WadMoveable, AnimatedModel> Moveables { get; } = new Dictionary<WadMoveable, AnimatedModel>();
        private IDictionary<WadStatic, StaticModel> Statics { get; } = new Dictionary<WadStatic, StaticModel>();

        private IList<RectPacker> TexturePackers { get;} = new List<RectPacker>();
        private IDictionary<WadTexture, VectorInt3> PackedTextures { get; } = new Dictionary<WadTexture, VectorInt3>();
        private bool _compactTexture;
        private bool _correctTexture;

        private bool _disposing = false;

        public WadRenderer(GraphicsDevice graphicsDevice, bool compactTexture, bool correctTexture)
        {
            GraphicsDevice = graphicsDevice;
            _compactTexture = compactTexture;
            _correctTexture = correctTexture;
            AddPacker();
        }

        private void AddPacker()
        {
            var size = new VectorInt2(TextureAtlasSize, TextureAtlasSize);
            TexturePackers.Add(_compactTexture ? new RectPackerTree(size) : new RectPackerSimpleStack(size));
        }

        // Size of the atlas
        // DX10 requires minimum 8K textures support for hardware certification so we should be safe with this
        // Raildex 27/05/2024:
        // Reduced Atlas Size to 1024, because dynamic array size allows more stuff to be packed.
        // Reduces the average wastage of memory.
        public const int TextureAtlasSize = 1024;

        public void Dispose()
        {
            // Avoid multiple attempts at cleanup
            if (_disposing)
                return;

            _disposing = true;

            Texture?.Dispose();
            Texture = null;

            foreach (var obj in Moveables.Values)
                obj.Dispose();
            Moveables.Clear();

            foreach (var obj in Statics.Values)
                obj.Dispose();
            Statics.Clear();

            _disposing = false;
        }

        public void GarbageCollect()
        {
            Dispose();
            InitializeTexture();
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
                model = AnimatedModel.FromWadMoveable(GraphicsDevice, moveable, AllocateTexture, _correctTexture);
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
                model.Meshes.Add(ObjectMesh.FromWad2(GraphicsDevice,  @static.Mesh, AllocateTexture, _correctTexture));
                model.UpdateBuffers();
            }
            catch (TextureAtlasFullException exc)
            {
                logger.Error(exc.Message);

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

        private VectorInt3 AllocateTexture(WadTexture texture)
        {
            // Check if the texture is already loaded
            {
                VectorInt3 position;
                if (PackedTextures.TryGetValue(texture, out position))
                    return position;
            }

            // Allocate texture
            {
                VectorInt3? position = null;

                for(int packerIndex = 0; position is null; packerIndex++)
                {
                    bool addedPacker = false;
                    if(packerIndex == TexturePackers.Count)
                    {
                        AddPacker();
                        addedPacker = true;
                    }
                        
                    var sizeToPack = texture.Image.Size;
                    var pos = TexturePackers[packerIndex].TryAdd(sizeToPack);
                    if (pos is not null)
                        position = new VectorInt3(pos.Value.X, pos.Value.Y, packerIndex);
                    else if (pos is null && addedPacker) /* We added a new empty atlas and the texture did not fit at all*/
                        throw new TextureAtlasFullException();
                }

                // Upload texture
                InitializeTexture();
                EnsureTextureCapacity();
                TextureLoad.Update(GraphicsDevice, Texture, texture.Image, position.Value);
                PackedTextures.Add(texture, position.Value);
                return position.Value;
            }
        }

        // At runtime we can't be sure how many textures the array contains,
        // so we create a new texture array on demand and copy the old content over
        private void EnsureTextureCapacity()
        {
            var arraySize = Texture.Description.ArraySize;
            
            if(TexturePackers.Count >= arraySize)
            {
                var newTexture = Texture2D.New(GraphicsDevice, TextureAtlasSize, TextureAtlasSize, SharpDX.DXGI.Format.B8G8R8A8_UNorm, TextureFlags.ShaderResource, TexturePackers.Count, SharpDX.Direct3D11.ResourceUsage.Default);
                for(int i = 0; i < arraySize;i++)
                {
                    var fromSubresource = Texture.GetSubResourceIndex(i, 0);
                    var toSubresource = newTexture.GetSubResourceIndex(i, 0);
                    GraphicsDevice.Copy(Texture, fromSubresource, newTexture, toSubresource);
                }
                Texture = newTexture;
            }
        }
        private void InitializeTexture()
        {
            if (Texture is null)
                Texture = Texture2D.New(GraphicsDevice, TextureAtlasSize, TextureAtlasSize, SharpDX.DXGI.Format.B8G8R8A8_UNorm, TextureFlags.ShaderResource, TexturePackers.Count, SharpDX.Direct3D11.ResourceUsage.Default);
        }
        

        public class TextureAtlasFullException : Exception
        {
            public TextureAtlasFullException()
                : base("Texture atlas is full. Unable to add a wad texture.")
            { }
        }
    }
}
