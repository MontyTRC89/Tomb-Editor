using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace TombLib.Rendering.DirectX11
{
    // D3D11 implementation of RenderingDrawingLines.
    //
    // Stores nothing on the GPU between SetVertices() calls when Description.Dynamic
    // (the default): the actual vertex bytes are uploaded via the device's shared
    // dynamic VB pool inside Render(). Local state is just:
    //   - the cached managed copy of the latest vertices (CPU-side scratch),
    //   - a small per-batch constant buffer for World+Tint (slot b1).
    //
    // The non-Dynamic path is reserved for the migration of legacy callers that own
    // a long-lived VB (e.g. _linesCube, _sphere). It is not implemented yet — every
    // current call site rebuilds geometry per frame anyway, so dynamic suffices.
    //
    // Memory layout in the dynamic slice
    // ----------------------------------
    // Strict SoA: positions then colors, contiguous. The IL has POSITION on slot 0
    // and COLOR on slot 1; we bind the same physical buffer twice with appropriate
    // offsets and per-attribute strides. Doing SoA (rather than packing the C#
    // SolidLineVertex struct) sidesteps any struct-padding mismatch between the
    // managed layout and what D3D expects.
    public sealed class Dx11RenderingDrawingLines : RenderingDrawingLines
    {
        // Layout MUST mirror the LineData cbuffer in LinesShaderVS.hlsl. Padded to
        // 16-byte alignment per HLSL packing rules.
        [StructLayout(LayoutKind.Explicit)]
        private struct LineDataLayout
        {
            [FieldOffset(0)]  public Matrix4x4 World;
            [FieldOffset(64)] public Vector4 Tint;
        }
        private static readonly int LineDataSize = ((Marshal.SizeOf(typeof(LineDataLayout)) + 15) / 16) * 16;

        private readonly Dx11RenderingDevice _device;
        private readonly Buffer _lineDataBuffer;

        // Cached vertices from the most recent SetVertices() call. Re-uploaded to the
        // ring on every Render() — cheap because it's a single MapSubresource into
        // a region the GPU isn't reading.
        private SolidLineVertex[] _vertices = Array.Empty<SolidLineVertex>();
        private int _vertexCount;

        public Dx11RenderingDrawingLines(Dx11RenderingDevice device, Description description)
        {
            _device = device;
            _lineDataBuffer = new Buffer(device.Device, LineDataSize, ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _lineDataBuffer.SetDebugName("DrawingLines.LineData");
        }

        public override void Dispose()
        {
            _lineDataBuffer.Dispose();
        }

        public override void SetVertices(ReadOnlySpan<SolidLineVertex> vertices)
        {
            // Grow the scratch array geometrically. Keeping the storage means a steady
            // stream of equal-sized SetVertices() calls allocates exactly once.
            if (vertices.Length > _vertices.Length)
                _vertices = new SolidLineVertex[Math.Max(vertices.Length, _vertices.Length * 2)];
            vertices.CopyTo(_vertices);
            _vertexCount = vertices.Length;
        }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_vertexCount == 0)
                return;

            var ctx = _device.Context;
            ((Dx11RenderingSwapChain)arg.RenderTarget).Bind();

            // Upload per-batch constants (slot b1). The state buffer (slot b0 with the
            // view-projection) is set by the shared PipelineState.Apply call below.
            LineDataLayout cb;
            cb.World = arg.World;
            cb.Tint = arg.Tint;
            ctx.UpdateSubresource(ref cb, _lineDataBuffer);

            // Reserve a single contiguous region in the ring buffer big enough for both
            // streams (positions then colors), then write them in SoA order.
            int posBytes = _vertexCount * sizeof(Vector3);
            int colBytes = _vertexCount * sizeof(Vector4);
            int totalBytes = posBytes + colBytes;
            var slice = _device.DynamicVertexBuffers.Allocate(totalBytes);

            byte* dst = (byte*)slice.Data;
            Vector3* posPtr = (Vector3*)dst;
            Vector4* colPtr = (Vector4*)(dst + posBytes);
            for (int i = 0; i < _vertexCount; ++i)
            {
                posPtr[i] = _vertices[i].Position;
                colPtr[i] = _vertices[i].Color;
            }

            Buffer vb = slice.Finish();
            var bindings = new VertexBufferBinding[] {
                new VertexBufferBinding(vb, sizeof(Vector3), slice.Offset),
                new VertexBufferBinding(vb, sizeof(Vector4), slice.Offset + posBytes) };

            _device.LinesShader.Apply(ctx, arg.StateBuffer);
            ctx.VertexShader.SetConstantBuffer(1, _lineDataBuffer);

            // PrimitiveTopology must be set AFTER PipelineState.Apply (which forces
            // TriangleList) so our LineList override sticks.
            ctx.InputAssembler.PrimitiveTopology = arg.Topology == Topology.TriangleList
                ? PrimitiveTopology.TriangleList
                : PrimitiveTopology.LineList;
            ctx.InputAssembler.SetVertexBuffers(0, bindings);

            ctx.Rasterizer.State = arg.Wireframe ? _device.RasterizerWireframe : _device.RasterizerNoCull;

            // Apply blend + depth state per RenderArgs. Defaults match the device-wide
            // state so callers that don't set them get current behaviour.
            ctx.OutputMerger.SetBlendState(arg.Blend switch
            {
                BlendMode.Opaque                 => _device.BlendingDisabled,
                BlendMode.NonPremultipliedAlpha  => _device.BlendingNonPremultipliedAlpha,
                BlendMode.Additive               => _device.BlendingAdditive,
                _                                => _device.BlendingPremultipliedAlpha,
            });
            ctx.OutputMerger.SetDepthStencilState(arg.Depth switch
            {
                DepthMode.DepthRead => _device.DepthStencilDepthRead,
                DepthMode.NoZ       => _device.DepthStencilNoZBuffer,
                _                   => _device.DepthStencilDefault,
            });

            ctx.Draw(_vertexCount, 0);

            // Restore the device-wide default rasterizer/blend/depth so downstream code
            // (room rendering, sprites, etc.) does not inherit our overrides.
            ctx.Rasterizer.State = _device.RasterizerBackCulling;
            ctx.OutputMerger.SetBlendState(_device.BlendingPremultipliedAlpha);
            ctx.OutputMerger.SetDepthStencilState(_device.DepthStencilDefault);
            ctx.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            if (slice.Oversized != null) vb.Dispose();
        }
    }
}
