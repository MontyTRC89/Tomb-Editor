using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.RenderingV2.Text;
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
    public readonly bool               ShowOtherObjects;
    public readonly bool               ShowLightMeshes;
    public readonly bool               ShowLightingWhiteTextureOnly;
    public readonly bool               ShowHorizon;
    public readonly EditorMode         Mode;
    public readonly float              GridLineWidth;
    public readonly BaseGizmo.PublicState? GizmoState;
    // Highlighted objects (currently selected single object, or all members
    // of a selected ObjectGroup). Drives the red selection tint on
    // moveables / statics / imported geometry. May be null when nothing is
    // selected.
    public readonly HighlightedObjects? Highlighted;
    public readonly Vector4 SelectionTint;
    // Text overlay for this frame — room names, coordinates, object info,
    // cardinal directions, FPS. Built by Panel3D; the renderer only projects
    // and draws them. May be null when there is nothing to label.
    public readonly IReadOnlyList<TextLabel>? Labels;
    // Editor overlay geometry toggles (drawn by EditorGeometryRenderer).
    public readonly bool ShowGhostBlocks;
    public readonly bool ShowVolumes;
    public readonly bool ShowBoundingBoxes;
    public readonly bool ShowRoomBounds;
    // Selected object's vertical line down to the floor under it (world space).
    public readonly (Vector3 From, Vector3 To)? ObjectHeightLine;
    // Object-brush overlay (painting mode): floor circle at Center, world-unit Radius.
    public readonly (Vector3 Center, float Radius)? Brush;
    // Flyby depth-of-field overlay (TombEngine) — null when no DOF flyby is selected.
    public readonly (Vector4 CenterRange, Vector4 DirectionDistance, Vector4 ColorStrength)? Dof;
    // Sector split highlighted in the editor (0 = none) — drives the split-highlight ribbons.
    public readonly int HighlightedSplit;
    // Flyby sequence whose solid path tube should be drawn (-1 = none selected).
    public readonly int FlybyPathSequence;

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
        bool              showOtherObjects,
        bool              showLightMeshes,
        bool              showLightingWhiteTextureOnly,
        bool              showHorizon,
        EditorMode        mode,
        float             gridLineWidth,
        BaseGizmo.PublicState? gizmoState = null,
        HighlightedObjects? highlighted = null,
        Vector4? selectionTint = null,
        IReadOnlyList<TextLabel>? labels = null,
        bool showGhostBlocks = false,
        bool showVolumes = false,
        bool showBoundingBoxes = false,
        bool showRoomBounds = false,
        (Vector3 From, Vector3 To)? objectHeightLine = null,
        (Vector3 Center, float Radius)? brush = null,
        (Vector4 CenterRange, Vector4 DirectionDistance, Vector4 ColorStrength)? dof = null,
        int highlightedSplit = 0,
        int flybyPathSequence = -1)
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
        ShowOtherObjects              = showOtherObjects;
        ShowLightMeshes               = showLightMeshes;
        ShowLightingWhiteTextureOnly  = showLightingWhiteTextureOnly;
        ShowHorizon                   = showHorizon;
        Mode                          = mode;
        GridLineWidth                 = gridLineWidth;
        GizmoState                    = gizmoState;
        Highlighted                   = highlighted;
        SelectionTint                 = selectionTint ?? new Vector4(1f, 0f, 0f, 1f);
        Labels                        = labels;
        ShowGhostBlocks               = showGhostBlocks;
        ShowVolumes                   = showVolumes;
        ShowBoundingBoxes             = showBoundingBoxes;
        ShowRoomBounds                = showRoomBounds;
        ObjectHeightLine              = objectHeightLine;
        Brush                         = brush;
        Dof                           = dof;
        HighlightedSplit              = highlightedSplit;
        FlybyPathSequence             = flybyPathSequence;
    }

    /// <summary>Texturing mode renders real textures full-bright, grid off.</summary>
    public bool TexturingMode => Mode == EditorMode.FaceEdit;
    /// <summary>Lighting mode renders real textures modulated by vertex lighting, grid off.</summary>
    public bool LightingMode => Mode == EditorMode.Lighting;

    /// <summary>
    /// One of three rendering strategies for a room's vertex buffer. Used as
    /// the mesh-cache invalidation key — flipping modes drops every cached
    /// room so it gets rebuilt with the new strategy.
    /// </summary>
    public enum RoomDrawKind : byte
    {
        /// <summary>Sector classification colours + sector overlay sprites + grid.</summary>
        Geometry,
        /// <summary>Real textures full-bright, no grid (FaceEdit / ObjectPlacement).</summary>
        Texturing,
        /// <summary>Real textures × per-vertex lighting, no grid (Lighting).</summary>
        Lighting,
    }

    public RoomDrawKind DrawKind => Mode switch
    {
        // FaceEdit + ObjectPlacement render textures full-bright with no
        // grid; they only differ in cursor / brush behaviour, not in the
        // room mesh build.
        EditorMode.FaceEdit        => RoomDrawKind.Texturing,
        EditorMode.ObjectPlacement => RoomDrawKind.Texturing,
        EditorMode.Lighting        => RoomDrawKind.Lighting,
        _                          => RoomDrawKind.Geometry,
    };
}
