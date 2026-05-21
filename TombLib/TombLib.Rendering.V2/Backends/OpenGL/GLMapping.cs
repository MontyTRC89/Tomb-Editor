using System;
using Silk.NET.OpenGL;
using RhiFormat = TombLib.RenderingV2.Rhi.Format;
using RhiCullMode = TombLib.RenderingV2.Rhi.CullMode;
using RhiFillMode = TombLib.RenderingV2.Rhi.FillMode;
using RhiCompareOp = TombLib.RenderingV2.Rhi.CompareOp;
using RhiBlendFactor = TombLib.RenderingV2.Rhi.BlendFactor;
using RhiBlendOp = TombLib.RenderingV2.Rhi.BlendOp;
using RhiFilterMode = TombLib.RenderingV2.Rhi.FilterMode;
using RhiAddressMode = TombLib.RenderingV2.Rhi.AddressMode;
using RhiPrimitiveTopology = TombLib.RenderingV2.Rhi.PrimitiveTopology;
using RhiIndexFormat = TombLib.RenderingV2.Rhi.IndexFormat;

namespace TombLib.RenderingV2.Backends.OpenGL;

internal static class GLMapping
{
    // Format → (internal, format, type). InternalFormat is the texture's
    // storage format; Format + Type describe the pixel data being uploaded.
    public static (InternalFormat ifmt, PixelFormat fmt, PixelType type) ToGl(RhiFormat f) => f switch
    {
        RhiFormat.R8G8B8A8_UNorm      => (InternalFormat.Rgba8,     PixelFormat.Rgba, PixelType.UnsignedByte),
        RhiFormat.R8G8B8A8_UNorm_SRgb => (InternalFormat.Srgb8Alpha8, PixelFormat.Rgba, PixelType.UnsignedByte),
        RhiFormat.B8G8R8A8_UNorm      => (InternalFormat.Rgba8,     PixelFormat.Bgra, PixelType.UnsignedByte),
        RhiFormat.R16G16B16A16_Float  => (InternalFormat.Rgba16f,   PixelFormat.Rgba, PixelType.HalfFloat),
        RhiFormat.D24_UNorm_S8_UInt   => (InternalFormat.Depth24Stencil8, PixelFormat.DepthStencil, PixelType.UnsignedInt248),
        RhiFormat.D32_Float           => (InternalFormat.DepthComponent32f, PixelFormat.DepthComponent, PixelType.Float),
        RhiFormat.R32_UInt            => (InternalFormat.R32ui,     PixelFormat.RedInteger, PixelType.UnsignedInt),
        RhiFormat.R32G32_Float        => (InternalFormat.RG32f,     PixelFormat.RG,   PixelType.Float),
        RhiFormat.R32G32B32_Float     => (InternalFormat.Rgb32f,    PixelFormat.Rgb,  PixelType.Float),
        RhiFormat.R32G32B32A32_Float  => (InternalFormat.Rgba32f,   PixelFormat.Rgba, PixelType.Float),
        RhiFormat.R32_Float           => (InternalFormat.R32f,      PixelFormat.Red,  PixelType.Float),
        RhiFormat.R16G16B16A16_UNorm  => (InternalFormat.Rgba16,    PixelFormat.Rgba, PixelType.UnsignedShort),
        RhiFormat.R8G8B8A8_UInt       => (InternalFormat.Rgba8ui,   PixelFormat.RgbaInteger, PixelType.UnsignedByte),
        RhiFormat.R16G16_UNorm        => (InternalFormat.RG16,      PixelFormat.RG,   PixelType.UnsignedShort),
        RhiFormat.R16G16_Float        => (InternalFormat.RG16f,     PixelFormat.RG,   PixelType.HalfFloat),
        _ => throw new ArgumentOutOfRangeException(nameof(f), f, "Unmapped format"),
    };

    // Vertex attribute format → (gl type, num components, normalized).
    public static (VertexAttribPointerType type, int components, bool normalized, bool isInteger) AttribFormat(RhiFormat f) => f switch
    {
        RhiFormat.R32G32B32_Float    => (VertexAttribPointerType.Float, 3, false, false),
        RhiFormat.R32G32_Float       => (VertexAttribPointerType.Float, 2, false, false),
        RhiFormat.R32G32B32A32_Float => (VertexAttribPointerType.Float, 4, false, false),
        RhiFormat.R32_Float          => (VertexAttribPointerType.Float, 1, false, false),
        RhiFormat.R8G8B8A8_UNorm     => (VertexAttribPointerType.UnsignedByte, 4, true, false),
        RhiFormat.R8G8B8A8_UInt      => (VertexAttribPointerType.UnsignedByte, 4, false, true),
        RhiFormat.R16G16_UNorm       => (VertexAttribPointerType.UnsignedShort, 2, true, false),
        RhiFormat.R16G16_Float       => (VertexAttribPointerType.HalfFloat, 2, false, false),
        RhiFormat.R16G16B16A16_UNorm => (VertexAttribPointerType.UnsignedShort, 4, true, false),
        RhiFormat.R16G16B16A16_Float => (VertexAttribPointerType.HalfFloat, 4, false, false),
        RhiFormat.R32_UInt           => (VertexAttribPointerType.UnsignedInt, 1, false, true),
        _ => throw new ArgumentOutOfRangeException(nameof(f), f, "Unmapped vertex format"),
    };

    public static PrimitiveType ToGl(RhiPrimitiveTopology t) => t switch
    {
        RhiPrimitiveTopology.TriangleList  => PrimitiveType.Triangles,
        RhiPrimitiveTopology.TriangleStrip => PrimitiveType.TriangleStrip,
        RhiPrimitiveTopology.LineList      => PrimitiveType.Lines,
        RhiPrimitiveTopology.LineStrip     => PrimitiveType.LineStrip,
        RhiPrimitiveTopology.PointList     => PrimitiveType.Points,
        _ => PrimitiveType.Triangles,
    };

    public static DepthFunction ToGl(RhiCompareOp c) => c switch
    {
        RhiCompareOp.Never        => DepthFunction.Never,
        RhiCompareOp.Less         => DepthFunction.Less,
        RhiCompareOp.Equal        => DepthFunction.Equal,
        RhiCompareOp.LessEqual    => DepthFunction.Lequal,
        RhiCompareOp.Greater      => DepthFunction.Greater,
        RhiCompareOp.NotEqual     => DepthFunction.Notequal,
        RhiCompareOp.GreaterEqual => DepthFunction.Gequal,
        RhiCompareOp.Always       => DepthFunction.Always,
        _ => DepthFunction.Lequal,
    };

    public static BlendingFactor ToGl(RhiBlendFactor f) => f switch
    {
        RhiBlendFactor.Zero             => BlendingFactor.Zero,
        RhiBlendFactor.One              => BlendingFactor.One,
        RhiBlendFactor.SrcColor         => BlendingFactor.SrcColor,
        RhiBlendFactor.OneMinusSrcColor => BlendingFactor.OneMinusSrcColor,
        RhiBlendFactor.DstColor         => BlendingFactor.DstColor,
        RhiBlendFactor.OneMinusDstColor => BlendingFactor.OneMinusDstColor,
        RhiBlendFactor.SrcAlpha         => BlendingFactor.SrcAlpha,
        RhiBlendFactor.OneMinusSrcAlpha => BlendingFactor.OneMinusSrcAlpha,
        RhiBlendFactor.DstAlpha         => BlendingFactor.DstAlpha,
        RhiBlendFactor.OneMinusDstAlpha => BlendingFactor.OneMinusDstAlpha,
        _ => BlendingFactor.Zero,
    };

    public static BlendEquationModeEXT ToGl(RhiBlendOp op) => op switch
    {
        RhiBlendOp.Add             => BlendEquationModeEXT.FuncAdd,
        RhiBlendOp.Subtract        => BlendEquationModeEXT.FuncSubtract,
        RhiBlendOp.ReverseSubtract => BlendEquationModeEXT.FuncReverseSubtract,
        RhiBlendOp.Min             => BlendEquationModeEXT.Min,
        RhiBlendOp.Max             => BlendEquationModeEXT.Max,
        _ => BlendEquationModeEXT.FuncAdd,
    };

    public static TextureWrapMode ToGl(RhiAddressMode m) => m switch
    {
        RhiAddressMode.Wrap   => TextureWrapMode.Repeat,
        RhiAddressMode.Mirror => TextureWrapMode.MirroredRepeat,
        RhiAddressMode.Clamp  => TextureWrapMode.ClampToEdge,
        RhiAddressMode.Border => TextureWrapMode.ClampToBorder,
        _ => TextureWrapMode.Repeat,
    };

    public static TextureMinFilter ToMinFilter(RhiFilterMode minFilter, RhiFilterMode mipFilter, bool hasMips)
    {
        if (minFilter == RhiFilterMode.Nearest)
            return hasMips
                   ? (mipFilter == RhiFilterMode.Linear ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.NearestMipmapNearest)
                   : TextureMinFilter.Nearest;
        // Linear or Anisotropic
        return hasMips
               ? (mipFilter == RhiFilterMode.Nearest ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.LinearMipmapLinear)
               : TextureMinFilter.Linear;
    }

    public static TextureMagFilter ToMagFilter(RhiFilterMode magFilter)
        => magFilter == RhiFilterMode.Nearest ? TextureMagFilter.Nearest : TextureMagFilter.Linear;

    public static DrawElementsType ToGl(RhiIndexFormat f)
        => f == RhiIndexFormat.U16 ? DrawElementsType.UnsignedShort : DrawElementsType.UnsignedInt;
}
