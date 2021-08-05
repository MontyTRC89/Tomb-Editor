using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace TombEditor.Controls
{
    public class RichTextLabel : RichTextBox
    {
        public RichTextLabel()
        {
            ReadOnly = true;
            BorderStyle = BorderStyle.None;
            TabStop = false;
            DoubleBuffered = true;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserMouse, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            MouseEnter += delegate (object sender, EventArgs e)
            {
                Cursor = Cursors.Default;
            };
        }

        [Category("Appearance")]
        [Browsable(true)]
        public new bool AutoSize { get; set; }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x204) return; // WM_RBUTTONDOWN
            if (m.Msg == 0x205) return; // WM_RBUTTONUP
            base.WndProc(ref m);
        }

        protected override void OnContentsResized(ContentsResizedEventArgs e)
        {
            if (AutoSize)
                Height = e.NewRectangle.Height + 5;
        }
    }
}
