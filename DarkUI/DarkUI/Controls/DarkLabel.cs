using DarkUI.Config;
using DarkUI.Extensions;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkLabel : Label
    {
        #region Field Region

        private BorderStyle _borderStyle;
        private bool _autoUpdateHeight;
        private bool _isGrowing;

        #endregion

        #region Property Region

        [Category("Layout")]
        [Description("Enables automatic height sizing based on the contents of the label.")]
        [DefaultValue(false)]
        public bool AutoUpdateHeight
        {
            get { return _autoUpdateHeight; }
            set
            {
                _autoUpdateHeight = value;

                if (_autoUpdateHeight)
                {
                    AutoSize = false;
                    ResizeLabel();
                }
            }
        }

        public new BorderStyle BorderStyle 
        { 
            get { return _borderStyle; }
            set { _borderStyle = value; Invalidate(); }
        }

        public new bool AutoSize
        {
            get { return base.AutoSize; }
            set
            {
                base.AutoSize = value;

                if (AutoSize)
                    AutoUpdateHeight = false;
            }
        }

        public new Color BackColor
        {
            get { return _backColor.HasValue ? _backColor.Value : base.BackColor; }
            set { base.BackColor = value.Multiply(Colors.FontBrightness); _backColor = value; }
        }
        private Color? _backColor;


        public new Color ForeColor
        {
            get { return _foreColor.HasValue ? _foreColor.Value : base.ForeColor; }
            set { base.ForeColor = value.Multiply(Colors.FontBrightness); _foreColor = value; }
        }
        private Color? _foreColor;


        #endregion

        #region Constructor Region

        public DarkLabel()
        {
            ForeColor = Colors.LightText;
            base.BorderStyle = BorderStyle.None;
        }

        #endregion

        #region Method Region

        private void ResizeLabel()
        {
            if (!_autoUpdateHeight || _isGrowing)
                return;

            try
            {
                _isGrowing = true;
                var sz = new Size(Width, int.MaxValue);
                sz = TextRenderer.MeasureText(Text, Font, sz, TextFormatFlags.WordBreak);
                Height = sz.Height + Padding.Vertical;
            }
            finally
            {
                _isGrowing = false;
            }
        }

        #endregion

        #region Event Handler Region

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            ResizeLabel();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ResizeLabel();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ResizeLabel();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (BorderStyle != BorderStyle.None)
            {
                var rect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
                using (var p = new Pen(Colors.GreySelection))
                    e.Graphics.DrawRectangle(p, rect);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled)
            {
                base.OnPaint(e);
                return;
            }

            using (var b = new SolidBrush(Colors.DisabledText))
                e.Graphics.DrawString(Text, Font, b, ClientRectangle);
        }

        #endregion
    }
}
