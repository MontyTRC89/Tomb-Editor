using System;
using Silk.NET.OpenGL;
using TombLib.Utils;

namespace TombLib.Rendering.OpenGL
{
    // GL_TEXTURE_2D_ARRAY atlas. Base class handles rect-packing + GC;
    // this class owns the GPU texture and implements per-region upload via
    // glTexSubImage3D.
    //
    // Format: RGBA8 (ImageC is BGRA in memory, uploaded as GL_BGRA).
    public sealed class OpenGLRenderingTextureAllocator : RenderingTextureAllocator
    {
        public readonly OpenGLRenderingDevice DeviceWrapper;
        public uint AtlasTexture { get; private set; }

        private readonly GL _gl;

        public unsafe OpenGLRenderingTextureAllocator(OpenGLRenderingDevice device, Description description)
            : base(device, description)
        {
            DeviceWrapper = device;
            _gl = device.Gl;

            uint tex = 0;
            _gl.GenTextures(1, &tex);
            _gl.BindTexture(TextureTarget.Texture2DArray, tex);
            _gl.TexStorage3D(TextureTarget.Texture2DArray, 1, SizedInternalFormat.Rgba8,
                (uint)description.Size.X, (uint)description.Size.Y, (uint)description.Size.Z);
            _gl.BindTexture(TextureTarget.Texture2DArray, 0);
            AtlasTexture = tex;
        }

        public override unsafe void Dispose()
        {
            if (AtlasTexture != 0)
            {
                uint t = AtlasTexture;
                _gl.DeleteTextures(1, &t);
                AtlasTexture = 0;
            }
        }

        protected override unsafe void UploadTexture(RenderingTexture texture, VectorInt3 pos)
        {
            int width = texture.To.X - texture.From.X;
            int height = texture.To.Y - texture.From.Y;

            // 1-pixel mirrored border to prevent atlas bleeding under linear
            // sampling — same trick as Dx11/Vulkan.
            var padded = ImageC.CreateNew(width + 2, height + 2);
            padded.CopyFrom(1, 1, texture.Image, texture.From.X, texture.From.Y, width, height);
            padded.SetPixel(0, 0,         padded.GetPixel(1, 1));
            padded.SetPixel(width + 1, 0, padded.GetPixel(width, 1));
            padded.SetPixel(0, height + 1,         padded.GetPixel(1, height));
            padded.SetPixel(width + 1, height + 1, padded.GetPixel(width, height));
            padded.CopyFrom(0, 1, padded, 1, 1, 1, height);
            padded.CopyFrom(width + 1, 1, padded, width, 1, 1, height);
            padded.CopyFrom(1, 0, padded, 1, 1, width, 1);
            padded.CopyFrom(1, height + 1, padded, 1, height, width, 1);

            _gl.BindTexture(TextureTarget.Texture2DArray, AtlasTexture);

            padded.GetIntPtr(ptr =>
            {
                _gl.TexSubImage3D(TextureTarget.Texture2DArray, 0,
                    pos.X, pos.Y, pos.Z,
                    (uint)padded.Width, (uint)padded.Height, 1,
                    PixelFormat.Bgra, PixelType.UnsignedByte, (void*)ptr);
            });

            _gl.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        // Debug readback — not ported, returns empty image.
        public override ImageC RetrieveTestImage() => ImageC.CreateNew(Size.X, Size.Y);
    }
}
