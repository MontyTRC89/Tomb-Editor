using System;
using System.Numerics;

namespace TombLib.Rendering.Graphics.Rhi;

// Fixed binding slot counts. Tuned to cover every pass we currently need
// without bloating the backend bookkeeping. Slot indices are shared across
// backends so a single shader source maps cleanly to all three.
public static class RhiLimits
{
    public const int MaxColorAttachments  = 4;
    public const int MaxVertexBuffers     = 8;
    public const int MaxVertexAttributes  = 16;
    public const int MaxConstantBuffers   = 4;   // b0..b3
    public const int MaxTextureBindings   = 8;   // t0..t7
    public const int MaxSamplerBindings   = 4;   // s0..s3
    public const int MaxStorageBuffers    = 2;   // SSBO / StructuredBuffer
    public const int PushConstantSize     = 128; // emulated via b0 on DX11/GL
}

public readonly struct BufferDesc
{
    public readonly int SizeBytes;
    public readonly BufferUsage Usage;
    public readonly BufferBindFlags BindFlags;
    public readonly int StructureStride;   // only for StructuredReadOnly
    public readonly string? DebugName;

    public BufferDesc(int sizeBytes, BufferUsage usage, BufferBindFlags bindFlags,
                     int structureStride = 0, string? debugName = null)
    {
        SizeBytes       = sizeBytes;
        Usage           = usage;
        BindFlags       = bindFlags;
        StructureStride = structureStride;
        DebugName       = debugName;
    }
}

public readonly struct TextureDesc
{
    public readonly TextureKind Kind;
    public readonly int Width;
    public readonly int Height;
    public readonly int ArrayLayers;    // for Texture2DArray; 1 otherwise
    public readonly int MipLevels;
    public readonly Format Format;
    public readonly TextureBindFlags BindFlags;
    public readonly int Samples;        // 1 = no MSAA
    public readonly string? DebugName;

    public TextureDesc(TextureKind kind, int width, int height, Format format,
                       TextureBindFlags bindFlags,
                       int arrayLayers = 1, int mipLevels = 1, int samples = 1,
                       string? debugName = null)
    {
        Kind        = kind;
        Width       = width;
        Height      = height;
        ArrayLayers = arrayLayers;
        MipLevels   = mipLevels;
        Format      = format;
        BindFlags   = bindFlags;
        Samples     = samples;
        DebugName   = debugName;
    }
}

public readonly struct SamplerDesc
{
    public readonly FilterMode MinFilter;
    public readonly FilterMode MagFilter;
    public readonly FilterMode MipFilter;
    public readonly AddressMode AddressU;
    public readonly AddressMode AddressV;
    public readonly AddressMode AddressW;
    public readonly int MaxAnisotropy;
    public readonly Vector4 BorderColor;

    public SamplerDesc(FilterMode minMag, AddressMode address, int maxAnisotropy = 1)
    {
        MinFilter      = minMag;
        MagFilter      = minMag;
        MipFilter      = minMag;
        AddressU = AddressV = AddressW = address;
        MaxAnisotropy  = maxAnisotropy;
        BorderColor    = Vector4.Zero;
    }
}

// Vertex layout. Mirrors DX11 InputElement / VK VertexInputAttribute.
public readonly struct VertexAttribute
{
    public readonly string SemanticName;   // matches HLSL semantic in the VS
    public readonly int    SemanticIndex;
    public readonly Format Format;
    public readonly int    BufferSlot;     // 0..MaxVertexBuffers-1
    public readonly int    Offset;         // byte offset inside the buffer slot
    public readonly bool   PerInstance;

    public VertexAttribute(string semanticName, int semanticIndex, Format format,
                           int bufferSlot, int offset, bool perInstance = false)
    {
        SemanticName  = semanticName;
        SemanticIndex = semanticIndex;
        Format        = format;
        BufferSlot    = bufferSlot;
        Offset        = offset;
        PerInstance   = perInstance;
    }
}

public readonly struct VertexBufferLayout
{
    public readonly int  StrideBytes;
    public readonly bool PerInstance;

    public VertexBufferLayout(int strideBytes, bool perInstance = false)
    {
        StrideBytes = strideBytes;
        PerInstance = perInstance;
    }
}

public readonly struct RasterizerState
{
    public readonly CullMode CullMode;
    public readonly FillMode FillMode;
    public readonly bool     FrontCounterClockwise;
    public readonly int      DepthBias;
    public readonly float    SlopeScaledDepthBias;
    public readonly bool     ScissorEnable;

    public RasterizerState(CullMode cull, FillMode fill = FillMode.Solid,
                           bool frontCcw = false, int depthBias = 0,
                           float slopeBias = 0.0f, bool scissor = false)
    {
        CullMode              = cull;
        FillMode              = fill;
        FrontCounterClockwise = frontCcw;
        DepthBias             = depthBias;
        SlopeScaledDepthBias  = slopeBias;
        ScissorEnable         = scissor;
    }

    public static RasterizerState CullBack   => new(CullMode.Back);
    public static RasterizerState CullNone   => new(CullMode.None);
    public static RasterizerState Wireframe  => new(CullMode.None, FillMode.Wireframe);
}

public readonly struct DepthStencilState
{
    public readonly bool      DepthTestEnable;
    public readonly bool      DepthWriteEnable;
    public readonly CompareOp DepthCompare;

    public DepthStencilState(bool test, bool write, CompareOp cmp)
    {
        DepthTestEnable  = test;
        DepthWriteEnable = write;
        DepthCompare     = cmp;
    }

    public static DepthStencilState Default       => new(true,  true,  CompareOp.LessEqual);
    public static DepthStencilState DepthReadOnly => new(true,  false, CompareOp.LessEqual);
    public static DepthStencilState Disabled      => new(false, false, CompareOp.Always);
}

public readonly struct BlendState
{
    public readonly bool           Enable;
    public readonly BlendFactor    SrcColor;
    public readonly BlendFactor    DstColor;
    public readonly BlendOp        ColorOp;
    public readonly BlendFactor    SrcAlpha;
    public readonly BlendFactor    DstAlpha;
    public readonly BlendOp        AlphaOp;
    public readonly ColorWriteMask WriteMask;

    public BlendState(bool enable, BlendFactor src, BlendFactor dst, BlendOp op,
                     BlendFactor srcA, BlendFactor dstA, BlendOp opA,
                     ColorWriteMask mask = ColorWriteMask.All)
    {
        Enable    = enable;
        SrcColor  = src;  DstColor  = dst;  ColorOp  = op;
        SrcAlpha  = srcA; DstAlpha  = dstA; AlphaOp  = opA;
        WriteMask = mask;
    }

    public static BlendState Opaque => new(false,
        BlendFactor.One, BlendFactor.Zero, BlendOp.Add,
        BlendFactor.One, BlendFactor.Zero, BlendOp.Add);

    public static BlendState AlphaBlend => new(true,
        BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha, BlendOp.Add,
        BlendFactor.One,      BlendFactor.OneMinusSrcAlpha, BlendOp.Add);

    public static BlendState Additive => new(true,
        BlendFactor.SrcAlpha, BlendFactor.One, BlendOp.Add,
        BlendFactor.One,      BlendFactor.One, BlendOp.Add);

    public static BlendState PremultipliedAlpha => new(true,
        BlendFactor.One, BlendFactor.OneMinusSrcAlpha, BlendOp.Add,
        BlendFactor.One, BlendFactor.OneMinusSrcAlpha, BlendOp.Add);
}

// Embedded shader bytecode for one stage. The build pipeline produces three
// variants per shader; the backend picks the matching one.
public readonly struct ShaderBytecode
{
    public readonly ShaderStage Stage;
    public readonly byte[]      DxbcBytes;   // DX11
    public readonly byte[]      SpirvBytes;  // Vulkan
    public readonly byte[]      Glsl430Utf8; // OpenGL 4.3 (text source as UTF-8)
    public readonly string      EntryPoint;

    public ShaderBytecode(ShaderStage stage, byte[] dxbc, byte[] spirv,
                          byte[] glsl, string entryPoint = "main")
    {
        Stage       = stage;
        DxbcBytes   = dxbc;
        SpirvBytes  = spirv;
        Glsl430Utf8 = glsl;
        EntryPoint  = entryPoint;
    }
}

// Full graphics pipeline description. Immutable. Backends compile / link
// once at creation; SetPipeline at draw time is a single state swap.
public sealed class PipelineDesc
{
    public ShaderBytecode VertexShader;
    public ShaderBytecode FragmentShader;
    public VertexAttribute[]    VertexAttributes    = Array.Empty<VertexAttribute>();
    public VertexBufferLayout[] VertexBufferLayouts = Array.Empty<VertexBufferLayout>();
    public PrimitiveTopology    Topology            = PrimitiveTopology.TriangleList;
    public RasterizerState      Rasterizer          = RasterizerState.CullBack;
    public DepthStencilState    DepthStencil        = DepthStencilState.Default;
    public BlendState[]         BlendStates         = { BlendState.Opaque };
    public Format[]             ColorAttachmentFormats = { Format.R8G8B8A8_UNorm };
    public Format               DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt;
    public string?              DebugName;
}

public readonly struct SwapchainDesc
{
    public readonly IntPtr WindowHandle;     // HWND on Windows
    public readonly int    Width;
    public readonly int    Height;
    public readonly Format ColorFormat;
    public readonly Format DepthFormat;
    public readonly int    Samples;          // 1 = no MSAA
    public readonly bool   VSync;

    public SwapchainDesc(IntPtr hwnd, int width, int height,
                         Format color, Format depth,
                         int samples = 1, bool vsync = true)
    {
        WindowHandle = hwnd;
        Width        = width; Height = height;
        ColorFormat  = color; DepthFormat = depth;
        Samples      = samples;
        VSync        = vsync;
    }
}

// Description for BeginPass. Color attachment 0 is typically the swapchain;
// pass TextureHandle.default to use the current swapchain target.
public struct PassDesc
{
    public TextureHandle[]    ColorAttachments;
    public TextureHandle      DepthAttachment;
    public LoadOp[]           ColorLoadOps;
    public StoreOp[]          ColorStoreOps;
    public LoadOp             DepthLoadOp;
    public StoreOp            DepthStoreOp;
    public Vector4[]          ClearColors;
    public float              ClearDepth;
    public byte               ClearStencil;
    public int                ViewportX, ViewportY, ViewportWidth, ViewportHeight;
    public bool               UseSwapchain;     // shortcut: render to current swapchain
    public SwapchainHandle    Swapchain;        // only when UseSwapchain == true
}

// Binding sets passed to ICommandList.SetBindings. Slot indices map 1:1
// across backends (b0..b3, t0..t7, s0..s3, ssbo 0..1).
public ref struct Bindings
{
    public ReadOnlySpan<BufferHandle>  ConstantBuffers;  // up to MaxConstantBuffers
    public ReadOnlySpan<TextureHandle> Textures;         // up to MaxTextureBindings
    public ReadOnlySpan<SamplerHandle> Samplers;         // up to MaxSamplerBindings
    public ReadOnlySpan<BufferHandle>  StorageBuffers;   // up to MaxStorageBuffers
}

public readonly struct VertexBufferBinding
{
    public readonly BufferHandle Buffer;
    public readonly int          OffsetBytes;

    public VertexBufferBinding(BufferHandle buffer, int offsetBytes = 0)
    {
        Buffer      = buffer;
        OffsetBytes = offsetBytes;
    }
}
