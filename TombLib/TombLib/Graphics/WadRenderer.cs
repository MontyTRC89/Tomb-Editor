using NLog;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Graphics
{
    // WAD asset cache. Owns the texture atlas (Texture2DArray + ShaderResourceView)
    // shared by every moveable / static / imported geometry rendered via the unified
    // path. After the SharpDX.Toolkit removal it works on raw SharpDX.Direct3D11.
    //
    // Lifecycle:
    //   - Constructor takes a Device (the underlying ID3D11Device, e.g. obtained via
    //     Dx11RenderingDevice.Device).
    //   - GetMoveable / GetStatic build per-WAD models on demand and cache them.
    //   - When the atlas runs out of room we Dispose the entire cache and rebuild —
    //     same behavior as the legacy WadRenderer (see GarbageCollect).
    //   - The renderer (Dx11RenderingDrawingMesh.RenderArgs.Atlas) accepts the
    //     ShaderResourceView returned by .Texture — bindable directly to a slot.
    public class WadRenderer : IDisposable
    {
        public record struct AllocationResult(VectorInt3 Position, VectorInt2 OriginalSize, VectorInt2 AllocatedSize, VectorInt2 AtlasDimension);

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Device Device { get; }
        // Atlas texture (Texture2DArray) and its SRV. Texture is created on first
        // upload and grown via EnsureTextureCapacity when more pages are needed.
        public Texture2D AtlasTexture { get; private set; }
        public ShaderResourceView Texture { get; private set; }

        private IDictionary<WadMoveable, AnimatedModel> Moveables { get; } = new Dictionary<WadMoveable, AnimatedModel>();
        private IDictionary<WadStatic, StaticModel> Statics { get; } = new Dictionary<WadStatic, StaticModel>();

        private IList<RectPacker> TexturePackers { get; } = new List<RectPacker>();
        private IDictionary<WadTexture, AllocationResult> PackedTextures { get; } = new Dictionary<WadTexture, WadRenderer.AllocationResult>();
        private bool _compactTexture;
        private bool _correctTexture;
        private bool _disposing = false;
        private int _textureAtlasSize;
        private int _maxTextureAllocationSize;
        public int TextureAtlasSize { get => _textureAtlasSize; }
        private bool _loadAnimations;

        public WadRenderer(Device device, bool compactTexture, bool correctTexture, int atlasSize, int maxAllocationSize, bool loadAnimations)
        {
            Device = device;
            _compactTexture = compactTexture;
            _correctTexture = correctTexture;
            _textureAtlasSize = atlasSize;
            _maxTextureAllocationSize = maxAllocationSize;
            _loadAnimations = loadAnimations;
            AddPacker();
        }

        private void AddPacker()
        {
            var size = new VectorInt2(_textureAtlasSize, _textureAtlasSize);
            TexturePackers.Add(_compactTexture ? new RectPackerTree(size) : new RectPackerSimpleStack(size));
        }

        public void Dispose()
        {
            if (_disposing)
                return;

            _disposing = true;
            Texture?.Dispose();
            Texture = null;
            AtlasTexture?.Dispose();
            AtlasTexture = null;

            foreach (var obj in Moveables.Values)
                obj.Dispose();
            Moveables.Clear();

            foreach (var obj in Statics.Values)
                obj.Dispose();
            Statics.Clear();

            PackedTextures.Clear();
            TexturePackers.Clear();
            AddPacker();

            _disposing = false;
        }

        public void GarbageCollect()
        {
            Dispose();
            InitializeTexture();
        }

        public AnimatedModel GetMoveable(WadMoveable moveable, bool maybeRebuildAll = true)
        {
            AnimatedModel model;
            if (Moveables.TryGetValue(moveable, out model))
            {
                if (model.Version >= moveable.Version)
                    return model;
                model.Dispose();
                Moveables.Remove(moveable);
            }

            try
            {
                model = AnimatedModel.FromWadMoveable(moveable, AllocateTexture, _correctTexture, _loadAnimations);
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
            StaticModel model;
            if (Statics.TryGetValue(@static, out model))
            {
                if (model.Version >= @static.Version)
                    return model;
                model.Dispose();
                Statics.Remove(@static);
            }

            try
            {
                model = new StaticModel();
                model.Meshes.Add(ObjectMesh.FromWad2(@static.Mesh, AllocateTexture, _correctTexture));
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

        private AllocationResult AllocateTexture(WadTexture texture)
        {
            if (PackedTextures.TryGetValue(texture, out AllocationResult position))
                return position;

            ImageC imageToPack = texture.Image;
            int originalWidth = texture.Image.Width;
            int originalHeight = texture.Image.Height;
            int biggestDimension = Math.Max(imageToPack.Width, imageToPack.Height);

            // Down-sample large textures to fit the per-image budget.
            if (biggestDimension > _maxTextureAllocationSize)
            {
                float scaleFactor = _maxTextureAllocationSize / (float)biggestDimension;
                int newWidth = Math.Max((int)(imageToPack.Width * scaleFactor), 4);
                int newHeight = Math.Max((int)(imageToPack.Height * scaleFactor), 4);
                imageToPack = ImageC.Resize(imageToPack, newWidth, newHeight);
            }

            VectorInt2 sizeToPack = imageToPack.Size;
            VectorInt3? newPosition = null;

            for (int packerIndex = 0; newPosition is null; packerIndex++)
            {
                bool addedPacker = false;
                if (packerIndex == TexturePackers.Count)
                {
                    AddPacker();
                    addedPacker = true;
                }

                VectorInt2? pos = TexturePackers[packerIndex].TryAdd(sizeToPack);
                if (pos is not null)
                {
                    newPosition = new VectorInt3(pos.Value.X, pos.Value.Y, packerIndex);
                    break;
                }
                else if (addedPacker) // Added a new empty atlas page and the texture STILL didn't fit
                    throw new TextureAtlasFullException();
            }

            // Upload texture
            InitializeTexture();
            EnsureTextureCapacity();
            TextureLoad.Update(Device.ImmediateContext, AtlasTexture, imageToPack, newPosition.Value);
            var result = new AllocationResult(newPosition.Value, new VectorInt2(originalWidth, originalHeight), new VectorInt2(imageToPack.Width, imageToPack.Height), new VectorInt2(_textureAtlasSize, _textureAtlasSize));
            PackedTextures.Add(texture, result);
            return result;
        }

        // At runtime we can't be sure how many texture array slices we'll need, so we
        // start with one slice and grow by allocating a new bigger array and copying
        // each existing slice across when more pages are needed.
        private void EnsureTextureCapacity()
        {
            int arraySize = AtlasTexture.Description.ArraySize;
            if (TexturePackers.Count > arraySize)
            {
                var newDesc = AtlasTexture.Description;
                newDesc.ArraySize = TexturePackers.Count;
                var newTexture = new Texture2D(Device, newDesc);
                for (int i = 0; i < arraySize; i++)
                {
                    int fromSubresource = Texture2D.CalculateSubResourceIndex(0, i, 1);
                    int toSubresource = Texture2D.CalculateSubResourceIndex(0, i, 1);
                    Device.ImmediateContext.CopySubresourceRegion(AtlasTexture, fromSubresource, null, newTexture, toSubresource);
                }
                Texture?.Dispose();
                AtlasTexture?.Dispose();
                AtlasTexture = newTexture;
                Texture = new ShaderResourceView(Device, AtlasTexture);
            }
        }

        private void InitializeTexture()
        {
            if (AtlasTexture is null)
            {
                AtlasTexture = new Texture2D(Device, new Texture2DDescription
                {
                    Width = _textureAtlasSize,
                    Height = _textureAtlasSize,
                    MipLevels = 1,
                    ArraySize = TexturePackers.Count,
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                });
                Texture = new ShaderResourceView(Device, AtlasTexture);
            }
        }

        public class TextureAtlasFullException : Exception
        {
            public TextureAtlasFullException()
                : base("Texture atlas is full. Unable to add a wad texture.")
            { }
        }
    }
}
