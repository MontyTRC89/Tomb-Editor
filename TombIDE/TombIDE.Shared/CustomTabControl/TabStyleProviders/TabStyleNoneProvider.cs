using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[ToolboxItem(false)]
	public class TabStyleNoneProvider : TabStyleProvider
	{
		public TabStyleNoneProvider(CustomTabControl tabControl) : base(tabControl)
		{
			// Nothing.
		}

		public override void AddTabBorder(GraphicsPath path, Rectangle tabBounds)
		{
			// Nothing.
		}
	}
}
