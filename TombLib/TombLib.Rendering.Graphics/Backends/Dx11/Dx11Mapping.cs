using System;
using TombLib.Rendering.Graphics.Rhi;
using DX = Silk.NET.Direct3D11;
using DXGI = Silk.NET.DXGI;
using SilkD3D = Silk.NET.Core.Native;

namespace TombLib.Rendering.Graphics.Backends.Dx11;

// Pure enum / flag translations between the RHI surface and Silk.NET's
// Direct3D11 / DXGI types. No runtime state, no D3D objects.
internal static class Dx11Mapping
{
    public static DXGI.Format ToDxgi(Format f) => f switch
    {
        Format.R8G8B8A8_UNorm       => DXGI.Format.FormatR8G8B8A8Unorm,
        Format.R8G8B8A8_UNorm_SRgb  => DXGI.Format.FormatR8G8B8A8UnormSrgb,
        Format.B8G8R8A8_UNorm       => DXGI.Format.FormatB8G8R8A8Unorm,
        Format.R16G16B16A16_Float   => DXGI.Format.FormatR16G16B16A16Float,
        Format.D24_UNorm_S8_UInt    => DXGI.Format.FormatD24UnormS8Uint,
        Format.D32_Float            => DXGI.Format.FormatD32Float,
        Format.R32_UInt             => DXGI.Format.FormatR32Uint,
        Format.R32G32_Float         => DXGI.Format.FormatR32G32Float,
        Format.R32G32B32_Float      => DXGI.Format.FormatR32G32B32Float,
        Format.R32G32B32A32_Float   => DXGI.Format.FormatR32G32B32A32Float,
        Format.R32_Float            => DXGI.Format.FormatR32Float,
        Format.R16G16B16A16_UNorm   => DXGI.Format.FormatR16G16B16A16Unorm,
        Format.R8G8B8A8_UInt        => DXGI.Format.FormatR8G8B8A8Uint,
        Format.R16G16_UNorm         => DXGI.Format.FormatR16G16Unorm,
        Format.R16G16_Float         => DXGI.Format.FormatR16G16Float,
        Format.Unknown              => DXGI.Format.FormatUnknown,
        _ => throw new ArgumentOutOfRangeException(nameof(f), f, "Unmapped Format"),
    };

    public static DX.CullMode ToD3d(CullMode m) => m switch
    {
        CullMode.None  => DX.CullMode.None,
        CullMode.Back  => DX.CullMode.Back,
        CullMode.Front => DX.CullMode.Front,
        _ => throw new ArgumentOutOfRangeException(nameof(m)),
    };

    public static DX.FillMode ToD3d(FillMode m) => m switch
    {
        FillMode.Solid     => DX.FillMode.Solid,
        FillMode.Wireframe => DX.FillMode.Wireframe,
        _ => throw new ArgumentOutOfRangeException(nameof(m)),
    };

    public static DX.ComparisonFunc ToD3d(CompareOp op) => op switch
    {
        CompareOp.Never        => DX.ComparisonFunc.Never,
        CompareOp.Less         => DX.ComparisonFunc.Less,
        CompareOp.Equal        => DX.ComparisonFunc.Equal,
        CompareOp.LessEqual    => DX.ComparisonFunc.LessEqual,
        CompareOp.Greater      => DX.ComparisonFunc.Greater,
        CompareOp.NotEqual     => DX.ComparisonFunc.NotEqual,
        CompareOp.GreaterEqual => DX.ComparisonFunc.GreaterEqual,
        CompareOp.Always       => DX.ComparisonFunc.Always,
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };

    public static DX.Blend ToD3d(BlendFactor f) => f switch
    {
        BlendFactor.Zero               => DX.Blend.Zero,
        BlendFactor.One                => DX.Blend.One,
        BlendFactor.SrcColor           => DX.Blend.SrcColor,
        BlendFactor.OneMinusSrcColor   => DX.Blend.InvSrcColor,
        BlendFactor.DstColor           => DX.Blend.DestColor,
        BlendFactor.OneMinusDstColor   => DX.Blend.InvDestColor,
        BlendFactor.SrcAlpha           => DX.Blend.SrcAlpha,
        BlendFactor.OneMinusSrcAlpha   => DX.Blend.InvSrcAlpha,
        BlendFactor.DstAlpha           => DX.Blend.DestAlpha,
        BlendFactor.OneMinusDstAlpha   => DX.Blend.InvDestAlpha,
        _ => throw new ArgumentOutOfRangeException(nameof(f)),
    };

    public static DX.BlendOp ToD3d(BlendOp op) => op switch
    {
        BlendOp.Add             => DX.BlendOp.Add,
        BlendOp.Subtract        => DX.BlendOp.Subtract,
        BlendOp.ReverseSubtract => DX.BlendOp.RevSubtract,
        BlendOp.Min             => DX.BlendOp.Min,
        BlendOp.Max             => DX.BlendOp.Max,
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };

    public static (DX.Filter, uint maxAniso) ToD3d(FilterMode minMag, FilterMode mip, int maxAniso)
    {
        if (minMag == FilterMode.Anisotropic || mip == FilterMode.Anisotropic)
            return (DX.Filter.Anisotropic, (uint)Math.Clamp(maxAniso, 1, 16));

        // Map min, mag, mip = nearest/linear x nearest/linear x nearest/linear -> Filter enum bit pattern.
        bool minLinear = minMag == FilterMode.Linear;
        bool magLinear = minMag == FilterMode.Linear;
        bool mipLinear = mip    == FilterMode.Linear;

        // D3D11_FILTER bit layout: 0x00 MMM(min) MMM(mag) MMM(mip), each MMM = 1 if linear, 0 if point.
        // Use the well-known constants from the Filter enum.
        DX.Filter filter = (minLinear, magLinear, mipLinear) switch
        {
            (false, false, false) => DX.Filter.MinMagMipPoint,
            (false, false, true)  => DX.Filter.MinMagPointMipLinear,
            (false, true,  false) => DX.Filter.MinPointMagLinearMipPoint,
            (false, true,  true)  => DX.Filter.MinPointMagMipLinear,
            (true,  false, false) => DX.Filter.MinLinearMagMipPoint,
            (true,  false, true)  => DX.Filter.MinLinearMagPointMipLinear,
            (true,  true,  false) => DX.Filter.MinMagLinearMipPoint,
            (true,  true,  true)  => DX.Filter.MinMagMipLinear,
        };
        return (filter, 1);
    }

    public static DX.TextureAddressMode ToD3d(AddressMode m) => m switch
    {
        AddressMode.Wrap   => DX.TextureAddressMode.Wrap,
        AddressMode.Mirror => DX.TextureAddressMode.Mirror,
        AddressMode.Clamp  => DX.TextureAddressMode.Clamp,
        AddressMode.Border => DX.TextureAddressMode.Border,
        _ => throw new ArgumentOutOfRangeException(nameof(m)),
    };

    public static Silk.NET.Core.Native.D3DPrimitiveTopology ToD3d(PrimitiveTopology t) => t switch
    {
        PrimitiveTopology.TriangleList  => Silk.NET.Core.Native.D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist,
        PrimitiveTopology.TriangleStrip => Silk.NET.Core.Native.D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglestrip,
        PrimitiveTopology.LineList      => Silk.NET.Core.Native.D3DPrimitiveTopology.D3D11PrimitiveTopologyLinelist,
        PrimitiveTopology.LineStrip     => Silk.NET.Core.Native.D3DPrimitiveTopology.D3D11PrimitiveTopologyLinestrip,
        PrimitiveTopology.PointList     => Silk.NET.Core.Native.D3DPrimitiveTopology.D3D11PrimitiveTopologyPointlist,
        _ => throw new ArgumentOutOfRangeException(nameof(t)),
    };

    public static byte ToD3dColorWriteMask(ColorWriteMask m)
    {
        byte v = 0;
        if ((m & ColorWriteMask.R) != 0) v |= (byte)DX.ColorWriteEnable.Red;
        if ((m & ColorWriteMask.G) != 0) v |= (byte)DX.ColorWriteEnable.Green;
        if ((m & ColorWriteMask.B) != 0) v |= (byte)DX.ColorWriteEnable.Blue;
        if ((m & ColorWriteMask.A) != 0) v |= (byte)DX.ColorWriteEnable.Alpha;
        return v;
    }

    public static DX.BindFlag ToBindFlags(BufferBindFlags f)
    {
        DX.BindFlag bf = 0;
        if ((f & BufferBindFlags.Vertex)     != 0) bf |= DX.BindFlag.VertexBuffer;
        if ((f & BufferBindFlags.Index)      != 0) bf |= DX.BindFlag.IndexBuffer;
        if ((f & BufferBindFlags.Constant)   != 0) bf |= DX.BindFlag.ConstantBuffer;
        if ((f & BufferBindFlags.Structured) != 0) bf |= DX.BindFlag.ShaderResource;
        return bf;
    }

    public static DX.BindFlag ToBindFlags(TextureBindFlags f)
    {
        DX.BindFlag bf = 0;
        if ((f & TextureBindFlags.ShaderResource) != 0) bf |= DX.BindFlag.ShaderResource;
        if ((f & TextureBindFlags.RenderTarget)   != 0) bf |= DX.BindFlag.RenderTarget;
        if ((f & TextureBindFlags.DepthStencil)   != 0) bf |= DX.BindFlag.DepthStencil;
        return bf;
    }

    public static (DX.Usage usage, DX.CpuAccessFlag cpu) ToD3d(BufferUsage u) => u switch
    {
        BufferUsage.Immutable           => (DX.Usage.Immutable, 0),
        BufferUsage.DynamicUniform      => (DX.Usage.Dynamic,   DX.CpuAccessFlag.Write),
        BufferUsage.DynamicVertex       => (DX.Usage.Dynamic,   DX.CpuAccessFlag.Write),
        BufferUsage.StructuredReadOnly  => (DX.Usage.Default,   0),
        _ => throw new ArgumentOutOfRangeException(nameof(u)),
    };
}
