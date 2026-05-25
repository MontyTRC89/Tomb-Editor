using Silk.NET.OpenGL;
using RhiFormat = TombLib.Rendering.Graphics.Rhi.Format;
using RhiBufferUsage = TombLib.Rendering.Graphics.Rhi.BufferUsage;
using RhiBufferBindFlags = TombLib.Rendering.Graphics.Rhi.BufferBindFlags;
using RhiTextureBindFlags = TombLib.Rendering.Graphics.Rhi.TextureBindFlags;
using RhiPrimitiveTopology = TombLib.Rendering.Graphics.Rhi.PrimitiveTopology;

namespace TombLib.Rendering.Graphics.Backends.OpenGL;

internal sealed class GLBufferRes
{
    public uint              Handle;
    public int               Size;
    public unsafe void*      Mapped;          // non-null for persistently-mapped dynamic buffers
    public RhiBufferUsage    Usage;
    public RhiBufferBindFlags BindFlags;
}

internal sealed class GLTextureRes
{
    public uint              Handle;
    public int               Width, Height, MipLevels;
    public RhiFormat         Format;
    public RhiTextureBindFlags BindFlags;
    public bool              IsDepth;          // for framebuffer attachment routing
    public uint              FramebufferHandle; // non-zero only if used as a render target (lazy)
}

internal sealed class GLSamplerRes
{
    public uint Handle;
}

internal sealed class GLPipelineRes
{
    public uint              Program;          // linked GL program
    public uint              Vao;              // VAO with all vertex attribute pointers configured
    public int[]             VertexStrides = System.Array.Empty<int>();
    public RhiPrimitiveTopology Topology;
    public PrimitiveType     GlTopology;
    public bool              DepthTest, DepthWrite;
    public DepthFunction     DepthFunc;
    public CullModeMaterial  CullMode;          // None / Back / Front
    public bool              CullEnable;
    public FrontFaceDirection FrontFace;
    public PolygonMode       PolygonMode;
    public bool              BlendEnable;
    public BlendingFactor    BlendSrcRgb, BlendDstRgb, BlendSrcA, BlendDstA;
    public BlendEquationModeEXT BlendOpRgb, BlendOpA;
    public bool              ScissorEnable;
    public System.Collections.Generic.Dictionary<int, int> SamplerLocations = new(); // textureSlot -> uniform location
}

internal sealed class GLSwapchainRes
{
    public System.IntPtr     Hwnd;
    public System.IntPtr     Hdc;              // device context for SwapBuffers
    public int               Width, Height;
    public RhiFormat         ColorFormat, DepthFormat;
    public bool              VSync;
    // OpenGL renders directly into the system framebuffer (FBO 0), so we
    // don't need to manage colour / depth views explicitly -- the WGL
    // context's default framebuffer is implicit.
}

// We don't have a "CullMode" enum from Silk.NET.OpenGL that matches what we
// need; reuse TombLib's RhiCullMode mapping via this helper enum.
internal enum CullModeMaterial { None, Back, Front }
