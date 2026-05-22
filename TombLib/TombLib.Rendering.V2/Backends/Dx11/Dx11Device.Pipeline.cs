using System;
using Silk.NET.Core.Native;
using DX = Silk.NET.Direct3D11;
using DXGI = Silk.NET.DXGI;
using TombLib.RenderingV2.Rhi;
// Selective import of Silk.NET extension methods on ComPtr<T>. The full
// namespace is not imported because it collides with Rhi descriptor types
// (Format, BufferDesc, SamplerDesc).
using static Silk.NET.Direct3D11.D3D11DeviceVtblExtensions;

namespace TombLib.RenderingV2.Backends.Dx11;

// Pipeline-state objects for the Direct3D 11 backend. D3D11 has no single
// monolithic PSO, so one Rhi PipelineDesc maps to a bundle of immutable
// state objects (shaders, input layout, rasterizer, depth/stencil, blend)
// that the command list binds together.
public unsafe sealed partial class Dx11Device
{
    public PipelineHandle CreatePipeline(PipelineDesc desc)
    {
        // ---- Shaders ----
        ComPtr<DX.ID3D11VertexShader> vertexShader = default;
        ComPtr<DX.ID3D11PixelShader>  pixelShader  = default;

        byte[] vertexShaderBytes = desc.VertexShader.DxbcBytes
            ?? throw new InvalidOperationException("VertexShader.DxbcBytes is null. Did the shader build pipeline run?");
        byte[] pixelShaderBytes = desc.FragmentShader.DxbcBytes
            ?? throw new InvalidOperationException("FragmentShader.DxbcBytes is null. Did the shader build pipeline run?");

        fixed (byte* pVertexShader = vertexShaderBytes)
        fixed (byte* pPixelShader  = pixelShaderBytes)
        {
            SilkMarshal.ThrowHResult(Device.CreateVertexShader(
                pVertexShader, (nuint)vertexShaderBytes.Length,
                (DX.ID3D11ClassLinkage*)null, vertexShader.GetAddressOf()));
            SilkMarshal.ThrowHResult(Device.CreatePixelShader(
                pPixelShader, (nuint)pixelShaderBytes.Length,
                (DX.ID3D11ClassLinkage*)null, pixelShader.GetAddressOf()));
        }

        // ---- Input layout ----
        ComPtr<DX.ID3D11InputLayout> inputLayout = default;
        var elements = new DX.InputElementDesc[desc.VertexAttributes.Length];

        // Pin one ASCII byte array per semantic name so its address stays
        // valid for the whole CreateInputLayout call.
        byte[][] semanticBlobs = new byte[desc.VertexAttributes.Length][];
        for (int i = 0; i < desc.VertexAttributes.Length; i++)
            semanticBlobs[i] = System.Text.Encoding.ASCII.GetBytes(desc.VertexAttributes[i].SemanticName + "\0");

        var semanticHandles = new System.Runtime.InteropServices.GCHandle[semanticBlobs.Length];
        try
        {
            for (int i = 0; i < desc.VertexAttributes.Length; i++)
            {
                var attribute = desc.VertexAttributes[i];
                semanticHandles[i] = System.Runtime.InteropServices.GCHandle.Alloc(
                    semanticBlobs[i], System.Runtime.InteropServices.GCHandleType.Pinned);
                elements[i] = new DX.InputElementDesc
                {
                    SemanticName         = (byte*)semanticHandles[i].AddrOfPinnedObject(),
                    SemanticIndex        = (uint)attribute.SemanticIndex,
                    Format               = Dx11Mapping.ToDxgi(attribute.Format),
                    InputSlot            = (uint)attribute.BufferSlot,
                    AlignedByteOffset    = (uint)attribute.Offset,
                    InputSlotClass       = attribute.PerInstance
                                           ? DX.InputClassification.PerInstanceData
                                           : DX.InputClassification.PerVertexData,
                    InstanceDataStepRate = attribute.PerInstance ? 1u : 0u,
                };
            }

            fixed (DX.InputElementDesc* pElements = elements)
            fixed (byte* pVertexShader = vertexShaderBytes)
            {
                SilkMarshal.ThrowHResult(Device.CreateInputLayout(
                    pElements, (uint)elements.Length,
                    pVertexShader, (nuint)vertexShaderBytes.Length,
                    inputLayout.GetAddressOf()));
            }
        }
        finally
        {
            for (int i = 0; i < semanticHandles.Length; i++)
                if (semanticHandles[i].IsAllocated) semanticHandles[i].Free();
        }

        // ---- Rasterizer state ----
        var rasterizer = desc.Rasterizer;
        var rasterizerDesc = new DX.RasterizerDesc
        {
            FillMode              = Dx11Mapping.ToD3d(rasterizer.FillMode),
            CullMode              = Dx11Mapping.ToD3d(rasterizer.CullMode),
            FrontCounterClockwise = rasterizer.FrontCounterClockwise,
            DepthBias             = rasterizer.DepthBias,
            DepthBiasClamp        = 0,
            SlopeScaledDepthBias  = rasterizer.SlopeScaledDepthBias,
            DepthClipEnable       = true,
            ScissorEnable         = rasterizer.ScissorEnable,
            // MSAA-aware coverage rules when the bound render target is
            // multisampled — harmless on single-sample targets.
            MultisampleEnable     = true,
            AntialiasedLineEnable = false,
        };
        ComPtr<DX.ID3D11RasterizerState> rasterizerState = default;
        SilkMarshal.ThrowHResult(Device.CreateRasterizerState(in rasterizerDesc, rasterizerState.GetAddressOf()));

        // ---- Depth / stencil state ----
        var depthStencil = desc.DepthStencil;
        var depthStencilDesc = new DX.DepthStencilDesc
        {
            DepthEnable      = depthStencil.DepthTestEnable,
            DepthWriteMask   = depthStencil.DepthWriteEnable ? DX.DepthWriteMask.All : DX.DepthWriteMask.Zero,
            DepthFunc        = Dx11Mapping.ToD3d(depthStencil.DepthCompare),
            StencilEnable    = false,
            StencilReadMask  = 0xFF,
            StencilWriteMask = 0xFF,
        };
        ComPtr<DX.ID3D11DepthStencilState> depthStencilState = default;
        SilkMarshal.ThrowHResult(Device.CreateDepthStencilState(in depthStencilDesc, depthStencilState.GetAddressOf()));

        // ---- Blend state (single render target) ----
        var blend = desc.BlendStates.Length > 0 ? desc.BlendStates[0] : BlendState.Opaque;
        var blendDesc = new DX.BlendDesc
        {
            AlphaToCoverageEnable  = false,
            IndependentBlendEnable = false,
        };
        blendDesc.RenderTarget[0] = new DX.RenderTargetBlendDesc
        {
            BlendEnable           = blend.Enable,
            SrcBlend              = Dx11Mapping.ToD3d(blend.SrcColor),
            DestBlend             = Dx11Mapping.ToD3d(blend.DstColor),
            BlendOp               = Dx11Mapping.ToD3d(blend.ColorOp),
            SrcBlendAlpha         = Dx11Mapping.ToD3d(blend.SrcAlpha),
            DestBlendAlpha        = Dx11Mapping.ToD3d(blend.DstAlpha),
            BlendOpAlpha          = Dx11Mapping.ToD3d(blend.AlphaOp),
            RenderTargetWriteMask = Dx11Mapping.ToD3dColorWriteMask(blend.WriteMask),
        };
        ComPtr<DX.ID3D11BlendState> blendState = default;
        SilkMarshal.ThrowHResult(Device.CreateBlendState(in blendDesc, blendState.GetAddressOf()));

        // ---- Per-slot vertex strides (read back by the command list on bind) ----
        var vertexStrides = new int[RhiLimits.MaxVertexBuffers];
        for (int i = 0; i < desc.VertexBufferLayouts.Length && i < vertexStrides.Length; i++)
            vertexStrides[i] = desc.VertexBufferLayouts[i].StrideBytes;

        uint id = AllocHandle();
        Pipelines[id] = new Dx11Pipeline
        {
            Vs            = vertexShader,
            Ps            = pixelShader,
            InputLayout   = inputLayout,
            Rasterizer    = rasterizerState,
            Blend         = blendState,
            Depth         = depthStencilState,
            Topology      = Dx11Mapping.ToD3d(desc.Topology),
            VertexStrides = vertexStrides,
        };
        return new PipelineHandle(id);
    }

    public void Destroy(PipelineHandle handle)
    {
        if (Pipelines.Remove(handle.Id, out var pipeline)) pipeline.Dispose();
    }
}
