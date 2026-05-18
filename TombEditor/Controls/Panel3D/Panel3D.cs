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
        private bool _drawHeightLine;
        // Endpoints of the height line, in room-local coordinates. The world translation
        // (_editor.SelectedObject.Room.WorldPos) is applied at draw time via the lines
        // batch's World matrix instead of being baked into a GPU buffer.
        private Vector3 _heightLineFrom, _heightLineTo;
        // Flyby path vertices, regenerated each frame by AddFlybyPath. The list is
        // reused across frames (Clear()+Add) so we don't churn allocations.
        private readonly List<SolidLineVertex> _flybyPathVertices = new List<SolidLineVertex>();

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

        // Tracks the only Configuration value that actually affects room
        // rendering (read in CacheRoom). On ConfigurationChangedEvent we
        // compare against this and invalidate the cache ONLY if it changed —
        // otherwise toggling ShowHorizon / ShowMoveables / any other config
        // flag would needlessly rebuild every room's vertex buffer (each
        // BuildVertexBuffer does a synchronous transient submit, causing a
        // ~1s stall on a ~100-room level).
        private bool? _lastProbeAttributesThroughPortals;

        // Tappa-1 unified path: shared dynamic line batch reused by every draw method
        // that previously built one-off SolidVertex buffers through the legacy stack.
        // SetVertices() + Render() per call site; the underlying ID3D11Buffer is the
        // device-wide ring (see Dx11DynamicVertexBufferPool).
        private RenderingDrawingLines _linesBatch;

        // Lazily-built RenderingDrawingMesh per legacy ObjectMesh. The legacy mesh
        // owns the canonical CPU-side vertex/index data (built by ObjectMesh.FromWad2
        // through WadRenderer); we mirror it into a unified-path GPU mesh on first
        // use and hold it until the legacy mesh is invalidated (a new ObjectMesh
        // reference is created).
        // NOTE: this introduces a memory duplication (legacy GPU buffer + new GPU
        // buffer for the same mesh) until the legacy path is removed entirely.
        // Stale entries are pruned on Panel3D dispose.
        private readonly Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh> _meshCache = new Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh>();
        // Same cache pattern but for imported-geometry meshes (different vertex layout
        // and shader). Keyed by the legacy ImportedGeometryMesh reference.
        private readonly Dictionary<TombLib.LevelData.ImportedGeometryMesh, RenderingDrawingImportedGeometry> _importedMeshCache = new Dictionary<TombLib.LevelData.ImportedGeometryMesh, RenderingDrawingImportedGeometry>();

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

                _renderingStateBuffer?.Dispose();
                _renderingTextures?.Dispose();
                _renderingCachedRooms?.Dispose();
                _linesBatch?.Dispose();
                foreach (var m in _meshCache.Values)
                    m.Dispose();
                _meshCache.Clear();
                foreach (var m in _importedMeshCache.Values)
                    m.Dispose();
                _importedMeshCache.Clear();
                _gizmo?.Dispose();
                _movementTimer?.Dispose();
                _flyModeTimer?.Dispose();
                _flybyPreview?.Dispose();
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
                if (obj is Editor.RoomGeometryChangedEvent || obj is Editor.RoomPositionChangedEvent)
                    foreach (var portal in room.Portals)
                        _renderingCachedRooms.Remove(portal.AdjoiningRoom);
            }

            if (obj is Editor.ObjectChangedEvent)
            {
                var value = (Editor.ObjectChangedEvent)obj;
                if (value.ChangeType != ObjectChangeType.Remove && value.Object is LightInstance)
                    _renderingCachedRooms.Remove(value.Object.Room);
            }

            // Reset rooms render cache
            if (obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.HighlightedSectorChangedEvent)
                _renderingCachedRooms.Remove(_editor.SelectedRoom);
            if (obj is Editor.SelectedRoomChangedEvent)
                _renderingCachedRooms.Remove(((Editor.SelectedRoomChangedEvent)obj).Previous);
            if (obj is Editor.RoomSectorPropertiesChangedEvent)
                _renderingCachedRooms.Remove(((Editor.RoomSectorPropertiesChangedEvent)obj).Room);
            if (obj is Editor.LoadedTexturesChangedEvent ||
                obj is Editor.LoadedImportedGeometriesChangedEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is SectorColoringManager.ChangeSectorColoringInfoEvent)
                _renderingCachedRooms.Clear();
            else if (obj is Editor.ConfigurationChangedEvent)
            {
                // Only invalidate the room cache if a setting that actually
                // affects room rendering changed. Currently that's just
                // UI_ProbeAttributesThroughPortals (read in CacheRoom). Other
                // bool toggles like ShowHorizon don't touch room geometry.
                bool current = _editor.Configuration.UI_ProbeAttributesThroughPortals;
                if (_lastProbeAttributesThroughPortals != current)
                {
                    _lastProbeAttributesThroughPortals = current;
                    _renderingCachedRooms.Clear();
                }
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

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            OnMouseWheelScroll(e.Delta, e.Location);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            OnMouseButtonDown(e.Button, e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            OnMouseButtonUp(e.Button, e.Location);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            OnMouseDoubleClicked(e.Button, e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            OnMouseMoved(e.Button, e.Location);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            OnMouseEntered();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            OnMouseDragEntered(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            OnMouseDragAndDrop(e);
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
    }
}