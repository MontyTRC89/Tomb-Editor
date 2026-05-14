using System;
using Silk.NET.OpenGL;
using TombLib.Utils;

namespace TombLib.Rendering.OpenGL
{
    // Standalone GL texture for per-submesh imported-geometry textures.
    // Equivalent to VulkanTexture2D. NOT the atlas — that lives on
    // OpenGLRenderingTextureAllocator.
    public sealed class OpenGLTexture2D : IDisposable
    {
        public uint Texture { get; private set; }
        public int Width { get; }
        public int Height { get; }

        private readonly GL _gl;

        public unsafe OpenGLTexture2D(OpenGLRenderingDevice device, ImageC source)
        {
            _gl = device.Gl;
            Width = source.Width;
            Height = source.Height;

            uint tex = 0;
            _gl.GenTextures(1, &tex);
            _gl.BindTexture(TextureTarget.Texture2D, tex);
            _gl.TexStorage2D(TextureTarget.Texture2D, 1, SizedInternalFormat.Rgba8, (uint)Width, (uint)Height);

            source.GetIntPtr(ptr =>
            {
                _gl.TexSubImage2D(TextureTarget.Texture2D, 0,
                    0, 0, (uint)Width, (uint)Height,
                    PixelFormat.Bgra, PixelType.UnsignedByte, (void*)ptr);
            });

            _gl.BindTexture(TextureTarget.Texture2D, 0);
            Texture = tex;
        }

        public unsafe void Dispose()
        {
            if (Texture != 0) { uint t = Texture; _gl.DeleteTextures(1, &t); Texture = 0; }
        }
    }
}
