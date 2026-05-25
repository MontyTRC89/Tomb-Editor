using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TombLib.Rendering.Graphics.Rhi;

namespace TombLib.Rendering.Graphics.Text;

/// <summary>One rasterised glyph: its rectangle in the atlas plus layout metrics.</summary>
public struct Glyph
{
    /// <summary>Top-left of the glyph cell inside the atlas texture, in texels.</summary>
    public int AtlasX, AtlasY;
    /// <summary>Glyph cell size in texels (includes a 1px transparent border).</summary>
    public int Width, Height;
    /// <summary>Pixel offset from the pen origin / line top to the cell's top-left.</summary>
    public float OffsetX, OffsetY;
    /// <summary>Horizontal advance to the next glyph, in pixels.</summary>
    public float Advance;
    /// <summary>False for whitespace / non-printing code points (no quad emitted).</summary>
    public bool HasPixels;
}

/// <summary>
/// Cross-platform dynamic glyph atlas for the V2 text pass. Glyphs are
/// rasterised on demand with SixLabors (fully managed — no GDI, no native
/// binaries) so non-Latin scripts work the same on every backend / OS.
///
/// <para>Coverage is stored in the texture's ALPHA channel (RGB is white).
/// A 4×4 fully-opaque block sits at (0,0) so the text pipeline can draw the
/// legacy "font overlay" background box by sampling <see cref="SolidUv"/>.</para>
///
/// <para>The atlas owns a CPU-side mirror and re-creates the GPU texture
/// whenever new glyphs were added (cheap: only on the first frame a code
/// point appears, then never again). On overflow it is cleared and repacked.</para>
/// </summary>
public sealed class GlyphAtlas : IDisposable
{
    /// <summary>Atlas texture dimensions (square).</summary>
    public const int Size = 1024;
    /// <summary>Font size used to rasterise glyphs, in pixels.</summary>
    public const float FontSizePx = 16f;

    private const int Pad        = 1;  // transparent border around each glyph
    private const int SolidBlock = 4;  // size of the opaque texel block at (0,0)

    private readonly IRhiDevice _device;
    private readonly byte[]     _pixels = new byte[Size * Size * 4];
    private readonly Dictionary<int, Glyph> _glyphs = new();

    private Font?         _font;
    private FontFamily[]  _fallbacks = Array.Empty<FontFamily>();
    private TextureHandle _texture;
    private SamplerHandle _sampler;
    private bool          _dirty;

    // Shelf packer cursor.
    private int _shelfX, _shelfY, _shelfH;

    /// <summary>False when no usable system font was found — the renderer then skips text.</summary>
    public bool Available { get; private set; }
    /// <summary>Vertical distance between consecutive text lines, in pixels.</summary>
    public float LineHeight { get; private set; } = FontSizePx * 1.3f;
    /// <summary>Current atlas texture (valid after <see cref="Flush"/>).</summary>
    public TextureHandle Texture => _texture;
    public SamplerHandle Sampler => _sampler;
    /// <summary>UV of the opaque texel used for background boxes.</summary>
    public Vector2 SolidUv => new(SolidBlock * 0.5f / Size, SolidBlock * 0.5f / Size);

    public GlyphAtlas(IRhiDevice device)
    {
        _device = device;

        FillSolidBlock();
        ResetShelf();

        try { InitFont(); Available = _font != null; }
        catch { Available = false; }

        _texture = CreateTexture();
        // Point sampling: glyph quads are emitted 1:1 (one atlas texel per
        // screen pixel) at integer positions, so nearest filtering reproduces
        // the rasterised (already anti-aliased) glyph exactly. Linear
        // filtering blurs it whenever the quad lands even a fraction of a
        // pixel off the texel grid.
        _sampler = _device.CreateSampler(new SamplerDesc(FilterMode.Nearest, AddressMode.Clamp));
    }

    // ----------------------------------------------------------- font setup

    private void InitFont()
    {
        // Preferred UI fonts (Latin / Cyrillic / Greek). First match wins.
        string[] preferred =
        {
            "Segoe UI", "Tahoma", "Arial", "Helvetica Neue", "Helvetica",
            "DejaVu Sans", "Liberation Sans", "Noto Sans", "Verdana",
        };
        FontFamily family = default;
        bool found = false;
        foreach (string name in preferred)
            if (SystemFonts.TryGet(name, out family)) { found = true; break; }
        if (!found)
            foreach (FontFamily fam in SystemFonts.Families) { family = fam; found = true; break; }
        if (!found) return;

        _font = family.CreateFont(FontSizePx, FontStyle.Regular);

        // Fallback families for code points the primary font lacks — chiefly
        // CJK, but also anything else SixLabors needs to fill in. Drawing /
        // measuring automatically walks this list per glyph.
        string[] fallbackNames =
        {
            "Microsoft YaHei", "Yu Gothic UI", "Yu Gothic", "Meiryo", "MS Gothic",
            "Malgun Gothic", "SimSun", "Microsoft JhengHei",
            "Noto Sans CJK SC", "Noto Sans CJK JP", "Noto Sans CJK KR",
            "Noto Sans SC", "Noto Sans JP", "Noto Sans KR", "Noto Sans",
            "PingFang SC", "Hiragino Sans", "WenQuanYi Zen Hei",
            "Arial Unicode MS", "DejaVu Sans",
        };
        var list = new List<FontFamily>();
        foreach (string name in fallbackNames)
            if (SystemFonts.TryGet(name, out FontFamily fam)) list.Add(fam);
        _fallbacks = list.ToArray();

        try
        {
            FontRectangle adv = TextMeasurer.MeasureAdvance("Ayg", MakeOptions(Vector2.Zero));
            if (adv.Height > 1f) LineHeight = adv.Height;
        }
        catch { /* keep the default line height */ }
    }

    private RichTextOptions MakeOptions(Vector2 origin) => new(_font!)
    {
        FallbackFontFamilies = _fallbacks,
        Origin               = origin,
    };

    // ------------------------------------------------------------- glyphs

    /// <summary>Returns the cached glyph for a code point, rasterising it on first use.</summary>
    public Glyph GetGlyph(int codepoint)
    {
        if (_glyphs.TryGetValue(codepoint, out Glyph g))
            return g;
        g = Available ? Rasterize(codepoint) : default;
        _glyphs[codepoint] = g;
        return g;
    }

    private Glyph Rasterize(int codepoint)
    {
        string s;
        try { s = char.ConvertFromUtf32(codepoint); }
        catch { return default; }

        RichTextOptions measure = MakeOptions(Vector2.Zero);
        FontRectangle bounds  = TextMeasurer.MeasureBounds(s, measure);
        FontRectangle advance = TextMeasurer.MeasureAdvance(s, measure);
        float adv = advance.Width;

        int iw = (int)MathF.Ceiling(bounds.Width);
        int ih = (int)MathF.Ceiling(bounds.Height);
        if (iw <= 0 || ih <= 0 || bounds.IsEmpty || float.IsNaN(bounds.Width))
            return new Glyph { Advance = adv, HasPixels = false };

        int cw = iw + Pad * 2;
        int ch = ih + Pad * 2;
        if (cw > Size || ch > Size)
            return new Glyph { Advance = adv, HasPixels = false };

        // Rasterise the glyph into a transparent image; alpha = coverage.
        byte[] coverage = new byte[cw * ch];
        try
        {
            using var img = new Image<Rgba32>(cw, ch);
            RichTextOptions draw = MakeOptions(new Vector2(Pad - bounds.X, Pad - bounds.Y));
            img.Mutate(ctx => ctx.DrawText(draw, s, Color.White));
            img.ProcessPixelRows(acc =>
            {
                for (int y = 0; y < acc.Height; y++)
                {
                    Span<Rgba32> row = acc.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                        coverage[y * cw + x] = row[x].A;
                }
            });
        }
        catch { return new Glyph { Advance = adv, HasPixels = false }; }

        if (!PlaceInShelf(cw, ch, out int ax, out int ay))
        {
            ClearAtlas();
            if (!PlaceInShelf(cw, ch, out ax, out ay))
                return new Glyph { Advance = adv, HasPixels = false };
        }

        for (int y = 0; y < ch; y++)
        {
            int dst = ((ay + y) * Size + ax) * 4;
            for (int x = 0; x < cw; x++)
            {
                int o = dst + x * 4;
                _pixels[o] = _pixels[o + 1] = _pixels[o + 2] = 255;
                _pixels[o + 3] = coverage[y * cw + x];
            }
        }
        _dirty = true;

        return new Glyph
        {
            AtlasX  = ax, AtlasY = ay, Width = cw, Height = ch,
            OffsetX = bounds.X - Pad, OffsetY = bounds.Y - Pad,
            Advance = adv, HasPixels = true,
        };
    }

    /// <summary>
    /// Re-upload the GPU texture if glyphs were added since the last call.
    /// MUST be called outside command-list recording: texture creation runs
    /// its own one-shot GPU submit on some backends, which would corrupt an
    /// open frame command buffer.
    /// </summary>
    public void Flush()
    {
        if (!_dirty) return;
        _dirty = false;
        // Destroy() defers the actual free to the next frame boundary, so the
        // old atlas stays alive as long as an in-flight frame still samples
        // it — no explicit GPU drain needed here.
        if (_texture.IsValid) _device.Destroy(_texture);
        _texture = CreateTexture();
    }

    // ------------------------------------------------------------ packing

    private TextureHandle CreateTexture() => _device.CreateTexture(
        new TextureDesc(TextureKind.Texture2D, Size, Size, Format.R8G8B8A8_UNorm,
                        TextureBindFlags.ShaderResource, debugName: "GlyphAtlas"),
        _pixels);

    private void ResetShelf() { _shelfX = SolidBlock; _shelfY = 0; _shelfH = SolidBlock; }

    private bool PlaceInShelf(int w, int h, out int x, out int y)
    {
        if (_shelfX + w > Size) { _shelfX = 0; _shelfY += _shelfH; _shelfH = 0; }
        if (_shelfY + h > Size) { x = y = 0; return false; }
        x = _shelfX; y = _shelfY;
        _shelfX += w;
        if (h > _shelfH) _shelfH = h;
        return true;
    }

    private void ClearAtlas()
    {
        _glyphs.Clear();
        Array.Clear(_pixels, 0, _pixels.Length);
        FillSolidBlock();
        ResetShelf();
    }

    private void FillSolidBlock()
    {
        for (int y = 0; y < SolidBlock; y++)
            for (int x = 0; x < SolidBlock; x++)
            {
                int o = (y * Size + x) * 4;
                _pixels[o] = _pixels[o + 1] = _pixels[o + 2] = _pixels[o + 3] = 255;
            }
    }

    public void Dispose()
    {
        if (_texture.IsValid) _device.Destroy(_texture);
        if (_sampler.IsValid) _device.Destroy(_sampler);
    }
}
