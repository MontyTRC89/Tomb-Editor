using System.Drawing;
using System.Numerics;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombEditor;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Immutable per-frame description of what the V2 renderer should draw.
/// Built by Panel3D right before <see cref="LevelRenderer.RenderFrame"/>;
/// renderer passes read it without touching editor state directly.
///
/// <para>Carries enough selection / coloring info for <see cref="SectorTextureDefault"/>
/// to classify every sector face exactly like the legacy renderer does.</para>
/// </summary>
public readonly struct RenderScene
{
    public readonly Level     Level;
    public readonly Camera    Camera;
    public readonly Size      ViewportSize;
    public readonly Matrix4x4 ViewProjection;

    public readonly Room?              SelectedRoom;
    public readonly RectangleInt2      SelectionArea;
    public readonly RectangleInt2      HighlightArea;
    public readonly ArrowType          SelectionArrow;
    public readonly SectorColoringInfo ColoringInfo;
    public readonly bool               ShowIllegalSlopes;
    public readonly bool               ShowSlideDirections;
    public readonly bool               ProbeAttributesThroughPortals;
    public readonly bool               HideHiddenRooms;
    public readonly bool               ShowAllRooms;
    public readonly bool               ShowPortals;
    public readonly bool               ShowMoveables;
    public readonly bool               ShowStatics;
    public readonly bool               ShowImportedGeometry;
    public readonly EditorMode         Mode;
    public readonly float              GridLineWidth;

    public RenderScene(
        Level             level,
        Camera            camera,
        Size              viewportSize,
        Room?             selectedRoom,
        RectangleInt2     selectionArea,
        RectangleInt2     highlightArea,
        ArrowType         selectionArrow,
        SectorColoringInfo coloringInfo,
        bool              showIllegalSlopes,
        bool              showSlideDirections,
        bool              probeAttributesThroughPortals,
        bool              hideHiddenRooms,
        bool              showAllRooms,
        bool              showPortals,
        bool              showMoveables,
        bool              showStatics,
        bool              showImportedGeometry,
        EditorMode        mode,
        float             gridLineWidth)
    {
        Level                         = level;
        Camera                        = camera;
        ViewportSize                  = viewportSize;
        ViewProjection                = camera.GetViewProjectionMatrix(viewportSize.Width, viewportSize.Height);
        SelectedRoom                  = selectedRoom;
        SelectionArea                 = selectionArea;
        HighlightArea                 = highlightArea;
        SelectionArrow                = selectionArrow;
        ColoringInfo                  = coloringInfo;
        ShowIllegalSlopes             = showIllegalSlopes;
        ShowSlideDirections           = showSlideDirections;
        ProbeAttributesThroughPortals = probeAttributesThroughPortals;
        HideHiddenRooms               = hideHiddenRooms;
        ShowAllRooms                  = showAllRooms;
        ShowPortals                   = showPortals;
        ShowMoveables                 = showMoveables;
        ShowStatics                   = showStatics;
        ShowImportedGeometry          = showImportedGeometry;
        Mode                          = mode;
        GridLineWidth                 = gridLineWidth;
    }

    /// <summary>Texturing mode renders real textures with lighting, grid off.</summary>
    public bool TexturingMode => Mode == EditorMode.FaceEdit;
}
