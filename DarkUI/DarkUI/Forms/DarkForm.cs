using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Forms
{
    public class DarkForm : Form
    {
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
