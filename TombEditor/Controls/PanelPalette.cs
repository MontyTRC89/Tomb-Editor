using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor.Controls
{
    public partial class PanelPalette : PictureBox
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SelectedColor
        {
            get { return getColorFromPalette(_selectedColorCoord); }
        }
        public event EventHandler SelectedColorChanged;
        
        private Point _selectedColorCoord = new Point(-1, -1);
        private static Pen _selectionPen = Pens.White;
        private static Pen _gridPen = Pens.Black;
        private const int _paletteWidth = 64;
        private const int _paletteHeight = 10;
        private const float _paletteCellWidth = 10;
        private const float _paletteCellHeight = 10;
        private List<Color> _palette { get; } = new List<Color>();

        public PanelPalette()
        {
            if (DesignMode || (LicenseManager.UsageMode == LicenseUsageMode.Designtime))
                return;

            using (FileStream stream = new FileStream("Editor\\Misc\\Palette.bin", FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader readerPalette = new BinaryReader(stream))
                    while (readerPalette.BaseStream.Position < readerPalette.BaseStream.Length)
                        _palette.Add(System.Drawing.Color.FromArgb(255, readerPalette.ReadByte(), readerPalette.ReadByte(), readerPalette.ReadByte()));
        }
        
        private Color getColorFromPalette(Point point)
        {
            if (_palette == null)
                return Color.Magenta;
            int index = point.Y * _paletteWidth + point.X;
            if ((index < 0) || (index >= _palette.Count))
                return Color.Magenta;
            return _palette[index];
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                _selectedColorCoord = new Point((int)(e.X / _paletteCellWidth), (int)(e.Y / _paletteCellHeight));
                SelectedColorChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            for (int y = 0; y < _paletteHeight; y++)
                for (int x = 0; x < _paletteWidth; x++)
                {
                    Color color = getColorFromPalette(new Point(x, y));
                    using (Brush brush = new SolidBrush(color))
                        e.Graphics.FillRectangle(brush, x * _paletteCellWidth, y * _paletteCellHeight, _paletteCellWidth, _paletteCellHeight);
                    e.Graphics.DrawRectangle(_gridPen, x * 10, y * 10, 10, 10);
                }

            if ((_selectedColorCoord.X >= 0) && (_selectedColorCoord.Y >= 0))
                e.Graphics.DrawRectangle(_selectionPen, _selectedColorCoord.X * _paletteCellWidth, 
                    _selectedColorCoord.Y * _paletteCellHeight, _paletteCellWidth, _paletteCellHeight);
        }
    }
}
