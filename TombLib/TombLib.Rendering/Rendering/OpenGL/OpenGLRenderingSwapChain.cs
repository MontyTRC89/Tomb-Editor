using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.OpenGL;
using TombLib.Utils;

namespace TombLib.Rendering.OpenGL
{
    // Per-window OpenGL presentation surface. Uses WGL to render to the panel's
    // HDC with the shared HGLRC. MSAA is implemented via a multisample FBO that
    // is resolved (blitted) to the default framebuffer before SwapBuffers.
    //
    // Frame cycle:
    //   1. Clear() — wglMakeCurrent(panelHDC, globalHGLRC), clear color+depth
    //   2. Drawing* classes issue GL draw commands
    //   3. Present() — if MSAA: blit MSAA FBO → default FB; SwapBuffers(panelHDC)
    public sealed class OpenGLRenderingSwapChain : RenderingSwapChain
    {
        public readonly OpenGLRenderingDevice DeviceWrapper;
        private readonly GL _gl;
        private readonly IntPtr _hwnd;
        private IntPtr _hdc;
        private readonly bool _antialiasRequested;

        // MSAA resources (0 when MSAA is off)
        private uint _msaaFbo;
        private uint _msaaColorRbo;
        private uint _msaaDepthRbo;
        private int _msaaSamples;

        // Non-MSAA depth renderbuffer attached to default FBO workaround:
        // The default framebuffer on Windows already has a depth buffer from
        // the pixel format. We don't need a separate one for non-MSAA.
        // But for MSAA we render to _msaaFbo which needs its own depth.

        // ---- Text overlay resources -----------------------------------------
        private uint _textProgram;
        private uint _textVao;
        private uint _textVbo;
        private uint _textVboCapacity;

        // ---- Sprite overlay resources ---------------------------------------
        private uint _spriteProgram;
        private uint _spriteVao;
        private uint _spriteVbo;
        private uint _spriteVboCapacity;

        // ---- GLSL shaders (inline, adapted from Vulkan #version 450 → 410) -

        private const string TextVertGlsl = @"#version 410
layout(location = 0) in vec2 inPosition;
layout(location = 1) in uvec2 inUvw;

out vec3 fsUvw;
flat out int fsBlendMode;

void main() {
    gl_Position = vec4(inPosition, 1.0, 1.0);
    uint u = inUvw.x & 0xffffffu;
    uint v = (inUvw.x >> 24) | ((inUvw.y & 0xffffu) << 8);
    uint w = (inUvw.y >> 16) & 0xfffu;
    fsUvw = vec3(float(u) / 16777216.0, float(v) / 16777216.0, float(w));
    fsBlendMode = int(inUvw.y >> 28);
}
";
        private const string TextFragGlsl = @"#version 410
uniform sampler2DArray FontTexture;

in vec3 fsUvw;
flat in int fsBlendMode;

layout(location = 0) out vec4 outColor;

void main() {
    if (fsBlendMode == 0) {
        vec4 s = texture(FontTexture, fsUvw);
        float a = (s.r + s.g + s.b) / 3.0;
        outColor = vec4(s.rgb * a, a);
    } else {
        outColor = vec4(0.0, 0.0, 0.0, 0.6);
    }
}
";

        private const string SpriteVertGlsl = @"#version 410
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inColor;
layout(location = 2) in uvec2 inUvw;

out vec4 fsColor;
out vec3 fsUvw;

void main() {
    gl_Position = vec4(inPosition, 1.0);
    uint u = inUvw.x & 0xffffffu;
    uint v = (inUvw.x >> 24) | ((inUvw.y & 0xffffu) << 8);
    uint w = (inUvw.y >> 16) & 0xfffu;
    fsUvw = vec3(float(u) / 16777216.0, float(v) / 16777216.0, float(w));
    fsColor = inColor;
}
";
        private const string SpriteFragGlsl = @"#version 410
uniform sampler2DArray SpriteTexture;

in vec4 fsColor;
in vec3 fsUvw;

layout(location = 0) out vec4 outColor;

void main() {
    vec4 s = texture(SpriteTexture, fsUvw);
    vec4 result = s * fsColor * s.a;
    result.rgb *= result.a;
    if (result.a <= 0.05)
        discard;
    outColor = result;
}
";

        public unsafe OpenGLRenderingSwapChain(OpenGLRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _gl = device.Gl;
            _hwnd = description.WindowHandle;
            _antialiasRequested = description.Antialias;
            Size = description.Size;

            // Get HDC for this window and set its pixel format.
            _hdc = WglNative.GetDC(_hwnd);
            var pfd = new WglNative.PIXELFORMATDESCRIPTOR
            {
                nSize = (ushort)System.Runtime.InteropServices.Marshal.SizeOf<WglNative.PIXELFORMATDESCRIPTOR>(),
                nVersion = 1,
                dwFlags = WglNative.PFD_DRAW_TO_WINDOW | WglNative.PFD_SUPPORT_OPENGL | WglNative.PFD_DOUBLEBUFFER,
                iPixelType = WglNative.PFD_TYPE_RGBA,
                cColorBits = 32,
                cDepthBits = 24,
                cStencilBits = 8,
                iLayerType = WglNative.PFD_MAIN_PLANE,
            };
            int pf = WglNative.ChoosePixelFormat(_hdc, ref pfd);
            WglNative.SetPixelFormat(_hdc, pf, ref pfd);

            device.MakeCurrent(_hdc);

            CreateMsaaFbo();
            CreateTextResources();
            CreateSpriteResources();
        }

        private unsafe void CreateMsaaFbo()
        {
            if (!_antialiasRequested || Size.X <= 0 || Size.Y <= 0)
            {
                _msaaSamples = 0;
                return;
            }

            _msaaSamples = 4;

            uint fbo = 0;
            _gl.GenFramebuffers(1, &fbo);
            _msaaFbo = fbo;

            uint colorRbo = 0;
            _gl.GenRenderbuffers(1, &colorRbo);
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRbo);
            _gl.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, (uint)_msaaSamples,
                InternalFormat.Rgba8, (uint)Size.X, (uint)Size.Y);
            _msaaColorRbo = colorRbo;

            uint depthRbo = 0;
            _gl.GenRenderbuffers(1, &depthRbo);
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRbo);
            _gl.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, (uint)_msaaSamples,
                InternalFormat.Depth24Stencil8, (uint)Size.X, (uint)Size.Y);
            _msaaDepthRbo = depthRbo;

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _msaaFbo);
            _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                RenderbufferTarget.Renderbuffer, _msaaColorRbo);
            _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer, _msaaDepthRbo);
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void DestroyMsaaFbo()
        {
            unsafe
            {
                if (_msaaFbo != 0) { uint f = _msaaFbo; _gl.DeleteFramebuffers(1, &f); _msaaFbo = 0; }
                if (_msaaColorRbo != 0) { uint r = _msaaColorRbo; _gl.DeleteRenderbuffers(1, &r); _msaaColorRbo = 0; }
                if (_msaaDepthRbo != 0) { uint r = _msaaDepthRbo; _gl.DeleteRenderbuffers(1, &r); _msaaDepthRbo = 0; }
            }
        }

        private void CreateTextResources()
        {
            _textProgram = DeviceWrapper.CreateProgram(TextVertGlsl, TextFragGlsl,
                samplers: new[] { ("FontTexture", 0) });

            unsafe
            {
                uint vao = 0; _gl.GenVertexArrays(1, &vao); _textVao = vao;
                uint vbo = 0; _gl.GenBuffers(1, &vbo); _textVbo = vbo;
            }
        }

        private void CreateSpriteResources()
        {
            _spriteProgram = DeviceWrapper.CreateProgram(SpriteVertGlsl, SpriteFragGlsl,
                samplers: new[] { ("SpriteTexture", 0) });

            unsafe
            {
                uint vao = 0; _gl.GenVertexArrays(1, &vao); _spriteVao = vao;
                uint vbo = 0; _gl.GenBuffers(1, &vbo); _spriteVbo = vbo;
            }
        }

        // ---- Frame cycle ----------------------------------------------------

        public override void Clear(Vector4 color)
        {
            DeviceWrapper.MakeCurrent(_hdc);

            // Bind MSAA FBO or default framebuffer
            if (_msaaSamples > 0)
                _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _msaaFbo);
            else
                _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            _gl.Viewport(0, 0, (uint)Size.X, (uint)Size.Y);
            _gl.ClearColor(color.X, color.Y, color.Z, color.W);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // Default state
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Lequal);
            _gl.DepthMask(true);
            _gl.Enable(EnableCap.CullFace);
            _gl.CullFace(TriangleFace.Back);
            _gl.FrontFace(FrontFaceDirection.CW); // Match D3D11 default (CW = front)
        }

        public override void ClearDepth()
        {
            _gl.Clear(ClearBufferMask.DepthBufferBit);
        }

        public override void Present()
        {
            if (_msaaSamples > 0)
            {
                // Blit MSAA FBO → default framebuffer (resolve)
                _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _msaaFbo);
                _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                _gl.BlitFramebuffer(0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y,
                    ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
                _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            WglNative.SwapBuffers(_hdc);
        }

        public override void Resize(VectorInt2 newSize)
        {
            if (newSize.X <= 0 || newSize.Y <= 0) return;
            if (newSize == Size) return;
            Size = newSize;

            DeviceWrapper.MakeCurrent(_hdc);
            DestroyMsaaFbo();
            CreateMsaaFbo();
        }

        // ---- Sprites --------------------------------------------------------

        public override unsafe void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, bool noZ, List<Sprite> sprites)
        {
            if (sprites.Count == 0) return;

            var glAllocator = (OpenGLRenderingTextureAllocator)textureAllocator;
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            int vertexCount = sprites.Count * 6;
            uint posBytes = (uint)(vertexCount * sizeof(Vector3));
            uint colBytes = (uint)(vertexCount * sizeof(Vector4));
            uint uvwBytes = (uint)(vertexCount * sizeof(ulong));
            uint totalBytes = posBytes + colBytes + uvwBytes;

            EnsureVbo(ref _spriteVbo, ref _spriteVboCapacity, totalBytes);

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _spriteVbo);
            byte* dst = (byte*)_gl.MapBufferRange(BufferTargetARB.ArrayBuffer, 0, totalBytes,
                MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapInvalidateBufferBit);

            Vector3* positions = (Vector3*)dst;
            Vector4* colours = (Vector4*)(dst + posBytes);
            ulong* uvws = (ulong*)(dst + posBytes + colBytes);

            for (int i = 0; i < sprites.Count; ++i)
            {
                Sprite sprite = sprites[i];
                VectorInt3 texPos = textureAllocator.Get(sprite.Texture);
                VectorInt2 texSize = sprite.Texture.To - sprite.Texture.From;
                float depth = sprite.Depth ?? 1.0f;

                positions[i * 6 + 0] = new Vector3(sprite.Pos00.X, sprite.Pos00.Y, depth);
                positions[i * 6 + 2] = positions[i * 6 + 3] = new Vector3(sprite.Pos10.X, sprite.Pos10.Y, depth);
                positions[i * 6 + 1] = positions[i * 6 + 4] = new Vector3(sprite.Pos01.X, sprite.Pos01.Y, depth);
                positions[i * 6 + 5] = new Vector3(sprite.Pos11.X, sprite.Pos11.Y, depth);

                uvws[i * 6 + 1] = uvws[i * 6 + 4] = CompressUvw(texPos, textureScaling, new Vector2(0.5f, 0.5f));
                uvws[i * 6 + 5] = CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, 0.5f));
                uvws[i * 6 + 0] = CompressUvw(texPos, textureScaling, new Vector2(0.5f, texSize.Y - 0.5f));
                uvws[i * 6 + 2] = uvws[i * 6 + 3] = CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, texSize.Y - 0.5f));

                for (int j = 0; j < 6; j++)
                    colours[i * 6 + j] = sprite.Tint;
            }

            _gl.UnmapBuffer(BufferTargetARB.ArrayBuffer);

            // Setup VAO
            _gl.BindVertexArray(_spriteVao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _spriteVbo);

            // location 0: vec3 position
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vector3), (void*)0);

            // location 1: vec4 color
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Vector4), (void*)posBytes);

            // location 2: uvec2 uvw
            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribIPointer(2, 2, VertexAttribIType.UnsignedInt, (uint)sizeof(ulong), (void*)(posBytes + colBytes));

            // Bind texture + sampler
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2DArray, glAllocator.AtlasTexture);
            _gl.BindSampler(0, linearFilter ? DeviceWrapper.SamplerAniso : DeviceWrapper.SamplerPoint);

            // Draw state
            _gl.UseProgram(_spriteProgram);
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

            if (noZ)
            {
                _gl.Disable(EnableCap.DepthTest);
                _gl.DepthMask(false);
            }
            else
            {
                _gl.Enable(EnableCap.DepthTest);
                _gl.DepthMask(true);
            }
            _gl.Disable(EnableCap.CullFace);

            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)vertexCount);

            // Restore
            _gl.BindVertexArray(0);
            _gl.BindSampler(0, 0);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthMask(true);
            _gl.Enable(EnableCap.CullFace);
        }

        // ---- Glyphs ---------------------------------------------------------

        public override unsafe void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays)
        {
            int vertexCount = glyphRenderInfos.Count * 6 + overlays.Count * 6;
            if (vertexCount == 0) return;

            var glAllocator = (OpenGLRenderingTextureAllocator)textureAllocator;
            Vector2 posScaling = new Vector2(1.0f) / (Size / 2);
            Vector2 posOffset = VectorInt2.FromRounded(posScaling * 0.5f);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            uint posBytes = (uint)(vertexCount * sizeof(Vector2));
            uint uvwBytes = (uint)(vertexCount * sizeof(ulong));
            uint totalBytes = posBytes + uvwBytes;

            EnsureVbo(ref _textVbo, ref _textVboCapacity, totalBytes);

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _textVbo);
            byte* dst = (byte*)_gl.MapBufferRange(BufferTargetARB.ArrayBuffer, 0, totalBytes,
                MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapInvalidateBufferBit);

            Vector2* positions = (Vector2*)dst;
            ulong* uvws = (ulong*)(dst + posBytes);

            int c = 0;
            ulong overlayUvw = CompressUvw(VectorInt3.Zero, Vector2.Zero, Vector2.Zero, 1);
            for (int i = 0; i < overlays.Count; ++i, ++c)
            {
                var overlay = overlays[i];
                Vector2 posStart = overlay.Start * posScaling + posOffset;
                Vector2 posEnd = (overlay.End + new Vector2(1)) * posScaling + posOffset;

                positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                uvws[c * 6 + 0] = uvws[c * 6 + 1] = uvws[c * 6 + 2] =
                uvws[c * 6 + 3] = uvws[c * 6 + 4] = uvws[c * 6 + 5] = overlayUvw;
            }

            for (int i = 0; i < glyphRenderInfos.Count; ++i, ++c)
            {
                RenderingFont.GlyphRenderInfo info = glyphRenderInfos[i];
                Vector2 posStart = info.PosStart * posScaling + posOffset;
                Vector2 posEnd = (info.PosEnd - new Vector2(1)) * posScaling + posOffset;

                positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                uvws[c * 6 + 0] = CompressUvw(info.TexStart, textureScaling, Vector2.Zero);
                uvws[c * 6 + 2] = uvws[c * 6 + 3] = CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, 0));
                uvws[c * 6 + 1] = uvws[c * 6 + 4] = CompressUvw(info.TexStart, textureScaling, new Vector2(0, info.TexSize.Y - 1));
                uvws[c * 6 + 5] = CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, info.TexSize.Y - 1));
            }

            _gl.UnmapBuffer(BufferTargetARB.ArrayBuffer);

            // Setup VAO
            _gl.BindVertexArray(_textVao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _textVbo);

            // location 0: vec2 position
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vector2), (void*)0);

            // location 1: uvec2 uvw
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribIPointer(1, 2, VertexAttribIType.UnsignedInt, (uint)sizeof(ulong), (void*)posBytes);

            // Bind font texture
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2DArray, glAllocator.AtlasTexture);
            _gl.BindSampler(0, DeviceWrapper.SamplerPoint);

            // Draw state: no depth, premultiplied alpha
            _gl.UseProgram(_textProgram);
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            _gl.Disable(EnableCap.DepthTest);
            _gl.DepthMask(false);
            _gl.Disable(EnableCap.CullFace);

            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)vertexCount);

            // Restore
            _gl.BindVertexArray(0);
            _gl.BindSampler(0, 0);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthMask(true);
            _gl.Enable(EnableCap.CullFace);
        }

        // ---- Helpers --------------------------------------------------------

        private unsafe void EnsureVbo(ref uint vbo, ref uint capacity, uint required)
        {
            if (capacity >= required) return;
            uint newCap = Math.Max(required, capacity * 2);
            if (newCap < 4096) newCap = 4096;
            capacity = newCap;

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, newCap, null, BufferUsageARB.StreamDraw);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        private static ulong CompressUvw(VectorInt3 position, Vector2 textureScaling, Vector2 uv, uint highestBits = 0)
        {
            uint blendMode2 = Math.Min(highestBits, 15);
            uint x = (uint)((position.X + uv.X) * textureScaling.X);
            uint y = (uint)((position.Y + uv.Y) * textureScaling.Y);
            return x | ((ulong)y << 24) | ((ulong)position.Z << 48) | ((ulong)blendMode2 << 60);
        }

        // ---- Dispose --------------------------------------------------------

        public override unsafe void Dispose()
        {
            DestroyMsaaFbo();

            if (_textVbo != 0) { uint b = _textVbo; _gl.DeleteBuffers(1, &b); _textVbo = 0; }
            if (_textVao != 0) { uint v = _textVao; _gl.DeleteVertexArrays(1, &v); _textVao = 0; }
            if (_textProgram != 0) { _gl.DeleteProgram(_textProgram); _textProgram = 0; }

            if (_spriteVbo != 0) { uint b = _spriteVbo; _gl.DeleteBuffers(1, &b); _spriteVbo = 0; }
            if (_spriteVao != 0) { uint v = _spriteVao; _gl.DeleteVertexArrays(1, &v); _spriteVao = 0; }
            if (_spriteProgram != 0) { _gl.DeleteProgram(_spriteProgram); _spriteProgram = 0; }

            if (_hdc != IntPtr.Zero && _hwnd != IntPtr.Zero)
            {
                WglNative.ReleaseDC(_hwnd, _hdc);
                _hdc = IntPtr.Zero;
            }
        }
    }
}
