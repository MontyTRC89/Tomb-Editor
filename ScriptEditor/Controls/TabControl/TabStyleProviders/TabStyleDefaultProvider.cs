using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[System.ComponentModel.ToolboxItem(false)]
	public class TabStyleDefaultProvider : TabStyleProvider
	{
		public TabStyleDefaultProvider(CustomTabControl tabControl) : base(tabControl)
		{
			_TextColor = Color.White;
			_TextColorSelected = Color.White;
			_CloserColor = Color.White;
			_CloserColorActive = Color.FromArgb(128, 128, 128);
			_FocusColor = Color.FromArgb(128, 128, 128);
			_FocusTrack = true;

			_Radius = 2;
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
			{
				return new Rectangle();
			}

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
