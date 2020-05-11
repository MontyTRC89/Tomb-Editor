using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Forms
{
    public class DarkTranslucentForm : Form
    {
        #region Property Region

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        #endregion

        #region Constructor Region

        public DarkTranslucentForm(Color backColor, double opacity = 0.6)
        {
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(1, 1);
            ShowInTaskbar = false;
            AllowTransparency = true;
            Opacity = opacity;
            BackColor = backColor;
        }

        #endregion

        #region Override region

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x88;
                return cp;
            }
        }

        public new void Show() => Win32.Native.ShowInactiveTopmost(this);

        #endregion
    }
}
