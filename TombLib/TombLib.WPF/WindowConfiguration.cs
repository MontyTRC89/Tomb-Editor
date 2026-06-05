#nullable enable

using System.Windows;
using Size = System.Drawing.Size;

namespace TombLib.WPF;

/// <summary>
/// WPF counterpart of <see cref="TombLib.Utils.ConfigurationBase"/>'s
/// <c>Configuration.ConfigureWindow(Form, ConfigurationBase)</c>.
/// Persists a <see cref="Window"/>'s size and maximized flag into
/// <see cref="ConfigurationBase"/> properties named <c>Window_&lt;key&gt;_Size / _Maximized</c>.
/// Position is intentionally NOT persisted: dialogs always open centered via their
/// <see cref="Window.WindowStartupLocation"/> (typically <c>CenterOwner</c>).
/// </summary>
public static class WindowConfiguration
{
    /// <summary>
    /// Load size/maximized state from <paramref name="config"/> onto <paramref name="window"/>,
    /// then hook <see cref="Window.Closing"/> to save it back. Pass <paramref name="key"/> when the
    /// legacy WinForms form name differs from the WPF window's class name, so existing user configs
    /// keep applying.
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
        var max = config.GetType().GetProperty(prefix + "_Maximized")?.GetValue(config);

        if (size is Size s)
        {
            window.Width = s.Width;
            window.Height = s.Height;

            // Re-center after resizing: ConfigureWindow runs on Loaded, after CenterOwner has already
            // positioned the (differently sized) window, so changing the size would leave it off-center.
            RecenterOnOwner(window);
        }

        if (max is bool m && m)
            window.WindowState = WindowState.Maximized;
    }

    private static void RecenterOnOwner(Window window)
    {
        if (window.WindowStartupLocation != WindowStartupLocation.CenterOwner || window.Owner is null)
            return;

        window.Left = window.Owner.Left + (window.Owner.ActualWidth - window.Width) / 2.0;
        window.Top = window.Owner.Top + (window.Owner.ActualHeight - window.Height) / 2.0;
    }

    private static void SaveWindowProperties(Window window, ConfigurationBase config, string key)
    {
        if (window.WindowState == WindowState.Minimized)
            return;

        var prefix = "Window_" + key;
        config.GetType().GetProperty(prefix + "_Maximized")?.SetValue(config, window.WindowState == WindowState.Maximized);

        if (window.WindowState != WindowState.Maximized)
            config.GetType().GetProperty(prefix + "_Size")?.SetValue(config, new Size((int)window.Width, (int)window.Height));
    }
}
