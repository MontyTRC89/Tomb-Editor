using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using DarkUI.Config;

namespace TombEditor.Controls
{
    class TabbedContainer : TabControl
    {
        public TabbedContainer()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);
        }

        public void HideTabs()
        {
            SetStyle(ControlStyles.UserPaint, true);

            Appearance = TabAppearance.FlatButtons;
            ItemSize = new Size(0, 1);
            SizeMode = TabSizeMode.Fixed;

            foreach (TabPage tab in TabPages)
                tab.Text = null;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var b = new SolidBrush(Colors.GreyBackground))
                e.Graphics.FillRectangle(b, ClientRectangle);
        }
    }
}
