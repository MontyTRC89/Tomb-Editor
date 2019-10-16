using DarkUI.Config;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkPanel : Panel
    {
        public DarkPanel()
        {
            SetStyle(ControlStyles.UserPaint    | 
                     ControlStyles.ResizeRedraw | 
                     ControlStyles.DoubleBuffer | 
                     ControlStyles.AllPaintingInWmPaint, true);

            base.BorderStyle = BorderStyle.None;
        }

        public new BorderStyle BorderStyle { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {  }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var b = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(b, ClientRectangle);

            if (BorderStyle == BorderStyle.FixedSingle)
            {
                using (var p = new Pen(Colors.GreySelection, 1))
                    e.Graphics.DrawRectangle(p, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }
    }
}
