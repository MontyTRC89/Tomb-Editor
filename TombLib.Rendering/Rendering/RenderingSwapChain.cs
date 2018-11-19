using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Rendering
{
    public class Text
    {
        public RenderingFont Font;
        public string String;
        public Vector2 Pos = new Vector2();
        public Vector2 PixelPos = new Vector2();
        public Vector2 TextAlignment = new Vector2(0.5f);
        public Vector2 ScreenAlignment = new Vector2(0.5f);
        public Vector2 Alignment { set { TextAlignment = ScreenAlignment = value; } }
        public bool Overlay;
    }

    public class Sprite
    {
        public RenderingTexture Texture;
        public Vector2 Pos00 = new Vector2(0.0f, 0.0f);
        public Vector2 Pos10 = new Vector2(1.0f, 0.0f);
        public Vector2 Pos01 = new Vector2(0.0f, 1.0f);
        public Vector2 Pos11 = new Vector2(1.0f, 1.0f);
        public Vector2 PosStart { set { Pos00 = value; Pos01.X = value.X; Pos10.Y = value.Y;  } }
        public Vector2 PosEnd { set { Pos11 = value; Pos10.X = value.X; Pos01.Y = value.Y;  } }
    }

    public abstract class RenderingSwapChain : IDisposable
    {
        public class Description
        {
            public IntPtr WindowHandle;
            public VectorInt2 Size;
            public bool Antialias;
        }

        public VectorInt2 Size { get; protected set; }

        public abstract void Dispose();
        public abstract void Clear(Vector4 color);
        public abstract void ClearDepth();
        public abstract void Present();
        public abstract void Resize(VectorInt2 newSize);
        public abstract void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, params Sprite[] sprites);
        /// <summary>Note that all fonts used in one call must be in the same texture allocator!</summary>
        public abstract void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays);
        public void RenderText(IEnumerable<Text> texts)
        {
            // Collect actual glyphs to render
            var glyphRenderInfos = new List<RenderingFont.GlyphRenderInfo>();
            var overlayRectangles = new List<RectangleInt2>();
            RenderingTextureAllocator textureAllocator = null;

            foreach (Text text in texts)
            {
                // Build glyphs using the right font
                Vector2 pixelPos = text.PixelPos + text.Pos * Size * 0.5f;
                pixelPos += (text.ScreenAlignment * 2 - new Vector2(1)) * Size * new Vector2(0.5f, -0.5f);
                RectangleInt2 rect = text.Font.ParseString(text.String, text.Overlay, glyphRenderInfos, VectorInt2.FromRounded(pixelPos), text.TextAlignment);
                if (rect != RectangleInt2.Zero) overlayRectangles.Add(rect);

                // Check texture allocator
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
