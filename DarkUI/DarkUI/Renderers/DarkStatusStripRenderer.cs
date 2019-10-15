using DarkUI.Config;
using DarkUI.Extensions;
using System.Windows.Forms;

namespace DarkUI.Renderers
{
    public class DarkStatusStripRenderer : ToolStripRenderer
    {
        protected override void Initialize(ToolStrip statusStrip)
        {
            base.Initialize(statusStrip);

            statusStrip.BackColor = Colors.GreyBackground;
            statusStrip.ForeColor = Colors.LightText;
        }

        protected override void InitializeItem(ToolStripItem item)
        {
            base.InitializeItem(item);

            item.BackColor = Colors.GreyBackground;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.TextColor.Multiply(Colors.Brightness);
            base.OnRenderItemText(e);
        }
    }
}
