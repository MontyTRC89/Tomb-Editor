using System;
using System.Runtime.InteropServices;
using TombLib.RenderingV2.Rhi;
using RhiFormat = TombLib.RenderingV2.Rhi.Format;

namespace TombLib.RenderingV2.Backends.OpenGL;

// Swapchain management for the OpenGL backend. A "swapchain" here is just a
// per-HWND WGL surface — OpenGL renders into the WGL context's default
// framebuffer (FBO 0) and Present is wglSwapBuffers.
public unsafe sealed partial class GLDevice
{
    public SwapchainHandle CreateSwapchain(in SwapchainDesc desc)
    {
        // The bootstrap context was made current on a hidden window's DC.
        // Re-bind it to the real Panel3D HWND's DC — the GL context itself
        // stays, only the target surface changes.
        IntPtr realHdc = GetDC(desc.WindowHandle);
        if (realHdc == IntPtr.Zero)
            throw new InvalidOperationException("GetDC failed for swapchain HWND");

        // Set a matching pixel format on the real DC. An HDC's pixel format
        // can't be changed twice, so this requires the Panel3D HWND to be
        // freshly created — which it is (the V2 path bypasses legacy SwapChain).
        var pixelFormatDesc = new PIXELFORMATDESCRIPTOR
        {
            nSize        = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion     = 1,
            dwFlags      = 0x4 | 0x20 | 0x1, // DRAW_TO_WINDOW | SUPPORT_OPENGL | DOUBLEBUFFER
            iPixelType   = 0,               // PFD_TYPE_RGBA
            cColorBits   = 32,
            cDepthBits   = (byte)(desc.DepthFormat == RhiFormat.D24_UNorm_S8_UInt ? 24 : 32),
            cStencilBits = (byte)(desc.DepthFormat == RhiFormat.D24_UNorm_S8_UInt ? 8  : 0),
            iLayerType   = 0,               // PFD_MAIN_PLANE
        };
        int pixelFormat = ChoosePixelFormat(realHdc, ref pixelFormatDesc);
        if (pixelFormat == 0)
            throw new InvalidOperationException("ChoosePixelFormat failed for swapchain HWND");
        if (!SetPixelFormat(realHdc, pixelFormat, ref pixelFormatDesc))
            throw new InvalidOperationException(
                "SetPixelFormat failed — was the Panel3D HWND already bound by another API?");

        // Make the context current on the real DC.
        if (!WglMakeCurrent(realHdc, Hglrc))
            throw new InvalidOperationException("wglMakeCurrent on real HWND failed");

        // Release the bootstrap DC — it's no longer needed.
        if (Hdc != IntPtr.Zero && Hwnd != IntPtr.Zero)
        {
            ReleaseDC(Hwnd, Hdc);
            Hdc = IntPtr.Zero;
        }

        // Track the real HWND / DC as the primary surface so Present knows
        // where to swap buffers.
        Hwnd = desc.WindowHandle;
        Hdc  = realHdc;

        // Optional swap interval (vsync) — applied via the WGL extension if present.
        SetSwapInterval(desc.VSync ? 1 : 0);

        var swapchain = new GLSwapchainRes
        {
            Hwnd        = desc.WindowHandle,
            Hdc         = realHdc,
            Width       = desc.Width,
            Height      = desc.Height,
            ColorFormat = desc.ColorFormat,
            DepthFormat = desc.DepthFormat,
            VSync       = desc.VSync,
        };
        uint id = AllocHandle();
        Swapchains[id] = swapchain;
        return new SwapchainHandle(id);
    }

    public void ResizeSwapchain(SwapchainHandle handle, int width, int height)
    {
        // GL needs no explicit resize of the default framebuffer — it follows
        // the window automatically. Just record the new size for the viewport
        // calculation in BeginPass.
        var swapchain = Swapchains[handle.Id];
        swapchain.Width  = width;
        swapchain.Height = height;
    }

    public void Destroy(SwapchainHandle handle)
    {
        if (!Swapchains.Remove(handle.Id, out var swapchain)) return;
        if (swapchain.Hdc != IntPtr.Zero && swapchain.Hwnd != IntPtr.Zero)
            ReleaseDC(swapchain.Hwnd, swapchain.Hdc);
    }

    public void Present(SwapchainHandle handle)
    {
        var swapchain = Swapchains[handle.Id];
        SwapBuffers(swapchain.Hdc);
    }

    private void SetSwapInterval(int interval)
    {
        IntPtr swapIntervalAddr = WglGetProcAddress("wglSwapIntervalEXT");
        if (swapIntervalAddr == IntPtr.Zero) return;
        var swapInterval = Marshal.GetDelegateForFunctionPointer<WglSwapIntervalEXTDelegate>(swapIntervalAddr);
        swapInterval(interval);
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate bool WglSwapIntervalEXTDelegate(int interval);
}
