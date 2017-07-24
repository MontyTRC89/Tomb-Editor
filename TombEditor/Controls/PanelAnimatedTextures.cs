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

        private short _x;
        private short _y;
        private short _page;

        private bool _selectedTexture;

        public short SelectedX
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        public short SelectedY
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }

        public short Page
        {
            get
            {
                return _page;
            }

            set
            {
                _page = value;
            }
        }

        public bool IsTextureSelected
        {
            get
            {
                return _selectedTexture;
            }
            set
            {
                _selectedTexture = value;
            }
        }

        private Pen _penSelectedTexture = new Pen(Brushes.Red, 3);

        public PanelAnimatedTextures()
        {
            InitializeComponent();
        }

        public PanelAnimatedTextures(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _x = (short)(Math.Floor(e.X / 64.0f) * 64.0f);
            _y = (short)(Math.Floor(e.Y/ 64.0f) * 64.0f);
            _page = (short)Math.Floor(_y / 256.0f);
            _y -= (short)(_page * 256);

            _selectedTexture = true;

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

            if (_selectedTexture)
            {
                g.DrawRectangle(_penSelectedTexture, new Rectangle(_x, _page * 256 + _y, 64, 64));
            }
        }
    }
}
