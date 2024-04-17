﻿using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DarkUI.Forms
{
    public class DarkForm : Form
    {
        #region Import Region

        private uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, uint attribute, ref int pvAttribute, uint cbAttribute);

        #endregion

        #region Field Region

        private bool _flatBorder;

        #endregion

        #region Property Region

        [Category("Appearance")]
        [Description("Determines whether a single pixel border should be rendered around the form.")]
        [DefaultValue(false)]
        public bool FlatBorder
        {
            get { return _flatBorder; }
            set
            {
                _flatBorder = value;
                Invalidate();
            }
        }

        #endregion

        #region Constructor Region

        public DarkForm()
        {
            BackColor = Colors.GreyBackground;
            Font = Consts.DefaultFont;
        }

        public sealed override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        #endregion

        #region Methods Region

        public new void Show(IWin32Window parent)
        {
            // A hack to well-known problem of non-modal forms being non-centered
            // https://stackoverflow.com/questions/8566582/how-to-centerparent-a-non-modal-form

            if (StartPosition == FormStartPosition.CenterParent && !Modal && parent != null && parent is DarkForm)
            {
                var form = (DarkForm)parent;
                StartPosition = FormStartPosition.Manual;
                Location = new Point(form.Location.X + (form.Width - Width) / 2, form.Location.Y + (form.Height - Height) / 2);
            }

            base.Show(parent);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Set window header according to a system's dark mode setting (only since Win10 build 1903)

            if (Environment.OSVersion.Version.Major < 6)
                return;

            try
            {
                int dark = 1;
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(uint));
            }
            catch (DllNotFoundException) { }
        }

        protected override void WndProc(ref Message m)
        {
            // Another hack which forces window to always accept first mouse click
            // as an event, without requiring one extra click for activation.
            // https://stackoverflow.com/questions/13836363/why-two-time-click-is-required-to-click-toolstripmenuitem

            const int WM_PARENTNOTIFY = 0x0210;
            const int WM_LBUTTONDOWN  = 0x0201;

            if (m.Msg == WM_PARENTNOTIFY && ((int)m.WParam & 0xFFFF) == WM_LBUTTONDOWN && ActiveForm != this && !Focused)
                Focus();

            base.WndProc(ref m);
        }

        #endregion

        #region Paint Region

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (!_flatBorder)
                return;

            var g = e.Graphics;

            using (var p = new Pen(Colors.DarkBorder))
            {
                var modRect = new Rectangle(ClientRectangle.Location, new Size(ClientRectangle.Width - 1, ClientRectangle.Height - 1));
                g.DrawRectangle(p, modRect);
            }
        }

        #endregion
    }
}
