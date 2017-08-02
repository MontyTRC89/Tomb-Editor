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

namespace TombEditor.Controls
{
    public abstract class PickingResult
    {
        public float Distance { get; set; }
    };

    public partial class PanelRendering3D : Panel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const float _rotationSpeed = 0.1f;
        private const float _keyboardZoomSpeed = 0.1f;

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

        private class ComparerMoveables : IComparer<int>
        {
            private readonly Room[] _rooms;

            public ComparerMoveables(Room[] rooms)
            {
                _rooms = rooms;
            }

            public int Compare(int x, int y)
            {
                var instanceX = (MoveableInstance)Editor.Instance.Level.Objects[x];
                var instanceY = (MoveableInstance)Editor.Instance.Level.Objects[y];

                int result = instanceX.ObjectId.CompareTo(instanceY.ObjectId);
                if (result != 0)
                    return result;
                return _rooms.ReferenceIndexOf(instanceX.Room).CompareTo(_rooms.ReferenceIndexOf(instanceY.Room));
            }
        }

        private class ComparerStaticMeshes : IComparer<int>
        {
            private readonly Room[] _rooms;

            public ComparerStaticMeshes(Room[] rooms)
            {
                _rooms = rooms;
            }

            public int Compare(int x, int y)
            {
                var instanceX = (StaticMeshInstance)Editor.Instance.Level.Objects[x];
                var instanceY = (StaticMeshInstance)Editor.Instance.Level.Objects[y];

                int result = instanceX.ObjectId.CompareTo(instanceY.ObjectId);
                if (result != 0)
                    return result;
                return _rooms.ReferenceIndexOf(instanceX.Room).CompareTo(_rooms.ReferenceIndexOf(instanceY.Room));
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
                Pos = pos;
                Face = face;
            }
        }

        private class PickingResultObject : PickingResult
        {
            public ObjectPtr ObjectPtr { get; set; }
            public PickingResultObject(float Distance,  ObjectPtr objectPtr)
            {
                ObjectPtr = objectPtr;
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
        private GeometricPrimitive _littleCube;
        private GeometricPrimitive _littleSphere;
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
        private List<int> _camerasToDraw;
        private List<int> _sinksToDraw;
        private List<int> _flybyToDraw;
        private List<int> _soundSourcesToDraw;
        private List<int> MoveablesToDraw;
        private List<int> StaticMeshesToDraw;

        // Debug lines
        private Buffer<EditorVertex> _objectHeightLineVertexBuffer;
        private bool _drawHeightLine = false;

        private Buffer<EditorVertex> _flybyPathVertexBuffer;
        private bool _drawFlybyPath = false;

        private Effect _roomEffect;

        public PanelRendering3D()
        {
            InitializeComponent();
            ResetCamera();

            _editor = Editor.Instance;
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

        public void ResetCamera()
        {
            Room room = _editor?.SelectedRoom;

            // Point the camera to the room's center
            Vector3 target = new Vector3();
            if (room != null)
                target = new Vector3(
                    room.Position.X * 1024.0f + room.NumXSectors * 512.0f,
                    room.Position.Y * 256.0f + room.Ceiling * 64.0f,
                    room.Position.Z * 1024.0f + room.NumZSectors * 512.0f);

            // Initialize a new camera
            Camera = new ArcBallCamera(target, DefaultCameraAngleX, DefaultCameraAngleY, - MathUtil.PiOverTwo, MathUtil.PiOverTwo, DefaultCameraDistance, 1000, 1000000);
        }

        public void MoveCameraToSector(DrawingPoint Sector)
        {
            if (!(Camera is ArcBallCamera))
                return;

            ArcBallCamera camera = (ArcBallCamera)(_editor.Camera);
            Vector3 center = _editor.SelectedRoom.GetLocalCenter();
            camera.Target = new Vector3(Sector.X * 1024.0f, center.Y, Sector.Y * 1024.0f) + _editor.SelectedRoom.WorldPos;
            Draw();
            return;
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
            ResetCamera();

            // Maybe I could use this as bounding box, scaling it properly before drawing
            GeometricPrimitive.Cube.New(_device, 1024);
            GeometricPrimitive.LinesCube.New(_device);

            // This sphere will be scaled up and down multiple times for using as In & Out of lights
            _sphere = GeometricPrimitive.Sphere.New(_device, 1024, 6);

            //Little cubes and little spheres are used as mesh for lights, cameras, sinks, etc
            _littleCube = GeometricPrimitive.Cube.New(_device, 256);
            _littleSphere = GeometricPrimitive.Sphere.New(_device, 256, 8);

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
                    (_editor.Camera as ArcBallCamera)?.Rotate(0, -_rotationSpeed);
                    Draw();
                    return true;

                case Keys.Down:
                    (_editor.Camera as ArcBallCamera)?.Rotate(0, _rotationSpeed);
                    Draw();
                    return true;

                case Keys.Left:
                    (_editor.Camera as ArcBallCamera)?.Rotate(_rotationSpeed, 0);
                    Draw();
                    return true;

                case Keys.Right:
                    (_editor.Camera as ArcBallCamera)?.Rotate(-_rotationSpeed, 0);
                    Draw();
                    return true;

                case Keys.PageUp:
                    (_editor.Camera as ArcBallCamera)?.Move(_keyboardZoomSpeed);
                    Draw();
                    return true;

                case Keys.PageDown:
                    (_editor.Camera as ArcBallCamera)?.Move(-_keyboardZoomSpeed);
                    Draw();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
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
                if ((newPicking is PickingResultBlock) && _editor.RelocateCameraActive)
                {
                    MoveCameraToSector(((PickingResultBlock)newPicking).Pos);
                    return;
                }
                else if (newPicking is PickingResultObject)
                {
                    _editor.SelectedObject = ((PickingResultObject)newPicking).ObjectPtr;
                    _editor.LoadStaticMeshColorInUI();
                    if (_editor.SelectedObject.Value.Type == ObjectInstanceType.Light)
                        _editor.EditLight();
                    Draw();
                }

                // Set gizmo axis (or none if another object was picked)
                _gizmo.SetGizmoAxis((newPicking as PickingResultGizmo)?.Axis ?? GizmoAxis.None);

                // Process editor actions
                switch (_editor.Action)
                {
                    case EditorAction.PlaceLight:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ObjectInstanceType.Light);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorAction.PlaceItem:
                        if (newPicking is PickingResultBlock)
                        {
                            if (_editor.ActionPlaceItem_Item.IsStatic)
                                EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ObjectInstanceType.StaticMesh);
                            else
                                EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ObjectInstanceType.Moveable);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorAction.PlaceSink:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ObjectInstanceType.Sink);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorAction.PlaceCamera:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ObjectInstanceType.Camera);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorAction.PlaceSound:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ObjectInstanceType.SoundSource);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorAction.PlaceFlyByCamera:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ObjectInstanceType.FlyByCamera);
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorAction.PlaceNoCollision:
                        if (newPicking is PickingResultBlock)
                            EditorActions.PlaceNoCollision(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ((PickingResultBlock)newPicking).Face);
                        break;
                    case EditorAction.Paste:
                        if (newPicking is PickingResultBlock)
                        {
                            Clipboard.Paste(_editor.Level, _editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos);
                            _editor.Action = EditorAction.None;
                            Draw();
                            _editor.UpdateStatusStrip();
                            _editor.DrawPanelGrid();
                        }
                        break;
                    case EditorAction.Stamp:
                        if (newPicking is PickingResultBlock)
                        {
                            Clipboard.Paste(_editor.Level, _editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos);
                            Draw();
                            _editor.UpdateStatusStrip();
                            _editor.DrawPanelGrid();
                        }
                        break;
                    case EditorAction.None:
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
                                    if (_editor.SelectedSectorAvailable && _editor.SelectedSector.Contains(pos))
                                    {
                                        // Rotate the arrows
                                        if (Control.ModifierKeys.HasFlag(Keys.Control))
                                        {
                                            if (_editor.SelectedSectorArrow == EditorArrowType.CornerSW)
                                                _editor.SelectedSectorArrow = EditorArrowType.EntireFace;
                                            else if (_editor.SelectedSectorArrow == EditorArrowType.CornerSE)
                                                _editor.SelectedSectorArrow = EditorArrowType.CornerSW;
                                            else if (_editor.SelectedSectorArrow == EditorArrowType.CornerNE)
                                                _editor.SelectedSectorArrow = EditorArrowType.CornerSE;
                                            else if (_editor.SelectedSectorArrow == EditorArrowType.CornerNW)
                                                _editor.SelectedSectorArrow = EditorArrowType.CornerNE;
                                            else
                                                _editor.SelectedSectorArrow = EditorArrowType.CornerNW;
                                        }
                                        else
                                        {
                                            if (_editor.SelectedSectorArrow == EditorArrowType.EdgeW)
                                                _editor.SelectedSectorArrow = EditorArrowType.EntireFace;
                                            else if (_editor.SelectedSectorArrow == EditorArrowType.EdgeS)
                                                _editor.SelectedSectorArrow = EditorArrowType.EdgeW;
                                            else if (_editor.SelectedSectorArrow == EditorArrowType.EdgeE)
                                                _editor.SelectedSectorArrow = EditorArrowType.EdgeS;
                                            else if (_editor.SelectedSectorArrow == EditorArrowType.EdgeN)
                                                _editor.SelectedSectorArrow = EditorArrowType.EdgeE;
                                            else
                                                _editor.SelectedSectorArrow = EditorArrowType.EdgeN;

                                        }
                                        Draw();
                                    }
                                    else
                                    {
                                        // Select rectangle
                                        _editor.SelectedSectorStart = pos;
                                        _editor.SelectedSectorEnd = _editor.SelectedSectorStart;
                                        _editor.SelectedSectorArrow = EditorArrowType.EntireFace;
                                        _doSectorSelection = true;

                                        Draw();
                                        _editor.DrawPanelGrid();
                                        _editor.UpdateStatusStrip();
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
                EditorActions.EditObject(_editor.SelectedRoom, ((PickingResultObject)newPicking).ObjectPtr, this.Parent);
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
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Move(-deltaY * 50);
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.Translate(new Vector3(deltaX, -deltaY, 0));
                else
                    Camera.Rotate((float)(deltaX / 500.0f), (float)(-deltaY / 500.0f));

                Draw();
            }
            else if (e.Button == MouseButtons.Left)
            {
                // Process gizmo
                _gizmo.MouseMoved(Camera.GetViewProjectionMatrix(Width, Height), e.X, e.Y, Control.ModifierKeys);
                
                // Calculate block selection
                if (_doSectorSelection)
                {
                    PickingResult newPicking = DoPicking(e.X, e.Y);
                    if (newPicking is PickingResultBlock)
                    {
                        _editor.SelectedSectorEnd = new SharpDX.DrawingPoint(
                            ((PickingResultBlock)newPicking).Pos.X,
                            ((PickingResultBlock)newPicking).Pos.Y);

                        Draw();
                        _editor.UpdateStatusStrip();
                        _editor.DrawPanelGrid();
                    }
                    return;
                }

                // Texture editing
                if ((_editor.Mode == EditorMode.FaceEdit) && (_editor.Action == EditorAction.None))
                {
                    PickingResultBlock newPicking = DoPicking(e.X, e.Y) as PickingResultBlock;

                    if (newPicking != null)
                        if (_editor.Action == EditorAction.None)
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

        private void DoMeshPicking<T>(ref PickingResult result, Ray ray, ObjectPtr objectPtr, Mesh<T> mesh, Matrix world) where T : struct, IVertex
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

            // First check lights
            for (int i = 0; i < room.Lights.Count; i++)
            {
                BoundingSphere sphere = new BoundingSphere(room.Lights[i].Position, 128.0f);
                if (ray.Intersects(ref sphere, out distance) && ((result == null) || (distance < result.Distance)))
                    result = new PickingResultObject(distance, new ObjectPtr(ObjectInstanceType.Light, i));
            }

            // Check for sinks
            for (int i = 0; i < room.Sinks.Count; i++)
            {
                BoundingBox box = new BoundingBox(
                    _editor.Level.Objects[room.Sinks[i]].Position - new Vector3(128.0f, 128.0f, 128.0f),
                    _editor.Level.Objects[room.Sinks[i]].Position + new Vector3(128.0f, 128.0f, 128.0f));
                if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                    result = new PickingResultObject(distance, new ObjectPtr(ObjectInstanceType.Sink, room.Sinks[i]));
            }

            // Check for cameras
            for (int i = 0; i < room.Cameras.Count; i++)
            {
                BoundingBox box = new BoundingBox(
                    _editor.Level.Objects[room.Cameras[i]].Position - new Vector3(128.0f, 128.0f, 128.0f),
                    _editor.Level.Objects[room.Cameras[i]].Position + new Vector3(128.0f, 128.0f, 128.0f));
                if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                    result = new PickingResultObject(distance, new ObjectPtr(ObjectInstanceType.Camera, room.Cameras[i]));
            }

            // Check for sound sources
            for (int i = 0; i < room.SoundSources.Count; i++)
            {
                BoundingBox box = new BoundingBox(
                    _editor.Level.Objects[room.SoundSources[i]].Position - new Vector3(128.0f, 128.0f, 128.0f),
                    _editor.Level.Objects[room.SoundSources[i]].Position + new Vector3(128.0f, 128.0f, 128.0f));
                if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                    result = new PickingResultObject(distance, new ObjectPtr(ObjectInstanceType.SoundSource, room.SoundSources[i]));
            }

            // Check for flyby cameras
            for (int i = 0; i < room.FlyByCameras.Count; i++)
            {
                BoundingBox box = new BoundingBox(
                    _editor.Level.Objects[room.FlyByCameras[i]].Position - new Vector3(128.0f, 128.0f, 128.0f),
                    _editor.Level.Objects[room.FlyByCameras[i]].Position + new Vector3(128.0f, 128.0f, 128.0f));
                if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                    result = new PickingResultObject(distance, new ObjectPtr(ObjectInstanceType.FlyByCamera, room.FlyByCameras[i]));
            }

            // Check for moveables
            for (int i = 0; i < room.Moveables.Count; i++)
            {
                MoveableInstance modelInfo = (MoveableInstance)_editor.Level.Objects[room.Moveables[i]];
                SkinnedModel model = modelInfo.Model;
                model.BuildAnimationPose(model.Animations[0].KeyFrames[0]);

                for (int j = 0; j < model.Meshes.Count; j++)
                {
                    SkinnedMesh mesh = model.Meshes[j];
                    Matrix world = model.AnimationTransforms[j] *
                                   Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.Rotation)) *
                                   Matrix.Translation(modelInfo.Position);
                    DoMeshPicking(ref result, ray, new ObjectPtr(ObjectInstanceType.Moveable, room.Moveables[i]), mesh, world);
                }
            }

            // Check for static meshes
            for (int i = 0; i < room.StaticMeshes.Count; i++)
            {
                StaticMeshInstance modelInfo = (StaticMeshInstance)_editor.Level.Objects[room.StaticMeshes[i]];
                StaticModel model = modelInfo.Model;

                StaticMesh mesh = model.Meshes[0];
                Matrix world = Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.Rotation)) *
                               Matrix.Translation(modelInfo.Position);
                DoMeshPicking(ref result, ray, new ObjectPtr(ObjectInstanceType.Moveable, room.StaticMeshes[i]), mesh, world);
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
                        if (room.RayIntersectsFace(ref ray, ref face, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultBlock(distance, new DrawingPoint(sx, sz), (BlockFaces)f);
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
            if (!_drawFlybyPath && !_drawHeightLine) return;

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

            for (int i = 0; i < room.Lights.Count; i++)
            {
                Light light = room.Lights[i];

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

                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.Light, i))
                    solidEffect.Parameters["SelectionEnabled"].SetValue(true);
                else
                    solidEffect.Parameters["SelectionEnabled"].SetValue(false);

                solidEffect.CurrentTechnique.Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
            }

            solidEffect.Parameters["SelectionEnabled"].SetValue(false);

            if (_editor.SelectedObject.HasValue && (_editor.SelectedObject.Value.Type == ObjectInstanceType.Light))
            {
                Light light = room.Lights[_editor.SelectedObject.Value.Id];

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

                    Matrix rotation = Matrix.RotationAxis(-Vector3.UnitX, MathUtil.DegreesToRadians(light.DirectionX)) *
                                      Matrix.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(light.DirectionY));
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

                    Matrix rotation = Matrix.RotationAxis(-Vector3.UnitX, MathUtil.DegreesToRadians(light.DirectionX)) *
                                      Matrix.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(light.DirectionY));

                    Matrix model = Matrix.Scaling(0.01f, 0.01f, 1.0f) * rotation * Matrix.Translation(light.Position) *
                                   Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                string message = "";

                if (light.Type == LightType.Light)
                    message = "Point light";
                if (light.Type == LightType.Spot)
                    message = "Spot light";
                if (light.Type == LightType.Shadow)
                    message = "Shadow";
                if (light.Type == LightType.Sun)
                    message = "Sun";
                if (light.Type == LightType.Effect)
                    message = "Effect";
                if (light.Type == LightType.FogBulb)
                    message = "Fog bulb";

                message += " (" + _editor.SelectedObject.Value.Id + ")";

                // Object position
                message += Environment.NewLine + GetObjectPositionString(room, light.Position);

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

            for (int i = 0; i < _camerasToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_camerasToDraw[i]];

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                var color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.Camera, instance.Id))
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _device.SetRasterizerState(_rasterizerWireframe);

                    string message = "Camera (" + instance.Id + ")";

                    // Object position
                    message += Environment.NewLine + GetObjectPositionString(room, instance.Position);

                    Matrix modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if ((trigger.TargetType == TriggerTargetType.Object ||
                             trigger.TargetType == TriggerTargetType.Camera) && trigger.Target == instance.Id)
                        {
                            message += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                       _editor.Level.Rooms.ReferenceIndexOf(trigger.Room) + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

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

            for (int i = 0; i < _flybyToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_flybyToDraw[i]];

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                Vector4 color = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.FlyByCamera, instance.Id))
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _device.SetRasterizerState(_rasterizerWireframe);
                }

                FlybyCameraInstance flyby = (FlybyCameraInstance) instance;

                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.FlyByCamera, instance.Id))
                {
                    string message = "Flyby Camera (" + flyby.Sequence + ":" + flyby.Number + ")";

                    // Object position
                    message += Environment.NewLine + GetObjectPositionString(room, instance.Position);

                    Matrix modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if ((trigger.TargetType == TriggerTargetType.Object ||
                             trigger.TargetType == TriggerTargetType.FlyByCamera) && trigger.Target == instance.Id)
                        {
                            message += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                       _editor.Level.Rooms.ReferenceIndexOf(trigger.Room) + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);

                    // Add the flyby path
                    AddFlybyPath(((FlybyCameraInstance)instance).Sequence);
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));

                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            for (int i = 0; i < _sinksToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_sinksToDraw[i]];

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                Vector4 color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.Sink, instance.Id))
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _device.SetRasterizerState(_rasterizerWireframe);

                    var message = "Sink (" + instance.Id + ")";

                    // Object position
                    message += Environment.NewLine + GetObjectPositionString(room, instance.Position);

                    var modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if ((trigger.TargetType == TriggerTargetType.Object ||
                             trigger.TargetType == TriggerTargetType.Sink) && trigger.Target == instance.Id)
                        {
                            message += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                       _editor.Level.Rooms.ReferenceIndexOf(trigger.Room) + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

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

            for (int i = 0; i < _soundSourcesToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_soundSourcesToDraw[i]];

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                Vector4 color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.SoundSource, instance.Id))
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _device.SetRasterizerState(_rasterizerWireframe);

                    SoundSourceInstance sound = (SoundSourceInstance) instance;

                    string message = "Sound Source";
                    if ((sound.SoundId >= 0) && (sound.SoundId < (_editor.Level?.Wad?.OriginalWad?.Sounds?.Count ?? 0)))
                        message += " (" + _editor.Level.Wad.OriginalWad.Sounds[sound.SoundId] + ") ";
                    else
                        message += " ( Invalid sound ) ";
                    message = " + instance.Id + ";

                    // Object position
                    message += Environment.NewLine + GetObjectPositionString(room, instance.Position);

                    Matrix modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if ((trigger.TargetType == TriggerTargetType.Object) && trigger.Target == instance.Id)
                        {
                            message += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                       _editor.Level.Rooms.ReferenceIndexOf(trigger.Room) + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

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

            _device.SetVertexBuffer(_cone.VertexBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
            _device.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);
            _device.SetRasterizerState(_rasterizerWireframe);

            for (int i = 0; i < _flybyToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_flybyToDraw[i]];

                FlybyCameraInstance flyby = (FlybyCameraInstance) instance;

                // Outer cone
                float coneAngle = (float) Math.Atan2(512, 1024);
                float cutoffScaleH = 1;
                float cutoffScaleW = MathUtil.DegreesToRadians(flyby.Fov / 2) / coneAngle * cutoffScaleH;

                Matrix rotation = Matrix.RotationAxis(-Vector3.UnitX, MathUtil.DegreesToRadians(flyby.DirectionX)) * Matrix.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(flyby.DirectionY));

                Matrix model = Matrix.Scaling(cutoffScaleW, cutoffScaleW, cutoffScaleH) * rotation * Matrix.Translation(flyby.Position) * Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                effect.CurrentTechnique.Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawMoveables(Matrix viewProjection, Room room)
        {
            _device.SetBlendState(_device.BlendStates.Opaque);

            Effect skinnedModelEffect = _deviceManager.Effects["Model"];

            skinnedModelEffect.Parameters["TextureEnabled"].SetValue(true);
            skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(false);

            MoveableInstance _lastObject = null;

            skinnedModelEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

            for (int k = 0; k < MoveablesToDraw.Count; k++)
            {
                MoveableInstance modelInfo = (MoveableInstance) _editor.Level.Objects[MoveablesToDraw[k]];

                Debug.NumMoveables++;

                SkinnedModel model = modelInfo.Model;

                if (k == 0 || model.ObjectID != _lastObject.ObjectId)
                {
                    _device.SetVertexBuffer(0, model.VertexBuffer);
                    _device.SetIndexBuffer(model.IndexBuffer, true);
                }

                if (k == 0)
                {
                    _device.SetVertexInputLayout(
                        VertexInputLayout.FromBuffer<SkinnedVertex>(0, model.VertexBuffer));
                }

                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.Moveable, modelInfo.Id))
                    skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(true);
                else
                    skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(false);

                Matrix world = Matrix.Identity;
                Matrix worldDebug = Matrix.Identity;

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    SkinnedMesh mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    var theRoom = modelInfo.Room;

                    world = model.AnimationTransforms[i] * Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.Rotation)) *
                                Matrix.Translation(modelInfo.Position) * Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));
                    worldDebug = Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));

                    skinnedModelEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);

                    skinnedModelEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                    Debug.NumVertices += mesh.NumIndices;
                    Debug.NumTriangles += mesh.NumIndices / 3;
                }

                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.Moveable, modelInfo.Id))
                {
                    Matrix modelViewProjection = worldDebug * viewProjection;
                    Vector3 screenPos = Vector3.Project(modelInfo.Position + 512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    string debugMessage = ObjectNames.GetMovableName((int)model.ObjectID) + " (" + modelInfo.Id + ")";

                    // Object position
                    debugMessage += Environment.NewLine + GetObjectPositionString(room, modelInfo.Position);

                    // Add OCB
                    if (modelInfo.Ocb != 0) debugMessage += Environment.NewLine + "OCB: " + modelInfo.Ocb;

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if (trigger.TargetType == TriggerTargetType.Object && trigger.Target == modelInfo.Id)
                        {
                            debugMessage += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                            _editor.Level.Rooms.ReferenceIndexOf(trigger.Room) + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(debugMessage, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, modelInfo.Position);
                }

                _lastObject = modelInfo;
            }
        }

        private void DrawStaticMeshes(Matrix viewProjection, Room room)
        {
            _device.SetBlendState(_device.BlendStates.Opaque);

            Effect staticMeshEffect = _deviceManager.Effects["StaticModel"];

            staticMeshEffect.Parameters["TextureEnabled"].SetValue(true);
            staticMeshEffect.Parameters["SelectionEnabled"].SetValue(false);
            staticMeshEffect.Parameters["LightEnabled"].SetValue(true);

            staticMeshEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
            staticMeshEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

            StaticMeshInstance _lastObject = null;

            for (int k = 0; k < StaticMeshesToDraw.Count; k++)
            {
                StaticMeshInstance modelInfo = (StaticMeshInstance) _editor.Level.Objects[StaticMeshesToDraw[k]];
                StaticModel model = modelInfo.Model;

                if (k == 0)
                {
                    _device.SetVertexInputLayout(
                        VertexInputLayout.FromBuffer<StaticVertex>(0, model.VertexBuffer));
                }

                if (k == 0 || model.ObjectID != _lastObject.ObjectId)
                {
                    _device.SetVertexBuffer(0, model.VertexBuffer);
                    _device.SetIndexBuffer(model.IndexBuffer, true);
                }

                Debug.NumStaticMeshes++;

                bool SelectionEnabled = _editor.SelectedObject == new ObjectPtr(ObjectInstanceType.StaticMesh, modelInfo.Id);
                staticMeshEffect.Parameters["SelectionEnabled"].SetValue(SelectionEnabled);
                staticMeshEffect.Parameters["Color"].SetValue(GetSharpdDXColor(modelInfo.Color));

                Matrix world = Matrix.Identity;
                Matrix worldDebug = Matrix.Identity;

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var theRoom = modelInfo.Room;

                    StaticMesh mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    world = Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.Rotation)) *
                            Matrix.Translation(modelInfo.Position) *
                            Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));
                    worldDebug = Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));

                    staticMeshEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);

                    staticMeshEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                    Debug.NumVertices += mesh.NumIndices;
                    Debug.NumTriangles += mesh.NumIndices / 3;
                }

                if (_editor.SelectedObject == new ObjectPtr(ObjectInstanceType.StaticMesh, modelInfo.Id))
                {
                    Matrix modelViewProjection = worldDebug * viewProjection;
                    Vector3 screenPos = Vector3.Project(modelInfo.Position + 512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, modelViewProjection);

                    string debugMessage = ObjectNames.GetStaticName((int)model.ObjectID) + " (" + modelInfo.Id + ")";

                    // Object position
                    debugMessage += Environment.NewLine + GetObjectPositionString(room, modelInfo.Position);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if (trigger.TargetType == TriggerTargetType.Object && trigger.Target == modelInfo.Id)
                        {
                            debugMessage += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                            _editor.Level.Rooms.ReferenceIndexOf(trigger.Room) + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(debugMessage, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, modelInfo.Position);
                }

                _lastObject = modelInfo;
            }
        }

        private void DrawSkyBox(Matrix viewProjection)
        {
            if (_editor?.Level?.Wad == null)
                return;
            if (!_editor.Level.Wad.Moveables.ContainsKey(459))
                return;

            _device.SetBlendState(_device.BlendStates.Opaque);

            Effect skinnedModelEffect = _deviceManager.Effects["Model"];

            skinnedModelEffect.Parameters["TextureEnabled"].SetValue(true);
            skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(false);

            SkinnedModel skinnedModel = _editor.Level.Wad.Moveables[459];
            skinnedModel.BuildAnimationPose(skinnedModel.Animations[0].KeyFrames[0]);

            skinnedModelEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
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
            _camerasToDraw = new List<int>();
            _sinksToDraw = new List<int>();
            _flybyToDraw = new List<int>();
            _soundSourcesToDraw = new List<int>();
            MoveablesToDraw = new List<int>();
            StaticMeshesToDraw = new List<int>();

            for (int i = 0; i < _roomsToDraw.Count; i++)
            {
                if (_roomsToDraw[i] == _editor.SelectedRoom)
                {
                    _camerasToDraw.AddRange(_roomsToDraw[i].Cameras);
                    _sinksToDraw.AddRange(_roomsToDraw[i].Sinks);
                    _flybyToDraw.AddRange(_roomsToDraw[i].FlyByCameras);
                    _soundSourcesToDraw.AddRange(_roomsToDraw[i].SoundSources);
                }

                MoveablesToDraw.AddRange(_roomsToDraw[i].Moveables);
                StaticMeshesToDraw.AddRange(_roomsToDraw[i].StaticMeshes);
            }

            MoveablesToDraw.Sort(new ComparerMoveables(_editor.Level.Rooms));
            StaticMeshesToDraw.Sort(new ComparerStaticMeshes(_editor.Level.Rooms));
        }

        private void CollectRoomsToDraw(Room room)
        {
            // New iterative version of the function 

            Vector3 cameraPosition = Camera.GetPosition();

            Stack<Room> stackRooms = new Stack<Room>();
            Stack<int> stackLimits = new Stack<int>();

            stackRooms.Push(room);
            stackLimits.Push(0);

            while (stackRooms.Count > 0)
            {
                var theRoom = stackRooms.Pop();
                int theLimit = stackLimits.Pop();

                if (theLimit > _editor.Configuration.DrawRoomsMaxDepth)
                    continue;

                theRoom.Visited = true;
                if (theRoom.Flipped && theRoom.AlternateRoom != null)
                    _roomsToDraw.Add(theRoom.AlternateRoom);
                else
                    _roomsToDraw.Add(theRoom);

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

                    if (portal.Room == theRoom && !portal.AdjoiningRoom.Visited &&
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
                                    (face.Texture == -1 && !face.Invisible))
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
                                        if (!face.Transparent && !face.Invisible)
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
                                        else if (face.Transparent)
                                        {
                                            RenderBucket bucket = new RenderBucket
                                            {
                                                FaceType = (BlockFaces) f,
                                                Face = face,
                                                X = x,
                                                Z = z,
                                                DoubleSided = (byte) (face.DoubleSided ? 1 : 0),
                                                Texture = face.Texture,
                                                Room = room,
                                                Plane = face.Plane
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

        public void Draw()
        {
            if (DesignMode)
                return;

            Stopwatch _watch = new Stopwatch();
            _watch.Start();

            // Reset gizmo and debug strings and lines
            Debug.Reset();
            _drawHeightLine = false;
            _drawFlybyPath = false;

            // Don't draw anything if device is not ready
            if (_device == null || _device.Presenter == null ||
                _editor.SelectedRoom == null)
                return;

            // reset the backbuffer
            bool IsFlipMap = _editor?.SelectedRoom?.Flipped ?? false;
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

            // Reset visited flag for collecting rooms
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] == null)
                    continue;
                _editor.Level.Rooms[i].Visited = false;
            }

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
            if (DrawHorizon)
            {
                DrawSkyBox(viewProjection);
                _device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1.0f, 0);
            }

            // Draw moveables and static meshes
            if (_editor != null && _editor.Level != null && _editor.Level.Wad != null && _editor.SelectedRoom != null)
            {
                DrawMoveables(viewProjection, _editor.SelectedRoom);
                DrawStaticMeshes(viewProjection, _editor.SelectedRoom);
            }

            // Prepare the shader
            _roomEffect = _deviceManager.Effects["Room"];

            // Set some common parameters of the shader
            _roomEffect.Parameters["CameraPosition"].SetValue(viewProjection.TranslationVector);
            _roomEffect.Parameters["LightingEnabled"].SetValue(false);
            _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
            _roomEffect.Parameters["TextureEnabled"].SetValue(false);
            _roomEffect.Parameters["EditorTextureEnabled"].SetValue(false);
            _roomEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);

            // Draw buckets
            DrawSolidBuckets(viewProjection);
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
                string message = (_roomsToDraw[i].Name != null ? _roomsToDraw[i].Name : "Room " + _editor.Level.Rooms.ReferenceIndexOf(_roomsToDraw[i]));

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
                    _roomEffect.Parameters["Texture"].SetResource(_editor.Level.Textures[0]);
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

                Debug.NumVertices += face.Vertices.Length;
                Debug.NumTriangles += face.Vertices.Length / 3;

                _lastBucket = bucket;
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawTransparentBuckets(Matrix viewProjection)
        {
            _device.SetBlendState(_device.BlendStates.Additive);

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
                }

                // Enable or disable double sided textures
                if (_lastBucket == null || _lastBucket.DoubleSided != bucket.DoubleSided)
                {
                    if (bucket.DoubleSided == 1)
                        _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                    else
                        _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                }

                // Change texture if needed
                if (_lastBucket == null /*|| _lastBucket.Texture!=bucket.Texture*/)
                {
                    LevelTexture textureSample = _editor.Level.TextureSamples[face.Texture];
                    _roomEffect.Parameters["Texture"].SetResource(_editor.Level.Textures[0 /*textureSample.Page*/]);
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

                Debug.NumVertices += face.Vertices.Length;
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
                int xMin = Math.Min(_editor.SelectedSectorStart.X, _editor.SelectedSectorEnd.X);
                int xMax = Math.Max(_editor.SelectedSectorStart.X, _editor.SelectedSectorEnd.X);
                int zMin = Math.Min(_editor.SelectedSectorStart.Y, _editor.SelectedSectorEnd.Y);
                int zMax = Math.Max(_editor.SelectedSectorStart.Y, _editor.SelectedSectorEnd.Y);

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
                        switch (_editor.SelectedSectorArrow)
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
                        switch (_editor.SelectedSectorArrow)
                        {
                            case EditorArrowType.EntireFace:
                            default:
                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                break;
                            case EditorArrowType.EdgeN:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_left"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeS:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.EdgeW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_right"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerNW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerNE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerSE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case EditorArrowType.CornerSW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
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

                            switch (_editor.SelectedSectorArrow)
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

                            switch (_editor.SelectedSectorArrow)
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

                            switch (_editor.SelectedSectorArrow)
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

                            switch (_editor.SelectedSectorArrow)
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

                    if ((room.Blocks[x, z].Flags & BlockFlags.Electricity) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorDeath));
                    if ((room.Blocks[x, z].Flags & BlockFlags.Death) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorDeath));
                    if ((room.Blocks[x, z].Flags & BlockFlags.Lava) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorDeath));
                    if ((room.Blocks[x, z].Flags & BlockFlags.Monkey) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorMonkey));
                    if ((room.Blocks[x, z].Flags & BlockFlags.Box) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorBox));
                    if ((room.Blocks[x, z].Flags & BlockFlags.NotWalkableFloor) != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorNotWalkable));
                    if (room.Blocks[x, z].Climb[0] ||
                        room.Blocks[x, z].Climb[1] ||
                        room.Blocks[x, z].Climb[2] ||
                        room.Blocks[x, z].Climb[3])
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

                Debug.NumVertices += bucket.Face.Vertices.Length;
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
            message += Environment.NewLine + "Height: " + Math.Round(height) + " units(" + (height / 256.0f) +
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

            foreach (ObjectInstance obj in _editor.Level.Objects.Values)
            {
                if (obj.Type == ObjectInstanceType.FlyByCamera)
                {
                    FlybyCameraInstance instance = (FlybyCameraInstance)obj;
                    if (instance.Sequence == sequence) flybyCameras.Add(instance);
                }
            }

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
