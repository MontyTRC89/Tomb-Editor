using DarkUI.Controls;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombLib.Utils;
using RectangleF = System.Drawing.RectangleF;

namespace TombEditor.Controls
{
    public partial class PanelTextureMap : Panel
    {
        private Editor _editor;
        private LevelTexture _texture;

        private Vector2 _viewPosition;
        private float _viewScale = 1.0f;

        private Vector2? _startPos;
        private int? _selectedTexCoordIndex;
        private Vector2? _viewMoveMouseTexCoord;

        private static readonly Pen textureSelectionPen = new Pen(Brushes.Yellow, 2.0f) { LineJoin = LineJoin.Round };
        private static readonly Pen textureSelectionPenTriangle = new Pen(Brushes.Red, 1.0f) { LineJoin = LineJoin.Round };
        private static readonly Brush textureSelectionBrushSelection = Brushes.DeepSkyBlue;
        private const float textureSelectionPointWidth = 6.0f;
        private const float textureSelectionPointSelectionRadius = 13.0f;
        private const float maxTextureSize = 255;
        private const float viewMargin = 10;

        private DarkScrollBarC _hScrollBar = new DarkScrollBarC { ScrollOrientation = DarkScrollOrientation.Horizontal };
        private DarkScrollBarC _vScrollBar = new DarkScrollBarC { ScrollOrientation = DarkScrollOrientation.Vertical };

        private int _scrollSize => DarkUI.Config.Consts.ScrollBarSize;
        private int _scrollSizeTotal => _scrollSize + 2;

        public PanelTextureMap()
        {
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            BorderStyle = BorderStyle.FixedSingle;

            DoubleBuffered = true;

            // Scroll bars
            _hScrollBar.Size = new Size(Width - _scrollSize, _scrollSize);
            _hScrollBar.Location = new Point(0, Height - _scrollSize);
            _hScrollBar.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            _hScrollBar.ValueChanged += (sender, e) => { ViewPosition = new Vector2((float)_hScrollBar.ValueCentered, ViewPosition.Y); };

            _vScrollBar.Size = new Size(_scrollSize, Height - _scrollSize);
            _vScrollBar.Location = new Point(Width - _scrollSize, 0);
            _vScrollBar.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            _vScrollBar.ValueChanged += (sender, e) => { ViewPosition = new Vector2(ViewPosition.X, (float)_vScrollBar.ValueCentered); };

            Controls.Add(_vScrollBar);
            Controls.Add(_hScrollBar);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
                _hScrollBar.Dispose();
                _vScrollBar.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateScrollBars();
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.SelectedTexturesChangedEvent)
            {
                // TODO Update the selected texture on the texture map
                Invalidate();
            }

            if ((obj is Editor.LevelChangedEvent) || (obj is Editor.LoadedTexturesChangedEvent))
            {
                _texture = _editor.Level.Settings.Textures.Count > 0 ? _editor.Level.Settings.Textures[0] : null;
                Height = _texture == null ? 0 : _texture.Image.Height;

                ViewPosition = new Vector2(128, Height / 2);
                ViewScale = 1.0f;

                Invalidate();
            }
        }
        
        private void UpdateScrollBars()
        {
            bool hasTexture = _texture?.IsAvailable ?? false;
            
            _vScrollBar.SetViewCentered(
                -viewMargin, 
                (hasTexture ? _texture.Image.Height : 1) + viewMargin * 2,
                (Height - _scrollSizeTotal) / ViewScale,
                ViewPosition.Y, hasTexture);
            _hScrollBar.SetViewCentered(
                -viewMargin,
                (hasTexture ? _texture.Image.Width : 1) + viewMargin * 2,
                (Width - _scrollSizeTotal) / ViewScale,
                ViewPosition.X, hasTexture);
        }

        private Vector2 FromVisualCoord(PointF pos, bool limited = true)
        {
             Vector2 textureCoord = new Vector2(
                (pos.X - Width * 0.5f) / ViewScale + ViewPosition.X,
                (pos.Y - Height * 0.5f) / ViewScale + ViewPosition.Y);
            if (limited)
                textureCoord = Vector2.Min(_texture.Image.Size - new Vector2(0.5f), Vector2.Max(new Vector2(0.5f), textureCoord));
            return textureCoord;
        }

        private PointF ToVisualCoord(Vector2 texCoord)
        {
            return new PointF(
                (texCoord.X - ViewPosition.X) * ViewScale + Width * 0.5f,
                (texCoord.Y - ViewPosition.Y) * ViewScale + Height * 0.5f);
        }

        private void MoveToFixedPoint(PointF visualPoint, Vector2 worldPoint)
        {
            //Adjust ViewPosition in such a way, that the FixedPoint does not move visually
            ViewPosition = -worldPoint;
            ViewPosition = -FromVisualCoord(visualPoint, false);
        }

        private void LimitPosition()
        {
            bool hasTexture = _texture?.IsAvailable ?? false;
            Vector2 minimum = new Vector2(-viewMargin);
            Vector2 maximum = (hasTexture ? _texture.Image.Size : new Vector2(1)) + new Vector2(viewMargin);
            ViewPosition = Vector2.Min(maximum, Vector2.Max(minimum, ViewPosition));
        }

        private static Vector2 Quantize(Vector2 texCoord, bool endX, bool endY, Keys keyModifiers)
        {
            if (keyModifiers.HasFlag(Keys.Alt))
                return texCoord;
            else
            {
                float RoundingPrecision;
                if (keyModifiers.HasFlag(Keys.Shift))
                    RoundingPrecision = 64.0f;
                else if (keyModifiers.HasFlag(Keys.Control))
                    RoundingPrecision = 1.0f;
                else
                    RoundingPrecision = 16.0f;
                
                texCoord -= new Vector2(endX ? -0.5f : 0.5f, endY ? -0.5f : 0.5f);
                texCoord /= RoundingPrecision;
                texCoord = new Vector2((float)Math.Round(texCoord.X), (float)Math.Round(texCoord.Y));
                texCoord *= RoundingPrecision;
                texCoord += new Vector2( endX ? -0.5f : 0.5f, endY ? -0.5f : 0.5f);
                
                return texCoord;
            }
        }

        private void SetRectangularTextureWithMouse(Vector2 texCoordStart, Vector2 texCoordEnd)
        {
            Vector2 texCoordStartQuantized = Quantize(texCoordStart, texCoordStart.X > texCoordEnd.X, texCoordStart.Y > texCoordEnd.Y, ModifierKeys);
            Vector2 texCoordEndQuantized = Quantize(texCoordEnd, !(texCoordStart.X > texCoordEnd.X), !(texCoordStart.Y > texCoordEnd.Y), ModifierKeys);
            texCoordEndQuantized = Vector2.Min(texCoordStartQuantized + new Vector2(maxTextureSize),
                Vector2.Max(texCoordStartQuantized - new Vector2(maxTextureSize), texCoordEndQuantized));

            TextureArea selectedTexture = _editor.SelectedTexture;
            selectedTexture.TexCoord0 = new Vector2(texCoordStartQuantized.X, texCoordEndQuantized.Y);
            selectedTexture.TexCoord1 = texCoordStartQuantized;
            selectedTexture.TexCoord2 = new Vector2(texCoordEndQuantized.X, texCoordStartQuantized.Y);
            selectedTexture.TexCoord3 = texCoordEndQuantized;
            selectedTexture.Texture = _texture;
            _editor.SelectedTexture = selectedTexture;
        }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _startPos = null;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    var mousePos = FromVisualCoord(e.Location);

                    // Check if mouse was on existing texture
                    var selectedTexture = _editor.SelectedTexture;
                    if (selectedTexture.Texture == _texture)
                    {
                        var texCoords = selectedTexture.TexCoords
                            .Where(texCoordPair => Vector2.Distance(texCoordPair.Value, mousePos) < textureSelectionPointSelectionRadius)
                            .OrderBy(texCoordPair => Vector2.Distance(texCoordPair.Value, mousePos))
                            .ToList();
                        if (texCoords.Count != 0)
                        {
                            // Select texture coords
                            _selectedTexCoordIndex = texCoords.First().Key;
                            Invalidate();
                            break;
                        }
                    }
                    if (_selectedTexCoordIndex != null)
                    {
                        _selectedTexCoordIndex = null;
                        Invalidate();
                    }

                    // Start selection ...
                    _startPos = mousePos;
                    break;

                case MouseButtons.Right:
                    // Move view with mouse curser
                    // Mouse curser is a fixed point
                    _viewMoveMouseTexCoord = FromVisualCoord(e.Location);
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_selectedTexCoordIndex.HasValue)
                    {
                        TextureArea currentTexture = _editor.SelectedTexture;

                        // Determine bounds
                        Vector2 texCoordMin = new Vector2(float.PositiveInfinity);
                        Vector2 texCoordMax = new Vector2(float.NegativeInfinity);
                        for (int i = 0; i < 4; ++i)
                        {
                            if (i == _selectedTexCoordIndex)
                                continue;
                            texCoordMin = Vector2.Min(texCoordMin, currentTexture.GetTexCoord(i));
                            texCoordMax = Vector2.Max(texCoordMax, currentTexture.GetTexCoord(i));
                        }
                        Vector2 texCoordMinBounds = texCoordMax - new Vector2(maxTextureSize);
                        Vector2 texCoordMaxBounds = texCoordMin + new Vector2(maxTextureSize);
                        
                        //Move texture coord
                        Vector2 newTextureCoord = FromVisualCoord(e.Location);
                        
                        float minArea = float.PositiveInfinity;
                        TextureArea minAreaTextureArea = currentTexture;
                        for (int i = 0; i < 4; ++i)
                        {
                            currentTexture.SetTexCoord(_selectedTexCoordIndex.Value,
                                Vector2.Min(texCoordMaxBounds, Vector2.Max(texCoordMinBounds,
                                    Quantize(newTextureCoord, (i & 1) != 0, (i & 2) != 0, ModifierKeys))));

                            float area = Math.Abs(currentTexture.QuadArea);
                            if (area < minArea)
                            {
                                minAreaTextureArea = currentTexture;
                                minArea = area;
                            }
                        }

                        // Use the configuration that covers the most area
                        _editor.SelectedTexture = minAreaTextureArea;
                    }

                    if (_startPos.HasValue)
                        SetRectangularTextureWithMouse(_startPos.Value, FromVisualCoord(e.Location));
                    break;

                case MouseButtons.Right:
                    // Move view with mouse curser
                    // Mouse curser is a fixed point
                    if (_viewMoveMouseTexCoord.HasValue)
                    {
                        MoveToFixedPoint(e.Location, _viewMoveMouseTexCoord.Value);
                        LimitPosition();
                    }
                    break;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_startPos.HasValue)
                        if (ModifierKeys.HasFlag(Keys.Shift))
                            SetRectangularTextureWithMouse(_startPos.Value, FromVisualCoord(e.Location));
                    break;
            }
            _viewMoveMouseTexCoord = null;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Vector2 FixedPointInWorld = FromVisualCoord(e.Location);
            ViewScale *= (float)Math.Exp(e.Delta * _editor.Configuration.TextureMap_NavigationSpeedMouseWheelZoom);
            MoveToFixedPoint(e.Location, FixedPointInWorld);
        }

        private static void DrawImageUnshifted(Graphics g, RectangleF destinationArea, Image image)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_editor == null)
                return;

            e.Graphics.IntersectClip(new RectangleF(0, 0, Width - _scrollSizeTotal, Height - _scrollSizeTotal));

            // Only proceed if texture is actually available
            if (_texture?.IsAvailable ?? false)
            {
                PointF drawStart = ToVisualCoord(new Vector2(0.0f, 0.0f));
                PointF drawEnd = ToVisualCoord(new Vector2(_texture.Image.Width, _texture.Image.Height));
                RectangleF drawArea = RectangleF.FromLTRB(drawStart.X, drawStart.Y, drawEnd.X, drawEnd.Y);

                // Draw background
                using (var textureBrush = new TextureBrush(Properties.Resources.TransparentBackground))
                    e.Graphics.FillRectangle(textureBrush, drawArea);

                // Draw image
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                _texture.Image.GetTempSystemDrawingBitmap((tempBitmap) =>
                    {
                        // System.Drawing being silly, it draws the first row of pixels only half, so everything would be shifted
                        // To work around it, we have to do some silly coodinate changes :/
                        e.Graphics.DrawImage(tempBitmap,
                            new RectangleF(drawArea.X, drawArea.Y, drawArea.Width + 0.5f * ViewScale, drawArea.Height + 0.5f * ViewScale),
                            new RectangleF(-0.5f, -0.5f, tempBitmap.Width + 0.5f, tempBitmap.Height + 0.5f),
                            GraphicsUnit.Pixel);
                    });
            
                // Draw selection
                var selectedTexture = _editor.SelectedTexture;
                if (selectedTexture.Texture == _texture)
                {
                    // This texture is currently selected ...
                    PointF[] points = new PointF[]
                        {
                            ToVisualCoord(selectedTexture.TexCoord0),
                            ToVisualCoord(selectedTexture.TexCoord1),
                            ToVisualCoord(selectedTexture.TexCoord2),
                            ToVisualCoord(selectedTexture.TexCoord3)
                        };

                    e.Graphics.DrawPolygon(textureSelectionPen, points);
                    e.Graphics.DrawPolygon(textureSelectionPenTriangle, new PointF[] { points[0], points[1], points[2] });

                    for (int i = 0; i < 4; ++i)
                    {
                        Brush brush = _selectedTexCoordIndex == i ? textureSelectionBrushSelection : textureSelectionPen.Brush;
                        e.Graphics.FillRectangle(brush,
                            points[i].X - textureSelectionPointWidth * 0.5f, points[i].Y - textureSelectionPointWidth * 0.5f,
                            textureSelectionPointWidth, textureSelectionPointWidth);
                    }
                }
            }

            // Draw border next to scroll bars
            using (Pen pen = new Pen(DarkUI.Config.Colors.LighterBackground, 1.0f))
                e.Graphics.DrawRectangle(pen, new RectangleF(-1, -1, Width - _scrollSizeTotal, Height - _scrollSizeTotal));
        }

        [DefaultValue(BorderStyle.FixedSingle)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector2 ViewPosition
        {
            get { return _viewPosition; }
            set
            {
                if (_viewPosition == value)
                    return;
                _viewPosition = value;
                UpdateScrollBars();
                Invalidate();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ViewScale
        {
            get { return _viewScale; }
            set
            {
                if (_viewScale == value)
                    return;
                _viewScale = value;
                UpdateScrollBars();
                Invalidate();
            }
        }
    }
}
