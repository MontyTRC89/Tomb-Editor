using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor.Controls
{
    public partial class PanelAnimatedTextures : PictureBox
    {
        private Editor _editor;
        private Pen _penSelectedTexture = new Pen(Brushes.Red, 3);

        public short SelectedX { get; set; }
        public short SelectedY { get; set; }
        public short Page { get; set; }
        public bool IsTextureSelected { get; set; }

        public PanelAnimatedTextures()
        { }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            SelectedX = (short)(Math.Floor(e.X / 64.0f) * 64.0f);
            SelectedY = (short)(Math.Floor(e.Y / 64.0f) * 64.0f);
            Page = (short)Math.Floor(SelectedY / 256.0f);
            SelectedY -= (short)(Page * 256);

            IsTextureSelected = true;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (_editor == null && Editor.Instance == null)
                return;
            else
                _editor = Editor.Instance;

            Graphics g = pe.Graphics;

            g.DrawLine(Pens.White, new Point(64, 0), new Point(64, Height));
            g.DrawLine(Pens.White, new Point(128, 0), new Point(128, Height));
            g.DrawLine(Pens.White, new Point(192, 0), new Point(192, Height));

            for (int i = 1; i < Height / 64; i++)
            {
                g.DrawLine(Pens.White, new Point(0, 64 * i), new Point(255, 64 * i));
            }

            if (IsTextureSelected)
            {
                g.DrawRectangle(_penSelectedTexture, new Rectangle(SelectedX, Page * 256 + SelectedY, 64, 64));
            }
        }
    }
}
