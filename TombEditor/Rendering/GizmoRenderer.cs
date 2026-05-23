using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TombLib.Graphics;
using TombLib.RenderingV2.Rhi;

namespace TombEditor.Rendering;

/// <summary>
/// Draws the translate / scale / rotate gizmo as world-space colored
/// geometry, using the RHI. Reads the gizmo state from a
/// <see cref="BaseGizmo"/> snapshot at the start of every <see cref="Render"/>
/// call — the actual picking + drag math still lives in BaseGizmo, this
/// class only handles the visuals.
///
/// <para>Geometry is rebuilt every frame into a single dynamic vertex
/// buffer. Gizmo geometry is tiny (a few thousand vertices) so the per-frame
/// upload is essentially free.</para>
/// </summary>
public sealed class GizmoRenderer : IDisposable
{
    private readonly IRhiDevice    _device;
    private readonly PipelineHandle _pipeline;
    private readonly BufferHandle   _viewCb;

    private BufferHandle _vb;
    private int          _vbCapacity;   // in vertices
    private byte[]       _vbCpu;

    private readonly BufferHandle[]        _scratchCbuf = new BufferHandle[1];
    private readonly VertexBufferBinding[] _scratchVbs  = new VertexBufferBinding[1];

    private const int AxisSegments   = 8;   // tube around translate arrow / scale cylinder
    private const int ConeSegments   = 16;  // translate cone
    private const int RingMajor      = 48;  // segments along the rotation torus loop
    private const int RingMinor      = 8;   // segments around the torus tube
    private const int PieSegments    = 64;  // rotation pie helper

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct GizmoVertex
    {
        public Vector3 Position;
        public uint    Color;   // RGBA8
    }
    private const int VertexStride = 16;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct ViewParams
    {
        public Matrix4x4 ViewProjection;
    }

    public GizmoRenderer(IRhiDevice device)
    {
        _device = device;

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
            Topology               = PrimitiveTopology.TriangleList,
            // Disable depth-test so the gizmo is always visible — it's an
            // editor overlay, even if the camera goes through a wall the
            // user should still see the handles for their selection.
            Rasterizer             = new RasterizerState(CullMode.None),
            DepthStencil           = DepthStencilState.Disabled,
            // Alpha-blend so the rotation pie (drawn at ~0.58 alpha) shows
            // through; opaque axes use alpha=255 and behave identically.
            BlendStates            = new[] { BlendState.AlphaBlend },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "GizmoPipeline",
        });

        _viewCb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: Unsafe.SizeOf<ViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "GizmoViewParams"),
            ReadOnlySpan<byte>.Empty);

        AllocateVb(8192);
    }

    private void AllocateVb(int vertexCapacity)
    {
        if (_vb.IsValid) _device.Destroy(_vb);
        _vbCapacity = vertexCapacity;
        _vbCpu = new byte[vertexCapacity * VertexStride];
        _vb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: vertexCapacity * VertexStride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "GizmoVertices"),
            ReadOnlySpan<byte>.Empty);
    }

    public void Render(ICommandList cl, in BaseGizmo.PublicState s, Matrix4x4 viewProjection)
    {
        if (!s.DrawGizmo) return;

        // Stage 1 — build geometry into _vbCpu.
        var span = MemoryMarshal.Cast<byte, GizmoVertex>(_vbCpu.AsSpan());
        int n = 0;
        BuildGeometry(s, span, ref n);
        if (n == 0) return;
        if (n > _vbCapacity)
        {
            // Resize then re-emit. We only get here once per (gizmo-config,
            // first-frame) pair — subsequent frames hit the same capacity.
            AllocateVb(Math.Max(n, _vbCapacity * 2));
            span = MemoryMarshal.Cast<byte, GizmoVertex>(_vbCpu.AsSpan());
            n = 0;
            BuildGeometry(s, span, ref n);
        }

        // Stage 2 — upload + bind + draw.
        cl.UpdateBuffer(_vb, 0, new ReadOnlySpan<byte>(_vbCpu, 0, n * VertexStride));

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
        cl.Draw(n);
    }

    public void Dispose()
    {
        if (_vb.IsValid)        _device.Destroy(_vb);
        if (_viewCb.IsValid)    _device.Destroy(_viewCb);
        if (_pipeline.IsValid)  _device.Destroy(_pipeline);
    }

    // =========================================================== Geometry

    // Colour palette: axis colour, plus a brighter version when active or
    // hovered. Last byte = A (255 = opaque).
    private const uint Cx = 0xFF_00_00_FFu; // R
    private const uint Cy = 0xFF_00_FF_00u; // G
    private const uint Cz = 0xFF_FF_00_00u; // B
    private const uint CxHi = 0xFF_60_60_FFu;
    private const uint CyHi = 0xFF_60_FF_60u;
    private const uint CzHi = 0xFF_FF_60_60u;

    // Pie colour: same axis hue, alpha ~0.58 (legacy _rotationAlpha).
    private const uint PieX = 0x94_00_00_FFu;
    private const uint PieY = 0x94_00_FF_00u;
    private const uint PieZ = 0x94_FF_00_00u;

    private static uint AxisColor(GizmoMode self, in BaseGizmo.PublicState s, uint normal, uint hi)
        => (s.ActiveMode == self || (s.ActiveMode == GizmoMode.None && s.HoveredMode == self)) ? hi : normal;

    private static void BuildGeometry(in BaseGizmo.PublicState s, Span<GizmoVertex> v, ref int n)
    {
        bool  upside    = s.Orientation == GizmoOrientation.UpsideDown;
        float arrowAxis = (upside ? -s.Size : s.Size);             // length of arrow shaft
        float coneTip   = arrowAxis * 1.13f;                       // where the cone apex lands
        float coneR     = s.TranslationConeSize;
        float axisR     = Math.Max(s.LineThickness, 1.0f);
        float scaleAxis = arrowAxis * 0.5f;                        // scale cylinder length
        float scaleR    = axisR * 1.1f;
        float scaleCube = s.ScaleCubeSize;

        // --- Rotation rings ------------------------------------------------
        // The legacy renderer pins the ring while a rotate drag is active
        // (uses the frozen matrix); we mirror that.
        if (s.SupportRotationY)
        {
            var m = s.ActiveMode == GizmoMode.RotateY ? s.FrozenRotateMatrixY : s.RotateMatrixY;
            // Around Y → loop in XZ plane.
            EmitRing(s.Position, axis: Vector3.UnitY, perpA: Vector3.UnitX, perpB: Vector3.UnitZ,
                     m, s.Size, s.LineThickness, AxisColor(GizmoMode.RotateY, s, Cy, CyHi), v, ref n);
        }
        if (s.SupportRotationX)
        {
            var m = s.ActiveMode == GizmoMode.RotateX ? s.FrozenRotateMatrixX : s.RotateMatrixX;
            // Around X → loop in YZ plane.
            EmitRing(s.Position, axis: Vector3.UnitX, perpA: Vector3.UnitY, perpB: Vector3.UnitZ,
                     m, s.Size, s.LineThickness, AxisColor(GizmoMode.RotateX, s, Cx, CxHi), v, ref n);
        }
        if (s.SupportRotationZ)
        {
            var m = s.ActiveMode == GizmoMode.RotateZ ? s.FrozenRotateMatrixZ : s.RotateMatrixZ;
            // Around Z → loop in XY plane.
            EmitRing(s.Position, axis: Vector3.UnitZ, perpA: Vector3.UnitX, perpB: Vector3.UnitY,
                     m, s.Size, s.LineThickness, AxisColor(GizmoMode.RotateZ, s, Cz, CzHi), v, ref n);
        }

        // --- Scale handles -------------------------------------------------
        // Cylinder + cube halfway down each axis. Picking math treats the
        // cube as the actual handle, the cylinder is just the visual stem.
        if (s.SupportScale)
        {
            EmitTube(s.Position, s.Position + Vector3.UnitX  * scaleAxis,
                     Vector3.UnitY, Vector3.UnitZ, scaleR, AxisSegments,
                     AxisColor(GizmoMode.ScaleX, s, Cx, CxHi), v, ref n);
            EmitCube(s.Position + Vector3.UnitX  * scaleAxis, scaleCube * 0.5f,
                     AxisColor(GizmoMode.ScaleX, s, Cx, CxHi), v, ref n);

            EmitTube(s.Position, s.Position + Vector3.UnitY  * scaleAxis,
                     Vector3.UnitX, Vector3.UnitZ, scaleR, AxisSegments,
                     AxisColor(GizmoMode.ScaleY, s, Cy, CyHi), v, ref n);
            EmitCube(s.Position + Vector3.UnitY  * scaleAxis, scaleCube * 0.5f,
                     AxisColor(GizmoMode.ScaleY, s, Cy, CyHi), v, ref n);

            // Z scale shifts in -Z to match the legacy picking (Position - UnitZ * scaleShift).
            EmitTube(s.Position, s.Position - Vector3.UnitZ  * scaleAxis,
                     Vector3.UnitX, Vector3.UnitY, scaleR, AxisSegments,
                     AxisColor(GizmoMode.ScaleZ, s, Cz, CzHi), v, ref n);
            EmitCube(s.Position - Vector3.UnitZ  * scaleAxis, scaleCube * 0.5f,
                     AxisColor(GizmoMode.ScaleZ, s, Cz, CzHi), v, ref n);
        }

        // --- Translate arrows ---------------------------------------------
        if (s.SupportTranslateX)
            EmitAxisAndCone(s.Position, Vector3.UnitX, arrowAxis, coneTip, axisR, coneR,
                            AxisColor(GizmoMode.TranslateX, s, Cx, CxHi), v, ref n);
        if (s.SupportTranslateY)
            EmitAxisAndCone(s.Position, Vector3.UnitY, arrowAxis, coneTip, axisR, coneR,
                            AxisColor(GizmoMode.TranslateY, s, Cy, CyHi), v, ref n);
        if (s.SupportTranslateZ)
            // TR Z axis points -Z in pick math (Position - UnitZ * arrowShift).
            EmitAxisAndCone(s.Position, -Vector3.UnitZ, arrowAxis, coneTip, axisR, coneR,
                            AxisColor(GizmoMode.TranslateZ, s, Cz, CzHi), v, ref n);

        // --- Centre cube (small white marker, not pickable) ---------------
        EmitCube(s.Position, s.CentreCubeSize, 0xFF_FF_FF_FFu, v, ref n);

        // --- Rotation pie helper (only while a rotate handle is dragged) --
        switch (s.ActiveMode)
        {
            case GizmoMode.RotateY:
                EmitRotationPie(s, Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ,
                                s.FrozenRotateMatrixY,
                                s.RotationPickAngle, s.RotationLastMouseAngle, PieY, v, ref n);
                break;
            case GizmoMode.RotateX:
                // Legacy: startAngle = -π/2 - rotationPickAngle, around X (loop in YZ).
                EmitRotationPie(s, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ,
                                s.FrozenRotateMatrixX,
                                -(float)Math.PI * 0.5f - s.RotationPickAngle,
                                -(float)Math.PI * 0.5f - s.RotationLastMouseAngle, PieX, v, ref n);
                break;
            case GizmoMode.RotateZ:
                EmitRotationPie(s, Vector3.UnitZ, Vector3.UnitX, Vector3.UnitY,
                                s.FrozenRotateMatrixZ,
                                (float)Math.PI + s.RotationPickAngle,
                                (float)Math.PI + s.RotationLastMouseAngle, PieZ, v, ref n);
                break;
        }
    }

    // ------------- Geometry primitives ------------------------------------

    // Translate arrow: cylinder shaft + cone cap.
    private static void EmitAxisAndCone(Vector3 origin, Vector3 dir,
                                         float axisLen, float coneApexAt,
                                         float axisRadius, float coneRadius,
                                         uint color,
                                         Span<GizmoVertex> v, ref int n)
    {
        // Orthonormal basis (dir, u, w) via Gram-Schmidt.
        Vector3 reference = Math.Abs(dir.Y) > 0.9f ? Vector3.UnitX : Vector3.UnitY;
        Vector3 u = Vector3.Normalize(Vector3.Cross(reference, dir));
        Vector3 w = Vector3.Cross(dir, u);

        Vector3 axisStart = origin;
        Vector3 axisEnd   = origin + dir * axisLen;
        EmitTube(axisStart, axisEnd, u, w, axisRadius, AxisSegments, color, v, ref n);

        Vector3 coneBase = axisEnd;
        Vector3 coneApex = origin + dir * coneApexAt;
        EmitCone(coneBase, coneApex, u, w, coneRadius, ConeSegments, color, v, ref n);
    }

    private static void EmitTube(Vector3 a, Vector3 b, Vector3 u, Vector3 w,
                                  float radius, int segments, uint color,
                                  Span<GizmoVertex> v, ref int n)
    {
        for (int i = 0; i < segments; i++)
        {
            float a0 = (float)(i       * (Math.PI * 2.0) / segments);
            float a1 = (float)((i + 1) * (Math.PI * 2.0) / segments);
            Vector3 off0 = (u * (float)Math.Cos(a0) + w * (float)Math.Sin(a0)) * radius;
            Vector3 off1 = (u * (float)Math.Cos(a1) + w * (float)Math.Sin(a1)) * radius;

            Push(a + off0, color, v, ref n);
            Push(a + off1, color, v, ref n);
            Push(b + off0, color, v, ref n);
            Push(b + off0, color, v, ref n);
            Push(a + off1, color, v, ref n);
            Push(b + off1, color, v, ref n);
        }
    }

    private static void EmitCone(Vector3 baseCentre, Vector3 apex,
                                  Vector3 u, Vector3 w, float radius, int segments,
                                  uint color, Span<GizmoVertex> v, ref int n)
    {
        for (int i = 0; i < segments; i++)
        {
            float a0 = (float)(i       * (Math.PI * 2.0) / segments);
            float a1 = (float)((i + 1) * (Math.PI * 2.0) / segments);
            Vector3 off0 = (u * (float)Math.Cos(a0) + w * (float)Math.Sin(a0)) * radius;
            Vector3 off1 = (u * (float)Math.Cos(a1) + w * (float)Math.Sin(a1)) * radius;

            Push(baseCentre + off0, color, v, ref n);
            Push(baseCentre + off1, color, v, ref n);
            Push(apex,                 color, v, ref n);

            Push(baseCentre,           color, v, ref n);
            Push(baseCentre + off1,    color, v, ref n);
            Push(baseCentre + off0,    color, v, ref n);
        }
    }

    private static void EmitCube(Vector3 centre, float halfExtent, uint color,
                                  Span<GizmoVertex> v, ref int n)
    {
        float h = halfExtent;
        Vector3 c000 = centre + new Vector3(-h, -h, -h);
        Vector3 c100 = centre + new Vector3(+h, -h, -h);
        Vector3 c010 = centre + new Vector3(-h, +h, -h);
        Vector3 c110 = centre + new Vector3(+h, +h, -h);
        Vector3 c001 = centre + new Vector3(-h, -h, +h);
        Vector3 c101 = centre + new Vector3(+h, -h, +h);
        Vector3 c011 = centre + new Vector3(-h, +h, +h);
        Vector3 c111 = centre + new Vector3(+h, +h, +h);

        // 6 faces × 2 tris. CullMode.None so winding is irrelevant.
        EmitQuad(c000, c010, c110, c100, color, v, ref n); // -Z
        EmitQuad(c001, c101, c111, c011, color, v, ref n); // +Z
        EmitQuad(c000, c100, c101, c001, color, v, ref n); // -Y
        EmitQuad(c010, c011, c111, c110, color, v, ref n); // +Y
        EmitQuad(c000, c001, c011, c010, color, v, ref n); // -X
        EmitQuad(c100, c110, c111, c101, color, v, ref n); // +X
    }

    private static void EmitQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d,
                                  uint color, Span<GizmoVertex> v, ref int n)
    {
        Push(a, color, v, ref n); Push(b, color, v, ref n); Push(c, color, v, ref n);
        Push(a, color, v, ref n); Push(c, color, v, ref n); Push(d, color, v, ref n);
    }

    // Torus ring around <axis>, of major radius <ringR> and tube radius
    // <tubeR>. The ring lies in the plane spanned by (perpA, perpB) in local
    // space, then is transformed by <rot> and translated by <origin>.
    private static void EmitRing(Vector3 origin, Vector3 axis, Vector3 perpA, Vector3 perpB,
                                  Matrix4x4 rot, float ringR, float tubeR, uint color,
                                  Span<GizmoVertex> v, ref int n)
    {
        // Pre-transform the basis vectors.
        Vector3 ax = Vector3.TransformNormal(axis,  rot);
        Vector3 pa = Vector3.TransformNormal(perpA, rot);
        Vector3 pb = Vector3.TransformNormal(perpB, rot);

        for (int i = 0; i < RingMajor; i++)
        {
            float ang0 = (float)(i       * (Math.PI * 2.0) / RingMajor);
            float ang1 = (float)((i + 1) * (Math.PI * 2.0) / RingMajor);
            Vector3 r0 = pa * (float)Math.Cos(ang0) + pb * (float)Math.Sin(ang0);
            Vector3 r1 = pa * (float)Math.Cos(ang1) + pb * (float)Math.Sin(ang1);
            Vector3 c0 = origin + r0 * ringR;
            Vector3 c1 = origin + r1 * ringR;

            for (int j = 0; j < RingMinor; j++)
            {
                float b0 = (float)(j       * (Math.PI * 2.0) / RingMinor);
                float b1 = (float)((j + 1) * (Math.PI * 2.0) / RingMinor);
                // Local tube basis: r is radial (outward), ax is along the rotation axis.
                Vector3 o00 = (r0 * (float)Math.Cos(b0) + ax * (float)Math.Sin(b0)) * tubeR;
                Vector3 o01 = (r0 * (float)Math.Cos(b1) + ax * (float)Math.Sin(b1)) * tubeR;
                Vector3 o10 = (r1 * (float)Math.Cos(b0) + ax * (float)Math.Sin(b0)) * tubeR;
                Vector3 o11 = (r1 * (float)Math.Cos(b1) + ax * (float)Math.Sin(b1)) * tubeR;

                Push(c0 + o00, color, v, ref n);
                Push(c0 + o01, color, v, ref n);
                Push(c1 + o10, color, v, ref n);
                Push(c1 + o10, color, v, ref n);
                Push(c0 + o01, color, v, ref n);
                Push(c1 + o11, color, v, ref n);
            }
        }
    }

    // Filled angular sector (the legacy "rotation helper geometry") + a thin
    // radial line to the cursor. Generated in the same local frame as the
    // matching ring so it sits flat on top of it.
    private static void EmitRotationPie(in BaseGizmo.PublicState s,
                                         Vector3 axisLocal, Vector3 perpALocal, Vector3 perpBLocal,
                                         Matrix4x4 frozenRot,
                                         float startAngle, float endAngle, uint color,
                                         Span<GizmoVertex> v, ref int n)
    {
        // Take the shorter arc (so a quick wrap-around doesn't fill 350°
        // instead of 10°) and order start < end.
        float shortest = endAngle - startAngle;
        shortest = (float)(shortest - Math.Round(shortest / (Math.PI * 2)) * (Math.PI * 2));
        endAngle = startAngle + shortest;
        if (startAngle > endAngle) (startAngle, endAngle) = (endAngle, startAngle);

        Vector3 pa = Vector3.TransformNormal(perpALocal, frozenRot);
        Vector3 pb = Vector3.TransformNormal(perpBLocal, frozenRot);

        float step = (endAngle - startAngle) / PieSegments;
        Vector3 prev = s.Position + (pa * (float)Math.Cos(startAngle) + pb * (float)-Math.Sin(startAngle)) * s.Size;
        for (int i = 0; i < PieSegments; i++)
        {
            float ang = startAngle + (i + 1) * step;
            Vector3 cur = s.Position + (pa * (float)Math.Cos(ang) + pb * (float)-Math.Sin(ang)) * s.Size;

            Push(s.Position, color, v, ref n);
            Push(prev,       color, v, ref n);
            Push(cur,        color, v, ref n);
            prev = cur;
        }

        // Thin radial spoke from origin to the current mouse position.
        // Drawn as a flat triangle pair around the ideal line, since this
        // pipeline only supports triangles.
        float mouseAngle = (float)((s.ActiveMode == GizmoMode.RotateY)
                                    ? s.RotationLastMouseAngle
                                    : (s.ActiveMode == GizmoMode.RotateX)
                                        ? -(Math.PI * 0.5) - s.RotationLastMouseAngle
                                        : Math.PI + s.RotationLastMouseAngle);
        Vector3 spokeDir   = pa * (float)Math.Cos(mouseAngle) + pb * (float)-Math.Sin(mouseAngle);
        Vector3 spokeTip   = s.Position + spokeDir * s.RotationLastMouseRadius;
        Vector3 spokeSide  = Vector3.Cross(spokeDir, Vector3.TransformNormal(axisLocal, frozenRot));
        if (spokeSide.LengthSquared() > 1e-6f)
            spokeSide = Vector3.Normalize(spokeSide) * (s.LineThickness * 0.5f);
        const uint spokeColor = 0xFF_FF_FF_FFu;
        Push(s.Position + spokeSide, spokeColor, v, ref n);
        Push(s.Position - spokeSide, spokeColor, v, ref n);
        Push(spokeTip   + spokeSide, spokeColor, v, ref n);
        Push(spokeTip   + spokeSide, spokeColor, v, ref n);
        Push(s.Position - spokeSide, spokeColor, v, ref n);
        Push(spokeTip   - spokeSide, spokeColor, v, ref n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Push(Vector3 p, uint color, Span<GizmoVertex> v, ref int n)
    {
        if (n >= v.Length) return; // overflow guard — Render() resizes on next pass
        v[n].Position = p;
        v[n].Color    = color;
        n++;
    }
}
