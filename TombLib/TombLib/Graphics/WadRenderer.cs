using NLog;
using System;
using System.Collections.Generic;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Graphics
{
    // WAD asset cache, abstract base. Owns the rect-packed atlas algorithm + the
    // per-WAD AnimatedModel / StaticModel CPU cache. Backend-specific atlas
    // upload (DX11 Texture2D vs Vulkan VkImage) is delegated through three
    // virtual hooks: OnInitializeTexture / OnEnsureCapacity / OnUploadSubregion.
    //
    // Subclasses:
    //   - Dx11WadRenderer (TombLib/Graphics/Dx11WadRenderer.cs) — Silk.NET D3D11 Texture2DArray.
    //   - VulkanWadRenderer (TombLib.Rendering/Rendering/Vulkan) — VkImage Texture2DArray.
    //
    // Construct via DeviceManager.CreateWadRenderer(...) so callers don't have
    // to switch on the active backend themselves.
    public abstract class WadRenderer : IDisposable
    {
        public record struct AllocationResult(VectorInt3 Position, VectorInt2 OriginalSize, VectorInt2 AllocatedSize, VectorInt2 AtlasDimension);

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Atlas resource handle that callers pass to RenderArgs.Atlas. Concrete
        // type depends on the backend: a SharpDX.Direct3D11.ShaderResourceView
        // on DX11, a Silk.NET.Vulkan.ImageView on Vulkan. Typed as object so the
        // abstract layer stays backend-agnostic.
        public abstract object Texture { get; }

        private IDictionary<WadMoveable, AnimatedModel> Moveables { get; } = new Dictionary<WadMoveable, AnimatedModel>();
        private IDictionary<WadStatic, StaticModel> Statics { get; } = new Dictionary<WadStatic, StaticModel>();

        private IList<RectPacker> TexturePackers { get; } = new List<RectPacker>();
        private IDictionary<WadTexture, AllocationResult> PackedTextures { get; } = new Dictionary<WadTexture, WadRenderer.AllocationResult>();
        private readonly bool _compactTexture;
        private readonly bool _correctTexture;
        private bool _disposing = false;
        private readonly int _textureAtlasSize;
        private readonly int _maxTextureAllocationSize;
        public int TextureAtlasSize => _textureAtlasSize;
        public int CurrentPageCount => TexturePackers.Count;
        private readonly bool _loadAnimations;

        protected WadRenderer(bool compactTexture, bool correctTexture, int atlasSize, int maxAllocationSize, bool loadAnimations)
        {
            _compactTexture = compactTexture;
            _correctTexture = correctTexture;
            _textureAtlasSize = atlasSize;
            _maxTextureAllocationSize = maxAllocationSize;
            _loadAnimations = loadAnimations;
            AddPacker();
        }

        // ===== Backend hooks =====

        // Create the underlying texture array on first upload (lazy).
        protected abstract void OnInitializeTexture();
        // Resize the underlying texture array to at least `pages` slices,
        // copying every existing slice across.
        protected abstract void OnEnsureCapacity(int pages);
        // Copy `image` into slice `position.Z`, at pixel offset (X,Y).
        protected abstract void OnUploadSubregion(ImageC image, VectorInt3 position);
        // Free the underlying atlas GPU resource. Called from Dispose() before
        // we re-add an empty packer for the next rebuild.
        protected abstract void OnDisposeTexture();

        // ===== Shared logic =====

        private void AddPacker()
        {
            var size = new VectorInt2(_textureAtlasSize, _textureAtlasSize);
            TexturePackers.Add(_compactTexture ? new RectPackerTree(size) : new RectPackerSimpleStack(size));
        }

        public virtual void Dispose()
        {
            if (_disposing)
                return;

            _disposing = true;
            OnDisposeTexture();

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
            OnInitializeTexture();
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

            // Upload texture via the backend hook.
            OnInitializeTexture();
            OnEnsureCapacity(TexturePackers.Count);
            OnUploadSubregion(imageToPack, newPosition.Value);
            var result = new AllocationResult(newPosition.Value,
                new VectorInt2(originalWidth, originalHeight),
                new VectorInt2(imageToPack.Width, imageToPack.Height),
                new VectorInt2(_textureAtlasSize, _textureAtlasSize));
            PackedTextures.Add(texture, result);
            return result;
        }

        public class TextureAtlasFullException : Exception
        {
            public TextureAtlasFullException()
                : base("Texture atlas is full. Unable to add a wad texture.")
            { }
        }
    }
}
