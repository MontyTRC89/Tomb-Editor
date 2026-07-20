#nullable enable

using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using TombLib.Controls;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services.Abstract;
using IWinFormsWindow = System.Windows.Forms.IWin32Window;
using WinFormsDialogResult = System.Windows.Forms.DialogResult;

namespace TombLib.Forms.Services;

/// <summary>
/// <see cref="IColorPickerService"/> implementation backed by the WinForms <see cref="RealtimeColorDialog"/>.
/// </summary>
public class ColorPickerService : IColorPickerService
{
	private readonly Func<ColorScheme?>? _colorSchemeProvider;

	/// <param name="colorSchemeProvider">
	/// Optional provider for the color scheme whose custom color slots the dialog reads and persists.
	/// </param>
	public ColorPickerService(Func<ColorScheme?>? colorSchemeProvider = null)
		=> _colorSchemeProvider = colorSchemeProvider;

	public Vector3? PickColor(Vector3 initialColor, Action<Vector3>? onColorChanged = null)
	{
		using var dialog = new RealtimeColorDialog(
			c => onColorChanged?.Invoke(c.ToFloat3Color()),
			_colorSchemeProvider?.Invoke());

		dialog.Color = initialColor.ToWinFormsColor();
		dialog.FullOpen = true;

		WinFormsDialogResult result = GetActiveWindowOwner() is { } owner
			? dialog.ShowDialog(owner)
			: dialog.ShowDialog();

		return result == WinFormsDialogResult.OK
			? dialog.Color.ToFloat3Color()
			: null;
	}

	private static IWinFormsWindow? GetActiveWindowOwner()
	{
		Window? window = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
		return window?.GetWin32Window();
	}
}
