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
    public partial class PanelPalette : PictureBox
    {
        private Editor _editor;

        private bool _drag;

        private short _x;
        private short _y;
        private short _w;
        private short _h;

        public Color SelectedColor { get; set; }

        private int _selectedColorIndex = -1;

        private Pen _pen;

        public PanelPalette()
        {
            InitializeComponent();

            _pen = new Pen(Brushes.White, 2);
        }

        public PanelPalette(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            _pen = new Pen(Brushes.White, 2);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_editor == null && Editor.Instance == null)
                return;
            else
                _editor = Editor.Instance;

            if (_editor.Palette == null) return;

            int x = (int)Math.Floor(e.X / 10.0f);
            int y = (int)Math.Floor(e.Y / 10.0f);

            _selectedColorIndex = y * 64 + x;

            SelectedColor = _editor.Palette[_selectedColorIndex];

            _editor.ChangeLightColorFromPalette();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (_editor == null && Editor.Instance == null)
                return;
            else
                _editor = Editor.Instance;

            if (_editor.Palette == null) return;

            Graphics g = pe.Graphics;

            int selectionX = 0;
            int selectionY = 0;

            int currentColor = 0;
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (currentColor > _editor.Palette.Count - 1) break;

                    Color color = _editor.Palette[currentColor];

                    g.FillRectangle(new SolidBrush(color), new Rectangle(x * 10, y * 10, 10, 10));
                    g.DrawRectangle(Pens.Black, new Rectangle(x * 10, y * 10, 10, 10));

                    if (currentColor == _selectedColorIndex)
                    {
                        selectionX = x;
                        selectionY = y;
                    }

                    currentColor++;
                }

                if (currentColor > _editor.Palette.Count - 1) break;
            }

            if (_selectedColorIndex != -1)
            {
                g.DrawRectangle(_pen, new Rectangle(selectionX * 10, selectionY * 10, 10, 10));
            }
        }
    }
}
