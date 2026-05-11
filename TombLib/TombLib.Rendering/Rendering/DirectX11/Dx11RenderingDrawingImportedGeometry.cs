using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
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
    public sealed class Dx11RenderingDrawingImportedGeometry : RenderingDrawingImportedGeometry
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
        private readonly Buffer _vertexBuffer;
        private readonly Buffer _indexBuffer;
        private readonly Buffer _cbuffer;
        private readonly VertexBufferBinding _vertexBufferBinding;
        private readonly Submesh[] _submeshes;

        public unsafe Dx11RenderingDrawingImportedGeometry(Dx11RenderingDevice device, Description description)
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
                _vertexBuffer = new Buffer(device.Device, new IntPtr(dst),
                    new BufferDescription(vertexBytes, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                    CpuAccessFlags.None, ResourceOptionFlags.None, 0));
            }
            _vertexBufferBinding = new VertexBufferBinding(_vertexBuffer, sizeof(Vertex), 0);

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
                _indexBuffer = new Buffer(device.Device, new IntPtr(dst),
                    new BufferDescription(indexBytes, ResourceUsage.Immutable, BindFlags.IndexBuffer,
                    CpuAccessFlags.None, ResourceOptionFlags.None, 0));
            }

            _cbuffer = new Buffer(device.Device, CbufferSize, ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _cbuffer.SetDebugName("DrawingImportedGeometry.MaterialData");
        }

        public override void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _cbuffer.Dispose();
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
            ctx.VertexShader.SetConstantBuffer(1, _cbuffer);
            ctx.PixelShader.SetConstantBuffer(1, _cbuffer);
            ctx.PixelShader.SetSampler(0, arg.BilinearFilter ? _device.SamplerDefault : _device.SamplerRoundToNearest);
            ctx.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);
            ctx.InputAssembler.SetIndexBuffer(_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            CullMode lastCull = CullMode.Back;
            BlendState lastBlend = _device.BlendingPremultipliedAlpha;
            ctx.Rasterizer.State = _device.RasterizerBackCulling;
            ctx.OutputMerger.SetBlendState(lastBlend);

            foreach (var sub in _submeshes)
            {
                if (sub.IndexCount == 0)
                    continue;

                ShaderResourceView srv = ResolveTextureSrv(sub.Texture);

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
                ctx.UpdateSubresource(ref cb, _cbuffer);
                ctx.PixelShader.SetShaderResources(0, srv);

                CullMode wantCull = sub.DoubleSided ? CullMode.None : CullMode.Back;
                if (wantCull != lastCull)
                {
                    ctx.Rasterizer.State = wantCull == CullMode.None
                        ? _device.RasterizerNoCull
                        : _device.RasterizerBackCulling;
                    lastCull = wantCull;
                }

                BlendState wantBlend = arg.ForceAdditive || sub.AdditiveBlending
                    ? _device.BlendingAdditive
                    : _device.BlendingPremultipliedAlpha;
                if (wantBlend != lastBlend)
                {
                    ctx.OutputMerger.SetBlendState(wantBlend);
                    lastBlend = wantBlend;
                }

                ctx.DrawIndexed(sub.IndexCount, sub.IndexStart, 0);
            }

            ctx.Rasterizer.State = _device.RasterizerBackCulling;
            ctx.OutputMerger.SetBlendState(_device.BlendingPremultipliedAlpha);
        }

        private static ShaderResourceView ResolveTextureSrv(object texture)
        {
            if (texture == null) return null;
            if (texture is ShaderResourceView srv) return srv;
            throw new ArgumentException("Texture must be a SharpDX.Direct3D11.ShaderResourceView, got " + texture.GetType().Name);
        }
    }
}
