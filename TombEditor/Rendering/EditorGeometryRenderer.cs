using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.RenderingV2.Rhi;
using TombLib.Utils;
using TombEditor.Controls.FlybyTimeline.Sequence;

namespace TombEditor.Rendering;

/// <summary>
/// Draws the editor's wireframe overlay geometry — the level-scene elements
/// the legacy renderer drew that are not actual textured meshes: room
/// bounding boxes, object bounding boxes, volumes (box / sphere), ghost
/// blocks, flyby-camera paths and the selected object's height line.
///
/// <para>Everything is emitted as world-space coloured line segments into a
/// single dynamic vertex buffer and drawn with one LineList pipeline — the
/// same shader the gizmo / light-volume passes use.</para>
/// </summary>
internal sealed class EditorGeometryRenderer : IDisposable
{
    private readonly IRhiDevice     _device;
    private readonly PipelineHandle _pipeline;     // LineList — wireframe overlays
    private readonly PipelineHandle _triPipeline;  // TriangleList — flyby path tube + cones
    private readonly BufferHandle   _viewCb;

    private BufferHandle _vb;
    private int          _vbCapacity;
    private byte[]       _vbCpu;

    private BufferHandle _triVb;
    private int          _triCapacity;
    private byte[]       _triCpu;

    private readonly BufferHandle[]        _scratchCbuf = new BufferHandle[1];
    private readonly VertexBufferBinding[] _scratchVbs  = new VertexBufferBinding[1];

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LineVertex
    {
        public Vector3 Position;
        public uint    Color;   // RGBA8
    }
    private const int Stride = 16;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct ViewParams
    {
        public Matrix4x4 ViewProjection;
    }

    // Colour palette (RGBA8). Matches the legacy renderer's at-a-glance hues.
    private const uint RoomBoundsColor = 0xFF_C8_C8_C8u; // light grey
    private const uint GhostColor      = 0xFF_40_D0_FFu;  // cyan
    private const uint BBoxColor       = 0xFF_30_FF_30u;  // green
    private const uint HeightColor     = 0xFF_FF_FF_FFu;  // white
    private const uint BrushColor      = 0xFF_00_FF_FFu;  // yellow
    private const uint SplitColor      = 0xFF_00_A0_FFu;  // orange
    // Volume state colours derived from the editor's configured ColorTrigger,
    // matching the legacy DrawVolumes formula:
    //   normal   = rgb × 0.6, α 0.55
    //   selected = rgb,        α 0.70
    //   disabled = luma of the equivalent tone, α 0.55
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal static uint VolumeFillColor(in Vector4 baseColor, bool enabled, bool selected)
    {
        var rgb = new Vector3(baseColor.X, baseColor.Y, baseColor.Z);
        if (!enabled)
        {
            var tone = selected ? rgb : rgb * 0.6f;
            float l = tone.GetLuma();
            return PackRgba(new Vector4(l, l, l, 0.55f));
        }
        return selected
            ? PackRgba(new Vector4(rgb,        0.70f))
            : PackRgba(new Vector4(rgb * 0.6f, 0.55f));
    }

    // Wireframe outline overlaid on the solid volume — half-bright, half-alpha
    // (legacy d=1 pass: new Vector4(color.To3() * 0.5f, 0.5f)).
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal static uint VolumeWireColor(in Vector4 baseColor, bool enabled, bool selected)
    {
        uint fill = VolumeFillColor(baseColor, enabled, selected);
        byte r = (byte)(((fill >> 0)  & 0xFF) >> 1);
        byte g = (byte)(((fill >> 8)  & 0xFF) >> 1);
        byte b = (byte)(((fill >> 16) & 0xFF) >> 1);
        return r | ((uint)g << 8) | ((uint)b << 16) | (0x80u << 24);
    }

    // Ghost-block volume tessellation reused for sphere volumes.
    private const int SphereSegments = 24;

    // Flyby path — solid Catmull-Rom tube (legacy values).
    private const float FlybyPathThickness  = 32f;
    private const int   FlybyPathSmoothness = 7;
    // Flyby direction cone.
    private const float ConeLength   = 512f;
    private const float ConeRadius   = 168f;
    private const int   ConeSegments = 18;

    public EditorGeometryRenderer(IRhiDevice device)
    {
        _device = device;

        // Reuse the gizmo shader: world-space Position + RGBA8 Color.
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
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: Stride) },
            Topology               = PrimitiveTopology.LineList,
            Rasterizer             = new RasterizerState(CullMode.None),
            DepthStencil           = DepthStencilState.Default,
            BlendStates            = new[] { BlendState.AlphaBlend },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "EditorGeometryPipeline",
        });

        // Triangle pipeline — same shader, TriangleList, depth-tested but
        // depth-read-only (overlay), alpha-blended for the translucent cones.
        _triPipeline = device.CreatePipeline(new PipelineDesc
        {
            VertexShader   = vs,
            FragmentShader = ps,
            VertexAttributes = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float, bufferSlot: 0, offset: 0),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,  bufferSlot: 0, offset: 12),
            },
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: Stride) },
            Topology               = PrimitiveTopology.TriangleList,
            Rasterizer             = new RasterizerState(CullMode.None),
            DepthStencil           = DepthStencilState.DepthReadOnly,
            BlendStates            = new[] { BlendState.AlphaBlend },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "EditorGeometryTriPipeline",
        });

        _viewCb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: Unsafe.SizeOf<ViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "EditorGeometryViewParams"),
            ReadOnlySpan<byte>.Empty);

        AllocVb(8192);
        AllocTriVb(8192);
    }

    private void AllocVb(int vertexCapacity)
    {
        if (_vb.IsValid) _device.Destroy(_vb);
        _vbCapacity = vertexCapacity;
        _vbCpu = new byte[vertexCapacity * Stride];
        _vb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: vertexCapacity * Stride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "EditorGeometryVertices"),
            ReadOnlySpan<byte>.Empty);
    }

    private void AllocTriVb(int vertexCapacity)
    {
        if (_triVb.IsValid) _device.Destroy(_triVb);
        _triCapacity = vertexCapacity;
        _triCpu = new byte[vertexCapacity * Stride];
        _triVb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: vertexCapacity * Stride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "EditorGeometryTriVertices"),
            ReadOnlySpan<byte>.Empty);
    }

    public void Render(ICommandList cl, IReadOnlyList<Room> rooms, Level level,
                       Matrix4x4 viewProjection, in RenderScene scene)
    {
        if (level == null || rooms == null || rooms.Count == 0) return;

        int n;
        for (;;)
        {
            var span = MemoryMarshal.Cast<byte, LineVertex>(_vbCpu.AsSpan());
            n = Build(span, rooms, level, scene);
            if (n <= _vbCapacity) break;
            AllocVb(Math.Max(n, _vbCapacity * 2));
        }

        int tn;
        for (;;)
        {
            var span = MemoryMarshal.Cast<byte, LineVertex>(_triCpu.AsSpan());
            tn = BuildTriangles(span, rooms, level, scene);
            if (tn <= _triCapacity) break;
            AllocTriVb(Math.Max(tn, _triCapacity * 2));
        }

        if (n == 0 && tn == 0) return;

        var vp = new ViewParams { ViewProjection = viewProjection };
        unsafe
        {
            var s = new ReadOnlySpan<byte>(&vp, sizeof(ViewParams));
            cl.UpdateBuffer(_viewCb, 0, s);
        }
        _scratchCbuf[0] = _viewCb;

        // Line pass — wireframe overlays.
        if (n > 0)
        {
            cl.UpdateBuffer(_vb, 0, new ReadOnlySpan<byte>(_vbCpu, 0, n * Stride));
            cl.SetPipeline(_pipeline);
            cl.SetBindings(new Bindings { ConstantBuffers = _scratchCbuf });
            _scratchVbs[0] = new VertexBufferBinding(_vb, 0);
            cl.SetVertexBuffers(_scratchVbs);
            cl.Draw(n);
        }

        // Triangle pass — solid flyby path tube + direction cones.
        if (tn > 0)
        {
            cl.UpdateBuffer(_triVb, 0, new ReadOnlySpan<byte>(_triCpu, 0, tn * Stride));
            cl.SetPipeline(_triPipeline);
            cl.SetBindings(new Bindings { ConstantBuffers = _scratchCbuf });
            _scratchVbs[0] = new VertexBufferBinding(_triVb, 0);
            cl.SetVertexBuffers(_scratchVbs);
            cl.Draw(tn);
        }
    }

    public void Dispose()
    {
        if (_vb.IsValid)          _device.Destroy(_vb);
        if (_triVb.IsValid)       _device.Destroy(_triVb);
        if (_viewCb.IsValid)      _device.Destroy(_viewCb);
        if (_pipeline.IsValid)    _device.Destroy(_pipeline);
        if (_triPipeline.IsValid) _device.Destroy(_triPipeline);
    }

    // ============================================================ build

    private static int Build(Span<LineVertex> v, IReadOnlyList<Room> rooms, Level level,
                             in RenderScene scene)
    {
        int n = 0;
        uint selRgba = PackRgba(scene.SelectionTint);
        var highlighted = scene.Highlighted;

        // ---- Room bounding box (selected room) ----
        if (scene.ShowRoomBounds && scene.SelectedRoom != null)
            EmitRoomBox(v, ref n, scene.SelectedRoom, RoomBoundsColor);

        // ---- Object height line (selected object → floor) ----
        if (scene.ObjectHeightLine is var hl && hl.HasValue)
            Line(v, ref n, hl.Value.From, hl.Value.To, HeightColor);

        // ---- Per-room objects ----
        foreach (Room room in rooms)
        {
            if (room == null) continue;
            Vector3 wp = room.WorldPos;

            if (room.Objects != null)
                foreach (var obj in room.Objects)
                {
                    bool sel = highlighted != null && highlighted.Contains(obj);
                    switch (obj)
                    {
                        case BoxVolumeInstance box when scene.ShowVolumes:
                            EmitWireBox(v, ref n, box.RotationPositionMatrix, box.Size * 0.5f,
                                        VolumeWireColor(scene.VolumeColor, box.Enabled, sel));
                            break;

                        case SphereVolumeInstance sphere when scene.ShowVolumes:
                            EmitWireSphere(v, ref n, wp + sphere.Position, sphere.Size,
                                           VolumeWireColor(scene.VolumeColor, sphere.Enabled, sel));
                            break;

                        case MoveableInstance mov when scene.ShowBoundingBoxes:
                            EmitMoveableBox(v, ref n, mov, level, sel ? selRgba : BBoxColor);
                            break;

                        case StaticInstance stat when scene.ShowBoundingBoxes:
                            EmitStaticBox(v, ref n, stat, level, sel ? selRgba : BBoxColor);
                            break;
                    }
                }

            // Ghost blocks are sector-based — held in a separate room collection.
            if (scene.ShowGhostBlocks && room.GhostBlocks != null)
                foreach (var ghost in room.GhostBlocks)
                {
                    bool gsel = highlighted != null && highlighted.Contains(ghost);
                    EmitGhostBlock(v, ref n, ghost, gsel ? selRgba : GhostColor);
                }
        }

        // ---- Object brush overlay (painting mode): floor circle ----
        if (scene.Brush is { } brush)
            EmitCircle(v, ref n, brush.Center, Vector3.UnitX, Vector3.UnitZ,
                       brush.Radius, BrushColor);

        // ---- Sector split highlights ----
        EmitSectorSplits(v, ref n, scene);

        return n;
    }

    // ---- Sector split highlights -------------------------------------------
    // Wireframe ribbons along the split edges of the selected sectors, ported
    // (simplified — no per-face geometry-lookup gating, no portal y-offset)
    // from the legacy DrawSectorSplitHighlights.

    private const float SplitRibbonHalfHeight = 12f;  // legacy ribbon = 24 tall
    private const float SplitEdgeOffset       = 8f;   // legacy XZ_OFFSET

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitSectorSplits(Span<LineVertex> v, ref int n, in RenderScene scene)
    {
        Room room = scene.SelectedRoom;
        if (scene.HighlightedSplit == 0 || room?.Sectors == null) return;

        RectangleInt2 area = scene.SelectionArea;
        if (area.X0 < 0 || area.Y0 < 0 || area.X1 < area.X0 || area.Y1 < area.Y0) return;

        int splitIndex = scene.HighlightedSplit - 2;
        int x0 = Math.Max(0, area.X0), z0 = Math.Max(0, area.Y0);
        int x1 = Math.Min(room.NumXSectors - 1, area.X1);
        int z1 = Math.Min(room.NumZSectors - 1, area.Y1);

        for (int x = x0; x <= x1; x++)
        for (int z = z0; z <= z1; z++)
        {
            Sector s = room.Sectors[x, z];
            if (splitIndex < 0 || splitIndex > 7)
            {
                EmitSplitSurface(v, ref n, room, x, z,
                    s.Floor.XnZn, s.Floor.XnZp, s.Floor.XpZn, s.Floor.XpZp, s.Floor.DiagonalSplit);
                EmitSplitSurface(v, ref n, room, x, z,
                    s.Ceiling.XnZn, s.Ceiling.XnZp, s.Ceiling.XpZn, s.Ceiling.XpZp, s.Ceiling.DiagonalSplit);
            }
            else
            {
                if (splitIndex < s.ExtraFloorSplits.Count)
                {
                    var sp = s.ExtraFloorSplits[splitIndex];
                    EmitSplitSurface(v, ref n, room, x, z,
                        sp.XnZn, sp.XnZp, sp.XpZn, sp.XpZp, s.Floor.DiagonalSplit);
                }
                if (splitIndex < s.ExtraCeilingSplits.Count)
                {
                    var sp = s.ExtraCeilingSplits[splitIndex];
                    EmitSplitSurface(v, ref n, room, x, z,
                        sp.XnZn, sp.XnZp, sp.XpZn, sp.XpZp, s.Ceiling.DiagonalSplit);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitSplitSurface(Span<LineVertex> v, ref int n, Room room, int x, int z,
                                         int xnzn, int xnzp, int xpzn, int xpzp, DiagonalSplit diag)
    {
        if (diag != DiagonalSplit.XpZn && diag != DiagonalSplit.XnZn)   // +Z edge
            EmitRibbon(v, ref n, room, x + 1, xpzp, z + 1, x, xnzp, z + 1, 0f, SplitEdgeOffset);
        if (diag != DiagonalSplit.XnZp && diag != DiagonalSplit.XnZn)   // +X edge
            EmitRibbon(v, ref n, room, x + 1, xpzn, z, x + 1, xpzp, z + 1, SplitEdgeOffset, 0f);
        if (diag != DiagonalSplit.XpZp && diag != DiagonalSplit.XnZp)   // -Z edge
            EmitRibbon(v, ref n, room, x, xnzn, z, x + 1, xpzn, z, 0f, -SplitEdgeOffset);
        if (diag != DiagonalSplit.XpZn && diag != DiagonalSplit.XpZp)   // -X edge
            EmitRibbon(v, ref n, room, x, xnzp, z + 1, x, xnzn, z, -SplitEdgeOffset, 0f);

        switch (diag)   // diagonal split edge
        {
            case DiagonalSplit.XnZp:
                EmitRibbon(v, ref n, room, x, xnzn, z, x + 1, xpzp, z + 1, SplitEdgeOffset, -SplitEdgeOffset);
                break;
            case DiagonalSplit.XpZp:
                EmitRibbon(v, ref n, room, x, xnzp, z + 1, x + 1, xpzn, z, -SplitEdgeOffset, -SplitEdgeOffset);
                break;
            case DiagonalSplit.XnZn:
                EmitRibbon(v, ref n, room, x + 1, xpzn, z, x, xnzp, z + 1, SplitEdgeOffset, SplitEdgeOffset);
                break;
            case DiagonalSplit.XpZn:
                EmitRibbon(v, ref n, room, x + 1, xpzp, z + 1, x, xnzn, z, -SplitEdgeOffset, SplitEdgeOffset);
                break;
        }
    }

    // One split edge as a wireframe ribbon (top + bottom edge + two ends).
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitRibbon(Span<LineVertex> v, ref int n, Room room,
                                   int ax, int ay, int az, int bx, int by, int bz,
                                   float xOff, float zOff)
    {
        Vector3 wp = room.WorldPos;
        const float U = Level.SectorSizeUnit;
        const float h = SplitRibbonHalfHeight;

        var topA = new Vector3(wp.X + ax * U + xOff, wp.Y + ay + h, wp.Z + az * U + zOff);
        var botA = new Vector3(wp.X + ax * U + xOff, wp.Y + ay - h, wp.Z + az * U + zOff);
        var topB = new Vector3(wp.X + bx * U + xOff, wp.Y + by + h, wp.Z + bz * U + zOff);
        var botB = new Vector3(wp.X + bx * U + xOff, wp.Y + by - h, wp.Z + bz * U + zOff);

        Line(v, ref n, topA, topB, SplitColor);
        Line(v, ref n, botA, botB, SplitColor);
        Line(v, ref n, topA, botA, SplitColor);
        Line(v, ref n, topB, botB, SplitColor);
    }

    // ---- Room bounding box ---------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitRoomBox(Span<LineVertex> v, ref int n, Room room, uint color)
    {
        Vector3 wp = room.WorldPos;
        var min = new Vector3(wp.X, wp.Y + room.GetLowestCorner(),  wp.Z);
        var max = new Vector3(wp.X + room.NumXSectors * Level.SectorSizeUnit,
                              wp.Y + room.GetHighestCorner(),
                              wp.Z + room.NumZSectors * Level.SectorSizeUnit);
        EmitBoxMinMax(v, ref n, min, max, color);
    }

    // ---- Volumes ------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitWireBox(Span<LineVertex> v, ref int n, Matrix4x4 m,
                                    Vector3 halfExtents, uint color)
    {
        Vector3 C(float sx, float sy, float sz) =>
            Vector3.Transform(new Vector3(sx * halfExtents.X, sy * halfExtents.Y, sz * halfExtents.Z), m);

        Vector3 c000 = C(-1, -1, -1), c100 = C(1, -1, -1), c110 = C(1, 1, -1), c010 = C(-1, 1, -1);
        Vector3 c001 = C(-1, -1, 1),  c101 = C(1, -1, 1),  c111 = C(1, 1, 1),  c011 = C(-1, 1, 1);
        EmitBoxEdges(v, ref n, c000, c100, c110, c010, c001, c101, c111, c011, color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitBoxMinMax(Span<LineVertex> v, ref int n, Vector3 lo, Vector3 hi, uint color)
    {
        var c000 = new Vector3(lo.X, lo.Y, lo.Z); var c100 = new Vector3(hi.X, lo.Y, lo.Z);
        var c110 = new Vector3(hi.X, hi.Y, lo.Z); var c010 = new Vector3(lo.X, hi.Y, lo.Z);
        var c001 = new Vector3(lo.X, lo.Y, hi.Z); var c101 = new Vector3(hi.X, lo.Y, hi.Z);
        var c111 = new Vector3(hi.X, hi.Y, hi.Z); var c011 = new Vector3(lo.X, hi.Y, hi.Z);
        EmitBoxEdges(v, ref n, c000, c100, c110, c010, c001, c101, c111, c011, color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitBoxEdges(Span<LineVertex> v, ref int n,
                                     Vector3 c000, Vector3 c100, Vector3 c110, Vector3 c010,
                                     Vector3 c001, Vector3 c101, Vector3 c111, Vector3 c011,
                                     uint color)
    {
        // 12 edges of the box.
        Line(v, ref n, c000, c100, color); Line(v, ref n, c100, c110, color);
        Line(v, ref n, c110, c010, color); Line(v, ref n, c010, c000, color);
        Line(v, ref n, c001, c101, color); Line(v, ref n, c101, c111, color);
        Line(v, ref n, c111, c011, color); Line(v, ref n, c011, c001, color);
        Line(v, ref n, c000, c001, color); Line(v, ref n, c100, c101, color);
        Line(v, ref n, c110, c111, color); Line(v, ref n, c010, c011, color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitWireSphere(Span<LineVertex> v, ref int n, Vector3 centre,
                                       float radius, uint color)
    {
        EmitCircle(v, ref n, centre, Vector3.UnitX, Vector3.UnitY, radius, color);
        EmitCircle(v, ref n, centre, Vector3.UnitX, Vector3.UnitZ, radius, color);
        EmitCircle(v, ref n, centre, Vector3.UnitY, Vector3.UnitZ, radius, color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitCircle(Span<LineVertex> v, ref int n, Vector3 centre,
                                   Vector3 a, Vector3 b, float radius, uint color)
    {
        for (int i = 0; i < SphereSegments; i++)
        {
            float t0 = (float)(i       * (Math.PI * 2.0) / SphereSegments);
            float t1 = (float)((i + 1) * (Math.PI * 2.0) / SphereSegments);
            Vector3 p0 = centre + (a * (float)Math.Cos(t0) + b * (float)Math.Sin(t0)) * radius;
            Vector3 p1 = centre + (a * (float)Math.Cos(t1) + b * (float)Math.Sin(t1)) * radius;
            Line(v, ref n, p0, p1, color);
        }
    }

    // ---- Ghost blocks -------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitGhostBlock(Span<LineVertex> v, ref int n, GhostBlockInstance ghost, uint color)
    {
        Vector3[]? floor;
        Vector3[]? ceiling;
        try
        {
            floor   = ghost.ControlPositions(true);
            ceiling = ghost.ControlPositions(false);
        }
        catch { return; }
        if (floor == null || ceiling == null || floor.Length < 4 || ceiling.Length < 4) return;

        // ControlPositions order: [0]=XnZn [1]=XpZp [2]=XpZn [3]=XnZp.
        // Walk the perimeter XnZn → XpZn → XpZp → XnZp.
        EmitQuad(v, ref n, floor[0],   floor[2],   floor[1],   floor[3],   color);
        EmitQuad(v, ref n, ceiling[0], ceiling[2], ceiling[1], ceiling[3], color);
        for (int i = 0; i < 4; i++)
            Line(v, ref n, floor[i], ceiling[i], color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitQuad(Span<LineVertex> v, ref int n,
                                 Vector3 a, Vector3 b, Vector3 c, Vector3 d, uint color)
    {
        Line(v, ref n, a, b, color); Line(v, ref n, b, c, color);
        Line(v, ref n, c, d, color); Line(v, ref n, d, a, color);
    }

    // Solid translucent body for a ghost block — ported from the legacy
    // DrawGhostBlockBodies. Emits the four wall faces (with diagonal-split
    // height shifts) plus the two split triangles and the diagonal pair, both
    // for the floor and ceiling halves.
    private static void EmitGhostBlockBody(Span<LineVertex> v, ref int n, GhostBlockInstance ghost,
                                           in Vector4 baseColor, bool selected)
    {
        if (!ghost.Valid) return;

        // Same colour formula as the legacy: p1c is the "front" tone, p2c the
        // dimmer "back" tone; both share the alpha.
        Vector3 rgb = new(baseColor.X, baseColor.Y, baseColor.Z);
        uint p1c = PackRgba(new Vector4(rgb * (selected ? 0.8f : 0.4f), selected ? 0.7f : 0.5f));
        uint p2c = PackRgba(new Vector4(rgb * (selected ? 0.5f : 0.2f), selected ? 0.7f : 0.5f));

        for (int f = 0; f < 2; f++)
        {
            bool floor = f == 0;
            if ((floor && !ghost.ValidFloor) || (!floor && !ghost.ValidCeiling)) continue;

            var split   = floor ? ghost.Sector.Floor.DiagonalSplit : ghost.Sector.Ceiling.DiagonalSplit;
            bool tog    = floor ? ghost.FloorSplitToggled          : ghost.CeilingSplitToggled;
            var vPos    = ghost.ControlPositions(floor, false);
            var vOrg    = ghost.ControlPositions(floor, true);

            bool s0 = split == DiagonalSplit.XpZp || split == DiagonalSplit.XpZn;
            bool s1 = split == DiagonalSplit.XpZp || split == DiagonalSplit.XnZp;
            bool s2 = split == DiagonalSplit.XnZn || split == DiagonalSplit.XnZp;
            bool s3 = split == DiagonalSplit.XnZn || split == DiagonalSplit.XpZn;

            // Four side walls (Xn, Zn, Xp, Zp).
            for (int i = 0; i < 4; i++)
            {
                Vector3 fp0, fp1, fp2, fp3;
                bool sh = i == 0 ? s0 : i == 1 ? s1 : i == 2 ? s2 : s3;
                switch (i)
                {
                    case 0: // Xn
                        fp0 = vOrg[0]; fp1 = vOrg[3]; fp2 = vPos[3]; fp3 = vPos[0];
                        if (sh)
                        {
                            if (split == DiagonalSplit.XpZp)
                            {
                                fp0.Y = vOrg[3].Y;
                                fp3.Y = (vOrg[3] + (vPos[0] - vOrg[0])).Y;
                            }
                            else
                            {
                                fp1.Y = vOrg[0].Y;
                                fp2.Y = (vOrg[0] + (vPos[3] - vOrg[3])).Y;
                            }
                        }
                        break;
                    case 1: // Zn
                        fp0 = vOrg[3]; fp1 = vOrg[2]; fp2 = vPos[2]; fp3 = vPos[3];
                        if (sh)
                        {
                            if (split == DiagonalSplit.XnZp)
                            {
                                fp0.Y = vOrg[2].Y;
                                fp3.Y = (vOrg[2] + (vPos[3] - vOrg[3])).Y;
                            }
                            else
                            {
                                fp1.Y = vOrg[3].Y;
                                fp2.Y = (vOrg[3] + (vPos[2] - vOrg[2])).Y;
                            }
                        }
                        break;
                    case 2: // Xp
                        fp0 = vOrg[2]; fp1 = vOrg[1]; fp2 = vPos[1]; fp3 = vPos[2];
                        if (sh)
                        {
                            if (split == DiagonalSplit.XnZn)
                            {
                                fp0.Y = vOrg[1].Y;
                                fp3.Y = (vOrg[1] + (vPos[2] - vOrg[2])).Y;
                            }
                            else
                            {
                                fp1.Y = vOrg[2].Y;
                                fp2.Y = (vOrg[2] + (vPos[1] - vOrg[1])).Y;
                            }
                        }
                        break;
                    default: // Zp
                        fp0 = vOrg[1]; fp1 = vOrg[0]; fp2 = vPos[0]; fp3 = vPos[1];
                        if (sh)
                        {
                            if (split == DiagonalSplit.XpZn)
                            {
                                fp0.Y = vOrg[0].Y;
                                fp3.Y = (vOrg[0] + (vPos[1] - vOrg[1])).Y;
                            }
                            else
                            {
                                fp1.Y = vOrg[1].Y;
                                fp2.Y = (vOrg[1] + (vPos[0] - vOrg[0])).Y;
                            }
                        }
                        break;
                }
                EmitTriRgba(v, ref n, fp0, fp1, fp3, p1c, p1c, p2c);
                EmitTriRgba(v, ref n, fp1, fp2, fp3, p1c, p1c, p2c);
            }

            // Two split triangles capping the top / bottom — hidden when the
            // sector edge equals the original height (the shift height matches
            // the corner reference for the active split).
            int r = split switch
            {
                DiagonalSplit.XpZn => 0,
                DiagonalSplit.XnZn => 1,
                DiagonalSplit.XnZp => 2,
                DiagonalSplit.XpZp => 3,
                _                  => 0,
            };

            for (int i = 0; i < 2; i++)
            {
                bool triShift = (i == 0 && (split == DiagonalSplit.XpZn || split == DiagonalSplit.XnZn)) ||
                                (i != 0 && (split == DiagonalSplit.XpZp || split == DiagonalSplit.XnZp));
                int ch0 = i == 0 ? (tog ? 3 : 0) : (tog ? 1 : 2);
                int ch1 = i == 0 ? (tog ? 0 : 1) : (tog ? 2 : 3);
                int ch2 = i == 0 ? (tog ? 1 : 2) : (tog ? 3 : 0);

                Vector3 t0 = vPos[ch0];
                if (triShift) t0.Y = vOrg[r].Y + (vPos[ch0] - vOrg[ch0]).Y;
                Vector3 t1 = vPos[ch1];
                Vector3 t2 = vPos[ch2];
                if (triShift) t2.Y = vOrg[r].Y + (vPos[ch2] - vOrg[ch2]).Y;

                bool degenerate = vPos[ch0] == vOrg[ch0]
                               && vPos[ch1] == vOrg[ch1]
                               && vPos[ch2] == vOrg[ch2];
                uint tc = degenerate ? 0u : (i == 1 ? p1c : p2c);
                EmitTriRgba(v, ref n, t0, t1, t2, tc, tc, tc);
            }

            // Diagonal (skipped when the floor / ceiling is a flat quad).
            bool flip = split == DiagonalSplit.XnZp || split == DiagonalSplit.XpZn;
            bool draw = split != DiagonalSplit.None && !(floor ? ghost.FloorIsQuad : ghost.CeilingIsQuad);
            uint dc1 = draw ? p1c : 0u;
            uint dc2 = draw ? p2c : 0u;
            Vector3 d0 = flip ? vOrg[1] : vOrg[0];
            Vector3 d1 = flip ? vOrg[3] : vOrg[2];
            Vector3 d2 = flip ? vPos[3] : vPos[2];
            Vector3 d3 = flip ? vPos[1] : vPos[0];
            EmitTriRgba(v, ref n, d0, d1, d2, dc1, dc2, dc1);
            EmitTriRgba(v, ref n, d2, d3, d0, dc1, dc2, dc1);
        }
    }

    // Triangle emitter with per-vertex colours — same fast-path as EmitTri,
    // just with three independent colours.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitTriRgba(Span<LineVertex> v, ref int n,
                                    Vector3 a, Vector3 b, Vector3 c, uint ca, uint cb, uint cc)
    {
        int i = n;
        if ((uint)(i + 3) <= (uint)v.Length)
        {
            ref var v0 = ref Unsafe.Add(ref MemoryMarshal.GetReference(v), i);
            v0.Position = a; v0.Color = ca;
            ref var v1 = ref Unsafe.Add(ref v0, 1);
            v1.Position = b; v1.Color = cb;
            ref var v2 = ref Unsafe.Add(ref v0, 2);
            v2.Position = c; v2.Color = cc;
        }
        n = i + 3;
    }

    // ---- Object bounding boxes ---------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitMoveableBox(Span<LineVertex> v, ref int n, MoveableInstance mov,
                                        Level level, uint color)
    {
        var wad = level.Settings?.WadTryGetMoveable(mov.WadObjectId);
        if (wad == null || wad.Animations.Count == 0 || wad.Animations[0].KeyFrames.Count == 0)
            return;
        var bb = wad.Animations[0].KeyFrames[0].BoundingBox;
        Matrix4x4 m = Matrix4x4.CreateTranslation(bb.Center) * mov.RotationPositionMatrix;
        EmitWireBox(v, ref n, m, bb.Size * 0.5f, color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitStaticBox(Span<LineVertex> v, ref int n, StaticInstance stat,
                                      Level level, uint color)
    {
        var wad = level.Settings?.WadTryGetStatic(stat.WadObjectId);
        if (wad == null) return;
        var bb = wad.CollisionBox;
        Matrix4x4 m = Matrix4x4.CreateTranslation(bb.Center * stat.Scale) * stat.RotationPositionMatrix;
        EmitWireBox(v, ref n, m, bb.Size * stat.Scale * 0.5f, color);
    }

    // ============================================ triangle (solid) geometry

    private static int BuildTriangles(Span<LineVertex> v, IReadOnlyList<Room> rooms, Level level,
                                      in RenderScene scene)
    {
        int n = 0;
        uint selRgba = PackRgba(scene.SelectionTint);
        var highlighted = scene.Highlighted;

        foreach (Room room in rooms)
        {
            if (room?.Objects == null) continue;
            Vector3 wp = room.WorldPos;
            foreach (var obj in room.Objects)
            {
                bool sel = highlighted != null && highlighted.Contains(obj);
                switch (obj)
                {
                    // Flyby direction cone — translucent so it doesn't hide the scene.
                    case FlybyCameraInstance fb when scene.ShowOtherObjects:
                    {
                        uint rgb = sel ? selRgba : SequenceColor(fb.Sequence);
                        EmitFlybyCone(v, ref n, fb, (rgb & 0x00FFFFFFu) | 0x66000000u);
                        break;
                    }
                    // Volumes — translucent fill using the editor's ColorTrigger.
                    case BoxVolumeInstance bx when scene.ShowVolumes:
                        EmitSolidBox(v, ref n, bx.RotationPositionMatrix, bx.Size * 0.5f,
                                     VolumeFillColor(scene.VolumeColor, bx.Enabled, sel));
                        break;
                    case SphereVolumeInstance sp when scene.ShowVolumes:
                        EmitSolidSphere(v, ref n, wp + sp.Position, sp.Size,
                                        VolumeFillColor(scene.VolumeColor, sp.Enabled, sel));
                        break;
                }
            }

            // Ghost block solid bodies — same translucent fill the legacy
            // DrawGhostBlockBodies path drew.
            if (scene.ShowGhostBlocks && room.GhostBlocks != null)
                foreach (var ghost in room.GhostBlocks)
                {
                    bool gsel = highlighted != null && highlighted.Contains(ghost);
                    EmitGhostBlockBody(v, ref n, ghost, scene.GhostBlockColor, gsel);
                }
        }

        // ---- Solid Catmull-Rom path tube for the selected flyby sequence ----
        if (scene.ShowOtherObjects && scene.FlybyPathSequence >= 0)
            EmitFlybyPathSolid(v, ref n, level, scene.FlybyPathSequence);

        return n;
    }

    // Solid translucent box (12 triangles) — winding is irrelevant, the
    // triangle pass is CullNone.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitSolidBox(Span<LineVertex> v, ref int n, Matrix4x4 m,
                                     Vector3 he, uint color)
    {
        Vector3 C(float x, float y, float z) =>
            Vector3.Transform(new Vector3(x * he.X, y * he.Y, z * he.Z), m);

        Vector3 c000 = C(-1, -1, -1), c100 = C(1, -1, -1), c110 = C(1, 1, -1), c010 = C(-1, 1, -1);
        Vector3 c001 = C(-1, -1, 1),  c101 = C(1, -1, 1),  c111 = C(1, 1, 1),  c011 = C(-1, 1, 1);

        Quad(v, ref n, c000, c100, c110, c010, color);  // -Z
        Quad(v, ref n, c101, c001, c011, c111, color);  // +Z
        Quad(v, ref n, c001, c000, c010, c011, color);  // -X
        Quad(v, ref n, c100, c101, c111, c110, color);  // +X
        Quad(v, ref n, c000, c001, c101, c100, color);  // -Y
        Quad(v, ref n, c010, c110, c111, c011, color);  // +Y
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void Quad(Span<LineVertex> v, ref int n,
                             Vector3 a, Vector3 b, Vector3 c, Vector3 d, uint color)
    {
        EmitTri(v, ref n, a, b, c, color);
        EmitTri(v, ref n, a, c, d, color);
    }

    // Solid translucent UV sphere.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitSolidSphere(Span<LineVertex> v, ref int n, Vector3 centre,
                                        float radius, uint color)
    {
        const int rings = 8, segs = 12;
        for (int ring = 0; ring < rings; ring++)
        {
            float lat0 = (float)(Math.PI * ring       / rings - Math.PI / 2);
            float lat1 = (float)(Math.PI * (ring + 1) / rings - Math.PI / 2);
            float y0 = (float)Math.Sin(lat0), r0 = (float)Math.Cos(lat0);
            float y1 = (float)Math.Sin(lat1), r1 = (float)Math.Cos(lat1);
            for (int s = 0; s < segs; s++)
            {
                float lon0 = (float)(2 * Math.PI * s       / segs);
                float lon1 = (float)(2 * Math.PI * (s + 1) / segs);
                float c0 = (float)Math.Cos(lon0), s0 = (float)Math.Sin(lon0);
                float c1 = (float)Math.Cos(lon1), s1 = (float)Math.Sin(lon1);
                Vector3 p00 = centre + new Vector3(r0 * c0, y0, r0 * s0) * radius;
                Vector3 p01 = centre + new Vector3(r0 * c1, y0, r0 * s1) * radius;
                Vector3 p10 = centre + new Vector3(r1 * c0, y1, r1 * s0) * radius;
                Vector3 p11 = centre + new Vector3(r1 * c1, y1, r1 * s1) * radius;
                EmitTri(v, ref n, p00, p01, p11, color);
                EmitTri(v, ref n, p00, p11, p10, color);
            }
        }
    }

    // Flyby path — a solid triangular-prism tube following the Catmull-Rom
    // spline through the sequence's cameras. Ported from the legacy AddFlybyPath.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitFlybyPathSolid(Span<LineVertex> v, ref int n, Level level, int sequence)
    {
        var cams = FlybySequenceHelper.GetCameras(level, sequence);
        if (cams == null || cams.Count < 2) return;

        Vector4 startCol = MathC.GetRandomColorByIndex(sequence, 32, 0.7f);
        Vector4 endCol   = MathC.GetRandomColorByIndex(sequence, 32, 0.3f);

        var camList = new List<Vector3>();
        for (int i = 0; i < cams.Count; i++)
        {
            var cam = cams[i];
            if (cam.Room != null)
                camList.Add(cam.Position + cam.Room.WorldPos);

            // Sequence cuts jump to another camera; the path restarts there.
            bool isCut = FlybySequenceHelper.TryResolveCutTargetIndex(cams, i, out int targetIndex);
            if (isCut) i = targetIndex - 1;
            bool isLast = i == cams.Count - 1;

            if (isCut || isLast)
            {
                var pts = CatmullRomSpline.EvaluatePositions(camList, FlybyPathSmoothness);
                for (int j = 0; j < pts.Count - 1; j++)
                {
                    uint c = PackRgba(Vector4.Lerp(startCol, endCol, j / (float)pts.Count));
                    EmitPrismSegment(v, ref n, pts[j], pts[j + 1], FlybyPathThickness, c);
                }
                camList.Clear();
                if (!isCut && !isLast && cam.Room != null)
                    camList.Add(cam.Position + cam.Room.WorldPos);
            }
        }
    }

    // One spline segment as a triangular prism (6 triangles). The cross-section
    // is the legacy triangle: the spline point plus two points offset up-and-out.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitPrismSegment(Span<LineVertex> v, ref int n, Vector3 p0, Vector3 p1,
                                         float th, uint color)
    {
        Vector3 a0 = p0, a1 = p0 + new Vector3(th, th, th), a2 = p0 + new Vector3(-th, th, th);
        Vector3 b0 = p1, b1 = p1 + new Vector3(th, th, th), b2 = p1 + new Vector3(-th, th, th);

        EmitTri(v, ref n, a0, a1, b1, color);  EmitTri(v, ref n, b1, b0, a0, color);
        EmitTri(v, ref n, a2, a1, b1, color);  EmitTri(v, ref n, b1, b2, a2, color);
        EmitTri(v, ref n, a0, a2, b2, color);  EmitTri(v, ref n, b2, b0, a0, color);
    }

    // Flyby direction cone — apex at the camera, opening along its view direction.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitFlybyCone(Span<LineVertex> v, ref int n, FlybyCameraInstance fb, uint color)
    {
        if (fb.Room == null) return;
        Vector3 apex = fb.Room.WorldPos + fb.Position;

        Vector3 dir = fb.GetDirection();
        if (dir.LengthSquared() < 1e-6f) dir = Vector3.UnitZ;
        dir = Vector3.Normalize(dir);

        Vector3 right = Vector3.Cross(dir, Vector3.UnitY);
        if (right.LengthSquared() < 1e-6f) right = Vector3.UnitX;
        right = Vector3.Normalize(right);
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, dir));

        Vector3 baseCentre = apex + dir * ConeLength;
        for (int i = 0; i < ConeSegments; i++)
        {
            float t0 = (float)(i       * (Math.PI * 2.0) / ConeSegments);
            float t1 = (float)((i + 1) * (Math.PI * 2.0) / ConeSegments);
            Vector3 b0 = baseCentre + (right * (float)Math.Cos(t0) + up * (float)Math.Sin(t0)) * ConeRadius;
            Vector3 b1 = baseCentre + (right * (float)Math.Cos(t1) + up * (float)Math.Sin(t1)) * ConeRadius;
            EmitTri(v, ref n, apex, b0, b1, color);        // cone side
            EmitTri(v, ref n, baseCentre, b1, b0, color);  // base cap
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static uint SequenceColor(int sequence)
    {
        var c = MathC.GetRandomColorByIndex(sequence, 32, 0.7f);
        return Rgb(c.X, c.Y, c.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitTri(Span<LineVertex> v, ref int n, Vector3 a, Vector3 b, Vector3 c, uint color)
    {
        int i = n;
        if ((uint)(i + 3) <= (uint)v.Length)
        {
            // Skip span bounds checks — the range fits.
            ref var v0 = ref System.Runtime.CompilerServices.Unsafe.Add(
                ref System.Runtime.InteropServices.MemoryMarshal.GetReference(v), i);
            v0.Position = a; v0.Color = color;
            ref var v1 = ref System.Runtime.CompilerServices.Unsafe.Add(ref v0, 1);
            v1.Position = b; v1.Color = color;
            ref var v2 = ref System.Runtime.CompilerServices.Unsafe.Add(ref v0, 2);
            v2.Position = c; v2.Color = color;
        }
        n = i + 3;
    }

    // ---- primitives ---------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void Line(Span<LineVertex> v, ref int n, Vector3 a, Vector3 b, uint color)
    {
        int i = n;
        if ((uint)(i + 2) <= (uint)v.Length)
        {
            ref var v0 = ref System.Runtime.CompilerServices.Unsafe.Add(
                ref System.Runtime.InteropServices.MemoryMarshal.GetReference(v), i);
            v0.Position = a; v0.Color = color;
            ref var v1 = ref System.Runtime.CompilerServices.Unsafe.Add(ref v0, 1);
            v1.Position = b; v1.Color = color;
        }
        n = i + 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static uint Rgb(float r, float g, float b)
    {
        uint ri = (uint)Math.Clamp((int)(r * 255f + 0.5f), 0, 255);
        uint gi = (uint)Math.Clamp((int)(g * 255f + 0.5f), 0, 255);
        uint bi = (uint)Math.Clamp((int)(b * 255f + 0.5f), 0, 255);
        return ri | (gi << 8) | (bi << 16) | (0xFFu << 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static uint PackRgba(Vector4 c)
    {
        uint r = (uint)Math.Clamp((int)(c.X * 255f + 0.5f), 0, 255);
        uint g = (uint)Math.Clamp((int)(c.Y * 255f + 0.5f), 0, 255);
        uint b = (uint)Math.Clamp((int)(c.Z * 255f + 0.5f), 0, 255);
        uint a = (uint)Math.Clamp((int)(c.W * 255f + 0.5f), 0, 255);
        return r | (g << 8) | (b << 16) | (a << 24);
    }
}
