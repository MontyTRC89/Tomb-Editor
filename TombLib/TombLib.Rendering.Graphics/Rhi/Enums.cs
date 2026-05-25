using System;

namespace TombLib.Rendering.Graphics.Rhi;

// Pixel / vertex formats. Names follow the DXGI/Vulkan convention so a
// reader who knows either API can map them mentally. Subset is small on
// purpose; widen only when a pass actually needs a new format.
public enum Format : byte
{
    Unknown,

    // Color attachments
    R8G8B8A8_UNorm,
    R8G8B8A8_UNorm_SRgb,
    B8G8R8A8_UNorm,
    R16G16B16A16_Float,

    // Depth / stencil
    D24_UNorm_S8_UInt,
    D32_Float,

    // Vertex / index
    R32_UInt,
    R32G32_Float,
    R32G32B32_Float,
    R32G32B32A32_Float,
    R32_Float,
    R16G16B16A16_UNorm,
    R8G8B8A8_UInt,
    R16G16_UNorm,    // packed atlas UV (0..1 with 1/65535 precision)
    R16G16_Float,    // packed grid UV (half-precision)
}

public enum BufferUsage : byte
{
    // Vertex / index buffers backed by GPU-only memory. Filled at create time.
    Immutable,
    // Constant / uniform buffer updated frequently from CPU.
    DynamicUniform,
    // Vertex or index buffer the CPU writes once per frame (sprite/text batchers).
    DynamicVertex,
    // Read-only shader-visible structured buffer (SSBO on GL/VK, StructuredBuffer SRV on DX11).
    StructuredReadOnly,
}

[Flags]
public enum BufferBindFlags : byte
{
    None         = 0,
    Vertex       = 1 << 0,
    Index        = 1 << 1,
    Constant     = 1 << 2,
    Structured   = 1 << 3,
}

public enum IndexFormat : byte
{
    U16,
    U32,
}

public enum TextureKind : byte
{
    Texture2D,
    Texture2DArray,
    TextureCube,
}

[Flags]
public enum TextureBindFlags : byte
{
    None             = 0,
    ShaderResource   = 1 << 0, // sampled in shaders
    RenderTarget     = 1 << 1, // can be a color attachment
    DepthStencil     = 1 << 2, // can be a depth attachment
}

public enum PrimitiveTopology : byte
{
    TriangleList,
    TriangleStrip,
    LineList,
    LineStrip,
    PointList,
}

public enum CullMode : byte { None, Back, Front }
public enum FillMode : byte { Solid, Wireframe }

public enum CompareOp : byte
{
    Never, Less, Equal, LessEqual,
    Greater, NotEqual, GreaterEqual, Always
}

public enum BlendFactor : byte
{
    Zero, One,
    SrcColor, OneMinusSrcColor,
    DstColor, OneMinusDstColor,
    SrcAlpha, OneMinusSrcAlpha,
    DstAlpha, OneMinusDstAlpha,
}

public enum BlendOp : byte { Add, Subtract, ReverseSubtract, Min, Max }

public enum FilterMode : byte { Nearest, Linear, Anisotropic }
public enum AddressMode : byte { Wrap, Mirror, Clamp, Border }

public enum ShaderStage : byte
{
    Vertex,
    Fragment,
}

public enum LoadOp : byte  { Load, Clear, DontCare }
public enum StoreOp : byte { Store, DontCare }

[Flags]
public enum ColorWriteMask : byte
{
    None  = 0,
    R     = 1 << 0,
    G     = 1 << 1,
    B     = 1 << 2,
    A     = 1 << 3,
    All   = R | G | B | A,
}
