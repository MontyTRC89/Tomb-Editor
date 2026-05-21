using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TombEditor.Controls.ContextMenus;
using TombEditor.Controls.FlybyTimeline.Preview;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D : RenderingPanel
    {
        private static readonly KeyMessageFilter _filter = new KeyMessageFilter();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Camera Camera { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowPortals { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowRoomNames { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowCardinalDirections { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowHorizon { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowMoveables { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAllRooms { get; set; } = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowStatics { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowImportedGeometry { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowGhostBlocks { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowVolumes { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowLightMeshes { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowBoundingBoxes { get; set; } = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowOtherObjects { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisablePickingForImportedGeometry { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowExtraBlendingModes { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowLightingWhiteTextureOnly { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowRealTintForObjects { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HideTransparentFaces { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool BilinearFilter { get; set; }

        // These options require explicit setters because they probe into room cache.

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowSlideDirections
        {
            get { return _drawSlideDirections; }
            set { if (value == _drawSlideDirections) return; _drawSlideDirections = value; _renderingCachedRooms?.Clear(); }
        }
        private bool _drawSlideDirections = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowIllegalSlopes
        {
            get { return _drawIllegalSlopes; }
            set { if (value == _drawIllegalSlopes) return; _drawIllegalSlopes = value; _renderingCachedRooms?.Clear(); }
        }
        private bool _drawIllegalSlopes = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisablePickingForHiddenRooms
        {
            get { return _disablePickingForHiddenRooms; }
            set { if (value == _disablePickingForHiddenRooms) return; _disablePickingForHiddenRooms = value; _renderingCachedRooms?.Clear(); }
        }
        private bool _disablePickingForHiddenRooms = false;

        // Overall state
        private readonly Editor _editor;
        private readonly Func<Camera> _getViewportCamera;
        private Vector3? _currentRoomLastPos;

        // Camera state
        private Vector3 _lastCameraPos;
        private Vector3 _nextCameraPos;
        private Vector2 _lastCameraRot;
        private Vector2 _nextCameraRot;
        private float _lastCameraDist;
        private float _nextCameraDist;
        private readonly Timer _flyModeTimer;
        private Camera _oldCamera;
        private Frustum _frustum;
        private Matrix4x4 _viewProjection;

        // Flyby preview state
        private FlybyPreview _flybyPreview;

        // Mouse interaction state
        private Point _lastMousePosition;
        private Point _startMousePosition;
        private bool _objectPlaced = false;
        private bool _doSectorSelection;
        private bool _noSelectionConfirm;
        private Gizmo _gizmo;
        private bool _gizmoEnabled = false;
        private BaseContextMenu _currentContextMenu;
        private ToolHandler _toolHandler;
        private readonly MovementTimer _movementTimer;
        private bool _dragObjectPicked = false;
        private bool _dragObjectMoved = false;
        private HighlightedObjects _highlightedObjects = HighlightedObjects.Create(null);

        // Legacy rendering state
        private WadRenderer _wadRenderer;
        private RasterizerState _rasterizerStateDepthBias;
        private GraphicsDevice _legacyDevice;
        private RasterizerState _rasterizerWireframe;
        private GeometricPrimitive _sphere;
        private GeometricPrimitive _cone;
        private GeometricPrimitive _linesCube;
        private GeometricPrimitive _littleCube;
        private GeometricPrimitive _littleSphere;
        private bool _drawHeightLine;
        private Buffer<SolidVertex> _objectHeightLineVertexBuffer;
        private Buffer<SolidVertex> _flybyPathVertexBuffer;
        private Buffer<SolidVertex> _ghostBlockVertexBuffer;
        private Buffer<SolidVertex> _boxVertexBuffer;

        // Flyby stuff
        private const float _flybyPathThickness = 32.0f;
        private const int _flybyPathSmoothness = 7;
        private static readonly List<VectorInt2> _flybyPathIndices = new List<VectorInt2>()
        {
            new VectorInt2(0, 0),
            new VectorInt2(1, 0),
            new VectorInt2(1, 1),
            new VectorInt2(1, 1),
            new VectorInt2(0, 1),
            new VectorInt2(0, 0),
            new VectorInt2(2, 0),
            new VectorInt2(1, 0),
            new VectorInt2(1, 1),
            new VectorInt2(1, 1),
            new VectorInt2(2, 1),
            new VectorInt2(2, 0),
            new VectorInt2(0, 0),
            new VectorInt2(2, 0),
            new VectorInt2(2, 1),
            new VectorInt2(2, 1),
            new VectorInt2(0, 1),
            new VectorInt2(0, 0)
        };

        // Other drawing consts
        private const float _littleCubeRadius = 128.0f;
        private const float _littleSphereRadius = 128.0f;
        private const float _coneRadius = 1024.0f;

        // Rendering state
        private RenderingStateBuffer _renderingStateBuffer;
        private RenderingTextureAllocator _renderingTextures;
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;
        private readonly Cache<Room, RenderingDrawingRoom> _renderingCachedRooms;

        // V2 renderer. Non-null only when Configuration.Rendering3D_UseV2Renderer
        // was set at panel init. When set, the legacy rendering path is bypassed
        // entirely (no swapchain / textures / state buffer / legacy device init).
        internal TombEditor.Rendering.V2.LevelRenderer _v2Renderer;

        // Render stats
        private readonly Stopwatch _watch = new Stopwatch();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private IntPtr _lastWindow { get; set; }

        public Panel3D()
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            if (Editor.Instance is not null)
            {
                _getViewportCamera = () => Camera;

                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
                _editor.GetViewportCamera = _getViewportCamera;

                _frustum = new Frustum();
                _viewProjection = Matrix4x4.Identity;

                _toolHandler = new ToolHandler(this);
                _movementTimer = new MovementTimer(MoveTimer_Tick);

                _flyModeTimer = new Timer { Interval = 1 };
                _flyModeTimer.Tick += FlyModeTimer_Tick;

                _renderingCachedRooms = new Cache<Room, RenderingDrawingRoom>(1024, CacheRoom);
                Application.AddMessageFilter(_filter);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_editor is not null)
                {
                    _editor.EditorEventRaised -= EditorEventRaised;

                    if (_editor.GetViewportCamera == _getViewportCamera)
                        _editor.GetViewportCamera = null;
                }

                _v2Renderer?.Dispose();
                _renderingStateBuffer?.Dispose();
                _renderingTextures?.Dispose();
                _renderingCachedRooms?.Dispose();
                _rasterizerWireframe?.Dispose();
                _objectHeightLineVertexBuffer?.Dispose();
                _flybyPathVertexBuffer?.Dispose();
                _gizmo?.Dispose();
                _sphere?.Dispose();
                _cone?.Dispose();
                _linesCube?.Dispose();
                _littleCube?.Dispose();
                _littleSphere?.Dispose();
                _movementTimer?.Dispose();
                _flyModeTimer?.Dispose();
                _flybyPreview?.Dispose();
                _rasterizerStateDepthBias?.Dispose();
                _currentContextMenu?.Dispose();
                _wadRenderer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CanUseGizmo()
        {
            if (_editor.CameraPreviewMode != CameraPreviewType.None || _editor.SelectedObject is null || Camera is null)
                return false;

            if (_editor.SelectedObject is FlybyCameraInstance flyby)
            {
                float minimumDistance = _coneRadius * 0.5f;
                return Vector3.DistanceSquared(flyby.WorldPosition, Camera.GetPosition()) >= minimumDistance * minimumDistance;
            }

            return true;
        }

        private IReadOnlyList<Keys> _splitHighlightHotkeys;

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.InitEvent)
            {
                _splitHighlightHotkeys = _editor.Configuration.UI_Hotkeys
                    .Where(x => x.Key.StartsWith("HighlightSplit"))
                        .SelectMany(kv => kv.Value.Select(hk => hk.Keys)).ToList();
            }

            // Update FOV
            if (obj is Editor.ConfigurationChangedEvent)
            {
                Camera.FieldOfView = _editor.Configuration.Rendering3D_FieldOfView * (float)(Math.PI / 180);

                _splitHighlightHotkeys = _editor.Configuration.UI_Hotkeys
                    .Where(x => x.Key.StartsWith("HighlightSplit"))
                        .SelectMany(kv => kv.Value.Select(hk => hk.Keys)).ToList();
            }

            // Move camera position with room movements
            if (obj is Editor.RoomPositionChangedEvent && _editor.Mode == EditorMode.Map2D && _currentRoomLastPos.HasValue)
            {
                Camera.MoveCameraLinear(_editor.SelectedRoom.WorldPos - _currentRoomLastPos.Value);
                _currentRoomLastPos = _editor.SelectedRoom.WorldPos;
            }
            else if (obj is Editor.SelectedRoomChangedEvent || obj is Editor.ModeChangedEvent)
                _currentRoomLastPos = _editor.SelectedRoom.WorldPos;

            // Reset tool handler state
            if (obj is Editor.ModeChangedEvent ||
                obj is Editor.ToolChangedEvent ||
               obj is Editor.SelectedRoomChangedEvent && _editor.Tool.Tool != EditorToolType.PortalDigger)
            {
                _toolHandler?.Disengage();
            }

            // Update rooms
            if (obj is IEditorRoomChangedEvent)
            {
                var room = ((IEditorRoomChangedEvent)obj).Room;

                _renderingCachedRooms.Remove(room);
                _v2Renderer?.InvalidateRoom(room);
                if (obj is Editor.RoomGeometryChangedEvent || obj is Editor.RoomPositionChangedEvent)
                    foreach (var portal in room.Portals)
                    {
                        _renderingCachedRooms.Remove(portal.AdjoiningRoom);
                        _v2Renderer?.InvalidateRoom(portal.AdjoiningRoom);
                    }
            }

            if (obj is Editor.ObjectChangedEvent)
            {
                var value = (Editor.ObjectChangedEvent)obj;
                if (value.ChangeType != ObjectChangeType.Remove && value.Object is LightInstance)
                {
                    _renderingCachedRooms.Remove(value.Object.Room);
                    _v2Renderer?.InvalidateRoom(value.Object.Room);
                }
            }

            // Reset rooms render cache
            if (obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.HighlightedSectorChangedEvent)
            {
                _renderingCachedRooms.Remove(_editor.SelectedRoom);
                _v2Renderer?.InvalidateRoom(_editor.SelectedRoom);
            }
            if (obj is Editor.SelectedRoomChangedEvent)
            {
                var prev = ((Editor.SelectedRoomChangedEvent)obj).Previous;
                _renderingCachedRooms.Remove(prev);
                _v2Renderer?.InvalidateRoom(prev);
                // V2 bakes the SectorTextureDefault state into the mesh, so
                // entering a new room must also rebuild it (otherwise stale
                // selection ghosts from the last time this room was current).
                if (_editor.SelectedRoom != null)
                    _v2Renderer?.InvalidateRoom(_editor.SelectedRoom);
            }
            if (obj is Editor.RoomSectorPropertiesChangedEvent)
            {
                _renderingCachedRooms.Remove(((Editor.RoomSectorPropertiesChangedEvent)obj).Room);
                _v2Renderer?.InvalidateRoom(((Editor.RoomSectorPropertiesChangedEvent)obj).Room);
            }
            if (obj is Editor.LoadedTexturesChangedEvent ||
                obj is Editor.LoadedImportedGeometriesChangedEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is Editor.ConfigurationChangedEvent ||
                obj is SectorColoringManager.ChangeSectorColoringInfoEvent)
            {
                _renderingCachedRooms.Clear();
                _v2Renderer?.InvalidateAllRooms();
            }

            // V2 picking caches: triangle lists are baked from WAD/imported
            // geometry the first time an object is tested against the ray.
            // After a level reload / wad swap / imported-geometry refresh the
            // backing assets are different objects, but the cache still holds
            // the old tris — which means we'd either get stale picks or, more
            // commonly, just dictionary entries that never resolve. Wipe them
            // on the same events that invalidate the renderer caches.
            if (obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.LoadedImportedGeometriesChangedEvent ||
                obj is Editor.LevelChangedEvent)
            {
                _v2PickStatic.Clear();
                _v2PickMoveable.Clear();
                _v2PickImported.Clear();
            }

            if (obj is Editor.ObjectBrushSettingsChangedEvent)
                Invalidate();

            // Update drawing
            if (_editor.Mode != EditorMode.Map2D)
                if (obj is IEditorObjectChangedEvent ||
                    obj is Editor.SelectedObjectChangedEvent ||
                    obj is IEditorRoomChangedEvent ||
                    obj is SectorColoringManager.ChangeSectorColoringInfoEvent ||
                    obj is Editor.ConfigurationChangedEvent ||
                    obj is Editor.SelectedSectorsChangedEvent ||
                    obj is Editor.HighlightedSectorChangedEvent ||
                    obj is Editor.SelectedRoomChangedEvent ||
                    obj is Editor.ModeChangedEvent ||
                    obj is Editor.ToolChangedEvent ||
                    obj is Editor.LoadedWadsChangedEvent ||
                    obj is Editor.LoadedTexturesChangedEvent ||
                    obj is Editor.LoadedImportedGeometriesChangedEvent ||
                    obj is Editor.MergedStaticsChangedEvent ||
                    obj is Editor.EventSetsChangedEvent ||
                    obj is Editor.GameVersionChangedEvent ||
                    obj is Editor.HideSelectionEvent ||
                    obj is Editor.EditorFocusedEvent)
                    Invalidate(false);

            // Clean up wad renderer
            if (obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.LevelChangedEvent)
                _wadRenderer?.GarbageCollect();

            // Update cursor
            if (obj is Editor.ActionChangedEvent)
            {
                IEditorAction currentAction = ((Editor.ActionChangedEvent)obj).Current;
                bool hasCrossCursor = currentAction is EditorActionPlace || currentAction is EditorActionRelocateCamera;
                Cursor = hasCrossCursor ? Cursors.Cross : Cursors.Arrow;
            }

            // Center camera
            if (obj is Editor.ResetCameraEvent resetEvent)
                ResetCamera(resetEvent.NewCamera, resetEvent.Room);

            // Toggle FlyMode
            if (obj is Editor.ToggleFlyModeEvent)
                ToggleFlyMode(((Editor.ToggleFlyModeEvent)obj).FlyModeState);

            // Toggle camera preview
            if (obj is Editor.ToggleCameraPreviewEvent previewEvent)
                ToggleCameraPreview(previewEvent.PreviewState, previewEvent.Object);

            // Update camera preview from dialog or timeline scrub.
            if (obj is Editor.CameraPreviewFrameEvent frameEvent)
            {
                if (frameEvent.FlybyCameraInstance != null)
                    UpdateFlybyFramePreview(frameEvent.FlybyCameraInstance);
                else if (frameEvent.Frame.HasValue && _editor.CameraPreviewMode == CameraPreviewType.Static && _flybyPreview != null)
                {
                    _flybyPreview.SetStaticFrame(Camera, frameEvent.Frame.Value);
                    Invalidate();
                }
            }

            // Stop camera animation if level is changing
            if (obj is Editor.LevelChangedEvent)
            {
                _movementTimer.Stop(true);

                if (_editor.CameraPreviewMode != CameraPreviewType.None)
                    ToggleCameraPreview(false);
            }

            // Move camera to sector
            if (obj is Editor.MoveCameraToSectorEvent)
            {
                var e = (Editor.MoveCameraToSectorEvent)obj;

                Vector3 center = _editor.SelectedRoom.GetLocalCenter();
                var nextPos = new Vector3(e.Sector.X * Level.SectorSizeUnit + Level.HalfSectorSizeUnit, center.Y, e.Sector.Y * Level.SectorSizeUnit + Level.HalfSectorSizeUnit) + _editor.SelectedRoom.WorldPos;

                if (_editor.Configuration.Rendering3D_AnimateCameraOnRelocation)
                    AnimateCamera(nextPos);
                else
                {
                    Camera.Target = nextPos;
                    Invalidate();
                }
            }

            if (obj is Editor.SelectedObjectChangedEvent)
                _highlightedObjects = HighlightedObjects.Create(_editor.SelectedObject);

            if (obj is Editor.HighlightedSplitChangedEvent)
                Invalidate(false);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            // Block keyboard input during camera preview (except ESC)
            if (_editor.CameraPreviewMode != CameraPreviewType.None)
            {
                if (e.KeyCode == Keys.Escape)
                    ToggleCameraPreview(false);

                return;
            }

            if ((ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.None)
                _movementTimer.Engage(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _movementTimer.Stop();

            if (_editor.FlyMode && e.KeyCode == Keys.Menu)
                e.Handled = true;

            if (_editor.HighlightedSplit != 0 && _splitHighlightHotkeys.Any(hk => hk.HasFlag(e.KeyCode)))
                _editor.HighlightedSplit = 0;
        }

        // While the V2 renderer is being filled in, the legacy interactions
        // (gizmo, picking, brush, object move) dereference fields the V2 init
        // path leaves null. We short-circuit them all, but route the bits
        // that *don't* depend on legacy state (camera rotate/zoom and
        // keyboard focus) through V2-specific handlers below.
        private bool LegacyMouseDisabled => _v2Renderer is not null;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (LegacyMouseDisabled) { V2MouseWheel(e); return; }
            OnMouseWheelScroll(e.Delta, e.Location);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (LegacyMouseDisabled) { V2MouseDown(e); return; }
            OnMouseButtonDown(e.Button, e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (LegacyMouseDisabled) { V2MouseUp(e); return; }
            OnMouseButtonUp(e.Button, e.Location);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (LegacyMouseDisabled) return;
            OnMouseDoubleClicked(e.Button, e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (LegacyMouseDisabled) { V2MouseMove(e); return; }
            OnMouseMoved(e.Button, e.Location);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (LegacyMouseDisabled) { if (!Focused) Focus(); return; }
            OnMouseEntered();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            if (LegacyMouseDisabled) return;
            OnMouseDragEntered(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            if (LegacyMouseDisabled) return;
            OnMouseDragAndDrop(e);
        }

        // --- V2 input handlers ------------------------------------------------

        private MouseButtons _v2DragButton;

        // Left-mouse interaction state, mirrors the legacy _doSectorSelection
        // logic:
        //   - Clicking outside (or with no selection) → reset selection to the
        //     picked sector, start a drag-select (_v2SelDragging = true).
        //   - Clicking inside the existing selection → leave the rectangle
        //     alone (_v2SelDragging = false) and cycle the edit arrow on
        //     mouse-up. Modifier Ctrl cycles corners instead of edges.
        private Room       _v2SelAnchorRoom;
        private VectorInt2 _v2SelAnchor;
        private bool       _v2SelDragging;
        private bool       _v2SelClickedOnSel;

        private void V2MouseDown(MouseEventArgs e)
        {
            if (!Focused) Focus();
            _lastMousePosition = e.Location;
            _v2DragButton = e.Button;
            if (e.Button is MouseButtons.Right or MouseButtons.Middle)
                Capture = true;
            if (e.Button == MouseButtons.Left)
            {
                // Priority order matches the legacy DoPicking:
                //   1. gizmo handle (translate axis / rotate ring / scale cube)
                //   2. object (moveable / static / imported geometry)
                //   3. sector
                if (CanUseGizmo() && V2TryGizmoPick(e.Location))
                {
                    Capture = true;
                    Invalidate();
                    return;
                }

                // Objects take priority over sectors: clicking a moveable /
                // static / imported geometry selects the object, not the
                // floor beneath it.
                if (V2PickObject(e.Location, out var pickedObj))
                {
                    _editor.SelectedObject = pickedObj;
                    return;
                }

                if (!V2PickFace(e.Location, out var room, out var pos, out var face))
                {
                    _v2SelDragging     = false;
                    _v2SelClickedOnSel = false;
                    return;
                }

                // In FaceEdit mode the click maps to texture actions instead
                // of sector selection — match the legacy mouse-down branch.
                if (_editor.Mode == EditorMode.FaceEdit && _editor.Tool.Tool != EditorToolType.Selection)
                {
                    if (_editor.SelectedRoom != room) _editor.SelectedRoom = room;
                    V2HandleTextureClick(room, pos, face);
                    return;
                }

                bool clickedOnExisting = _editor.SelectedSectors.Valid
                                      && _editor.SelectedRoom == room
                                      && _editor.SelectedSectors.Area.Contains(pos);
                if (clickedOnExisting)
                {
                    // Don't reset the rectangle, don't drag. Wait for mouse-up
                    // to cycle the arrow.
                    _v2SelDragging     = false;
                    _v2SelClickedOnSel = true;
                }
                else
                {
                    if (_editor.SelectedRoom != room) _editor.SelectedRoom = room;
                    _v2SelAnchorRoom = room;
                    _v2SelAnchor     = pos;
                    _v2SelDragging   = true;
                    _v2SelClickedOnSel = false;
                    _editor.SelectedSectors = new SectorSelection
                    {
                        Area  = new TombLib.RectangleInt2(pos.X, pos.Y, pos.X, pos.Y),
                        Arrow = TombLib.Rendering.ArrowType.EntireFace,
                    };
                }
                Capture = true;
            }
        }

        // FaceEdit mode click dispatch — same modifier semantics as the
        // legacy Panel3DMouseDownLeft texturing branch:
        //   Shift → rotate texture
        //   Ctrl  → mirror texture
        //   Alt   → pick texture
        //   none  → apply currently-selected texture
        private void V2HandleTextureClick(Room room, VectorInt2 pos, TombLib.LevelData.SectorEnums.SectorFace face)
        {
            if (ModifierKeys.HasFlag(Keys.Shift))
                EditorActions.RotateTexture(room, pos, face);
            else if (ModifierKeys.HasFlag(Keys.Control))
                EditorActions.MirrorTexture(room, pos, face);
            else if (ModifierKeys.HasFlag(Keys.Alt))
                EditorActions.PickTexture(room, pos, face);
            else
                EditorActions.ApplyTexture(room, pos, face, _editor.SelectedTexture);
        }

        private void V2MouseUp(MouseEventArgs e)
        {
            if (e.Button == _v2DragButton)
            {
                _v2DragButton = MouseButtons.None;
                Capture = false;
            }
            if (e.Button == MouseButtons.Left)
            {
                // Release any active gizmo drag — re-enables sector pick on
                // the next click.
                if (_gizmo != null && _gizmo.MouseUp())
                    Invalidate();

                if (_v2SelClickedOnSel && _editor.SelectedSectors.Valid)
                    CycleSelectionArrow(ModifierKeys.HasFlag(Keys.Control));

                _v2SelDragging     = false;
                _v2SelClickedOnSel = false;
                _v2SelAnchorRoom   = null;
            }
        }

        // Build a world-space pick ray from screen coords, using the camera +
        // current viewport. Single helper so V2 picking + gizmo picking share
        // the same math.
        private bool V2BuildRay(System.Drawing.Point screenPos, out TombLib.Ray ray, out System.Numerics.Matrix4x4 vp)
        {
            ray = default;
            vp  = default;
            if (Camera == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return false;
            vp  = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            ray = TombLib.Ray.GetPickRay(
                new System.Numerics.Vector2(screenPos.X, screenPos.Y),
                vp, ClientSize.Width, ClientSize.Height);
            return true;
        }

        // Attempt to hit one of the gizmo handles; if successful, activates
        // the gizmo (its DoPicking sets the internal _mode so subsequent
        // MouseMoved calls drive the transform).
        private bool V2TryGizmoPick(System.Drawing.Point screenPos)
        {
            if (_gizmo == null) return false;
            if (!V2BuildRay(screenPos, out var ray, out _)) return false;
            var pick = _gizmo.DoPicking(ray);
            if (pick == null) return false;
            _gizmo.ActivateGizmo(pick);
            return true;
        }

        // Cycles SelectedSectors.Arrow on each in-selection click. Same order
        // as Panel3DMouseUp's legacy block: Edge_N → Edge_E → Edge_S → Edge_W
        // → EntireFace, and Corner_NW → Corner_NE → Corner_SE → Corner_SW
        // → EntireFace under Ctrl.
        private void CycleSelectionArrow(bool corners)
        {
            var cur = _editor.SelectedSectors.Arrow;
            TombLib.Rendering.ArrowType next;
            if (corners)
            {
                next = cur switch
                {
                    TombLib.Rendering.ArrowType.CornerSW => TombLib.Rendering.ArrowType.EntireFace,
                    TombLib.Rendering.ArrowType.CornerSE => TombLib.Rendering.ArrowType.CornerSW,
                    TombLib.Rendering.ArrowType.CornerNE => TombLib.Rendering.ArrowType.CornerSE,
                    TombLib.Rendering.ArrowType.CornerNW => TombLib.Rendering.ArrowType.CornerNE,
                    _                                    => TombLib.Rendering.ArrowType.CornerNW,
                };
            }
            else
            {
                next = cur switch
                {
                    TombLib.Rendering.ArrowType.EdgeW => TombLib.Rendering.ArrowType.EntireFace,
                    TombLib.Rendering.ArrowType.EdgeS => TombLib.Rendering.ArrowType.EdgeW,
                    TombLib.Rendering.ArrowType.EdgeE => TombLib.Rendering.ArrowType.EdgeS,
                    TombLib.Rendering.ArrowType.EdgeN => TombLib.Rendering.ArrowType.EdgeE,
                    _                                 => TombLib.Rendering.ArrowType.EdgeN,
                };
            }
            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(next);
        }

        private void V2MouseMove(MouseEventArgs e)
        {
            if (Camera == null) return;

            // Gizmo drag takes priority over sector-rectangle drag while a
            // gizmo handle is active.
            if (_gizmo != null && (e.Button & MouseButtons.Left) != 0)
            {
                if (V2BuildRay(e.Location, out var giRay, out var giVp))
                {
                    if (_gizmo.MouseMoved(giVp, giRay))
                    {
                        Invalidate();
                        _lastMousePosition = e.Location;
                        return;
                    }
                }
            }

            if (_v2SelDragging && (e.Button & MouseButtons.Left) != 0 && _v2SelAnchorRoom != null)
            {
                if (V2PickRaw(e.Location, out var hitRoom, out var pos) && hitRoom == _v2SelAnchorRoom)
                {
                    int x0 = Math.Min(_v2SelAnchor.X, pos.X);
                    int z0 = Math.Min(_v2SelAnchor.Y, pos.Y);
                    int x1 = Math.Max(_v2SelAnchor.X, pos.X);
                    int z1 = Math.Max(_v2SelAnchor.Y, pos.Y);
                    var newArea = new TombLib.RectangleInt2(x0, z0, x1, z1);
                    if (_editor.SelectedSectors.Area != newArea)
                    {
                        _editor.SelectedSectors = new SectorSelection
                        {
                            Area  = newArea,
                            Arrow = TombLib.Rendering.ArrowType.EntireFace,
                        };
                    }
                }
            }
            else if (_v2DragButton is MouseButtons.Right or MouseButtons.Middle)
            {
                var delta = Delta(e.Location, _lastMousePosition);
                if (ModifierKeys.HasFlag(Keys.Shift))
                    Camera.MoveCameraPlane(new System.Numerics.Vector3(delta.X, delta.Y, 0) *
                        _editor.Configuration.Rendering3D_NavigationSpeedMouseTranslate);
                else if (ModifierKeys.HasFlag(Keys.Control))
                    Camera.Zoom((_editor.Configuration.Rendering3D_InvertMouseZoom ? delta.Y : -delta.Y) *
                        _editor.Configuration.Rendering3D_NavigationSpeedMouseZoom);
                else
                    Camera.Rotate(
                        delta.X * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                       -delta.Y * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);
                Invalidate();
            }
            else if (e.Button == MouseButtons.None && _gizmo != null)
            {
                // Hover highlight on gizmo handles — mirrors the legacy
                // OnMouseMoved fallback path. The redraw flag tells us
                // whether the highlighted handle actually changed.
                bool redraw;
                if (CanUseGizmo() && V2BuildRay(e.Location, out var hoverRay, out _))
                    redraw = _gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(hoverRay));
                else
                    redraw = _gizmo.GizmoUpdateHoverEffect(null);
                if (redraw)
                {
                    Invalidate();
                    Update(); // legacy "magic fix for gizmo stiffness" — paint immediately
                }
            }
            _lastMousePosition = e.Location;
        }

        // Ray-cast against every visible moveable / static / imported
        // geometry instance and return the closest one. Each instance is
        // tested in its own local space via the inverse of ObjectMatrix —
        // the geometry stays cached in model coordinates so we don't have
        // to transform vertices on the CPU.
        private bool V2PickObject(System.Drawing.Point pos, out ObjectInstance picked)
        {
            picked = null;
            ObjectInstance best = null;
            if (_editor?.Level == null || Camera == null) return false;
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return false;

            var vp = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            var ray = TombLib.Ray.GetPickRay(
                new System.Numerics.Vector2(pos.X, pos.Y), vp, ClientSize.Width, ClientSize.Height);

            float bestDist = float.PositiveInfinity;

            var rooms = _v2Renderer != null
                ? _v2Renderer.LastVisibleRooms
                : (System.Collections.Generic.IReadOnlyList<Room>)Array.Empty<Room>();

            foreach (var room in rooms)
            {
                if (room?.Objects == null) continue;
                var wp = room.WorldPos;
                foreach (var obj in room.Objects)
                {
                    // --- Service objects: simple bounding sphere / box hit
                    // (matches the legacy Panel3DPicking fallback so the user
                    // can click lights / cameras / sinks / sounds / memos).
                    if (ShowOtherObjects)
                    {
                        switch (obj)
                        {
                            case LightInstance light:
                                {
                                    var sphere = new TombLib.BoundingSphere(
                                        wp + light.Position,
                                        TombEditor.Rendering.V2.ServiceObjectRenderer.LightSphereRadius);
                                    if (TombLib.Utils.Collision.RayIntersectsSphere(ray, sphere, out float d) && d < bestDist)
                                    { bestDist = d; best = obj; }
                                    continue;
                                }
                            case CameraInstance:
                            case FlybyCameraInstance:
                            case SinkInstance:
                            case SoundSourceInstance:
                            case MemoInstance:
                            case SpriteInstance:
                                {
                                    var pbi = (PositionBasedObjectInstance)obj;
                                    var half = new System.Numerics.Vector3(
                                        TombEditor.Rendering.V2.ServiceObjectRenderer.MarkerHalfExtent);
                                    var box = new TombLib.BoundingBox(
                                        wp + pbi.Position - half, wp + pbi.Position + half);
                                    if (TombLib.Utils.Collision.RayIntersectsBox(ray, box, out float d) && d < bestDist)
                                    { bestDist = d; best = obj; }
                                    continue;
                                }
                        }
                    }

                    // --- Asset-backed instances: mesh-triangle pick (current
                    // behaviour). Missing-asset placeholders are picked as
                    // boxes via the same service-object path above (after
                    // the WAD lookup fails the mesh branch returns null).
                    System.Collections.Generic.IList<System.Numerics.Vector3> verts = null;
                    System.Numerics.Matrix4x4 model = default;
                    bool isPlaceholder = false;
                    switch (obj)
                    {
                        case StaticInstance si when ShowStatics:
                            {
                                var s = _editor.Level.Settings?.WadTryGetStatic(si.WadObjectId);
                                if (s?.Mesh != null)
                                {
                                    verts = BuildStaticPickVertices(s);
                                    model = si.ObjectMatrix;
                                }
                                else isPlaceholder = true;
                                break;
                            }
                        case MoveableInstance mi when ShowMoveables:
                            {
                                var mv = _editor.Level.Settings?.WadTryGetMoveable(mi.WadObjectId);
                                if (mv != null)
                                {
                                    verts = BuildMoveablePickVertices(mv);
                                    model = mi.ObjectMatrix;
                                }
                                else isPlaceholder = true;
                                break;
                            }
                        case ImportedGeometryInstance ii when ShowImportedGeometry:
                            {
                                if (ii.Model?.DirectXModel != null &&
                                    ii.Model.DirectXModel.Meshes.Count > 0 && !ii.Hidden)
                                {
                                    verts = BuildImportedPickVertices(ii.Model);
                                    if (verts != null)
                                        model = ii.RotationPositionMatrix * System.Numerics.Matrix4x4.CreateScale(ii.Scale);
                                }
                                else isPlaceholder = true;
                                break;
                            }
                    }

                    // Placeholder cube fallback for missing wad/imported assets.
                    if (isPlaceholder && ShowOtherObjects && obj is PositionBasedObjectInstance pbi2)
                    {
                        var half = new System.Numerics.Vector3(
                            TombEditor.Rendering.V2.ServiceObjectRenderer.MarkerHalfExtent);
                        var box = new TombLib.BoundingBox(
                            wp + pbi2.Position - half, wp + pbi2.Position + half);
                        if (TombLib.Utils.Collision.RayIntersectsBox(ray, box, out float d) && d < bestDist)
                        { bestDist = d; best = obj; }
                        continue;
                    }

                    if (verts == null || verts.Count < 3) continue;
                    if (!System.Numerics.Matrix4x4.Invert(model, out var inv)) continue;
                    var localPos = System.Numerics.Vector3.Transform(ray.Position, inv);
                    var localDir = System.Numerics.Vector3.Normalize(
                        System.Numerics.Vector3.TransformNormal(ray.Direction, inv));
                    var localRay = new TombLib.Ray(localPos, localDir);
                    for (int i = 0; i + 2 < verts.Count; i += 3)
                    {
                        if (TombLib.Utils.Collision.RayIntersectsTriangle(
                                localRay, verts[i], verts[i + 1], verts[i + 2], true, out float d) && d < bestDist)
                        {
                            bestDist = d;
                            best = obj;
                        }
                    }
                }
            }
            picked = best;
            return picked != null;
        }

        // CPU-side pick caches — one triangle-list per source asset, reused
        // every time the picking ray runs. Cleared on level/wad reload via
        // the existing IEditorRoomChangedEvent / LoadedWadsChangedEvent
        // handlers (see _v2PickCache.Clear below).
        private readonly System.Collections.Generic.Dictionary<TombLib.Wad.WadStatic, System.Collections.Generic.List<System.Numerics.Vector3>>          _v2PickStatic   = new();
        private readonly System.Collections.Generic.Dictionary<TombLib.Wad.WadMoveable, System.Collections.Generic.List<System.Numerics.Vector3>>        _v2PickMoveable = new();
        private readonly System.Collections.Generic.Dictionary<TombLib.LevelData.ImportedGeometry, System.Collections.Generic.List<System.Numerics.Vector3>> _v2PickImported = new();

        private System.Collections.Generic.List<System.Numerics.Vector3> BuildStaticPickVertices(TombLib.Wad.WadStatic s)
        {
            if (_v2PickStatic.TryGetValue(s, out var list)) return list;
            list = new System.Collections.Generic.List<System.Numerics.Vector3>();
            AppendTrisFromWadMesh(list, s.Mesh, System.Numerics.Matrix4x4.Identity);
            _v2PickStatic[s] = list;
            return list;
        }

        private System.Collections.Generic.List<System.Numerics.Vector3> BuildMoveablePickVertices(TombLib.Wad.WadMoveable mv)
        {
            if (_v2PickMoveable.TryGetValue(mv, out var list)) return list;
            list = new System.Collections.Generic.List<System.Numerics.Vector3>();

            // Same first-frame pose math as ObjectRenderer.BuildMoveableMesh
            // so the pickable hull lines up with what's drawn on screen.
            TombLib.Wad.WadKeyFrame frame = (mv.Animations.Count > 0 && mv.Animations[0].KeyFrames.Count > 0)
                                            ? mv.Animations[0].KeyFrames[0] : null;

            var transforms = new System.Collections.Generic.Dictionary<TombLib.Wad.WadBone, System.Numerics.Matrix4x4>(mv.Bones.Count);
            int bi = 0;
            foreach (var bone in mv.Bones)
            {
                var rot = (frame != null && bi < frame.Angles.Count)
                          ? frame.Angles[bi].RotationMatrix
                          : System.Numerics.Matrix4x4.Identity;
                System.Numerics.Matrix4x4 g;
                if (bone.Parent == null)
                {
                    var off = frame != null ? frame.Offset : System.Numerics.Vector3.Zero;
                    g = rot * System.Numerics.Matrix4x4.CreateTranslation(off);
                }
                else
                {
                    var pt = transforms.TryGetValue(bone.Parent, out var par)
                             ? par : System.Numerics.Matrix4x4.Identity;
                    g = rot * System.Numerics.Matrix4x4.CreateTranslation(bone.Translation) * pt;
                }
                transforms[bone] = g;
                if (bone.Mesh != null) AppendTrisFromWadMesh(list, bone.Mesh, g);
                bi++;
            }
            _v2PickMoveable[mv] = list;
            return list;
        }

        private System.Collections.Generic.List<System.Numerics.Vector3> BuildImportedPickVertices(TombLib.LevelData.ImportedGeometry imp)
        {
            if (imp?.DirectXModel == null) return null;
            if (_v2PickImported.TryGetValue(imp, out var list)) return list;
            list = new System.Collections.Generic.List<System.Numerics.Vector3>();
            foreach (var mesh in imp.DirectXModel.Meshes)
            {
                foreach (var submesh in mesh.Submeshes.Values)
                {
                    for (int k = 0; k < submesh.NumIndices; k++)
                    {
                        int vi = mesh.Indices[submesh.BaseIndex + k];
                        list.Add(mesh.Vertices[vi].Position);
                    }
                }
            }
            _v2PickImported[imp] = list;
            return list;
        }

        private static void AppendTrisFromWadMesh(System.Collections.Generic.List<System.Numerics.Vector3> list,
                                                  TombLib.Wad.WadMesh mesh, System.Numerics.Matrix4x4 transform)
        {
            if (mesh == null) return;
            var pos = mesh.VertexPositions;
            foreach (var poly in mesh.Polys)
            {
                void P(int i) { if (i >= 0 && i < pos.Count) list.Add(System.Numerics.Vector3.Transform(pos[i], transform)); }
                if (poly.Shape == TombLib.Wad.WadPolygonShape.Triangle)
                { P(poly.Index0); P(poly.Index1); P(poly.Index2); }
                else
                { P(poly.Index0); P(poly.Index1); P(poly.Index2);
                  P(poly.Index0); P(poly.Index2); P(poly.Index3); }
            }
        }

        // Common ray-pick used by single-click, drag-select and texturing.
        // While a drag is active it sticks to the anchor room (so the
        // selection rectangle can't jump into a neighbour); otherwise it
        // picks within the currently selected room (legacy default).
        private bool V2PickRaw(System.Drawing.Point pos, out Room room, out VectorInt2 sector) =>
            V2PickFace(pos, out room, out sector, out _);

        private bool V2PickFace(System.Drawing.Point pos, out Room room, out VectorInt2 sector,
                                out TombLib.LevelData.SectorEnums.SectorFace face)
        {
            room   = null;
            sector = default;
            face   = default;
            if (_editor?.Level == null || Camera == null) return false;
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return false;

            var target = _v2SelDragging && _v2SelAnchorRoom != null
                       ? _v2SelAnchorRoom
                       : _editor.SelectedRoom;
            if (target?.RoomGeometry == null) return false;

            var vp = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            var ray = TombLib.Ray.GetPickRay(
                new System.Numerics.Vector2(pos.X, pos.Y), vp, ClientSize.Width, ClientSize.Height);
            ray.Position -= target.WorldPos;

            var hit = target.RoomGeometry.RayIntersectsGeometry(ray);
            if (hit == null) return false;

            room   = target;
            sector = hit.Value.Pos;
            face   = hit.Value.Face;
            return true;
        }

        private void V2MouseWheel(MouseEventArgs e)
        {
            if (Camera == null) return;
            float dir = e.Delta > 0 ? -1f : 1f;
            Camera.Zoom(dir * _editor.Configuration.Rendering3D_NavigationSpeedMouseWheelZoom);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _movementTimer.Stop();
        }

        // Do NOT call this method to redraw the scene!
        // Call Invalidate() instead to schedule a redraw in the message loop.
        protected override void OnDraw()
        {
            DrawScene();
        }

        // When the V2 renderer is active, we bypass RenderingPanel's paint flow
        // (which assumes the legacy SwapChain is alive) and let V2 own the
        // entire client area: no background clear, no fallback messages, no
        // legacy Clear/Present.
        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
        {
            if (_v2Renderer is not null) return;
            base.OnPaintBackground(e);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (_v2Renderer is not null)
            {
                if (_editor?.Level is not null && Camera is not null && ClientSize.Width > 0 && ClientSize.Height > 0)
                {
                    TombLib.Graphics.BaseGizmo.PublicState? gizmoSnap = null;
                    if (_gizmo != null && CanUseGizmo())
                        gizmoSnap = _gizmo.GetPublicState();

                    var scene = new TombEditor.Rendering.V2.RenderScene(
                        level:                         _editor.Level,
                        camera:                        Camera,
                        viewportSize:                  ClientSize,
                        selectedRoom:                  _editor.SelectedRoom,
                        selectionArea:                 _editor.SelectedSectors.Area,
                        highlightArea:                 _editor.HighlightedSectors.Area,
                        selectionArrow:                _editor.SelectedSectors.Arrow,
                        coloringInfo:                  _editor.SectorColoringManager.ColoringInfo,
                        showIllegalSlopes:             ShowIllegalSlopes,
                        showSlideDirections:           ShowSlideDirections,
                        probeAttributesThroughPortals: _editor.Configuration.UI_ProbeAttributesThroughPortals,
                        hideHiddenRooms:               DisablePickingForHiddenRooms,
                        showAllRooms:                  ShowAllRooms,
                        showPortals:                   ShowPortals,
                        showMoveables:                 ShowMoveables,
                        showStatics:                   ShowStatics,
                        showImportedGeometry:          ShowImportedGeometry,
                        showOtherObjects:              ShowOtherObjects,
                        showLightMeshes:               ShowLightMeshes,
                        showLightingWhiteTextureOnly:  ShowLightingWhiteTextureOnly,
                        showHorizon:                   ShowHorizon,
                        mode:                          _editor.Mode,
                        gridLineWidth:                 _editor.Configuration.Rendering3D_LineWidth,
                        gizmoState:                    gizmoSnap,
                        highlighted:                   _highlightedObjects,
                        selectionTint:                 _editor.Configuration.UI_ColorScheme.ColorSelection);
                    _v2Renderer.RenderFrame(scene);
                }
                else
                {
                    _v2Renderer.RenderFrame();
                }
                return;
            }
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (_v2Renderer is not null && ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                _v2Renderer.Resize(ClientSize.Width, ClientSize.Height);
                Invalidate();
                return;
            }
            base.OnResize(e);
        }
    }
}