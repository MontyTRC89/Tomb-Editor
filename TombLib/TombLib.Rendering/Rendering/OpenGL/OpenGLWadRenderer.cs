using Silk.NET.OpenGL;
using TombLib.Graphics;
using TombLib.Utils;

namespace TombLib.Rendering.OpenGL
{
    // OpenGL 4.1 implementation of WadRenderer's atlas. The atlas is a GL
    // TEXTURE_2D_ARRAY (immutable storage, RGBA8). Growing the layer count
    // requires allocating a fresh texture and copying old layers across,
    // because glTexStorage3D is immutable — same strategy as Vulkan/DX11.
    //
    // Layer copy uses an FBO + glCopyTexSubImage3D (works on 4.1; avoids the
    // 4.3-only glCopyImageSubData).
    //
    // The asset cache (Moveables/Statics dictionaries) and rect-packing logic
    // live in WadRenderer (base class) and are reused unchanged.
    public sealed class OpenGLWadRenderer : WadRenderer
    {
        public OpenGLRenderingDevice DeviceWrapper { get; }
        public uint AtlasTexture { get; private set; }

        private readonly GL _gl;
        private uint _copyFbo;
        private int _currentArrayLayers;

        // RenderArgs.Atlas is typed as object; OpenGLRenderingDrawingMesh /
        // OpenGLRenderingDrawingImportedGeometry both recognise a boxed uint
        // GL texture name via their ResolveAtlas / ResolveTexture helpers.
        public override object Texture => AtlasTexture;

        public OpenGLWadRenderer(OpenGLRenderingDevice device, bool compactTexture, bool correctTexture, int atlasSize, int maxAllocationSize, bool loadAnimations)
            : base(compactTexture, correctTexture, atlasSize, maxAllocationSize, loadAnimations)
        {
            DeviceWrapper = device;
            _gl = device.Gl;
        }

        protected override unsafe void OnInitializeTexture()
        {
            if (AtlasTexture != 0) return;
            CreateAtlas(CurrentPageCount);
        }

        protected override unsafe void OnEnsureCapacity(int pages)
        {
            if (pages <= _currentArrayLayers) return;

            uint oldTex = AtlasTexture;
            int oldLayers = _currentArrayLayers;

            // Allocate the new (larger) texture array.
            uint newTex = 0;
            _gl.GenTextures(1, &newTex);
            _gl.BindTexture(TextureTarget.Texture2DArray, newTex);
            _gl.TexStorage3D(TextureTarget.Texture2DArray, 1, SizedInternalFormat.Rgba8,
                (uint)TextureAtlasSize, (uint)TextureAtlasSize, (uint)pages);

            // Copy each existing layer via a single reusable read FBO.
            if (oldLayers > 0 && oldTex != 0)
            {
                if (_copyFbo == 0)
                {
                    uint fbo = 0;
                    _gl.GenFramebuffers(1, &fbo);
                    _copyFbo = fbo;
                }
                _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _copyFbo);
                for (int i = 0; i < oldLayers; i++)
                {
                    _gl.FramebufferTextureLayer(FramebufferTarget.ReadFramebuffer,
                        FramebufferAttachment.ColorAttachment0, oldTex, 0, i);
                    _gl.CopyTexSubImage3D(TextureTarget.Texture2DArray, 0,
                        0, 0, i,
                        0, 0,
                        (uint)TextureAtlasSize, (uint)TextureAtlasSize);
                }
                _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);

                uint t = oldTex;
                _gl.DeleteTextures(1, &t);
            }

            _gl.BindTexture(TextureTarget.Texture2DArray, 0);
            AtlasTexture = newTex;
            _currentArrayLayers = pages;
        }

        private unsafe void CreateAtlas(int pages)
        {
            uint tex = 0;
            _gl.GenTextures(1, &tex);
            _gl.BindTexture(TextureTarget.Texture2DArray, tex);
            _gl.TexStorage3D(TextureTarget.Texture2DArray, 1, SizedInternalFormat.Rgba8,
                (uint)TextureAtlasSize, (uint)TextureAtlasSize, (uint)pages);
            _gl.BindTexture(TextureTarget.Texture2DArray, 0);
            AtlasTexture = tex;
            _currentArrayLayers = pages;
        }

        protected override unsafe void OnUploadSubregion(ImageC image, VectorInt3 position)
        {
            if (image.Width == 0 || image.Height == 0) return;

            _gl.BindTexture(TextureTarget.Texture2DArray, AtlasTexture);
            image.GetIntPtr(ptr =>
            {
                _gl.TexSubImage3D(TextureTarget.Texture2DArray, 0,
                    position.X, position.Y, position.Z,
                    (uint)image.Width, (uint)image.Height, 1,
                    PixelFormat.Bgra, PixelType.UnsignedByte, (void*)ptr);
            });
            _gl.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        protected override unsafe void OnDisposeTexture()
        {
            if (AtlasTexture != 0)
            {
                uint t = AtlasTexture;
                _gl.DeleteTextures(1, &t);
                AtlasTexture = 0;
            }
            if (_copyFbo != 0)
            {
                uint f = _copyFbo;
                _gl.DeleteFramebuffers(1, &f);
                _copyFbo = 0;
            }
            _currentArrayLayers = 0;
        }
    }
}
