#nullable enable
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TombLib.Graphics;
using TombLib.RenderingV2.Rhi;

namespace TombLib.RenderingV2.Preview;

/// <summary>
/// Minimal line-list renderer for editor wireframe overlays — grids,
/// bounding boxes, debug helpers. Reuses the Gizmo shader (Position +
/// packed RGBA) with a <see cref="PrimitiveTopology.LineList"/> pipeline.
///
/// <para>Geometry is uploaded into a single dynamic VB on every
/// <c>Flush</c>; depth-test is disabled so overlays stay visible behind
/// surfaces, mirroring the legacy wireframe drawing.</para>
/// </summary>
public sealed class LinePrimitiveRenderer : IDisposable
{
    private readonly IRhiDevice     _device;
    private readonly PipelineHandle _pipeline;
    private readonly BufferHandle   _viewCb;

    private BufferHandle _vb;
    private int          _vbCapacityBytes;
    private byte[]       _vbCpu;
    private int          _vertexCount;

    private readonly BufferHandle[]        _scratchCbuf = new BufferHandle[1];
    private readonly VertexBufferBinding[] _scratchVbs  = new VertexBufferBinding[1];

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LineVertex
    {
        public Vector3 Position;
        public uint    Color;
    }
    private const int VertexStride = 16;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct ViewParams
    {
        public Matrix4x4 ViewProjection;
    }

    public LinePrimitiveRenderer(IRhiDevice device)
    {
        _device = device;

        // Reuse the Gizmo shader — same vertex format (Position + Color),
        // same ViewProjection constant buffer. We only need to swap the
        // topology to LineList.
        var (vs, ps) = ShaderLibrary.Load("Gizmo");
        _pipeline = device.CreatePipeline(new PipelineDesc
        {
            VertexShader   = vs,
            FragmentShader = ps,
            VertexAttributes = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float, bufferSlot: 0, offset: 0),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,  bufferSlot: 0, offset: 12),
            },
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: VertexStride) },
            Topology               = PrimitiveTopology.LineList,
            Rasterizer             = new RasterizerState(CullMode.None),
            // Wireframe overlays draw on top — depth-write off keeps the
            // colour without occluding solid geometry behind it.
            DepthStencil           = DepthStencilState.DepthReadOnly,
            BlendStates            = new[] { BlendState.AlphaBlend },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "LinePrimitivePipeline",
        });

        _viewCb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: Unsafe.SizeOf<ViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "LinePrimitiveViewParams"),
            ReadOnlySpan<byte>.Empty);

        AllocateVb(8192);
    }

    private void AllocateVb(int vertexCapacity)
    {
        if (_vb.IsValid) _device.Destroy(_vb);
        _vbCapacityBytes = vertexCapacity * VertexStride;
        _vbCpu = new byte[_vbCapacityBytes];
        _vb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: _vbCapacityBytes,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "LinePrimitiveVertices"),
            ReadOnlySpan<byte>.Empty);
    }

    /// <summary>Drop all buffered vertices — start a new frame's batch.</summary>
    public void Begin() => _vertexCount = 0;

    /// <summary>Emit one line segment (two vertices) into the current batch.</summary>
    public void AddLine(Vector3 a, Vector3 b, uint colorA, uint colorB)
    {
        EnsureCapacity(_vertexCount + 2);
        var span = MemoryMarshal.Cast<byte, LineVertex>(_vbCpu.AsSpan());
        span[_vertexCount].Position = a; span[_vertexCount].Color = colorA;
        span[_vertexCount + 1].Position = b; span[_vertexCount + 1].Color = colorB;
        _vertexCount += 2;
    }

    /// <summary>Emit a 3D axis-aligned bounding box (12 segments).</summary>
    public void AddBox(BoundingBox box, Matrix4x4 transform, uint color)
    {
        Vector3 min = box.Minimum, max = box.Maximum;
        Vector3 c000 = Vector3.Transform(new Vector3(min.X, min.Y, min.Z), transform);
        Vector3 c100 = Vector3.Transform(new Vector3(max.X, min.Y, min.Z), transform);
        Vector3 c010 = Vector3.Transform(new Vector3(min.X, max.Y, min.Z), transform);
        Vector3 c110 = Vector3.Transform(new Vector3(max.X, max.Y, min.Z), transform);
        Vector3 c001 = Vector3.Transform(new Vector3(min.X, min.Y, max.Z), transform);
        Vector3 c101 = Vector3.Transform(new Vector3(max.X, min.Y, max.Z), transform);
        Vector3 c011 = Vector3.Transform(new Vector3(min.X, max.Y, max.Z), transform);
        Vector3 c111 = Vector3.Transform(new Vector3(max.X, max.Y, max.Z), transform);

        // Bottom rect.
        AddLine(c000, c100, color, color);
        AddLine(c100, c101, color, color);
        AddLine(c101, c001, color, color);
        AddLine(c001, c000, color, color);
        // Top rect.
        AddLine(c010, c110, color, color);
        AddLine(c110, c111, color, color);
        AddLine(c111, c011, color, color);
        AddLine(c011, c010, color, color);
        // Pillars.
        AddLine(c000, c010, color, color);
        AddLine(c100, c110, color, color);
        AddLine(c001, c011, color, color);
        AddLine(c101, c111, color, color);
    }

    /// <summary>
    /// Emit a wireframe sphere as three axis-aligned great circles
    /// (XY, YZ, XZ planes), <paramref name="segments"/> segments each. Approximates
    /// the legacy <c>GeometricPrimitive.Sphere</c> rendered with the wireframe
    /// rasterizer state — cheaper than full triangle wireframe + matches
    /// the editor's visual style for light/radius indicators.
    /// </summary>
    public void AddWireSphere(Vector3 center, float radius, int segments, uint color)
    {
        if (segments < 3 || radius <= 0f) return;
        float step = (float)(Math.PI * 2.0) / segments;
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * step;
            float a1 = (i + 1) * step;
            float c0 = (float)Math.Cos(a0), s0 = (float)Math.Sin(a0);
            float c1 = (float)Math.Cos(a1), s1 = (float)Math.Sin(a1);

            // XY plane (Z=0).
            AddLine(center + new Vector3(c0 * radius, s0 * radius, 0f),
                    center + new Vector3(c1 * radius, s1 * radius, 0f), color, color);
            // XZ plane (Y=0).
            AddLine(center + new Vector3(c0 * radius, 0f, s0 * radius),
                    center + new Vector3(c1 * radius, 0f, s1 * radius), color, color);
            // YZ plane (X=0).
            AddLine(center + new Vector3(0f, c0 * radius, s0 * radius),
                    center + new Vector3(0f, c1 * radius, s1 * radius), color, color);
        }
    }

    /// <summary>
    /// Emit an axis-aligned grid of <paramref name="cells"/> cells per side,
    /// total extent <paramref name="size"/>, centred on the origin, lying in
    /// the XZ plane. Matches the legacy editor's white reference grid.
    /// </summary>
    public void AddGridXZ(float size, int cells, uint color)
    {
        if (cells <= 0) return;
        float half = size * 0.5f;
        float step = size / cells;
        for (int i = 0; i <= cells; i++)
        {
            float x = -half + i * step;
            AddLine(new Vector3(x, 0f, -half), new Vector3(x, 0f, half), color, color);
            float z = -half + i * step;
            AddLine(new Vector3(-half, 0f, z), new Vector3(half, 0f, z), color, color);
        }
    }

    /// <summary>Upload + draw everything queued since <see cref="Begin"/>.</summary>
    public void Flush(ICommandList cl, Matrix4x4 viewProjection)
    {
        if (_vertexCount == 0) return;

        cl.UpdateBuffer(_vb, 0, new ReadOnlySpan<byte>(_vbCpu, 0, _vertexCount * VertexStride));

        var vp = new ViewParams { ViewProjection = viewProjection };
        unsafe
        {
            var vpSpan = new ReadOnlySpan<byte>(&vp, sizeof(ViewParams));
            cl.UpdateBuffer(_viewCb, 0, vpSpan);
        }

        cl.SetPipeline(_pipeline);
        _scratchCbuf[0] = _viewCb;
        cl.SetBindings(new Bindings { ConstantBuffers = _scratchCbuf });
        _scratchVbs[0] = new VertexBufferBinding(_vb, 0);
        cl.SetVertexBuffers(_scratchVbs);
        cl.Draw(_vertexCount);
    }

    private void EnsureCapacity(int needVertices)
    {
        int needBytes = needVertices * VertexStride;
        if (needBytes <= _vbCapacityBytes) return;
        int newCap = _vbCapacityBytes / VertexStride;
        while (newCap < needVertices) newCap *= 2;
        // Preserve existing buffered vertices on resize.
        var old = _vbCpu;
        AllocateVb(newCap);
        Buffer.BlockCopy(old, 0, _vbCpu, 0, _vertexCount * VertexStride);
    }

    public void Dispose()
    {
        if (_vb.IsValid)        _device.Destroy(_vb);
        if (_viewCb.IsValid)    _device.Destroy(_viewCb);
        if (_pipeline.IsValid)  _device.Destroy(_pipeline);
    }
}
