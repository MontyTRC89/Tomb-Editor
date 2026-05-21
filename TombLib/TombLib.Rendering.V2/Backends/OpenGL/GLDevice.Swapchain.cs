using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using TombLib.RenderingV2.Rhi;
using RhiFormat = TombLib.RenderingV2.Rhi.Format;

namespace TombLib.RenderingV2.Backends.OpenGL;

public unsafe sealed partial class GLDevice
{
    public SwapchainHandle CreateSwapchain(in SwapchainDesc desc)
    {
        // The bootstrap context was made current on a hidden window's DC.
        // Now we re-bind it to the real Panel3D HWND's DC. The GL context
        // itself stays; we just swap the target surface.
        IntPtr realHdc = GetDC(desc.WindowHandle);
        if (realHdc == IntPtr.Zero)
            throw new InvalidOperationException("GetDC failed for swapchain HWND");

        // Set a matching pixel format on the real DC. We can't change the
        // pixel format of an HDC twice, so this requires the Panel3D HWND
        // to be created fresh — which it is (V2 path bypasses legacy SwapChain).
        var pfd = new PIXELFORMATDESCRIPTOR
        {
            nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            dwFlags  = 0x4 | 0x20 | 0x1, // DRAW_TO_WINDOW | SUPPORT_OPENGL | DOUBLEBUFFER
            iPixelType = 0,              // PFD_TYPE_RGBA
            cColorBits = 32,
            cDepthBits = (byte)(desc.DepthFormat == RhiFormat.D24_UNorm_S8_UInt ? 24 : 32),
            cStencilBits = (byte)(desc.DepthFormat == RhiFormat.D24_UNorm_S8_UInt ? 8 : 0),
            iLayerType = 0,              // PFD_MAIN_PLANE
        };
        int fmt = ChoosePixelFormat(realHdc, ref pfd);
        if (fmt == 0) throw new InvalidOperationException("ChoosePixelFormat failed for swapchain HWND");
        if (!SetPixelFormat(realHdc, fmt, ref pfd))
            throw new InvalidOperationException(
                "SetPixelFormat failed — was the Panel3D HWND already bound by another API?");

        // Make context current on the real DC.
        if (!WglMakeCurrent(realHdc, Hglrc))
            throw new InvalidOperationException("wglMakeCurrent on real HWND failed");

        // Release the bootstrap DC; we no longer need it.
        if (Hdc != IntPtr.Zero && Hwnd != IntPtr.Zero)
        {
            ReleaseDC(Hwnd, Hdc);
            Hdc = IntPtr.Zero;
        }

        // Track the real HWND/DC as the "primary surface" so Present knows
        // where to swap buffers.
        Hwnd = desc.WindowHandle;
        Hdc  = realHdc;

        // Optional swap-interval (vsync). Loaded via WGL ext if present.
        SetSwapInterval(desc.VSync ? 1 : 0);

        var sc = new GLSwapchainRes
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
        Swapchains[id] = sc;
        return new SwapchainHandle(id);
    }

    public void ResizeSwapchain(SwapchainHandle handle, int width, int height)
    {
        var sc = Swapchains[handle.Id];
        sc.Width  = width;
        sc.Height = height;
        // GL doesn't need an explicit resize on the default framebuffer —
        // it follows the window automatically. Just record the new size for
        // the viewport calculation in BeginPass.
    }

    public void Destroy(SwapchainHandle h)
    {
        if (!Swapchains.Remove(h.Id, out var sc)) return;
        if (sc.Hdc != IntPtr.Zero && sc.Hwnd != IntPtr.Zero)
            ReleaseDC(sc.Hwnd, sc.Hdc);
    }

    public void Present(SwapchainHandle handle)
    {
        var sc = Swapchains[handle.Id];
        SwapBuffers(sc.Hdc);
    }

    private void SetSwapInterval(int interval)
    {
        IntPtr p = WglGetProcAddress("wglSwapIntervalEXT");
        if (p == IntPtr.Zero) return;
        var del = Marshal.GetDelegateForFunctionPointer<WglSwapIntervalEXTDelegate>(p);
        del(interval);
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate bool WglSwapIntervalEXTDelegate(int interval);
}
