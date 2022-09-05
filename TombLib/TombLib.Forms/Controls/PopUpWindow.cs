using System;
using System.Windows.Forms;
using DarkUI.Forms;
using System.Drawing;

namespace TombLib.Forms
{
    public partial class PopUpWindow : DarkForm
    {
        private readonly Timer _animTimer = new Timer() { Interval = 10 };
        private float _animProgress = 0.0f;
        private Point _initialPosition;

        public PopUpWindow() { }
        public PopUpWindow(Point position)
        {
            Opacity = 0.0f;

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

        private void UpdateTimer(object sender, EventArgs e)
        {
            if (_animProgress >= 0.0f) // Intro animation
            {
                _animProgress += 0.1f;
                if (_animProgress > 1.0f) _animProgress = 1.0f;

                var shift = Math.Min(Size.Height / 4, 32);

                // Clamp position
                var bounds = Screen.FromControl(this).Bounds;
                var x = Math.Min(_initialPosition.X + Width, bounds.Width) - Width;
                var y = Math.Min(_initialPosition.Y + Height, bounds.Height) - Height;
                _initialPosition = new Point(x, y);

                // Smoothly descend pop-up window from parent control using sine function
                Location = new Point(_initialPosition.X, _initialPosition.Y - shift + (int)(shift * Math.Sin(_animProgress * Math.PI / 2)));

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
}
