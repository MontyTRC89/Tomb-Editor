using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TombEditor;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    /// <summary>
    /// Sprite preview panel — a single <see cref="WadSprite"/>'s image,
    /// aspect-fit inside the panel. Pure GDI+ (no V2 RHI) because the legacy
    /// 3D path was overkill for a 2D bitmap.
    /// </summary>
    public class PanelRenderingSprite : Panel
    {
        private readonly Editor _editor;
        private List<WadSprite> _spriteList = new();

        private int _spriteID;
        public int SpriteID
        {
            get => _spriteID;
            set { _spriteID = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }

        public PanelRenderingSprite()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw, true);

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
                RebuildSpriteList();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _editor != null)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.LoadedWadsChangedEvent
             || obj is Editor.LevelChangedEvent
             || obj is Editor.EditorFocusedEvent)
            {
                RebuildSpriteList();
                Invalidate();
            }
        }

        private void RebuildSpriteList()
            => _spriteList = _editor?.Level?.Settings?.WadGetAllSprites() ?? new List<WadSprite>();

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var c = _editor?.Configuration?.UI_ColorScheme.Color3DBackground;
            if (!c.HasValue)
            {
                base.OnPaintBackground(e);
                return;
            }
            var v = c.Value;
            using var brush = new SolidBrush(Color.FromArgb(
                Clamp255(v.W * 255f), Clamp255(v.X * 255f),
                Clamp255(v.Y * 255f), Clamp255(v.Z * 255f)));
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_spriteList == null || SpriteID < 0 || SpriteID >= _spriteList.Count) return;
            var sprite = _spriteList[SpriteID];
            if (sprite.Texture?.Image == null) return;

            var img = sprite.Texture.Image;
            if (img.Width <= 0 || img.Height <= 0) return;

            // Aspect-fit inside 90% of the viewport, matching the legacy
            // RenderSprites's PosStart/PosEnd of ±0.9.
            float aspectImg  = (float)img.Width / img.Height;
            float aspectView = (float)ClientSize.Width / ClientSize.Height;
            float w, h;
            if (aspectImg >= aspectView) { w = ClientSize.Width  * 0.9f; h = w / aspectImg; }
            else                          { h = ClientSize.Height * 0.9f; w = h * aspectImg; }
            float x = (ClientSize.Width  - w) * 0.5f;
            float y = (ClientSize.Height - h) * 0.5f;

            using var bmp = ImageCToBitmap(img);
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode   = PixelOffsetMode.Half;
            e.Graphics.DrawImage(bmp, x, y, w, h);
        }

        private static int Clamp255(float v) => v < 0 ? 0 : v > 255 ? 255 : (int)v;

        private static Bitmap ImageCToBitmap(ImageC img)
        {
            // ImageC stores pixels in BGRA order, which matches GDI+
            // Format32bppArgb's in-memory byte layout (little-endian ARGB).
            var bmp  = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, img.Width, img.Height);
            var data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            try
            {
                byte[] bytes = img.ToByteArray();
                if (bytes.Length == data.Stride * img.Height)
                {
                    Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                }
                else
                {
                    // Stride mismatch (very rare) — copy row by row.
                    int rowBytes = img.Width * 4;
                    for (int row = 0; row < img.Height; row++)
                        Marshal.Copy(bytes, row * rowBytes, data.Scan0 + row * data.Stride, rowBytes);
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }
            return bmp;
        }
    }
}
