using System;
using System.Windows.Forms;
using DarkUI.Forms;
using System.Drawing;

namespace TombLib.Forms
{
    public partial class PopUpWindow : DarkForm
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private readonly Timer _animTimer = new Timer() { Interval = 10 };
        private float _animProgress = 0.0f;
        private Point _initialPosition;

        public PopUpWindow() { }
        public PopUpWindow(Point position)
        {
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

                // Smoothly descend pop-up window from parent control using sine function
                Location = new Point(_initialPosition.X, _initialPosition.Y - shift + (int)(shift * Math.Sin(_animProgress * Math.PI / 2)));

                Opacity = _animProgress;

                if (_animProgress == 1.0f)
                {
                    _animProgress = 0.0f;
                    _animTimer.Stop();
                }
            }
        }

        public void Drag(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
