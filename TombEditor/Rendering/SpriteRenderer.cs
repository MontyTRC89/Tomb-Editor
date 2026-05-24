using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TombLib.LevelData;
using TombLib.RenderingV2.Rhi;

namespace TombEditor.Rendering;

/// <summary>
/// Screen-space pass for TR1 / TR2 <see cref="SpriteInstance"/> objects —
/// the only engines whose level data carries placed sprites. Each instance's
/// world position is projected to viewport pixels here on the CPU; the GPU
/// renders a fixed-pixel-size quad at the projected anchor with the WAD
/// sprite's <c>Alignment</c> rectangle and samples the level atlas at the
/// frame's precomputed UVs. Higher engines (TR3+) have no SpriteInstance,
/// so the renderer is effectively a no-op there.
/// </summary>
public sealed class SpriteRenderer : IDisposable
{
    private readonly IRhiDevice     _device;
    private readonly PipelineHandle _pipeline;
    private readonly BufferHandle   _viewCb;

    private BufferHandle _vb;
    private int          _vbCapacity;   // in vertices
    private byte[]       _vbCpu;

    private readonly BufferHandle[]        _scratchCbuf = new BufferHandle[1];
    private readonly TextureHandle[]       _scratchTex  = new TextureHandle[1];
    private readonly SamplerHandle[]       _scratchSamp = new SamplerHandle[1];
    private readonly VertexBufferBinding[] _scratchVbs  = new VertexBufferBinding[1];

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SpriteVertex
    {
        public Vector2 Pos;     // pixel coordinates, top-left origin
        public Vector2 Uv;
        public uint    Color;   // RGBA8
    }
    private const int Stride = 20;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct ViewParams
    {
        public Vector4 InvViewport;   // xy = 2 / viewport size
    }

    public SpriteRenderer(IRhiDevice device)
    {
        _device = device;

        var (vs, ps) = ShaderLibrary.Load("Sprite");
        _pipeline = device.CreatePipeline(new PipelineDesc
        {
            VertexShader   = vs,
            FragmentShader = ps,
            VertexAttributes = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32_Float,   bufferSlot: 0, offset: 0),
                new VertexAttribute("TEXCOORD", 0, Format.R32G32_Float,   bufferSlot: 0, offset: 8),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm, bufferSlot: 0, offset: 16),
            },
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: Stride) },
            Topology               = PrimitiveTopology.TriangleList,
            Rasterizer             = new RasterizerState(CullMode.None),
            // Sprites are an editor overlay — always on top, no depth interaction.
            DepthStencil           = DepthStencilState.Disabled,
            BlendStates            = new[] { BlendState.AlphaBlend },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "SpritePipeline",
        });

        _viewCb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: Unsafe.SizeOf<ViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "SpriteViewParams"),
            ReadOnlySpan<byte>.Empty);

        AllocVb(1024);
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
                debugName: "SpriteVertices"),
            ReadOnlySpan<byte>.Empty);
    }

    /// <summary>
    /// Records the sprite pass. Call AFTER the text pass so sprites paint over
    /// text labels (matching legacy compositing). The level atlas already
    /// contains every TR1 / TR2 sprite frame the level uses (packed by
    /// <see cref="TextureAtlas"/> during build).
    /// </summary>
    public void Render(ICommandList cl, IReadOnlyList<Room> visibleRooms, Level level,
                       TextureAtlas atlas, Matrix4x4 viewProjection, Size viewport,
                       HighlightedObjects highlighted, Vector4 selectionTint)
    {
        if (level?.Settings == null || atlas == null || visibleRooms == null) return;
        if (viewport.Width <= 0 || viewport.Height <= 0) return;
        if (level.Settings.GameVersion.Native() > TRVersion.Game.TR2) return;

        var sequences = level.Settings.WadGetAllSpriteSequences();
        if (sequences == null || sequences.Count == 0) return;

        uint selRgba = PackRgba(selectionTint);

        // Build the quad stream, growing the VB if a frame needs more room.
        int n;
        for (;;)
        {
            var span = MemoryMarshal.Cast<byte, SpriteVertex>(_vbCpu.AsSpan());
            n = BuildVertices(span, visibleRooms, sequences, atlas, viewProjection,
                              viewport, highlighted, selRgba);
            if (n <= _vbCapacity) break;
            AllocVb(Math.Max(n, _vbCapacity * 2));
        }
        if (n == 0) return;

        cl.UpdateBuffer(_vb, 0, new ReadOnlySpan<byte>(_vbCpu, 0, n * Stride));

        var vp = new ViewParams { InvViewport = new Vector4(2f / viewport.Width, 2f / viewport.Height, 0f, 0f) };
        unsafe
        {
            var s = new ReadOnlySpan<byte>(&vp, sizeof(ViewParams));
            cl.UpdateBuffer(_viewCb, 0, s);
        }

        cl.SetPipeline(_pipeline);
        _scratchCbuf[0] = _viewCb;
        _scratchTex[0]  = atlas.Texture;
        _scratchSamp[0] = atlas.Sampler;
        cl.SetBindings(new Bindings
        {
            ConstantBuffers = _scratchCbuf,
            Textures        = _scratchTex,
            Samplers        = _scratchSamp,
        });
        _scratchVbs[0] = new VertexBufferBinding(_vb, 0);
        cl.SetVertexBuffers(_scratchVbs);
        cl.Draw(n);
    }

    public void Dispose()
    {
        if (_vb.IsValid)       _device.Destroy(_vb);
        if (_viewCb.IsValid)   _device.Destroy(_viewCb);
        if (_pipeline.IsValid) _device.Destroy(_pipeline);
    }

    // ============================================================ helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int BuildVertices(Span<SpriteVertex> v, IReadOnlyList<Room> rooms,
                                     IDictionary<TombLib.Wad.WadSpriteSequenceId, TombLib.Wad.WadSpriteSequence> sequences,
                                     TextureAtlas atlas, Matrix4x4 vp, Size viewport,
                                     HighlightedObjects highlighted, uint selRgba)
    {
        int n = 0;
        for (int r = 0; r < rooms.Count; r++)
        {
            var room = rooms[r];
            if (room?.Objects == null) continue;
            Vector3 wp = room.WorldPos;
            foreach (var obj in room.Objects)
            {
                if (obj is not SpriteInstance sprite) continue;

                // Look up the sprite frame.
                TombLib.Wad.WadSpriteSequence? seq = null;
                foreach (var kv in sequences)
                    if (kv.Key.TypeId == sprite.Sequence) { seq = kv.Value; break; }
                if (seq == null || sprite.Frame < 0 || sprite.Frame >= seq.Sprites.Count) continue;

                if (!atlas.TryGetSpriteFrameUv(seq, sprite.Frame, out var uvMin, out var uvMax)) continue;
                var alignment = seq.Sprites[sprite.Frame].Alignment;

                // Project the world position to viewport pixels (top-left origin).
                if (!Project(wp + sprite.Position, vp, viewport, out Vector2 anchor, out float depth)) continue;
                if (depth < 0f || depth >= 1f) continue;

                float x0 = anchor.X + alignment.X0;
                float y0 = anchor.Y + alignment.Y0;
                float x1 = anchor.X + alignment.X1;
                float y1 = anchor.Y + alignment.Y1;

                uint tint = (highlighted != null && highlighted.Contains(sprite))
                            ? selRgba
                            : 0xFF_FF_FF_FFu;

                EmitQuad(v, ref n, x0, y0, x1, y1, uvMin.X, uvMin.Y, uvMax.X, uvMax.Y, tint);
            }
        }
        return n;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool Project(Vector3 world, Matrix4x4 vp, Size viewport, out Vector2 px, out float ndcZ)
    {
        Vector4 clip = Vector4.Transform(new Vector4(world, 1f), vp);
        if (clip.W <= 1e-4f) { px = default; ndcZ = -1f; return false; }
        float inv = 1f / clip.W;
        float ndcX = clip.X * inv;
        float ndcY = clip.Y * inv;
        ndcZ       = clip.Z * inv;
        px = new Vector2((ndcX * 0.5f + 0.5f) * viewport.Width,
                         (0.5f - ndcY * 0.5f) * viewport.Height);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void EmitQuad(Span<SpriteVertex> v, ref int n,
                                 float x0, float y0, float x1, float y1,
                                 float u0, float v0Tex, float u1, float v1Tex, uint color)
    {
        int i = n;
        if ((uint)(i + 6) > (uint)v.Length) { n = i + 6; return; }
        ref var p0 = ref Unsafe.Add(ref MemoryMarshal.GetReference(v), i);
        p0.Pos = new Vector2(x0, y0); p0.Uv = new Vector2(u0, v0Tex); p0.Color = color;
        ref var p1 = ref Unsafe.Add(ref p0, 1);
        p1.Pos = new Vector2(x1, y0); p1.Uv = new Vector2(u1, v0Tex); p1.Color = color;
        ref var p2 = ref Unsafe.Add(ref p0, 2);
        p2.Pos = new Vector2(x1, y1); p2.Uv = new Vector2(u1, v1Tex); p2.Color = color;
        ref var p3 = ref Unsafe.Add(ref p0, 3);
        p3.Pos = new Vector2(x0, y0); p3.Uv = new Vector2(u0, v0Tex); p3.Color = color;
        ref var p4 = ref Unsafe.Add(ref p0, 4);
        p4.Pos = new Vector2(x1, y1); p4.Uv = new Vector2(u1, v1Tex); p4.Color = color;
        ref var p5 = ref Unsafe.Add(ref p0, 5);
        p5.Pos = new Vector2(x0, y1); p5.Uv = new Vector2(u0, v1Tex); p5.Color = color;
        n = i + 6;
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
