namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Exposes the name of the currently selected color scheme on an editor configuration.
/// </summary>
public interface IColorSchemeConfig
{
	string SelectedColorSchemeName { get; set; }
}
