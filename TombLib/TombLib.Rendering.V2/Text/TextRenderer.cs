using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TombLib.RenderingV2.Rhi;

namespace TombLib.RenderingV2.Text;

/// <summary>
/// Screen-space text pass for the V2 renderer. Takes the per-frame
/// <see cref="TextLabel"/> list, projects world-anchored labels, lays the
/// glyphs out and batches every quad into one dynamic vertex buffer / draw
/// call. Backend-agnostic: it only talks to <see cref="IRhiDevice"/>.
///
/// <para>Owns the <see cref="GlyphAtlas"/>; glyph rasterisation happens lazily
/// the first time a code point is seen.</para>
/// </summary>
public sealed class TextRenderer : IDisposable
{
    private readonly IRhiDevice     _device;
    private readonly PipelineHandle _pipeline;
    private readonly BufferHandle   _viewCb;
    private readonly GlyphAtlas     _atlas;

    private BufferHandle _vb;
    private int          _vbCapacity;   // in vertices
    private byte[]       _vbCpu;

    private readonly BufferHandle[]        _scratchCbuf = new BufferHandle[1];
    private readonly TextureHandle[]       _scratchTex  = new TextureHandle[1];
    private readonly SamplerHandle[]       _scratchSamp = new SamplerHandle[1];
    private readonly VertexBufferBinding[] _scratchVbs  = new VertexBufferBinding[1];
    private readonly List<string>          _lineScratch = new();

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TextVertex
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

    // Background box colour for labels with Background set (legacy "font
    // overlay"): near-black, ~78% opaque.
    private const uint BackgroundColor = 0xC8_00_00_00u;

    public TextRenderer(IRhiDevice device)
    {
        _device = device;
        _atlas  = new GlyphAtlas(device);

        var (vs, ps) = ShaderLibrary.Load("Text");
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
            // Text is an editor overlay — always on top, no depth interaction.
            DepthStencil           = DepthStencilState.Disabled,
            BlendStates            = new[] { BlendState.AlphaBlend },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "TextPipeline",
        });

        _viewCb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: Unsafe.SizeOf<ViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "TextViewParams"),
            ReadOnlySpan<byte>.Empty);

        AllocVb(4096);
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
                debugName: "TextVertices"),
            ReadOnlySpan<byte>.Empty);
    }

    /// <summary>
    /// Rasterises every glyph the frame needs and (re)uploads the atlas
    /// texture. MUST be called BEFORE <see cref="IRhiDevice.BeginCommandList"/>
    /// — atlas creation runs its own one-shot GPU submit, which deadlocks the
    /// frame if done while the command buffer is open.
    /// </summary>
    public void Prepare(IReadOnlyList<TextLabel> labels)
    {
        if (!_atlas.Available || labels == null || labels.Count == 0)
            return;
        foreach (TextLabel label in labels)
            EnsureGlyphs(label.Text);
        _atlas.GetGlyph(' ');   // tab-width metric, used by the layout pass
        _atlas.Flush();
    }

    /// <summary>
    /// Records the text pass. Call last in the frame (after the gizmo) so
    /// labels paint over everything. World labels are projected with
    /// <paramref name="viewProjection"/>; off-screen ones are dropped.
    /// <see cref="Prepare"/> must have run first, before the command list.
    /// </summary>
    public void Render(ICommandList cl, IReadOnlyList<TextLabel> labels,
                       Matrix4x4 viewProjection, int width, int height)
    {
        if (!_atlas.Available || labels == null || labels.Count == 0 || width <= 0 || height <= 0)
            return;

        // Build the quad stream, growing the VB if a frame needs more room.
        int n;
        for (;;)
        {
            var span = MemoryMarshal.Cast<byte, TextVertex>(_vbCpu.AsSpan());
            n = BuildVertices(span, labels, viewProjection, width, height);
            if (n <= _vbCapacity) break;
            AllocVb(Math.Max(n, _vbCapacity * 2));
        }
        if (n == 0) return;

        // 3. Upload + draw.
        cl.UpdateBuffer(_vb, 0, new ReadOnlySpan<byte>(_vbCpu, 0, n * Stride));

        var vp = new ViewParams { InvViewport = new Vector4(2f / width, 2f / height, 0f, 0f) };
        unsafe
        {
            var s = new ReadOnlySpan<byte>(&vp, sizeof(ViewParams));
            cl.UpdateBuffer(_viewCb, 0, s);
        }

        cl.SetPipeline(_pipeline);
        _scratchCbuf[0] = _viewCb;
        _scratchTex[0]  = _atlas.Texture;
        _scratchSamp[0] = _atlas.Sampler;
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
        _atlas.Dispose();
    }

    // ============================================================ helpers

    private void EnsureGlyphs(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        foreach (Rune rune in text.EnumerateRunes())
        {
            int cp = rune.Value;
            if (cp == '\n' || cp == '\r' || cp == '\t') continue;
            _atlas.GetGlyph(cp);
        }
    }

    private int BuildVertices(Span<TextVertex> v, IReadOnlyList<TextLabel> labels,
                              Matrix4x4 vp, int width, int height)
    {
        int n = 0;
        float lineHeight = _atlas.LineHeight;
        float tabWidth   = _atlas.GetGlyph(' ').Advance * 4f;

        foreach (TextLabel label in labels)
        {
            if (string.IsNullOrEmpty(label.Text)) continue;

            // Resolve the anchor pixel.
            Vector2 anchor;
            if (label.ScreenSpace)
                anchor = label.ScreenPosition;
            else if (!Project(label.WorldPosition, vp, width, height, out anchor))
                continue;
            anchor += label.PixelOffset;

            // Split into lines and measure the block bounds.
            SplitLines(label.Text);
            float blockW = 0f;
            foreach (string line in _lineScratch)
            {
                float w = MeasureLine(line, tabWidth);
                if (w > blockW) blockW = w;
            }
            float blockH = _lineScratch.Count * lineHeight;

            float blockLeft = MathF.Round(anchor.X - label.Alignment.X * blockW);
            float blockTop  = MathF.Round(anchor.Y - label.Alignment.Y * blockH);

            uint color = PackRgba(label.Color);

            // Background box first, so glyphs paint over it.
            if (label.Background)
                EmitSolidQuad(v, ref n, blockLeft - 3f, blockTop - 2f,
                              blockW + 6f, blockH + 4f, BackgroundColor);

            // Glyphs, line by line. Lines are left-aligned within the block.
            for (int i = 0; i < _lineScratch.Count; i++)
            {
                float penX    = 0f;
                float lineTop = blockTop + i * lineHeight;
                foreach (Rune rune in _lineScratch[i].EnumerateRunes())
                {
                    int cp = rune.Value;
                    if (cp == '\t') { penX += tabWidth; continue; }
                    Glyph g = _atlas.GetGlyph(cp);
                    if (g.HasPixels)
                    {
                        float gx = MathF.Round(blockLeft + penX + g.OffsetX);
                        float gy = MathF.Round(lineTop + g.OffsetY);
                        EmitGlyphQuad(v, ref n, gx, gy, g, color);
                    }
                    penX += g.Advance;
                }
            }
        }
        return n;
    }

    private void SplitLines(string text)
    {
        _lineScratch.Clear();
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '\n') continue;
            int end = (i > start && text[i - 1] == '\r') ? i - 1 : i;
            _lineScratch.Add(text.Substring(start, end - start));
            start = i + 1;
        }
        _lineScratch.Add(text.Substring(start));
    }

    private float MeasureLine(string line, float tabWidth)
    {
        float w = 0f;
        foreach (Rune rune in line.EnumerateRunes())
            w += rune.Value == '\t' ? tabWidth : _atlas.GetGlyph(rune.Value).Advance;
        return w;
    }

    private static bool Project(Vector3 world, Matrix4x4 vp, int width, int height, out Vector2 px)
    {
        // Row-vector multiply — matches the shader's mul(VP, pos) once the
        // raw .NET row-major matrix is read column-major on the GPU.
        Vector4 clip = Vector4.Transform(new Vector4(world, 1f), vp);
        if (clip.W <= 1e-4f) { px = default; return false; }   // behind the camera
        float inv = 1f / clip.W;
        float ndcX = clip.X * inv, ndcY = clip.Y * inv, ndcZ = clip.Z * inv;
        if (ndcZ < 0f || ndcZ > 1f) { px = default; return false; }
        px = new Vector2((ndcX * 0.5f + 0.5f) * width, (0.5f - ndcY * 0.5f) * height);
        return true;
    }

    private void EmitGlyphQuad(Span<TextVertex> v, ref int n, float x, float y, in Glyph g, uint color)
    {
        if (n + 6 > v.Length) { n += 6; return; }
        float inv = 1f / GlyphAtlas.Size;
        float u0 = g.AtlasX * inv;
        float v0 = g.AtlasY * inv;
        float u1 = (g.AtlasX + g.Width)  * inv;
        float v1 = (g.AtlasY + g.Height) * inv;
        Quad(v, ref n, x, y, g.Width, g.Height, u0, v0, u1, v1, color);
    }

    private void EmitSolidQuad(Span<TextVertex> v, ref int n, float x, float y,
                               float w, float h, uint color)
    {
        if (n + 6 > v.Length) { n += 6; return; }
        Vector2 uv = _atlas.SolidUv;
        Quad(v, ref n, x, y, w, h, uv.X, uv.Y, uv.X, uv.Y, color);
    }

    private static void Quad(Span<TextVertex> v, ref int n, float x, float y, float w, float h,
                             float u0, float v0, float u1, float v1, uint color)
    {
        var p00 = new TextVertex { Pos = new Vector2(x,     y),     Uv = new Vector2(u0, v0), Color = color };
        var p10 = new TextVertex { Pos = new Vector2(x + w, y),     Uv = new Vector2(u1, v0), Color = color };
        var p11 = new TextVertex { Pos = new Vector2(x + w, y + h), Uv = new Vector2(u1, v1), Color = color };
        var p01 = new TextVertex { Pos = new Vector2(x,     y + h), Uv = new Vector2(u0, v1), Color = color };
        v[n++] = p00; v[n++] = p10; v[n++] = p11;
        v[n++] = p00; v[n++] = p11; v[n++] = p01;
    }

    private static uint PackRgba(Vector4 c)
    {
        uint r = (uint)Math.Clamp((int)(c.X * 255f + 0.5f), 0, 255);
        uint g = (uint)Math.Clamp((int)(c.Y * 255f + 0.5f), 0, 255);
        uint b = (uint)Math.Clamp((int)(c.Z * 255f + 0.5f), 0, 255);
        uint a = (uint)Math.Clamp((int)(c.W * 255f + 0.5f), 0, 255);
        return r | (g << 8) | (b << 16) | (a << 24);
    }
}
