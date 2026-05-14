using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using D3D11Usage = Silk.NET.Direct3D11.Usage;
using Format = Silk.NET.DXGI.Format;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace TombLib.Rendering.DirectX11
{
    // D3D11 implementation of RenderingDrawingImportedGeometry.
    //
    // Per-submesh texture binding distinguishes this from Dx11RenderingDrawingMesh:
    // we cannot collapse all submeshes into a single material. Each submesh's
    // ReciprocalTextureSize and Texture must be uploaded before its DrawIndexed.
    //
    // The cbuffer slot 1 layout (ImportedGeometryData) is updated PER SUBMESH because
    // ReciprocalTextureSize and TextureEnabled change submesh-by-submesh. The header
    // fields (World, Tint, UseVertexColors, AlphaTest) are updated ONCE per Render.
    public sealed unsafe class Dx11RenderingDrawingImportedGeometry : RenderingDrawingImportedGeometry
    {
        // Layout MUST mirror the ImportedGeometryData cbuffer in
        // ImportedGeometryShaderVS.hlsl/PS.hlsl.
        [StructLayout(LayoutKind.Explicit, Size = 64 + 16 + 8 + 4 + 4 + 4 + 12)]
        private struct ImportedGeometryDataLayout
        {
            [FieldOffset(0)]   public Matrix4x4 World;
            [FieldOffset(64)]  public Vector4 Tint;
            [FieldOffset(80)]  public Vector2 ReciprocalTextureSize;
            [FieldOffset(88)]  public int TextureEnabled;
            [FieldOffset(92)]  public int UseVertexColors;
            [FieldOffset(96)]  public int AlphaTest;
            // 12 bytes padding to align to 16
        }
        private static readonly int CbufferSize = ((Marshal.SizeOf<ImportedGeometryDataLayout>() + 15) / 16) * 16;

        private readonly Dx11RenderingDevice _device;
        private readonly ID3D11Buffer* _vertexBuffer;
        private readonly ID3D11Buffer* _indexBuffer;
        private readonly ID3D11Buffer* _cbuffer;
        private readonly Dx11VertexBufferBinding _vertexBufferBinding;
        private readonly Submesh[] _submeshes;

        public Dx11RenderingDrawingImportedGeometry(Dx11RenderingDevice device, Description description)
        {
            _device = device;
            _submeshes = description.Submeshes?.ToArray() ?? Array.Empty<Submesh>();

            int vertexCount = description.Vertices.Count;
            int indexCount = description.Indices.Count;

            int vertexBytes = vertexCount * sizeof(Vertex);
            byte[] vertexData = new byte[vertexBytes];
            fixed (byte* dst = vertexData)
            {
                Vertex* dstPtr = (Vertex*)dst;
                for (int i = 0; i < vertexCount; ++i)
                    dstPtr[i] = description.Vertices[i];
            }
            fixed (byte* dst = vertexData)
            {
                var vbDesc = new BufferDesc
                {
                    ByteWidth = (uint)vertexBytes,
                    Usage = D3D11Usage.Immutable,
                    BindFlags = (uint)BindFlag.VertexBuffer,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    StructureByteStride = 0,
                };
                var subresData = new SubresourceData
                {
                    PSysMem = dst,
                };
                ID3D11Buffer* buf;
                SilkMarshal.ThrowHResult(device.Device->CreateBuffer(&vbDesc, &subresData, &buf));
                _vertexBuffer = buf;
            }
            _vertexBufferBinding = new Dx11VertexBufferBinding(_vertexBuffer, sizeof(Vertex), 0);

            int indexBytes = indexCount * sizeof(int);
            byte[] indexData = new byte[indexBytes];
            fixed (byte* dst = indexData)
            {
                int* dstPtr = (int*)dst;
                for (int i = 0; i < indexCount; ++i)
                    dstPtr[i] = description.Indices[i];
            }
            fixed (byte* dst = indexData)
            {
                var ibDesc = new BufferDesc
                {
                    ByteWidth = (uint)indexBytes,
                    Usage = D3D11Usage.Immutable,
                    BindFlags = (uint)BindFlag.IndexBuffer,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    StructureByteStride = 0,
                };
                var subresData = new SubresourceData
                {
                    PSysMem = dst,
                };
                ID3D11Buffer* buf;
                SilkMarshal.ThrowHResult(device.Device->CreateBuffer(&ibDesc, &subresData, &buf));
                _indexBuffer = buf;
            }

            {
                var cbDesc = new BufferDesc
                {
                    ByteWidth = (uint)CbufferSize,
                    Usage = D3D11Usage.Default,
                    BindFlags = (uint)BindFlag.ConstantBuffer,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    StructureByteStride = 0,
                };
                ID3D11Buffer* buf;
                SilkMarshal.ThrowHResult(device.Device->CreateBuffer(&cbDesc, null, &buf));
                _cbuffer = buf;
            }
            Dx11RenderingDevice.SetDebugName((ID3D11DeviceChild*)_cbuffer, "DrawingImportedGeometry.MaterialData");
        }

        public override void Dispose()
        {
            _vertexBuffer->Release();
            _indexBuffer->Release();
            _cbuffer->Release();
        }

        public override void Render(RenderArgs arg)
        {
            if (_submeshes.Length == 0)
                return;

            var ctx = _device.Context;
            // RenderTarget == null means the caller has bound a custom RT (offscreen).
            if (arg.RenderTarget != null)
                ((Dx11RenderingSwapChain)arg.RenderTarget).Bind();

            _device.ImportedGeometryShader.Apply(ctx, arg.StateBuffer);
            { var b = _cbuffer; ctx->VSSetConstantBuffers(1, 1, &b); }
            { var b = _cbuffer; ctx->PSSetConstantBuffers(1, 1, &b); }
            { var ss = arg.BilinearFilter ? _device.SamplerDefault : _device.SamplerRoundToNearest; ctx->PSSetSamplers(0, 1, &ss); }
            {
                var bindings = new Dx11VertexBufferBinding[] { _vertexBufferBinding };
                Dx11RenderingDevice.SetVertexBuffers(ctx, 0, bindings);
            }
            ctx->IASetIndexBuffer(_indexBuffer, Format.FormatR32Uint, 0);

            CullMode lastCull = CullMode.Back;
            ID3D11BlendState* lastBlend = _device.BlendingPremultipliedAlpha;
            ctx->RSSetState(_device.RasterizerBackCulling);
            ctx->OMSetBlendState(lastBlend, null, 0xFFFFFFFF);

            foreach (var sub in _submeshes)
            {
                if (sub.IndexCount == 0)
                    continue;

                ID3D11ShaderResourceView* srv = ResolveTextureSrv(sub.Texture);

                // Update the per-submesh cbuffer — World/Tint repeat for every
                // submesh but the shader cost is negligible compared to a context
                // round-trip.
                ImportedGeometryDataLayout cb;
                cb.World = arg.World;
                cb.Tint = arg.Tint;
                cb.ReciprocalTextureSize = sub.TextureSize.X > 0 && sub.TextureSize.Y > 0
                    ? new Vector2(1.0f / sub.TextureSize.X, 1.0f / sub.TextureSize.Y)
                    : Vector2.One;
                cb.TextureEnabled = (srv != null) ? 1 : 0;
                cb.UseVertexColors = arg.UseVertexColors ? 1 : 0;
                cb.AlphaTest = arg.AlphaTest ? 1 : 0;
                ctx->UpdateSubresource((ID3D11Resource*)_cbuffer, 0, null, &cb, (uint)sizeof(ImportedGeometryDataLayout), 0);
                if (srv != null)
                { var s = srv; ctx->PSSetShaderResources(0, 1, &s); }

                CullMode wantCull = sub.DoubleSided ? CullMode.None : CullMode.Back;
                if (wantCull != lastCull)
                {
                    ctx->RSSetState(wantCull == CullMode.None
                        ? _device.RasterizerNoCull
                        : _device.RasterizerBackCulling);
                    lastCull = wantCull;
                }

                ID3D11BlendState* wantBlend = arg.ForceAdditive || sub.AdditiveBlending
                    ? _device.BlendingAdditive
                    : _device.BlendingPremultipliedAlpha;
                if (wantBlend != lastBlend)
                {
                    ctx->OMSetBlendState(wantBlend, null, 0xFFFFFFFF);
                    lastBlend = wantBlend;
                }

                ctx->DrawIndexed((uint)sub.IndexCount, (uint)sub.IndexStart, 0);
            }

            ctx->RSSetState(_device.RasterizerBackCulling);
            ctx->OMSetBlendState(_device.BlendingPremultipliedAlpha, null, 0xFFFFFFFF);
        }

        private static ID3D11ShaderResourceView* ResolveTextureSrv(object texture)
        {
            if (texture == null) return null;
            if (texture is nint ptr) return (ID3D11ShaderResourceView*)ptr;
            throw new ArgumentException("Texture must be an nint wrapping ID3D11ShaderResourceView*, got " + texture.GetType().Name);
        }
    }
}
