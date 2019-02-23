using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[System.ComponentModel.ToolboxItem(false)]
	public class TabStyleDefaultProvider : TabStyleProvider
	{
		public TabStyleDefaultProvider(CustomTabControl tabControl) : base(tabControl)
		{
			// Nothing
		}

		protected override Brush GetTabBackgroundBrush(int index)
		{
			if (_TabControl.SelectedIndex == index)
				return new LinearGradientBrush(this.GetTabRect(index), Color.FromArgb(96, 96, 96), Color.FromArgb(96, 96, 96), LinearGradientMode.Vertical);
			else
				return new LinearGradientBrush(this.GetTabRect(index), Color.FromArgb(48, 48, 48), Color.FromArgb(48, 48, 48), LinearGradientMode.Vertical);
		}

		public override void AddTabBorder(GraphicsPath path, Rectangle tabBounds)
		{
			path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y);
			path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right, tabBounds.Y);
			path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom);
		}

		public override Rectangle GetTabRect(int index)
		{
			if (index < 0)
			{
				return new Rectangle();
			}

			Rectangle tabBounds = base.GetTabRect(index);
			tabBounds.Height += 1;

			EnsureFirstTabIsInView(ref tabBounds, index);
			return tabBounds;
		}
	}
}
