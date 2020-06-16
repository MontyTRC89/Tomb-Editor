using DarkUI.Config;
using DarkUI.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkTextBox : TextBox
    {
        #region Constructor Region

        public DarkTextBox()
        {
            BackColor = Colors.LightBackground;
            ForeColor = Colors.LightText;
            Padding = new Padding(2, 2, 2, 2);
            BorderStyle = BorderStyle.FixedSingle;
        }

        #endregion

        #region Property Region

        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get { return base.Padding; }
            set { base.Padding = value; }
        }

        [DefaultValue(BorderStyle.FixedSingle)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        #endregion

        #region Methods Region

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (IsDisposed || Disposing) return;

            if (m.Msg == 0x000F && BorderStyle == BorderStyle.FixedSingle)
            {
                var hdc = Native.GetWindowDC(Handle);
                using (var g = Graphics.FromHdcInternal(hdc))
                {
                    var rect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
                    using (var p = new Pen(Colors.GreySelection))
                        g.DrawRectangle(p, rect);
                }
            }
        }

        #endregion
    }
}
