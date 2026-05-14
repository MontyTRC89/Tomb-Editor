using System;
using System.IO;
using System.Linq;
using NLog;
using Silk.NET.OpenGL;
using TombLib.Utils;

namespace TombLib.Rendering.OpenGL
{
    // OpenGL 4.1 rendering backend. Uses Silk.NET.OpenGL as a thin binding.
    // Single shared HGLRC context; per-window rendering via wglMakeCurrent to
    // each panel's HDC before drawing.
    //
    // WGL context setup:
    //   1. Create a hidden dummy HWND
    //   2. GetDC → HDC, ChoosePixelFormat + SetPixelFormat (32-bit color, 24-bit depth, double-buffer)
    //   3. wglCreateContext → HGLRC
    //   4. wglMakeCurrent
    //   5. GL.GetApi via wglGetProcAddress
    //   6. Create shared resources (samplers, sector textures, etc.)
    public sealed class OpenGLRenderingDevice : RenderingDevice
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public GL Gl { get; private set; }
        public IntPtr Hglrc { get; private set; }

        // Shared samplers
        public uint SamplerAniso { get; private set; }
        public uint SamplerPoint { get; private set; }

        // Sector-overlay texture array (arrows, slides, illegal-slope markers).
        // 256x256 per-layer, BGRA8. Used by OpenGLRenderingDrawingRoom.
        public uint SectorTextureArray { get; private set; }

        // Dummy window used for initial context creation.
        private IntPtr _dummyHwnd;
        private IntPtr _dummyHdc;

        public unsafe OpenGLRenderingDevice()
        {
            CreateDummyContext();

            Gl = GL.GetApi(name => WglNative.GetAnyGLProcAddress(name));

            string version = Gl.GetStringS(StringName.Version);
            string renderer = Gl.GetStringS(StringName.Renderer);
            logger.Info("OpenGLRenderingDevice initialised. GL=\"{0}\" renderer=\"{1}\"", version, renderer);

            CreateSamplers();
            CreateSectorTextureArray();
        }

        private void CreateDummyContext()
        {
            IntPtr hInstance = WglNative.GetModuleHandleW(null);
            // Use the built-in "STATIC" window class to avoid RegisterClass.
            _dummyHwnd = WglNative.CreateWindowExW(
                0, "STATIC", "TombEditorGLDummy", 0,
                0, 0, 1, 1,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
            if (_dummyHwnd == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create dummy window for GL context.");

            _dummyHdc = WglNative.GetDC(_dummyHwnd);

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

            int pixelFormat = WglNative.ChoosePixelFormat(_dummyHdc, ref pfd);
            if (pixelFormat == 0)
                throw new InvalidOperationException("ChoosePixelFormat failed.");
            WglNative.SetPixelFormat(_dummyHdc, pixelFormat, ref pfd);

            Hglrc = WglNative.wglCreateContext(_dummyHdc);
            if (Hglrc == IntPtr.Zero)
                throw new InvalidOperationException("wglCreateContext failed.");
            WglNative.wglMakeCurrent(_dummyHdc, Hglrc);
        }

        // Ensures the GL context is current on the given HDC. Call this before
        // any GL operations targeting a specific window.
        public void MakeCurrent(IntPtr hdc)
        {
            WglNative.wglMakeCurrent(hdc, Hglrc);
        }

        private unsafe void CreateSamplers()
        {
            uint aniso = 0;
            Gl.GenSamplers(1, &aniso);
            Gl.SamplerParameter(aniso, SamplerParameterI.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            Gl.SamplerParameter(aniso, SamplerParameterI.TextureMagFilter, (int)TextureMagFilter.Linear);
            Gl.SamplerParameter(aniso, SamplerParameterI.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.SamplerParameter(aniso, SamplerParameterI.TextureWrapT, (int)TextureWrapMode.Repeat);
            Gl.SamplerParameter(aniso, SamplerParameterF.MaxAnisotropy, 4.0f);
            SamplerAniso = aniso;

            uint point = 0;
            Gl.GenSamplers(1, &point);
            Gl.SamplerParameter(point, SamplerParameterI.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.SamplerParameter(point, SamplerParameterI.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Gl.SamplerParameter(point, SamplerParameterI.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.SamplerParameter(point, SamplerParameterI.TextureWrapT, (int)TextureWrapMode.Repeat);
            SamplerPoint = point;
        }

        // Load sector-overlay PNGs from embedded resources into a GL_TEXTURE_2D_ARRAY.
        // Same data as the Vulkan/DX11 backends: 256x256 per layer, BGRA.
        private unsafe void CreateSectorTextureArray()
        {
            const int Size = 256;
            string[] names = Enum.GetNames(typeof(SectorTexture)).Skip(1).ToArray();
            int layers = names.Length;
            var assembly = typeof(OpenGLRenderingDevice).Assembly;

            uint tex = 0;
            Gl.GenTextures(1, &tex);
            Gl.BindTexture(TextureTarget.Texture2DArray, tex);
            Gl.TexStorage3D(TextureTarget.Texture2DArray, 1, SizedInternalFormat.Rgba8, (uint)Size, (uint)Size, (uint)layers);

            for (int i = 0; i < layers; i++)
            {
                string resourceName = "TombLib.Rendering.SectorTextures." + names[i] + ".png";
                using (Stream s = assembly.GetManifestResourceStream(resourceName))
                {
                    if (s == null)
                        throw new InvalidOperationException("Missing embedded resource: " + resourceName);
                    ImageC img = ImageC.FromStream(s);
                    if (img.Width != Size || img.Height != Size)
                        throw new ArgumentOutOfRangeException("SectorTexture wrong size: " + resourceName);

                    img.GetIntPtr(ptr =>
                    {
                        Gl.TexSubImage3D(TextureTarget.Texture2DArray, 0,
                            0, 0, i,
                            (uint)Size, (uint)Size, 1,
                            PixelFormat.Bgra, PixelType.UnsignedByte, (void*)ptr);
                    });
                }
            }

            Gl.BindTexture(TextureTarget.Texture2DArray, 0);
            SectorTextureArray = tex;
        }

        // ---- Factory methods ------------------------------------------------

        public override RenderingSwapChain CreateSwapChain(RenderingSwapChain.Description description)
            => new OpenGLRenderingSwapChain(this, description);

        public override RenderingTextureAllocator CreateTextureAllocator(RenderingTextureAllocator.Description description)
            => new OpenGLRenderingTextureAllocator(this, description);

        public override RenderingStateBuffer CreateStateBuffer()
            => new OpenGLRenderingStateBuffer(this);

        public override RenderingFont CreateFont(RenderingFont.Description description)
            => new RenderingFont(description);

        public override RenderingDrawingLines CreateDrawingLines(RenderingDrawingLines.Description description)
            => new OpenGLRenderingDrawingLines(this, description);

        public override RenderingDrawingTest CreateDrawingTest(RenderingDrawingTest.Description description)
            => new OpenGLRenderingDrawingTest(this, description);

        public override RenderingDrawingRoom CreateDrawingRoom(RenderingDrawingRoom.Description description)
            => new OpenGLRenderingDrawingRoom(this, description);

        public override RenderingDrawingMesh CreateDrawingMesh(RenderingDrawingMesh.Description description)
            => new OpenGLRenderingDrawingMesh(this, description);

        public override RenderingDrawingImportedGeometry CreateDrawingImportedGeometry(RenderingDrawingImportedGeometry.Description description)
            => new OpenGLRenderingDrawingImportedGeometry(this, description);

        // ---- Shader helpers -------------------------------------------------

        // Compiles a GLSL shader and returns its GL name. Throws on error.
        public uint CompileShader(ShaderType type, string source)
        {
            uint shader = Gl.CreateShader(type);
            Gl.ShaderSource(shader, source);
            Gl.CompileShader(shader);
            Gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string log = Gl.GetShaderInfoLog(shader);
                Gl.DeleteShader(shader);
                throw new InvalidOperationException($"GLSL {type} compile error: {log}");
            }
            return shader;
        }

        // Links a VS+FS into a program. Deletes shaders after linking.
        public uint LinkProgram(uint vs, uint fs)
        {
            uint program = Gl.CreateProgram();
            Gl.AttachShader(program, vs);
            Gl.AttachShader(program, fs);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                string log = Gl.GetProgramInfoLog(program);
                Gl.DeleteProgram(program);
                throw new InvalidOperationException("GLSL link error: " + log);
            }
            Gl.DetachShader(program, vs);
            Gl.DetachShader(program, fs);
            Gl.DeleteShader(vs);
            Gl.DeleteShader(fs);
            return program;
        }

        // Compiles VS+FS source strings, links, and sets up uniform block bindings
        // and sampler texture unit bindings.
        public uint CreateProgram(string vsSource, string fsSource,
            (string name, uint bindingPoint)[] uniformBlocks = null,
            (string name, int textureUnit)[] samplers = null)
        {
            uint vs = CompileShader(ShaderType.VertexShader, vsSource);
            uint fs = CompileShader(ShaderType.FragmentShader, fsSource);
            uint program = LinkProgram(vs, fs);

            if (uniformBlocks != null)
            {
                foreach (var (name, bp) in uniformBlocks)
                {
                    uint idx = Gl.GetUniformBlockIndex(program, name);
                    if (idx != uint.MaxValue) // GL_INVALID_INDEX
                        Gl.UniformBlockBinding(program, idx, bp);
                }
            }

            if (samplers != null)
            {
                Gl.UseProgram(program);
                foreach (var (name, unit) in samplers)
                {
                    int loc = Gl.GetUniformLocation(program, name);
                    if (loc >= 0)
                        Gl.Uniform1(loc, unit);
                }
                Gl.UseProgram(0);
            }

            return program;
        }

        // ---- Cleanup --------------------------------------------------------

        public override unsafe void Dispose()
        {
            if (Gl != null)
            {
                if (SectorTextureArray != 0) { uint t = SectorTextureArray; Gl.DeleteTextures(1, &t); SectorTextureArray = 0; }
                if (SamplerAniso != 0) { uint s = SamplerAniso; Gl.DeleteSamplers(1, &s); SamplerAniso = 0; }
                if (SamplerPoint != 0) { uint s = SamplerPoint; Gl.DeleteSamplers(1, &s); SamplerPoint = 0; }
            }

            if (Hglrc != IntPtr.Zero)
            {
                WglNative.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                WglNative.wglDeleteContext(Hglrc);
                Hglrc = IntPtr.Zero;
            }
            if (_dummyHdc != IntPtr.Zero && _dummyHwnd != IntPtr.Zero)
            {
                WglNative.ReleaseDC(_dummyHwnd, _dummyHdc);
                _dummyHdc = IntPtr.Zero;
            }
            if (_dummyHwnd != IntPtr.Zero)
            {
                WglNative.DestroyWindow(_dummyHwnd);
                _dummyHwnd = IntPtr.Zero;
            }
            Gl?.Dispose();
            Gl = null;
        }
    }
}
