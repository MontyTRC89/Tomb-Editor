using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace TombLib.Controls
{
    /// <summary>
    /// Plain WinForms <see cref="Panel"/> base that legacy editor panels still
    /// inherit from. Provides the cursor-warping helper used during
    /// right/middle-mouse camera drag, plus an <see cref="AllowRendering"/>
    /// flag the editor sets while in modal flows to suppress WM_PAINT.
    /// </summary>
    public class RenderingPanel : Panel
    {
        public bool AllowRendering { get; set; } = true;

        public RenderingPanel()
        {
            BorderStyle = BorderStyle.None;
        }

        public Vector2 Delta(Point currentPosition, Point previousMousePosition)
        {
             return new Vector2((currentPosition.X - previousMousePosition.X) / (float)Height,
                                (currentPosition.Y - previousMousePosition.Y) / (float)Height);
        }

        public Vector2 WarpMouseCursor(Point currentPosition, Point previousMousePosition)
        {
            // Use height for X coordinate because the camera FOV per pixel is defined by the height.
            var coordinate = currentPosition;
            var delta = Delta(currentPosition, previousMousePosition);

            if (coordinate.X <= 0)
                Cursor.Position = new Point(Cursor.Position.X + Width - 2, Cursor.Position.Y);
            else if (coordinate.X >= Width - 1)
                Cursor.Position = new Point(Cursor.Position.X - Width + 2, Cursor.Position.Y);

            if (coordinate.Y <= 0)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + Height - 2);
            else if (coordinate.Y >= Height - 1)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - Height + 2);

            if (coordinate.X - previousMousePosition.X >= (float)Width / 2 ||
                coordinate.X - previousMousePosition.X <= -(float)Width / 2)
                delta.X = 0;

            if (coordinate.Y - previousMousePosition.Y >= (float)Height / 2 ||
                coordinate.Y - previousMousePosition.Y <= -(float)Height / 2)
                delta.Y = 0;

            return delta;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0x000F;

            if (m.Msg == WM_PAINT && !AllowRendering)
                return;

            base.WndProc(ref m);
        }

        protected virtual Vector4 ClearColor { get; } = new Vector4(0.392f, 0.584f, 0.929f, 1.0f);
    }
}
