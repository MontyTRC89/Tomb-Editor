using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Rendering
{
    // Render command for a piece of text. The actual glyph layout is computed by
    // RenderingFont.ParseString during RenderText() — at the SwapChain layer the only
    // thing we keep is the source string + positioning intent.
    //
    // Position model: Pos is in NDC (-1..+1), PixelPos is an additional pixel offset
    // applied after Pos. ScreenAlignment is the anchor of the text rectangle relative
    // to the screen (0,0 = top-left, 1,1 = bottom-right). TextAlignment is the anchor
    // of the rectangle within itself (0.5,0.5 = centered around the position).
    public class Text
    {
        public RenderingFont Font;
        public string String;
        public Vector2 Pos = new Vector2();
        public Vector2 PixelPos = new Vector2();
        public Vector2 TextAlignment = new Vector2(0.5f);
        public Vector2 ScreenAlignment = new Vector2(0.5f);
        public Vector2 Alignment { set { TextAlignment = ScreenAlignment = value; } }
        public bool Overlay; // draw a solid colored background rectangle behind the text
    }

    // A single textured quad in NDC space.
    //   Pos00 - Pos10
    //     |       |
    //   Pos01 - Pos11
    // Depth = null means the sprite is depth-independent (drawn after the gizmo with
    // depth test disabled); a value places it in the depth buffer for proper sorting.
    public class Sprite
    {
        public RenderingTexture Texture;
        public Vector2 Pos00 = new Vector2(0.0f, 0.0f);
        public Vector2 Pos10 = new Vector2(1.0f, 0.0f);
        public Vector2 Pos01 = new Vector2(0.0f, 1.0f);
        public Vector2 Pos11 = new Vector2(1.0f, 1.0f);
        public Vector2 PosStart { set { Pos00 = value; Pos01.X = value.X; Pos10.Y = value.Y;  } }
        public Vector2 PosEnd { set { Pos11 = value; Pos10.X = value.X; Pos01.Y = value.Y;  } }
        public float? Depth = null;
        public Vector4 Tint = Vector4.One;
    }

    // The SwapChain wraps both the DXGI presentation surface AND the high-level 2D
    // overlay primitives (sprites, glyphs). The overlay primitives live here rather
    // than on the device because they must render directly into THIS back buffer with
    // its own depth attachment.
    public abstract class RenderingSwapChain : IDisposable
    {
        public class Description
        {
            public IntPtr WindowHandle; // Win32 HWND of the host control
            public VectorInt2 Size;
            public bool Antialias;
        }

        public VectorInt2 Size { get; protected set; }

        // Set to non-null on device-removed / device-hung errors. The editor reads it on
        // every frame and switches to "safe mode" instead of crashing. See
        // Panel3DDraw.DrawScene for the reading side.
        public Exception RenderException { get; protected set; }

        public abstract void Dispose();
        public abstract void Clear(Vector4 color);
        public abstract void ClearDepth();
        public abstract void Present();
        public abstract void Resize(VectorInt2 newSize);

        // Legacy bookkeeping hook from the DX11 backend. DX11 callers need to
        // re-bind the back-buffer view + reset the depth-stencil state after
        // any third-party SharpDX.Toolkit usage that may have mutated the
        // device state. Under Vulkan the equivalent setup happens inside
        // Clear() (BeginRenderPass + viewport/scissor) so this is a no-op.
        // Promoted from a DX11-only method so callers can avoid hard-casts.
        public virtual void BindForce() { }

        // Renders an unsorted list of sprites in one draw call. Caller is responsible
        // for depth-sorting if Depth is set. linearFilter=true uses anisotropic mipmap
        // filtering; false picks nearest-neighbour (e.g. for crisp icons).
        public abstract void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, bool noZ, List<Sprite> sprites);

        /// <summary>Note that all fonts used in one call must be in the same texture allocator!</summary>
        public abstract void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays);
        // High-level entry point: takes Text descriptors, runs the GDI-driven layout via
        // RenderingFont.ParseString to produce per-glyph quads, batches them all into a
        // single RenderGlyphs() call. Every Text in one call must share the same font
        // atlas (RenderingFont.TextureAllocator) — different atlases would need separate
        // SRV bindings, and we deliberately do not split the batch.
        public void RenderText(IEnumerable<Text> texts)
        {
            var glyphRenderInfos = new List<RenderingFont.GlyphRenderInfo>();
            var overlayRectangles = new List<RectangleInt2>();
            RenderingTextureAllocator textureAllocator = null;

            foreach (Text text in texts)
            {
                if (text == null || string.IsNullOrEmpty(text.String))
                    continue;

                // Convert from NDC (Pos) + pixel offset to absolute pixel position the
                // font layout expects. ScreenAlignment is biased by (-1..+1) to produce
                // an offset relative to the screen anchor (Y is flipped: -0.5 puts the
                // anchor at top, +0.5 at bottom).
                Vector2 pixelPos = text.PixelPos + text.Pos * Size * 0.5f;
                pixelPos += (text.ScreenAlignment * 2 - new Vector2(1)) * Size * new Vector2(0.5f, -0.5f);
                RectangleInt2 rect = text.Font.ParseString(text.String, text.Overlay, glyphRenderInfos, VectorInt2.FromRounded(pixelPos), text.TextAlignment);
                if (rect != RectangleInt2.Zero) overlayRectangles.Add(rect);

                if (textureAllocator == null)
                    textureAllocator = text.Font.TextureAllocator;
                else if (textureAllocator != text.Font.TextureAllocator)
                    throw new ArgumentException("Texts are using different texture allocators. This is not allowed in a single 'RenderText' call.");
            }
            if (glyphRenderInfos.Count == 0)
                return;
            RenderGlyphs(textureAllocator, glyphRenderInfos, overlayRectangles);
        }
        public void RenderText(params Text[] text) => RenderText((IEnumerable<Text>)text);
    }
}
