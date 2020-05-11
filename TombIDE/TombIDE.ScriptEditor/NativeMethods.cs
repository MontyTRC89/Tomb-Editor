﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TombIDE.ScriptEditor
{
	internal enum ScrollDirection
	{
		Left,
		Right
	}

	internal static class NativeMethods
	{
		private const int WM_SCROLL = 276;
		private const int SB_LINELEFT = 0;
		private const int SB_LINERIGHT = 1;
		private const int SB_LEFT = 6;
		private const int SB_RIGHT = 7;

		private const int WM_SETREDRAW = 11;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);

		public static void PerformSingleHorizontalScroll(IntPtr controlHandle, ScrollDirection scrollDirection)
		{
			switch (scrollDirection)
			{
				case ScrollDirection.Left:
					SendMessage(controlHandle, WM_SCROLL, (IntPtr)SB_LINELEFT, IntPtr.Zero);
					break;

				case ScrollDirection.Right:
					SendMessage(controlHandle, WM_SCROLL, (IntPtr)SB_LINERIGHT, IntPtr.Zero);
					break;
			}
		}

		public static void PerformFullHorizontalScroll(IntPtr controlHandle, ScrollDirection scrollDirection)
		{
			switch (scrollDirection)
			{
				case ScrollDirection.Left:
					SendMessage(controlHandle, WM_SCROLL, (IntPtr)SB_LEFT, IntPtr.Zero);
					break;

				case ScrollDirection.Right:
					SendMessage(controlHandle, WM_SCROLL, (IntPtr)SB_RIGHT, IntPtr.Zero);
					break;
			}
		}

		/// <summary>
		/// Suspends painting for the target control. Do NOT forget to call EndControlUpdate!!!
		/// </summary>
		public static void BeginControlUpdate(IntPtr controlHandle)
		{
			Message msgSuspendUpdate = Message.Create(controlHandle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

			NativeWindow window = NativeWindow.FromHandle(controlHandle);
			window.DefWndProc(ref msgSuspendUpdate);
		}

		/// <summary>
		/// Resumes painting for the target control. Intended to be called following a call to BeginControlUpdate()
		/// </summary>
		public static void EndControlUpdate(IntPtr controlHandle)
		{
			Message msgResumeUpdate = Message.Create(controlHandle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);

			NativeWindow window = NativeWindow.FromHandle(controlHandle);
			window.DefWndProc(ref msgResumeUpdate);
		}
	}
}
