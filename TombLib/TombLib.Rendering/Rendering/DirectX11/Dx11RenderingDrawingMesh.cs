using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector4 = System.Numerics.Vector4;

namespace TombLib.Rendering.DirectX11
{
    // D3D11 implementation of RenderingDrawingMesh.
    //
    // Owns IMMUTABLE vertex + index buffers built once from Description (mirrors the
    // legacy Mesh<ObjectVertex>.UpdateBuffers usage pattern). Per-batch shader data
    // (World, Tint, Bones[32], flags) lives in a small Default-usage cbuffer updated
    // on every Render via UpdateSubresource.
    //
    // Atlas binding: the caller passes the Texture2DArray's ShaderResourceView via
    // RenderArgs.Atlas (typed `object` because the abstraction is backend-agnostic;
    // we cast here). To accommodate the existing legacy pipeline that exposes textures
    // via SharpDX.Toolkit.Graphics.Texture2D, this impl also accepts a Texture2D and
    // extracts the SRV on demand.
    public sealed class Dx11RenderingDrawingMesh : RenderingDrawingMesh
    {
        // Layout MUST mirror the MeshData cbuffer in MeshShaderVS.hlsl/PS.hlsl.
        // Total size includes 32 bones × 64 bytes = 2048 bytes plus the 16-byte
        // header plus the 32-byte footer of flags = 2160, padded to 16-byte alignment.
        // 32 bones is the legacy MAX_BONES from Model.fx.
        private const int MaxBones = 32;
        [StructLayout(LayoutKind.Explicit, Size = 16 + 16 + 64 + MaxBones * 64 + 16)]
        private struct MeshDataLayout
        {
            // World (matrix, 64B)
            [FieldOffset(0)]   public Matrix4x4 World;
            // Tint (vec4, 16B)
            [FieldOffset(64)]  public Vector4 Tint;
            // Bones[MAX_BONES] (matrix array, 32 × 64 = 2048B)
            // Manipulated as a span of Matrix4x4 via SetBones() to keep the marshalling
            // explicit; FieldOffset(80) is just informational here.
            // Flags (4 ints, 16B): Skinned, StaticLighting, ColoredVertices, AlphaTest
            [FieldOffset(80 + MaxBones * 64)] public int Skinned;
            [FieldOffset(84 + MaxBones * 64)] public int StaticLighting;
            [FieldOffset(88 + MaxBones * 64)] public int ColoredVertices;
            [FieldOffset(92 + MaxBones * 64)] public int AlphaTest;
        }
        // Total buffer bytes including bones, padded to 16-byte multiple.
        private static readonly int MeshDataSize = ((Marshal.SizeOf<MeshDataLayout>() + 15) / 16) * 16;

        private readonly Dx11RenderingDevice _device;
        private readonly Buffer _vertexBuffer;
        private readonly Buffer _indexBuffer;
        private readonly Buffer _meshDataBuffer;
        private readonly VertexBufferBinding _vertexBufferBinding;
        private readonly Submesh[] _submeshes;
        // Heap-allocated staging buffer for the cbuffer upload. Reused across frames.
        private readonly byte[] _cbufferStaging = new byte[MeshDataSize];

        public unsafe Dx11RenderingDrawingMesh(Dx11RenderingDevice device, Description description)
        {
            _device = device;

            int vertexCount = description.Vertices.Count;
            int indexCount = description.Indices.Count;
            _submeshes = description.Submeshes?.ToArray() ?? Array.Empty<Submesh>();

            // Vertices: pack into a contiguous byte array and create an immutable VB.
            int vertexBytes = vertexCount * sizeof(MeshVertex);
            byte[] vertexData = new byte[vertexBytes];
            fixed (byte* dst = vertexData)
            {
                MeshVertex* dstPtr = (MeshVertex*)dst;
                for (int i = 0; i < vertexCount; ++i)
                    dstPtr[i] = description.Vertices[i];
            }
            fixed (byte* dst = vertexData)
            {
                _vertexBuffer = new Buffer(device.Device, new IntPtr(dst),
                    new BufferDescription(vertexBytes, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                    CpuAccessFlags.None, ResourceOptionFlags.None, 0));
            }
            _vertexBufferBinding = new VertexBufferBinding(_vertexBuffer, sizeof(MeshVertex), 0);

            // Indices: 32-bit (matches the legacy Mesh<T>.Indices = List<int>).
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

            _meshDataBuffer = new Buffer(device.Device, MeshDataSize, ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _meshDataBuffer.SetDebugName("DrawingMesh.MeshData");
        }

        public override void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _meshDataBuffer.Dispose();
        }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_submeshes.Length == 0)
                return;

            var ctx = _device.Context;
            // RenderTarget == null means the caller has already bound a custom render
            // target (e.g. OffscreenItemRenderer for thumbnails). We skip Bind() so we
            // don't overwrite their setup.
            if (arg.RenderTarget != null)
                ((Dx11RenderingSwapChain)arg.RenderTarget).Bind();

            // Resolve atlas SRV from the opaque object passed in RenderArgs.
            ShaderResourceView srv = ResolveAtlasSrv(arg.Atlas);
            if (srv == null)
                return; // No texture → nothing to draw

            // Build cbuffer payload in the staging array.
            fixed (byte* dst = _cbufferStaging)
            {
                // Header: World (offset 0..63), Tint (64..79)
                Matrix4x4* worldPtr = (Matrix4x4*)(dst + 0);
                worldPtr[0] = arg.World;
                Vector4* tintPtr = (Vector4*)(dst + 64);
                tintPtr[0] = arg.Tint;

                // Bones[MAX_BONES] starting at offset 80. If BoneMatrices is shorter than
                // MAX_BONES, the unused tail is zero-initialised — the shader skips the
                // skinning math when Skinned=0 anyway, so the unused bones don't affect
                // output even when the array is filled with garbage from a previous draw.
                Matrix4x4* bonesPtr = (Matrix4x4*)(dst + 80);
                int boneCount = arg.BoneMatrices?.Length ?? 0;
                int useBones = Math.Min(boneCount, MaxBones);
                for (int i = 0; i < useBones; ++i)
                    bonesPtr[i] = arg.BoneMatrices[i];
                for (int i = useBones; i < MaxBones; ++i)
                    bonesPtr[i] = Matrix4x4.Identity;

                // Footer flags
                int* flagsPtr = (int*)(dst + 80 + MaxBones * 64);
                flagsPtr[0] = arg.Skinned ? 1 : 0;
                flagsPtr[1] = arg.StaticLighting ? 1 : 0;
                flagsPtr[2] = arg.ColoredVertices ? 1 : 0;
                flagsPtr[3] = arg.AlphaTest ? 1 : 0;

                ctx.UpdateSubresource(new DataBox(new IntPtr(dst), MeshDataSize, 0), _meshDataBuffer);
            }

            // Bind shader, IA, sampler, atlas SRV.
            _device.MeshShader.Apply(ctx, arg.StateBuffer);
            ctx.VertexShader.SetConstantBuffer(1, _meshDataBuffer);
            ctx.PixelShader.SetConstantBuffer(1, _meshDataBuffer);
            ctx.PixelShader.SetSampler(0, arg.BilinearFilter ? _device.SamplerDefault : _device.SamplerRoundToNearest);
            ctx.PixelShader.SetShaderResources(0, srv);
            ctx.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);
            ctx.InputAssembler.SetIndexBuffer(_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            // Submit each submesh as a separate draw to honour per-submesh material
            // (DoubleSided / AdditiveBlending). Cull / blend state changes are minimal —
            // we only re-set when the value actually differs from the last draw.
            CullMode lastCull = CullMode.Back;
            BlendState lastBlend = _device.BlendingPremultipliedAlpha;
            ctx.Rasterizer.State = _device.RasterizerBackCulling;
            ctx.OutputMerger.SetBlendState(lastBlend);

            foreach (var sub in _submeshes)
            {
                if (sub.IndexCount == 0)
                    continue;

                CullMode wantCull = sub.DoubleSided ? CullMode.None : CullMode.Back;
                if (wantCull != lastCull)
                {
                    ctx.Rasterizer.State = wantCull == CullMode.None
                        ? _device.RasterizerNoCull
                        : _device.RasterizerBackCulling;
                    lastCull = wantCull;
                }

                BlendState wantBlend = sub.AdditiveBlending
                    ? _device.BlendingAdditive
                    : _device.BlendingPremultipliedAlpha;
                if (wantBlend != lastBlend)
                {
                    ctx.OutputMerger.SetBlendState(wantBlend);
                    lastBlend = wantBlend;
                }

                ctx.DrawIndexed(sub.IndexCount, sub.IndexStart, 0);
            }

            // Restore device-wide defaults so downstream code does not inherit our
            // overrides (mirrors what Dx11RenderingDrawingLines does).
            ctx.Rasterizer.State = _device.RasterizerBackCulling;
            ctx.OutputMerger.SetBlendState(_device.BlendingPremultipliedAlpha);
        }

        // The abstraction passes the atlas as an opaque `object` (the abstraction
        // itself is backend-agnostic). The D3D11 backend only knows how to bind a
        // SharpDX.Direct3D11.ShaderResourceView; null disables texturing.
        private static ShaderResourceView ResolveAtlasSrv(object atlas)
        {
            if (atlas == null)
                return null;
            if (atlas is ShaderResourceView srv)
                return srv;
            throw new ArgumentException("Atlas must be a SharpDX.Direct3D11.ShaderResourceView, got " + atlas.GetType().Name);
        }
    }
}
