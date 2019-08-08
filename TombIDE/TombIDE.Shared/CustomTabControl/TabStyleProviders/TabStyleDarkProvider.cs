using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[ToolboxItem(false)]
	public class TabStyleDarkProvider : TabStyleProvider
	{
		public TabStyleDarkProvider(CustomTabControl tabControl) : base(tabControl)
		{
			_BorderColor = Color.FromArgb(96, 96, 96);
			_BorderColorHot = Color.FromArgb(96, 96, 96);
			_BorderColorSelected = Color.FromArgb(96, 96, 96);
			_TextColor = Color.FromArgb(153, 153, 153);
			_TextColorSelected = Color.FromArgb(152, 196, 232);
			_TextColorDisabled = Color.FromArgb(96, 96, 96);
			_CloserColor = Color.White;
			_CloserColorActive = Color.FromArgb(152, 196, 232);

			_HotTrack = false;
			_FocusTrack = false;

			_Radius = 10;
			_Overlap = 0;
			_Opacity = 1F;

			_Padding = new Point(6, 3);
		}

		protected override Brush GetTabBackgroundBrush(int index)
		{
			if (_TabControl.SelectedIndex == index)
				return new LinearGradientBrush(GetTabRect(index), Color.FromArgb(60, 63, 65), Color.FromArgb(60, 63, 65), LinearGradientMode.Vertical);
			else
				return new LinearGradientBrush(GetTabRect(index), Color.FromArgb(48, 48, 48), Color.FromArgb(48, 48, 48), LinearGradientMode.Vertical);
		}

		public override void AddTabBorder(GraphicsPath path, Rectangle tabBounds)
		{
			switch (_TabControl.Alignment)
			{
				case TabAlignment.Top:
					path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y);
					path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right, tabBounds.Y);
					path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom);
					break;

				case TabAlignment.Bottom:
					path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom);
					path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X, tabBounds.Bottom);
					path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y);
					break;

				case TabAlignment.Left:
					path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X, tabBounds.Bottom);
					path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y);
					path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right, tabBounds.Y);
					break;

				case TabAlignment.Right:
					path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right, tabBounds.Y);
					path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom);
					path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X, tabBounds.Bottom);
					break;
			}
		}

		public override Rectangle GetTabRect(int index)
		{
			if (index < 0)
				return new Rectangle();

			Rectangle tabBounds = base.GetTabRect(index);

			switch (_TabControl.Alignment)
			{
				case TabAlignment.Top:
					tabBounds.Height += 1;
					break;

				case TabAlignment.Bottom:
					tabBounds = new Rectangle(tabBounds.Location.X, tabBounds.Location.Y - 1, tabBounds.Width, tabBounds.Height + 1);
					break;
			}

			EnsureFirstTabIsInView(ref tabBounds, index);

			return tabBounds;
		}
	}
}
