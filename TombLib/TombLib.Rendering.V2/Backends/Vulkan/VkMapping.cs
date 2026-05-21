using System;
using Silk.NET.Vulkan;
using RhiFormat = TombLib.RenderingV2.Rhi.Format;
using RhiCullMode = TombLib.RenderingV2.Rhi.CullMode;
using RhiFillMode = TombLib.RenderingV2.Rhi.FillMode;
using RhiCompareOp = TombLib.RenderingV2.Rhi.CompareOp;
using RhiBlendFactor = TombLib.RenderingV2.Rhi.BlendFactor;
using RhiBlendOp = TombLib.RenderingV2.Rhi.BlendOp;
using RhiFilterMode = TombLib.RenderingV2.Rhi.FilterMode;
using RhiAddressMode = TombLib.RenderingV2.Rhi.AddressMode;
using RhiPrimitiveTopology = TombLib.RenderingV2.Rhi.PrimitiveTopology;

namespace TombLib.RenderingV2.Backends.Vulkan;

/// <summary>
/// RHI → Vulkan enum mappings. Mirrors the corresponding Dx11Mapping helpers.
/// All conversions are one-to-one and total: every RHI enum value gets a
/// concrete Vulkan equivalent.
/// </summary>
internal static class VkMapping
{
    public static Silk.NET.Vulkan.Format ToVk(RhiFormat f) => f switch
    {
        RhiFormat.Unknown            => Silk.NET.Vulkan.Format.Undefined,
        RhiFormat.R8G8B8A8_UNorm     => Silk.NET.Vulkan.Format.R8G8B8A8Unorm,
        RhiFormat.R8G8B8A8_UNorm_SRgb=> Silk.NET.Vulkan.Format.R8G8B8A8Srgb,
        RhiFormat.B8G8R8A8_UNorm     => Silk.NET.Vulkan.Format.B8G8R8A8Unorm,
        RhiFormat.R16G16B16A16_Float => Silk.NET.Vulkan.Format.R16G16B16A16Sfloat,
        RhiFormat.D24_UNorm_S8_UInt  => Silk.NET.Vulkan.Format.D24UnormS8Uint,
        RhiFormat.D32_Float          => Silk.NET.Vulkan.Format.D32Sfloat,
        RhiFormat.R32_UInt           => Silk.NET.Vulkan.Format.R32Uint,
        RhiFormat.R32G32_Float       => Silk.NET.Vulkan.Format.R32G32Sfloat,
        RhiFormat.R32G32B32_Float    => Silk.NET.Vulkan.Format.R32G32B32Sfloat,
        RhiFormat.R32G32B32A32_Float => Silk.NET.Vulkan.Format.R32G32B32A32Sfloat,
        RhiFormat.R32_Float          => Silk.NET.Vulkan.Format.R32Sfloat,
        RhiFormat.R16G16B16A16_UNorm => Silk.NET.Vulkan.Format.R16G16B16A16Unorm,
        RhiFormat.R8G8B8A8_UInt      => Silk.NET.Vulkan.Format.R8G8B8A8Uint,
        RhiFormat.R16G16_UNorm       => Silk.NET.Vulkan.Format.R16G16Unorm,
        RhiFormat.R16G16_Float       => Silk.NET.Vulkan.Format.R16G16Sfloat,
        _ => throw new ArgumentOutOfRangeException(nameof(f), f, "Unmapped format")
    };

    public static int BytesPerPixel(RhiFormat f) => f switch
    {
        RhiFormat.R8G8B8A8_UNorm or RhiFormat.R8G8B8A8_UNorm_SRgb or RhiFormat.B8G8R8A8_UNorm
            or RhiFormat.R8G8B8A8_UInt or RhiFormat.R32_UInt or RhiFormat.R32_Float
            or RhiFormat.R16G16_UNorm or RhiFormat.R16G16_Float                          => 4,
        RhiFormat.R16G16B16A16_Float or RhiFormat.R16G16B16A16_UNorm or RhiFormat.R32G32_Float => 8,
        RhiFormat.R32G32B32_Float                                                         => 12,
        RhiFormat.R32G32B32A32_Float                                                      => 16,
        RhiFormat.D24_UNorm_S8_UInt                                                       => 4,
        RhiFormat.D32_Float                                                               => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(f), f, "BytesPerPixel undefined"),
    };

    public static bool IsDepthFormat(RhiFormat f) =>
        f == RhiFormat.D24_UNorm_S8_UInt || f == RhiFormat.D32_Float;

    public static ImageAspectFlags AspectOf(RhiFormat f) =>
        IsDepthFormat(f) ? (ImageAspectFlags.DepthBit | (f == RhiFormat.D24_UNorm_S8_UInt ? ImageAspectFlags.StencilBit : 0))
                         : ImageAspectFlags.ColorBit;

    public static CullModeFlags ToVk(RhiCullMode c) => c switch
    {
        RhiCullMode.None  => CullModeFlags.None,
        RhiCullMode.Back  => CullModeFlags.BackBit,
        RhiCullMode.Front => CullModeFlags.FrontBit,
        _ => CullModeFlags.None,
    };

    public static PolygonMode ToVk(RhiFillMode f) => f switch
    {
        RhiFillMode.Solid     => PolygonMode.Fill,
        RhiFillMode.Wireframe => PolygonMode.Line,
        _ => PolygonMode.Fill,
    };

    public static CompareOp ToVk(RhiCompareOp c) => c switch
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

    public static Silk.NET.Vulkan.BlendFactor ToVk(RhiBlendFactor f) => f switch
    {
        RhiBlendFactor.Zero             => Silk.NET.Vulkan.BlendFactor.Zero,
        RhiBlendFactor.One              => Silk.NET.Vulkan.BlendFactor.One,
        RhiBlendFactor.SrcColor         => Silk.NET.Vulkan.BlendFactor.SrcColor,
        RhiBlendFactor.OneMinusSrcColor => Silk.NET.Vulkan.BlendFactor.OneMinusSrcColor,
        RhiBlendFactor.DstColor         => Silk.NET.Vulkan.BlendFactor.DstColor,
        RhiBlendFactor.OneMinusDstColor => Silk.NET.Vulkan.BlendFactor.OneMinusDstColor,
        RhiBlendFactor.SrcAlpha         => Silk.NET.Vulkan.BlendFactor.SrcAlpha,
        RhiBlendFactor.OneMinusSrcAlpha => Silk.NET.Vulkan.BlendFactor.OneMinusSrcAlpha,
        RhiBlendFactor.DstAlpha         => Silk.NET.Vulkan.BlendFactor.DstAlpha,
        RhiBlendFactor.OneMinusDstAlpha => Silk.NET.Vulkan.BlendFactor.OneMinusDstAlpha,
        _ => Silk.NET.Vulkan.BlendFactor.Zero,
    };

    public static Silk.NET.Vulkan.BlendOp ToVk(RhiBlendOp op) => op switch
    {
        RhiBlendOp.Add             => Silk.NET.Vulkan.BlendOp.Add,
        RhiBlendOp.Subtract        => Silk.NET.Vulkan.BlendOp.Subtract,
        RhiBlendOp.ReverseSubtract => Silk.NET.Vulkan.BlendOp.ReverseSubtract,
        RhiBlendOp.Min             => Silk.NET.Vulkan.BlendOp.Min,
        RhiBlendOp.Max             => Silk.NET.Vulkan.BlendOp.Max,
        _ => Silk.NET.Vulkan.BlendOp.Add,
    };

    public static Silk.NET.Vulkan.SamplerAddressMode ToVk(RhiAddressMode m) => m switch
    {
        RhiAddressMode.Wrap   => SamplerAddressMode.Repeat,
        RhiAddressMode.Mirror => SamplerAddressMode.MirroredRepeat,
        RhiAddressMode.Clamp  => SamplerAddressMode.ClampToEdge,
        RhiAddressMode.Border => SamplerAddressMode.ClampToBorder,
        _ => SamplerAddressMode.Repeat,
    };

    public static PrimitiveTopology ToVk(RhiPrimitiveTopology t) => t switch
    {
        RhiPrimitiveTopology.TriangleList  => PrimitiveTopology.TriangleList,
        RhiPrimitiveTopology.TriangleStrip => PrimitiveTopology.TriangleStrip,
        RhiPrimitiveTopology.LineList      => PrimitiveTopology.LineList,
        RhiPrimitiveTopology.LineStrip     => PrimitiveTopology.LineStrip,
        RhiPrimitiveTopology.PointList     => PrimitiveTopology.PointList,
        _ => PrimitiveTopology.TriangleList,
    };

    public static SampleCountFlags ToSampleCount(int samples) => samples switch
    {
        1  => SampleCountFlags.Count1Bit,
        2  => SampleCountFlags.Count2Bit,
        4  => SampleCountFlags.Count4Bit,
        8  => SampleCountFlags.Count8Bit,
        16 => SampleCountFlags.Count16Bit,
        _  => SampleCountFlags.Count1Bit,
    };
}
