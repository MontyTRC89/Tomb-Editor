using System;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Forms;

namespace TombLib.Forms
{
    public partial class PopUpInfo : DarkForm
    {
        public enum PopupPosition
        {
            Center,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public enum PopupType
        {
            None,
            Info,
            Warning,
            Error
        }

        private const float _shiftCoeff = 0.25f; // Intro animation push coefficient

        private readonly Color _warningColor = Color.FromArgb(255, 192, 128);
        private readonly Color _errorColor = Color.FromArgb(255, 128, 128);
        private readonly Color _infoColor = Color.FromArgb(192, 192, 255);

        private readonly Timer _animTimer = new Timer() { Interval = 10 };
        private float _animProgress = 0.0f;
        private float _animTimeout = 1000.0f;

        private PopupPosition _position;
        private int _padding;
        private float _finalOpacity = 0.85f;
        private Control _parent;

        public PopUpInfo()
        {
            InitializeComponent();
            _animTimer.Tick += UpdateTimer;
        }

        // Common message helpers
        public void ShowInfo(Control parent, string message, string title = "Information")
        {
            Show(parent, PopupPosition.BottomRight, message, title, PopupType.Info);
        }
        public void ShowWarning(Control parent, string message, string title = "Warning")
        {
            Show(parent, PopupPosition.BottomRight, message, title, PopupType.Warning);
        }
        public void ShowError(Control parent, string message, string title = "Error")
        {
            Show(parent, PopupPosition.BottomRight, message, title, PopupType.Error);
        }
        public void ShowSimple(Control parent, string message)
        {
            Show(parent, PopupPosition.BottomRight, message);
        }

        public void Show(string message, PopupType type)
        {
            switch (type)
            {
                default:
                case PopupType.Info:
                    ShowInfo(this, message);
                    break;
                case PopupType.Warning:
                    ShowWarning(this, message);
                    break;
                case PopupType.Error:
                    ShowError(this, message);
                    break;
            }
        }

        public void Show(Control parent, PopupPosition startPos, string message, string title = "", PopupType type = PopupType.Info, int timeout = 0, int padding = 10)
        {
            // No message means kill current pop-up
            if(message == "")
            {
                Hide();
                return;
            }

            // Setup
            _parent = parent;
            _position = startPos;
            _padding = padding;

            // Reset sizes
            Size = MinimumSize;
            MaximumSize = new Size((int)(parent.Size.Width * 0.7f), parent.Size.Height / 4);

            // Hide header if not specified
            bool titleIsVisible = (type == PopupType.None || title != string.Empty);
            if (titleIsVisible)
            {
                panelTitle.Visible = true;
                lblTitle.Text = title;

                // Setup header type
                switch (type)
                {
                    default:
                    case PopupType.Info:
                        lblTitle.ForeColor = _infoColor;
                        break;
                    case PopupType.Warning:
                        lblTitle.ForeColor = _warningColor;
                        break;
                    case PopupType.Error:
                        lblTitle.ForeColor = _errorColor;
                        break;
                }
            }
            else
                panelTitle.Visible = false;

            // Measure text dimensions
            SizeF size = TextRenderer.MeasureText(message,
                                                  lblMessage.Font,
                                                  new Size(MaximumSize.Width - panelMessage.Padding.Horizontal, 
                                                           MaximumSize.Height - panelMessage.Padding.Vertical - (titleIsVisible ? panelTitle.Height : 0)),
                                                  TextFormatFlags.WordBreak);
            lblMessage.Text = message;
            Width = (int)Math.Ceiling(size.Width) + panelMessage.Padding.Horizontal + 1;
            Height = (int)Math.Ceiling(size.Height) + (titleIsVisible ? panelTitle.Height : 0) + panelMessage.Padding.Vertical + 1;

            // Setup message timeout
            if(timeout > 0)
                _animTimeout = timeout * 10.0f;
            else
            {
                // Default timeout is based on average reading speed.
                int wordCount = 0, index = 0;
                while (index < message.Length)
                {
                    while (index < message.Length && !char.IsWhiteSpace(message[index]))
                        index++;
                    wordCount++;
                    while (index < message.Length && char.IsWhiteSpace(message[index]))
                        index++;
                }
                _animTimeout = wordCount * 2.5f;
            }

            // Start intro animation
            _animProgress = 0.0f;
            _animTimer.Start();

            // Clear opacity to prevent flicker
            Opacity = 0.0f;

            if(!Visible)
                Show(parent);
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            _animProgress += 0.1f;

            if (_animProgress <= 1.0f)
            {
                // Smoothly descend pop-up window from parent control using sine function
                var callbackControlLocation = _parent.PointToScreen(Point.Empty);
                var shiftDistance = Size.Height * _shiftCoeff;
                int currentShift = (int)(shiftDistance - (shiftDistance * Math.Sin(_animProgress * Math.PI / 2)));

                switch (_position)
                {
                    default:
                    case PopupPosition.BottomLeft:
                        Location = new Point(callbackControlLocation.X + _padding, callbackControlLocation.Y + _parent.Size.Height - Size.Height - _padding - currentShift);
                        break;
                    case PopupPosition.BottomRight:
                        Location = new Point(callbackControlLocation.X + _parent.Size.Width - Size.Width - _padding, callbackControlLocation.Y + _parent.Size.Height - Size.Height - _padding - currentShift);
                        break;
                    case PopupPosition.TopLeft:
                        Location = new Point(callbackControlLocation.X + _padding, callbackControlLocation.Y + _padding + currentShift);
                        break;
                    case PopupPosition.TopRight:
                        Location = new Point(callbackControlLocation.X + _parent.Size.Width - Size.Width - _padding, callbackControlLocation.Y + _padding + currentShift);
                        break;
                    case PopupPosition.Center:
                        Location = new Point(callbackControlLocation.X + callbackControlLocation.X / 2, callbackControlLocation.Y + callbackControlLocation.Y / 2 - (int)(Size.Height / 4 * Math.Sin(_animProgress * Math.PI / 2)));
                        break;
                }
                Opacity = _animProgress * _finalOpacity;

                if (_animProgress == 1.0f)
                {
                    _animProgress = 0.0f;
                    _animTimer.Stop();
                }
            }
            else
            {
                var outroTime = _animTimeout - _animProgress;

                if (outroTime < 1.0f)
                {
                    if (outroTime < 0.0f)
                        outroTime = 0.0f;
                    Opacity = outroTime * _finalOpacity;
                    if (outroTime == 0.0f)
                    {
                        _animProgress = 0.0f;
                        _animTimer.Stop();
                        Hide();
                    }
                }
                else if (Opacity < 1.0f *_finalOpacity)
                    Opacity = 1.0f * _finalOpacity;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000 | 0x20; // Set the form click-through
                return cp;
            }
        }
    }
}
