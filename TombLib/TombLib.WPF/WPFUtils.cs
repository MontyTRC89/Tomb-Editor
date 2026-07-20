using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TombLib.WPF;

public static class WPFUtils
{
	private static readonly System.Windows.Forms.IWin32Window EmptyWin32Window = new EmptyWindow();

	public static System.Windows.Forms.IWin32Window GetWin32WindowOwner()
	{
		if (System.Windows.Forms.Form.ActiveForm is { } activeForm && CanOwnDialogs(activeForm))
			return activeForm;

		foreach (System.Windows.Forms.Form form in System.Windows.Forms.Application.OpenForms)
		{
			if (CanOwnDialogs(form))
				return form;
		}

		// No WinForms Form can own dialogs (the host app is WPF). Fall back to the active — or
		// failing that the main — WPF window. Without a valid owner, non-modal windows shown via
		// SetOwner() end up ownerless and drop behind the editor when the main window is activated
		// (e.g. clicking the 3D viewport), instead of staying on top.
		if (TryGetActiveWpfWindowHandle(out IntPtr hwnd))
			return new Win32WindowHandle(hwnd);

		return EmptyWin32Window;
	}

	private static bool TryGetActiveWpfWindowHandle(out IntPtr handle)
	{
		handle = IntPtr.Zero;

		if (System.Windows.Application.Current is not { } app)
			return false;

		System.Windows.Window? target = null;
		foreach (System.Windows.Window window in app.Windows)
		{
			if (window.IsActive)
			{
				target = window;
				break;
			}
		}

		target ??= app.MainWindow;
		if (target is null)
			return false;

		handle = new System.Windows.Interop.WindowInteropHelper(target).Handle;
		return handle != IntPtr.Zero;
	}

	public static Color ToWPFColor(this Vector3 color) => Color.FromRgb((byte)(color.X * 255.0f), (byte)(color.Y * 255.0f), (byte)(color.Z * 255.0f));
	public static Color ToWPFColor(this Vector4 color) => Color.FromArgb((byte)(color.W * 255.0f), (byte)(color.X * 255.0f), (byte)(color.Y * 255.0f), (byte)(color.Z * 255.0f));

	public static Brush ToWPFBrush(this Vector3 color) => new Vector4(color, 1.0f).ToWPFBrush();
	public static Vector3 ToFloat3Color(this Color color) => new Vector3(color.R, color.G, color.B) / 255.0f;
	public static Vector4 ToFloat4Color(this Color color) => new Vector4(color.R, color.G, color.B, color.A) / 255.0f;

	public static Brush ToWPFBrush(this Vector4 color, float? alpha = null)
	{
		var brushColor = Color.FromArgb(
			(byte)Math.Max(0, Math.Min(255, Math.Round((alpha.HasValue ? Math.Clamp(alpha.Value, 0.0, 1.0) : color.W) * 255.0f))),
			(byte)Math.Max(0, Math.Min(255, Math.Round(color.X * 255.0f))),
			(byte)Math.Max(0, Math.Min(255, Math.Round(color.Y * 255.0f))),
			(byte)Math.Max(0, Math.Min(255, Math.Round(color.Z * 255.0f))));

		return BrushHelpers.CreateFrozenBrush(brushColor);
	}

	public static Brush ToWPFBrush(this Vector3 color, float alpha)
	{
		return ToWPFBrush(new Vector4(color.X, color.Y, color.Z, alpha));
	}

	public static float GetBrightness(this Brush b)
	{
		if (b is SolidColorBrush scb)
		{
			Color c = scb.Color;
			return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B).GetBrightness();
		}
		else
		{
			return 1.0f;
		}
	}

	public static IEnumerable<Control> AllSubControls(DependencyObject? depObj)
	{
		if (depObj is null)
			yield break;

		foreach (var child in LogicalTreeHelper.GetChildren(depObj))
		{
			if (child is DependencyObject dpo)
			{
				if (child is Control t)
					yield return t;

				foreach (Control childOfChild in AllSubControls(dpo))
					yield return childOfChild;
			}
		}
	}

	private sealed class EmptyWindow : System.Windows.Forms.IWin32Window
	{
		public IntPtr Handle => IntPtr.Zero;
	}

	private sealed class Win32WindowHandle : System.Windows.Forms.IWin32Window
	{
		public IntPtr Handle { get; }

		public Win32WindowHandle(IntPtr handle) => Handle = handle;
	}

	private static bool CanOwnDialogs(System.Windows.Forms.Form form)
		=> !form.IsDisposed && form.Handle != IntPtr.Zero;
}
