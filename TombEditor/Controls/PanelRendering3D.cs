using DarkUI.Controls;
using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TombEditor.Controls.ContextMenus;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Controls
{
    public class PanelRendering3D : RenderingPanel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
        private bool _drawIllegalSlopes = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowIllegalSlopes { get { return _drawIllegalSlopes; } set { if (value == _drawIllegalSlopes) return; _drawIllegalSlopes = value; _renderingCachedRooms.Clear(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowMoveables { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAllRooms { get; set; } = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowStatics { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowImportedGeometry { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowLightMeshes { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowOtherObjects { get; set; } = true;
        private bool _drawSlideDirections = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowSlideDirections { get { return _drawSlideDirections; } set { if (value == _drawSlideDirections) return; _drawSlideDirections = value; _renderingCachedRooms.Clear(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisablePickingForImportedGeometry { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowExtraBlendingModes { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static bool FlyMode { get; private set; } = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static Keys ToggleFlyModeKeys { get; set; } = Keys.Z;

        private Camera _oldCamera;

        // Overall state
        private readonly Editor _editor;
        private Vector3? _currentRoomLastPos;

        // Camera state
        private Vector3 _lastCameraPos;
        private Vector3 _nextCameraPos;
        private Vector2 _lastCameraRot;
        private Vector2 _nextCameraRot;
        private float _lastCameraDist;
        private float _nextCameraDist;
        private readonly Timer _flyModeTimer;

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
        private const float _littleCubeRadius = 128.0f;
        private const float _littleSphereRadius = 128.0f;
        private Buffer<SolidVertex> _objectHeightLineVertexBuffer;
        private bool _drawHeightLine;
        private Buffer<SolidVertex> _flybyPathVertexBuffer;
        private bool _drawFlybyPath;

        // Rendering state
        private RenderingStateBuffer _renderingStateBuffer;
        private RenderingTextureAllocator _renderingTextures;
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;
        private readonly Cache<Room, RenderingDrawingRoom> _renderingCachedRooms;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private IntPtr LastWindow { get; set; }

        public PanelRendering3D()
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;

                _toolHandler = new ToolHandler(this);
                _movementTimer = new MovementTimer(MoveTimer_Tick);

                _flyModeTimer = new Timer { Interval = 1 };
                _flyModeTimer.Tick += new EventHandler(FlyModeTimer_Tick);

                _renderingCachedRooms = new Cache<Room, RenderingDrawingRoom>(1024,
                    delegate (Room room)
                    {
                        var sectorTextures = new SectorTextureDefault
                        {
                            ColoringInfo = _editor.SectorColoringManager.ColoringInfo,
                            DrawIllegalSlopes = ShowIllegalSlopes,
                            DrawSlideDirections = ShowSlideDirections,
                            ProbeAttributesThroughPortals = _editor.Configuration.UI_ProbeAttributesThroughPortals,
                        };

                        if (_editor.SelectedRoom == room)
                        {
                            sectorTextures.HighlightArea = _editor.HighlightedSectors.Area;
                            sectorTextures.SelectionArea = _editor.SelectedSectors.Area;
                            sectorTextures.SelectionArrow = _editor.SelectedSectors.Arrow;
                        }

                        return Device.CreateDrawingRoom(
                             new RenderingDrawingRoom.Description
                             {
                                 Room = room,
                                 TextureAllocator = _renderingTextures,
                                 SectorTextureGet = sectorTextures.Get
                             });
                    });
            }
        }

        private Room GetCurrentRoom()
        {
            foreach (var room in _editor.Level.Rooms)
            {
                if (room == null)
                    continue;

                Vector3 p = Camera.GetPosition();
                BoundingBox b = room.WorldBoundingBox;

                if (p.X >= b.Minimum.X && p.Y >= b.Minimum.Y && p.Z >= b.Minimum.Z &&
                    p.X <= b.Maximum.X && p.Y <= b.Maximum.Y && p.Z <= b.Maximum.Z)
                {
                    return room;
                }
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
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
                _rasterizerStateDepthBias?.Dispose();
                _currentContextMenu?.Dispose();
                _wadRenderer?.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void InitializeRendering(RenderingDevice device, bool antialias)
        {
            base.InitializeRendering(device, antialias);

            _renderingTextures = device.CreateTextureAllocator(new RenderingTextureAllocator.Description());
            _renderingStateBuffer = device.CreateStateBuffer();
            _fontTexture = device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });

            _fontDefault = device.CreateFont(new RenderingFont.Description
            {
                FontName = _editor.Configuration.Rendering3D_FontName,
                FontSize = _editor.Configuration.Rendering3D_FontSize,
                FontIsBold = _editor.Configuration.Rendering3D_FontIsBold,
                TextureAllocator = _fontTexture
            });
            // Legacy
            {
                _legacyDevice = DeviceManager.DefaultDeviceManager.___LegacyDevice;
                _wadRenderer = new WadRenderer(_legacyDevice, true);

                // Maybe I could use this as bounding box, scaling it properly before drawing
                _linesCube = GeometricPrimitive.LinesCube.New(_legacyDevice, 128, 128, 128);

                // This sphere will be scaled up and down multiple times for using as In & Out of lights
                _sphere = GeometricPrimitive.Sphere.New(_legacyDevice, 1024, 6);

                //Little cubes and little spheres are used as mesh for lights, cameras, sinks, etc
                _littleCube = GeometricPrimitive.Cube.New(_legacyDevice, 2 * _littleSphereRadius);
                _littleSphere = GeometricPrimitive.Sphere.New(_legacyDevice, 2 * _littleCubeRadius, 8);

                _cone = GeometricPrimitive.Cone.New(_legacyDevice, 1024, 1024, 18);

                // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
                new BasicEffect(_legacyDevice);

                // Initialize the rasterizer state for wireframe drawing
                var renderStateDesc =
                    new SharpDX.Direct3D11.RasterizerStateDescription
                    {
                        CullMode = SharpDX.Direct3D11.CullMode.None,
                        DepthBias = 0,
                        DepthBiasClamp = 0,
                        FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
                        IsAntialiasedLineEnabled = true,
                        IsDepthClipEnabled = true,
                        IsFrontCounterClockwise = false,
                        IsMultisampleEnabled = true,
                        IsScissorEnabled = false,
                        SlopeScaledDepthBias = 0
                    };
                _rasterizerWireframe = RasterizerState.New(_legacyDevice, renderStateDesc);

                _rasterizerStateDepthBias = RasterizerState.New(_legacyDevice, new SharpDX.Direct3D11.RasterizerStateDescription
                {
                    CullMode = SharpDX.Direct3D11.CullMode.Back,
                    FillMode = SharpDX.Direct3D11.FillMode.Solid,
                    DepthBias = -2,
                    SlopeScaledDepthBias = -2
                });

                _gizmo = new Gizmo(device, DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"]);

                ResetCamera(true);
            }
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update FOV
            if (obj is Editor.ConfigurationChangedEvent)
                Camera.FieldOfView = ((Editor.ConfigurationChangedEvent)obj).Current.Rendering3D_FieldOfView * (float)(Math.PI / 180);

            // Move camera position with room movements
            if (obj is Editor.RoomGeometryChangedEvent && _editor.Mode == EditorMode.Map2D && _currentRoomLastPos.HasValue)
            {
                Camera.MoveCameraLinear(_editor.SelectedRoom.WorldPos - _currentRoomLastPos.Value);
                _currentRoomLastPos = _editor.SelectedRoom.WorldPos;
            }
            else if (obj is Editor.SelectedRoomChangedEvent || obj is Editor.ModeChangedEvent)
                _currentRoomLastPos = _editor.SelectedRoom.WorldPos;

            // Reset tool handler state
            if ( obj is Editor.ModeChangedEvent ||
                 obj is Editor.ToolChangedEvent ||
                (obj is Editor.SelectedRoomChangedEvent && _editor.Tool.Tool != EditorToolType.PortalDigger))
            {
                _toolHandler?.Disengage();
            }

            // Update rooms
            if (obj is IEditorRoomChangedEvent)
            {
                _renderingCachedRooms.Remove(((IEditorRoomChangedEvent)obj).Room);
                if (obj is Editor.RoomGeometryChangedEvent)
                    foreach (var portal in ((Editor.RoomGeometryChangedEvent)obj).Room.Portals)
                        _renderingCachedRooms.Remove(portal.AdjoiningRoom);
            }
            if (obj is Editor.ObjectChangedEvent)
            {
                var value = (Editor.ObjectChangedEvent)obj;
                if (value.ChangeType != ObjectChangeType.Remove && value.Object is LightInstance)
                    _renderingCachedRooms.Remove(((Editor.ObjectChangedEvent)obj).Object.Room);
            }
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
                obj is Editor.ConfigurationChangedEvent ||
                obj is SectorColoringManager.ChangeSectorColoringInfoEvent)
                _renderingCachedRooms.Clear();

            // Update drawing
            if (obj is IEditorObjectChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent ||
                obj is IEditorRoomChangedEvent ||
                obj is SectorColoringManager.ChangeSectorColoringInfoEvent ||
                obj is Editor.ConfigurationChangedEvent ||
                obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.HighlightedSectorChangedEvent ||
                obj is Editor.SelectedRoomChangedEvent ||
                obj is Editor.ModeChangedEvent ||
                obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.LoadedTexturesChangedEvent ||
                obj is Editor.LoadedImportedGeometriesChangedEvent)
            {
                if (_editor.Mode != EditorMode.Map2D)
                    Invalidate();
            }

            // Update cursor
            if (obj is Editor.ActionChangedEvent)
            {
                IEditorAction currentAction = ((Editor.ActionChangedEvent)obj).Current;
                bool hasCrossCursor = currentAction is EditorActionPlace || currentAction is EditorActionRelocateCamera;
                Cursor = hasCrossCursor ? Cursors.Cross : Cursors.Arrow;
            }

            // Center camera
            if (obj is Editor.ResetCameraEvent)
                ResetCamera(((Editor.ResetCameraEvent)obj).NewCamera);

            // Stop camera animation if level is changing
            if (obj is Editor.LevelChangedEvent)
                _movementTimer.Stop(true);

            // Move camera to sector
            if (obj is Editor.MoveCameraToSectorEvent)
            {
                var e = (Editor.MoveCameraToSectorEvent)obj;

                Vector3 center = _editor.SelectedRoom.GetLocalCenter();
                var nextPos = new Vector3(e.Sector.X * 1024.0f + 512.0f, center.Y, e.Sector.Y * 1024.0f + 512.0f) + _editor.SelectedRoom.WorldPos;

                if (_editor.Configuration.Rendering3D_AnimateCameraOnRelocation)
                    AnimateCamera(nextPos);
                else
                {
                    Camera.Target = nextPos;
                    Invalidate();
                }
            }
        }

        public void ResetCamera(bool forceNewCamera = false)
        {
            Room room = _editor?.SelectedRoom;

            // Point the camera to the room's center
            Vector3 target = new Vector3();
            if (room != null)
                target = room.WorldPos + room.GetLocalCenter();

            // Calculate camera distance
            Vector2 roomDiagonal = new Vector2(room?.NumXSectors ?? 0, room?.NumZSectors ?? 0);

            var dist = (roomDiagonal.Length() * 0.8f + 2.1f) * 1024.0f;
            var rotX = 0.6f;
            var rotY = (float)Math.PI;

            // Initialize a new camera
            if (Camera == null || forceNewCamera || !_editor.Configuration.Rendering3D_AnimateCameraOnReset)
            {
                Camera = new ArcBallCamera(target, rotX, rotY, -(float)Math.PI / 2, (float)Math.PI / 2, dist, 1, 1000000, _editor.Configuration.Rendering3D_FieldOfView * (float)(Math.PI / 180));
                Invalidate();
            }
            else
                AnimateCamera(target, new Vector2(rotX, rotY), dist);
        }

        public int TranslateCameraMouseMovement(Point value, bool horizontal = false)
        {
            if (Camera.RotationY < Math.PI * (1.0 / 4.0))
                return horizontal ? value.X : value.Y;
            else if (Camera.RotationY < Math.PI * (3.0 / 4.0))
                return horizontal ? value.Y : -value.X;
            else if (Camera.RotationY < Math.PI * (5.0 / 4.0))
                return horizontal ? -value.X : -value.Y;
            else if (Camera.RotationY < Math.PI * (7.0 / 4.0))
                return horizontal ? -value.Y : value.X;
            else
                return horizontal ? value.X : value.Y;
        }
        
        private void AnimateCamera(Vector3 oldPos, Vector3 newPos, Vector2 oldRot, Vector2 newRot, float oldDist, float newDist, float speed = 0.5f)
        {
            _nextCameraPos = newPos;
            _lastCameraPos = oldPos;
            _lastCameraRot = oldRot;
            _nextCameraRot = newRot;
            _lastCameraDist = oldDist;
            _nextCameraDist = newDist;

            _movementTimer.Animate(speed);
        }
        private void AnimateCamera(Vector3 newPos, Vector2 newRot, float newDist, float speed = 0.5f)
            => AnimateCamera(Camera.Target, newPos, new Vector2(Camera.RotationX, Camera.RotationY), newRot, Camera.Distance, newDist, speed);
        private void AnimateCamera(Vector3 newPos, float speed = 0.5f)
            => AnimateCamera(newPos, new Vector2(Camera.RotationX, Camera.RotationY), Camera.Distance, speed);

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if ((ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.None)
                _movementTimer.Engage(e.KeyCode);

            // This is here because we want to disable FlyMode immediately after pressing ESC and not after releasing it
            if (e.KeyCode == Keys.Escape && FlyMode)
                DisableFlyMode();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _movementTimer.Stop();

            if (FlyMode && e.KeyCode == Keys.Menu)
                e.Handled = true;

            if (e.KeyCode == ToggleFlyModeKeys && e.Modifiers == Keys.None && !FlyMode)
                EnableFlyMode();
            else if (e.KeyCode == ToggleFlyModeKeys && e.Modifiers == Keys.None && FlyMode)
                DisableFlyMode();
        }

        private void EnableFlyMode()
        {
            LastWindow = GetForegroundWindow();

            _oldCamera = Camera;
            Camera = new FreeCamera(_oldCamera.GetPosition(), _oldCamera.RotationX, _oldCamera.RotationY - (float)Math.PI,
                _oldCamera.MinRotationX, _oldCamera.MaxRotationX, _oldCamera.FieldOfView);

            Cursor.Hide();

            _flyModeTimer.Start();
            FlyMode = true;
        }

        private void DisableFlyMode()
        {
            Capture = false;

            var p = Camera.GetPosition();
            var d = Camera.GetDirection();
            var t = Camera.GetTarget();

            t = p + d * 1024.0f;

            _oldCamera.RotationX = Camera.RotationX;
            _oldCamera.RotationY = Camera.RotationY - (float)Math.PI;

            Camera = _oldCamera;
            Camera.Distance = 1024.0f;
            Camera.Position = p;
            Camera.Target = t;

            Cursor.Position = PointToScreen(new Point(Width / 2, Height / 2)); // Center cursor
            Cursor.Show();

            _flyModeTimer.Stop();
            FlyMode = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!_movementTimer.Animating)
            {
                Console.WriteLine("Delta: " + e.Delta);
                Camera.Zoom(-e.Delta * _editor.Configuration.Rendering3D_NavigationSpeedMouseWheelZoom);
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if(FlyMode)
                return; // Selecting in FlyMode is not allowed

            _lastMousePosition = e.Location;
            _doSectorSelection = false;
            _objectPlaced = false;

            //https://stackoverflow.com/questions/14191219/receive-mouse-move-even-cursor-is-outside-control
            Capture = true; // Capture mouse for zoom and panning

            if (e.Button == MouseButtons.Left)
            {
                // Do picking on the scene
                PickingResult newPicking = DoPicking(GetRay(e.X, e.Y), _editor.Configuration.Rendering3D_SelectObjectsInAnyRoom);

                if (newPicking is PickingResultBlock)
                {
                    PickingResultBlock newBlockPicking = (PickingResultBlock)newPicking;

                    _editor.SelectedObject = null;

                    // Ignore block picking if it's not from current room.
                    // Alternately, if room autoswitch is active, switch and select it.

                    if (newBlockPicking.Room != _editor.SelectedRoom)
                    {
                        if (_editor.Configuration.Rendering3D_AutoswitchCurrentRoom)
                            _editor.SelectedRoom = newBlockPicking.Room;
                        else
                            return;
                    }

                    // Move camera to selected sector
                    if (_editor.Action is EditorActionRelocateCamera)
                    {
                        _editor.MoveCameraToSector(newBlockPicking.Pos);
                        return;
                    }

                    // Place objects
                    if (_editor.Action is IEditorActionPlace)
                    {
                        var action = (IEditorActionPlace)_editor.Action;
                        EditorActions.PlaceObject(_editor.SelectedRoom, newBlockPicking.Pos, action.CreateInstance(_editor.Level, _editor.SelectedRoom));
                        _objectPlaced = true;
                        if (!action.ShouldBeActive)
                            _editor.Action = null;
                        return;
                    }

                    VectorInt2 pos = newBlockPicking.Pos;

                    // Handle face selection
                    if ((_editor.Tool.Tool == EditorToolType.Selection || _editor.Tool.Tool == EditorToolType.Group || _editor.Tool.Tool >= EditorToolType.Drag)
                         && ModifierKeys == Keys.None)
                    {
                        if (!_editor.SelectedSectors.Valid || !_editor.SelectedSectors.Area.Contains(pos))
                        {
                            // Select rectangle
                            if (ModifierKeys.HasFlag(Keys.Control))
                            {
                                // Multiple separate tile selection - To Be Implemented...
                                _editor.SelectedSectors = new SectorSelection { Start = pos, End = pos };
                            }
                            else
                                _editor.SelectedSectors = new SectorSelection { Start = pos, End = pos };
                            _doSectorSelection = true;
                            return;
                        }
                    }

                    // Act based on editor mode
                    bool belongsToFloor = newBlockPicking.BelongsToFloor;

                    switch (_editor.Mode)
                    {
                        case EditorMode.Geometry:
                            if (_editor.Tool.Tool != EditorToolType.Selection && _editor.Tool.Tool != EditorToolType.PortalDigger)
                            {
                                _toolHandler.Engage(e.X, e.Y, newBlockPicking);

                                if (_editor.Tool.Tool == EditorToolType.Brush || _editor.Tool.Tool == EditorToolType.Shovel)
                                    _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom.AndAdjoiningRooms);
                                else if (_editor.Tool.Tool < EditorToolType.Drag)
                                    _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

                                if (!ModifierKeys.HasFlag(Keys.Alt) && !ModifierKeys.HasFlag(Keys.Shift) && _toolHandler.Process(pos.X, pos.Y))
                                {
                                    if (_editor.Tool.Tool == EditorToolType.Smooth)
                                        EditorActions.SmoothSector(_editor.SelectedRoom, pos.X, pos.Y, belongsToFloor ? BlockVertical.Floor : BlockVertical.Ceiling);
                                    else if (_editor.Tool.Tool < EditorToolType.Flatten)
                                        EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                            new RectangleInt2(pos, pos),
                                            ArrowType.EntireFace,
                                            belongsToFloor ? BlockVertical.Floor : BlockVertical.Ceiling,
                                            (short)((_editor.Tool.Tool == EditorToolType.Shovel || _editor.Tool.Tool == EditorToolType.Pencil && ModifierKeys.HasFlag(Keys.Control)) ^ belongsToFloor ? 1 : -1),
                                            _editor.Tool.Tool == EditorToolType.Brush || _editor.Tool.Tool == EditorToolType.Shovel,
                                            false, false, true, true);
                                }
                            }
                            else if (_editor.Tool.Tool == EditorToolType.PortalDigger && _editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos))
                            {
                                Room newRoom = null;

                                if (newBlockPicking.IsVerticalPlane)
                                {
                                    newRoom = EditorActions.CreateAdjoiningRoom(_editor.SelectedRoom,
                                        _editor.SelectedSectors,
                                        PortalInstance.GetOppositeDirection(PortalInstance.GetDirection(BlockFaceExtensions.GetDirection(newBlockPicking.Face))),
                                        1, !ModifierKeys.HasFlag(Keys.Control));
                                }
                                else
                                {
                                    newRoom = EditorActions.CreateAdjoiningRoom(_editor.SelectedRoom,
                                        _editor.SelectedSectors,
                                        newBlockPicking.BelongsToFloor ? PortalDirection.Floor : PortalDirection.Ceiling,
                                        (short)(ModifierKeys.HasFlag(Keys.Shift) ? 1 : 4), !ModifierKeys.HasFlag(Keys.Control),
                                        ModifierKeys.HasFlag(Keys.Alt));
                                }

                                if (newRoom != null)
                                {
                                    if (!ModifierKeys.HasFlag(Keys.Control))
                                        _editor.HighlightedSectors = new SectorSelection() { Area = newRoom.LocalArea };
                                    _toolHandler.Engage(e.X, e.Y, newBlockPicking, false, newRoom);

                                    if (!ShowPortals && !ShowAllRooms)
                                        _editor.SendMessage("Parent is invisible. Turn on Draw Portals mode.", TombLib.Forms.PopupType.Info);
                                }
                                return;
                            }
                            break;

                        case EditorMode.Lighting:
                        case EditorMode.FaceEdit:
                            // Disable texturing in lighting mode, if option is set
                            if (_editor.Mode == EditorMode.Lighting &&
                                !_editor.Configuration.Rendering3D_AllowTexturingInLightingMode)
                                break;

                            // Do texturing
                            if (_editor.Tool.Tool != EditorToolType.Group && _editor.Tool.Tool != EditorToolType.Paint2x2)
                            {
                                if (ModifierKeys.HasFlag(Keys.Shift))
                                {
                                    EditorActions.RotateTexture(_editor.SelectedRoom, pos, newBlockPicking.Face, ModifierKeys.HasFlag(Keys.Control));
                                    break;
                                }
                                else if (ModifierKeys.HasFlag(Keys.Control))
                                {
                                    EditorActions.MirrorTexture(_editor.SelectedRoom, pos, newBlockPicking.Face);
                                    break;
                                }
                            }

                            if (ModifierKeys.HasFlag(Keys.Alt))
                            {
                                EditorActions.PickTexture(_editor.SelectedRoom, pos, newBlockPicking.Face);
                            }
                            else if (_editor.Tool.Tool == EditorToolType.Paint2x2)
                            {
                                EditorActions.TexturizeGroup(_editor.SelectedRoom,
                                    _editor.HighlightedSectors,
                                    _editor.SelectedSectors,
                                    _editor.SelectedTexture,
                                    newBlockPicking.Face,
                                    ModifierKeys.HasFlag(Keys.Control),
                                    !ModifierKeys.HasFlag(Keys.Shift));
                                _toolHandler.Engage(e.X, e.Y, newBlockPicking, false);
                            }
                            else if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos) || _editor.SelectedSectors.Empty)
                            {
                                switch (_editor.Tool.Tool)
                                {
                                    case EditorToolType.Fill:
                                        if (newBlockPicking.IsFloorHorizontalPlane)
                                            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Floor);
                                        else if (newBlockPicking.IsCeilingHorizontalPlane)
                                            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Ceiling);
                                        else if (newBlockPicking.IsVerticalPlane)
                                            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Wall);
                                        break;

                                    case EditorToolType.Group:
                                        if (_editor.SelectedSectors.Valid)
                                            EditorActions.TexturizeGroup(_editor.SelectedRoom,
                                                _editor.SelectedSectors,
                                                _editor.SelectedSectors,
                                                _editor.SelectedTexture,
                                                newBlockPicking.Face,
                                                ModifierKeys.HasFlag(Keys.Control),
                                                !ModifierKeys.HasFlag(Keys.Shift));
                                        break;

                                    case EditorToolType.Brush:
                                    case EditorToolType.Pencil:
                                        EditorActions.ApplyTexture(_editor.SelectedRoom, pos, newBlockPicking.Face, _editor.SelectedTexture);
                                        break;

                                    default:
                                        break;
                                }

                            }
                            break;
                    }
                }
                else if (newPicking is PickingResultGizmo)
                {
                    if (_editor.SelectedObject is PositionBasedObjectInstance)
                        _editor.UndoManager.PushObjectTransformed((PositionBasedObjectInstance)_editor.SelectedObject);

                    // Set gizmo axis
                    _gizmo.ActivateGizmo((PickingResultGizmo)newPicking);
                    _gizmoEnabled = true;
                }
                else if (newPicking is PickingResultObject)
                {
                    var obj = ((PickingResultObject)newPicking).ObjectInstance;

                    if (obj.Room != _editor.SelectedRoom && _editor.Configuration.Rendering3D_AutoswitchCurrentRoom)
                        _editor.SelectedRoom = obj.Room;

                    // Select or bookmark object
                    if (ModifierKeys.HasFlag(Keys.Alt) && obj is ItemInstance)
                        _editor.ChosenItem = ((ItemInstance)obj).ItemType;
                    else
                    {
                        _editor.SelectedObject = obj;
                        if (!(obj is ImportedGeometryInstance))
                            EditorActions.BookmarkObject(obj);
                    }
                }
                else if (newPicking == null)
                {
                    // Click outside room; if mouse is released without action, unselect all
                    _noSelectionConfirm = true;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                _startMousePosition = e.Location;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            _objectPlaced = false;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    PickingResult newPicking = DoPicking(GetRay(e.X, e.Y), true);
                    if (newPicking is PickingResultObject)
                    {
                        var pickedObject = ((PickingResultObject)newPicking).ObjectInstance;
                        EditorActions.EditObject(pickedObject, Parent);
                    }
                    else if (newPicking is PickingResultBlock)
                    {
                        var block = (PickingResultBlock)newPicking;
                        if (block.Room != _editor.SelectedRoom)
                        {
                            _editor.SelectedRoom = block.Room;
                            if (_editor.Configuration.Rendering3D_AnimateCameraOnDoubleClickRoomSwitch && (ModifierKeys == Keys.None))
                            {
                                Vector3 center = block.Room.GetLocalCenter();
                                var nextPos = new Vector3(block.Pos.X * 1024.0f + 512.0f, center.Y, block.Pos.Y * 1024.0f + 512.0f) + block.Room.WorldPos;
                                AnimateCamera(nextPos);
                            }
                        }
                    }
                    break;

                case MouseButtons.Right:
                    _editor.ResetCamera();
                    break;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!Focused)
                Focus(); // Enable keyboard interaction
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (FlyMode)
                return;

            bool redrawWindow = false;

            // Reset internal bool for deselection
            _noSelectionConfirm = false;

            // Hover effect on gizmo
            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(GetRay(e.X, e.Y))))
                redrawWindow = true;

            var pressedButton = e.Button;

            // Process action
            switch (pressedButton)
            {
                case MouseButtons.Middle:
                case MouseButtons.Right:
                    // Don't do anything while camera is animating!
                    if (_movementTimer.Animating)
                        break;

                    // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                    float relativeDeltaX = (e.X - _lastMousePosition.X) / (float)Height;
                    float relativeDeltaY = (e.Y - _lastMousePosition.Y) / (float)Height;
                    if (ModifierKeys.HasFlag(Keys.Shift) || e.Button == MouseButtons.Middle)
                        Camera.MoveCameraPlane(new Vector3(relativeDeltaX, relativeDeltaY, 0) *
                            _editor.Configuration.Rendering3D_NavigationSpeedMouseTranslate);
                    else if (ModifierKeys.HasFlag(Keys.Control))
                        Camera.Zoom((_editor.Configuration.Rendering3D_InvertMouseZoom ? relativeDeltaY : -relativeDeltaY) * _editor.Configuration.Rendering3D_NavigationSpeedMouseZoom);
                    else
                    {
                        if (_editor.Configuration.Rendering3D_CursorWarping)
                        {
                            if (e.X <= 0)
                                Cursor.Position = new Point(Cursor.Position.X + Width - 2, Cursor.Position.Y);
                            else if (e.X >= Width - 1)
                                Cursor.Position = new Point(Cursor.Position.X - Width + 2, Cursor.Position.Y);

                            if (e.Y <= 0)
                                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + Height - 2);
                            else if (e.Y >= Height - 1)
                                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - Height + 2);

                            if (e.X - _lastMousePosition.X >= (float)Width / 2 || e.X - _lastMousePosition.X <= -(float)Width / 2)
                                relativeDeltaX = 0;

                            if (e.Y - _lastMousePosition.Y >= (float)Height / 2 || e.Y - _lastMousePosition.Y <= -(float)Height / 2)
                                relativeDeltaY = 0;
                        }

                        Camera.Rotate(
                            relativeDeltaX * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                            -relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);
                    }

                    _gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), GetRay(e.X, e.Y)); // Update gizmo
                    redrawWindow = true;
                    break;

                case MouseButtons.Left:
                    if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), GetRay(e.X, e.Y)))
                    {
                        // Process gizmo
                        redrawWindow = true;
                    }
                    else if (_editor.Tool.Tool >= EditorToolType.Drag && _toolHandler.Engaged && !_doSectorSelection)
                    {
                        if (_editor.Tool.Tool != EditorToolType.PortalDigger && !_toolHandler.Dragged && _toolHandler.PositionDiffers(e.X, e.Y))
                            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

                        var dragValue = _toolHandler.UpdateDragState(e.X, e.Y,
                            _editor.Tool.Tool == EditorToolType.Drag || _editor.Tool.Tool == EditorToolType.PortalDigger, 
                            _editor.Tool.Tool != EditorToolType.PortalDigger);

                        if (dragValue.HasValue)
                        {
                            if (_editor.Tool.Tool == EditorToolType.PortalDigger)
                            {
                                VectorInt3 move = VectorInt3.Zero;
                                VectorInt2 drag = VectorInt2.Zero;
                                Point invertedDragValue = new Point(dragValue.Value.X, -dragValue.Value.Y);
                                var currRoom = _toolHandler.ReferenceRoom;
                                RectangleInt2 resizeArea = new RectangleInt2(currRoom.LocalArea.Start, currRoom.LocalArea.End);
                                short[] resizeHeight = { (short)currRoom.GetLowestCorner(), (short)currRoom.GetHighestCorner() };
                                PortalDirection portalDirection;
                                int verticalPrecision = ModifierKeys.HasFlag(Keys.Shift) ? 1 : 4;

                                if (_toolHandler.ReferencePicking.IsVerticalPlane)
                                    portalDirection = PortalInstance.GetOppositeDirection
                                                     (PortalInstance.GetDirection
                                                     (BlockFaceExtensions.GetDirection(_toolHandler.ReferencePicking.Face)));
                                else
                                {
                                    portalDirection = _toolHandler.ReferencePicking.BelongsToFloor ? PortalDirection.Floor : PortalDirection.Ceiling;
                                    move = new VectorInt3(0, dragValue.Value.Y * verticalPrecision, 0);
                                }

                                switch(portalDirection)
                                {
                                    case PortalDirection.Floor:
                                    case PortalDirection.Ceiling:
                                        var newHeight = (-dragValue.Value.Y * verticalPrecision);
                                        if (resizeHeight[1] - resizeHeight[0] + (portalDirection == PortalDirection.Floor ? newHeight : -newHeight) <= 0)
                                            return;  // Limit inward dragging
                                        resizeHeight[0] = (short)(portalDirection == PortalDirection.Floor ? 0 : newHeight);
                                        resizeHeight[1] = (short)(portalDirection == PortalDirection.Floor ? newHeight : 0);
                                        break;

                                    case PortalDirection.WallNegativeX:
                                    case PortalDirection.WallPositiveX:
                                        drag = new VectorInt2(TranslateCameraMouseMovement(invertedDragValue, true), 0);
                                        if (portalDirection == PortalDirection.WallNegativeX)
                                            resizeArea.Start += drag;
                                        else
                                            resizeArea.End += drag;
                                        break;

                                    case PortalDirection.WallNegativeZ:
                                    case PortalDirection.WallPositiveZ:
                                        drag = new VectorInt2(0, TranslateCameraMouseMovement(invertedDragValue, false));
                                        if (portalDirection == PortalDirection.WallNegativeZ)
                                            resizeArea.Start += drag;
                                        else
                                            resizeArea.End += drag;
                                        break;
                                }

                                // Only resize if any dimension is bigger than 3 and less than 32
                                if (resizeArea.Size.X > 1 && resizeArea.Size.Y > 1 && resizeArea.Size.X < 32 && resizeArea.Size.Y < 32)
                                {
                                    bool? operateOnFloor = null;
                                    if (_toolHandler.ReferencePicking.IsVerticalPlane) operateOnFloor = true;
                                    currRoom.Resize(_editor.Level, resizeArea, resizeHeight[0], resizeHeight[1], operateOnFloor);
                                    EditorActions.MoveRooms(move, currRoom.Versions, true);
                                    if (_toolHandler.ReferenceRoom == _editor.SelectedRoom)
                                        _editor.HighlightedSectors = new SectorSelection() { Area = _toolHandler.ReferenceRoom.LocalArea };
                                }
                            }
                            else if (_editor.SelectedSectors.Valid)
                            {
                                BlockVertical subdivisionToEdit = _toolHandler.ReferencePicking.BelongsToFloor ?
                                    (ModifierKeys.HasFlag(Keys.Control) ? BlockVertical.Ed : BlockVertical.Floor) :
                                    (ModifierKeys.HasFlag(Keys.Control) ? BlockVertical.Rf : BlockVertical.Ceiling);

                                switch (_editor.Tool.Tool)
                                {
                                    case EditorToolType.Drag:
                                        EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                            _editor.SelectedSectors.Area,
                                            _editor.SelectedSectors.Arrow,
                                            subdivisionToEdit,
                                            (short)Math.Sign(dragValue.Value.Y),
                                            ModifierKeys.HasFlag(Keys.Alt),
                                            _toolHandler.ReferenceIsOppositeDiagonalStep, true, true, true);
                                        break;
                                    case EditorToolType.Terrain:
                                        _toolHandler.DiscardEditedGeometry();
                                        EditorActions.ApplyHeightmap(_editor.SelectedRoom,
                                            _editor.SelectedSectors.Area,
                                            _editor.SelectedSectors.Arrow,
                                            subdivisionToEdit,
                                            _toolHandler.RandomHeightMap,
                                            dragValue.Value.Y,
                                            ModifierKeys.HasFlag(Keys.Shift),
                                            ModifierKeys.HasFlag(Keys.Alt));
                                        break;
                                    default:
                                        _toolHandler.DiscardEditedGeometry();
                                        EditorActions.ShapeGroup(_editor.SelectedRoom,
                                            _editor.SelectedSectors.Area,
                                            _editor.SelectedSectors.Arrow,
                                            _editor.Tool.Tool,
                                            subdivisionToEdit,
                                            dragValue.Value.Y,
                                            ModifierKeys.HasFlag(Keys.Shift),
                                            ModifierKeys.HasFlag(Keys.Alt));
                                        break;
                                }
                            }
                            redrawWindow = true;
                        }
                    }
                    else
                    {
                        PickingResultBlock newBlockPicking = DoPicking(GetRay(e.X, e.Y)) as PickingResultBlock;

                        if (newBlockPicking != null)
                        {
                            VectorInt2 pos = newBlockPicking.Pos;
                            bool belongsToFloor = newBlockPicking.BelongsToFloor;

                            if ((_editor.Tool.Tool == EditorToolType.Selection || _editor.Tool.Tool == EditorToolType.Group || _editor.Tool.Tool >= EditorToolType.Drag) && _doSectorSelection)
                            {
                                var newSelection = new SectorSelection
                                {
                                    Start = _editor.SelectedSectors.Start,
                                    End = new VectorInt2(pos.X, pos.Y)
                                };

                                if (_editor.SelectedSectors != newSelection)
                                {
                                    _editor.SelectedSectors = newSelection;
                                    redrawWindow = true;
                                }
                            }
                            else if (_editor.Mode == EditorMode.Geometry && _toolHandler.Engaged && !ModifierKeys.HasFlag(Keys.Alt | Keys.Shift))
                            {
                                if (!ModifierKeys.HasFlag(Keys.Alt) && !ModifierKeys.HasFlag(Keys.Shift) && _toolHandler.Process(pos.X, pos.Y))
                                {
                                    if (_editor.SelectedRoom.Blocks[pos.X, pos.Y].IsAnyWall == _toolHandler.ReferenceBlock.IsAnyWall)
                                    {
                                        switch (_editor.Tool.Tool)
                                        {
                                            case EditorToolType.Flatten:
                                                for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                                {
                                                    if (belongsToFloor && _toolHandler.ReferencePicking.BelongsToFloor)
                                                    {
                                                        _editor.SelectedRoom.Blocks[pos.X, pos.Y].Floor.SetHeight(edge, _toolHandler.ReferenceBlock.Floor.Min);
                                                        _editor.SelectedRoom.Blocks[pos.X, pos.Y].SetHeight(BlockVertical.Ed, edge, Math.Min(
                                                            Math.Min(_toolHandler.ReferenceBlock.GetHeight(BlockVertical.Ed, BlockEdge.XnZp), _toolHandler.ReferenceBlock.GetHeight(BlockVertical.Ed, BlockEdge.XpZp)),
                                                            Math.Min(_toolHandler.ReferenceBlock.GetHeight(BlockVertical.Ed, BlockEdge.XpZn), _toolHandler.ReferenceBlock.GetHeight(BlockVertical.Ed, BlockEdge.XnZn))));
                                                    }
                                                    else if (!belongsToFloor && !_toolHandler.ReferencePicking.BelongsToFloor)
                                                    {
                                                        _editor.SelectedRoom.Blocks[pos.X, pos.Y].Ceiling.SetHeight(edge, _toolHandler.ReferenceBlock.Ceiling.Min);
                                                        _editor.SelectedRoom.Blocks[pos.X, pos.Y].SetHeight(BlockVertical.Rf, edge, Math.Min(
                                                            Math.Min(_toolHandler.ReferenceBlock.GetHeight(BlockVertical.Rf, BlockEdge.XnZp), _toolHandler.ReferenceBlock.GetHeight(BlockVertical.Rf, BlockEdge.XpZp)),
                                                            Math.Min(_toolHandler.ReferenceBlock.GetHeight(BlockVertical.Rf, BlockEdge.XpZn), _toolHandler.ReferenceBlock.GetHeight(BlockVertical.Rf, BlockEdge.XnZn))));
                                                    }
                                                }
                                                EditorActions.SmartBuildGeometry(_editor.SelectedRoom, new RectangleInt2(pos, pos));
                                                break;

                                            case EditorToolType.Smooth:
                                                if (belongsToFloor != _toolHandler.ReferencePicking.BelongsToFloor)
                                                    break;

                                                EditorActions.SmoothSector(_editor.SelectedRoom, pos.X, pos.Y, belongsToFloor ? BlockVertical.Floor : BlockVertical.Ceiling, true);
                                                break;

                                            case EditorToolType.Drag:
                                            case EditorToolType.Terrain:
                                                break;

                                            default:
                                                if (belongsToFloor != _toolHandler.ReferencePicking.BelongsToFloor)
                                                    break;

                                                EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                                    new RectangleInt2(pos, pos),
                                                    ArrowType.EntireFace,
                                                    belongsToFloor ? BlockVertical.Floor : BlockVertical.Ceiling,
                                                    (short)((_editor.Tool.Tool == EditorToolType.Shovel || _editor.Tool.Tool == EditorToolType.Pencil && ModifierKeys.HasFlag(Keys.Control)) ^ belongsToFloor ? 1 : -1),
                                                    _editor.Tool.Tool == EditorToolType.Brush || _editor.Tool.Tool == EditorToolType.Shovel,
                                                    false, false, true, true);
                                                break;
                                        }
                                        redrawWindow = true;
                                    }
                                }
                            }
                            else
                            {
                                // Disable texturing in lighting mode, if option is set
                                if (_editor.Mode == EditorMode.Lighting &&
                                    !_editor.Configuration.Rendering3D_AllowTexturingInLightingMode)
                                    break;
                                else if ((_editor.Mode == EditorMode.FaceEdit || _editor.Mode == EditorMode.Lighting) && _editor.Action == null && ModifierKeys == Keys.None && !_objectPlaced)
                                {
                                    if (_editor.Tool.Tool == EditorToolType.Brush)
                                    {
                                        if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos) ||
                                            _editor.SelectedSectors.Empty)
                                            redrawWindow = EditorActions.ApplyTexture(_editor.SelectedRoom, pos, newBlockPicking.Face, _editor.SelectedTexture, true);
                                    }
                                    else if (_editor.Tool.Tool == EditorToolType.Paint2x2)
                                    {
                                        var point = new VectorInt2
                                            ((pos.X - ((pos.X + (_toolHandler.ReferencePicking.Pos.X % 2)) % 2)),
                                                (pos.Y - ((pos.Y + (_toolHandler.ReferencePicking.Pos.Y % 2)) % 2)));
                                        var newSelection = new SectorSelection { Start = point, End = point + VectorInt2.One };

                                        if (_editor.HighlightedSectors != newSelection)
                                        {
                                            _editor.HighlightedSectors = newSelection;
                                            EditorActions.TexturizeGroup(_editor.SelectedRoom,
                                                _editor.HighlightedSectors,
                                                _editor.SelectedSectors,
                                                _editor.SelectedTexture,
                                                _toolHandler.ReferencePicking.Face,
                                                ModifierKeys.HasFlag(Keys.Control),
                                                !ModifierKeys.HasFlag(Keys.Shift),
                                                true);
                                            redrawWindow = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    if (_editor.Tool.Tool == EditorToolType.Paint2x2)
                    {
                        // Disable highlight in lighting mode, if option is set
                        if (_editor.Mode == EditorMode.Lighting &&
                            !_editor.Configuration.Rendering3D_AllowTexturingInLightingMode)
                            break;

                        PickingResultBlock newBlockPicking = DoPicking(GetRay(e.X, e.Y)) as PickingResultBlock;
                        if (newBlockPicking != null)
                        {
                            VectorInt2 pos = newBlockPicking.Pos;
                            var newSelection = new SectorSelection
                            {
                                Start = new VectorInt2(pos.X, pos.Y),
                                  End = new VectorInt2(pos.X, pos.Y) + VectorInt2.One
                            };

                            if (_editor.HighlightedSectors != newSelection)
                            {
                                _editor.HighlightedSectors = newSelection;
                                redrawWindow = true;
                            }
                        }
                        else
                            if (_editor.HighlightedSectors != SectorSelection.None)
                            _editor.HighlightedSectors = SectorSelection.None;
                    }
                    break;
            }

            if (redrawWindow)
                Invalidate();

            _lastMousePosition = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_editor.Mode == EditorMode.Geometry && !_gizmoEnabled && !_objectPlaced)
                    {
                        PickingResultBlock newBlockPicking = DoPicking(GetRay(e.X, e.Y)) as PickingResultBlock;
                        if (newBlockPicking != null && !_toolHandler.Dragged)
                        {
                            var pos = newBlockPicking.Pos;
                            var zone = _editor.SelectedSectors.Empty ? new RectangleInt2(pos, pos) : _editor.SelectedSectors.Area;
                            bool belongsToFloor = newBlockPicking.BelongsToFloor;

                            if (ModifierKeys.HasFlag(Keys.Alt) && zone.Contains(pos))
                            {
                                // Split the faces
                                if (belongsToFloor)
                                    EditorActions.FlipFloorSplit(_editor.SelectedRoom, zone);
                                else
                                    EditorActions.FlipCeilingSplit(_editor.SelectedRoom, zone);
                                return;
                            }
                            else if (ModifierKeys.HasFlag(Keys.Shift) && zone.Contains(pos))
                            {
                                // Rotate sector
                                EditorActions.RotateSectors(_editor.SelectedRoom, zone, belongsToFloor);
                                return;
                            }
                            else if (_editor.Tool.Tool == EditorToolType.Selection || (_editor.Tool.Tool >= EditorToolType.Drag && _editor.Tool.Tool < EditorToolType.PortalDigger))
                                if (!_doSectorSelection && _editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos))
                                    // Rotate the arrows
                                    if (ModifierKeys.HasFlag(Keys.Control))
                                    {
                                        if (_editor.SelectedSectors.Arrow == ArrowType.CornerSW)
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EntireFace);
                                        else if (_editor.SelectedSectors.Arrow == ArrowType.CornerSE)
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerSW);
                                        else if (_editor.SelectedSectors.Arrow == ArrowType.CornerNE)
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerSE);
                                        else if (_editor.SelectedSectors.Arrow == ArrowType.CornerNW)
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerNE);
                                        else
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerNW);
                                    }
                                    else
                                    {
                                        if (_editor.SelectedSectors.Arrow == ArrowType.EdgeW)
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EntireFace);
                                        else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeS)
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeW);
                                        else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeE)
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeS);
                                        else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeN)
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeE);
                                        else
                                            _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeN);
                                    }
                        }
                    }

                    break;

                case MouseButtons.Right:
                    var distance = new Vector2(_startMousePosition.X, _startMousePosition.Y) - new Vector2(e.Location.X, e.Location.Y);
                    if (distance.Length() < 4.0f)
                    {
                        _currentContextMenu?.Dispose();
                        _currentContextMenu = null;

                        PickingResult newPicking = DoPicking(GetRay(e.X, e.Y), true);
                        if (newPicking is PickingResultObject)
                        {
                            ObjectInstance target = ((PickingResultObject)newPicking).ObjectInstance;
                            if (target is PositionBasedObjectInstance)
                                _currentContextMenu = new PositionBasedObjectContextMenu(_editor, this, (PositionBasedObjectInstance)target);
                        }
                        else if (newPicking is PickingResultBlock)
                        {
                            var pickedBlock = newPicking as PickingResultBlock;
                            if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pickedBlock.Pos))
                                _currentContextMenu = new SelectedGeometryContextMenu(_editor, this, pickedBlock.Room, _editor.SelectedSectors.Area, pickedBlock.Pos);
                            else
                                _currentContextMenu = new BlockContextMenu(_editor, this, pickedBlock.Room, pickedBlock.Pos);
                        }
                        _currentContextMenu?.Show(PointToScreen(e.Location));
                    }
                    break;
            }

            // Click outside room
            if (_noSelectionConfirm)
            {
                _editor.SelectedSectors = SectorSelection.None;
                _editor.SelectedObject = null;
                _noSelectionConfirm = false;    // It gets already set on MouseMove, but it's better to prevent obscure errors and unwanted behavior later on
            }

            _toolHandler.Disengage();
            _doSectorSelection = false;
            _gizmoEnabled = false;
            if (_gizmo.MouseUp())
                Invalidate();
            Capture = false;
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _movementTimer.Stop();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            if (e.Data.GetDataPresent(typeof(ItemType)))
                e.Effect = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(typeof(DarkFloatingToolboxContainer)))
            {
                e.Effect = DragDropEffects.Move;
                Parent.Cursor = Cursors.Arrow;
            }
            else if (EditorActions.DragDropFileSupported(e, true))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            // Check if we are done with all common file tasks
            var filesToProcess = EditorActions.DragDropCommonFiles(e, FindForm());
            if (filesToProcess == 0)
                return;

            // Now try to put data on pointed sector
            Point loc = PointToClient(new Point(e.X, e.Y));
            PickingResult newPicking = DoPicking(GetRay(loc.X, loc.Y), _editor.Configuration.Rendering3D_AutoswitchCurrentRoom);

            if (newPicking is PickingResultBlock)
            {
                var newBlockPicking = (PickingResultBlock)newPicking;

                // Switch room if needed
                if (newBlockPicking.Room != _editor.SelectedRoom)
                    _editor.SelectedRoom = newBlockPicking.Room;

                if (e.Data.GetDataPresent(typeof(ItemType)))
                {
                    // Put item from object browser
                    EditorActions.PlaceObject(_editor.SelectedRoom,
                        newBlockPicking.Pos,
                        ItemInstance.FromItemType((ItemType)e.Data.GetData(typeof(ItemType))));
                }
                else if (filesToProcess != -1)
                {
                    // Try to put custom geometry files, if any
                    List<string> files = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList();

                    foreach (var file in files)
                    {
                        if (!ImportedGeometry.FileExtensions.Matches(file))
                            continue;

                        ImportedGeometry geometryToDrop = _editor.Level.Settings.ImportedGeometries.Find(
                            item => _editor.Level.Settings.MakeAbsolute(item.Info.Path).Equals(file, StringComparison.InvariantCultureIgnoreCase));
                        if (geometryToDrop == null)
                        {
                            var info = ImportedGeometryInfo.Default;
                            info.Path = _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory);
                            info.Name = Path.GetFileNameWithoutExtension(file);

                            geometryToDrop = new ImportedGeometry();
                            _editor.Level.Settings.ImportedGeometryUpdate(geometryToDrop, info);
                            _editor.Level.Settings.ImportedGeometries.Add(geometryToDrop);
                            _editor.LoadedImportedGeometriesChange();
                        }

                        EditorActions.PlaceObject(_editor.SelectedRoom, newBlockPicking.Pos,
                            new ImportedGeometryInstance { Model = geometryToDrop });
                    }
                }
            }
        }

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (_movementTimer.Animating)
            {
                var lerpedRot = Vector2.Lerp(_lastCameraRot, _nextCameraRot, _movementTimer.MoveMultiplier);
                Camera.Target = Vector3.Lerp(_lastCameraPos, _nextCameraPos, _movementTimer.MoveMultiplier);
                Camera.RotationX = lerpedRot.X;
                Camera.RotationY = lerpedRot.Y;
                Camera.Distance = (float)MathC.Lerp(_lastCameraDist, _nextCameraDist, _movementTimer.MoveMultiplier);
                Invalidate();
            }
            else
            {
                switch (_movementTimer.MoveKey)
                {
                    case Keys.Up:
                        Camera.Rotate(0, -_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.Down:
                        Camera.Rotate(0, _editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.Left:
                        Camera.Rotate(_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier, 0);
                        Invalidate();
                        break;

                    case Keys.Right:
                        Camera.Rotate(-_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier, 0);
                        Invalidate();
                        break;

                    case Keys.PageUp:
                        Camera.Zoom(-_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.PageDown:
                        Camera.Zoom(_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;
                }
            }
        }

        private void FlyModeTimer_Tick(object sender, EventArgs e)
        {
            if (LastWindow != GetForegroundWindow())
            {
                DisableFlyMode();
                LastWindow = GetForegroundWindow();
                return;
            }

            Capture = true;

            /* Camera position handling */
            var newCameraPos = new Vector3();
            var cameraMoveSpeed = 10 * _editor.Configuration.Rendering3D_FlyModeMoveSpeed;

            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
                cameraMoveSpeed *= 2;
            else if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt))
                cameraMoveSpeed /= 2;

            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.W))
                newCameraPos.Z -= cameraMoveSpeed;

            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.A))
                newCameraPos.X += cameraMoveSpeed;

            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.S))
                newCameraPos.Z += cameraMoveSpeed;

            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.D))
                newCameraPos.X -= cameraMoveSpeed;

            Camera.MoveCameraPlane(newCameraPos);

            var room = GetCurrentRoom();

            if (room != null)
                _editor.SelectedRoom = room;

            /* Camera rotation handling */
            var cursorPos = PointToClient(Cursor.Position);

            float relativeDeltaX = (cursorPos.X - _lastMousePosition.X) / (float)Height;
            float relativeDeltaY = (cursorPos.Y - _lastMousePosition.Y) / (float)Height;

            if (cursorPos.X <= 0)
                Cursor.Position = new Point(Cursor.Position.X + Width - 2, Cursor.Position.Y);
            else if (cursorPos.X >= Width - 1)
                Cursor.Position = new Point(Cursor.Position.X - Width + 2, Cursor.Position.Y);

            if (cursorPos.Y <= 0)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + Height - 2);
            else if (cursorPos.Y >= Height - 1)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - Height + 2);

            if (cursorPos.X - _lastMousePosition.X >= (float)Width / 2 || cursorPos.X - _lastMousePosition.X <= -(float)Width / 2)
                relativeDeltaX = 0;

            if (cursorPos.Y - _lastMousePosition.Y >= (float)Height / 2 || cursorPos.Y - _lastMousePosition.Y <= -(float)Height / 2)
                relativeDeltaY = 0;

            Camera.Rotate(
                relativeDeltaX * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                -relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);

            _gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), GetRay(cursorPos.X, cursorPos.Y));

            Invalidate();

            _lastMousePosition = cursorPos;
        }

        private static float TransformRayDistance(ref Ray sourceRay, ref Matrix4x4 transform, ref Ray destinationRay, float sourceDistance)
        {
            Vector3 sourcePos = sourceRay.Position + sourceDistance * sourceRay.Direction;
            Vector3 destinationPos = MathC.HomogenousTransform(sourcePos, transform);
            float destinationDistance = (destinationPos - destinationRay.Position).Length();
            return destinationDistance;
        }

        private void DoMeshPicking<T>(ref PickingResult result, Ray ray, ObjectInstance objectPtr, Mesh<T> mesh, Matrix4x4 objectMatrix) where T : struct, IVertex
        {
            // Transform view ray to object space space
            Matrix4x4 inverseObjectMatrix;
            if (!Matrix4x4.Invert(objectMatrix, out inverseObjectMatrix))
                return;
            Vector3 transformedRayPos = MathC.HomogenousTransform(ray.Position, inverseObjectMatrix);
            Vector3 transformedRayDestination = MathC.HomogenousTransform(ray.Position + ray.Direction, inverseObjectMatrix);
            Ray transformedRay = new Ray(transformedRayPos, transformedRayDestination - transformedRayPos);
            transformedRay.Direction = Vector3.Normalize(transformedRay.Direction);

            // Do a fast bounding box check
            float minDistance;
            {
                BoundingBox box = mesh.BoundingBox;
                float distance;
                if (!Collision.RayIntersectsBox(transformedRay, box, out distance))
                    return;

                minDistance = result == null ? float.PositiveInfinity : TransformRayDistance(ref ray, ref inverseObjectMatrix, ref transformedRay, result.Distance);
                if (!(distance < minDistance))
                    return;
            }

            // Now do a ray - triangle intersection test
            bool hit = false;
            foreach (var submesh in mesh.Submeshes)
                for (int k = 0; k < submesh.Value.Indices.Count; k += 3)
                {
                    Vector3 p1 = mesh.Vertices[submesh.Value.Indices[k]].Position;
                    Vector3 p2 = mesh.Vertices[submesh.Value.Indices[k + 1]].Position;
                    Vector3 p3 = mesh.Vertices[submesh.Value.Indices[k + 2]].Position;

                    float distance;
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }
                }

            if (hit)
                result = new PickingResultObject(TransformRayDistance(ref transformedRay, ref objectMatrix, ref ray, minDistance), objectPtr);
        }

        private PickingResult DoPicking(Ray ray, bool pickAnyRoom = false)
        {
            PickingResult result;

            // The gizmo has the priority because it always drawn on top
            result = _gizmo.DoPicking(ray);
            if (result != null)
                return result;

            List<Room> rooms = pickAnyRoom ? CollectRoomsToDraw(_editor.SelectedRoom) : new List<Room> { _editor.SelectedRoom };
            float distance;

            foreach (var room in rooms)
            {
                // First check for all objects in the room
                foreach (var instance in room.Objects)
                    if (instance is MoveableInstance && ShowMoveables)
                    {
                        MoveableInstance modelInfo = (MoveableInstance)instance;
                        WadMoveable moveable = _editor?.Level?.Settings?.WadTryGetMoveable(modelInfo.WadObjectId);
                        if (moveable != null)
                        {
                            // TODO Make picking independent of the rendering data.
                            AnimatedModel model = _wadRenderer.GetMoveable(moveable);
                            for (int j = 0; j < model.Meshes.Count; j++)
                            {
                                var mesh = model.Meshes[j];
                                DoMeshPicking(ref result, ray, instance, mesh, model.AnimationTransforms[j] * instance.ObjectMatrix);
                            }
                        }
                        else
                        {
                            BoundingBox box = new BoundingBox(
                                room.WorldPos + modelInfo.Position - new Vector3(_littleCubeRadius),
                                room.WorldPos + modelInfo.Position + new Vector3(_littleCubeRadius));
                            if (Collision.RayIntersectsBox(ray, box, out distance) && (result == null || distance < result.Distance))
                                result = new PickingResultObject(distance, instance);
                        }
                    }
                    else if (instance is StaticInstance && ShowStatics)
                    {
                        StaticInstance modelInfo = (StaticInstance)instance;
                        WadStatic @static = _editor?.Level?.Settings?.WadTryGetStatic(modelInfo.WadObjectId);
                        if (@static != null)
                        {
                            // TODO Make picking independent of the rendering data.
                            StaticModel model = _wadRenderer.GetStatic(@static);
                            var mesh = model.Meshes[0];
                            DoMeshPicking(ref result, ray, instance, mesh, instance.ObjectMatrix);
                        }
                        else
                        {
                            BoundingBox box = new BoundingBox(
                                room.WorldPos + modelInfo.Position - new Vector3(_littleCubeRadius),
                                room.WorldPos + modelInfo.Position + new Vector3(_littleCubeRadius));
                            if (Collision.RayIntersectsBox(ray, box, out distance) && (result == null || distance < result.Distance))
                                result = new PickingResultObject(distance, instance);
                        }
                    }
                    else if (instance is ImportedGeometryInstance && ShowImportedGeometry && !DisablePickingForImportedGeometry)
                    {
                        var geometry = (ImportedGeometryInstance)instance;

                        bool testedMesh = false;
                        foreach (ImportedGeometryMesh mesh in geometry?.Model?.DirectXModel?.Meshes ?? Enumerable.Empty<ImportedGeometryMesh>())
                            if (geometry.MeshNameMatchesFilter(mesh.Name))
                            {
                                DoMeshPicking(ref result, ray, instance, mesh, geometry.ObjectMatrix);
                                testedMesh = true;
                            }
                        if (!testedMesh)
                        {
                            BoundingBox box = new BoundingBox(
                                room.WorldPos + geometry.Position - new Vector3(_littleCubeRadius),
                                room.WorldPos + geometry.Position + new Vector3(_littleCubeRadius));
                            if (Collision.RayIntersectsBox(ray, box, out distance) && (result == null || distance < result.Distance))
                                result = new PickingResultObject(distance, instance);
                        }
                    }
                    else if (ShowOtherObjects)
                    {
                        if (instance is LightInstance)
                        {
                            BoundingSphere sphere = new BoundingSphere(room.WorldPos + instance.Position, _littleSphereRadius);
                            if (Collision.RayIntersectsSphere(ray, sphere, out distance) && (result == null || distance < result.Distance))
                                result = new PickingResultObject(distance, instance);
                        }
                        else
                        {
                            BoundingBox box = new BoundingBox(
                                room.WorldPos + instance.Position - new Vector3(_littleCubeRadius),
                                room.WorldPos + instance.Position + new Vector3(_littleCubeRadius));
                            if (Collision.RayIntersectsBox(ray, box, out distance) && (result == null || distance < result.Distance))
                                result = new PickingResultObject(distance, instance);
                        }
                    }

                // Check room geometry
                var roomIntersectInfo = room.RoomGeometry?.RayIntersectsGeometry(new Ray(ray.Position - room.WorldPos, ray.Direction));
                if (roomIntersectInfo != null && (result == null || roomIntersectInfo.Value.Distance < result.Distance))
                    result = new PickingResultBlock(roomIntersectInfo.Value.Distance, roomIntersectInfo.Value.VerticalCoord, roomIntersectInfo.Value.Pos, room, roomIntersectInfo.Value.Face);
            }

            return result;
        }

        private Ray GetRay(float x, float y)
        {
            return Ray.GetPickRay(new Vector2(x, y), Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ClientSize.Width, ClientSize.Height);
        }

        private void DrawDebugLines(Matrix4x4 viewProjection)
        {
            var drawRoomBounds = _editor.Configuration.Rendering3D_AlwaysShowCurrentRoomBounds || 
                ((_editor.Mode == EditorMode.FaceEdit || _editor.Mode == EditorMode.Lighting) && (ShowPortals || ShowAllRooms));

            if (!_drawFlybyPath && !_drawHeightLine && !drawRoomBounds)
                return;

            _legacyDevice.SetRasterizerState(_rasterizerWireframe);
            Effect solidEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"];
            Matrix4x4 model = Matrix4x4.CreateTranslation(_editor.SelectedRoom.WorldPos);
            solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
            solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

            if (_drawHeightLine)
            {
                _legacyDevice.SetVertexBuffer(_objectHeightLineVertexBuffer);
                _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _objectHeightLineVertexBuffer));
                Matrix4x4 model2 = Matrix4x4.CreateTranslation(_editor.SelectedObject.Room.WorldPos);
                solidEffect.Parameters["ModelViewProjection"].SetValue((model2 * viewProjection).ToSharpDX());
                solidEffect.CurrentTechnique.Passes[0].Apply();
                _legacyDevice.Draw(PrimitiveType.LineList, 2);
            }

            if (_drawFlybyPath)
            {
                _legacyDevice.SetVertexBuffer(_flybyPathVertexBuffer);
                _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _flybyPathVertexBuffer));
                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                solidEffect.CurrentTechnique.Passes[0].Apply();
                _legacyDevice.Draw(PrimitiveType.LineList, _flybyPathVertexBuffer.ElementCount);
            }

            if (drawRoomBounds)
            {
                // Draw room bounding box
                _legacyDevice.SetVertexBuffer(_linesCube.VertexBuffer);
                _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _linesCube.VertexBuffer));
                _legacyDevice.SetIndexBuffer(_linesCube.IndexBuffer, false);

                float height = _editor.SelectedRoom.GetHighestCorner() - _editor.SelectedRoom.GetLowestCorner();
                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(_editor.SelectedRoom.NumXSectors * 4.0f, height, _editor.SelectedRoom.NumZSectors * 4.0f);
                float boxX = _editor.SelectedRoom.WorldPos.X + (_editor.SelectedRoom.NumXSectors * 1024.0f) / 2.0f;
                float boxY = _editor.SelectedRoom.WorldPos.Y + (_editor.SelectedRoom.GetHighestCorner() + _editor.SelectedRoom.GetLowestCorner()) * 256.0f / 2.0f;
                float boxZ = _editor.SelectedRoom.WorldPos.Z + (_editor.SelectedRoom.NumZSectors * 1024.0f) / 2.0f;
                Matrix4x4 translateMatrix = Matrix4x4.CreateTranslation(new Vector3(boxX, boxY, boxZ));
                solidEffect.Parameters["ModelViewProjection"].SetValue((scaleMatrix * translateMatrix * viewProjection).ToSharpDX());
                solidEffect.CurrentTechnique.Passes[0].Apply();
                _legacyDevice.DrawIndexed(PrimitiveType.LineList, _linesCube.IndexBuffer.ElementCount);
            }
        }

        private string BuildTriggeredByMessage(ObjectInstance instance)
        {
            string message = "";
            foreach (var room in _editor.Level.Rooms.Where(room => room != null))
                foreach (var trigger in room.Triggers)
                    if (trigger.Target == instance || trigger.Timer == instance || trigger.Extra == instance)
                        message += "\nTriggered in Room " + trigger.Room + " on sectors " + trigger.Area;
            return message;
        }

        private void DrawLights(Matrix4x4 viewProjection, Room[] roomsWhoseObjectsToDraw, List<Text> textToDraw)
        {
            _legacyDevice.SetRasterizerState(_rasterizerWireframe);
            _legacyDevice.SetVertexBuffer(_littleSphere.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleSphere.VertexBuffer));
            _legacyDevice.SetIndexBuffer(_littleSphere.IndexBuffer, _littleSphere.IsIndex32Bits);

            Effect solidEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"];

            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var light in room.Objects.OfType<LightInstance>())
                {
                    solidEffect.Parameters["ModelViewProjection"].SetValue((light.ObjectMatrix * viewProjection).ToSharpDX());

                    if (light.Type == LightType.Point)
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 0.25f, 1.0f));
                    if (light.Type == LightType.Spot)
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 0.25f, 1.0f));
                    if (light.Type == LightType.FogBulb)
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 0.0f, 1.0f, 1.0f));
                    if (light.Type == LightType.Shadow)
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    if (light.Type == LightType.Effect)
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 0.25f, 1.0f));
                    if (light.Type == LightType.Sun)
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 0.5f, 0.0f, 1.0f));

                    if (_editor.SelectedObject == light)
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 0.2f, 0.2f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
                }

            if (Array.IndexOf(roomsWhoseObjectsToDraw, _editor.SelectedObject?.Room) != -1 && _editor.SelectedObject is LightInstance)
            {
                LightInstance light = (LightInstance)_editor.SelectedObject;

                if (light.Type == LightType.Point || light.Type == LightType.Shadow || light.Type == LightType.FogBulb)
                {
                    _legacyDevice.SetVertexBuffer(_sphere.VertexBuffer);
                    _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
                    _legacyDevice.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);

                    Matrix4x4 model;

                    if (light.Type == LightType.Point || light.Type == LightType.Shadow)
                    {
                        model = Matrix4x4.CreateScale(light.InnerRange * 2.0f) * light.ObjectMatrix;
                        solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                        solidEffect.CurrentTechnique.Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                    }

                    model = Matrix4x4.CreateScale(light.OuterRange * 2.0f) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                }
                else if (light.Type == LightType.Spot)
                {
                    _legacyDevice.SetVertexBuffer(_cone.VertexBuffer);
                    _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _legacyDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    // Inner cone
                    float coneAngle = (float)Math.Atan2(512, 1024);
                    float lenScaleH = light.InnerRange;
                    float lenScaleW = light.InnerAngle * (float)(Math.PI / 180) / coneAngle * lenScaleH;

                    Matrix4x4 Model = Matrix4x4.CreateScale(lenScaleW, lenScaleW, lenScaleH) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue((Model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);

                    // Outer cone
                    float cutoffScaleH = light.OuterRange;
                    float cutoffScaleW = light.OuterAngle * (float)(Math.PI / 180) / coneAngle * cutoffScaleH;

                    Matrix4x4 model2 = Matrix4x4.CreateScale(cutoffScaleW, cutoffScaleW, cutoffScaleH) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model2 * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }
                else if (light.Type == LightType.Sun)
                {
                    _legacyDevice.SetVertexBuffer(_cone.VertexBuffer);
                    _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _legacyDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    Matrix4x4 model = Matrix4x4.CreateScale(0.01f, 0.01f, 1.0f) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                // Add text message
                textToDraw.Add(CreateTextTagForObject(
                    light.ObjectMatrix * viewProjection,
                    light.Type + " Light" + "\n" + GetObjectPositionString(light.Room, light)));

                // Add the line height of the object
                AddObjectHeightLine(light.Room, light.Position);
            }

            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
        }

        private void DrawObjects(Matrix4x4 viewProjection, Room[] roomsWhoseObjectsToDraw, List<Text> textToDraw)
        {
            Effect effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"];

            _legacyDevice.SetVertexBuffer(_littleCube.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleCube.VertexBuffer));
            _legacyDevice.SetIndexBuffer(_littleCube.IndexBuffer, _littleCube.IsIndex32Bits);

            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<CameraInstance>())
                {
                    _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
                    var color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                        _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                        // Add text message
                        textToDraw.Add(CreateTextTagForObject(
                            instance.RotationPositionMatrix * viewProjection,
                            "Camera " + (instance.Fixed ? "(Fixed)" : "") +
                                " [ID = " + (instance.ScriptId?.ToString() ?? "<None>") + "]" +
                                "\n" + GetObjectPositionString(room, instance) + BuildTriggeredByMessage(instance)));

                        // Add the line height of the object
                        AddObjectHeightLine(room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }

            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
                {
                    _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                    Vector4 color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                        _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                        // Add text message
                        textToDraw.Add(CreateTextTagForObject(
                            instance.RotationPositionMatrix * viewProjection,
                            "Flyby camera (" + instance.Sequence + ":" + instance.Number + ") " +
                                "[ID = " + (instance.ScriptId?.ToString() ?? "<None>") + "]" +
                                "\n" + GetObjectPositionString(room, instance) + BuildTriggeredByMessage(instance)));

                        // Add the line height of the object
                        AddObjectHeightLine(room, instance.Position);

                        // Add the path of the flyby
                        AddFlybyPath(instance.Sequence);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }


            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<SinkInstance>())
                {
                    _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                    Vector4 color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                        _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                        // Add text message
                        textToDraw.Add(CreateTextTagForObject(
                            instance.RotationPositionMatrix * viewProjection,
                            "Sink[ID = " + (instance.ScriptId?.ToString() ?? " < None > ") + "]" +
                                "\n" + GetObjectPositionString(room, instance) + BuildTriggeredByMessage(instance)));

                        // Add the line height of the object
                        AddObjectHeightLine(room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }


            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<SoundSourceInstance>())
                {
                    _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                    Vector4 color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                        _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                        // Add text message
                        textToDraw.Add(CreateTextTagForObject(
                            instance.RotationPositionMatrix * viewProjection,
                            "Sound source [ID = " + (instance.ScriptId?.ToString() ?? "<None>") +
                                "](" + instance.SoundNameToDisplay + ")\n" + GetObjectPositionString(room, instance)));

                        // Add the line height of the object
                        AddObjectHeightLine(room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }

            if (_editor.SelectedRoom != null)
            {
                foreach (Room room in roomsWhoseObjectsToDraw)
                    foreach (var instance in room.Objects.OfType<MoveableInstance>())
                    {
                        if (_editor?.Level?.Settings?.WadTryGetMoveable(instance.WadObjectId) != null)
                            continue;
                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        Vector4 color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                        if (_editor.SelectedObject == instance)
                        {
                            color = new Vector4(1.0f, 0.4f, 0.4f, 1.0f);
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            // Add text message
                            textToDraw.Add(CreateTextTagForObject(
                                instance.RotationPositionMatrix * viewProjection,
                                instance + "\nUnavailable " + instance.ItemType +
                                    "\n" + GetObjectPositionString(room, instance) + BuildTriggeredByMessage(instance)));

                            // Add the line height of the object
                            AddObjectHeightLine(room, instance.Position);
                        }

                        effect.Parameters["ModelViewProjection"].SetValue((instance.RotationPositionMatrix * viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(color);

                        effect.Techniques[0].Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                    }

                foreach (Room room in roomsWhoseObjectsToDraw)
                    foreach (var instance in room.Objects.OfType<StaticInstance>())
                    {
                        if (_editor?.Level?.Settings?.WadTryGetStatic(instance.WadObjectId) != null)
                            continue;

                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        Vector4 color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                        if (_editor.SelectedObject == instance)
                        {
                            color = new Vector4(1.0f, 0.4f, 0.4f, 1.0f);
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            // Add text message
                            textToDraw.Add(CreateTextTagForObject(
                                instance.RotationPositionMatrix * viewProjection,
                                instance + "\nUnavailable " + instance.ItemType + BuildTriggeredByMessage(instance)));

                            // Add the line height of the object
                            AddObjectHeightLine(room, instance.Position);
                        }

                        effect.Parameters["ModelViewProjection"].SetValue((instance.RotationPositionMatrix * viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(color);

                        effect.Techniques[0].Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                    }

                foreach (Room room in roomsWhoseObjectsToDraw)
                    foreach (var instance in room.Objects.OfType<ImportedGeometryInstance>())
                    {
                        if (instance.Model?.DirectXModel != null)
                            if (instance.Model.DirectXModel.Meshes.Any(mesh => instance.MeshNameMatchesFilter(mesh.Name)))
                                continue;

                        Vector4 color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                        if (_editor.SelectedObject == instance)
                        {
                            color = new Vector4(1.0f, 0.4f, 0.4f, 1.0f);
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            // Add text message
                            textToDraw.Add(CreateTextTagForObject(
                                instance.RotationPositionMatrix * viewProjection,
                                instance.ToString()));

                            // Add the line height of the object
                            AddObjectHeightLine(room, instance.Position);
                        }

                        effect.Parameters["ModelViewProjection"].SetValue((instance.RotationPositionMatrix * viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(color);

                        effect.Techniques[0].Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                    }
            }

            _legacyDevice.SetVertexBuffer(_cone.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
            _legacyDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);
            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
                {
                    if (_editor.SelectedObject == instance)
                    {
                        // Outer cone
                        float coneAngle = (float)Math.Atan2(512, 1024);
                        float cutoffScaleH = 1;
                        float cutoffScaleW = instance.Fov * (float)(Math.PI / 360) / coneAngle * cutoffScaleH;

                        Matrix4x4 model = Matrix4x4.CreateScale(cutoffScaleW, cutoffScaleW, cutoffScaleH) * instance.ObjectMatrix;

                        effect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));

                        effect.CurrentTechnique.Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                    }
                }

            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
        }

        private void DrawSkybox(Matrix4x4 viewProjection)
        {
            _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);

            Effect skinnedModelEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_legacyDevice.SamplerStates.Default);

            WadMoveable moveable = _editor?.Level?.Settings?.WadTryGetMoveable(WadMoveableId.SkyBox);
            if (moveable == null)
                return;

            AnimatedModel model = _wadRenderer.GetMoveable(moveable);
           
            skinnedModelEffect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
            skinnedModelEffect.Parameters["Color"].SetValue(new Vector4(1.0f));

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh.Vertices.Count == 0)
                    continue;

                _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                Matrix4x4 world = Matrix4x4.CreateScale(128.0f) *
                                  model.AnimationTransforms[i] *
                                  Matrix4x4.CreateTranslation(Camera.GetPosition());

                skinnedModelEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());

                skinnedModelEffect.Techniques[0].Passes[0].Apply();

                foreach (var submesh in mesh.Submeshes)
                {
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                }
            }

            SwapChain.ClearDepth();
        }

        private void DrawMoveables(Matrix4x4 viewProjection, List<MoveableInstance> moveablesToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);

            Effect skinnedModelEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

            MoveableInstance _lastObject = null;

            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_legacyDevice.SamplerStates.Default);

            foreach (var instance in moveablesToDraw)
            {
                WadMoveable moveable = _editor?.Level?.Settings?.WadTryGetMoveable(instance.WadObjectId);
                if (moveable == null)
                    continue;

                AnimatedModel model = _wadRenderer.GetMoveable(moveable);
                AnimatedModel skin = model;
                if (instance.WadObjectId == WadMoveableId.Lara) // Show Lara
                {
                    WadMoveable skinMoveable = _editor?.Level?.Settings?.WadTryGetMoveable(WadMoveableId.LaraSkin);
                    if (skinMoveable != null)
                        skin = _wadRenderer.GetMoveable(skinMoveable);
                }

                Room room = instance.Room;

                skinnedModelEffect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                skinnedModelEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                if (!disableSelection && _editor.SelectedObject == instance) // Selection
                    skinnedModelEffect.Parameters["Color"].SetValue(_editor.Configuration.UI_ColorScheme.ColorSelection);

                for (int i = 0; i < skin.Meshes.Count; i++)
                {
                    var mesh = skin.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                    _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                    Matrix4x4 world = model.AnimationTransforms[i] * instance.ObjectMatrix;

                    skinnedModelEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());

                    skinnedModelEffect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                    {
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                    }
                }

                if (_editor.SelectedObject == instance)
                {
                    // Add text message
                    textToDraw.Add(CreateTextTagForObject(
                        instance.RotationPositionMatrix * viewProjection,
                        moveable.ToString(_editor.Level.Settings.WadGameVersion) +
                        (_editor.Level.Settings.GameVersion != GameVersion.TRNG ? 
                        "" :
                        " [ID = " + (instance.ScriptId?.ToString() ?? "<None>") + "]") +
                        (_editor.Level.Settings.GameVersion != GameVersion.TR5Main ?
                        "" :
                        " [LUA ID = " + (instance.LuaId.ToString()) + "]") +
                            "\n" + GetObjectPositionString(room, instance) +
                            "\nRotation Y: " + Math.Round(instance.RotationY, 2) +
                            (instance.Ocb == 0 ? "" : "\nOCB: " + instance.Ocb) +
                            BuildTriggeredByMessage(instance)));

                    // Add the line height of the object
                    AddObjectHeightLine(room, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private void DrawRoomImportedGeometry(Matrix4x4 viewProjection, List<ImportedGeometryInstance> importedGeometryToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            var geometryEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["RoomGeometry"];

            ImportedGeometryInstance _lastObject = null;

            for (var k = 0; k < importedGeometryToDraw.Count; k++)
            {
                var instance = importedGeometryToDraw[k];
                if (instance.Model?.DirectXModel == null)
                    continue;

                var model = instance.Model.DirectXModel;
                var room = instance.Room;
                var roomIndex = _editor.Level.Rooms.ReferenceIndexOf(room);

                var meshes = model.Meshes;
                
                for (var i = 0; i < meshes.Count; i++)
                {
                    var mesh = meshes[i];
                    if (!instance.MeshNameMatchesFilter(mesh.Name))
                        continue;
                    if (mesh.Vertices.Count == 0)
                        continue;

                    _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));
                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);

                    geometryEffect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());

                    geometryEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                    if (/*!disableSelection &&*/ _editor.SelectedObject == instance)
                        geometryEffect.Parameters["Color"].SetValue(_editor.Configuration.UI_ColorScheme.ColorSelection);

                    foreach (var submesh in mesh.Submeshes)
                    {
                        var texture = submesh.Value.Material.Texture;
                        if (texture != null)
                        {
                            geometryEffect.Parameters["TextureEnabled"].SetValue(true);
                            if (texture is ImportedGeometryTexture)
                            {
                                geometryEffect.Parameters["Texture"].SetResource(((ImportedGeometryTexture)texture).DirectXTexture);
                                geometryEffect.Parameters["ReciprocalTextureSize"].SetValue(new Vector2(1.0f / texture.Image.Width, 1.0f / texture.Image.Height));
                            }
                            else
                            {
                                int TODO_PORT_IMPORTED_GEOMETRY_RENDERING;
                                //geometryEffect.Parameters["Texture"].SetResource(_textureAtlas);
                                //geometryEffect.Parameters["ReciprocalTextureSize"].SetValue(new Vector2(1.0f / _textureAtlas.Width, 1.0f / _textureAtlas.Height));
                            }
                            geometryEffect.Parameters["TextureSampler"].SetResource(_legacyDevice.SamplerStates.AnisotropicWrap);
                        }
                        else
                        {
                            geometryEffect.Parameters["TextureEnabled"].SetValue(false);
                        }

                        geometryEffect.Techniques[0].Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                    }
                }

                if (_editor.SelectedObject == instance)
                {
                    // Add text message
                    textToDraw.Add(CreateTextTagForObject(
                        instance.RotationPositionMatrix * viewProjection,
                        instance + "\n" + GetObjectPositionString(_editor.SelectedRoom, instance)));

                    // Add the line height of the object
                    AddObjectHeightLine(_editor.SelectedRoom, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private void DrawStatics(Matrix4x4 viewProjection, List<StaticInstance> staticsToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);

            Effect staticMeshEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["StaticModel"];

            staticMeshEffect.Parameters["TextureSampler"].SetResource(_legacyDevice.SamplerStates.Default);

            StaticInstance _lastObject = null;

            foreach (var instance in staticsToDraw)
            {
                WadStatic @static = _editor?.Level?.Settings?.WadTryGetStatic(instance.WadObjectId);
                if (@static == null)
                    continue;
                StaticModel model = _wadRenderer.GetStatic(@static);

                staticMeshEffect.Parameters["Color"].SetValue(_editor.Mode == EditorMode.Lighting ? instance.Color : new Vector3(1.0f));
                staticMeshEffect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                if (!disableSelection && _editor.SelectedObject == instance)
                    staticMeshEffect.Parameters["Color"].SetValue(_editor.Configuration.UI_ColorScheme.ColorSelection);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));
                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);

                    staticMeshEffect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());

                    staticMeshEffect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                    {
                        _legacyDevice.Draw(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                    }
                }

                if (_editor.SelectedObject == instance)
                {
                    // Add text message
                    textToDraw.Add(CreateTextTagForObject(
                        instance.RotationPositionMatrix * viewProjection,
                        @static.ToString(_editor.Level.Settings.WadGameVersion) +
                        (_editor.Level.Settings.GameVersion != GameVersion.TRNG ?
                        "" :
                        " [ID = " + (instance.ScriptId?.ToString() ?? "<None>") + "]") +
                        (_editor.Level.Settings.GameVersion != GameVersion.TR5Main ?
                        "" :
                        " [LUA ID = " + (instance.LuaId.ToString()) + "]") +
                            "\n" + GetObjectPositionString(_editor.SelectedRoom, instance) +
                            "\n" + "Rotation Y: " + Math.Round(instance.RotationY, 2) +
                            BuildTriggeredByMessage(instance)));

                    // Add the line height of the object
                    AddObjectHeightLine(_editor.SelectedRoom, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private Text CreateTextTagForObject(Matrix4x4 matrix, string message)
        {
            return new Text
            {
                Font = _fontDefault,
                TextAlignment = new Vector2(0.0f, 0.0f),
                PixelPos = new VectorInt2(10, -10),
                Pos = matrix.TransformPerspectively(new Vector3()).To2(),
                Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                String = message
            };
        }


        private List<Room> CollectRoomsToDraw(Room baseRoom)
        {
            if (ShowAllRooms)
                return _editor.Level.Rooms.Where(room => room != null && room.AlternateBaseRoom == null).ToList();
            else if (!ShowPortals)
                return new List<Room>(new[] { baseRoom });

            // New iterative version of the function
            List<Room> result = new List<Room>();
            Vector3 cameraPosition = Camera.GetPosition();
            Stack<Room> stackRooms = new Stack<Room>();
            Stack<int> stackLimits = new Stack<int>();
            HashSet<Room> visitedRooms = new HashSet<Room>();

            stackRooms.Push(baseRoom);
            stackLimits.Push(0);

            bool isFlipped = baseRoom.Alternated && baseRoom.AlternateBaseRoom != null;

            while (stackRooms.Count > 0)
            {
                var theRoom = stackRooms.Pop();
                int theLimit = stackLimits.Pop();

                if (theLimit > _editor.Configuration.Rendering3D_DrawRoomsMaxDepth)
                    continue;

                if (isFlipped)
                {
                    if (!theRoom.Alternated)
                    {
                        visitedRooms.Add(theRoom);
                        if (!result.Contains(theRoom))
                            result.Add(theRoom);
                    }
                    else
                    {
                        if (theRoom.AlternateRoom != null)
                        {
                            visitedRooms.Add(theRoom);
                            if (!result.Contains(theRoom.AlternateRoom))
                                result.Add(theRoom.AlternateRoom);
                        }
                        else
                        {
                            visitedRooms.Add(theRoom);
                            if (!result.Contains(theRoom))
                                result.Add(theRoom);
                        }
                    }
                }
                else
                {
                    visitedRooms.Add(theRoom);
                    if (!result.Contains(theRoom))
                        result.Add(theRoom);
                }

                foreach (var portal in theRoom.Portals)
                {
                    Vector3 normal = Vector3.Zero;

                    if (portal.Direction == PortalDirection.WallPositiveZ)
                        normal = -Vector3.UnitZ;
                    if (portal.Direction == PortalDirection.WallPositiveX)
                        normal = -Vector3.UnitX;
                    if (portal.Direction == PortalDirection.WallNegativeZ)
                        normal = Vector3.UnitZ;
                    if (portal.Direction == PortalDirection.WallNegativeX)
                        normal = Vector3.UnitX;
                    if (portal.Direction == PortalDirection.Floor)
                        normal = Vector3.UnitY;
                    if (portal.Direction == PortalDirection.Ceiling)
                        normal = -Vector3.UnitY;

                    Vector3 cameraDirection = cameraPosition - Camera.Target;

                    if (Vector3.Dot(normal, cameraDirection) < -0.1f && theLimit > 1)
                        continue;

                    if (!visitedRooms.Contains(portal.AdjoiningRoom) &&
                        !stackRooms.Contains(portal.AdjoiningRoom))
                    {
                        stackRooms.Push(portal.AdjoiningRoom);
                        stackLimits.Push(theLimit + 1);
                    }
                }
            }
            return result;
        }

        protected override Vector4 ClearColor =>
            _editor?.SelectedRoom?.AlternateBaseRoom != null ?
                _editor.Configuration.UI_ColorScheme.ColorFlipRoom :
                (ShowHorizon ? new Vector4(0) : _editor.Configuration.UI_ColorScheme.Color3DBackground);

        // Do NOT call this method to redraw the scene!
        // Call Invalidate() instead to schedule a redraw in the message loop.
        protected override void OnDraw()
        {
            // Verify that editor is ready
            if (_editor == null || _editor.Level == null || _editor.SelectedRoom == null || _legacyDevice == null)
                return;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // New rendering setup
            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            _renderingStateBuffer.Set(new RenderingState
            {
                ShowExtraBlendingModes = ShowExtraBlendingModes,
                RoomGridForce = _editor.Mode == EditorMode.Geometry,
                RoomDisableVertexColors = _editor.Mode == EditorMode.FaceEdit,
                RoomGridLineWidth = _editor.Configuration.Rendering3D_LineWidth,
                TransformMatrix = viewProjection
            });

            // Reset
            List<Text> textToDraw = new List<Text>();
            _drawHeightLine = false;
            _drawFlybyPath = false;
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            _legacyDevice.SetDepthStencilState(_legacyDevice.DepthStencilStates.Default);
            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

            // Collect rooms to draw
            Room[] roomsToDraw = CollectRoomsToDraw(_editor.SelectedRoom).ToArray();
            float[] roomsToDrawDistanceSquared = new float[roomsToDraw.Length];
            for (int i = 0; i < roomsToDraw.Length; ++i)
                roomsToDrawDistanceSquared[i] = Vector3.DistanceSquared(Camera.GetPosition(), roomsToDraw[i].WorldPos + roomsToDraw[i].GetLocalCenter());

            Array.Sort(roomsToDrawDistanceSquared, roomsToDraw);
            Array.Reverse(roomsToDraw);

            // Collect objects to draw
            List<MoveableInstance> moveablesToDraw = new List<MoveableInstance>();
            List<StaticInstance> staticsToDraw = new List<StaticInstance>();
            List<ImportedGeometryInstance> importedGeometryToDraw = new List<ImportedGeometryInstance>();
            for (int i = 0; i < roomsToDraw.Length; i++)
            {
                moveablesToDraw.AddRange(roomsToDraw[i].Objects.OfType<MoveableInstance>());
                staticsToDraw.AddRange(roomsToDraw[i].Objects.OfType<StaticInstance>());
                importedGeometryToDraw.AddRange(roomsToDraw[i].Objects.OfType<ImportedGeometryInstance>());
            }
            var comparer = new Comparer(_editor.Level);
            moveablesToDraw.Sort(comparer);
            staticsToDraw.Sort(comparer);

            // Draw room names
            if (ShowRoomNames)
            {
                Size size = ClientSize;
                for (int i = 0; i < roomsToDraw.Length; i++)
                    textToDraw.Add(new Text
                    {
                        Font = _fontDefault,
                        Pos = (Matrix4x4.CreateTranslation(roomsToDraw[i].WorldPos) * viewProjection).TransformPerspectively(roomsToDraw[i].GetLocalCenter()).To2(),
                        Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                        String = roomsToDraw[i].Name
                    });
            }

            // Draw North, South, East and West
            if (ShowCardinalDirections)
            {
                string[] messages = { "+Z (North)", "-Z (South)", "+X (East)", "-X (West)" };
                Vector3[] positions = new Vector3[4]
                    {
                        new Vector3(0, 0, _editor.SelectedRoom.NumZSectors * 512.0f),
                        new Vector3(0, 0, _editor.SelectedRoom.NumZSectors * -512.0f),
                        new Vector3(_editor.SelectedRoom.NumXSectors * 512.0f, 0, 0),
                        new Vector3(_editor.SelectedRoom.NumXSectors * -512.0f, 0, 0)
                     };

                Vector3 center = _editor.SelectedRoom.GetLocalCenter();
                Matrix4x4 matrix = Matrix4x4.CreateTranslation(_editor.SelectedRoom.WorldPos) * viewProjection;
                for (int i = 0; i < 4; i++)
                    textToDraw.Add(new Text
                    {
                        Font = _fontDefault,
                        Pos = matrix.TransformPerspectively(center + positions[i]).To2(),
                        Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                        String = messages[i]
                    });
            }

            // Draw skybox
            if (ShowHorizon)
                DrawSkybox(viewProjection);

            // Draw rooms
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();
            foreach (Room room in roomsToDraw)
                _renderingCachedRooms[room].Render(new RenderingDrawingRoom.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer
                });

            // Determine the window we're currently in. If no window is selected, it means
            // we're in color dialog and have to disable selection highlight for convinience.

            var drawSelection = Form.ActiveForm == null;


            // Draw moveables and static meshes
            {
                // Before drawing custom geometry, apply a depth bias for reducing Z fighting
                _legacyDevice.SetRasterizerState(_rasterizerStateDepthBias);

                if (ShowMoveables)
                    DrawMoveables(viewProjection, moveablesToDraw, textToDraw, drawSelection);
                if (ShowStatics)
                    DrawStatics(viewProjection, staticsToDraw, textToDraw, drawSelection);

                _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
            }

            // Draw room imported geometry
            if (importedGeometryToDraw.Count != 0)
            {
                // If picking for imported geometry is disabled, then draw geometry translucent
                if (DisablePickingForImportedGeometry)
                    _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Additive);

                // Before drawing custom geometry, apply a depth bias for reducing Z fighting
                _legacyDevice.SetRasterizerState(_rasterizerStateDepthBias);

                // Draw imported geometry
                DrawRoomImportedGeometry(viewProjection, importedGeometryToDraw, textToDraw, drawSelection);

                // Reset GPU states
                _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
                if (DisablePickingForImportedGeometry)
                    _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);
            }

            if (ShowOtherObjects)
            {
                // Draw objects (sinks, cameras, fly-by cameras and sound sources) only for current room
                DrawObjects(viewProjection, roomsToDraw, textToDraw);
                // Draw light objects and bounding volumes only for current room
                DrawLights(viewProjection, roomsToDraw, textToDraw);
            }

            // Draw the height of the object
            DrawDebugLines(viewProjection);

            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            // Draw the gizmo
            SwapChain.ClearDepth();
            _gizmo.Draw(viewProjection);

            watch.Stop();

            // Construct debug string

            string DebugString = "";
            if (_editor.Configuration.Rendering3D_ShowFPS)
                DebugString += "FPS: " + Math.Round(1.0f / watch.Elapsed.TotalSeconds, 2) + ", Rooms vertices: " + roomsToDraw.Sum(room => room.RoomGeometry.VertexPositions.Count) + "\n";

            if (_editor.Configuration.Rendering3D_ShowRenderingStatistics)
                DebugString += "Rooms: " + roomsToDraw.Length + ", Moveables: " + moveablesToDraw.Count + ", Static Meshes: " + staticsToDraw.Count + "\n";

            if (_editor.SelectedObject != null)
                DebugString += "Selected Object: " + _editor.SelectedObject.ToShortString();

            // Draw debug string
            textToDraw.Add(new Text
            {
                Font = _fontDefault,
                PixelPos = new Vector2(10, -5),
                Alignment = new Vector2(0.0f, 0.0f),
                Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                String = DebugString
            });

            // Finish strings
            SwapChain.RenderText(textToDraw);
        }

        private static float GetFloorHeight(Room room, Vector3 position)
        {
            int xBlock = (int)Math.Max(0, Math.Min(room.NumXSectors - 1, Math.Floor(position.X / 1024.0f)));
            int zBlock = (int)Math.Max(0, Math.Min(room.NumZSectors - 1, Math.Floor(position.Z / 1024.0f)));

            // Get the base floor height
            return room.Blocks[xBlock, zBlock].Floor.Min * 256.0f;
        }

        private static string GetObjectPositionString(Room room, PositionBasedObjectInstance instance)
        {
            // Get the distance between point and floor in units
            float height = instance.Position.Y - GetFloorHeight(room, instance.Position);

            string message = "Position: [" + instance.Position.X + ", " + instance.Position.Y + ", " + instance.Position.Z + "]";
            message += "\nSector Position: [" + instance.SectorPosition.X + ", " + instance.SectorPosition.Y + "]";
            message += "\nHeight: " + Math.Round(height) + " units(" + height / 256.0f +
                       " clicks)";

            return message;
        }

        private void AddObjectHeightLine(Room room, Vector3 position)
        {
            float floorHeight = GetFloorHeight(room, position);

            // Get the distance between point and floor in units
            float height = position.Y - floorHeight;

            // Prepare two vertices for the line
            var vertices = new[]
            {
                new SolidVertex { Position = position, Color = Vector4.One },
                new SolidVertex { Position = new Vector3(position.X, floorHeight, position.Z), Color = Vector4.One }
            };

            // Prepare the Vertex Buffer
            if (_objectHeightLineVertexBuffer != null)
                _objectHeightLineVertexBuffer.Dispose();
            _objectHeightLineVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_legacyDevice,
                vertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _drawHeightLine = true;
        }

        private void AddFlybyPath(int sequence)
        {
            // Collect all flyby cameras
            List<FlybyCameraInstance> flybyCameras = new List<FlybyCameraInstance>();

            foreach (var room in _editor.Level.Rooms.Where(room => room != null))
                foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
                {
                    if (instance.Sequence == sequence)
                        flybyCameras.Add(instance);
                }

            // Is it actually necessary to show the path?
            if (flybyCameras.Count < 2)
                return;

            // Sort cameras
            flybyCameras.Sort((x, y) => x.Number.CompareTo(y.Number));

            // Create a vertex array
            List<SolidVertex> vertices = new List<SolidVertex>();

            for (int i = 0; i < flybyCameras.Count - 1; i++)
            {
                Vector3 room1pos = flybyCameras[i].Room.WorldPos;
                Vector3 room2pos = flybyCameras[i + 1].Room.WorldPos;

                var v1 = new SolidVertex();
                v1.Position = flybyCameras[i].Position + room1pos;
                v1.Color = Vector4.One;

                var v2 = new SolidVertex();
                v2.Position = flybyCameras[i + 1].Position + room2pos;
                v2.Color = Vector4.One;

                vertices.Add(v1);
                vertices.Add(v2);
            }

            // Prepare the Vertex Buffer
            if (_flybyPathVertexBuffer != null)
                _flybyPathVertexBuffer.Dispose();
            _flybyPathVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_legacyDevice, vertices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _drawFlybyPath = true;
        }

        private class Comparer : IComparer<StaticInstance>, IComparer<MoveableInstance>
        {
            private readonly Dictionary<Room, int> _rooms = new Dictionary<Room, int>();

            public Comparer(Level level)
            {
                for (int i = 0; i < level.Rooms.Length; ++i)
                    if (level.Rooms[i] != null)
                        _rooms.Add(level.Rooms[i], i);
            }

            public int Compare(StaticInstance x, StaticInstance y)
            {
                return x.WadObjectId.TypeId.CompareTo(y.WadObjectId.TypeId);
            }

            public int Compare(MoveableInstance x, MoveableInstance y)
            {
                return x.WadObjectId.TypeId.CompareTo(y.WadObjectId.TypeId);
            }
        }

        private class PickingResultBlock : PickingResult
        {
            public float VerticalCoord { get; set; }
            public VectorInt2 Pos { get; set; }
            public Room Room { get; set; }
            public BlockFace Face { get; set; }

            public bool IsFloorHorizontalPlane => Face == BlockFace.Floor || Face == BlockFace.FloorTriangle2;
            public bool IsCeilingHorizontalPlane => Face == BlockFace.Ceiling || Face == BlockFace.CeilingTriangle2;
            public bool IsVerticalPlane => !IsFloorHorizontalPlane && !IsCeilingHorizontalPlane;
            public bool BelongsToFloor => IsFloorHorizontalPlane || Face <= BlockFace.DiagonalMiddle;
            public bool BelongsToCeiling => IsCeilingHorizontalPlane || Face > BlockFace.DiagonalMiddle;
            public PickingResultBlock(float distance, float verticalCoord, VectorInt2 pos, Room room, BlockFace face)
            {
                Distance = distance;
                VerticalCoord = verticalCoord;
                Pos = pos;
                Room = room;
                Face = face;
            }
        }

        private class PickingResultObject : PickingResult
        {
            public ObjectInstance ObjectInstance { get; set; }
            public PickingResultObject(float distance, ObjectInstance objectPtr)
            {
                Distance = distance;
                ObjectInstance = objectPtr;
            }
        }

        private class ToolHandler
        {
            private class ReferenceCell
            {
                public readonly short[,] Heights = new short[2, 4] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
                public bool Processed = false;
            }

            private readonly PanelRendering3D _parent;
            private ReferenceCell[,] _actionGrid;
            private Point _referencePosition;
            private Point _newPosition;

            private Point GetQuantizedPosition(int x, int y) => new Point((int)(x * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity), (int)(y * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity));

            // Terrain map resolution must be ALWAYS POWER OF 2 PLUS 1 - this is the requirement of diamond square algorithm.
            public float[,] RandomHeightMap;

            public bool Engaged { get; private set; }
            public bool Dragged { get; private set; }
            public bool PositionDiffers(int x, int y) => _newPosition != GetQuantizedPosition(x, y);

            public PickingResultBlock ReferencePicking { get; private set; }
            public Room ReferenceRoom { get; private set; }
            public Block ReferenceBlock => ReferenceRoom.GetBlockTry(ReferencePicking.Pos.X, ReferencePicking.Pos.Y);
            public bool ReferenceIsDiagonalStep => ReferencePicking.BelongsToFloor ? ReferenceBlock.Floor.DiagonalSplit != DiagonalSplit.None : ReferenceBlock.Ceiling.DiagonalSplit != DiagonalSplit.None;
            public bool ReferenceIsOppositeDiagonalStep
            {
                get
                {
                    if (ReferenceIsDiagonalStep)
                    {
                        if (ReferencePicking.BelongsToFloor)
                        {
                            switch (ReferenceBlock.Floor.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZp:
                                    if (ReferencePicking.Face == BlockFace.FloorTriangle2 ||
                                        ReferencePicking.Face == BlockFace.NegativeZ_QA ||
                                        ReferencePicking.Face == BlockFace.NegativeZ_ED ||
                                        ReferencePicking.Face == BlockFace.PositiveX_QA ||
                                        ReferencePicking.Face == BlockFace.PositiveX_ED)
                                        return true;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (ReferencePicking.Face == BlockFace.Floor ||
                                        ReferencePicking.Face == BlockFace.NegativeX_QA ||
                                        ReferencePicking.Face == BlockFace.NegativeX_ED ||
                                        ReferencePicking.Face == BlockFace.PositiveZ_QA ||
                                        ReferencePicking.Face == BlockFace.PositiveZ_ED)
                                        return true;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (ReferencePicking.Face == BlockFace.FloorTriangle2 ||
                                        ReferencePicking.Face == BlockFace.NegativeX_QA ||
                                        ReferencePicking.Face == BlockFace.NegativeX_ED ||
                                        ReferencePicking.Face == BlockFace.NegativeZ_QA ||
                                        ReferencePicking.Face == BlockFace.NegativeZ_ED)
                                        return true;
                                    break;
                                case DiagonalSplit.XnZn:
                                    if (ReferencePicking.Face == BlockFace.Floor ||
                                        ReferencePicking.Face == BlockFace.PositiveX_QA ||
                                        ReferencePicking.Face == BlockFace.PositiveX_ED ||
                                        ReferencePicking.Face == BlockFace.PositiveZ_QA ||
                                        ReferencePicking.Face == BlockFace.PositiveZ_ED)
                                        return true;
                                    break;
                            }
                        }
                        else
                        {
                            switch (ReferenceBlock.Ceiling.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZp:
                                    if (ReferencePicking.Face == BlockFace.CeilingTriangle2 ||
                                        ReferencePicking.Face == BlockFace.NegativeZ_WS ||
                                        ReferencePicking.Face == BlockFace.NegativeZ_RF ||
                                        ReferencePicking.Face == BlockFace.PositiveX_WS ||
                                        ReferencePicking.Face == BlockFace.PositiveX_RF)
                                        return true;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (ReferencePicking.Face == BlockFace.Ceiling ||
                                        ReferencePicking.Face == BlockFace.NegativeX_WS ||
                                        ReferencePicking.Face == BlockFace.NegativeX_RF ||
                                        ReferencePicking.Face == BlockFace.PositiveZ_WS ||
                                        ReferencePicking.Face == BlockFace.PositiveZ_RF)
                                        return true;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (ReferencePicking.Face == BlockFace.CeilingTriangle2 ||
                                        ReferencePicking.Face == BlockFace.NegativeX_WS ||
                                        ReferencePicking.Face == BlockFace.NegativeX_RF ||
                                        ReferencePicking.Face == BlockFace.NegativeZ_WS ||
                                        ReferencePicking.Face == BlockFace.NegativeZ_RF)
                                        return true;
                                    break;
                                case DiagonalSplit.XnZn:
                                    if (ReferencePicking.Face == BlockFace.Ceiling ||
                                        ReferencePicking.Face == BlockFace.PositiveX_WS ||
                                        ReferencePicking.Face == BlockFace.PositiveX_RF ||
                                        ReferencePicking.Face == BlockFace.PositiveZ_WS ||
                                        ReferencePicking.Face == BlockFace.PositiveZ_RF)
                                        return true;
                                    break;
                            }
                        }
                    }
                    return false;
                }
            }


            public ToolHandler(PanelRendering3D parent)
            {
                _parent = parent;
            }

            private void PrepareActionGrid()
            {
                _actionGrid = new ReferenceCell[ReferenceRoom.NumXSectors, ReferenceRoom.NumZSectors];
                for (int x = 0; x < _actionGrid.GetLength(0); x++)
                    for (int z = 0; z < _actionGrid.GetLength(1); z++)
                    {
                        _actionGrid[x, z] = new ReferenceCell();
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; edge++)
                            if (ReferencePicking.BelongsToFloor)
                            {
                                _actionGrid[x, z].Heights[0, (int)edge] = ReferenceRoom.Blocks[x, z].Floor.GetHeight(edge);
                                _actionGrid[x, z].Heights[1, (int)edge] = ReferenceRoom.Blocks[x, z].GetHeight(BlockVertical.Ed, edge);
                            }
                            else
                            {
                                _actionGrid[x, z].Heights[0, (int)edge] = ReferenceRoom.Blocks[x, z].Ceiling.GetHeight(edge);
                                _actionGrid[x, z].Heights[1, (int)edge] = ReferenceRoom.Blocks[x, z].GetHeight(BlockVertical.Rf, edge);
                            }
                    }
            }

            private void GenerateNewTerrain()
            {
                // Algorithm used here is naive Diamond-Square, which should be enough for low-res TR geometry.

                int s = RandomHeightMap.GetLength(0) - 1;

                if ((s & (s - 1)) != 0)
                    throw new Exception("Wrong heightmap size defined for Diamond-Square algorithm. Must be power of 2.");

                float range = 1.0f;
                float rough = 0.9f;
                float average = 0.0f;
                int sideLength, halfSide, x, y;

                Random rndValue = new Random();
                Array.Clear(RandomHeightMap, 0, RandomHeightMap.Length);

                // While the side length is greater than 1
                for (sideLength = s; sideLength > 1; sideLength /= 2)
                {
                    halfSide = sideLength / 2;

                    // Run Diamond Step
                    for (x = 0; x < s; x += sideLength)
                        for (y = 0; y < s; y += sideLength)
                        {
                            // Get the average of the corners
                            average = RandomHeightMap[x, y];
                            average += RandomHeightMap[x + sideLength, y];
                            average += RandomHeightMap[x, y + sideLength];
                            average += RandomHeightMap[x + sideLength, y + sideLength];
                            average /= 4.0f;

                            // Offset by a random value
                            average += ((float)rndValue.NextDouble() - 0.5f) * (2.0f * range);
                            RandomHeightMap[x + halfSide, y + halfSide] = average;
                        }

                    // Run Square Step
                    for (x = 0; x < s; x += halfSide)
                        for (y = (x + halfSide) % sideLength; y < s; y += sideLength)
                        {
                            // Get the average of the corners
                            average = RandomHeightMap[(x - halfSide + s) % s, y];
                            average += RandomHeightMap[(x + halfSide) % s, y];
                            average += RandomHeightMap[x, (y + halfSide) % s];
                            average += RandomHeightMap[x, (y - halfSide + s) % s];
                            average /= 4.0f;

                            // Offset by a random value
                            average += ((float)rndValue.NextDouble() - 0.5f) * (2.0f * range);

                            // Set the height value to be the calculated average
                            RandomHeightMap[x, y] = average + range;

                            // Set the height on the opposite edge if this is an edge piece
                            if (x == 0)
                                RandomHeightMap[s, y] = average;
                            if (y == 0)
                                RandomHeightMap[x, s] = average;
                        }

                    // Lower the random value range
                    range -= range * 0.5f * rough;
                }

                // Hacky postprocess first point to be in sync during scaling operations
                RandomHeightMap[0, 0] = (RandomHeightMap[0, 1] + RandomHeightMap[1, 0]) / 2.0f;
            }

            private void RelocatePicking()
            {
                // We need to relocate picked diagonal faces, because behaviour is undefined
                // for these cases if diagonal step was raised above limit and swapped.
                // Also, we relocate middle face pickings for walls to nearest floor or ceiling face.

                if (ReferencePicking.Face == BlockFace.DiagonalED ||
                    ReferencePicking.Face == BlockFace.DiagonalQA)
                {
                    switch (ReferenceBlock.Floor.DiagonalSplit)
                    {
                        case DiagonalSplit.XnZp:
                        case DiagonalSplit.XpZp:
                            ReferencePicking.Face = BlockFace.Floor;
                            break;
                        case DiagonalSplit.XpZn:
                        case DiagonalSplit.XnZn:
                            ReferencePicking.Face = BlockFace.FloorTriangle2;
                            break;
                    }
                }
                else if (ReferencePicking.Face == BlockFace.DiagonalWS ||
                         ReferencePicking.Face == BlockFace.DiagonalRF)
                {
                    switch (ReferenceBlock.Ceiling.DiagonalSplit)
                    {
                        case DiagonalSplit.XnZp:
                        case DiagonalSplit.XpZp:
                            ReferencePicking.Face = BlockFace.Ceiling;
                            break;
                        case DiagonalSplit.XpZn:
                        case DiagonalSplit.XnZn:
                            ReferencePicking.Face = BlockFace.CeilingTriangle2;
                            break;
                    }
                }
                else if (ReferencePicking.Face == BlockFace.NegativeX_Middle ||
                         ReferencePicking.Face == BlockFace.NegativeZ_Middle ||
                         ReferencePicking.Face == BlockFace.PositiveX_Middle ||
                         ReferencePicking.Face == BlockFace.PositiveZ_Middle ||
                         ReferencePicking.Face == BlockFace.DiagonalMiddle)
                {
                    Direction direction;
                    switch (ReferencePicking.Face)
                    {
                        case BlockFace.NegativeX_Middle:
                            direction = Direction.NegativeX;
                            break;
                        case BlockFace.PositiveX_Middle:
                            direction = Direction.PositiveX;
                            break;
                        case BlockFace.NegativeZ_Middle:
                            direction = Direction.NegativeZ;
                            break;
                        case BlockFace.PositiveZ_Middle:
                            direction = Direction.PositiveZ;
                            break;
                        default:
                            direction = Direction.Diagonal;
                            break;
                    }

                    var face = EditorActions.GetFaces(ReferenceRoom, ReferencePicking.Pos, direction, BlockFaceType.Wall).First(item => item.Key == ReferencePicking.Face);

                    if (face.Value[0] - ReferencePicking.VerticalCoord > ReferencePicking.VerticalCoord - face.Value[1])
                        switch (ReferenceBlock.Floor.DiagonalSplit)
                        {
                            default:
                            case DiagonalSplit.XnZp:
                            case DiagonalSplit.XpZp:
                                ReferencePicking.Face = BlockFace.Floor;
                                break;
                            case DiagonalSplit.XpZn:
                            case DiagonalSplit.XnZn:
                                ReferencePicking.Face = BlockFace.FloorTriangle2;
                                break;
                        }
                    else
                        switch (ReferenceBlock.Ceiling.DiagonalSplit)
                        {
                            default:
                            case DiagonalSplit.XnZp:
                            case DiagonalSplit.XpZp:
                                ReferencePicking.Face = BlockFace.Ceiling;
                                break;
                            case DiagonalSplit.XpZn:
                            case DiagonalSplit.XnZn:
                                ReferencePicking.Face = BlockFace.CeilingTriangle2;
                                break;
                        }
                }
            }

            public void Engage(int refX, int refY, PickingResultBlock refPicking, bool relocatePicking = true, Room refRoom = null)
            {
                if (!Engaged)
                {
                    Engaged = true;
                    _referencePosition = new Point((int)(refX * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity), (int)(refY * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity));
                    _newPosition = _referencePosition;
                    ReferencePicking = refPicking;
                    ReferenceRoom = refRoom ?? _parent._editor.SelectedRoom;

                    // Relocate picking may be not needed for texture operations (e.g. wall 4x4 painting)
                    if (relocatePicking)
                        RelocatePicking();

                    // Initialize data structures
                    PrepareActionGrid();

                    int randomHeightMapSize = 1;
                    while (randomHeightMapSize < Math.Max(ReferenceRoom.NumXSectors, ReferenceRoom.NumZSectors))
                        randomHeightMapSize *= 2; // Find random height map that is a power of two plus.
                    ++randomHeightMapSize;
                    RandomHeightMap = new float[randomHeightMapSize, randomHeightMapSize];

                    if (_parent._editor.Tool.Tool == EditorToolType.Terrain)
                        GenerateNewTerrain();
                }
            }

            public void Disengage()
            {
                if (Engaged)
                {
                    Engaged = false;
                    Dragged = false;
                    _parent._editor.HighlightedSectors = SectorSelection.None;
                    _parent._renderingCachedRooms.Remove(ReferenceRoom); // To update highlight state
                }
            }

            public bool Process(int x, int y)
            {
                if ((_parent._editor.SelectedSectors.Valid && _parent._editor.SelectedSectors.Area.Contains(new VectorInt2(x, y)) || _parent._editor.SelectedSectors.Empty) && !_actionGrid[x, y].Processed)
                {
                    _actionGrid[x, y].Processed = true;
                    return true;
                }
                else
                    return false;
            }

            public Point? UpdateDragState(int newX, int newY, bool relative, bool highlightSelection = true)
            {
                var newPosition = GetQuantizedPosition(newX, newY);

                if (newPosition != _newPosition)
                {
                    Point delta;
                    if (relative)
                        delta = new Point(Math.Sign(_newPosition.X - newPosition.X), Math.Sign(_newPosition.Y - newPosition.Y));
                    else
                        delta = new Point(_referencePosition.X - newPosition.X, _referencePosition.Y - newPosition.Y);
                    _newPosition = newPosition;
                    Dragged = true;
                    if (highlightSelection)
                        _parent._editor.HighlightedSectors = _parent._editor.SelectedSectors;
                    _parent._renderingCachedRooms.Remove(ReferenceRoom); // To update highlight state
                    return delta;
                }
                else
                    return null;
            }

            public void DiscardEditedGeometry(bool autoUpdate = false)
            {
                for (int x = 0; x < ReferenceRoom.NumXSectors; x++)
                    for (int z = 0; z < ReferenceRoom.NumZSectors; z++)
                    {
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; edge++)
                        {
                            if (ReferencePicking.BelongsToFloor)
                            {
                                ReferenceRoom.Blocks[x, z].Floor.SetHeight(edge, _actionGrid[x, z].Heights[0, (int)edge]);
                                ReferenceRoom.Blocks[x, z].SetHeight(BlockVertical.Ed, edge, _actionGrid[x, z].Heights[1, (int)edge]);
                            }
                            else
                            {
                                ReferenceRoom.Blocks[x, z].Ceiling.SetHeight(edge, _actionGrid[x, z].Heights[0, (int)edge]);
                                ReferenceRoom.Blocks[x, z].SetHeight(BlockVertical.Rf, edge, _actionGrid[x, z].Heights[1, (int)edge]);
                            }
                        }
                    }

                if (autoUpdate)
                    EditorActions.SmartBuildGeometry(ReferenceRoom, ReferenceRoom.LocalArea);
            }
        }
    }
}
