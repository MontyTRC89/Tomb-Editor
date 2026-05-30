#nullable enable
using System;
using System.Windows;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace TombLib.WPF;

/// <summary>
/// WPF counterpart of <see cref="TombLib.Utils.ConfigurationBase"/>'s
/// <c>Configuration.ConfigureWindow(Form, ConfigurationBase)</c>.
/// Persists a <see cref="Window"/>'s position, size and maximized flag into
/// <see cref="ConfigurationBase"/> properties named
/// <c>Window_&lt;key&gt;_Size / _Position / _Maximized</c>.
/// </summary>
public static class WindowConfiguration
{
	/// <summary>
	/// Load layout from <paramref name="config"/> onto <paramref name="window"/>,
	/// then hook <see cref="Window.Closing"/> to save it back. Pass
	/// <paramref name="key"/> when the legacy WinForms form name differs from
	/// the WPF window's class name, so existing user configs keep applying.
	/// </summary>
	public static void ConfigureWindow(Window window, ConfigurationBase config, string? key = null)
	{
		if (window is null || config is null)
			return;

		var effectiveKey = key ?? window.GetType().Name;
		LoadWindowProperties(window, config, effectiveKey);
		window.Closing += (_, _) => SaveWindowProperties(window, config, effectiveKey);
	}

	private static void LoadWindowProperties(Window window, ConfigurationBase config, string key)
	{
		var prefix = "Window_" + key;
		var size = config.GetType().GetProperty(prefix + "_Size")?.GetValue(config);
		var pos  = config.GetType().GetProperty(prefix + "_Position")?.GetValue(config);
		var max  = config.GetType().GetProperty(prefix + "_Maximized")?.GetValue(config);

		if (size is Size s)
		{
			window.Width  = s.Width;
			window.Height = s.Height;
		}

		if (pos is Point p && (p.X != -1 || p.Y != -1))
		{
			window.WindowStartupLocation = WindowStartupLocation.Manual;
			window.Left = p.X;
			window.Top  = p.Y;
			ClampWindowLocation(window);
		}

		if (max is bool m && m)
			window.WindowState = WindowState.Maximized;
	}

	private static void SaveWindowProperties(Window window, ConfigurationBase config, string key)
	{
		if (window.WindowState == WindowState.Minimized)
			return;

		var prefix = "Window_" + key;
		config.GetType().GetProperty(prefix + "_Maximized")?.SetValue(config, window.WindowState == WindowState.Maximized);

		if (window.WindowState != WindowState.Maximized)
		{
			config.GetType().GetProperty(prefix + "_Size")?.SetValue(config, new Size((int)window.Width, (int)window.Height));
			config.GetType().GetProperty(prefix + "_Position")?.SetValue(config, new Point((int)window.Left, (int)window.Top));
		}
	}

	// Mirror legacy single-display clamp: skip clamp on multi-monitor setups so
	// users with windows positioned on a secondary screen don't get yanked back.
	private static void ClampWindowLocation(Window window)
	{
		if (System.Windows.Forms.Screen.AllScreens.Length != 1)
			return;

		var maxX = Math.Max(0, SystemParameters.PrimaryScreenWidth  - window.Width);
		var maxY = Math.Max(0, SystemParameters.PrimaryScreenHeight - window.Height);
		window.Left = Math.Max(0, Math.Min(window.Left, maxX));
		window.Top  = Math.Max(0, Math.Min(window.Top,  maxY));
	}
}
