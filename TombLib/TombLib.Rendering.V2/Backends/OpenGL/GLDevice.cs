using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;
using TombLib.RenderingV2.Rhi;
using GLEnum = Silk.NET.OpenGL.GLEnum;
using RhiFormat   = TombLib.RenderingV2.Rhi.Format;
using BufferDesc  = TombLib.RenderingV2.Rhi.BufferDesc;
using TextureDesc = TombLib.RenderingV2.Rhi.TextureDesc;
using SamplerDesc = TombLib.RenderingV2.Rhi.SamplerDesc;
using SwapchainDesc = TombLib.RenderingV2.Rhi.SwapchainDesc;

namespace TombLib.RenderingV2.Backends.OpenGL;

/// <summary>
/// OpenGL 4.3 Core Profile backend for <see cref="IRhiDevice"/>. Uses
/// WGL on Windows to create the GL context against an HWND's DC, and
/// Silk.NET.OpenGL for the GL function loader.
///
/// <para>Design choices that mirror what the DX11 / Vulkan backends do:
///   - Single-threaded. All calls must come from the rendering thread the
///     context was made current on.
///   - Resource pools (Buffers / Textures / Samplers / Pipelines / Swapchains)
///     keyed by uint handles so the RHI surface stays opaque.
///   - One pipeline = one linked GL program + one VAO that captures the
///     vertex attribute layout. SetPipeline becomes glUseProgram +
///     glBindVertexArray + apply fixed-function state.
///   - The "swapchain" is really just the WGL context's default
///     framebuffer (FBO 0); Present = wglSwapBuffers.</para>
/// </summary>
public unsafe sealed partial class GLDevice : IRhiDevice
{
    internal GL Gl = null!;
    internal IntPtr Hwnd;
    internal IntPtr Hdc;
    internal IntPtr Hglrc;
    private  WglLoader? _loader;

    // Resource pools.
    private uint _nextHandle = 1;
    internal readonly Dictionary<uint, GLBufferRes>    Buffers    = new();
    internal readonly Dictionary<uint, GLTextureRes>   Textures   = new();
    internal readonly Dictionary<uint, GLSamplerRes>   Samplers   = new();
    internal readonly Dictionary<uint, GLPipelineRes>  Pipelines  = new();
    internal readonly Dictionary<uint, GLSwapchainRes> Swapchains = new();

    public RhiCapabilities Capabilities { get; }

    public GLDevice()
    {
        // We need a Win32 window to create a GL context. The editor's
        // existing Panel3D HWND will be passed via CreateSwapchain — at
        // device-creation time we don't have one yet. Trick: create a
        // throwaway "dummy" Win32 window just to bootstrap a context, then
        // re-bind to the real HWND when CreateSwapchain is called.
        InitDummyContext();

        // Cache device capabilities.
        Gl.GetInteger(GetPName.MaxTextureSize, out int maxTex);
        Gl.GetInteger(GetPName.MaxArrayTextureLayers, out int maxLayers);
        bool aniso = HasExtension("GL_EXT_texture_filter_anisotropic")
                  || HasExtension("GL_ARB_texture_filter_anisotropic");
        Capabilities = new RhiCapabilities(
            backend:        RhiBackendKind.OpenGL,
            instanced:      true,
            structured:     true,
            nativePush:     false,
            anisotropy:     aniso,
            debugMarkers:   true,
            maxTexSize:     maxTex,
            maxArrayLayers: maxLayers);
    }

    internal uint AllocHandle() => _nextHandle++;

    public void Dispose()
    {
        foreach (var s in Swapchains.Values) { /* swapchain just holds HDC/Hwnd, nothing to free */ }
        foreach (var p in Pipelines.Values)
        {
            if (p.Vao != 0)    Gl.DeleteVertexArray(p.Vao);
            if (p.Program != 0) Gl.DeleteProgram(p.Program);
        }
        foreach (var s in Samplers.Values) if (s.Handle != 0) Gl.DeleteSampler(s.Handle);
        foreach (var t in Textures.Values)
        {
            if (t.FramebufferHandle != 0) Gl.DeleteFramebuffer(t.FramebufferHandle);
            if (t.Handle != 0) Gl.DeleteTexture(t.Handle);
        }
        foreach (var b in Buffers.Values)
        {
            if (b.Mapped != null) Gl.UnmapNamedBuffer(b.Handle);
            if (b.Handle != 0) Gl.DeleteBuffer(b.Handle);
        }
        Swapchains.Clear();
        Pipelines.Clear();
        Samplers.Clear();
        Textures.Clear();
        Buffers.Clear();

        if (Hglrc != IntPtr.Zero)
        {
            WglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            WglDeleteContext(Hglrc);
            Hglrc = IntPtr.Zero;
        }
        if (Hdc != IntPtr.Zero && Hwnd != IntPtr.Zero)
        {
            ReleaseDC(Hwnd, Hdc);
            Hdc = IntPtr.Zero;
        }
    }

    public void WaitIdle()
    {
        if (Gl != null) Gl.Finish();
    }

    private bool HasExtension(string name)
    {
        Gl.GetInteger(GetPName.NumExtensions, out int n);
        for (uint i = 0; i < (uint)n; i++)
        {
            string ext = Gl.GetStringS(StringName.Extensions, i);
            if (ext == name) return true;
        }
        return false;
    }

    // ====================================================== Context bootstrap

    private const uint PFD_DRAW_TO_WINDOW = 0x00000004;
    private const uint PFD_SUPPORT_OPENGL = 0x00000020;
    private const uint PFD_DOUBLEBUFFER   = 0x00000001;
    private const byte PFD_TYPE_RGBA      = 0;
    private const byte PFD_MAIN_PLANE     = 0;
    private const int  WGL_CONTEXT_MAJOR_VERSION_ARB    = 0x2091;
    private const int  WGL_CONTEXT_MINOR_VERSION_ARB    = 0x2092;
    private const int  WGL_CONTEXT_PROFILE_MASK_ARB     = 0x9126;
    private const int  WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
    private const int  WGL_CONTEXT_FLAGS_ARB            = 0x2094;
    private const int  WGL_CONTEXT_DEBUG_BIT_ARB        = 0x00000001;

    private void InitDummyContext()
    {
        // 1. Create a hidden Win32 message-only window so we can pin a DC to
        //    it for the bootstrap context.
        Hwnd = CreateMessageWindow();
        Hdc  = GetDC(Hwnd);
        if (Hdc == IntPtr.Zero) throw new InvalidOperationException("GetDC failed for bootstrap window");

        var pfd = new PIXELFORMATDESCRIPTOR
        {
            nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            dwFlags  = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
            iPixelType = PFD_TYPE_RGBA,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
            iLayerType = PFD_MAIN_PLANE,
        };
        int format = ChoosePixelFormat(Hdc, ref pfd);
        if (format == 0) throw new InvalidOperationException("ChoosePixelFormat failed");
        if (!SetPixelFormat(Hdc, format, ref pfd))
            throw new InvalidOperationException("SetPixelFormat failed");

        // 2. Legacy context (any GL version) just to bootstrap.
        IntPtr legacy = WglCreateContext(Hdc);
        if (legacy == IntPtr.Zero) throw new InvalidOperationException("wglCreateContext failed");
        if (!WglMakeCurrent(Hdc, legacy))
            throw new InvalidOperationException("wglMakeCurrent on legacy context failed");

        // 3. Try to upgrade to a 4.3 core context via wglCreateContextAttribsARB.
        IntPtr wglCreateAttribs = WglGetProcAddress("wglCreateContextAttribsARB");
        if (wglCreateAttribs != IntPtr.Zero)
        {
            var del = Marshal.GetDelegateForFunctionPointer<WglCreateContextAttribsARB>(wglCreateAttribs);
            int[] attribs = {
                WGL_CONTEXT_MAJOR_VERSION_ARB, 4,
                WGL_CONTEXT_MINOR_VERSION_ARB, 3,
                WGL_CONTEXT_PROFILE_MASK_ARB,  WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
                // Debug bit when env var asks for it — costs ~5% on most drivers.
                WGL_CONTEXT_FLAGS_ARB,
                Environment.GetEnvironmentVariable("TOMBEDITOR_GL_DEBUG") == "1" ? WGL_CONTEXT_DEBUG_BIT_ARB : 0,
                0
            };
            IntPtr core;
            unsafe { fixed (int* p = attribs) { core = del(Hdc, IntPtr.Zero, (IntPtr)p); } }
            if (core != IntPtr.Zero)
            {
                WglMakeCurrent(Hdc, core);
                WglDeleteContext(legacy);
                Hglrc = core;
            }
            else
            {
                Hglrc = legacy; // fall back to legacy (will be reported below if version is too low)
            }
        }
        else
        {
            Hglrc = legacy;
        }

        // 4. Load the GL function table via Silk.NET. The loader callback
        //    asks WGL (or the system loader) for each function pointer.
        _loader = new WglLoader();
        Gl = GL.GetApi(_loader);

        // Confirm we got the version we wanted; older drivers may have
        // silently downgraded.
        Gl.GetInteger(GetPName.MajorVersion, out int major);
        Gl.GetInteger(GetPName.MinorVersion, out int minor);
        if (major * 100 + minor < 403)
            throw new InvalidOperationException(
                $"GL context is {major}.{minor}, need 4.3 or newer for the V2 renderer.");

        // 5. Hook the debug-message callback so any GL error reaches our log.
        if (HasExtension("GL_KHR_debug"))
        {
            Gl.Enable(EnableCap.DebugOutputSynchronous);
            _glDebugCallback = OnGlDebugMessage;
            Gl.DebugMessageCallback(_glDebugCallback, null);
        }
    }

    private static DebugProc? _glDebugCallback;
    private static void OnGlDebugMessage(GLEnum source, GLEnum type, int id, GLEnum severity,
                                          int length, nint message, nint userParam)
    {
        if (severity == GLEnum.DebugSeverityNotification) return;
        string msg = Marshal.PtrToStringAnsi(message, length) ?? "(no msg)";
        try
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TombEditorGL.log");
            System.IO.File.AppendAllText(path,
                $"[{DateTime.Now:HH:mm:ss.fff}] GL [{severity}] {msg}\n");
        }
        catch { /* never fail because of logging */ }
    }

    // -------------------- Win32 P/Invoke for WGL bootstrap --------------------

    [StructLayout(LayoutKind.Sequential)]
    private struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize, nVersion;
        public uint dwFlags;
        public byte iPixelType, cColorBits, cRedBits, cRedShift, cGreenBits, cGreenShift,
                    cBlueBits, cBlueShift, cAlphaBits, cAlphaShift,
                    cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits,
                    cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
        public uint dwLayerMask, dwVisibleMask, dwDamageMask;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowExW(uint dwExStyle, string lpClassName, string? lpWindowName,
        uint dwStyle, int X, int Y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hwnd);
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport("gdi32.dll")]
    private static extern bool SetPixelFormat(IntPtr hdc, int format, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport("gdi32.dll")]
    private static extern bool SwapBuffers(IntPtr hdc);

    [DllImport("opengl32.dll", EntryPoint = "wglCreateContext")]
    private static extern IntPtr WglCreateContext(IntPtr hdc);
    [DllImport("opengl32.dll", EntryPoint = "wglDeleteContext")]
    private static extern bool WglDeleteContext(IntPtr hglrc);
    [DllImport("opengl32.dll", EntryPoint = "wglMakeCurrent")]
    private static extern bool WglMakeCurrent(IntPtr hdc, IntPtr hglrc);
    /// <summary>Exposed for GLCommandList.BeginPass — same WGL call.</summary>
    internal static bool WglMakeCurrentExt(IntPtr hdc, IntPtr hglrc) => WglMakeCurrent(hdc, hglrc);
    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress")]
    private static extern IntPtr WglGetProcAddress(string name);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandleW(string? lpModuleName);
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string name);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr WglCreateContextAttribsARB(IntPtr hdc, IntPtr hShareContext, IntPtr attribs);

    private static IntPtr s_msgWindow;
    private static IntPtr CreateMessageWindow()
    {
        if (s_msgWindow != IntPtr.Zero) return s_msgWindow;
        const uint WS_POPUP = 0x80000000;
        // Re-use any built-in Win32 class — "STATIC" works and is always
        // registered. We never make this window visible.
        s_msgWindow = CreateWindowExW(0, "STATIC", "TombEditorGL", WS_POPUP,
                                       0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, GetModuleHandleW(null), IntPtr.Zero);
        if (s_msgWindow == IntPtr.Zero) throw new InvalidOperationException("CreateWindowEx failed for GL bootstrap");
        return s_msgWindow;
    }

    /// <summary>
    /// Silk.NET INativeContext loader implementation that funnels every
    /// GL function lookup through wglGetProcAddress + opengl32.dll
    /// GetProcAddress (the latter for GL 1.1 entry points that
    /// wglGetProcAddress won't return).
    /// </summary>
    private sealed class WglLoader : INativeContext
    {
        private static IntPtr _opengl32 = GetModuleHandleW("opengl32.dll");
        public nint GetProcAddress(string proc, int? slot = null)
        {
            IntPtr p = WglGetProcAddress(proc);
            if (p == IntPtr.Zero || p == (IntPtr)1 || p == (IntPtr)2 || p == (IntPtr)3 || p == (IntPtr)(-1))
                p = GLDevice.GetProcAddress(_opengl32, proc);
            return p;
        }
        public bool TryGetProcAddress(string proc, out nint addr, int? slot = null)
        {
            addr = GetProcAddress(proc);
            return addr != IntPtr.Zero;
        }
        public void Dispose() { }
    }

    // ====================================================== Buffers

    public BufferHandle CreateBuffer(in BufferDesc desc, ReadOnlySpan<byte> initialData)
    {
        uint bo;
        Gl.CreateBuffers(1, &bo);

        // Always allocate with DynamicStorageBit so glNamedBufferSubData
        // works for any later UpdateBuffer call. We *don't* use persistent
        // mapping anymore — it required per-driver memory barriers to make
        // CPU writes visible to subsequent draws, and getting that wrong
        // shows up as ghost / flickering frames after editor state changes.
        // glNamedBufferSubData is enough for the editor's update rates.
        BufferStorageMask flags = BufferStorageMask.DynamicStorageBit;

        if (initialData.Length > 0)
        {
            fixed (byte* src = initialData)
                Gl.NamedBufferStorage(bo, (nuint)desc.SizeBytes, src, flags);
        }
        else
        {
            Gl.NamedBufferStorage(bo, (nuint)desc.SizeBytes, null, flags);
        }

        var res = new GLBufferRes
        {
            Handle    = bo,
            Size      = desc.SizeBytes,
            Mapped    = null,
            Usage     = desc.Usage,
            BindFlags = desc.BindFlags,
        };
        uint id = AllocHandle();
        Buffers[id] = res;
        return new BufferHandle(id);
    }

    public void Destroy(BufferHandle h)
    {
        if (Buffers.Remove(h.Id, out var b))
        {
            if (b.Handle != 0) Gl.DeleteBuffer(b.Handle);
        }
    }

    // ====================================================== Textures

    public TextureHandle CreateTexture(in TextureDesc desc, ReadOnlySpan<byte> initialData)
    {
        var (ifmt, pfmt, ptype) = GLMapping.ToGl(desc.Format);
        int mip = Math.Max(1, desc.MipLevels);

        uint tex;
        Gl.CreateTextures(TextureTarget.Texture2D, 1, &tex);
        Gl.TextureStorage2D(tex, (uint)mip, (SizedInternalFormat)ifmt, (uint)desc.Width, (uint)desc.Height);

        if (initialData.Length > 0)
        {
            fixed (byte* src = initialData)
                Gl.TextureSubImage2D(tex, 0, 0, 0, (uint)desc.Width, (uint)desc.Height, pfmt, ptype, src);
        }

        // Default texture parameters — samplers will override per-binding,
        // but reasonable defaults make naked sampling work too.
        Gl.TextureParameter(tex, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        Gl.TextureParameter(tex, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        Gl.TextureParameter(tex, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        Gl.TextureParameter(tex, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        var res = new GLTextureRes
        {
            Handle    = tex,
            Width     = desc.Width,
            Height    = desc.Height,
            MipLevels = mip,
            Format    = desc.Format,
            BindFlags = desc.BindFlags,
            IsDepth   = GLMapping.ToGl(desc.Format).fmt is PixelFormat.DepthComponent or PixelFormat.DepthStencil,
        };
        uint id = AllocHandle();
        Textures[id] = res;
        return new TextureHandle(id);
    }

    public void UpdateTexture(TextureHandle handle, int subresource,
                              int x, int y, int width, int height,
                              int rowPitchBytes, ReadOnlySpan<byte> data)
    {
        var t = Textures[handle.Id];
        var (_, pfmt, ptype) = GLMapping.ToGl(t.Format);
        // GL pixel stores rowsBytes via GL_UNPACK_ROW_LENGTH (in pixels).
        int bpp = BytesPerPixelOf(t.Format);
        Gl.PixelStore(PixelStoreParameter.UnpackRowLength, rowPitchBytes / bpp);
        try
        {
            fixed (byte* src = data)
                Gl.TextureSubImage2D(t.Handle, subresource, x, y, (uint)width, (uint)height, pfmt, ptype, src);
        }
        finally
        {
            Gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
        }
    }

    public byte[] ReadTexture(TextureHandle handle, int subresource = 0)
    {
        var t = Textures[handle.Id];
        var (_, pfmt, ptype) = GLMapping.ToGl(t.Format);
        int bpp = BytesPerPixelOf(t.Format);
        int size = t.Width * t.Height * bpp;
        byte[] result = new byte[size];
        fixed (byte* dst = result)
            Gl.GetTextureImage(t.Handle, subresource, pfmt, ptype, (uint)size, dst);
        return result;
    }

    public void Destroy(TextureHandle h)
    {
        if (Textures.Remove(h.Id, out var t))
        {
            if (t.FramebufferHandle != 0) Gl.DeleteFramebuffer(t.FramebufferHandle);
            if (t.Handle != 0) Gl.DeleteTexture(t.Handle);
        }
    }

    private static int BytesPerPixelOf(RhiFormat f) => f switch
    {
        RhiFormat.R8G8B8A8_UNorm or RhiFormat.R8G8B8A8_UNorm_SRgb or RhiFormat.B8G8R8A8_UNorm
            or RhiFormat.R8G8B8A8_UInt or RhiFormat.R32_UInt or RhiFormat.R32_Float
            or RhiFormat.R16G16_UNorm or RhiFormat.R16G16_Float
            or RhiFormat.D24_UNorm_S8_UInt or RhiFormat.D32_Float => 4,
        RhiFormat.R16G16B16A16_Float or RhiFormat.R16G16B16A16_UNorm or RhiFormat.R32G32_Float => 8,
        RhiFormat.R32G32B32_Float => 12,
        RhiFormat.R32G32B32A32_Float => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(f), f, "BytesPerPixel undefined"),
    };

    // ====================================================== Samplers

    public SamplerHandle CreateSampler(in SamplerDesc desc)
    {
        uint s;
        Gl.CreateSamplers(1, &s);

        // Lod range: with mip filter active, use the full range; otherwise 0..0.
        bool hasMips = desc.MipFilter != FilterMode.Nearest || desc.MinFilter != FilterMode.Nearest;
        Gl.SamplerParameter(s, SamplerParameterI.TextureMinFilter,
            (int)GLMapping.ToMinFilter(desc.MinFilter, desc.MipFilter, hasMips));
        Gl.SamplerParameter(s, SamplerParameterI.TextureMagFilter,
            (int)GLMapping.ToMagFilter(desc.MagFilter));
        Gl.SamplerParameter(s, SamplerParameterI.TextureWrapS, (int)GLMapping.ToGl(desc.AddressU));
        Gl.SamplerParameter(s, SamplerParameterI.TextureWrapT, (int)GLMapping.ToGl(desc.AddressV));
        Gl.SamplerParameter(s, SamplerParameterI.TextureWrapR, (int)GLMapping.ToGl(desc.AddressW));
        if (desc.MinFilter == FilterMode.Anisotropic && desc.MaxAnisotropy > 1
            && (HasExtension("GL_EXT_texture_filter_anisotropic")
             || HasExtension("GL_ARB_texture_filter_anisotropic")))
        {
            const int GL_TEXTURE_MAX_ANISOTROPY = 0x84FE;
            Gl.SamplerParameter(s, (SamplerParameterF)GL_TEXTURE_MAX_ANISOTROPY, (float)desc.MaxAnisotropy);
        }

        uint id = AllocHandle();
        Samplers[id] = new GLSamplerRes { Handle = s };
        return new SamplerHandle(id);
    }

    public void Destroy(SamplerHandle h)
    {
        if (Samplers.Remove(h.Id, out var s)) Gl.DeleteSampler(s.Handle);
    }

    // ====================================================== Command stream

    public ICommandList BeginCommandList()
    {
        // We do NOT make any context current here — every swapchain owns
        // its own HDC, and BeginCommandList doesn't know which swapchain
        // the caller's BeginPass will target. The right binding happens in
        // GLCommandList.BeginPass and in Present, both of which receive a
        // SwapchainHandle and can pick the matching HDC. If we used a
        // single shared HDC here, the most-recently-created swapchain
        // (typically an item-preview panel) would steal the current target
        // and the main Panel3D viewport would silently render to a hidden
        // surface — exactly the "frozen viewport after level load" symptom
        // the user reported.
        return new GLCommandList(this);
    }

    public void Submit(ICommandList commandList)
    {
        // No-op: commands were already issued by the time Submit is called.
        _ = (GLCommandList)commandList;
    }
}
