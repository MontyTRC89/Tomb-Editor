using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using TombEditor.Geometry;
using SharpDX.Toolkit.Graphics;
using TombLib.Graphics;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using TombLib.Wad;

namespace TombEditor.Controls
{
    public abstract class PickingResult
    {
        public float Distance { get; set; }
    };

    public partial class PanelRendering3D : Panel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        

        private class RenderBucket
        {
            public Room Room { get; set; }
            public int Texture { get; set; }
            public byte DoubleSided { get; set; }
            public int X { get; set; }
            public int Z { get; set; }
            public byte Invisible { get; set; }
            public Plane Plane { get; set; }
            public float Distance { get; set; }
            public Vector3 Center { get; set; }
            public List<BlockFace> Faces { get; set; }
            public BlockFaces FaceType { get; set; }
            public BlockFace Face { get; set; }
            public Buffer<short> IndexBuffer { get; set; }
            public List<short> Indices { get; set; }
            public byte AlphaTest { get; set; }

            public RenderBucket()
            {
                Faces = new List<BlockFace>();
                Indices = new List<short>();
            }
        }

        private class ComparerOpaqueBuckets : IComparer<RenderBucket>
        {
            private readonly Room[] _rooms;

            public ComparerOpaqueBuckets(Room[] rooms)
            {
                _rooms = rooms;
            }

            public int Compare(RenderBucket x, RenderBucket y)
            {
                if (x == null || y == null)
                    return 0;

                int result = x.DoubleSided.CompareTo(y.DoubleSided);
                if (result != 0)
                    return result;
                result = _rooms.ReferenceIndexOf(x.Room).CompareTo(_rooms.ReferenceIndexOf(y.Room));
                if (result != 0)
                    return result;
                return x.Texture.CompareTo(y.Texture);
            }
        }

        private class ComparerTransparentBuckets : IComparer<RenderBucket>
        {
            private readonly Room[] _rooms;

            public ComparerTransparentBuckets(Room[] rooms)
            {
                _rooms = rooms;
            }

            public int Compare(RenderBucket x, RenderBucket y)
            {
                int result = x.Distance.CompareTo(y.Distance);
                if (result != 0)
                    return result;
                result = x.DoubleSided.CompareTo(y.DoubleSided);
                if (result != 0)
                    return result;
                result = _rooms.ReferenceIndexOf(x.Room).CompareTo(_rooms.ReferenceIndexOf(y.Room));
                if (result != 0)
                    return result;
                result = x.AlphaTest.CompareTo(y.AlphaTest);
                if (result != 0)
                    return result;
                return x.Texture.CompareTo(y.Texture);
            }
        }

        private class ComparerInvisibleBuckets : IComparer<RenderBucket>
        {
            private readonly Room[] _rooms;

            public ComparerInvisibleBuckets(Room[] rooms)
            {
                _rooms = rooms;
            }

            public int Compare(RenderBucket x, RenderBucket y)
            {
                return _rooms.ReferenceIndexOf(x.Room).CompareTo(_rooms.ReferenceIndexOf(y.Room));
            }
        }

        private class ComparerMoveables : IComparer<MoveableInstance>
        {
            private readonly Room[] _rooms;

            public ComparerMoveables(Room[] rooms)
            {
                _rooms = rooms;
            }

            public int Compare(MoveableInstance x, MoveableInstance y)
            {
                int result = x.WadObjectId.CompareTo(y.WadObjectId);
                if (result != 0)
                    return result;
                return _rooms.ReferenceIndexOf(x.Room).CompareTo(_rooms.ReferenceIndexOf(y.Room));
            }
        }

        private class ComparerStaticMeshes : IComparer<StaticInstance>
        {
            private readonly Room[] _rooms;

            public ComparerStaticMeshes(Room[] rooms)
            {
                _rooms = rooms;
            }

            public int Compare(StaticInstance x, StaticInstance y)
            {
                int result = x.WadObjectId.CompareTo(y.WadObjectId);
                if (result != 0)
                    return result;
                return _rooms.ReferenceIndexOf(x.Room).CompareTo(_rooms.ReferenceIndexOf(y.Room));
            }
        }

        private class ComparerFlybyCameras : IComparer<FlybyCameraInstance>
        {
            public int Compare(FlybyCameraInstance x, FlybyCameraInstance y)
            {
                int result = (x.Number > y.Number ? 1 : -1);
                return result;
            }
        }

        private class PickingResultBlock : PickingResult
        {
            public DrawingPoint Pos { get; set; }
            public BlockFaces Face { get; set; }
            public PickingResultBlock(float Distance, DrawingPoint pos, BlockFaces face)
            {
                this.Distance = Distance;
                this.Pos = pos;
                this.Face = face;
            }
        }

        private class PickingResultObject : PickingResult
        {
            public ObjectInstance ObjectInstance { get; set; }
            public PickingResultObject(float Distance,  ObjectInstance objectPtr)
            {
                this.Distance = Distance;
                this.ObjectInstance = objectPtr;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawPortals { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawRoomNames { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawHorizon { get; set; }

        private Editor _editor;
        private DeviceManager _deviceManager;
        private GraphicsDevice _device;
        private SwapChainGraphicsPresenter _presenter;
        private RasterizerState _rasterizerWireframe;
        private GeometricPrimitive _sphere;
        private GeometricPrimitive _cone;
        private GeometricPrimitive _linesCube;
        private GeometricPrimitive _littleCube;
        private GeometricPrimitive _littleSphere;
        private const float _littleCubeRadius = 128.0f;
        private const float _littleSphereRadius = 128.0f;
        private int _lastX;
        private int _lastY;
        private bool _doSectorSelection = false;
    
        // Gizmo
        private Gizmo _gizmo;

        // Rooms to draw
        private List<Room> _roomsToDraw;

        // Geometry buckets to draw
        private List<RenderBucket> _opaqueBuckets;
        private List<RenderBucket> _solidBuckets;
        private List<RenderBucket> _transparentBuckets;
        private List<RenderBucket> _invisibleBuckets;

        // Items to draw
        private readonly List<MoveableInstance> _moveablesToDraw = new List<MoveableInstance>();
        private readonly List<StaticInstance> _staticsToDraw = new List<StaticInstance>();
        private List<RoomGeometryInstance> RoomGeometryToDraw;

        // Debug lines
        private Buffer<EditorVertex> _objectHeightLineVertexBuffer;
        private bool _drawHeightLine = false;

        private Buffer<EditorVertex> _flybyPathVertexBuffer;
        private bool _drawFlybyPath = false;
        private bool _drawRoomBoundingBox = false;

        private Effect _roomEffect;

        public PanelRendering3D()
        {
            CenterCamera();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update drawing
            if ((obj is IEditorObjectChangedEvent) ||
                (obj is IEditorRoomChangedEvent) ||
                (obj is Editor.ConfigurationChangedEvent) ||
                (obj is Editor.SelectedObjectChangedEvent) ||
                (obj is Editor.SelectedSectorsChangedEvent) ||
                (obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.ModeChangedEvent) ||
                (obj is Editor.LoadedWadsChangedEvent) ||
                (obj is Editor.LoadedTexturesChangedEvent))
            {
                if (_editor.Mode != EditorMode.Map2D)
                    Invalidate();
            }

            // Update curser
            if (obj is Editor.ActionChangedEvent)
            {
                EditorAction currentAction = ((Editor.ActionChangedEvent)obj).Current;

                bool hasCrossCurser = currentAction.RelocateCameraActive;
                switch (currentAction.Action)
                {
                    case EditorActionType.Paste:
                    case EditorActionType.PlaceCamera:
                    case EditorActionType.PlaceFlyByCamera:
                    case EditorActionType.PlaceItem:
                    case EditorActionType.PlaceLight:
                    case EditorActionType.PlaceNoCollision:
                    case EditorActionType.PlaceSink:
                    case EditorActionType.PlaceSoundSource:
                    case EditorActionType.Stamp:
                        hasCrossCurser = true;
                        break;
                }
                Cursor = hasCrossCurser ? Cursors.Cross : Cursors.Arrow;
            }

            // Center camera
            if (obj is Editor.CenterCameraEvent)
                CenterCamera();

            // Move camera to sector
            if (obj is Editor.MoveCameraToSectorEvent)
            {
                var e = (Editor.MoveCameraToSectorEvent)obj;

                Vector3 center = _editor.SelectedRoom.GetLocalCenter();
                Camera.Target = new Vector3(e.Sector.X * 1024.0f, center.Y, e.Sector.Y * 1024.0f) + _editor.SelectedRoom.WorldPos;
                Invalidate();
            }
        }

        private float DefaultCameraDistance
        {
            get
            {
                Room room = _editor?.SelectedRoom;
                Vector2 roomDiagonal = new Vector2(room?.NumXSectors ?? 0, room?.NumZSectors ?? 0);
                return (roomDiagonal.Length() * 0.8f + 2.1f) * 1024.0f;
            }
        }
        private float DefaultCameraAngleX
        {
            get { return (float)Math.PI; }
        }
        private float DefaultCameraAngleY
        {
            get { return 0.6f; }
        }

        public void CenterCamera()
        {
            Room room = _editor?.SelectedRoom;

            // Point the camera to the room's center
            Vector3 target = new Vector3();
            if (room != null)
                target = room.WorldPos + room.GetLocalCenter();

            // Initialize a new camera
            Camera = new ArcBallCamera(target, DefaultCameraAngleX, DefaultCameraAngleY, - MathUtil.PiOverTwo, MathUtil.PiOverTwo, DefaultCameraDistance, 1000, 1000000);
            Invalidate();
        }
        
        public void InitializePanel(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
            _device = deviceManager.Device;
            logger.Info("Starting DirectX 11");

            // Initialize the viewport, after the panel is added and sized on the form
            var pp = new PresentationParameters
            {
                BackBufferFormat = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                BackBufferWidth = Width,
                BackBufferHeight = Height,
                DepthStencilFormat = DepthFormat.Depth24Stencil8,
                DeviceWindowHandle = this,
                IsFullScreen = false,
                MultiSampleCount = MSAALevel.None,
                PresentationInterval = PresentInterval.Immediate,
                RenderTargetUsage = SharpDX.DXGI.Usage.RenderTargetOutput | SharpDX.DXGI.Usage.BackBuffer,
                Flags = SharpDX.DXGI.SwapChainFlags.None
            };

            _presenter = new SwapChainGraphicsPresenter(_device, pp);
            CenterCamera();

            // Maybe I could use this as bounding box, scaling it properly before drawing
            _linesCube = GeometricPrimitive.LinesCube.New(_device);

            // This sphere will be scaled up and down multiple times for using as In & Out of lights
            _sphere = GeometricPrimitive.Sphere.New(_device, 1024, 6);

            //Little cubes and little spheres are used as mesh for lights, cameras, sinks, etc
            _littleCube = GeometricPrimitive.Cube.New(_device, 2 * _littleSphereRadius);
            _littleSphere = GeometricPrimitive.Sphere.New(_device, 2 * _littleCubeRadius, 8);

            _cone = GeometricPrimitive.Cone.New(_device, 1024, 1024, 18);

            // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
            new BasicEffect(_device);

            // Initialize the rasterizer state for wireframe drawing
            SharpDX.Direct3D11.RasterizerStateDescription renderStateDesc =
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

            _rasterizerWireframe = RasterizerState.New(_device, renderStateDesc);
            _gizmo = new Gizmo(deviceManager);

            logger.Info("Graphic Device ready");
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {}

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_presenter != null && Width != 0 && Height != 0)
                _presenter.Resize(Width, Height, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // I intercept arrow keys here otherwise they would processed by the form and 
            // camera would move only if Panel3D is focused
            switch (keyData)
            {
                case Keys.Up:
                    Camera.Rotate(0, -_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate);
                    Invalidate();
                    return true;

                case Keys.Down:
                    Camera.Rotate(0, _editor.Configuration.Rendering3D_NavigationSpeedKeyRotate);
                    Invalidate();
                    return true;

                case Keys.Left:
                    Camera.Rotate(_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate, 0);
                    Invalidate();
                    return true;

                case Keys.Right:
                    Camera.Rotate(-_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate, 0);
                    Invalidate();
                    return true;

                case Keys.PageUp:
                    Camera.Zoom(-_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom);
                    Invalidate();
                    return true;

                case Keys.PageDown:
                    Camera.Zoom(_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom);
                    Invalidate();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            // TODO: maybe put the constant 0.0005f in configuration?
            Camera.Zoom(-e.Delta * _editor.Configuration.Rendering3D_NavigationSpeedMouseZoom * 0.0005f);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _lastX = e.X;
            _lastY = e.Y;
            _doSectorSelection = false;

            if (e.Button == MouseButtons.Left)
            {
                // Do picking on the scene
                PickingResult newPicking = DoPicking(e.X, e.Y);

                // Move camera to selected sector
                if ((newPicking is PickingResultBlock) && (_editor.Action.RelocateCameraActive))
                {
                    _editor.MoveCameraToSector(((PickingResultBlock)newPicking).Pos);
                    return;
                }
                else if (newPicking is PickingResultObject)
                {
                    _editor.SelectedObject = ((PickingResultObject)newPicking).ObjectInstance;
                }

                // Set gizmo axis (or none if another object was picked)
                _gizmo.SetGizmoAxis((newPicking as PickingResultGizmo)?.Axis ?? GizmoAxis.None);

                // Process editor actions
                switch (_editor.Action.Action)
                {
                    case EditorActionType.PlaceLight:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceLight(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, _editor.Action.LightType);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceItem:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceItem(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, _editor.Action.ItemType);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceSink:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceSink(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceCamera:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceCamera(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceSoundSource:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceSoundSource(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceFlyByCamera:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceFlyByCamera(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceNoCollision:
                        if (newPicking is PickingResultBlock)
                            EditorActions.PlaceNoCollision(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ((PickingResultBlock)newPicking).Face);
                        break;
                    case EditorActionType.Paste:
                        if (newPicking is PickingResultBlock)
                        {
                            _editor.ObjectChange(Clipboard.Paste(_editor.Level, _editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos));
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.Stamp:
                        if (newPicking is PickingResultBlock)
                            _editor.ObjectChange(Clipboard.Paste(_editor.Level, _editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos));
                        break;
                    case EditorActionType.None:
                        switch (_editor.Mode)
                        {
                            case EditorMode.Geometry:
                                if (newPicking is PickingResultBlock)
                                {
                                    DrawingPoint pos = ((PickingResultBlock)newPicking).Pos;
                                    BlockFaces face = ((PickingResultBlock)newPicking).Face;

                                    // Split the faces
                                    if (ModifierKeys.HasFlag(Keys.Alt))
                                    {
                                        if (face == BlockFaces.Floor || face == BlockFaces.FloorTriangle2)
                                        {
                                            EditorActions.FlipFloorSplit(_editor.SelectedRoom, new Rectangle(pos.X, pos.Y, pos.X, pos.Y));
                                            return;
                                        }
                                        else if (face == BlockFaces.Ceiling || face == BlockFaces.CeilingTriangle2)
                                        {
                                            EditorActions.FlipCeilingSplit(_editor.SelectedRoom, new Rectangle(pos.X, pos.Y, pos.X, pos.Y));
                                            return;
                                        }
                                    }

                                    // Handle face selection
                                    if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos))
                                    {
                                        // Rotate the arrows
                                        if (Control.ModifierKeys.HasFlag(Keys.Control))
                                        {
                                            if (_editor.SelectedSectors.Arrow == EditorArrowType.CornerSW)
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.EntireFace);
                                            else if (_editor.SelectedSectors.Arrow == EditorArrowType.CornerSE)
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.CornerSW);
                                            else if (_editor.SelectedSectors.Arrow == EditorArrowType.CornerNE)
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.CornerSE);
                                            else if (_editor.SelectedSectors.Arrow == EditorArrowType.CornerNW)
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.CornerNE);
                                            else
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.CornerNW);
                                        }
                                        else
                                        {
                                            if (_editor.SelectedSectors.Arrow == EditorArrowType.EdgeW)
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.EntireFace);
                                            else if (_editor.SelectedSectors.Arrow == EditorArrowType.EdgeS)
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.EdgeW);
                                            else if (_editor.SelectedSectors.Arrow == EditorArrowType.EdgeE)
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.EdgeS);
                                            else if (_editor.SelectedSectors.Arrow == EditorArrowType.EdgeN)
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.EdgeE);
                                            else
                                                _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(EditorArrowType.EdgeN);

                                        }
                                    }
                                    else
                                    {
                                        // Select rectangle
                                        _editor.SelectedSectors = new SectorSelection { Start = pos, End = pos };
                                        _doSectorSelection = true;
                                    }
                                }
                                break;

                            case EditorMode.FaceEdit:

                                // Do texturing
                                if (newPicking is PickingResultBlock)
                                {
                                    var newBlockPicking = (PickingResultBlock)newPicking;
                                    if (Control.ModifierKeys == Keys.Control)
                                        EditorActions.FlipTexture(_editor.SelectedRoom, newBlockPicking.Pos, newBlockPicking.Face);
                                    else if (Control.ModifierKeys == Keys.Shift)
                                        EditorActions.RotateTexture(_editor.SelectedRoom, newBlockPicking.Pos, newBlockPicking.Face);
                                    else
                                        EditorActions.PlaceTexture(_editor.SelectedRoom, newBlockPicking.Pos, newBlockPicking.Face);
                                }
                                break;
                        }
                        break;
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            PickingResult newPicking = DoPicking(e.X, e.Y);
            if (newPicking is PickingResultObject)
                EditorActions.EditObject(_editor.SelectedRoom, ((PickingResultObject)newPicking).ObjectInstance, this.Parent);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                return;

            int deltaX = e.X - _lastX;
            int deltaY = e.Y - _lastY;

            _lastX = e.X;
            _lastY = e.Y;

            // Right click is for camera motion
            if (e.Button == MouseButtons.Right)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float relativeDeltaX = deltaX / (float)Height;
                float relativeDeltaY = deltaY / (float)Height; 
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Zoom(-relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseZoom);
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.MoveCameraPlane(new Vector3(-relativeDeltaX, -relativeDeltaY, 0) * 
                        _editor.Configuration.Rendering3D_NavigationSpeedMouseTranslate);
                else
                    Camera.Rotate(
                        relativeDeltaX * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                        -relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);

                Invalidate();
            }
            else if (e.Button == MouseButtons.Left)
            {
                // Process gizmo
                if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(Width, Height), e.X, e.Y, Control.ModifierKeys))
                    return;
                
                // Calculate block selection
                if (_doSectorSelection)
                {
                    PickingResult newPicking = DoPicking(e.X, e.Y);
                    if (newPicking is PickingResultBlock)
                    {
                        _editor.SelectedSectors = new SectorSelection
                            {
                                Start = _editor.SelectedSectors.Start,
                                End = new SharpDX.DrawingPoint(
                                    ((PickingResultBlock)newPicking).Pos.X,
                                    ((PickingResultBlock)newPicking).Pos.Y)
                            };
                    }
                    return;
                }

                // Texture editing
                if ((_editor.Mode == EditorMode.FaceEdit) && (_editor.Action.Action == EditorActionType.None))
                {
                    PickingResultBlock newPicking = DoPicking(e.X, e.Y) as PickingResultBlock;

                    if (newPicking != null)
                        EditorActions.PlaceTexture(_editor.SelectedRoom, newPicking.Pos, newPicking.Face);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _doSectorSelection = false;
            _gizmo.SetGizmoAxis(GizmoAxis.None);
        }

        private void DoMeshPicking<T>(ref PickingResult result, Ray ray, ObjectInstance objectPtr, Mesh<T> mesh, Matrix world) where T : struct, IVertex
        {
            Vector3 center = mesh.BoundingSphere.Center;

            Vector3 min = mesh.BoundingBox.Minimum;
            Vector3 max = mesh.BoundingBox.Maximum;

            Vector4 transformedMin;
            Vector3.Transform(ref min, ref world, out transformedMin);
            Vector4 transformedMax;
            Vector3.Transform(ref max, ref world, out transformedMax);

            BoundingBox box = new BoundingBox(new Vector3(transformedMin.X, transformedMin.Y, transformedMin.Z),
                new Vector3(transformedMax.X, transformedMax.Y, transformedMax.Z));
            
            float distance;
            if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
            {
                // Now do a ray - triangle intersection test
                for (int k = 0; k < mesh.Indices.Count; k += 3)
                {
                    Vector4 transformed1;
                    Vector4 transformed2;
                    Vector4 transformed3;

                    Vector4 p1t = mesh.Vertices[mesh.Indices[k]].Position;
                    Vector4 p2t = mesh.Vertices[mesh.Indices[k + 1]].Position;
                    Vector4 p3t = mesh.Vertices[mesh.Indices[k + 2]].Position;

                    Vector4.Transform(ref p1t, ref world, out transformed1);
                    Vector4.Transform(ref p2t, ref world, out transformed2);
                    Vector4.Transform(ref p3t, ref world, out transformed3);

                    Vector3 p1 = new Vector3(transformed1.X, transformed1.Y, transformed1.Z);
                    Vector3 p2 = new Vector3(transformed2.X, transformed2.Y, transformed2.Z);
                    Vector3 p3 = new Vector3(transformed3.X, transformed3.Y, transformed3.Z);

                    if (ray.Intersects(ref p1, ref p2, ref p3, out distance) && ((result == null) || (distance < result.Distance)))
                        result = new PickingResultObject(distance, objectPtr);
                }
            }
        }

        private PickingResult DoPicking(Ray ray)
        {
            Room room = _editor.SelectedRoom;
            PickingResult result;
            float distance;

            // The gizmo has the priority because it always drawn on top
            result = _gizmo.DoPicking(ray);
            if (result != null)
                return result;

            // First check for all objects in the room
            foreach (var instance in room.Objects)
                if (instance is Light)
                {
                    BoundingSphere sphere = new BoundingSphere(instance.Position, _littleSphereRadius);
                    if (ray.Intersects(ref sphere, out distance) && ((result == null) || (distance < result.Distance)))
                        result = new PickingResultObject(distance, instance);
                }
                else if (instance is MoveableInstance)
                {
                    MoveableInstance modelInfo = (MoveableInstance)instance;
                    if (_editor?.Level?.Wad?.DirectXMoveables?.ContainsKey(modelInfo.WadObjectId) ?? false)
                    {
                        SkinnedModel model = _editor.Level.Wad.DirectXMoveables[modelInfo.WadObjectId];
                        model.BuildAnimationPose(model.Animations[0].KeyFrames[0]);

                        for (int j = 0; j < model.Meshes.Count; j++)
                        {
                            SkinnedMesh mesh = model.Meshes[j];
                            Matrix world = model.AnimationTransforms[j] *
                                           Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.RotationY)) *
                                           Matrix.Translation(modelInfo.Position);
                            DoMeshPicking(ref result, ray, instance, mesh, world);
                        }
                    }
                    else
                    {
                        BoundingBox box = new BoundingBox(modelInfo.Position - new Vector3(_littleCubeRadius), modelInfo.Position + new Vector3(_littleCubeRadius));
                        if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                }
                else if (instance is ItemInstance)
                {
                    StaticInstance modelInfo = (StaticInstance)instance;
                    if (_editor?.Level?.Wad?.DirectXStatics?.ContainsKey(modelInfo.WadObjectId) ?? false)
                    {
                        StaticModel model = _editor.Level.Wad.DirectXStatics[modelInfo.WadObjectId];

                        StaticMesh mesh = model.Meshes[0];
                        Matrix world = Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.RotationY)) *
                                       Matrix.Translation(modelInfo.Position);
                        DoMeshPicking(ref result, ray, instance, mesh, world);
                    }
                    else
                    {
                        BoundingBox box = new BoundingBox(modelInfo.Position - new Vector3(_littleCubeRadius), modelInfo.Position + new Vector3(_littleCubeRadius));
                        if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                }
                else if (instance is RoomGeometryInstance)
                {
                    BoundingBox box = ((RoomGeometryInstance)instance).Model.BoundingBox;
                    if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                        result = new PickingResultObject(distance, instance);
                }
                else
                {
                    BoundingBox box = new BoundingBox(
                        instance.Position - new Vector3(_littleCubeRadius),
                        instance.Position + new Vector3(_littleCubeRadius));
                    if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                        result = new PickingResultObject(distance, instance);
                }
            
            // Check room geometry
            for (int sx = 0; sx < room.NumXSectors; sx++)
                for (int sz = 0; sz < room.NumZSectors; sz++)
                    for (int f = 0; f < 29; f++)
                    {
                        distance = 0;
                        BlockFace face = room.Blocks[sx, sz].Faces[f];
                        if (!face.Defined)
                            continue;
                        if (room.RayIntersectsFace(ref ray, ref face, out distance))
                        {
                            if ((result == null) || (distance < result.Distance))
                                result = new PickingResultBlock(distance, new DrawingPoint(sx, sz), (BlockFaces)f);
                        }
                    }

            return result;
        }

        private PickingResult DoPicking(float x, float y)
        {
            // Get the current ViewProjection matrix
            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
            
            // First get the ray in 3D space from X, Y mouse coordinates
            Ray ray = Ray.GetPickRay((int)Math.Round(x), (int)Math.Round(y), new ViewportF(0, 0, Width, Height),
                Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position)) * viewProjection);

            return DoPicking(ray);
        }

        private void DrawDebugLines(Matrix viewProjection)
        {
            if (!_drawFlybyPath && !_drawHeightLine && !_drawRoomBoundingBox) return;

            _device.SetRasterizerState(_rasterizerWireframe);
            
            Effect solidEffect = _deviceManager.Effects["Solid"];

            Matrix model = Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            solidEffect.Parameters["SelectionEnabled"].SetValue(false);

            if (_drawHeightLine)
            {
                _device.SetVertexBuffer(_objectHeightLineVertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _objectHeightLineVertexBuffer));

                Matrix model2 = Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));

                solidEffect.Parameters["ModelViewProjection"].SetValue(model2 * viewProjection);
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, 2);
            }

            if (_drawFlybyPath)
            {
                _device.SetVertexBuffer(_flybyPathVertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _flybyPathVertexBuffer));
                
                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection);
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, _flybyPathVertexBuffer.ElementCount);
            }

            if (_drawRoomBoundingBox)
            {
                _device.SetVertexBuffer(_linesCube.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _linesCube.VertexBuffer));
                _device.SetIndexBuffer(_linesCube.IndexBuffer, false);

                float height = (_editor.SelectedRoom.GetHighestCorner() - _editor.SelectedRoom.GetLowestCorner());

                Matrix scaleMatrix = Matrix.Scaling(_editor.SelectedRoom.NumXSectors * 1024.0f / 256.0f,
                                                    height,
                                                    _editor.SelectedRoom.NumZSectors * 1024.0f / 256.0f);

                Matrix translateMatrix = Matrix.Translation(_editor.SelectedRoom.NumXSectors * 1024.0f / 2.0f,
                                                            height * 256.0f / 2.0f,
                                                            _editor.SelectedRoom.NumZSectors * 1024.0f / 2.0f);

                solidEffect.Parameters["ModelViewProjection"].SetValue(scaleMatrix * 
                                                                       _editor.SelectedRoom.Transform * 
                                                                       translateMatrix * 
                                                                       viewProjection);
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.DrawIndexed(PrimitiveType.LineList, _linesCube.IndexBuffer.ElementCount);
            }
        }

        private void BuildTriggeredByMessage(ref string message, ObjectInstance instance)
        {
            foreach (var room in _editor.Level.Rooms.Where(room => room != null))
                foreach (var trigger in room.Triggers)
                    if (trigger.TargetObj == instance)
                        message += "\nTriggered in Room " + trigger.Room + " by " + trigger + ".";
        }

        private void DrawLights(Matrix viewProjection, Room room)
        {
            if (room == null)
                return;

            _device.SetRasterizerState(_rasterizerWireframe);
            _device.SetVertexBuffer(_littleSphere.VertexBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleSphere.VertexBuffer));
            _device.SetIndexBuffer(_littleSphere.IndexBuffer, _littleSphere.IsIndex32Bits);

            Effect solidEffect = _deviceManager.Effects["Solid"];


            foreach (var light in room.Objects.OfType<Light>())
            {
                /*if (light.Type==LightType.Spot)
                {
                    //_device.SetRasterizerState(_device.RasterizerStates.CullBack);

                    _device.SetVertexBuffer(_littleWireframedCube.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleWireframedCube.VertexBuffer));
                    _device.SetIndexBuffer(_littleWireframedCube.IndexBuffer, false);

                    Effect effect = _deviceManager.Effects["Solid"];

                    //effect.Parameters["TextureEnabled"].SetValue(true);
                    //effect.Parameters["Texture"].SetResource<Texture2D>(_deviceManager.Textures["spotlight"]);

                    effect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 0.25f, 1.0f));

                    Matrix model = Matrix.Translation(light.Position) * Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.Level.Rooms[room].Position));
                    effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                      
                    effect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.LineList, 49);

                    continue;
                }*/

                Matrix model = Matrix.Translation(light.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);

                if (light.Type == LightType.Light)
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
                    solidEffect.Parameters["SelectionEnabled"].SetValue(true);
                else
                    solidEffect.Parameters["SelectionEnabled"].SetValue(false);

                solidEffect.CurrentTechnique.Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
            }

            solidEffect.Parameters["SelectionEnabled"].SetValue(false);

            if (_editor.SelectedObject is Light)
            {
                Light light = (Light)_editor.SelectedObject;

                if (light.Type == LightType.Light || light.Type == LightType.Shadow || light.Type == LightType.FogBulb)
                {
                    _device.SetVertexBuffer(_sphere.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
                    _device.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);

                    if (light.Type == LightType.Light || light.Type == LightType.Shadow)
                    {
                        Matrix model = Matrix.Scaling(light.In * 2.0f) * Matrix.Translation(light.Position) *
                                       Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                        solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                        solidEffect.CurrentTechnique.Passes[0].Apply();
                        _device.DrawIndexed(PrimitiveType.TriangleList,
                            _littleSphere.IndexBuffer.ElementCount);
                    }

                    if (light.Type == LightType.Light || light.Type == LightType.Shadow ||
                        light.Type == LightType.FogBulb)
                    {
                        Matrix model = Matrix.Scaling(light.Out * 2.0f) * Matrix.Translation(light.Position) *
                                       Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                        solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                        solidEffect.CurrentTechnique.Passes[0].Apply();
                        _device.DrawIndexed(PrimitiveType.TriangleList,
                            _littleSphere.IndexBuffer.ElementCount);
                    }
                }
                else if (light.Type == LightType.Spot)
                {
                    _device.SetVertexBuffer(_cone.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _device.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    // Inner cone
                    float coneAngle = (float)Math.Atan2(512, 1024);
                    float lenScaleH = light.Len;
                    float lenScaleW = MathUtil.DegreesToRadians(light.In) / coneAngle * lenScaleH;

                    Matrix rotation = Matrix.RotationAxis(-Vector3.UnitX, MathUtil.DegreesToRadians(light.RotationX)) *
                                      Matrix.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(light.RotationY));
                    Matrix Model = Matrix.Scaling(lenScaleW, lenScaleW, lenScaleH) * rotation *
                                   Matrix.Translation(light.Position) *
                                   Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                    solidEffect.Parameters["ModelViewProjection"].SetValue(Model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);

                    // Outer cone
                    float cutoffScaleH = light.Cutoff;
                    float cutoffScaleW = MathUtil.DegreesToRadians(light.Out) / coneAngle * cutoffScaleH;

                    Matrix model2 = Matrix.Scaling(cutoffScaleW, cutoffScaleW, cutoffScaleH) * rotation *
                                    Matrix.Translation(light.Position) *
                                    Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model2 * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }
                else if (light.Type == LightType.Sun)
                {
                    _device.SetVertexBuffer(_cone.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _device.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    Matrix rotation = Matrix.RotationAxis(-Vector3.UnitX, MathUtil.DegreesToRadians(light.RotationX)) *
                                      Matrix.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(light.RotationY));

                    Matrix model = Matrix.Scaling(0.01f, 0.01f, 1.0f) * rotation * Matrix.Translation(light.Position) *
                                   Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                string message = light.ToString();
                
                // Object position
                message += "\n" + GetObjectPositionString(room, light.Position);

                Matrix modelViewProjection =
                    Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                    viewProjection;
                Vector3 screenPos = Vector3.Project(light.Position, 0, 0, Width, Height,
                    _device.Viewport.MinDepth,
                    _device.Viewport.MaxDepth, modelViewProjection);
                Debug.AddString(message, screenPos);

                // Add the line height of the object
                AddObjectHeightLine(viewProjection, room, light.Position);
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawObjects(Matrix viewProjection, Room room)
        {
            Effect effect = _deviceManager.Effects["Solid"];

            _device.SetVertexBuffer(_littleCube.VertexBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleCube.VertexBuffer));
            _device.SetIndexBuffer(_littleCube.IndexBuffer, _littleCube.IsIndex32Bits);

            foreach (var instance in room.Objects.OfType<CameraInstance>())
            {
                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                var color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                if (_editor.SelectedObject == instance)
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _device.SetRasterizerState(_rasterizerWireframe);

                    string message = instance.ToString();

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance.Position);

                    Matrix modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    BuildTriggeredByMessage(ref message, instance);

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));

                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
            {
                _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                Vector4 color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                if (_editor.SelectedObject == instance)
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _device.SetRasterizerState(_rasterizerWireframe);

                    string message = instance.ToString();

                    FlybyCameraInstance flyby = (FlybyCameraInstance)instance;

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance.Position);

                    var modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    BuildTriggeredByMessage(ref message, instance);

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);

                    // Add the path of the flyby
                    AddFlybyPath(flyby.Sequence);
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            foreach (var instance in room.Objects.OfType<SinkInstance>())
            {
                _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                Vector4 color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                if (_editor.SelectedObject == instance)
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _device.SetRasterizerState(_rasterizerWireframe);

                    var message = instance.ToString();

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance.Position);

                    var modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    BuildTriggeredByMessage(ref message, instance);

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            foreach (var instance in room.Objects.OfType<SoundSourceInstance>())
            {
                _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                Vector4 color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                if (_editor.SelectedObject == instance)
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _device.SetRasterizerState(_rasterizerWireframe);

                    SoundSourceInstance sound = (SoundSourceInstance) instance;

                    string message = instance.ToString();
                    if ((sound.SoundId >= 0) && (sound.SoundId < (_editor.Level?.Wad?.OriginalWad?.Sounds?.Count ?? 0)))
                        message += " (" + _editor.Level.Wad.OriginalWad.Sounds[sound.SoundId] + ") ";
                    else
                        message += " ( Invalid sound ) ";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance.Position);

                    Matrix modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    BuildTriggeredByMessage(ref message, instance);

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            if (_editor.SelectedRoom != null)
            {
                foreach (var instance in room.Objects.OfType<MoveableInstance>())
                {
                    if (_editor?.Level?.Wad?.DirectXMoveables?.ContainsKey(instance.WadObjectId) ?? false)
                        continue;
                    _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                    Vector4 color = new Vector4(0.2f, 0.2f, 0.5f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(0.5f, 0.2f, 0.2f, 1.0f);
                        _device.SetRasterizerState(_rasterizerWireframe);

                        string message = instance.ToString();
                        message += "\nUnavailable " + instance.ItemType.ToString();

                        // Object position
                        message += "\n" + GetObjectPositionString(room, instance.Position);

                        var modelViewProjection =
                            Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                            viewProjection;
                        Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                            _device.Viewport.MinDepth,
                            _device.Viewport.MaxDepth, modelViewProjection);

                        BuildTriggeredByMessage(ref message, instance);

                        Debug.AddString(message, screenPos);

                        // Add the line height of the object
                        AddObjectHeightLine(viewProjection, room, instance.Position);
                    }

                    Matrix model = Matrix.Translation(instance.Position) *
                                   Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                    effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }

                foreach (var instance in room.Objects.OfType<StaticInstance>())
                {
                    if (_editor?.Level?.Wad?.DirectXStatics?.ContainsKey(instance.WadObjectId) ?? false)
                        continue;

                    _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                    Vector4 color = new Vector4(0.2f, 0.2f, 0.5f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(0.5f, 0.2f, 0.2f, 1.0f);
                        _device.SetRasterizerState(_rasterizerWireframe);

                        string message = instance.ToString();
                        message += "\nUnavailable " + instance.ItemType.ToString();
                        
                        // Add the line height of the object
                        AddObjectHeightLine(viewProjection, room, instance.Position);
                    }

                    Matrix model = Matrix.Translation(instance.Position) *
                                   Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                    effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }
            }
            
            _device.SetVertexBuffer(_cone.VertexBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
            _device.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);
            _device.SetRasterizerState(_rasterizerWireframe);

            foreach (var flyby in room.Objects.OfType<FlybyCameraInstance>())
            {
                // Outer cone
                float coneAngle = (float) Math.Atan2(512, 1024);
                float cutoffScaleH = 1;
                float cutoffScaleW = MathUtil.DegreesToRadians(flyby.Fov / 2) / coneAngle * cutoffScaleH;

                Matrix rotation = Matrix.RotationAxis(-Vector3.UnitX, flyby.GetRotationXRadians()) * Matrix.RotationAxis(Vector3.UnitY, flyby.GetRotationYRadians());

                Matrix model = Matrix.Scaling(cutoffScaleW, cutoffScaleW, cutoffScaleH) * rotation * Matrix.Translation(flyby.Position) * Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                effect.CurrentTechnique.Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawMoveables(Matrix viewProjection)
        {
            _device.SetBlendState(_device.BlendStates.Opaque);

            Effect skinnedModelEffect = _deviceManager.Effects["Model"];
            
            MoveableInstance _lastObject = null;

            skinnedModelEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

            foreach (var instance in _moveablesToDraw)
            {
                if (!(_editor?.Level?.Wad?.DirectXMoveables?.ContainsKey(instance.WadObjectId) ?? false))
                    continue;
                SkinnedModel model = _editor.Level.Wad.DirectXMoveables[instance.WadObjectId];
                Debug.NumMoveables++;

                Room room = instance.Room;

                if (_lastObject == null || instance.WadObjectId != _lastObject.WadObjectId)
                {
                    _device.SetVertexBuffer(0, model.VertexBuffer);
                    _device.SetIndexBuffer(model.IndexBuffer, true);
                }

                if (_lastObject == null)
                {
                    _device.SetVertexInputLayout(
                        VertexInputLayout.FromBuffer<SkinnedVertex>(0, model.VertexBuffer));
                }

                if (_editor.SelectedObject == instance)
                    skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(true);
                else
                    skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(false);
                skinnedModelEffect.Parameters["Color"].SetValue(GetSharpdDXColor(_editor.Mode == EditorMode.Lighting ? instance.Color : System.Drawing.Color.Gray));

                Matrix world = Matrix.Identity;
                Matrix worldDebug = Matrix.Identity;

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    SkinnedMesh mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    var theRoom = instance.Room;

                    world = model.AnimationTransforms[i] * Matrix.RotationY(MathUtil.DegreesToRadians(instance.RotationY)) *
                                Matrix.Translation(instance.Position) * Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));
                    worldDebug = Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));

                    skinnedModelEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);

                    skinnedModelEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                    Debug.NumVertices += mesh.NumIndices;
                    Debug.NumTriangles += mesh.NumIndices / 3;
                }

                if (_editor.SelectedObject == instance)
                {
                    Matrix modelViewProjection = worldDebug * viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position + 512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    string message = instance.ToString();
                    message += "\n" + _editor.Level.Wad.WadMoveables[instance.WadObjectId].ToString();

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance.Position);

                    // Add OCB
                    if (instance.Ocb != 0)
                        message += "\nOCB: " + instance.Ocb;

                    BuildTriggeredByMessage(ref message, instance);

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private void DrawRoomGeometry(Matrix viewProjection)
        {
            _device.SetBlendState(_device.BlendStates.Opaque);

            Effect geometryEffect = _deviceManager.Effects["RoomGeometry"];
            
            RoomGeometryInstance _lastObject = null;

            for (int k = 0; k < RoomGeometryToDraw.Count; k++)
            {
                RoomGeometryInstance modelInfo = RoomGeometryToDraw[k];
                RoomGeometryModel model = modelInfo.Model;

                //Debug.NumMoveables++;

                Room room = modelInfo.Room;
                
                Matrix world = Matrix.Identity;
                Matrix worldDebug = Matrix.Identity;

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    RoomGeometryMesh mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    if (k == 0 && i == 0)
                    {
                        _device.SetVertexInputLayout(
                            VertexInputLayout.FromBuffer<RoomGeometryVertex>(0, mesh.VertexBuffer));
                    }

                    _device.SetVertexBuffer(0, mesh.VertexBuffer);
                    _device.SetIndexBuffer(mesh.IndexBuffer, true);

                    var theRoom = modelInfo.Room;

                    world = Matrix.Translation(modelInfo.Position) * Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));
                    worldDebug = Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));

                    geometryEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);

                    if (mesh.Texture != null)
                    {
                        geometryEffect.Parameters["TextureEnabled"].SetValue(true);
                        geometryEffect.Parameters["Texture"].SetResource(mesh.Texture);
                        geometryEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);
                    }
                    else
                    {
                        geometryEffect.Parameters["TextureEnabled"].SetValue(false);
                    }

                    geometryEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.IndexCount, mesh.BaseIndex);

                    Debug.NumVertices += mesh.NumIndices;
                    Debug.NumTriangles += mesh.NumIndices / 3;
                }

                if (_editor.SelectedObject == modelInfo)
                {
                    Matrix modelViewProjection = worldDebug * viewProjection;
                    Vector3 screenPos = Vector3.Project(modelInfo.Position + 512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    string message = modelInfo.ToString();

                    // Object position
                    message += "\n" + GetObjectPositionString(_editor.SelectedRoom, modelInfo.Position);

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, _editor.SelectedRoom, modelInfo.Position);
                }

                _lastObject = modelInfo;
            }
        }
        
        private void DrawStatics(Matrix viewProjection)
        {
            _device.SetBlendState(_device.BlendStates.Opaque);

            Effect staticMeshEffect = _deviceManager.Effects["StaticModel"];
            
            staticMeshEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
            staticMeshEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

            StaticInstance _lastObject = null;

            foreach (var instance in _staticsToDraw)
            {
                if (!(_editor?.Level?.Wad?.DirectXStatics?.ContainsKey(instance.WadObjectId) ?? false))
                    continue;
                StaticModel model = _editor.Level.Wad.DirectXStatics[instance.WadObjectId];

                if (_lastObject == null)
                {
                    _device.SetVertexInputLayout(
                        VertexInputLayout.FromBuffer<StaticVertex>(0, model.VertexBuffer));
                }

                if (_lastObject == null || instance.WadObjectId != _lastObject.WadObjectId)
                {
                    _device.SetVertexBuffer(0, model.VertexBuffer);
                    _device.SetIndexBuffer(model.IndexBuffer, true);
                }

                Debug.NumStaticMeshes++;

                bool SelectionEnabled = _editor.SelectedObject == instance;
                staticMeshEffect.Parameters["SelectionEnabled"].SetValue(SelectionEnabled);
                staticMeshEffect.Parameters["Color"].SetValue(GetSharpdDXColor(_editor.Mode == EditorMode.Lighting ? instance.Color : System.Drawing.Color.Gray));
                
                Matrix world = Matrix.Identity;
                Matrix worldDebug = Matrix.Identity;

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var theRoom = instance.Room;

                    StaticMesh mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    world = Matrix.RotationY(MathUtil.DegreesToRadians(instance.RotationY)) *
                            Matrix.Translation(instance.Position) *
                            Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));
                    worldDebug = Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));

                    staticMeshEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);

                    staticMeshEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                    Debug.NumVertices += mesh.NumIndices;
                    Debug.NumTriangles += mesh.NumIndices / 3;
                }

                if (_editor.SelectedObject == instance)
                {
                    Matrix modelViewProjection = worldDebug * viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position + 512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    string message = instance.ToString();
                    message += "\n" + _editor.Level.Wad.WadStatics[instance.WadObjectId].ToString();

                    // Object position
                    message += "\n" + GetObjectPositionString(_editor.SelectedRoom, instance.Position);

                    BuildTriggeredByMessage(ref message, instance);

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, _editor.SelectedRoom, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private void DrawSkyBox(Matrix viewProjection)
        {
            if (_editor?.Level?.Wad == null)
                return;
            if (!_editor.Level.Wad.WadMoveables.ContainsKey(459))
                return;

            _device.SetBlendState(_device.BlendStates.Opaque);

            Effect skinnedModelEffect = _deviceManager.Effects["Model"];

            skinnedModelEffect.Parameters["TextureEnabled"].SetValue(true);
            skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(false);

            SkinnedModel skinnedModel = _editor.Level.Wad.DirectXMoveables[459];
            skinnedModel.BuildAnimationPose(skinnedModel.Animations[0].KeyFrames[0]);

            skinnedModelEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<SkinnedVertex>(0, skinnedModel.VertexBuffer));

            _device.SetVertexBuffer(0, skinnedModel.VertexBuffer);
            _device.SetIndexBuffer(skinnedModel.IndexBuffer, true);

            for (int i = 0; i < skinnedModel.Meshes.Count; i++)
            {
                SkinnedMesh mesh = skinnedModel.Meshes[i];

                Matrix modelMatrix = Matrix.Scaling(16.0f) * skinnedModel.AnimationTransforms[i] * Matrix.Translation(viewProjection.TranslationVector - new Vector3(0, -5120, 0));
                skinnedModelEffect.Parameters["ModelViewProjection"].SetValue(modelMatrix * viewProjection);

                skinnedModelEffect.Techniques[0].Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                Debug.NumVertices += mesh.NumIndices;
                Debug.NumTriangles += mesh.NumIndices / 3;
            }
        }

        private Ray ConvertMouseToRay(Vector2 mousePosition)
        {
            Vector3 nearPoint = new Vector3(mousePosition, 0);
            Vector3 farPoint = new Vector3(mousePosition, 1);
            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);

            nearPoint = _device.Viewport.Unproject(nearPoint, viewProjection, Matrix.Identity, Matrix.Identity);
            farPoint = _device.Viewport.Unproject(farPoint, viewProjection, Matrix.Identity, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        private static Vector4 GetSharpdDXColor(System.Drawing.Color color)
        {
            return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, 1.0f);
        }

        private void CollectObjectsToDraw()
        {
            _moveablesToDraw.Clear();
            _staticsToDraw.Clear();
            RoomGeometryToDraw = new List<RoomGeometryInstance>();

            for (int i = 0; i < _roomsToDraw.Count; i++)
            {
                _moveablesToDraw.AddRange(_roomsToDraw[i].Objects.OfType<MoveableInstance>());
                _staticsToDraw.AddRange(_roomsToDraw[i].Objects.OfType<StaticInstance>());
                RoomGeometryToDraw.AddRange(_roomsToDraw[i].Objects.OfType<RoomGeometryInstance>());
            }

            _moveablesToDraw.Sort(new ComparerMoveables(_editor.Level.Rooms));
            _staticsToDraw.Sort(new ComparerStaticMeshes(_editor.Level.Rooms));
        }

        private void CollectRoomsToDraw(Room room)
        {
            // New iterative version of the function 

            Vector3 cameraPosition = Camera.GetPosition();

            Stack<Room> stackRooms = new Stack<Room>();
            Stack<int> stackLimits = new Stack<int>();
            HashSet<Room> visitedRooms = new HashSet<Room>();

            stackRooms.Push(room);
            stackLimits.Push(0);

            bool isFlipped = (room.Flipped && room.AlternateBaseRoom != null);

            while (stackRooms.Count > 0)
            {
                var theRoom = stackRooms.Pop();
                int theLimit = stackLimits.Pop();

                if (theLimit > _editor.Configuration.Rendering3D_DrawRoomsMaxDepth)
                    continue;

                /*
                visitedRooms.Add(theRoom);

                if (theRoom == room)
                {
                    _roomsToDraw.Add(theRoom);
                }
                else
                {
                    if (isFlipped && theRoom.Flipped && theRoom.AlternateRoom != null)
                    {
                        _roomsToDraw.Add(theRoom.AlternateRoom);
                    }
                    else
                    {
                        _roomsToDraw.Add(theRoom);
                    }
                }*/

                if (isFlipped)
                {
                    if (!theRoom.Flipped)
                    {
                        visitedRooms.Add(theRoom);
                        if (!_roomsToDraw.Contains(theRoom)) _roomsToDraw.Add(theRoom);
                    }
                    else
                    {
                        if (theRoom.AlternateRoom != null)
                        {
                            visitedRooms.Add(theRoom);
                            if (!_roomsToDraw.Contains(theRoom.AlternateRoom)) _roomsToDraw.Add(theRoom.AlternateRoom);
                        }
                        else
                        {
                            visitedRooms.Add(theRoom);
                            if (!_roomsToDraw.Contains(theRoom)) _roomsToDraw.Add(theRoom);
                        }
                    }
                }
                else
                {
                    visitedRooms.Add(theRoom);
                    if (!_roomsToDraw.Contains(theRoom)) _roomsToDraw.Add(theRoom);
                }

                foreach (var portal in theRoom.Portals)
                {
                    Vector3 normal = Vector3.Zero;

                    if (portal.Direction == PortalDirection.North)
                        normal = -Vector3.UnitZ;
                    if (portal.Direction == PortalDirection.East)
                        normal = -Vector3.UnitX;
                    if (portal.Direction == PortalDirection.South)
                        normal = Vector3.UnitZ;
                    if (portal.Direction == PortalDirection.West)
                        normal = Vector3.UnitX;
                    if (portal.Direction == PortalDirection.Floor)
                        normal = Vector3.UnitY;
                    if (portal.Direction == PortalDirection.Ceiling)
                        normal = -Vector3.UnitY;

                    Vector3 cameraDirection = cameraPosition - Camera.Target;

                    Vector3 v1 = Vector3.UnitX;
                    Vector3 v2 = Vector3.UnitZ;
                    float dot = Vector3.Dot(v1, v2);

                    if (Vector3.Dot(normal, cameraDirection) < -0.1f && theLimit > 1)
                        continue;

                    if (/*portal.Room == theRoom &&*/ !visitedRooms.Contains(portal.AdjoiningRoom) &&
                        !stackRooms.Contains(portal.AdjoiningRoom))
                    {
                        stackRooms.Push(portal.AdjoiningRoom);
                        stackLimits.Push(theLimit + 1);
                    }
                }
            }
        }

        private void PrepareRenderBuckets(Matrix viewProjection)
        {
            _opaqueBuckets = new List<RenderBucket>();
            _transparentBuckets = new List<RenderBucket>();
            _invisibleBuckets = new List<RenderBucket>();
            _solidBuckets = new List<RenderBucket>();

            BlockFace face;
            Vector3 cameraPosition = Camera.GetPosition();
            Vector3 viewVector = Camera.Target - cameraPosition;

            // Build buckets
            for (int i = 0; i < _roomsToDraw.Count; i++)
            {
                var room = _roomsToDraw[i];

                for (int x = 0; x < room.NumXSectors; x++)
                {
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int f = 0; f < room.Blocks[x, z].Faces.Count(); f++)
                        {
                            face = room.Blocks[x, z].Faces[f];

                            //float dot = Vector3.Dot(-viewVector, face.Plane.Normal);
                            //if (dot < 0.01f) continue;

                            if (face.Defined)
                            {
                                if ((_editor.Mode == EditorMode.Geometry && room == _editor.SelectedRoom) ||
                                    (face.Texture == -1 && !face.Invisible) ||
                                    (_editor.Level.DirectXTextures.Count == 0))
                                {
                                    RenderBucket bucket = new RenderBucket
                                    {
                                        FaceType = (BlockFaces) f,
                                        Face = face,
                                        X = x,
                                        Z = z,
                                        Room = room
                                    };

                                    _solidBuckets.Add(bucket);
                                }
                                else
                                {
                                    if (face.Invisible)
                                    {
                                        RenderBucket bucket = new RenderBucket
                                        {
                                            FaceType = (BlockFaces) f,
                                            Face = face,
                                            X = x,
                                            Z = z,
                                            Invisible = (byte) (face.Invisible ? 1 : 0),
                                            Texture = face.Texture,
                                            Room = room
                                        };

                                        _invisibleBuckets.Add(bucket);
                                    }
                                    else if (face.Texture == -1)
                                    {
                                        if (_editor.Mode == EditorMode.FaceEdit && room == _editor.SelectedRoom)
                                        {
                                            RenderBucket bucket = new RenderBucket
                                            {
                                                FaceType = (BlockFaces) f,
                                                Face = face,
                                                X = x,
                                                Z = z,
                                                Room = room
                                            };

                                            _solidBuckets.Add(bucket);
                                        }
                                    }
                                    else
                                    {
                                        LevelTexture texture = _editor.Level.TextureSamples[face.Texture];

                                        if (!face.Transparent && !face.Invisible && !texture.AlphaTest)
                                        {
                                            int found = -1;
                                            for (int b = 0; b < _opaqueBuckets.Count; b++)
                                            {
                                                RenderBucket currentBucket = _opaqueBuckets[b];
                                                if (currentBucket.Room == room && currentBucket.DoubleSided ==
                                                    (face.DoubleSided ? 1 : 0))
                                                {
                                                    found = b;
                                                    break;
                                                }
                                            }

                                            if (found == -1)
                                            {
                                                RenderBucket bucket = new RenderBucket
                                                {
                                                    DoubleSided = (byte) (face.DoubleSided ? 1 : 0),
                                                    Room = room
                                                };

                                                _opaqueBuckets.Add(bucket);

                                                found = _opaqueBuckets.Count - 1;
                                            }

                                            _opaqueBuckets[found].Indices
                                                .AddRange(face.IndicesForSolidBucketsRendering);
                                        }
                                        else if (face.Invisible)
                                        {
                                            RenderBucket bucket = new RenderBucket
                                            {
                                                FaceType = (BlockFaces) f,
                                                Face = face,
                                                X = x,
                                                Z = z,
                                                Invisible = (byte) (1),
                                                Texture = face.Texture,
                                                Room = room
                                            };

                                            _invisibleBuckets.Add(bucket);
                                        }
                                        else if (face.Transparent || texture.AlphaTest)
                                        {
                                            RenderBucket bucket = new RenderBucket
                                            {
                                                FaceType = (BlockFaces)f,
                                                Face = face,
                                                X = x,
                                                Z = z,
                                                DoubleSided = (byte)(face.DoubleSided ? 1 : 0),
                                                Texture = face.Texture,
                                                Room = room,
                                                Plane = face.Plane,
                                                AlphaTest = (byte)(face.Transparent ? 0 : 1)
                                            };

                                            // calcolo il piano passante per la faccia

                                            // calcolo il centro della faccia
                                            Vector4 center = Vector4.Zero;

                                            for (int j = 0; j < face.Vertices.Length; j++)
                                            {
                                                center += face.Vertices[j].Position;
                                            }

                                            center /= face.Vertices.Length;
                                            bucket.Center = new Vector3(center.X, center.Y, center.Z);

                                            // calcolo la distanza
                                            bucket.Distance = (bucket.Center - cameraPosition).Length();

                                            // aggiungo la struttura alla lista
                                            _transparentBuckets.Add(bucket);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Sort buckets
            _opaqueBuckets.Sort(new ComparerOpaqueBuckets(_editor.Level.Rooms));
            _transparentBuckets.Sort(new ComparerTransparentBuckets(_editor.Level.Rooms));
            _invisibleBuckets.Sort(new ComparerInvisibleBuckets(_editor.Level.Rooms));

            Parallel.ForEach(_opaqueBuckets, PrepareIndexBuffer);
        }

        private void PrepareIndexBuffer(RenderBucket item)
        {
            item.IndexBuffer = SharpDX.Toolkit.Graphics.Buffer.New(_device,
                item.Indices.ToArray(), BufferFlags.IndexBuffer);
        }

        // Do NOT call this method to redraw the scene!
        // Call Invalidate() instead to schedule a redraw in the message loop.
        private void Draw()
        {
            if (DesignMode)
                return;

            Stopwatch _watch = new Stopwatch();
            _watch.Start();

            // Reset gizmo and debug strings and lines
            Debug.Reset();
            _drawHeightLine = false;
            _drawFlybyPath = false;
            _drawRoomBoundingBox = ((_editor.Mode == EditorMode.FaceEdit || _editor.Mode == EditorMode.Lighting) && DrawPortals);

            // Don't draw anything if device is not ready
            if (_device == null || _editor.SelectedRoom == null)
                return;

            // reset the backbuffer
            bool IsFlipMap = (_editor?.SelectedRoom?.AlternateBaseRoom != null);
            _device.Presenter = _presenter;
            _device.SetViewports(new ViewportF(0, 0, Width, Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer,
                _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target,
                IsFlipMap ? Color.LightCoral : Color.CornflowerBlue, 1.0f, 0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);

            // verify that editor is ready
            if (_editor.Level == null)
                return;
            if (_editor.Level.Rooms.Length == 0)
                return;
            if (_editor.SelectedRoom == null)
                return;
            if (_editor.SelectedRoom.VertexBuffer == null)
                return;

            //Precalculate the view projection matrix
            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);

            // First collect rooms to draw
            _roomsToDraw = new List<Room>();

            // Collect rooms to draw
            if (DrawPortals)
                CollectRoomsToDraw(_editor.SelectedRoom);
            else
                _roomsToDraw.Add(_editor.SelectedRoom);

            Debug.NumRooms = _roomsToDraw.Count;

            Task task1 = Task.Factory.StartNew(RenderTask1, viewProjection);
            Task task2 = Task.Factory.StartNew(RenderTask2, viewProjection);
            Task.WaitAll(task1, task2);

            // Draw the skybox if present
            if (_editor != null && _editor.Level != null && _editor.Level.Wad != null && DrawHorizon)
            {
                DrawSkyBox(viewProjection);
                _device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1.0f, 0);
            }

            // Draw moveables and static meshes
            if (_editor != null && _editor.Level != null && _editor.Level.Wad != null)
            {
                DrawMoveables(viewProjection);
                DrawStatics(viewProjection);
            }

            // Draw room geometry imported
            if (_editor != null && _editor.Level != null)
            {
                DrawRoomGeometry(viewProjection);
            }

            // Set some common parameters of the shader
            _roomEffect = _deviceManager.Effects["Room"];
            _roomEffect.Parameters["CameraPosition"].SetValue(viewProjection.TranslationVector);
            _roomEffect.Parameters["LightingEnabled"].SetValue(false);
            _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
            _roomEffect.Parameters["TextureEnabled"].SetValue(false);
            _roomEffect.Parameters["EditorTextureEnabled"].SetValue(false);
            _roomEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);

            // Draw buckets
            DrawSolidBuckets(viewProjection);

            DrawSelectedFogBulb();

            DrawOpaqueBuckets(viewProjection);
            DrawTransparentBuckets(viewProjection);
            DrawInvisibleBuckets(viewProjection);

            // Draw objects (sinks, cameras, fly-by cameras and sound sources) only for current room
            DrawObjects(viewProjection, _roomsToDraw[0]);

            // Draw light objects and bounding volumes only for current room
            DrawLights(viewProjection, _editor.SelectedRoom);

            // Draw the height of the object
            DrawDebugLines(viewProjection);

            // Draw the gizmo
            _gizmo.Draw(viewProjection);

            _watch.Stop();
            long mils = _watch.ElapsedMilliseconds;
            float fps = (mils != 0 ? 1000 / mils : 60);

            Debug.Fps = fps;

            // Draw debug info
            Debug.Draw(_deviceManager, _editor.SelectedObject?.ToString());

            _device.Present();

            logger.Debug($"Draw Call! {mils}ms");
        }

        private void DrawSelectedFogBulb()
        {
            Light light = _editor.SelectedObject as Light;
            if (light != null)
            {
                if (light.Type == LightType.FogBulb)
                {
                    _roomEffect.Parameters["FogBulbEnabled"].SetValue(true);
                    _roomEffect.Parameters["FogBulbIntensity"].SetValue(light.Intensity);
                    _roomEffect.Parameters["FogBulbPosition"].SetValue(Vector3.Transform(light.Position, _editor.SelectedRoom.Transform));
                    _roomEffect.Parameters["FogBulbRadius"].SetValue(light.Out * 1024.0f);
                }
                else
                {
                    _roomEffect.Parameters["FogBulbEnabled"].SetValue(false);
                }
            }
            else
            {
                _roomEffect.Parameters["FogBulbEnabled"].SetValue(false);
            }
        }

        private void RenderTask1(object viewProjection_)
        {
            Matrix viewProjection = (Matrix)viewProjection_;

            // Collect objects to draw
            CollectObjectsToDraw();

            // Now group faces to render based on various things
            PrepareRenderBuckets(viewProjection);
        }

        private void RenderTask2(object viewProjection_)
        {
            Matrix viewProjection = (Matrix)viewProjection_;

            // Add room names
            if (DrawRoomNames)
            {
                AddRoomNamesToDebug(viewProjection);
            }

            // Draw North, South, East and West
            AddDirectionsToDebug(viewProjection);
        }

        private void AddRoomNamesToDebug(Matrix viewProjection)
        {
            for (int i = 0; i < _roomsToDraw.Count; i++)
            {
                string message = _roomsToDraw[i].Name ?? ("Room " + _editor.Level.Rooms.ReferenceIndexOf(_roomsToDraw[i]));

                Vector3 pos = _roomsToDraw[i].Position;
                Matrix wvp = Matrix.Translation(Utils.PositionInWorldCoordinates(pos)) * viewProjection;
                Vector3 screenPos = Vector3.Project(_roomsToDraw[i].GetLocalCenter(), 0, 0, Width, Height,
                    _device.Viewport.MinDepth,
                    _device.Viewport.MaxDepth, wvp);
                Debug.AddString(message, screenPos);
            }
        }

        private void AddDirectionsToDebug(Matrix viewProjection)
        {
            float xBlocks = _editor.SelectedRoom.NumXSectors / 2.0f * 1024.0f;
            float zBlocks = _editor.SelectedRoom.NumZSectors / 2.0f * 1024.0f;

            string[] messages = {"North", "South", "East", "West"};
            Vector3[] positions = new Vector3[4];

            Vector3 center = _editor.SelectedRoom.GetLocalCenter();
            Vector3 pos = Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position);

            positions[0] = center + new Vector3(0, 0, zBlocks);
            positions[1] = center + new Vector3(0, 0, -zBlocks);
            positions[2] = center + new Vector3(xBlocks, 0, 0);
            positions[3] = center + new Vector3(-xBlocks, 0, 0);

            Matrix wvp = Matrix.Translation(pos) * viewProjection;

            for (int i = 0; i < 4; i++)
            {
                Vector3 screenPos = Vector3.Project(positions[i], 0, 0, Width, Height,
                    _device.Viewport.MinDepth,
                    _device.Viewport.MaxDepth, wvp);
                Debug.AddString(messages[i], screenPos);
            }
        }

        private void DrawOpaqueBuckets(Matrix viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(true);
            _roomEffect.Parameters["EditorTextureEnabled"].SetValue(false);
            _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
            _roomEffect.Parameters["InvisibleFaceEnabled"].SetValue(false);

            RenderBucket _lastBucket = null;

            // Draw opaque faces
            for (int i = 0; i < _opaqueBuckets.Count; i++)
            {
                RenderBucket bucket = _opaqueBuckets[i];

                var room = bucket.Room;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _device.SetVertexBuffer(room.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
                    _roomEffect.Parameters["ModelViewProjection"].SetValue(room.Transform * viewProjection);

                    // Enable or disable static lighting
                    bool lights = (room != _editor.SelectedRoom ||
                                   (room == _editor.SelectedRoom && _editor.Mode == EditorMode.Lighting));
                    _roomEffect.Parameters["LightingEnabled"].SetValue(lights);

                    _roomEffect.Parameters["Model"].SetValue(room.Transform);
                }

                _device.SetIndexBuffer(bucket.IndexBuffer, false);

                // Enable or disable double sided textures
                if (_lastBucket == null || _lastBucket.DoubleSided != bucket.DoubleSided)
                {
                    if (bucket.DoubleSided == 1)
                        _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                    else
                        _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                }

                // Change texture if needed
                if (_lastBucket == null)
                {
                    _roomEffect.Parameters["Texture"].SetResource(_editor.Level.DirectXTextures[0]);
                    _roomEffect.Parameters["TextureSampler"]
                        .SetResource(_device.SamplerStates.AnisotropicWrap);
                }

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the face
                _device.DrawIndexed(PrimitiveType.TriangleList,
                    bucket.IndexBuffer.ElementCount); // face.Vertices.Count, face.StartVertex);

                Debug.NumVertices += bucket.IndexBuffer.ElementCount;
                Debug.NumTriangles += bucket.IndexBuffer.ElementCount / 3;

                _lastBucket = bucket;
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawInvisibleBuckets(Matrix viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(false);
            _roomEffect.Parameters["EditorTextureEnabled"].SetValue(false);
            _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
            _roomEffect.Parameters["InvisibleFaceEnabled"].SetValue(true);
            _roomEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            _roomEffect.Parameters["LightingEnabled"].SetValue(false);

            RenderBucket _lastBucket = null;

            // Draw opaque faces
            for (int i = 0; i < _invisibleBuckets.Count; i++)
            {
                RenderBucket bucket = _invisibleBuckets[i];

                var room = bucket.Room;
                int x = bucket.X;
                int z = bucket.Z;
                int index = (int)bucket.FaceType;
                BlockFace face = bucket.Face;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _device.SetVertexBuffer(room.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
                    _roomEffect.Parameters["ModelViewProjection"].SetValue(room.Transform * viewProjection);
                }

                // Set shape
                if (face.Shape == BlockFaceShape.Rectangle)
                    _roomEffect.Parameters["Shape"].SetValue(0);
                else
                    _roomEffect.Parameters["Shape"].SetValue(1);

                _roomEffect.Parameters["SplitMode"].SetValue(face.SplitMode);

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the face
                _device.Draw(PrimitiveType.TriangleList, face.Vertices.Length, face.StartVertex);

                Debug.NumVertices += face.IndicesForSolidBucketsRendering.Count;
                Debug.NumTriangles += face.Vertices.Length / 3;

                _lastBucket = bucket;
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawTransparentBuckets(Matrix viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(true);
            _roomEffect.Parameters["EditorTextureEnabled"].SetValue(false);
            _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
            _roomEffect.Parameters["InvisibleFaceEnabled"].SetValue(false);

            RenderBucket _lastBucket = null;

            // Draw opaque faces
            for (int i = 0; i < _transparentBuckets.Count; i++)
            {
                RenderBucket bucket = _transparentBuckets[i];

                var room = bucket.Room;
                int x = bucket.X;
                int z = bucket.Z;
                int index = (int)bucket.FaceType;
                BlockFace face = bucket.Face;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _device.SetVertexBuffer(room.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
                    _roomEffect.Parameters["ModelViewProjection"].SetValue(room.Transform * viewProjection);

                    // Enable or disable static lighting
                    bool lights = (room != _editor.SelectedRoom ||
                                   (room == _editor.SelectedRoom && _editor.Mode == EditorMode.Lighting));
                    _roomEffect.Parameters["LightingEnabled"].SetValue(lights);

                    _roomEffect.Parameters["Model"].SetValue(room.Transform);
                }

                // Enable or disable double sided textures
                if (_lastBucket == null || _lastBucket.DoubleSided != bucket.DoubleSided)
                {
                    if (bucket.DoubleSided == 1)
                        _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                    else
                        _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                }

                // Check if is alpha trasparency or magenta trasparency
                if (_lastBucket == null || _lastBucket.AlphaTest != bucket.AlphaTest)
                {
                    if (bucket.AlphaTest == 1)
                        _device.SetBlendState(_device.BlendStates.AlphaBlend);
                    else
                        _device.SetBlendState(_device.BlendStates.Additive);
                }

                // Change texture if needed
                if (_lastBucket == null /*|| _lastBucket.Texture!=bucket.Texture*/)
                {
                    LevelTexture textureSample = _editor.Level.TextureSamples[face.Texture];
                    _roomEffect.Parameters["Texture"].SetResource(_editor.Level.DirectXTextures[0 /*textureSample.Page*/]);
                    _roomEffect.Parameters["TextureSampler"]
                        .SetResource(_device.SamplerStates.AnisotropicWrap);
                }

                // Set shape
                if (face.Shape == BlockFaceShape.Rectangle)
                    _roomEffect.Parameters["Shape"].SetValue(0);
                else
                    _roomEffect.Parameters["Shape"].SetValue(1);

                _roomEffect.Parameters["SplitMode"].SetValue(face.SplitMode);

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the face
                _device.Draw(PrimitiveType.TriangleList, face.Vertices.Length, face.StartVertex);

                Debug.NumVertices += face.IndicesForSolidBucketsRendering.Count;
                Debug.NumTriangles += face.Vertices.Length / 3;

                _lastBucket = bucket;
            }

            _device.SetBlendState(_device.BlendStates.AlphaBlend);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawSolidBuckets(Matrix viewProjection)
        {
            RenderBucket _lastBucket = null;

            // Draw solid faces
            for (int i = 0; i < _solidBuckets.Count; i++)
            {
                RenderBucket bucket = _solidBuckets[i];
                int x = bucket.X;
                int z = bucket.Z;
                int index = (int)bucket.FaceType;
                var room = bucket.Room;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _device.SetVertexBuffer(room.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
                    _roomEffect.Parameters["ModelViewProjection"].SetValue(room.Transform * viewProjection);

                    // Enable or disable static lighting
                    bool lights = (room != _editor.SelectedRoom ||
                                   (room == _editor.SelectedRoom && _editor.Mode == EditorMode.Lighting));
                    _roomEffect.Parameters["LightingEnabled"].SetValue(lights);
                }

                // Calculate the bounds of the current selection
                int xMin = Math.Min(_editor.SelectedSectors.Start.X, _editor.SelectedSectors.End.X);
                int xMax = Math.Max(_editor.SelectedSectors.Start.X, _editor.SelectedSectors.End.X);
                int zMin = Math.Min(_editor.SelectedSectors.Start.Y, _editor.SelectedSectors.End.Y);
                int zMax = Math.Max(_editor.SelectedSectors.Start.Y, _editor.SelectedSectors.End.Y);

                bool noCollision = false;

                if (x >= xMin && x <= xMax && xMin != -1 && zMin != -1 && xMax != -1 && zMax != -1 &&
                    z >= zMin && z <= zMax)
                {
                    // sono in un'area selezionata, quindi attivo la selezione a prescindere
                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);

                    // attivo le coordinate UV dell'editor a prescindere
                    _roomEffect.Parameters["EditorTextureEnabled"].SetValue(true);

                    // applico la texture della freccia al pavimento e al soffitto
                    if (index == 25 || index == 26)
                    {
                        switch (_editor.SelectedSectors.Arrow)
                        {
                            case EditorArrowType.EntireFace:
                            default:
                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                break;

                            case EditorArrowType.EdgeN:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_right"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeS:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_left"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerNW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerNE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerSE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerSW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                        }
                    }
                    else if (index == 27 || index == 28)
                    {
                        switch (_editor.SelectedSectors.Arrow)
                        {
                            case EditorArrowType.EntireFace:
                            default:
                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                break;

                            case EditorArrowType.EdgeN:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_right"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeS:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_left"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerNW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerNE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerSE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerSW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                        }
                    }
                    else if (index == 4 || index == 9 || index == 14 || index == 19 || index == 24 ||
                             (room.Blocks[x, z].Type != BlockType.Wall &&
                              room.Blocks[x, z].Type != BlockType.BorderWall))
                    {
                        _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                        _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                    }
                    else
                    {
                        // applico le texture delle frecce ai muri
                        int qa, ed, rf, ws, middle;

                        if (z == room.NumZSectors - 1)
                        {
                            qa = (int)BlockFaces.SouthQA;
                            ws = (int)BlockFaces.SouthWS;
                            ed = (int)BlockFaces.SouthED;
                            rf = (int)BlockFaces.SouthRF;
                            middle = (int)BlockFaces.SouthMiddle;

                            switch (_editor.SelectedSectors.Arrow)
                            {
                                default:
                                case EditorArrowType.EntireFace:
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                    break;
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    break;
                                case EditorArrowType.EdgeE:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeS:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeW:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerSE:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerSW:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                            }

                            if (index == middle)
                            {
                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                            }
                        }

                        if (x == room.NumXSectors - 1)
                        {
                            qa = (int)BlockFaces.WestQA;
                            ws = (int)BlockFaces.WestWS;
                            ed = (int)BlockFaces.WestED;
                            rf = (int)BlockFaces.WestRF;
                            middle = (int)BlockFaces.WestMiddle;

                            switch (_editor.SelectedSectors.Arrow)
                            {
                                default:
                                case EditorArrowType.EntireFace:
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                    break;
                                case EditorArrowType.EdgeN:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeS:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeW:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerNW:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerSW:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                            }

                            if (index == middle)
                            {
                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                            }
                        }

                        if (z == 0)
                        {
                            qa = (int)BlockFaces.NorthQA;
                            ws = (int)BlockFaces.NorthWS;
                            ed = (int)BlockFaces.NorthED;
                            rf = (int)BlockFaces.NorthRF;
                            middle = (int)BlockFaces.NorthMiddle;

                            switch (_editor.SelectedSectors.Arrow)
                            {
                                default:
                                case EditorArrowType.EntireFace:
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                    break;
                                case EditorArrowType.EdgeN:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeE:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeW:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerNW:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerNE:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                            }

                            if (index == middle)
                            {
                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                            }
                        }

                        if (x == 0)
                        {
                            qa = (int)BlockFaces.EastQA;
                            ws = (int)BlockFaces.EastWS;
                            ed = (int)BlockFaces.EastED;
                            rf = (int)BlockFaces.EastRF;
                            middle = (int)BlockFaces.EastMiddle;

                            switch (_editor.SelectedSectors.Arrow)
                            {
                                default:
                                case EditorArrowType.EntireFace:
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                    break;
                                case EditorArrowType.EdgeN:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeE:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeS:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerNE:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);

                                    break;
                                case EditorArrowType.CornerSE:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                            }

                            if (index == middle)
                            {
                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                            }
                        }
                    }
                }
                else
                {
                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                }

                if (index < 10)
                {
                    _roomEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 80.0f / 255.0f, 0.0f, 1.0f));
                }
                else if (index >= 10 && index < 15)
                {
                    _roomEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 160.0f / 255.0f, 0.0f, 1.0f));
                }
                else if (index >= 15 && index < 25)
                {
                    _roomEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 240.0f / 255.0f, 0.0f, 1.0f));
                }
                else
                {
                    _roomEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 200.0f / 255.0f, 200.0f / 255.0f, 1.0f));

                    if ((room.Blocks[x, z].Flags & BlockFlags.DeathElectricity) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorDeath));
                    if ((room.Blocks[x, z].Flags & BlockFlags.DeathFire) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorDeath));
                    if ((room.Blocks[x, z].Flags & BlockFlags.DeathLava) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorDeath));
                    if ((room.Blocks[x, z].Flags & BlockFlags.Monkey) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorMonkey));
                    if ((room.Blocks[x, z].Flags & BlockFlags.Box) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorBox));
                    if ((room.Blocks[x, z].Flags & BlockFlags.NotWalkableFloor) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorNotWalkable));
                    if ((room.Blocks[x, z].Flags & BlockFlags.ClimbAny) != BlockFlags.None)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorClimb));
                    if ((room.Blocks[x, z].NoCollisionFloor &&
                         (index == (int) BlockFaces.Floor || index == (int) BlockFaces.FloorTriangle2)))
                    {
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorNoCollision));
                        noCollision = true;
                    }
                    if (room.Blocks[x, z].Triggers.Count != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorTrigger));
                    if ((room.Blocks[x, z].NoCollisionCeiling &&
                         (index == (int) BlockFaces.Ceiling || index == (int) BlockFaces.CeilingTriangle2)))
                    {
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorNoCollision));
                        noCollision = true;
                    }
                }

                // Portals
                if (index < 25)
                {
                    if (room.Blocks[x, z].WallPortal != null)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                if ((index == 25 || index == 26) && !noCollision)
                {
                    if (room.Blocks[x, z].FloorPortal != null)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                if ((index == 27 || index == 28) && !noCollision)
                {
                    if (room.Blocks[x, z].CeilingPortal != null)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                if (bucket.Face.Shape == BlockFaceShape.Rectangle)
                    _roomEffect.Parameters["Shape"].SetValue(0);
                else
                    _roomEffect.Parameters["Shape"].SetValue(1);

                _roomEffect.Parameters["SplitMode"].SetValue(bucket.Face.SplitMode);

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                _device.Draw(PrimitiveType.TriangleList, bucket.Face.Vertices.Length,
                    bucket.Face.StartVertex);

                Debug.NumVertices += bucket.Face.IndicesForSolidBucketsRendering.Count;
                Debug.NumTriangles += bucket.Face.Vertices.Length / 3;

                _lastBucket = bucket;
            }
        }

        private static float GetFloorHeight(Room room, Vector3 position)
        {
            int xBlock = (int)Math.Max(0, Math.Min(room.NumXSectors - 1, Math.Floor(position.X / 1024.0f)));
            int zBlock = (int)Math.Max(0, Math.Min(room.NumZSectors - 1, Math.Floor(position.Z / 1024.0f)));

            // Get the base floor height
            return room.GetLowestFloorCorner(xBlock, zBlock) * 256.0f;
        }


        private static string GetObjectPositionString(Room room, Vector3 position)
        {
            // Get the distance between point and floor in units
            float height = position.Y - GetFloorHeight(room, position);

            string message = "Position: [" + position.X + ", " + position.Y + ", " + position.Z + "]";
            message += "\nHeight: " + Math.Round(height) + " units(" + (height / 256.0f) +
                       " clicks)";

            return message;
        }

        private void AddObjectHeightLine(Matrix viewProjection, Room room, Vector3 position)
        {
            float floorHeight = GetFloorHeight(room, position);

            // Get the distance between point and floor in units
            float height = position.Y - floorHeight;

            // Prepare two vertices for the line
            EditorVertex v1 = new EditorVertex();
            v1.Position = new Vector4(position.X, floorHeight, position.Z, 1.0f);

            EditorVertex v2 = new EditorVertex();
            v2.Position = new Vector4(position.X, position.Y, position.Z, 1.0f);

            EditorVertex[] vertices = new EditorVertex[] {v1, v2};

            // Prepare the Vertex Buffer
            if (_objectHeightLineVertexBuffer != null)
                _objectHeightLineVertexBuffer.Dispose();
            _objectHeightLineVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<EditorVertex>(_device,
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
            flybyCameras.Sort(new ComparerFlybyCameras());

            // Create a vertex array
            List<EditorVertex> vertices = new List<EditorVertex>();

            for (int i = 0; i < flybyCameras.Count - 1; i++)
            {
                Vector3 room1pos = Utils.PositionInWorldCoordinates(flybyCameras[i].Room.Position);
                Vector3 room2pos = Utils.PositionInWorldCoordinates(flybyCameras[i + 1].Room.Position);

                EditorVertex v1 = new EditorVertex();
                v1.Position = new Vector4(flybyCameras[i].Position.X + room1pos.X,
                                          flybyCameras[i].Position.Y + room1pos.Y,
                                          flybyCameras[i].Position.Z + room1pos.Z,
                                          1.0f);

                EditorVertex v2 = new EditorVertex();
                v2.Position = new Vector4(flybyCameras[i + 1].Position.X + room2pos.X,
                                          flybyCameras[i + 1].Position.Y + room2pos.Y,
                                          flybyCameras[i + 1].Position.Z + room2pos.Z,
                                          1.0f);

                vertices.Add(v1);
                vertices.Add(v2);
            }
                       
            // Prepare the Vertex Buffer
            if (_flybyPathVertexBuffer != null) _flybyPathVertexBuffer.Dispose();
            _flybyPathVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<EditorVertex>(_device, vertices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _drawFlybyPath = true;
        }
    }
}
