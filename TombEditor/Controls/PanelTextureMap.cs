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
    public partial class PanelTextureMap : PictureBox
    {
        private Editor _editor;

        private bool _drag;
        private float _lastX;
        private float _lastY;
        private float _deltaX;
        private float _deltaY;

        private short _x;
        private short _y;
        private short _w;
        private short _h;

        private TextureTileType _triangle;

        public PanelTextureMap()
        {
            InitializeComponent();
        }

        public PanelTextureMap(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_editor == null && Editor.Instance == null)
                return;
            else
                _editor = Editor.Instance;

            MouseEventArgs args = (MouseEventArgs)e;

            /*   int texture = _editor.Level.AddTexture((short)args.X, (short)args.Y, (short)64, (short)64);
               _editor.SelectedTexture = texture;
               Invalidate();*/
            _editor.InvisiblePolygon = false;

            if (_editor.Level.TextureMap == null)
            {
                
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _drag = true;
            _lastX = e.X;
            _lastY = e.Y;

            _editor.InvisiblePolygon = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_drag)
            {
                _editor.InvisiblePolygon = false;

                _deltaX = e.X - _lastX;
                _deltaY = e.Y - _lastY;

                _x = (short)(Math.Floor(_lastX / 16.0f) * 16);
                _y = (short)(Math.Floor(_lastY / 16.0f) * 16);
                _w = (short)(Math.Ceiling(_deltaX / 16.0f) * 16);
                _h = (short)(Math.Ceiling(_deltaY / 16.0f) * 16);

                // verifico di non aver attraversato una pagina
                short page = (short)(Math.Floor(_lastY / 256.0f));
                short maxHeight = (short)(256 - (_y - 256 * page));
                short maxWidth = (short)(256 - _x);
                if (_h > maxHeight) _h = maxHeight;
                if (_w > maxWidth) _w = maxWidth;

                if (_w < 0)
                {
                    _drag = false;
                    return;
                //    _x -= _w;
               //     _w = (short)-_w;
                }

                if (_h < 0)
                {
                    _drag = false;
                    return;
                //    _y -= _h;
                //    _h = (short)-_h;
                }
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            _editor.InvisiblePolygon = false;

            _deltaX = e.X - _lastX;
            _deltaY = e.Y - _lastY;

           // _x = (short) (Math.Floor(_lastX / 16.0f) * 16);
        //    _y = (short)(Math.Floor(_lastY / 16.0f) * 16);
            _w = (short)(Math.Ceiling(_deltaX / 16.0f) * 16);
            _h = (short)(Math.Ceiling(_deltaY / 16.0f) * 16);

            if (_w < 0) 
            {
                _drag = false;
                return; _x -= _w;
                _w = (short)-_w;
            }

            if (_h < 0)
            {
                _drag = false;
                return;
                _y -= _h;
                _h = (short)-_h;
            }

            LevelTexture sample;

            // verifico di non aver attraversato una pagina
            short page = (short)(Math.Floor(_lastY / 256.0f));
            
            if (Math.Abs(_deltaX) < 8 && Math.Abs(_deltaY) < 8)
            {
                // click singolo, tile da 64x64 o confermo la texture corrente
                if (_editor.SelectedTexture != -1)
                {
                    sample = _editor.Level.TextureSamples[_editor.SelectedTexture];

                    if (_lastX >= sample.X && _lastX <= sample.X + sample.Width && _lastY >= sample.Y + sample.Page * 256 &&
                        _lastY <= sample.Y + sample.Page * 256 + sample.Height)
                    {
                        sample = _editor.Level.TextureSamples[_editor.SelectedTexture];
                    }
                    else
                    {
                        _x = (short)(Math.Floor(_lastX / 64.0f) * 64);
                        _y = (short)(Math.Floor(_lastY / 64.0f) * 64);
                        _w = 64;
                        _h = 64;

                        _editor.SelectedTexture = _editor.Level.AddTexture(_x, _y, _w, _h);
                        sample = _editor.Level.TextureSamples[_editor.SelectedTexture];
                    }
                }
                else
                {
                    _x = (short)(Math.Floor(_lastX / 64.0f) * 64);
                    _y = (short)(Math.Floor(_lastY / 64.0f) * 64);
                    _w = 64;
                    _h = 64;

                    _editor.SelectedTexture = _editor.Level.AddTexture(_x, _y, _w, _h);
                    sample = _editor.Level.TextureSamples[_editor.SelectedTexture];
                }

            /*    _x = (short)(Math.Floor(_lastX / 64.0f) * 64);
                _y = (short)(Math.Floor(_lastY / 64.0f) * 64);
                _w = 64;
                _h = 64;*/
            }
            else
            {
                short maxHeight = (short)(256 - (_y - 256 * page));
                short maxWidth = (short)(256 - _x);
                if (_h > maxHeight) _h = maxHeight;
                if (_w > maxWidth) _w = maxWidth;

                // trascinamento prolungato, tile di forma variabile
                _editor.SelectedTexture = _editor.Level.AddTexture(_x, _y, _w, _h);
                sample = _editor.Level.TextureSamples[_editor.SelectedTexture];
            }            

            // trovo il triangolo
            if (_lastX >= sample.X && _lastX <= sample.X + sample.Width / 2 && _lastY >= sample.Y + page * 256 &&
                _lastY <= sample.Y + page * 256 + sample.Height / 2)
            {
                _triangle = TextureTileType.TriangleNW;
            }

            if (_lastX >= sample.X + sample.Width / 2 && _lastX <= sample.X + sample.Width && _lastY >= sample.Y + page * 256 &&
                _lastY <= sample.Y + page * 256 + sample.Height / 2)
            {
                _triangle = TextureTileType.TriangleNE;
            }

            if (_lastX >= sample.X + sample.Width / 2 && _lastX <= sample.X + sample.Width && _lastY >= sample.Y + page * 256 + sample.Height / 2 &&
                _lastY <= sample.Y + page * 256 + sample.Height)
            {
                _triangle = TextureTileType.TriangleSE;
            }

            if (_lastX >= sample.X && _lastX <= sample.X + sample.Width / 2 && _lastY >= sample.Y + page * 256 + sample.Height / 2 &&
                _lastY <= sample.Y + page * 256 + sample.Height)
            {
                _triangle = TextureTileType.TriangleSW;
            }

            _editor.TextureTriangle = _triangle;

            _drag = false;

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

            if (_drag)
            {
                g.DrawRectangle(Pens.White, new Rectangle(_x, _y, _w, _h));
            }
            else
            {
                if (_editor.SelectedTexture != -1)
                {
                    LevelTexture texture = _editor.Level.TextureSamples[_editor.SelectedTexture];

                    g.DrawRectangle(Pens.White, new Rectangle(texture.X, texture.Y + 256 * texture.Page, texture.Width, texture.Height));

                    Pen penTriangle = Pens.Yellow;

                    // disegno il triangolo
                    if (_triangle == TextureTileType.TriangleNW)
                    {
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page));
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page), new Point(texture.X, texture.Y + 256 * texture.Page + texture.Height));
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page + texture.Height), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page));
                    }

                    if (_triangle == TextureTileType.TriangleNE)
                    {
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page));
                        g.DrawLine(penTriangle, new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page + texture.Height));
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page + texture.Height));
                    }

                    if (_triangle == TextureTileType.TriangleSE)
                    {
                        g.DrawLine(penTriangle, new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page + texture.Height));
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page + texture.Height), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page + texture.Height));
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page + texture.Height), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page));
                    }

                    if (_triangle == TextureTileType.TriangleSW)
                    {
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page), new Point(texture.X, texture.Y + 256 * texture.Page + texture.Height));
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page + texture.Height), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page + texture.Height));
                        g.DrawLine(penTriangle, new Point(texture.X, texture.Y + 256 * texture.Page), new Point(texture.X + texture.Width, texture.Y + 256 * texture.Page + texture.Height));
                    }
                }
            }
        }

    }
}
