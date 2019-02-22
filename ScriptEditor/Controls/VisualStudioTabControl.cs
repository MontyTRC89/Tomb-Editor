using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScriptEditor
{
	public class VisualStudioTabControl : TabControl
	{
		private Color BackgroundColor = Color.FromArgb(45, 45, 48);
		private Color SelectedTabColor = Color.FromArgb(0, 122, 204);
		private Color UnselectedTabColor = Color.FromArgb(63, 63, 70);
		private Color HoverTabColor = Color.FromArgb(28, 151, 234);
		private Color HoverTabButtonColor = Color.FromArgb(82, 176, 239);
		private Color HoverUnselectedTabButtonColor = Color.FromArgb(85, 85, 85);
		private Color SelectedTabButtonColor = Color.FromArgb(28, 151, 234);
		private Color UnselectedBorderTabLineColor = Color.FromArgb(63, 63, 70);
		private Color BorderTabLineColor = Color.FromArgb(0, 122, 204);
		private Color UnderBorderTabLineColor = Color.FromArgb(67, 67, 70);
		private Color TextColor = Color.FromArgb(255, 255, 255);
		private Color UpDownBackColor = Color.FromArgb(63, 63, 70);
		private Color UpDownTextColor = Color.FromArgb(109, 109, 112);

		private StringFormat CenterSF;
		private TabPage predraggedTab;
		private int hoveringTabIndex;

		private SubClass scUpDown = null;
		private bool bUpDown = false;
		private bool hasFocus = false;

		public VisualStudioTabControl()
		{
			base.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
			this.DoubleBuffered = true;
			this.CenterSF = new StringFormat
			{
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Center
			};

			this.Padding = new Point(14, 4);
			this.AllowDrop = true;
			this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
		}

		protected override void CreateHandle()
		{
			base.CreateHandle();
			base.Alignment = TabAlignment.Top;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			var mouseRect = new Rectangle(e.X, e.Y, 1, 1);
			var hoveringTabs = Enumerable.Range(0, this.TabCount).Where(i => this.GetTabRect(i).IntersectsWith(mouseRect));

			if (hoveringTabs.Any())
			{
				var tabIndex = hoveringTabs.First();
				var tabBase = new Rectangle(new Point(base.GetTabRect(tabIndex).Location.X + 2, base.GetTabRect(tabIndex).Location.Y), new Size(base.GetTabRect(tabIndex).Width, base.GetTabRect(tabIndex).Height));
				var tabExitRectangle = new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15);

				if (tabExitRectangle.Contains(this.PointToClient(Cursor.Position)))
				{
					try { this.TabPages.Remove(this.TabPages[tabIndex]); } catch { }

					return;
				}
			}

			this.predraggedTab = this.getPointedTab();
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			this.predraggedTab = null;
			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.SelectedIndex == -1)
			{
				base.OnMouseMove(e);
				return;
			}

			// check whether they are hovering over a tab button
			var tabIndex = this.SelectedIndex;
			var tabBase = new Rectangle(new Point(base.GetTabRect(tabIndex).Location.X + 2, base.GetTabRect(tabIndex).Location.Y), new Size(base.GetTabRect(tabIndex).Width, base.GetTabRect(tabIndex).Height));

			var mouseRect = new Rectangle(e.X, e.Y, 1, 1);
			var hoveringTabs = Enumerable.Range(0, this.TabCount).Where(i => this.GetTabRect(i).IntersectsWith(mouseRect));

			if (hoveringTabs.Any())
				hoveringTabIndex = hoveringTabs.First();

			if (e.Button == MouseButtons.Left && this.predraggedTab != null)
				base.DoDragDrop(this.predraggedTab, DragDropEffects.Move);

			if (e.Y < 25) // purely for performance reasons, only necessary for hovering button states
				this.Invalidate();

			base.OnMouseMove(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			if (hoveringTabIndex != -1)
			{
				hoveringTabIndex = -1;
				this.Invalidate();
			}

			base.OnLeave(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (hoveringTabIndex != -1)
			{
				hoveringTabIndex = -1;
				this.Invalidate();
			}

			base.OnMouseLeave(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;

			g.SmoothingMode = SmoothingMode.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;

			g.Clear(this.BackgroundColor);

			g.DrawLine(new Pen(new SolidBrush(this.FindForm() == Form.ActiveForm ? this.BorderTabLineColor : this.UnselectedBorderTabLineColor), 2), new Point(0, 22), new Point(base.Width, 22));
			g.FillRectangle(new SolidBrush(this.UnderBorderTabLineColor), 0, 23, base.Width, 1);

			// ugly way to check whether the parent form has focus or not
			if (!hasFocus && this.FindForm() == Form.ActiveForm)
			{
				this.Invalidate(new Rectangle(0, 21, base.Width, 24));
				hasFocus = true;
			}
			else if (hasFocus && this.FindForm() != Form.ActiveForm)
			{
				this.Invalidate(new Rectangle(0, 21, base.Width, 24));
				hasFocus = false;
			}

			for (var i = 0; i < this.TabCount; i++)
			{
				var tabBase = new Rectangle(new Point(base.GetTabRect(i).Location.X + 2, base.GetTabRect(i).Location.Y), new Size(base.GetTabRect(i).Width, base.GetTabRect(i).Height));
				var tabSize = new Rectangle(tabBase.Location, new Size(tabBase.Width, tabBase.Height - 4));

				// draw tab highlights
				if (this.FindForm() != Form.ActiveForm && base.SelectedIndex == i) // unselected selected tab
					g.FillRectangle(new SolidBrush(this.UnselectedTabColor), tabSize);
				else if (base.SelectedIndex == i) // selected tab
					g.FillRectangle(new SolidBrush(this.SelectedTabColor), tabSize);
				else if (hoveringTabIndex == i) // hovering tab
					g.FillRectangle(new SolidBrush(this.HoverTabColor), tabSize);
				else // unselected tab
					g.FillRectangle(new SolidBrush(this.BackgroundColor), tabSize);

				// if current selected tab
				if (base.SelectedIndex == i)
				{
					// hovering over selected tab button
					if (new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15).Contains(this.PointToClient(Cursor.Position)))
						g.FillRectangle(new SolidBrush(this.FindForm() == Form.ActiveForm ? this.SelectedTabButtonColor : this.HoverUnselectedTabButtonColor),
							new RectangleF((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15));

					g.TextContrast = 0;
					g.DrawString("×", new Font(this.Font.FontFamily, 15f), new SolidBrush(this.TextColor),
						new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 5), tabBase.Location.Y - 3, tabBase.Width, tabBase.Height), this.CenterSF);
				}
				else
				{
					// if hovering over a tab
					if (hoveringTabIndex == i)
					{
						// hovering over hovered tab button
						if (new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15).Contains(this.PointToClient(Cursor.Position)))
							g.FillRectangle(new SolidBrush(this.HoverTabButtonColor),
								new RectangleF((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15));

						g.TextContrast = 0;
						g.DrawString("×", new Font(this.Font.FontFamily, 15f), new SolidBrush(this.TextColor),
							new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 5), tabBase.Location.Y - 3, tabBase.Width, tabBase.Height), this.CenterSF);
					}
				}

				g.TextContrast = 12;
				g.DrawString(base.TabPages[i].Text, new Font(this.Font.FontFamily, this.Font.Size), new SolidBrush(this.TextColor),
					new Rectangle(tabBase.Location.X + 3, tabBase.Location.Y - 1, tabBase.Width, tabBase.Height + 1), this.CenterSF);
			}

			if (this.SelectedIndex != -1)
				base.SelectedTab.BorderStyle = BorderStyle.None;
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			var draggedTab = (TabPage)drgevent.Data.GetData(typeof(TabPage));
			var pointedTab = this.getPointedTab();

			if (draggedTab == this.predraggedTab && pointedTab != null)
			{
				drgevent.Effect = DragDropEffects.Move;

				if (pointedTab != draggedTab)
					this.swapTabPages(draggedTab, pointedTab);
			}

			base.OnDragOver(drgevent);
		}

		private TabPage getPointedTab()
		{
			checked
			{
				for (var i = 0; i <= base.TabPages.Count - 1; i++)
				{
					if (base.GetTabRect(i).Contains(base.PointToClient(Cursor.Position)))
						return base.TabPages[i];
				}
				return null;
			}
		}

		private void swapTabPages(TabPage src, TabPage dst)
		{
			var srci = base.TabPages.IndexOf(src);
			var dsti = base.TabPages.IndexOf(dst);
			base.TabPages[dsti] = src;
			base.TabPages[srci] = dst;

			base.SelectedIndex = (base.SelectedIndex == srci) ? dsti : srci;

			this.Refresh();
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			FindUpDown();
		}

		protected override void OnControlAdded(ControlEventArgs e)
		{
			FindUpDown();
			UpdateUpDown();

			base.OnControlAdded(e);
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			FindUpDown();
			UpdateUpDown();

			base.OnControlRemoved(e);
		}

		private void FindUpDown()
		{
			var bFound = false;
			var pWnd = Win32.GetWindow(this.Handle, Win32.GW_CHILD);

			while (pWnd != IntPtr.Zero)
			{
				var className = new char[33];
				var length = Win32.GetClassName(pWnd, className, 32);
				var s = new string(className, 0, length);

				if (s == "msctls_updown32")
				{
					bFound = true;

					if (!bUpDown)
					{
						this.scUpDown = new SubClass(pWnd, true);
						this.scUpDown.SubClassedWndProc += new SubClass.SubClassWndProcEventHandler(scUpDown_SubClassedWndProc);

						bUpDown = true;
					}
					break;
				}

				pWnd = Win32.GetWindow(pWnd, Win32.GW_HWNDNEXT);
			}

			if ((!bFound) && (bUpDown))
				bUpDown = false;
		}

		private void UpdateUpDown()
		{
			if (bUpDown)
			{
				if (Win32.IsWindowVisible(scUpDown.Handle))
				{
					var rect = new Rectangle();

					Win32.GetClientRect(scUpDown.Handle, ref rect);
					Win32.InvalidateRect(scUpDown.Handle, ref rect, true);
				}
			}
		}

		private int scUpDown_SubClassedWndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case Win32.WM_PAINT:
				{
					var hDC = Win32.GetWindowDC(scUpDown.Handle);
					var g = Graphics.FromHdc(hDC);

					DrawIcons(g);

					g.Dispose();
					Win32.ReleaseDC(scUpDown.Handle, hDC);
					m.Result = IntPtr.Zero;

					var rect = new Rectangle();

					Win32.GetClientRect(scUpDown.Handle, ref rect);
					Win32.ValidateRect(scUpDown.Handle, ref rect);
				}
				return 1;
			}

			return 0;
		}

		internal void DrawIcons(Graphics g)
		{
			var TabControlArea = this.ClientRectangle;
			var r0 = new Rectangle();
			Win32.GetClientRect(scUpDown.Handle, ref r0);

			Brush br = new SolidBrush(UpDownBackColor);
			g.FillRectangle(br, r0);
			br.Dispose();

			g.DrawString("◀", new Font(this.Font.FontFamily, 12f),
				new SolidBrush(UpDownTextColor), r0);

			g.DrawString("▶", new Font(this.Font.FontFamily, 12f),
				new SolidBrush(UpDownTextColor),
				new Rectangle(r0.X + 20, r0.Y, r0.Width, r0.Height));
		}
	}

	internal static class Win32
	{
		public const int GW_HWNDFIRST = 0;
		public const int GW_HWNDLAST = 1;
		public const int GW_HWNDNEXT = 2;
		public const int GW_HWNDPREV = 3;
		public const int GW_OWNER = 4;
		public const int GW_CHILD = 5;

		public const int WM_NCCALCSIZE = 0x83;
		public const int WM_WINDOWPOSCHANGING = 0x46;
		public const int WM_PAINT = 0xF;
		public const int WM_CREATE = 0x1;
		public const int WM_NCCREATE = 0x81;
		public const int WM_NCPAINT = 0x85;
		public const int WM_PRINT = 0x317;
		public const int WM_DESTROY = 0x2;
		public const int WM_SHOWWINDOW = 0x18;
		public const int WM_SHARED_MENU = 0x1E2;
		public const int HC_ACTION = 0;
		public const int WH_CALLWNDPROC = 4;
		public const int GWL_WNDPROC = -4;

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowDC(IntPtr handle);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr ReleaseDC(IntPtr handle, IntPtr hDC);

		[DllImport("Gdi32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hwnd, char[] className, int maxCount);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindow(IntPtr hwnd, int uCmd);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern bool IsWindowVisible(IntPtr hwnd);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern int GetClientRect(IntPtr hwnd, ref RECT lpRect);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern int GetClientRect(IntPtr hwnd, [In, Out] ref Rectangle rect);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool UpdateWindow(IntPtr hwnd);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool InvalidateRect(IntPtr hwnd, ref Rectangle rect, bool bErase);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool ValidateRect(IntPtr hwnd, ref Rectangle rect);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern bool GetWindowRect(IntPtr hWnd, [In, Out] ref Rectangle rect);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPOS
		{
			public IntPtr hwnd;
			public IntPtr hwndAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public uint flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NCCALCSIZE_PARAMS
		{
			public RECT rgc;
			public WINDOWPOS wndpos;
		}
	}

	internal class SubClass : NativeWindow
	{
		public delegate int SubClassWndProcEventHandler(ref System.Windows.Forms.Message m);

		public event SubClassWndProcEventHandler SubClassedWndProc;

		private bool IsSubClassed = false;

		public SubClass(IntPtr Handle, bool _SubClass)
		{
			base.AssignHandle(Handle);
			this.IsSubClassed = _SubClass;
		}

		public bool SubClassed
		{
			get => this.IsSubClassed;
			set => this.IsSubClassed = value;
		}

		protected override void WndProc(ref Message m)
		{
			if (this.IsSubClassed)
			{
				if (OnSubClassedWndProc(ref m) != 0)
					return;
			}
			base.WndProc(ref m);
		}

		public void CallDefaultWndProc(ref Message m)
		{
			base.WndProc(ref m);
		}

		public int HiWord(int Number)
		{
			return ((Number >> 16) & 0xffff);
		}

		public int LoWord(int Number)
		{
			return (Number & 0xffff);
		}

		public int MakeLong(int LoWord, int HiWord)
		{
			return (HiWord << 16) | (LoWord & 0xffff);
		}

		public IntPtr MakeLParam(int LoWord, int HiWord)
		{
			return (IntPtr)((HiWord << 16) | (LoWord & 0xffff));
		}

		private int OnSubClassedWndProc(ref Message m)
		{
			if (SubClassedWndProc != null)
			{
				return this.SubClassedWndProc(ref m);
			}

			return 0;
		}
	}
}
