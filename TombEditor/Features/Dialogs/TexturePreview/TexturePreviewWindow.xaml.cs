using System.Windows;
using System.Windows.Interop;
using TombLib.LevelData;

namespace TombEditor.Features.Dialogs.TexturePreview;

/// <summary>
/// Borderless pop-up that previews a <see cref="LevelTexture"/>. Closes when it
/// loses focus, mirroring the WinForms <c>FormPreviewTexture</c> behaviour.
/// </summary>
public partial class TexturePreviewWindow : Window
{
	public TexturePreviewWindow(LevelTexture texture)
	{
		InitializeComponent();
		MapView.VisibleTexture = texture;
		Deactivated += (_, _) => Close();
	}

	/// <summary>Convenience helper for WinForms callers — sets the WPF owner from an HWND.</summary>
	public void SetOwnerFromHwnd(System.IntPtr hwnd)
	{
		if (hwnd == System.IntPtr.Zero)
			return;

		new WindowInteropHelper(this).Owner = hwnd;
	}
}
