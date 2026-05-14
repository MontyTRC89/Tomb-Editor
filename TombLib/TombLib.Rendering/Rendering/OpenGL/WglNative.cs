using System;
using System.Runtime.InteropServices;

namespace TombLib.Rendering.OpenGL
{
    // P/Invoke declarations for WGL and GDI32 needed by the OpenGL backend.
    // Only the subset required for context creation and per-window rendering
    // is declared here — no need for wglChoosePixelFormatARB or extensions.
    internal static class WglNative
    {
        // ---- GDI32 / User32 pixel format + DC ---------------------------------

        [StructLayout(LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize;
            public ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits, cRedShift, cGreenBits, cGreenShift, cBlueBits, cBlueShift;
            public byte cAlphaBits, cAlphaShift;
            public byte cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public byte iLayerType;
            public byte bReserved;
            public uint dwLayerMask, dwVisibleMask, dwDamageMask;
        }

        public const uint PFD_DRAW_TO_WINDOW = 0x00000004;
        public const uint PFD_SUPPORT_OPENGL = 0x00000020;
        public const uint PFD_DOUBLEBUFFER   = 0x00000001;
        public const byte PFD_TYPE_RGBA      = 0;
        public const byte PFD_MAIN_PLANE     = 0;

        [DllImport("gdi32.dll")]  public static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR ppfd);
        [DllImport("gdi32.dll")]  public static extern bool SetPixelFormat(IntPtr hdc, int format, ref PIXELFORMATDESCRIPTOR ppfd);
        [DllImport("gdi32.dll")]  public static extern bool SwapBuffers(IntPtr hdc);
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        // ---- WGL context management -------------------------------------------

        [DllImport("opengl32.dll")] public static extern IntPtr wglCreateContext(IntPtr hdc);
        [DllImport("opengl32.dll")] public static extern bool wglDeleteContext(IntPtr hglrc);
        [DllImport("opengl32.dll")] public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);
        [DllImport("opengl32.dll")] public static extern IntPtr wglGetCurrentContext();
        [DllImport("opengl32.dll")] public static extern IntPtr wglGetProcAddress(string lpszProc);

        // ---- Dummy window for initial context ---------------------------------

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowExW(
            uint dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")] public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")] public static extern IntPtr GetModuleHandleW(string lpModuleName);
        [DllImport("kernel32.dll")] public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll")] public static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);

        // Helper: resolve an OpenGL extension function, falling back to opengl32.dll exports.
        public static IntPtr GetAnyGLProcAddress(string name)
        {
            IntPtr addr = wglGetProcAddress(name);
            if (addr != IntPtr.Zero) return addr;
            IntPtr gl32 = LoadLibraryW("opengl32.dll");
            return GetProcAddress(gl32, name);
        }
    }
}
