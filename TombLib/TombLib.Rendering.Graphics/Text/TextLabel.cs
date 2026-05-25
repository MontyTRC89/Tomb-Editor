using System.Numerics;

namespace TombLib.Rendering.Graphics.Text;

/// <summary>
/// One piece of text the V2 renderer should draw this frame. Mirrors the
/// legacy renderer's <c>Text</c> tag: a label is either anchored to a
/// world-space point (projected every frame — room names, object labels) or
/// pinned to a fixed screen pixel (FPS / status read-outs).
///
/// <para>Built by the editor (Panel3D) and handed to the renderer through
/// <c>RenderScene</c>; the renderer never inspects editor state to produce
/// text.</para>
/// </summary>
public readonly struct TextLabel
{
    /// <summary>The string to draw. May contain '\n' for multiple lines.</summary>
    public readonly string Text;

    /// <summary>
    /// true  → <see cref="ScreenPosition"/> is an absolute pixel anchor.
    /// false → <see cref="WorldPosition"/> is projected to screen each frame.
    /// </summary>
    public readonly bool ScreenSpace;

    /// <summary>World-space anchor (used when <see cref="ScreenSpace"/> is false).</summary>
    public readonly Vector3 WorldPosition;

    /// <summary>Absolute pixel anchor (used when <see cref="ScreenSpace"/> is true).</summary>
    public readonly Vector2 ScreenPosition;

    /// <summary>
    /// Extra pixel offset applied to the anchor after projection. Lets a
    /// world-anchored label sit slightly above-right of its object, the way
    /// the legacy renderer offsets object tags by (10, -10).
    /// </summary>
    public readonly Vector2 PixelOffset;

    /// <summary>
    /// Where the text block attaches to the anchor, 0..1 on each axis:
    /// (0,0) = block top-left at the anchor, (0.5,0.5) = block centred on it.
    /// </summary>
    public readonly Vector2 Alignment;

    /// <summary>Text colour, RGBA 0..1.</summary>
    public readonly Vector4 Color;

    /// <summary>Draw a dark semi-transparent box behind the text (legacy "font overlay").</summary>
    public readonly bool Background;

    private TextLabel(string text, bool screenSpace, Vector3 worldPos, Vector2 screenPos,
                      Vector2 pixelOffset, Vector2 alignment, Vector4 color, bool background)
    {
        Text           = text;
        ScreenSpace    = screenSpace;
        WorldPosition  = worldPos;
        ScreenPosition = screenPos;
        PixelOffset    = pixelOffset;
        Alignment      = alignment;
        Color          = color;
        Background     = background;
    }

    /// <summary>A label anchored to a world-space point.</summary>
    public static TextLabel World(string text, Vector3 worldPosition, Vector4 color,
                                  bool background, Vector2 alignment, Vector2 pixelOffset = default)
        => new(text, false, worldPosition, default, pixelOffset, alignment, color, background);

    /// <summary>A label pinned to a fixed screen pixel.</summary>
    public static TextLabel Screen(string text, Vector2 screenPosition, Vector4 color,
                                   bool background, Vector2 alignment)
        => new(text, true, default, screenPosition, default, alignment, color, background);
}
