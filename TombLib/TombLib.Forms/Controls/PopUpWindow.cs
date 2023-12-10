using System;
using System.Windows.Forms;
using DarkUI.Forms;
using System.Drawing;
using DarkUI.Config;

namespace TombLib.Forms
{
    public partial class PopUpWindow : DarkForm
    {
        protected readonly Timer _animTimer = new Timer() { Interval = 10 };
        protected float _animProgress = 0.0f;
        protected Point _initialPosition;

        protected bool _descend = true;

        public PopUpWindow() { }
        public PopUpWindow(Point position, bool descend)
        {
            Opacity = 0.0f;

            _descend = descend;

            // Store initial pos
            _initialPosition = position;

            // Start intro animation
            _animProgress = 0.0f;
            _animTimer.Tick += UpdateTimer;
            _animTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            _animTimer.Tick -= UpdateTimer;
            _animTimer.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            using (var pen = new Pen(Colors.DarkBackground, 2))
                e.Graphics.DrawRectangle(pen, ClientRectangle);
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            if (_animProgress < 0.0f)
                return;

            _animProgress += 0.1f;
            if (_animProgress > 1.0f) _animProgress = 1.0f;

            var shift = Math.Min(Size.Height, 32);
            var offset = (int)(shift * Math.Sin(_animProgress * Math.PI / 2));

            if (_descend)
                offset = -offset + shift;
            else
                offset -= shift;

            // Clamp position
            var bounds = Screen.FromPoint(_initialPosition).Bounds;
            var x = Math.Min(_initialPosition.X + Width, bounds.X + bounds.Width) - Width;
            var y = Math.Min(_initialPosition.Y + Height, bounds.Y + bounds.Height) - Height;
            _initialPosition = new Point(x, y);

            // Smoothly descend pop-up window from parent control using sine function
            Location = new Point(_initialPosition.X, _initialPosition.Y - offset);

            Opacity = _animProgress;

            if (_animProgress == 1.0f)
            {
                _animProgress = 0.0f;
                _animTimer.Stop();
                Focus();
            }
        }
    }
}
