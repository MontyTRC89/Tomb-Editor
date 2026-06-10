using System.Numerics;
using TombLib.Controls;

namespace TombEditor.Features.Dialogs.WadPreview;

/// <summary>
/// Read-only WinForms 3D item-preview panel hosted by <see cref="WadPreviewWindow"/>;
/// reads all rendering settings from the editor configuration.
/// </summary>
public sealed class PanelRenderingItemPreview : PanelItemPreview
{
	public Editor Editor { get; set; }

	protected override Vector4 ClearColor => Editor.Configuration.UI_ColorScheme.Color3DBackground;
	public override float FieldOfView => Editor.Configuration.RenderingItem_FieldOfView;
	public override float NavigationSpeedMouseWheelZoom => Editor.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
	public override float NavigationSpeedMouseZoom => Editor.Configuration.RenderingItem_NavigationSpeedMouseZoom;
	public override float NavigationSpeedMouseTranslate => Editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate;
	public override float NavigationSpeedMouseRotate => Editor.Configuration.RenderingItem_NavigationSpeedMouseRotate;
	public override bool ReadOnly => true;
}
