#nullable enable

using System.Windows.Media;
using TombLib.LevelData;
using TombLib.WPF.Controls;
using TombLib.WPF.Features.AnimatedTextures;

namespace WadTool.Controls;

/// <summary>
/// Wad Tool flavour of the WPF texture-map view for the animated-textures editor: same
/// configuration hooks as the WinForms <see cref="PanelTextureMap"/>, plus the animated-texture
/// set outlines (WPF counterpart of <c>PanelTextureMapForAnimations</c>).
/// </summary>
public class WpfAnimatedTextureMapView : TextureMapBase, IAnimatedTextureMap
{
    private readonly WadToolClass _tool;

    public WpfAnimatedTextureMapView(WadToolClass tool)
    {
        _tool = tool;
    }

    protected override float TileSelectionSize => 32.0f;
    protected override bool ResetAttributesOnNewSelection => false;
    protected override bool MouseWheelMovesTheTextureInsteadOfZooming => _tool.Configuration.MeshEditor_MouseWheelMovesTheTextureInsteadOfZooming;
    protected override float NavigationSpeedKeyMove => 100.0f;
    protected override float NavigationSpeedKeyZoom => 0.15f;
    protected override float NavigationSpeedMouseZoom => _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom * 0.00225f;
    protected override float NavigationSpeedMouseWheelZoom => _tool.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom * 0.00025f;
    protected override float NavigationMaxZoom => 2000.0f;
    protected override float NavigationMinZoom => 0.5f;
    protected override bool DrawSelectionDirectionIndicators => true;

    protected override float MaxTextureSize => float.PositiveInfinity;
    protected override bool DrawTriangle => false;

    private AnimatedTextureSet? _selectedSet;
    public AnimatedTextureSet? SelectedSet
    {
        get => _selectedSet;
        set
        {
            _selectedSet = value;
            InvalidateVisual();
        }
    }

    protected override void OnPaintSelection(DrawingContext drawingContext)
    {
        if (_tool.DestinationWad is not null)
            AnimatedTextureSetsPainter.Paint(drawingContext, _tool.DestinationWad.AnimatedTextureSets, _selectedSet,
                VisibleTexture, ToVisualCoord, VisualTreeHelper.GetDpi(this).PixelsPerDip);

        base.OnPaintSelection(drawingContext);
    }
}
