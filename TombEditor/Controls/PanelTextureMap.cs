using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
        private float LastY;
        private float _deltaX;
        private float _deltaY;

        private short _x;
        private short _y;
        private short _w;
        private short _h;

        private TextureTileType _triangle;

        public PanelTextureMap()
        {
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.SelectedTexturesChangedEvent)
            {
                // TODO Update the selected texture on the texture map
                Invalidate();
            }

            if ((obj is Editor.LevelChangedEvent) || (obj is Editor.LoadedTexturesChangedEvent))
                if (Image != _editor.Level.TextureMap)
                {
                    if (_editor.Level.TextureMap == null)
                    {
                        Image = null;
                        Height = 0;
                    }
                    else
                    {
                        Image = _editor.Level.TextureMap;
                        Height = 0;
                        Invalidate();
                    }
                    Invalidate();
                }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            MouseEventArgs args = (MouseEventArgs)e;

            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.Invisible = false;
            _editor.SelectedTexture = selectedTexture;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _drag = true;
            _lastX = e.X;
            LastY = e.Y;

            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.Invisible = false;
            _editor.SelectedTexture = selectedTexture;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_drag)
            {
                var selectedTexture = _editor.SelectedTexture;
                selectedTexture.Invisible = false;
                _editor.SelectedTexture = selectedTexture;

                _deltaX = e.X - _lastX;
                _deltaY = e.Y - LastY;

                _x = (short)(Math.Floor(_lastX / 16.0f) * 16);
                _y = (short)(Math.Floor(LastY / 16.0f) * 16);
                _w = (short)(Math.Ceiling(_deltaX / 16.0f) * 16);
                _h = (short)(Math.Ceiling(_deltaY / 16.0f) * 16);

                // verifico di non aver attraversato una pagina
                short page = (short)(Math.Floor(LastY / 256.0f));
                short maxHeight = (short)(256 - (_y - 256 * page));
                short maxWidth = (short)(256 - _x);
                if (_h > maxHeight)
                    _h = maxHeight;
                if (_w > maxWidth)
                    _w = maxWidth;

                if (_w < 0)
                {
                    _drag = false;
                    return;
                }

                if (_h < 0)
                {
                    _drag = false;
                    return;
                }
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            _deltaX = e.X - _lastX;
            _deltaY = e.Y - LastY;

            _w = (short)(Math.Ceiling(_deltaX / 16.0f) * 16);
            _h = (short)(Math.Ceiling(_deltaY / 16.0f) * 16);

            if (_w < 0)
            {
                _drag = false;
                return;
            }

            if (_h < 0)
            {
                _drag = false;
                return;
            }

            LevelTexture sample;

            // Check if I've not changed page
            short page = (short)(Math.Floor(LastY / 256.0f));

            TextureSelection selectedTexture = _editor.SelectedTexture;
            selectedTexture.Invisible = false;
            if (Math.Abs(_deltaX) < 8 && Math.Abs(_deltaY) < 8)
            {
                // Single click: 64x64 texture or confirm current texture
                if (selectedTexture.Index != -1)
                {
                    sample = _editor.Level.TextureSamples[selectedTexture.Index];

                    if (_lastX >= sample.X && _lastX <= sample.X + sample.Width && LastY >= sample.Y + sample.Page * 256 &&
                        LastY <= sample.Y + sample.Page * 256 + sample.Height)
                    {
                        sample = _editor.Level.TextureSamples[selectedTexture.Index];
                    }
                    else
                    {
                        _x = (short)(Math.Floor(_lastX / 64.0f) * 64);
                        _y = (short)(Math.Floor(LastY / 64.0f) * 64);
                        _w = 64;
                        _h = 64;

                        selectedTexture.Index = _editor.Level.AddTexture(_x, _y, _w, _h, selectedTexture.DoubleSided, selectedTexture.Transparent);
                        sample = _editor.Level.TextureSamples[selectedTexture.Index];
                    }
                }
                else
                {
                    _x = (short)(Math.Floor(_lastX / 64.0f) * 64);
                    _y = (short)(Math.Floor(LastY / 64.0f) * 64);
                    _w = 64;
                    _h = 64;

                    selectedTexture.Index = _editor.Level.AddTexture(_x, _y, _w, _h, selectedTexture.DoubleSided, selectedTexture.Transparent);
                    sample = _editor.Level.TextureSamples[selectedTexture.Index];
                }
            }
            else
            {
                short maxHeight = (short)(256 - (_y - 256 * page));
                short maxWidth = (short)(256 - _x);
                if (_h > maxHeight)
                    _h = maxHeight;
                if (_w > maxWidth)
                    _w = maxWidth;

                // If drag, then variable size texture
                selectedTexture.Index = _editor.Level.AddTexture(_x, _y, _w, _h, selectedTexture.DoubleSided, selectedTexture.Transparent);
                sample = _editor.Level.TextureSamples[selectedTexture.Index];
            }

            // Serach the correct triangle
            if (_lastX >= sample.X && _lastX <= sample.X + sample.Width / 2 && LastY >= sample.Y + page * 256 &&
                LastY <= sample.Y + page * 256 + sample.Height / 2)
            {
                _triangle = TextureTileType.TriangleNW;
            }

            if (_lastX >= sample.X + sample.Width / 2 && _lastX <= sample.X + sample.Width && LastY >= sample.Y + page * 256 &&
                LastY <= sample.Y + page * 256 + sample.Height / 2)
            {
                _triangle = TextureTileType.TriangleNE;
            }

            if (_lastX >= sample.X + sample.Width / 2 && _lastX <= sample.X + sample.Width && LastY >= sample.Y + page * 256 + sample.Height / 2 &&
                LastY <= sample.Y + page * 256 + sample.Height)
            {
                _triangle = TextureTileType.TriangleSE;
            }

            if (_lastX >= sample.X && _lastX <= sample.X + sample.Width / 2 && LastY >= sample.Y + page * 256 + sample.Height / 2 &&
                LastY <= sample.Y + page * 256 + sample.Height)
            {
                _triangle = TextureTileType.TriangleSW;
            }

            selectedTexture.Triangle = _triangle;
            _editor.SelectedTexture = selectedTexture;

            _drag = false;
            
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            if (_editor == null)
                return;
            
            Graphics g = pe.Graphics;

            if (_drag)
            {
                g.DrawRectangle(Pens.White, new Rectangle(_x, _y, _w, _h));
            }
            else
            {
                if (_editor.SelectedTexture.Index != -1)
                {
                    LevelTexture texture = _editor.Level.TextureSamples[_editor.SelectedTexture.Index];

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
