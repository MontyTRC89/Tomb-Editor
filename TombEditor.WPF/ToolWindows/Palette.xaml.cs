using System.Collections.Generic;
using System.Windows.Controls;
using TombEditor.Controls;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.WPF.ToolWindows;

/// <summary>
/// Interaction logic for Palette.xaml
/// </summary>
public partial class Palette : UserControl
{
	private readonly Editor _editor;
	private List<ColorC> _lastTexturePalette;

	private PanelPalette lightPalette = new();

	public Palette()
	{
		InitializeComponent();

		_editor = Editor.Instance;
		_editor.EditorEventRaised += EditorEventRaised;

		// Reset palette to default and prepare controls
		lightPalette.LoadPalette(LevelSettings.LoadPalette());
		UpdateControls(false);

		PaletteGridHost.Child = lightPalette;

		Loaded += Palette_Loaded;
	}

	private void Palette_Loaded(object sender, System.Windows.RoutedEventArgs e)
	{
		CommandHandler.AssignCommandsToControls(Editor.Instance, this);
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.LevelChangedEvent)
		{
			_lastTexturePalette = null;
			lightPalette.LoadPalette(((Editor.LevelChangedEvent)obj).Current.Settings.Palette);
		}

		if (obj is Editor.SelectedObjectChangedEvent)
		{
			butEditColor.IsEnabled = ((Editor.SelectedObjectChangedEvent)obj).Current.CanBeColored();
			lightPalette.PickColor();
		}

		if (obj is Editor.ObjectChangedEvent)
		{
			var o = obj as Editor.ObjectChangedEvent;
			if (o.ChangeType == ObjectChangeType.Change && o.Object == _editor.SelectedObject && o.Object is IColorable)
				lightPalette.PickColor();
		}

		if (obj is Editor.ResetPaletteEvent)
		{
			if (!butSampleFromTextures.IsChecked.Value)
			{
				lightPalette.LoadPalette(LevelSettings.LoadPalette());
				_editor.Level.Settings.Palette = lightPalette.Palette;
			}
		}

		if (obj is Editor.SelectedLevelTextureChangedSetEvent)
		{
			var o = (Editor.SelectedLevelTextureChangedSetEvent)obj;
			_lastTexturePalette = new List<ColorC>(o.Texture.Image.Palette);
			UpdateControls();
		}

		if (obj is Editor.ConfigurationChangedEvent)
		{
			var o = (Editor.ConfigurationChangedEvent)obj;
			if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
				CommandHandler.AssignCommandsToControls(_editor, this, true);
			UpdateControls();
		}
	}

	private void UpdateControls(bool reloadPalette = true)
	{
		bool useTexture = _editor.Configuration.Palette_TextureSamplingMode;

		butSampleFromTextures.IsChecked = useTexture;
		butResetToDefaults.IsEnabled = !useTexture;
		lightPalette.Editable = !useTexture;

		if (reloadPalette)
		{
			if (useTexture && _lastTexturePalette != null && _lastTexturePalette.Count > 0)
				lightPalette.LoadPalette(_lastTexturePalette);
			else
				lightPalette.LoadPalette(_editor.Level.Settings.Palette);

			// Also update last colour on editor so next created light will receive updated colour.
			_editor.LastUsedPaletteColourChange(lightPalette.SelectedColor);
		}
	}
}
