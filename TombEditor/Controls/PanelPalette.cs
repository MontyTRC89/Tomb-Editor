using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls
{
    public class PanelPalette : PictureBox
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<ColorC> Palette => new List<ColorC>(_palette);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SelectedColor => GetColorFromPalette(_selectedColorCoord);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size PaletteSize => new Size((int)(Math.Floor((ClientSize.Width - Padding.Horizontal)  / _paletteCellWidth)),
                                            (int)(Math.Floor((ClientSize.Height - Padding.Vertical) / _paletteCellHeight)));

        public bool Editable { get; set; } = true;

        private static readonly Pen _selectionPen = Pens.White;
        private static readonly Pen _gridPen = Pens.Black;
        private const float _paletteCellWidth = 10;
        private const float _paletteCellHeight = 10;

        private Point _selectedColorCoord = new Point(-1, -1);
        private Size _oldPaletteSize = new Size();

        private Editor _editor = Editor.Instance;
        private List<ColorC> _palette { get; } = new List<ColorC>();

        public PanelPalette()
        {
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            SetStyle(ControlStyles.Selectable |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            TabStop = true;
            _oldPaletteSize = PaletteSize;
        }

        public void LoadPalette(List<ColorC> palette)
        {
            if (palette.Count < 1) return; // No suitable color data found
            _palette.Clear();
            foreach (var c in palette) _palette.Add(new ColorC(c.R, c.G, c.B));
            Invalidate();
        }

        private Color GetColorFromPalette(Point point)
        {
            if (_palette == null)
                return BackColor;
            int index = point.Y * PaletteSize.Width + point.X;
            if (index < 0 || index >= _palette.Count)
                return BackColor;

            var c = _palette[index];
            return Color.FromArgb(255, c.R, c.G, c.B);
        }

        private void SetColorToPalette(Color color, Point point)
        {
            int index = point.Y * PaletteSize.Width + point.X;
            if (_palette == null || index < 0)
                return;

            var c = new ColorC(color.R, color.G, color.B);
            if (index >= _palette.Count)
                _palette.Add(c);
            else
                _palette[index] = c;
        }

        private void ChangeColorByMouse(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Focus();
                _selectedColorCoord = new Point((int)MathC.Clamp((e.X / _paletteCellWidth), 0, PaletteSize.Width - 1),
                                                (int)MathC.Clamp((e.Y / _paletteCellHeight), 0, PaletteSize.Height - 1));

                if (_editor.SelectedObject is LightInstance)
                {
                    var light = _editor.SelectedObject as LightInstance;
                    light.Color = SelectedColor.ToFloat3Color() * 2.0f;
                    light.Room.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                    _editor.ObjectChange(light, ObjectChangeType.Change);
                }
                else if (_editor.SelectedObject is StaticInstance)
                {
                    var instance = _editor.SelectedObject as StaticInstance;
                    instance.Color = SelectedColor.ToFloat3Color() * 2.0f;
                    _editor.ObjectChange(instance, ObjectChangeType.Change);
                }
                else if (_editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main && _editor.SelectedObject is MoveableInstance)
                {
                    var instance = _editor.SelectedObject as MoveableInstance;
                    instance.Color = SelectedColor.ToFloat3Color() * 2.0f;
                    _editor.ObjectChange(instance, ObjectChangeType.Change);
                }

                _editor.LastUsedPaletteColourChange(SelectedColor);
                Invalidate();
            }
        }

        private void PickColour(bool onlyFromPalette = true)
        {
            using (var colorDialog = new RealtimeColorDialog(
                _editor.Configuration.ColorDialog_Position.X,
                _editor.Configuration.ColorDialog_Position.Y,
                null,
                _editor.Configuration.UI_ColorScheme))
            {
                colorDialog.Color = GetColorFromPalette(_selectedColorCoord);

                if (!onlyFromPalette)
                {
                    var obj = _editor.SelectedObject;
                    if (obj is LightInstance)
                        colorDialog.Color = ((obj as LightInstance).Color * 0.5f).ToWinFormsColor();
                    else if (obj is StaticInstance)
                        colorDialog.Color = ((obj as StaticInstance).Color * 0.5f).ToWinFormsColor();
                    else if (_editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main && obj is MoveableInstance)
                        colorDialog.Color = (obj as MoveableInstance).Color.ToWinFormsColor();
                }

                if (colorDialog.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    SetColorToPalette(colorDialog.Color, _selectedColorCoord);
                    _editor.Level.Settings.Palette = Palette;
                    Invalidate();
                }

                _editor.Configuration.ColorDialog_Position = colorDialog.Position;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                if (Control.ModifierKeys.HasFlag(Keys.Alt))
                    PickColour(false);
                else
                {
                    // Save undo in case we're editing selected light colour
                    if (_editor.SelectedObject is LightInstance ||
                        _editor.SelectedObject is StaticInstance ||
                        _editor.SelectedObject is MoveableInstance)
                        _editor.UndoManager.PushObjectPropertyChanged((PositionBasedObjectInstance)_editor.SelectedObject);

                    _editor.ToggleHiddenSelection(true);
                    ChangeColorByMouse(e);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _editor.ToggleHiddenSelection(false);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (Editable && e.Button == MouseButtons.Left)
                PickColour();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
            {
                ChangeColorByMouse(e);
                Update(); // Invalidate gets extremely slow here for some reason
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            // Recalculate selected color coordinate
            if (_selectedColorCoord != new Point(-1))
            {
                int colorIndex = _oldPaletteSize.Width * _selectedColorCoord.Y + _selectedColorCoord.X;
                _selectedColorCoord = new Point((colorIndex % PaletteSize.Width), colorIndex / PaletteSize.Width);
            }
            _oldPaletteSize = PaletteSize;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var startX = Padding.Left;
            var startY = Padding.Top;

            // Draw colours
            for (int y = 0; y < PaletteSize.Height; y++)
                for (int x = 0; x < PaletteSize.Width; x++)
                {
                    Color color = GetColorFromPalette(new Point(x, y));
                    using (Brush brush = new SolidBrush(color))
                        e.Graphics.FillRectangle(brush, startX + x * _paletteCellWidth, startY + y * _paletteCellHeight, _paletteCellWidth, _paletteCellHeight);
                }

            var rect = new Rectangle(startX, startY, PaletteSize.Width * (int)_paletteCellWidth, 
                                                     PaletteSize.Height * (int)_paletteCellHeight);
            // Draw outline rect
            e.Graphics.DrawRectangle(_gridPen, rect);

            // Draw grid
            for (int y = 0; y < PaletteSize.Height; y++)
                for (int x = 0; x < PaletteSize.Width; x++)
            {
                int lineX = startX + x * (int)_paletteCellWidth;
                int lineY = startY + y * (int)_paletteCellHeight;

                e.Graphics.DrawLine(_gridPen, new Point(lineX, startY), new Point(lineX, rect.Bottom));
                e.Graphics.DrawLine(_gridPen, new Point(startX, lineY), new Point(rect.Right, lineY));
            }

            // Draw selection rect
            if (_selectedColorCoord.X >= 0 && _selectedColorCoord.Y >= 0 &&
                _selectedColorCoord.X < PaletteSize.Width && _selectedColorCoord.Y < PaletteSize.Height)
                e.Graphics.DrawRectangle(_selectionPen, startX + _selectedColorCoord.X * _paletteCellWidth,
                    startY + _selectedColorCoord.Y * _paletteCellHeight, _paletteCellWidth, _paletteCellHeight);
        }
    }
}
