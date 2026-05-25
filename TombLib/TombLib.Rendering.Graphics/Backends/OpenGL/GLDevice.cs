using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;
using TombLib.Rendering.Graphics.Rhi;
using GLEnum = Silk.NET.OpenGL.GLEnum;
using RhiFormat   = TombLib.Rendering.Graphics.Rhi.Format;
using BufferDesc  = TombLib.Rendering.Graphics.Rhi.BufferDesc;
using TextureDesc = TombLib.Rendering.Graphics.Rhi.TextureDesc;
using SamplerDesc = TombLib.Rendering.Graphics.Rhi.SamplerDesc;

namespace TombLib.Rendering.Graphics.Backends.OpenGL;

/// <summary>
/// OpenGL 4.3 Core Profile backend for <see cref="IRhiDevice"/>. Uses WGL on
/// Windows to create the GL context against an HWND's DC, and Silk.NET.OpenGL
/// as the GL function loader.
///
/// <para>Design choices that mirror the DX11 / Vulkan backends:
///   - Single-threaded. All calls must come from the rendering thread the
///     context was made current on.
///   - Resource pools (buffers / textures / samplers / pipelines / swapchains)
///     keyed by uint handles so the RHI surface stays opaque.
///   - One pipeline = one linked GL program + one VAO that captures the vertex
///     attribute layout. SetPipeline becomes glUseProgram + glBindVertexArray
///     + apply the fixed-function state.
///   - The "swapchain" is really just the WGL context's default framebuffer
///     (FBO 0); Present = wglSwapBuffers.</para>
///
/// <para>Split across partial files by concern:
///   GLDevice.cs           -- device, resources (buffers / textures / samplers).
///   GLDevice.Pipeline.cs  -- pipeline programs and VAOs.
///   GLDevice.Swapchain.cs -- per-HWND WGL surfaces.</para>
/// </summary>
public unsafe sealed partial class GLDevice : IRhiDevice
{
    internal GL     Gl = null!;
    internal IntPtr Hwnd;
    internal IntPtr Hdc;
    internal IntPtr Hglrc;
    private  WglLoader? _loader;

    // Resource pools (handle id -> resource).
    private uint _nextHandle = 1;
    internal readonly Dictionary<uint, GLBufferRes>    Buffers    = new();
    internal readonly Dictionary<uint, GLTextureRes>   Textures   = new();
    internal readonly Dictionary<uint, GLSamplerRes>   Samplers   = new();
    internal readonly Dictionary<uint, GLPipelineRes>  Pipelines  = new();
    internal readonly Dictionary<uint, GLSwapchainRes> Swapchains = new();

    public RhiCapabilities Capabilities { get; }

    public GLDevice()
    {
        // A GL context needs a Win32 window. The editor's Panel3D HWND arrives
        // later via CreateSwapchain -- at device-creation time there's none yet.
        // Trick: bootstrap a context on a throwaway "dummy" window, then bind
        // to the real HWND when CreateSwapchain is first called.
        InitDummyContext();

        // Cache device capabilities.
        Gl.GetInteger(GetPName.MaxTextureSize, out int maxTextureSize);
        Gl.GetInteger(GetPName.MaxArrayTextureLayers, out int maxArrayLayers);
        bool anisotropySupported = HasExtension("GL_EXT_texture_filter_anisotropic")
                                || HasExtension("GL_ARB_texture_filter_anisotropic");
        Capabilities = new RhiCapabilities(
            backend:        RhiBackendKind.OpenGL,
            instanced:      true,
            structured:     true,
            nativePush:     false,
            anisotropy:     anisotropySupported,
            debugMarkers:   true,
            maxTexSize:     maxTextureSize,
            maxArrayLayers: maxArrayLayers);
    }

    internal uint AllocHandle() => _nextHandle++;

    public void Dispose()
    {
        // Swapchains only hold an HDC / HWND borrowed from Panel3D -- nothing
        // GL-side to free for them.
        foreach (var pipeline in Pipelines.Values)
        {
            if (pipeline.Vao     != 0) Gl.DeleteVertexArray(pipeline.Vao);
            if (pipeline.Program != 0) Gl.DeleteProgram(pipeline.Program);
        }
        foreach (var sampler in Samplers.Values)
            if (sampler.Handle != 0) Gl.DeleteSampler(sampler.Handle);
        foreach (var texture in Textures.Values)
        {
            if (texture.FramebufferHandle != 0) Gl.DeleteFramebuffer(texture.FramebufferHandle);
            if (texture.Handle != 0)            Gl.DeleteTexture(texture.Handle);
        }
        foreach (var buffer in Buffers.Values)
        {
            if (buffer.Mapped != null) Gl.UnmapNamedBuffer(buffer.Handle);
            if (buffer.Handle != 0)    Gl.DeleteBuffer(buffer.Handle);
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
        Gl.GetInteger(GetPName.NumExtensions, out int extensionCount);
        for (uint i = 0; i < (uint)extensionCount; i++)
        {
            string extension = Gl.GetStringS(StringName.Extensions, i);
            if (extension == name)
                return true;
        }
        return false;
    }

    // ===================================================== Context bootstrap

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
        // 1. Create a hidden Win32 message-only window so a DC can be pinned
        //    to it for the bootstrap context.
        Hwnd = CreateMessageWindow();
        Hdc  = GetDC(Hwnd);
        if (Hdc == IntPtr.Zero)
            throw new InvalidOperationException("GetDC failed for bootstrap window");

        var pixelFormatDesc = new PIXELFORMATDESCRIPTOR
        {
            nSize        = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion     = 1,
            dwFlags      = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
            iPixelType   = PFD_TYPE_RGBA,
            cColorBits   = 32,
            cDepthBits   = 24,
            cStencilBits = 8,
            iLayerType   = PFD_MAIN_PLANE,
        };
        int pixelFormat = ChoosePixelFormat(Hdc, ref pixelFormatDesc);
        if (pixelFormat == 0)
            throw new InvalidOperationException("ChoosePixelFormat failed");
        if (!SetPixelFormat(Hdc, pixelFormat, ref pixelFormatDesc))
            throw new InvalidOperationException("SetPixelFormat failed");

        // 2. Legacy context (any GL version) just to bootstrap.
        IntPtr legacyContext = WglCreateContext(Hdc);
        if (legacyContext == IntPtr.Zero)
            throw new InvalidOperationException("wglCreateContext failed");
        if (!WglMakeCurrent(Hdc, legacyContext))
            throw new InvalidOperationException("wglMakeCurrent on legacy context failed");

        // 3. Try to upgrade to a 4.3 core context via wglCreateContextAttribsARB.
        IntPtr createContextAttribsAddr = WglGetProcAddress("wglCreateContextAttribsARB");
        if (createContextAttribsAddr != IntPtr.Zero)
        {
            var createContextAttribs =
                Marshal.GetDelegateForFunctionPointer<WglCreateContextAttribsARB>(createContextAttribsAddr);
            int[] contextAttribs =
            {
                WGL_CONTEXT_MAJOR_VERSION_ARB, 4,
                WGL_CONTEXT_MINOR_VERSION_ARB, 3,
                WGL_CONTEXT_PROFILE_MASK_ARB,  WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
                // Debug bit when the env var asks for it -- costs ~5% on most drivers.
                WGL_CONTEXT_FLAGS_ARB,
                Environment.GetEnvironmentVariable("TOMBEDITOR_GL_DEBUG") == "1" ? WGL_CONTEXT_DEBUG_BIT_ARB : 0,
                0,
            };
            IntPtr coreContext;
            fixed (int* pAttribs = contextAttribs)
                coreContext = createContextAttribs(Hdc, IntPtr.Zero, (IntPtr)pAttribs);

            if (coreContext != IntPtr.Zero)
            {
                WglMakeCurrent(Hdc, coreContext);
                WglDeleteContext(legacyContext);
                Hglrc = coreContext;
            }
            else
            {
                Hglrc = legacyContext; // fall back (the version check below will catch it)
            }
        }
        else
        {
            Hglrc = legacyContext;
        }

        // 4. Load the GL function table via Silk.NET. The loader callback asks
        //    WGL (or the system loader) for each function pointer.
        _loader = new WglLoader();
        Gl = GL.GetApi(_loader);

        // Confirm we got the version we asked for; older drivers may have
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
        if (severity == GLEnum.DebugSeverityNotification)
            return;
        string text = Marshal.PtrToStringAnsi(message, length) ?? "(no msg)";
        try
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TombEditorGL.log");
            System.IO.File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss.fff}] GL [{severity}] {text}\n");
        }
        catch { /* never fail because of logging */ }
    }

    // -------------------- Win32 P/Invoke for the WGL bootstrap --------------------

    [StructLayout(LayoutKind.Sequential)]
    private struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize, nVersion;
        public uint   dwFlags;
        public byte   iPixelType, cColorBits, cRedBits, cRedShift, cGreenBits, cGreenShift,
                      cBlueBits, cBlueShift, cAlphaBits, cAlphaShift,
                      cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits,
                      cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
        public uint   dwLayerMask, dwVisibleMask, dwDamageMask;
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
    /// <summary>Exposed for GLCommandList.BeginPass -- the same WGL call.</summary>
    internal static bool WglMakeCurrentExt(IntPtr hdc, IntPtr hglrc) => WglMakeCurrent(hdc, hglrc);
    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress")]
    private static extern IntPtr WglGetProcAddress(string name);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandleW(string? lpModuleName);
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string name);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr WglCreateContextAttribsARB(IntPtr hdc, IntPtr hShareContext, IntPtr attribs);

    private static IntPtr s_messageWindow;

    private static IntPtr CreateMessageWindow()
    {
        if (s_messageWindow != IntPtr.Zero)
            return s_messageWindow;
        const uint WS_POPUP = 0x80000000;
        // Reuse a built-in Win32 class -- "STATIC" is always registered. This
        // window is never made visible.
        s_messageWindow = CreateWindowExW(0, "STATIC", "TombEditorGL", WS_POPUP,
                                          0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, GetModuleHandleW(null), IntPtr.Zero);
        if (s_messageWindow == IntPtr.Zero)
            throw new InvalidOperationException("CreateWindowEx failed for GL bootstrap");
        return s_messageWindow;
    }

    /// <summary>
    /// Silk.NET INativeContext loader that funnels every GL function lookup
    /// through wglGetProcAddress + opengl32.dll GetProcAddress (the latter for
    /// the GL 1.1 entry points that wglGetProcAddress won't return).
    /// </summary>
    private sealed class WglLoader : INativeContext
    {
        private static readonly IntPtr _opengl32 = GetModuleHandleW("opengl32.dll");

        public nint GetProcAddress(string proc, int? slot = null)
        {
            IntPtr address = WglGetProcAddress(proc);
            if (address == IntPtr.Zero || address == (IntPtr)1 || address == (IntPtr)2 ||
                address == (IntPtr)3   || address == (IntPtr)(-1))
                address = GLDevice.GetProcAddress(_opengl32, proc);
            return address;
        }

        public bool TryGetProcAddress(string proc, out nint addr, int? slot = null)
        {
            addr = GetProcAddress(proc);
            return addr != IntPtr.Zero;
        }

        public void Dispose() { }
    }

    // ================================================================ Buffers

    public BufferHandle CreateBuffer(in BufferDesc desc, ReadOnlySpan<byte> initialData)
    {
        uint bufferObject;
        Gl.CreateBuffers(1, &bufferObject);

        // Always allocate with DynamicStorageBit so glNamedBufferSubData works
        // for any later UpdateBuffer call. Persistent mapping is deliberately
        // NOT used -- it required per-driver memory barriers to make CPU writes
        // visible to subsequent draws, and getting that wrong showed up as
        // ghost / flickering frames after editor state changes.
        // glNamedBufferSubData is enough for the editor's update rates.
        const BufferStorageMask storageFlags = BufferStorageMask.DynamicStorageBit;

        if (initialData.Length > 0)
        {
            fixed (byte* src = initialData)
                Gl.NamedBufferStorage(bufferObject, (nuint)desc.SizeBytes, src, storageFlags);
        }
        else
        {
            Gl.NamedBufferStorage(bufferObject, (nuint)desc.SizeBytes, null, storageFlags);
        }

        uint id = AllocHandle();
        Buffers[id] = new GLBufferRes
        {
            Handle    = bufferObject,
            Size      = desc.SizeBytes,
            Mapped    = null,
            Usage     = desc.Usage,
            BindFlags = desc.BindFlags,
        };
        return new BufferHandle(id);
    }

    public void Destroy(BufferHandle handle)
    {
        if (Buffers.Remove(handle.Id, out var buffer) && buffer.Handle != 0)
            Gl.DeleteBuffer(buffer.Handle);
    }

    // =============================================================== Textures

    public TextureHandle CreateTexture(in TextureDesc desc, ReadOnlySpan<byte> initialData)
    {
        var (internalFormat, pixelFormat, pixelType) = GLMapping.ToGl(desc.Format);
        int mipLevels = Math.Max(1, desc.MipLevels);

        uint texture;
        Gl.CreateTextures(TextureTarget.Texture2D, 1, &texture);
        Gl.TextureStorage2D(texture, (uint)mipLevels, (SizedInternalFormat)internalFormat,
                            (uint)desc.Width, (uint)desc.Height);

        if (initialData.Length > 0)
        {
            fixed (byte* src = initialData)
                Gl.TextureSubImage2D(texture, 0, 0, 0, (uint)desc.Width, (uint)desc.Height,
                                     pixelFormat, pixelType, src);
        }

        // Default texture parameters -- samplers override these per-binding,
        // but reasonable defaults make naked sampling work too.
        Gl.TextureParameter(texture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        Gl.TextureParameter(texture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        Gl.TextureParameter(texture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        Gl.TextureParameter(texture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        uint id = AllocHandle();
        Textures[id] = new GLTextureRes
        {
            Handle    = texture,
            Width     = desc.Width,
            Height    = desc.Height,
            MipLevels = mipLevels,
            Format    = desc.Format,
            BindFlags = desc.BindFlags,
            IsDepth   = pixelFormat is PixelFormat.DepthComponent or PixelFormat.DepthStencil,
        };
        return new TextureHandle(id);
    }

    public void UpdateTexture(TextureHandle handle, int subresource,
                              int x, int y, int width, int height,
                              int rowPitchBytes, ReadOnlySpan<byte> data)
    {
        var texture = Textures[handle.Id];
        var (_, pixelFormat, pixelType) = GLMapping.ToGl(texture.Format);

        // GL specifies the source row stride via GL_UNPACK_ROW_LENGTH (in pixels).
        int bytesPerPixel = BytesPerPixelOf(texture.Format);
        Gl.PixelStore(PixelStoreParameter.UnpackRowLength, rowPitchBytes / bytesPerPixel);
        try
        {
            fixed (byte* src = data)
                Gl.TextureSubImage2D(texture.Handle, subresource, x, y, (uint)width, (uint)height,
                                     pixelFormat, pixelType, src);
        }
        finally
        {
            Gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
        }
    }

    public byte[] ReadTexture(TextureHandle handle, int subresource = 0)
    {
        var texture = Textures[handle.Id];
        var (_, pixelFormat, pixelType) = GLMapping.ToGl(texture.Format);
        int bytesPerPixel = BytesPerPixelOf(texture.Format);
        int size = texture.Width * texture.Height * bytesPerPixel;

        byte[] result = new byte[size];
        fixed (byte* dst = result)
            Gl.GetTextureImage(texture.Handle, subresource, pixelFormat, pixelType, (uint)size, dst);
        return result;
    }

    public void Destroy(TextureHandle handle)
    {
        if (Textures.Remove(handle.Id, out var texture))
        {
            if (texture.FramebufferHandle != 0) Gl.DeleteFramebuffer(texture.FramebufferHandle);
            if (texture.Handle != 0)            Gl.DeleteTexture(texture.Handle);
        }
    }

    private static int BytesPerPixelOf(RhiFormat format) => format switch
    {
        RhiFormat.R8G8B8A8_UNorm or RhiFormat.R8G8B8A8_UNorm_SRgb or RhiFormat.B8G8R8A8_UNorm
            or RhiFormat.R8G8B8A8_UInt or RhiFormat.R32_UInt or RhiFormat.R32_Float
            or RhiFormat.R16G16_UNorm or RhiFormat.R16G16_Float
            or RhiFormat.D24_UNorm_S8_UInt or RhiFormat.D32_Float                                 => 4,
        RhiFormat.R16G16B16A16_Float or RhiFormat.R16G16B16A16_UNorm or RhiFormat.R32G32_Float     => 8,
        RhiFormat.R32G32B32_Float                                                                 => 12,
        RhiFormat.R32G32B32A32_Float                                                              => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "BytesPerPixel undefined"),
    };

    // =============================================================== Samplers

    public SamplerHandle CreateSampler(in SamplerDesc desc)
    {
        uint sampler;
        Gl.CreateSamplers(1, &sampler);

        // With a mip filter active, use the full LOD range; otherwise 0..0.
        bool hasMips = desc.MipFilter != FilterMode.Nearest || desc.MinFilter != FilterMode.Nearest;
        Gl.SamplerParameter(sampler, SamplerParameterI.TextureMinFilter,
            (int)GLMapping.ToMinFilter(desc.MinFilter, desc.MipFilter, hasMips));
        Gl.SamplerParameter(sampler, SamplerParameterI.TextureMagFilter,
            (int)GLMapping.ToMagFilter(desc.MagFilter));
        Gl.SamplerParameter(sampler, SamplerParameterI.TextureWrapS, (int)GLMapping.ToGl(desc.AddressU));
        Gl.SamplerParameter(sampler, SamplerParameterI.TextureWrapT, (int)GLMapping.ToGl(desc.AddressV));
        Gl.SamplerParameter(sampler, SamplerParameterI.TextureWrapR, (int)GLMapping.ToGl(desc.AddressW));

        if (desc.MinFilter == FilterMode.Anisotropic && desc.MaxAnisotropy > 1
            && (HasExtension("GL_EXT_texture_filter_anisotropic")
             || HasExtension("GL_ARB_texture_filter_anisotropic")))
        {
            const int GL_TEXTURE_MAX_ANISOTROPY = 0x84FE;
            Gl.SamplerParameter(sampler, (SamplerParameterF)GL_TEXTURE_MAX_ANISOTROPY, (float)desc.MaxAnisotropy);
        }

        uint id = AllocHandle();
        Samplers[id] = new GLSamplerRes { Handle = sampler };
        return new SamplerHandle(id);
    }

    public void Destroy(SamplerHandle handle)
    {
        if (Samplers.Remove(handle.Id, out var sampler))
            Gl.DeleteSampler(sampler.Handle);
    }

    // ========================================================= Command stream

    public ICommandList BeginCommandList()
    {
        // No context is made current here -- every swapchain owns its own HDC,
        // and BeginCommandList doesn't know which swapchain the caller's
        // BeginPass will target. The correct binding happens in
        // GLCommandList.BeginPass and in Present, both of which receive a
        // SwapchainHandle and pick the matching HDC. A single shared HDC here
        // would let the most-recently-created swapchain (typically an
        // item-preview panel) steal the current target and make the main
        // Panel3D viewport silently render to a hidden surface -- exactly the
        // "frozen viewport after level load" symptom that was reported.
        return new GLCommandList(this);
    }

    public void Submit(ICommandList commandList)
    {
        // No-op: commands were already issued by the time Submit is called.
        _ = (GLCommandList)commandList;
    }
}
