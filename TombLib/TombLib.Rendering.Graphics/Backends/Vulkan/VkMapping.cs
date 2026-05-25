using System;
using Silk.NET.Vulkan;
using RhiFormat = TombLib.Rendering.Graphics.Rhi.Format;
using RhiCullMode = TombLib.Rendering.Graphics.Rhi.CullMode;
using RhiFillMode = TombLib.Rendering.Graphics.Rhi.FillMode;
using RhiCompareOp = TombLib.Rendering.Graphics.Rhi.CompareOp;
using RhiBlendFactor = TombLib.Rendering.Graphics.Rhi.BlendFactor;
using RhiBlendOp = TombLib.Rendering.Graphics.Rhi.BlendOp;
using RhiAddressMode = TombLib.Rendering.Graphics.Rhi.AddressMode;
using RhiPrimitiveTopology = TombLib.Rendering.Graphics.Rhi.PrimitiveTopology;
using VkFormat = Silk.NET.Vulkan.Format;
using VkBlendFactor = Silk.NET.Vulkan.BlendFactor;
using VkBlendOp = Silk.NET.Vulkan.BlendOp;

namespace TombLib.Rendering.Graphics.Backends.Vulkan;

/// <summary>
/// RHI -> Vulkan enum mappings. Mirrors the corresponding Dx11Mapping helpers.
/// All conversions are one-to-one and total: every RHI enum value gets a
/// concrete Vulkan equivalent.
/// </summary>
internal static class VkMapping
{
    public static VkFormat ToVk(RhiFormat format) => format switch
    {
        RhiFormat.Unknown             => VkFormat.Undefined,
        RhiFormat.R8G8B8A8_UNorm      => VkFormat.R8G8B8A8Unorm,
        RhiFormat.R8G8B8A8_UNorm_SRgb => VkFormat.R8G8B8A8Srgb,
        RhiFormat.B8G8R8A8_UNorm      => VkFormat.B8G8R8A8Unorm,
        RhiFormat.R16G16B16A16_Float  => VkFormat.R16G16B16A16Sfloat,
        RhiFormat.D24_UNorm_S8_UInt   => VkFormat.D24UnormS8Uint,
        RhiFormat.D32_Float           => VkFormat.D32Sfloat,
        RhiFormat.R32_UInt            => VkFormat.R32Uint,
        RhiFormat.R32G32_Float        => VkFormat.R32G32Sfloat,
        RhiFormat.R32G32B32_Float     => VkFormat.R32G32B32Sfloat,
        RhiFormat.R32G32B32A32_Float  => VkFormat.R32G32B32A32Sfloat,
        RhiFormat.R32_Float           => VkFormat.R32Sfloat,
        RhiFormat.R16G16B16A16_UNorm  => VkFormat.R16G16B16A16Unorm,
        RhiFormat.R8G8B8A8_UInt       => VkFormat.R8G8B8A8Uint,
        RhiFormat.R16G16_UNorm        => VkFormat.R16G16Unorm,
        RhiFormat.R16G16_Float        => VkFormat.R16G16Sfloat,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unmapped format"),
    };

    public static int BytesPerPixel(RhiFormat format) => format switch
    {
        RhiFormat.R8G8B8A8_UNorm or RhiFormat.R8G8B8A8_UNorm_SRgb or RhiFormat.B8G8R8A8_UNorm
            or RhiFormat.R8G8B8A8_UInt or RhiFormat.R32_UInt or RhiFormat.R32_Float
            or RhiFormat.R16G16_UNorm or RhiFormat.R16G16_Float                                   => 4,
        RhiFormat.R16G16B16A16_Float or RhiFormat.R16G16B16A16_UNorm or RhiFormat.R32G32_Float     => 8,
        RhiFormat.R32G32B32_Float                                                                 => 12,
        RhiFormat.R32G32B32A32_Float                                                              => 16,
        RhiFormat.D24_UNorm_S8_UInt                                                               => 4,
        RhiFormat.D32_Float                                                                       => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "BytesPerPixel undefined"),
    };

    public static bool IsDepthFormat(RhiFormat format) =>
        format == RhiFormat.D24_UNorm_S8_UInt || format == RhiFormat.D32_Float;

    /// <summary>Image aspect mask for a format -- colour, or depth (+ stencil for D24S8).</summary>
    public static ImageAspectFlags AspectOf(RhiFormat format) =>
        IsDepthFormat(format)
            ? ImageAspectFlags.DepthBit |
              (format == RhiFormat.D24_UNorm_S8_UInt ? ImageAspectFlags.StencilBit : 0)
            : ImageAspectFlags.ColorBit;

    public static CullModeFlags ToVk(RhiCullMode cullMode) => cullMode switch
    {
        RhiCullMode.None  => CullModeFlags.None,
        RhiCullMode.Back  => CullModeFlags.BackBit,
        RhiCullMode.Front => CullModeFlags.FrontBit,
        _ => CullModeFlags.None,
    };

    public static PolygonMode ToVk(RhiFillMode fillMode) => fillMode switch
    {
        RhiFillMode.Solid     => PolygonMode.Fill,
        RhiFillMode.Wireframe => PolygonMode.Line,
        _ => PolygonMode.Fill,
    };

    public static CompareOp ToVk(RhiCompareOp compareOp) => compareOp switch
    {
        RhiCompareOp.Never        => CompareOp.Never,
        RhiCompareOp.Less         => CompareOp.Less,
        RhiCompareOp.Equal        => CompareOp.Equal,
        RhiCompareOp.LessEqual    => CompareOp.LessOrEqual,
        RhiCompareOp.Greater      => CompareOp.Greater,
        RhiCompareOp.NotEqual     => CompareOp.NotEqual,
        RhiCompareOp.GreaterEqual => CompareOp.GreaterOrEqual,
        RhiCompareOp.Always       => CompareOp.Always,
        _ => CompareOp.LessOrEqual,
    };

    public static VkBlendFactor ToVk(RhiBlendFactor blendFactor) => blendFactor switch
    {
        RhiBlendFactor.Zero             => VkBlendFactor.Zero,
        RhiBlendFactor.One              => VkBlendFactor.One,
        RhiBlendFactor.SrcColor         => VkBlendFactor.SrcColor,
        RhiBlendFactor.OneMinusSrcColor => VkBlendFactor.OneMinusSrcColor,
        RhiBlendFactor.DstColor         => VkBlendFactor.DstColor,
        RhiBlendFactor.OneMinusDstColor => VkBlendFactor.OneMinusDstColor,
        RhiBlendFactor.SrcAlpha         => VkBlendFactor.SrcAlpha,
        RhiBlendFactor.OneMinusSrcAlpha => VkBlendFactor.OneMinusSrcAlpha,
        RhiBlendFactor.DstAlpha         => VkBlendFactor.DstAlpha,
        RhiBlendFactor.OneMinusDstAlpha => VkBlendFactor.OneMinusDstAlpha,
        _ => VkBlendFactor.Zero,
    };

    public static VkBlendOp ToVk(RhiBlendOp blendOp) => blendOp switch
    {
        RhiBlendOp.Add             => VkBlendOp.Add,
        RhiBlendOp.Subtract        => VkBlendOp.Subtract,
        RhiBlendOp.ReverseSubtract => VkBlendOp.ReverseSubtract,
        RhiBlendOp.Min             => VkBlendOp.Min,
        RhiBlendOp.Max             => VkBlendOp.Max,
        _ => VkBlendOp.Add,
    };

    public static SamplerAddressMode ToVk(RhiAddressMode addressMode) => addressMode switch
    {
        RhiAddressMode.Wrap   => SamplerAddressMode.Repeat,
        RhiAddressMode.Mirror => SamplerAddressMode.MirroredRepeat,
        RhiAddressMode.Clamp  => SamplerAddressMode.ClampToEdge,
        RhiAddressMode.Border => SamplerAddressMode.ClampToBorder,
        _ => SamplerAddressMode.Repeat,
    };

    public static PrimitiveTopology ToVk(RhiPrimitiveTopology topology) => topology switch
    {
        RhiPrimitiveTopology.TriangleList  => PrimitiveTopology.TriangleList,
        RhiPrimitiveTopology.TriangleStrip => PrimitiveTopology.TriangleStrip,
        RhiPrimitiveTopology.LineList      => PrimitiveTopology.LineList,
        RhiPrimitiveTopology.LineStrip     => PrimitiveTopology.LineStrip,
        RhiPrimitiveTopology.PointList     => PrimitiveTopology.PointList,
        _ => PrimitiveTopology.TriangleList,
    };

    public static SampleCountFlags ToSampleCount(int sampleCount) => sampleCount switch
    {
        1  => SampleCountFlags.Count1Bit,
        2  => SampleCountFlags.Count2Bit,
        4  => SampleCountFlags.Count4Bit,
        8  => SampleCountFlags.Count8Bit,
        16 => SampleCountFlags.Count16Bit,
        _  => SampleCountFlags.Count1Bit,
    };
}
