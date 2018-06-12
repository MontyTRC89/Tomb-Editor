using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        public Vector2 Alignment
        {
            get { return ScreenAlignment; }
            set { TextAlignment = ScreenAlignment = value; }
        }
    }

    public abstract class RenderingSwapChain : IDisposable
    {
        public class Description
        {
            public IntPtr WindowHandle;
            public VectorInt2 Size;
        }

        public VectorInt2 Size { get; protected set; }

        public abstract void Dispose();
        public abstract void Clear(Vector4 color);
        public abstract void ClearDepth();
        public abstract void Present();
        public abstract void Resize(VectorInt2 newSize);
        /// <summary>Note that all fonts used in one call must be in the same texture allocator!</summary>
        public abstract void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos);
        public void RenderText(IEnumerable<Text> texts)
        {
            // Collect actual glyphs to render
            List<RenderingFont.GlyphRenderInfo> glyphRenderInfos = new List<RenderingFont.GlyphRenderInfo>();
            RenderingTextureAllocator textureAllocator = null;
            foreach (Text text in texts)
            {
                // Build glyphs using the right font
                Vector2 pixelPos = text.PixelPos + text.Pos * Size * 0.5f;
                pixelPos += (text.ScreenAlignment * 2 - new Vector2(1)) * Size * new Vector2(0.5f, -0.5f);
                text.Font.ParseString(text.String, glyphRenderInfos, VectorInt2.FromRounded(pixelPos), text.TextAlignment);

                // Check texture allocator
                if (textureAllocator == null)
                    textureAllocator = text.Font.TextureAllocator;
                else if (textureAllocator != text.Font.TextureAllocator)
                    throw new ArgumentException("Texts are using different texture allocators. This is not allowed in a single 'RenderText' call.");
            }
            if (glyphRenderInfos.Count == 0)
                return;
            RenderGlyphs(textureAllocator, glyphRenderInfos);
        }
        public void RenderText(params Text[] text) => RenderText((IEnumerable<Text>)text);
    }
}
