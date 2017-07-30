using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDX;
using TombEditor.Geometry;
using SharpDX.Toolkit.Graphics;
using TombLib.Wad;
using TombLib.Graphics;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using TombEditor.Compilers;

namespace TombEditor.Controls
{
    public struct RenderFaceInfo
    {
        public Plane Plane;
        public float Distance;
        public Vector3 Centre;
        public BlockFaces Type;
        public int X;
        public int Z;
        public bool Invisible;
        public bool DoubleSided;
    }

    public struct DebugString
    {
        public Vector2 Position;
        public string Content;
    }

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
            public Vector3 Centre { get; set; }
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
                var instanceX = (MoveableInstance) Editor.Instance.Level.Objects[x];
                var instanceY = (MoveableInstance) Editor.Instance.Level.Objects[y];

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
                var instanceX = (StaticMeshInstance) Editor.Instance.Level.Objects[x];
                var instanceY = (StaticMeshInstance) Editor.Instance.Level.Objects[y];

                int result = instanceX.ObjectId.CompareTo(instanceY.ObjectId);
                if (result != 0)
                    return result;
                return _rooms.ReferenceIndexOf(instanceX.Room).CompareTo(_rooms.ReferenceIndexOf(instanceY.Room));
            }
        }

        public int RoomIndex { get; set; }
        public int DeltaX { get; set; }
        public int DeltaY { get; set; }
        public int LastX { get; set; }
        public int LastY { get; set; }
        public bool Drag { get; set; }
        public Vector3 DeltaVector { get; set; }
        public int LightIndex { get; set; }
        public SwapChainGraphicsPresenter Presenter { get; set; }
        public Viewport Viewport { get; set; }
        public ArcBallCamera Camera { get; set; }

        private Editor _editor;
        private bool _firstSelection;
        private RasterizerState _rasterizerWireframe;
        private GeometricPrimitive _sphere;
        private GeometricPrimitive _cone;
        private GeometricPrimitive _littleCube;
        private GeometricPrimitive _littleSphere;

        // Gizmo
        private Gizmo _gizmo;

        private bool _drawGizmo = false;

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

        private Buffer<EditorVertex> _objectHeightLineVertex;
        private bool _drawHeightLine = false;

        private Effect _roomEffect;

        public PanelRendering3D()
        {
            InitializeComponent();

            _editor = Editor.Instance;
        }

        public void ResetCamera()
        {
            Room room = _editor.SelectedRoom;

            // Point the camera to the room's centre
            Vector3 target = new Vector3(room.Position.X * 1024.0f + room.NumXSectors * 512.0f,
                room.Position.Y * 256.0f + room.Ceiling * 64.0f,
                room.Position.Z * 1024.0f + room.NumZSectors * 512.0f);

            // Initialize a new camera
            Camera = new ArcBallCamera(target, (float) Math.PI, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 3072, 1000,
                1000000);
        }

        public void InitializePanel()
        {
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

            Presenter = new SwapChainGraphicsPresenter(_editor.GraphicsDevice, pp);
            Viewport = new Viewport(0, 0, Width, Height, 10.0f, 100000.0f);

            // Initialize the Arc-Ball Camera
            Camera = new ArcBallCamera(Vector3.Zero, (float) Math.PI, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 3000,
                1000, 1000000);

            // Maybe I could use this as bounding box, scaling it properly before drawing
            GeometricPrimitive.Cube.New(_editor.GraphicsDevice, 1024);
            GeometricPrimitive.LinesCube.New(_editor.GraphicsDevice);

            // This sphere will be scaled up and down multiple times for using as In & Out of lights
            _sphere = GeometricPrimitive.Sphere.New(_editor.GraphicsDevice, 1024, 6);

            //Little cubes and little spheres are used as mesh for lights, cameras, sinks, etc
            _littleCube = GeometricPrimitive.Cube.New(_editor.GraphicsDevice, 256);
            _littleSphere = GeometricPrimitive.Sphere.New(_editor.GraphicsDevice, 256, 8);

            _cone = GeometricPrimitive.Cone.New(_editor.GraphicsDevice, 1024, 1024, 18);

            // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
            new BasicEffect(_editor.GraphicsDevice);

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

            _rasterizerWireframe = RasterizerState.New(_editor.GraphicsDevice, renderStateDesc);

            _gizmo = new Gizmo();

            logger.Info("Graphic Device ready");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Presenter != null && Width != 0 && Height != 0)
                Presenter.Resize(Width, Height, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!Drag)
                Drag = true;

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Camera movement, just store coordinates
                LastX = e.X;
                LastY = e.Y;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Do picking on the scene
                PickingResult newPicking = DoPicking(e.X, e.Y);
                _editor.PickingResult = newPicking;

                if (newPicking.Gizmo)
                    return;

                // Try to paste or stamp
                if (Clipboard.Action != PasteAction.None && Clipboard.Paste())
                    return;

                switch (_editor.Mode)
                {
                    case EditorMode.Geometry:
                        // Check if is a first selection or not, because I must cycle arrows
                        if (_editor.Action == EditorAction.None)
                        {
                            int xBlock = newPicking.Element >> 5;
                            int zBlock = newPicking.Element & 31;

                            // if was not selected a block, then exit
                            if (newPicking.ElementType != PickingElementType.Block)
                            {
                                _editor.BlockSelectionReset();
                                _firstSelection = true;
                                _editor.BlockEditingType = 0;
                                return;
                            }

                            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                            {
                                BlockFaces face = (BlockFaces)newPicking.SubElement;

                                if (face == BlockFaces.Floor || face == BlockFaces.FloorTriangle2)
                                {
                                    EditorActions.FlipFloorSplit(xBlock, xBlock, zBlock, zBlock);
                                    return;
                                }

                                if (face == BlockFaces.Ceiling || face == BlockFaces.CeilingTriangle2)
                                {
                                    EditorActions.FlipCeilingSplit(xBlock, xBlock, zBlock, zBlock);
                                    return;
                                }
                            }

                            _firstSelection = false;

                            // if one of the four corners of the selection is equal to -1, then is a first selection
                            if (_editor.BlockSelectionAvailable)
                            {
                                _editor.BlockSelectionStart = new System.Drawing.Point(newPicking.Element >> 5, newPicking.Element & 31);
                                _editor.BlockSelectionEnd = _editor.BlockSelectionStart;
                                _firstSelection = true;
                                _editor.BlockEditingType = 0;
                            }
                            else
                            {
                                int xMin = Math.Min(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
                                int xMax = Math.Max(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
                                int zMin = Math.Min(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);
                                int zMax = Math.Max(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);

                                if (xBlock >= xMin && xBlock <= xMax && xBlock != -1 && zBlock >= zMin &&
                                    zBlock <= zMax)
                                {
                                    // is not a first selection
                                    _firstSelection = false;
                                }
                                else
                                {
                                    _editor.BlockSelectionStart = new System.Drawing.Point(newPicking.Element >> 5, newPicking.Element & 31);
                                    _editor.BlockSelectionEnd = _editor.BlockSelectionStart;
                                    _firstSelection = true;
                                    _editor.BlockEditingType = 0;
                                }
                            }
                        }
                        break;

                    case EditorMode.Map2D:
                        break;

                    case EditorMode.FaceEdit:
                        if (_editor.Action == EditorAction.None)
                        {
                            if (Control.ModifierKeys == Keys.Control)
                                FlipTexture();
                            else if (Control.ModifierKeys == Keys.Shift)
                                RotateTexture();
                            else
                                PlaceTexture();
                        }
                        break;
                }
            }

            Draw();
            _editor.DrawPanelGrid();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            PickingResult newPicking = DoPicking(e.X, e.Y);
            _editor.PickingResult = newPicking;
            switch (newPicking.ElementType)
            {
                case PickingElementType.Trigger:
                    using (FormTrigger form = new FormTrigger())
                        form.ShowDialog(this.Parent);
                    break;
                case PickingElementType.Sink:
                    using (FormSink form = new FormSink())
                        form.ShowDialog(this.Parent);
                    break;
                case PickingElementType.FlyByCamera:
                    using (FormFlybyCamera form = new FormFlybyCamera())
                        form.ShowDialog(this.Parent);
                    break;
                case PickingElementType.SoundSource:
                    using (FormSound form = new FormSound())
                        form.ShowDialog(this.Parent);
                    break;
                case PickingElementType.Moveable:
                    using (FormMoveable form = new FormMoveable())
                        form.ShowDialog(this.Parent);
                    break;
            }
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                return;

            DeltaX = e.X - LastX;
            DeltaY = e.Y - LastY;

            LastX = e.X;
            LastY = e.Y;

            // Right click is for camera motion
            if (Drag && e.Button == MouseButtons.Right)
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    Camera.Move(-DeltaY * 50);
                }
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    Camera.Translate(new Vector3(DeltaX, -DeltaY, 0));
                }
                else
                {
                    Camera.Rotate((float) (DeltaX / 500.0f), (float) (-DeltaY / 500.0f));
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // If gizmo was picked, then don't do picking again
                if (_editor.PickingResult.Gizmo)
                {
                    // TODO: it's just a test, need to figure out the real math
                    //float delta = (float)Math.Floor(DeltaX * 40.0f / 32.0f) * 32.0f;

                    Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);

                    // For picking, I'll check first sphere/cubes bounding boxes and then eventually
                    Room room = _editor.SelectedRoom;

                    // First get the ray in 3D space from X, Y mouse coordinates
                    Ray ray = Ray.GetPickRay((int) e.X, (int) e.Y, _editor.GraphicsDevice.Viewport,
                        Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) * viewProjection);

                    float delta = 0;

                    if (_editor.PickingResult.GizmoAxis == GizmoAxis.X)
                    {
                        Plane plane = new Plane(_gizmo.Position, Vector3.UnitY);
                        Vector3 intersection;

                        ray.Intersects(ref plane, out intersection);
                        delta = intersection.X - (_gizmo.Position.X + 1024.0f);
                        // delta = (float)Math.Floor(delta / 64.0f) * 64.0f;
                    }

                    if (_editor.PickingResult.GizmoAxis == GizmoAxis.Y)
                    {
                        Plane plane = new Plane(_gizmo.Position, Vector3.UnitX);
                        Vector3 intersection;

                        ray.Intersects(ref plane, out intersection);
                        delta = intersection.Y - (_gizmo.Position.Y + 1024.0f);
                        //delta = (float)Math.Floor(delta / 64.0f) * 64.0f;
                    }

                    if (_editor.PickingResult.GizmoAxis == GizmoAxis.Z)
                    {
                        Plane plane = new Plane(_gizmo.Position, Vector3.UnitY);
                        Vector3 intersection;

                        ray.Intersects(ref plane, out intersection);
                        delta = intersection.Z - (_gizmo.Position.Z - 1024.0f);
                        // delta = (float)Math.Floor(delta / 64.0f) * 64.0f;
                    }

                    bool smooth = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

                    if (_editor.PickingResult.ElementType == PickingElementType.Camera)
                    {
                        EditorActions.MoveObject(EditorActions.ObjectType.Camera, _editor.PickingResult.Element,
                            _editor.PickingResult.GizmoAxis, delta, smooth);
                    }

                    if (_editor.PickingResult.ElementType == PickingElementType.FlyByCamera)
                    {
                        EditorActions.MoveObject(EditorActions.ObjectType.FlybyCamera, _editor.PickingResult.Element,
                            _editor.PickingResult.GizmoAxis, delta, smooth);
                    }

                    if (_editor.PickingResult.ElementType == PickingElementType.Sink)
                    {
                        EditorActions.MoveObject(EditorActions.ObjectType.Sink, _editor.PickingResult.Element,
                            _editor.PickingResult.GizmoAxis, delta, smooth);
                    }

                    if (_editor.PickingResult.ElementType == PickingElementType.SoundSource)
                    {
                        EditorActions.MoveObject(EditorActions.ObjectType.SoundSource, _editor.PickingResult.Element,
                            _editor.PickingResult.GizmoAxis, delta, smooth);
                    }

                    if (_editor.PickingResult.ElementType == PickingElementType.Light)
                    {
                        EditorActions.MoveObject(EditorActions.ObjectType.Light, _editor.PickingResult.Element,
                            _editor.PickingResult.GizmoAxis, delta, smooth);
                        _editor.SelectedRoom.CalculateLightingForThisRoom();
                        _editor.SelectedRoom.UpdateBuffers();
                    }

                    if (_editor.PickingResult.ElementType == PickingElementType.Moveable)
                    {
                        EditorActions.MoveObject(EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
                            _editor.PickingResult.GizmoAxis, delta, smooth);
                    }

                    if (_editor.PickingResult.ElementType == PickingElementType.StaticMesh)
                    {
                        EditorActions.MoveObject(EditorActions.ObjectType.StaticMesh, _editor.PickingResult.Element,
                            _editor.PickingResult.GizmoAxis, delta, smooth);
                    }

                    Draw();

                    return;
                }
                
                // calculate block selection
                switch (_editor.Mode)
                {
                    case EditorMode.Geometry:
                        if (_editor.Action == EditorAction.None)
                        {
                            PickingResult newPicking = DoPicking(e.X, e.Y);
                            if (_editor.PickingResult.ElementType != PickingElementType.Block)
                            {
                                _editor.BlockSelectionReset();
                                _firstSelection = true;
                                _editor.BlockEditingType = 0;

                                return;
                            }
                            
                            if (_firstSelection)
                            {
                                _editor.BlockSelectionEnd = new System.Drawing.Point(newPicking.Element >> 5, newPicking.Element & 31);
                                _editor.BlockEditingType = 0;
                                _firstSelection = true;
                            }
                        }
                        break;

                    case EditorMode.Map2D:
                        break;

                    case EditorMode.FaceEdit:
                        if (_editor.Action == EditorAction.None)
                            PlaceTexture();

                        break;
                }
            }

            if (Drag)
                Draw();
            _editor.DrawPanelGrid();
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                return;

            this.Focus();
            this.Select();

            LastX = e.X;
            LastY = e.Y;

            // gestisco il drag & drop e la telecamera
            if (e.Button == MouseButtons.Right)
            {
                DeltaX = e.X - LastX;
                DeltaY = e.Y - LastY;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    Camera.Move(-DeltaY / 10.0f);
                }
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    Camera.Translate(new Vector3(DeltaX, -DeltaY, 0));
                }
                else
                {
                    Camera.Rotate(DeltaX / 5000, -DeltaY / 5000);
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                switch (_editor.Mode)
                {
                    case EditorMode.Geometry:
                        if (_editor.Action == EditorAction.PlaceNoCollision)
                        {
                            PlaceNoCollision();
                        }
                        else if (_editor.Action == EditorAction.None)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.StaticMesh)
                            {
                                _editor.LoadStaticMeshColorInUI();
                            }
                            else
                            {
                                // se la selezione ha come x o z uguali a -1 allora non la inizio neanche
                                if (_editor.PickingResult.ElementType != PickingElementType.Block)
                                {
                                    _editor.BlockSelectionReset();
                                    _firstSelection = true;
                                    _editor.BlockEditingType = 0;

                                    return;
                                }

                                // verifico se si tratta di una prima selezione o no
                                if (_firstSelection)
                                {
                                    logger.Debug($"Selected range: ({_editor.BlockSelectionStart.X}, {_editor.BlockSelectionStart.Y}) - ({_editor.BlockSelectionEnd.X}, {_editor.BlockSelectionEnd.Y})");
                                    _editor.LoadTriggersInUI();
                                    _editor.BlockEditingType = 0;
                                }
                                else
                                {
                                    // ruoto le frecce
                                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                                    {
                                        if (_editor.BlockEditingType == 8)
                                        {
                                            _editor.BlockEditingType = 0;
                                        }
                                        else
                                        {
                                            if (_editor.BlockEditingType == 7)
                                                _editor.BlockEditingType = 8;
                                            else if (_editor.BlockEditingType == 6)
                                                _editor.BlockEditingType = 7;
                                            else if (_editor.BlockEditingType == 5)
                                                _editor.BlockEditingType = 6;
                                            else
                                                _editor.BlockEditingType = 5;
                                        }
                                    }
                                    else
                                    {
                                        if (_editor.BlockEditingType == 4)
                                        {
                                            _editor.BlockEditingType = 0;
                                        }
                                        else
                                        {
                                            if (_editor.BlockEditingType == 3)
                                                _editor.BlockEditingType = 4;
                                            else if (_editor.BlockEditingType == 2)
                                                _editor.BlockEditingType = 3;
                                            else if (_editor.BlockEditingType == 1)
                                                _editor.BlockEditingType = 2;
                                            else
                                                _editor.BlockEditingType = 1;
                                        }
                                    }
                                }
                            }
                        }
                        else if (_editor.Action == EditorAction.PlaceItem)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceItem();
                        }
                        else if (_editor.Action == EditorAction.PlaceSink)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceSink();
                        }
                        else if (_editor.Action == EditorAction.PlaceCamera)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceCamera();
                        }
                        else if (_editor.Action == EditorAction.PlaceFlyByCamera)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceFlyByCamera();
                        }
                        else if (_editor.Action == EditorAction.PlaceSound)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceSound();
                        }

                        break;

                    case EditorMode.Map2D:

                        break;

                    case EditorMode.Lighting:
                        if (_editor.Action == EditorAction.PlaceLight)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceLight();
                        }
                        else
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Light)
                            {
                                _editor.EditLight();
                            }
                            else
                            {
                                _editor.LightIndex = -1;
                            }

                            if (_editor.PickingResult.ElementType == PickingElementType.StaticMesh)
                            {
                                _editor.LoadStaticMeshColorInUI();
                            }
                        }

                        break;

                    case EditorMode.FaceEdit:
                        if (_editor.Action == EditorAction.PlaceItem)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceItem();
                        }
                        else if (_editor.Action == EditorAction.PlaceSink)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceSink();
                        }
                        else if (_editor.Action == EditorAction.PlaceCamera)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceCamera();
                        }
                        else if (_editor.Action == EditorAction.PlaceFlyByCamera)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceFlyByCamera();
                        }
                        else if (_editor.Action == EditorAction.PlaceSound)
                        {
                            if (_editor.PickingResult.ElementType == PickingElementType.Block)
                                PlaceSound();
                        }

                        break;
                }
            }

            Drag = false;

            Draw();
            _editor.DrawPanelGrid();
        }

        private void DrawObjectHeightLine(Matrix viewProjection)
        {
            _editor.GraphicsDevice.SetRasterizerState(_rasterizerWireframe);
            _editor.GraphicsDevice.SetVertexBuffer(_objectHeightLineVertex);
            _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _objectHeightLineVertex));

            Effect solidEffect = _editor.Effects["Solid"];

            Matrix model = Matrix.Identity *
                           Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));

            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            solidEffect.Parameters["SelectionEnabled"].SetValue(false);

            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.Draw(PrimitiveType.LineList, 2);
        }

        private void DrawLights(Matrix viewProjection, Room room)
        {
            if (room == null)
                return;

            _editor.GraphicsDevice.SetRasterizerState(_rasterizerWireframe);
            _editor.GraphicsDevice.SetVertexBuffer(_littleSphere.VertexBuffer);
            _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleSphere.VertexBuffer));
            _editor.GraphicsDevice.SetIndexBuffer(_littleSphere.IndexBuffer, _littleSphere.IsIndex32Bits);

            Effect solidEffect = _editor.Effects["Solid"];

            for (int i = 0; i < room.Lights.Count; i++)
            {
                Light light = room.Lights[i];

                /*if (light.Type==LightType.Spot)
                {
                    //_editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);

                    _editor.GraphicsDevice.SetVertexBuffer(_littleWireframedCube.VertexBuffer);
                    _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleWireframedCube.VertexBuffer));
                    _editor.GraphicsDevice.SetIndexBuffer(_littleWireframedCube.IndexBuffer, false);

                    Effect effect = _editor.Effects["Solid"];

                    //effect.Parameters["TextureEnabled"].SetValue(true);
                    //effect.Parameters["Texture"].SetResource<Texture2D>(_editor.Textures["spotlight"]);

                    effect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 0.25f, 1.0f));

                    Matrix model = Matrix.Translation(light.Position) * Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.Level.Rooms[room].Position));
                    effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                      
                    effect.CurrentTechnique.Passes[0].Apply();
                    _editor.GraphicsDevice.DrawIndexed(PrimitiveType.LineList, 49);

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

                if (_editor.LightIndex == i)
                    solidEffect.Parameters["SelectionEnabled"].SetValue(true);
                else
                    solidEffect.Parameters["SelectionEnabled"].SetValue(false);

                solidEffect.CurrentTechnique.Passes[0].Apply();
                _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
            }

            solidEffect.Parameters["SelectionEnabled"].SetValue(false);

            if (_editor.LightIndex != -1)
            {
                Light light = room.Lights[_editor.LightIndex];

                if (light.Type == LightType.Light || light.Type == LightType.Shadow || light.Type == LightType.FogBulb)
                {
                    _editor.GraphicsDevice.SetVertexBuffer(_sphere.VertexBuffer);
                    _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
                    _editor.GraphicsDevice.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);

                    if (light.Type == LightType.Light || light.Type == LightType.Shadow)
                    {
                        Matrix model = Matrix.Scaling(light.In * 2.0f) * Matrix.Translation(light.Position) *
                                       Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                        solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                        solidEffect.CurrentTechnique.Passes[0].Apply();
                        _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList,
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
                        _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList,
                            _littleSphere.IndexBuffer.ElementCount);
                    }
                }
                else if (light.Type == LightType.Spot)
                {
                    _editor.GraphicsDevice.SetVertexBuffer(_cone.VertexBuffer);
                    _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _editor.GraphicsDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    // Inner cone
                    float coneAngle = (float) Math.Atan2(512, 1024);
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

                    _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);

                    // Outer cone
                    float cutoffScaleH = light.Cutoff;
                    float cutoffScaleW = MathUtil.DegreesToRadians(light.Out) / coneAngle * cutoffScaleH;

                    Matrix model2 = Matrix.Scaling(cutoffScaleW, cutoffScaleW, cutoffScaleH) * rotation *
                                    Matrix.Translation(light.Position) *
                                    Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model2 * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }
                else if (light.Type == LightType.Sun)
                {
                    _editor.GraphicsDevice.SetVertexBuffer(_cone.VertexBuffer);
                    _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _editor.GraphicsDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    Matrix rotation = Matrix.RotationAxis(-Vector3.UnitX, MathUtil.DegreesToRadians(light.DirectionX)) *
                                      Matrix.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(light.DirectionY));

                    Matrix model = Matrix.Scaling(0.01f, 0.01f, 1.0f) * rotation * Matrix.Translation(light.Position) *
                                   Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
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

                message += " (" + _editor.LightIndex + ")";

                // Object position
                message += Environment.NewLine + GetObjectPositionString(light.Position);

                Matrix modelViewProjection =
                    Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position)) *
                    viewProjection;
                Vector3 screenPos = Vector3.Project(light.Position, 0, 0, Width, Height,
                    _editor.GraphicsDevice.Viewport.MinDepth,
                    _editor.GraphicsDevice.Viewport.MaxDepth, modelViewProjection);
                Debug.AddString(message, screenPos);

                // Add the line height of the object
                AddObjectHeightLine(light.Position, viewProjection);

                // Draw gizmo
                _drawGizmo = true;
                _gizmo.Position = light.Position;
            }

            _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
        }

        private void DrawObjects(Matrix viewProjection, Room room)
        {
            Effect effect = _editor.Effects["Solid"];

            _editor.GraphicsDevice.SetVertexBuffer(_littleCube.VertexBuffer);
            _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleCube.VertexBuffer));
            _editor.GraphicsDevice.SetIndexBuffer(_littleCube.IndexBuffer, _littleCube.IsIndex32Bits);

            for (int i = 0; i < _camerasToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_camerasToDraw[i]];

                _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
                var color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                if (_editor.PickingResult.ElementType == PickingElementType.Camera &&
                    instance.Id == _editor.PickingResult.Element)
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _editor.GraphicsDevice.SetRasterizerState(_rasterizerWireframe);

                    string message = "Camera (" + instance.Id + ")";

                    // Object position
                    message += Environment.NewLine + GetObjectPositionString(instance.Position);

                    Matrix modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _editor.GraphicsDevice.Viewport.MinDepth,
                        _editor.GraphicsDevice.Viewport.MaxDepth, modelViewProjection);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if ((trigger.TargetType == TriggerTargetType.Object ||
                             trigger.TargetType == TriggerTargetType.Camera) && trigger.Target == instance.Id)
                        {
                            message += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                       trigger.Room + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(instance.Position, viewProjection);

                    _drawGizmo = true;
                    _gizmo.Position = instance.Position;
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));

                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            for (int i = 0; i < _flybyToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_flybyToDraw[i]];

                _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);

                Vector4 color = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
                if (_editor.PickingResult.ElementType == PickingElementType.FlyByCamera &&
                    instance.Id == _editor.PickingResult.Element)
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _editor.GraphicsDevice.SetRasterizerState(_rasterizerWireframe);
                }

                FlybyCameraInstance flyby = (FlybyCameraInstance) instance;

                if (_editor.PickingResult.ElementType == PickingElementType.FlyByCamera &&
                    _editor.PickingResult.Element == instance.Id)
                {
                    string message = "Flyby Camera (" + flyby.Sequence + ":" + flyby.Number + ")";

                    // Object position
                    message += Environment.NewLine + GetObjectPositionString(instance.Position);

                    Matrix modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _editor.GraphicsDevice.Viewport.MinDepth,
                        _editor.GraphicsDevice.Viewport.MaxDepth, modelViewProjection);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if ((trigger.TargetType == TriggerTargetType.Object ||
                             trigger.TargetType == TriggerTargetType.FlyByCamera) && trigger.Target == instance.Id)
                        {
                            message += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                       trigger.Room + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(instance.Position, viewProjection);

                    _drawGizmo = true;
                    _gizmo.Position = instance.Position;
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));

                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            for (int i = 0; i < _sinksToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_sinksToDraw[i]];

                _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);

                Vector4 color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                if (_editor.PickingResult.ElementType == PickingElementType.Sink &&
                    instance.Id == _editor.PickingResult.Element)
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _editor.GraphicsDevice.SetRasterizerState(_rasterizerWireframe);

                    var message = "Sink (" + instance.Id + ")";

                    // Object position
                    message += Environment.NewLine + GetObjectPositionString(instance.Position);

                    var modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _editor.GraphicsDevice.Viewport.MinDepth,
                        _editor.GraphicsDevice.Viewport.MaxDepth, modelViewProjection);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if ((trigger.TargetType == TriggerTargetType.Object ||
                             trigger.TargetType == TriggerTargetType.Sink) && trigger.Target == instance.Id)
                        {
                            message += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                       trigger.Room + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(instance.Position, viewProjection);

                    _drawGizmo = true;
                    _gizmo.Position = instance.Position;
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            for (int i = 0; i < _soundSourcesToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_soundSourcesToDraw[i]];

                _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);

                Vector4 color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                if (_editor.PickingResult.ElementType == PickingElementType.SoundSource &&
                    instance.Id == _editor.PickingResult.Element)
                {
                    color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    _editor.GraphicsDevice.SetRasterizerState(_rasterizerWireframe);

                    SoundInstance sound = (SoundInstance) instance;

                    string message = "Sound Source (" + _editor.Level.Wad.OriginalWad.Sounds[sound.SoundId] + ") (" +
                                     instance.Id + ")";

                    // Object position
                    message += Environment.NewLine + GetObjectPositionString(instance.Position);

                    Matrix modelViewProjection =
                        Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position)) *
                        viewProjection;
                    Vector3 screenPos = Vector3.Project(instance.Position, 0, 0, Width, Height,
                        _editor.GraphicsDevice.Viewport.MinDepth,
                        _editor.GraphicsDevice.Viewport.MaxDepth, modelViewProjection);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if ((trigger.TargetType == TriggerTargetType.Object) && trigger.Target == instance.Id)
                        {
                            message += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                       trigger.Room + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(instance.Position, viewProjection);

                    _drawGizmo = true;
                    _gizmo.Position = instance.Position;
                }

                Matrix model = Matrix.Translation(instance.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(color);

                effect.Techniques[0].Passes[0].Apply();
                _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
            }

            _editor.GraphicsDevice.SetVertexBuffer(_cone.VertexBuffer);
            _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
            _editor.GraphicsDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);
            _editor.GraphicsDevice.SetRasterizerState(_rasterizerWireframe);

            for (int i = 0; i < _flybyToDraw.Count; i++)
            {
                ObjectInstance instance = _editor.Level.Objects[_flybyToDraw[i]];

                FlybyCameraInstance flyby = (FlybyCameraInstance) instance;

                // Outer cone
                float coneAngle = (float) Math.Atan2(512, 1024);
                float cutoffScaleH = 1;
                float cutoffScaleW = MathUtil.DegreesToRadians(flyby.Fov / 2) / coneAngle * cutoffScaleH;

                Matrix rotation = Matrix.RotationAxis(-Vector3.UnitX, MathUtil.DegreesToRadians(flyby.DirectionX)) *
                                  Matrix.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(flyby.DirectionY));

                Matrix model = Matrix.Scaling(cutoffScaleW, cutoffScaleW, cutoffScaleH) * rotation *
                               Matrix.Translation(flyby.Position) *
                               Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position));
                effect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                effect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                effect.CurrentTechnique.Passes[0].Apply();
                _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
            }

            _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
        }

        private void DrawMoveables(Matrix viewProjection)
        {
            _editor.GraphicsDevice.SetBlendState(_editor.GraphicsDevice.BlendStates.Opaque);

            Effect skinnedModelEffect = _editor.Effects["Model"];

            skinnedModelEffect.Parameters["TextureEnabled"].SetValue(true);
            skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(false);

            MoveableInstance _lastObject = null;

            skinnedModelEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_editor.GraphicsDevice.SamplerStates.Default);

            for (int k = 0; k < MoveablesToDraw.Count; k++)
            {
                MoveableInstance modelInfo = (MoveableInstance) _editor.Level.Objects[MoveablesToDraw[k]];

                Debug.NumMoveables++;

                SkinnedModel model = modelInfo.Model;

                if (k == 0 || model.ObjectID != _lastObject.ObjectId)
                {
                    _editor.GraphicsDevice.SetVertexBuffer(0, model.VertexBuffer);
                    _editor.GraphicsDevice.SetIndexBuffer(model.IndexBuffer, true);
                }

                if (k == 0)
                {
                    _editor.GraphicsDevice.SetVertexInputLayout(
                        VertexInputLayout.FromBuffer<SkinnedVertex>(0, model.VertexBuffer));
                }

                if (_editor.PickingResult.ElementType == PickingElementType.Moveable &&
                    _editor.PickingResult.Element == modelInfo.Id)
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

                    world = model.AnimationTransforms[i] *
                            Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.Rotation)) *
                            Matrix.Translation(modelInfo.Position) *
                            Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));
                    worldDebug = Matrix.Translation(Utils.PositionInWorldCoordinates(theRoom.Position));

                    skinnedModelEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);

                    skinnedModelEffect.Techniques[0].Passes[0].Apply();
                    _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                    Debug.NumVertices += mesh.NumIndices;
                    Debug.NumTriangles += mesh.NumIndices / 3;
                }

                if (_editor.PickingResult.ElementType == PickingElementType.Moveable &&
                    _editor.PickingResult.Element == modelInfo.Id)
                {
                    Matrix modelViewProjection = worldDebug * viewProjection;
                    Vector3 screenPos = Vector3.Project(modelInfo.Position + 512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _editor.GraphicsDevice.Viewport.MinDepth,
                        _editor.GraphicsDevice.Viewport.MaxDepth, modelViewProjection);

                    string debugMessage = _editor.MoveableNames[(int)model.ObjectID] + " (" + modelInfo.Id + ")";

                    // Object position
                    debugMessage += Environment.NewLine + GetObjectPositionString(modelInfo.Position);

                    // Add OCB
                    if (modelInfo.Ocb != 0) debugMessage += Environment.NewLine + "OCB: " + modelInfo.Ocb;

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if (trigger.TargetType == TriggerTargetType.Object && trigger.Target == modelInfo.Id)
                        {
                            debugMessage += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                            trigger.Room + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(debugMessage, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(modelInfo.Position, viewProjection);

                    // Draw gizmo
                    _drawGizmo = true;
                    _gizmo.Position = modelInfo.Position;
                }

                _lastObject = modelInfo;
            }
        }

        private void DrawStaticMeshes(Matrix viewProjection)
        {
            _editor.GraphicsDevice.SetBlendState(_editor.GraphicsDevice.BlendStates.Opaque);

            Effect staticMeshEffect = _editor.Effects["StaticModel"];

            staticMeshEffect.Parameters["TextureEnabled"].SetValue(true);
            staticMeshEffect.Parameters["SelectionEnabled"].SetValue(false);
            staticMeshEffect.Parameters["LightEnabled"].SetValue(true);

            staticMeshEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
            staticMeshEffect.Parameters["TextureSampler"].SetResource(_editor.GraphicsDevice.SamplerStates.Default);

            StaticMeshInstance _lastObject = null;

            for (int k = 0; k < StaticMeshesToDraw.Count; k++)
            {
                StaticMeshInstance modelInfo = (StaticMeshInstance) _editor.Level.Objects[StaticMeshesToDraw[k]];
                StaticModel model = modelInfo.Model;

                if (k == 0)
                {
                    _editor.GraphicsDevice.SetVertexInputLayout(
                        VertexInputLayout.FromBuffer<StaticVertex>(0, model.VertexBuffer));
                }

                if (k == 0 || model.ObjectID != _lastObject.ObjectId)
                {
                    _editor.GraphicsDevice.SetVertexBuffer(0, model.VertexBuffer);
                    _editor.GraphicsDevice.SetIndexBuffer(model.IndexBuffer, true);
                }

                Debug.NumStaticMeshes++;

                bool SelectionEnabled = _editor.PickingResult.ElementType == PickingElementType.StaticMesh &&
                                        _editor.PickingResult.Element == modelInfo.Id;
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
                    _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                    Debug.NumVertices += mesh.NumIndices;
                    Debug.NumTriangles += mesh.NumIndices / 3;
                }

                if (_editor.PickingResult.ElementType == PickingElementType.StaticMesh &&
                    _editor.PickingResult.Element == modelInfo.Id)
                {
                    Matrix modelViewProjection = worldDebug * viewProjection;
                    Vector3 screenPos = Vector3.Project(modelInfo.Position + 512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _editor.GraphicsDevice.Viewport.MinDepth,
                        _editor.GraphicsDevice.Viewport.MaxDepth, modelViewProjection);

                    string debugMessage = _editor.StaticNames[(int)model.ObjectID] + " (" + modelInfo.Id + ")";

                    // Object position
                    debugMessage += Environment.NewLine + GetObjectPositionString(modelInfo.Position);

                    for (int n = 0; n < _editor.Level.Triggers.Count; n++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(n).Value;
                        if (trigger.TargetType == TriggerTargetType.Object && trigger.Target == modelInfo.Id)
                        {
                            debugMessage += Environment.NewLine + "Triggered by Trigger #" + trigger.Id + " in Room #" +
                                            trigger.Room + " at X = " + trigger.X + ", Z = " + trigger.Z;
                        }
                    }

                    Debug.AddString(debugMessage, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(modelInfo.Position, viewProjection);

                    // Draw gizmo
                    _drawGizmo = true;
                    _gizmo.Position = modelInfo.Position;
                }

                _lastObject = modelInfo;
            }
        }

        private void DrawSkyBox(Matrix viewProjection)
        {
            if (_editor.Level.Wad == null)
                return;
            if (!_editor.Level.Wad.Moveables.ContainsKey(459))
                return;

            _editor.GraphicsDevice.SetBlendState(_editor.GraphicsDevice.BlendStates.Opaque);

            Effect skinnedModelEffect = _editor.Effects["Model"];

            skinnedModelEffect.Parameters["TextureEnabled"].SetValue(true);
            skinnedModelEffect.Parameters["SelectionEnabled"].SetValue(false);

            SkinnedModel skinnedModel = _editor.Level.Wad.Moveables[459];
            skinnedModel.BuildAnimationPose(skinnedModel.Animations[0].KeyFrames[0]);

            skinnedModelEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_editor.GraphicsDevice.SamplerStates.Default);

            _editor.GraphicsDevice.SetVertexInputLayout(
                VertexInputLayout.FromBuffer<SkinnedVertex>(0, skinnedModel.VertexBuffer));

            _editor.GraphicsDevice.SetVertexBuffer(0, skinnedModel.VertexBuffer);
            _editor.GraphicsDevice.SetIndexBuffer(skinnedModel.IndexBuffer, true);

            for (int i = 0; i < skinnedModel.Meshes.Count; i++)
            {
                SkinnedMesh mesh = skinnedModel.Meshes[i];

                Matrix modelMatrix = Matrix.Scaling(16.0f) * skinnedModel.AnimationTransforms[i] *
                                     Matrix.Translation(new Vector3(Camera.Position.X, Camera.Position.Y - 5120.0f,
                                         Camera.Position.Z));
                skinnedModelEffect.Parameters["ModelViewProjection"].SetValue(modelMatrix * viewProjection);

                skinnedModelEffect.Techniques[0].Passes[0].Apply();
                _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                Debug.NumVertices += mesh.NumIndices;
                Debug.NumTriangles += mesh.NumIndices / 3;
            }
        }

        private Ray ConvertMouseToRay(Vector2 mousePosition)
        {
            Vector3 nearPoint = new Vector3(mousePosition, 0);
            Vector3 farPoint = new Vector3(mousePosition, 1);
            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);

            nearPoint = _editor.GraphicsDevice.Viewport.Unproject(nearPoint, viewProjection, Matrix.Identity,
                Matrix.Identity);
            farPoint = _editor.GraphicsDevice.Viewport.Unproject(farPoint, viewProjection, Matrix.Identity,
                Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        private PickingResult DoPicking(float x, float y)
        {
            // Reset the Viewport because GraphicsDevice is shared also with item panel
            _editor.GraphicsDevice.SetViewports(new ViewportF(0, 0, Width, Height));

            // Get the current ViewProjection matrix
            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);

            // For picking, I'll check first sphere/cubes bounding boxes and then eventually
            Room room = _editor.SelectedRoom;

            // First get the ray in 3D space from X, Y mouse coordinates
            Ray ray = Ray.GetPickRay((int) x, (int) y, _editor.GraphicsDevice.Viewport,
                Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) * viewProjection);

            float minDistance = float.MaxValue;
            float distance = 0;
            PickingResult result = new PickingResult();

            // The gizmo has the priority
            // Don't init a new PickingResult, because we also need the current selected object
            // Simply set Gizmo = true on current PickingResult
            if (_drawGizmo)
            {
                BoundingSphere sphereX = new BoundingSphere(_gizmo.Position + Vector3.UnitX * 1024.0f, 64.0f);
                if (ray.Intersects(ref sphereX, out distance))
                {
                    result = _editor.PickingResult;
                    result.Gizmo = true;
                    result.GizmoAxis = GizmoAxis.X;
                    return result;
                }

                BoundingSphere sphereY = new BoundingSphere(_gizmo.Position + Vector3.UnitY * 1024.0f, 64.0f);
                if (ray.Intersects(ref sphereY, out distance))
                {
                    result = _editor.PickingResult;
                    result.Gizmo = true;
                    result.GizmoAxis = GizmoAxis.Y;
                    return result;
                }

                BoundingSphere sphereZ = new BoundingSphere(_gizmo.Position - Vector3.UnitZ * 1024.0f, 64.0f);
                if (ray.Intersects(ref sphereZ, out distance))
                {
                    result = _editor.PickingResult;
                    result.Gizmo = true;
                    result.GizmoAxis = GizmoAxis.Z;
                    return result;
                }
            }

            // First check lights
            for (int i = 0; i < room.Lights.Count; i++)
            {
                distance = 0;
                BoundingSphere sphere = new BoundingSphere(room.Lights[i].Position, 128.0f);
                if (ray.Intersects(ref sphere, out distance) && distance < minDistance)
                {
                    minDistance = distance;
                    result.ElementType = PickingElementType.Light;
                    result.Element = i;
                }
            }

            // Check for sinks
            for (int i = 0; i < room.Sinks.Count; i++)
            {
                distance = 0;
                BoundingBox box = new BoundingBox(
                    _editor.Level.Objects[room.Sinks[i]].Position - new Vector3(128.0f, 128.0f, 128.0f),
                    _editor.Level.Objects[room.Sinks[i]].Position + new Vector3(128.0f, 128.0f, 128.0f));
                if (ray.Intersects(ref box, out distance) && distance < minDistance)
                {
                    minDistance = distance;
                    result.ElementType = PickingElementType.Sink;
                    result.Element = room.Sinks[i];
                }
            }

            // Check for cameras
            for (int i = 0; i < room.Cameras.Count; i++)
            {
                distance = 0;
                BoundingBox box = new BoundingBox(
                    _editor.Level.Objects[room.Cameras[i]].Position - new Vector3(128.0f, 128.0f, 128.0f),
                    _editor.Level.Objects[room.Cameras[i]].Position + new Vector3(128.0f, 128.0f, 128.0f));
                if (ray.Intersects(ref box, out distance) && distance < minDistance)
                {
                    minDistance = distance;
                    result.ElementType = PickingElementType.Camera;
                    result.Element = room.Cameras[i];
                }
            }

            // Check for sound sources
            for (int i = 0; i < room.SoundSources.Count; i++)
            {
                distance = 0;
                BoundingBox box = new BoundingBox(
                    _editor.Level.Objects[room.SoundSources[i]].Position - new Vector3(128.0f, 128.0f, 128.0f),
                    _editor.Level.Objects[room.SoundSources[i]].Position + new Vector3(128.0f, 128.0f, 128.0f));
                if (ray.Intersects(ref box, out distance) && distance < minDistance)
                {
                    minDistance = distance;
                    result.ElementType = PickingElementType.SoundSource;
                    result.Element = room.SoundSources[i];
                }
            }

            // Check for flyby cameras
            for (int i = 0; i < room.FlyByCameras.Count; i++)
            {
                distance = 0;
                BoundingBox box = new BoundingBox(
                    _editor.Level.Objects[room.FlyByCameras[i]].Position - new Vector3(128.0f, 128.0f, 128.0f),
                    _editor.Level.Objects[room.FlyByCameras[i]].Position + new Vector3(128.0f, 128.0f, 128.0f));
                if (ray.Intersects(ref box, out distance) && distance < minDistance)
                {
                    minDistance = distance;
                    result.ElementType = PickingElementType.FlyByCamera;
                    result.Element = room.FlyByCameras[i];
                }
            }

            // Check for moveables
            for (int i = 0; i < room.Moveables.Count; i++)
            {
                distance = 0;
                MoveableInstance modelInfo = (MoveableInstance) _editor.Level.Objects[room.Moveables[i]];

                SkinnedModel model = modelInfo.Model;
                model.BuildAnimationPose(model.Animations[0].KeyFrames[0]);

                for (int j = 0; j < model.Meshes.Count; j++)
                {
                    SkinnedMesh mesh = model.Meshes[j];

                    Matrix world = model.AnimationTransforms[j] *
                                   Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.Rotation)) *
                                   Matrix.Translation(modelInfo.Position);

                    Vector3 centre = mesh.BoundingSphere.Center;

                    Vector3 min = mesh.BoundingBox.Minimum;
                    Vector3 max = mesh.BoundingBox.Maximum;

                    Vector4 transformedMin;
                    Vector3.Transform(ref min, ref world, out transformedMin);
                    Vector4 transformedMax;
                    Vector3.Transform(ref max, ref world, out transformedMax);

                    BoundingBox box = new BoundingBox(new Vector3(transformedMin.X, transformedMin.Y, transformedMin.Z),
                        new Vector3(transformedMax.X, transformedMax.Y, transformedMax.Z));

                    distance = 0;
                    if (ray.Intersects(ref box, out distance) && distance < minDistance)
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

                            distance = 0;
                            if (ray.Intersects(ref p1, ref p2, ref p3, out distance) && distance < minDistance)
                            {
                                minDistance = distance;
                                result.ElementType = PickingElementType.Moveable;
                                result.Element = room.Moveables[i];
                            }
                        }
                    }
                }
            }

            // Check for static meshes
            for (int i = 0; i < room.StaticMeshes.Count; i++)
            {
                distance = 0;
                StaticMeshInstance modelInfo = (StaticMeshInstance) _editor.Level.Objects[room.StaticMeshes[i]];

                StaticModel model = modelInfo.Model;

                StaticMesh mesh = model.Meshes[0];

                Matrix world = Matrix.RotationY(MathUtil.DegreesToRadians(modelInfo.Rotation)) *
                               Matrix.Translation(modelInfo.Position);

                Vector3 centre = mesh.BoundingSphere.Center;


                Vector3 min = mesh.BoundingBox.Minimum;
                Vector3 max = mesh.BoundingBox.Maximum;

                Vector4 transformedMin;
                Vector3.Transform(ref min, ref world, out transformedMin);
                Vector4 transformedMax;
                Vector3.Transform(ref max, ref world, out transformedMax);

                BoundingBox box = new BoundingBox(new Vector3(transformedMin.X, transformedMin.Y, transformedMin.Z),
                    new Vector3(transformedMax.X, transformedMax.Y, transformedMax.Z));

                distance = 0;
                if (ray.Intersects(ref box, out distance) && distance < minDistance)
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

                        distance = 0;
                        if (ray.Intersects(ref p1, ref p2, ref p3, out distance) && distance < minDistance)
                        {
                            minDistance = distance;
                            result.ElementType = PickingElementType.StaticMesh;
                            result.Element = room.StaticMeshes[i];
                        }
                    }
                }
            }
            
            // Now check geometry
            for (byte sx = 0; sx < room.NumXSectors; sx++)
            {
                for (byte sz = 0; sz < room.NumZSectors; sz++)
                {
                    for (byte f = 0; f < 29; f++)
                    {
                        distance = 0;
                        BlockFace face = room.Blocks[sx, sz].Faces[f];
                        if (!face.Defined)
                            continue;
                        if (room.RayIntersectsFace(ref ray, ref face, out distance) && distance < minDistance)
                        {
                            minDistance = distance;
                            result.ElementType = PickingElementType.Block;
                            result.Element = (sx << 5) + sz;
                            result.SubElement = f;
                        }
                    }
                }
            }
            
            switch (result.ElementType)
            {
                case PickingElementType.Moveable:
                    logger.Debug("Selected: Moveable #" + result.Element);
                    break;

                case PickingElementType.StaticMesh:
                    logger.Debug("Selected: Static Mesh #" + result.Element);
                    break;

                case PickingElementType.Sink:
                    logger.Debug("Selected: Sink #" + result.Element);
                    break;

                case PickingElementType.Camera:
                    logger.Debug("Selected: Camera #" + result.Element);
                    break;

                case PickingElementType.FlyByCamera:
                    logger.Debug("Selected: Flyby camera #" + result.Element);
                    break;

                case PickingElementType.SoundSource:
                    logger.Debug("Selected: Sound source #" + result.Element);
                    break;

                case PickingElementType.Light:
                    logger.Debug("Selected: Light #" + result.Element);
                    break;
            }

            Debug.SelectedItem = "";
            if (result.ElementType == PickingElementType.Moveable)
            {
                MoveableInstance model = (MoveableInstance)_editor.Level.Objects[result.Element];
                Debug.SelectedItem = "Moveable '" + _editor.MoveableNames[(int)model.Model.ObjectID] + "' (" + result.Element + ")";
            }
            else if (result.ElementType == PickingElementType.StaticMesh)
            {
                StaticMeshInstance model = (StaticMeshInstance)_editor.Level.Objects[result.Element];
                Debug.SelectedItem = "Static Mesh '" + _editor.StaticNames[(int)model.Model.ObjectID] + "' (" + result.Element + ")";
            }

            return result;
        }

        private void RotateTexture()
        {
            // recupero le coordinate X e Z della faccia
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            if (_editor.PickingResult.SubElement > 28)
                return;

            // salvo un puntatore alla faccia per accesso rapido
            BlockFace face = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement];
            BlockFaces faceType = (BlockFaces) _editor.PickingResult.SubElement;

            if (_editor.InvisiblePolygon || face.Invisible)
                return;

            if (face.Shape == BlockFaceShape.Triangle)
            {
                Vector2 temp3 = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[2];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[2] =
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[1];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[1] =
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[0];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[0] = temp3;

                if (faceType == BlockFaces.FloorTriangle2)
                {
                    Vector2 temp4 = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement]
                        .TriangleUV2[2];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[2] = _editor
                        .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[1];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[1] = _editor
                        .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[0];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[0] = temp4;
                }

                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].Rotation += 1;
                if (_editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].Rotation == 3)
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].Rotation = 0;
            }
            else
            {
                Vector2 temp2 = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement]
                    .RectangleUV[3];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[3] = _editor
                    .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[2];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[2] = _editor
                    .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[1];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[1] = _editor
                    .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[0];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[0] = temp2;

                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].Rotation += 1;
                if (_editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].Rotation == 4)
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].Rotation = 0;
            }

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();
        }

        private void FlipTexture()
        {
            // recupero le coordinate X e Z della faccia
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            if (_editor.PickingResult.SubElement > 28)
                return;

            // salvo un puntatore alla faccia per accesso rapido
            BlockFace face = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement];
            BlockFaces faceType = (BlockFaces) _editor.PickingResult.SubElement;

            if (_editor.InvisiblePolygon || face.Invisible || face.Texture == -1)
                return;

            if (face.Shape == BlockFaceShape.Triangle)
            {
                Vector2[] UV = new Vector2[4];

                // calcolo le nuove UV
                LevelTexture texture = _editor.Level.TextureSamples[face.Texture];

                UV[0] = new Vector2(texture.X / 256.0f, texture.Y / 256.0f);
                UV[1] = new Vector2((texture.X + texture.Width) / 256.0f, texture.Y / 256.0f);

                UV[2] = new Vector2((texture.X + texture.Width) / 256.0f, (texture.Y + texture.Height) / 256.0f);
                UV[3] = new Vector2(texture.X / 256.0f, (texture.Y + texture.Height) / 256.0f);

                if (_editor.TextureTriangle == TextureTileType.TriangleNW)
                {
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[0] = UV[1];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[1] = UV[0];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[2] = UV[2];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[0] =
                            UV[1];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[1] =
                            UV[0];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[2] =
                            UV[2];
                    }
                }

                if (_editor.TextureTriangle == TextureTileType.TriangleNE)
                {
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[0] = UV[0];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[1] = UV[3];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[2] = UV[1];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[0] =
                            UV[0];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[1] =
                            UV[3];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[2] =
                            UV[1];
                    }
                }

                if (_editor.TextureTriangle == TextureTileType.TriangleSE)
                {
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[0] = UV[3];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[1] = UV[2];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[2] = UV[0];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[0] =
                            UV[3];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[1] =
                            UV[2];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[2] =
                            UV[0];
                    }
                }

                if (_editor.TextureTriangle == TextureTileType.TriangleSW)
                {
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[0] = UV[2];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[1] = UV[1];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[2] = UV[3];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[0] =
                            UV[2];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[1] =
                            UV[1];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[2] =
                            UV[3];
                    }
                }

                for (int k = 0; k < face.Rotation; k++)
                {
                    Vector2 temp3 = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement]
                        .TriangleUV[2];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[2] = _editor
                        .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[1];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[1] = _editor
                        .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[0];
                    _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV[0] = temp3;

                    if (faceType == BlockFaces.FloorTriangle2)
                    {
                        Vector2 temp4 = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement]
                            .TriangleUV2[2];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[2] =
                            _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[1];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[1] =
                            _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[0];
                        _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].TriangleUV2[0] =
                            temp4;
                    }
                }
            }
            else
            {
                Vector2 temp2 = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement]
                    .RectangleUV[1];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[1] = _editor
                    .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[0];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[0] = temp2;

                temp2 = _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[3];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[3] = _editor
                    .SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[2];
                _editor.SelectedRoom.Blocks[x, z].Faces[_editor.PickingResult.SubElement].RectangleUV[2] = temp2;
            }

            face.Flipped = !face.Flipped;

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();
        }

        private void PlaceTexture()
        {
            // Recover the X and Z coordinates of the face
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            if (_editor.PickingResult.ElementType != PickingElementType.Block)
                return;
            if (_editor.PickingResult.SubElement > 28)
                return;

            BlockFaces faceType = (BlockFaces) _editor.PickingResult.SubElement;
            EditorActions.PlaceTexture(x, z, faceType);
        }

        private void PlaceNoCollision()
        {
            // Recover the X and Z coordinates of the face
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            BlockFaces faceType = (BlockFaces) _editor.PickingResult.SubElement;

            EditorActions.PlaceNoCollision(x, z, faceType);
        }

        private void PlaceItem()
        {
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            if (_editor.ItemType == EditorItemType.Moveable)
                EditorActions.PlaceObject(x, z, EditorActions.ObjectType.Moveable, _editor.SelectedItem);
            else
                EditorActions.PlaceObject(x, z, EditorActions.ObjectType.StaticMesh, _editor.SelectedItem);

            _editor.Action = EditorAction.None;
        }

        private void PlaceCamera()
        {
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            EditorActions.PlaceObject(x, z, EditorActions.ObjectType.Camera, 0);

            _editor.Action = EditorAction.None;
        }

        private void PlaceFlyByCamera()
        {
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            EditorActions.PlaceObject(x, z, EditorActions.ObjectType.FlybyCamera, 0);

            _editor.Action = EditorAction.None;
        }

        private void PlaceSink()
        {
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            EditorActions.PlaceObject(x, z, EditorActions.ObjectType.Sink, 0);

            _editor.Action = EditorAction.None;
        }

        private void PlaceSound()
        {
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            EditorActions.PlaceObject(x, z, EditorActions.ObjectType.SoundSource, 0);

            _editor.Action = EditorAction.None;
        }

        private void PlaceLight()
        {
            int x = _editor.PickingResult.Element >> 5;
            int z = _editor.PickingResult.Element & 31;

            EditorActions.PlaceLight(x, z, _editor.LightType);

            Invalidate();

            _editor.Action = EditorAction.None;
        }

        private Vector4 GetSharpdDXColor(System.Drawing.Color color)
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

            Stack<Room> stackRooms = new Stack<Room>();
            Stack<int> stackLimits = new Stack<int>();

            stackRooms.Push(room);
            stackLimits.Push(0);

            while (stackRooms.Count > 0)
            {
                var theRoom = stackRooms.Pop();
                int theLimit = stackLimits.Pop();

                if (theLimit > Configuration.DrawRoomsMaxDepth)
                    continue;

                if (_editor.IsFlipMap)
                {
                    if (!theRoom.Flipped)
                    {
                        theRoom.Visited = true;
                        _roomsToDraw.Add(theRoom);
                    }
                    else
                    {
                        if (theRoom.AlternateRoom != null)
                        {
                            theRoom.Visited = true;
                            _roomsToDraw.Add(theRoom.AlternateRoom);
                        }
                        else
                        {
                            theRoom.Visited = true;
                            _roomsToDraw.Add(theRoom);
                        }
                    }
                }
                else
                {
                    theRoom.Visited = true;
                    _roomsToDraw.Add(theRoom);
                }

                for (int p = 0; p < theRoom.Portals.Count; p++)
                {
                    Portal portal = _editor.Level.Portals[theRoom.Portals[p]];

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

                    Vector3 cameraDirection = Camera.Position - Camera.Target;

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

        private void PrepareRenderBuckets()
        {
            _opaqueBuckets = new List<RenderBucket>();
            _transparentBuckets = new List<RenderBucket>();
            _invisibleBuckets = new List<RenderBucket>();
            _solidBuckets = new List<RenderBucket>();

            BlockFace face;
            Vector3 viewVector = Camera.Target - Camera.Position;

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
                                            Vector4 centre = Vector4.Zero;

                                            for (int j = 0; j < face.Vertices.Length; j++)
                                            {
                                                centre += face.Vertices[j].Position;
                                            }

                                            centre /= face.Vertices.Length;
                                            bucket.Centre = new Vector3(centre.X, centre.Y, centre.Z);

                                            // calcolo la distanza
                                            bucket.Distance = (bucket.Centre - Camera.Position).Length();

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
            item.IndexBuffer = SharpDX.Toolkit.Graphics.Buffer.New(Editor.Instance.GraphicsDevice,
                item.Indices.ToArray(), BufferFlags.IndexBuffer);
        }

        public void Draw()
        {
            if (DesignMode)
                return;

            Stopwatch _watch = new Stopwatch();
            _watch.Start();

            Debug.Reset();

            _drawHeightLine = false;
            _drawGizmo = false;

            // Don't draw anything if device is not ready
            if (_editor.GraphicsDevice == null || _editor.GraphicsDevice.Presenter == null ||
                _editor.SelectedRoom == null)
                return;

            // reset the backbuffer
            _editor.GraphicsDevice.Presenter = Presenter;
            _editor.GraphicsDevice.SetViewports(new ViewportF(0, 0, Width, Height));
            _editor.GraphicsDevice.SetRenderTargets(_editor.GraphicsDevice.Presenter.DepthStencilBuffer,
                _editor.GraphicsDevice.Presenter.BackBuffer);
            _editor.GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target,
                (!_editor.IsFlipMap ? Color.CornflowerBlue : Color.LightCoral), 1.0f, 0);
            _editor.GraphicsDevice.SetDepthStencilState(_editor.GraphicsDevice.DepthStencilStates.Default);

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
            if (_editor.DrawPortals)
                CollectRoomsToDraw(_editor.SelectedRoom);
            else
                _roomsToDraw.Add(_editor.SelectedRoom);

            Debug.NumRooms = _roomsToDraw.Count;

            Task task1 = Task.Factory.StartNew(RenderTask1, viewProjection);
            Task task2 = Task.Factory.StartNew(RenderTask2, viewProjection);
            Task.WaitAll(task1, task2);

            // Draw the skybox if present
            if ((_editor.Mode == EditorMode.FaceEdit || _editor.Mode == EditorMode.Lighting) &&
                _editor != null && _editor.Level != null && _editor.Level.Wad != null && _editor.DrawHorizon)
            {
                DrawSkyBox(viewProjection);
                _editor.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1.0f, 0);
            }

            // Draw moveables and static meshes
            if (_editor != null && _editor.Level != null && _editor.Level.Wad != null)
            {
                DrawMoveables(viewProjection);
                DrawStaticMeshes(viewProjection);
            }

            // Prepare the shader
            _roomEffect = _editor.Effects["Room"];

            // Set some common parameters of the shader
            _roomEffect.Parameters["CameraPosition"].SetValue(Camera.Position);
            _roomEffect.Parameters["LightingEnabled"].SetValue(false);
            _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
            _roomEffect.Parameters["TextureEnabled"].SetValue(false);
            _roomEffect.Parameters["EditorTextureEnabled"].SetValue(false);
            _roomEffect.Parameters["TextureSampler"].SetResource(_editor.GraphicsDevice.SamplerStates.AnisotropicWrap);

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
            if (_drawHeightLine) DrawObjectHeightLine(viewProjection);

            // Draw the gizmo
            if (_drawGizmo)
            {
                _editor.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color4.White, 1.0f, 0);
                _gizmo.ViewProjection = viewProjection;
                _gizmo.Draw();
            }

            _watch.Stop();
            long mils = _watch.ElapsedMilliseconds;
            float fps = (mils != 0 ? 1000 / mils : 60);

            _editor.FPS = fps;

            // Draw debug info
            Debug.Draw();

            _editor.GraphicsDevice.Present();

            logger.Debug($"Draw Call! {mils}ms");
        }

        private void RenderTask1(object viewProjection_)
        {
            Matrix viewProjection = (Matrix) viewProjection_;

            // Collect objects to draw
            CollectObjectsToDraw();

            // Now group faces to render based on various things
            PrepareRenderBuckets();
        }

        private void RenderTask2(object viewProjection_)
        {
            Matrix viewProjection = (Matrix) viewProjection_;

            // Add room names
            if (_editor.DrawRoomNames)
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
                Vector3 screenPos = Vector3.Project(_roomsToDraw[i].Centre, 0, 0, Width, Height,
                    _editor.GraphicsDevice.Viewport.MinDepth,
                    _editor.GraphicsDevice.Viewport.MaxDepth, wvp);
                Debug.AddString(message, screenPos);
            }
        }

        private void AddDirectionsToDebug(Matrix viewProjection)
        {
            float xBlocks = _editor.SelectedRoom.NumXSectors / 2.0f * 1024.0f;
            float zBlocks = _editor.SelectedRoom.NumZSectors / 2.0f * 1024.0f;

            string[] messages = {"North", "South", "East", "West"};
            Vector3[] positions = new Vector3[4];

            Vector3 centre = _editor.SelectedRoom.Centre;
            Vector3 pos = Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position);

            positions[0] = centre + new Vector3(0, 0, zBlocks);
            positions[1] = centre + new Vector3(0, 0, -zBlocks);
            positions[2] = centre + new Vector3(xBlocks, 0, 0);
            positions[3] = centre + new Vector3(-xBlocks, 0, 0);

            Matrix wvp = Matrix.Translation(pos) * viewProjection;

            for (int i = 0; i < 4; i++)
            {
                Vector3 screenPos = Vector3.Project(positions[i], 0, 0, Width, Height,
                    _editor.GraphicsDevice.Viewport.MinDepth,
                    _editor.GraphicsDevice.Viewport.MaxDepth, wvp);
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
                    _editor.GraphicsDevice.SetVertexBuffer(room.VertexBuffer);
                    _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
                    _roomEffect.Parameters["ModelViewProjection"].SetValue(room.Transform * viewProjection);

                    // Enable or disable static lighting
                    bool lights = (room != _editor.SelectedRoom ||
                                   (room == _editor.SelectedRoom && _editor.Mode == EditorMode.Lighting));
                    _roomEffect.Parameters["LightingEnabled"].SetValue(lights);
                }

                _editor.GraphicsDevice.SetIndexBuffer(bucket.IndexBuffer, false);

                // Enable or disable double sided textures
                if (_lastBucket == null || _lastBucket.DoubleSided != bucket.DoubleSided)
                {
                    if (bucket.DoubleSided == 1)
                        _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullNone);
                    else
                        _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
                }

                // Change texture if needed
                if (_lastBucket == null)
                {
                    _roomEffect.Parameters["Texture"].SetResource(_editor.Level.Textures[0]);
                    _roomEffect.Parameters["TextureSampler"]
                        .SetResource(_editor.GraphicsDevice.SamplerStates.AnisotropicWrap);
                }

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the face
                _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList,
                    bucket.IndexBuffer.ElementCount); // face.Vertices.Count, face.StartVertex);

                Debug.NumVertices += bucket.IndexBuffer.ElementCount;
                Debug.NumTriangles += bucket.IndexBuffer.ElementCount / 3;

                _lastBucket = bucket;
            }

            _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
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
                int index = (int) bucket.FaceType;
                BlockFace face = bucket.Face;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _editor.GraphicsDevice.SetVertexBuffer(room.VertexBuffer);
                    _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
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
                _editor.GraphicsDevice.Draw(PrimitiveType.TriangleList, face.Vertices.Length, face.StartVertex);

                Debug.NumVertices += face.Vertices.Length;
                Debug.NumTriangles += face.Vertices.Length / 3;

                _lastBucket = bucket;
            }

            _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
        }

        private void DrawTransparentBuckets(Matrix viewProjection)
        {
            _editor.GraphicsDevice.SetBlendState(_editor.GraphicsDevice.BlendStates.Additive);

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
                int index = (int) bucket.FaceType;
                BlockFace face = bucket.Face;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _editor.GraphicsDevice.SetVertexBuffer(room.VertexBuffer);
                    _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
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
                        _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullNone);
                    else
                        _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
                }

                // Change texture if needed
                if (_lastBucket == null /*|| _lastBucket.Texture!=bucket.Texture*/)
                {
                    // DEBUG

                    LevelTexture textureSample = _editor.Level.TextureSamples[face.Texture];
                    _roomEffect.Parameters["Texture"].SetResource(_editor.Level.Textures[0 /*textureSample.Page*/]);
                    _roomEffect.Parameters["TextureSampler"]
                        .SetResource(_editor.GraphicsDevice.SamplerStates.AnisotropicWrap);
                }

                // Set shape
                if (face.Shape == BlockFaceShape.Rectangle)
                    _roomEffect.Parameters["Shape"].SetValue(0);
                else
                    _roomEffect.Parameters["Shape"].SetValue(1);

                _roomEffect.Parameters["SplitMode"].SetValue(face.SplitMode);

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the face
                _editor.GraphicsDevice.Draw(PrimitiveType.TriangleList, face.Vertices.Length, face.StartVertex);

                Debug.NumVertices += face.Vertices.Length;
                Debug.NumTriangles += face.Vertices.Length / 3;

                _lastBucket = bucket;
            }

            SharpDX.Direct3D11.BlendStateDescription desc = new SharpDX.Direct3D11.BlendStateDescription();
            desc.RenderTarget[0].BlendOperation = SharpDX.Direct3D11.BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = SharpDX.Direct3D11.BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = SharpDX.Direct3D11.BlendOption.InverseDestinationAlpha;
            desc.IndependentBlendEnable = false;

            _editor.GraphicsDevice.SetBlendState(_editor.GraphicsDevice.BlendStates.AlphaBlend);
            _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
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
                int index = (int) bucket.FaceType;
                var room = bucket.Room;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _editor.GraphicsDevice.SetVertexBuffer(room.VertexBuffer);
                    _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
                    _roomEffect.Parameters["ModelViewProjection"].SetValue(room.Transform * viewProjection);

                    // Enable or disable static lighting
                    bool lights = (room != _editor.SelectedRoom ||
                                   (room == _editor.SelectedRoom && _editor.Mode == EditorMode.Lighting));
                    _roomEffect.Parameters["LightingEnabled"].SetValue(lights);
                }

                // Calculate the bounds of the current selection
                int xMin = Math.Min(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
                int xMax = Math.Max(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
                int zMin = Math.Min(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);
                int zMax = Math.Max(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);

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
                        switch (_editor.BlockEditingType)
                        {
                            case 0:
                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                break;

                            case 1:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 2:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_right"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 3:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 4:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_left"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 5:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 6:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 7:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 8:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                        }
                    }
                    else if (index == 27 || index == 28)
                    {
                        switch (_editor.BlockEditingType)
                        {
                            case 0:

                                _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                break;
                            case 1:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 2:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_left"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 3:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 4:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_right"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 5:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 6:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 7:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                break;
                            case 8:
                                _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
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
                            qa = (int) BlockFaces.SouthQA;
                            ws = (int) BlockFaces.SouthWS;
                            ed = (int) BlockFaces.SouthED;
                            rf = (int) BlockFaces.SouthRF;
                            middle = (int) BlockFaces.SouthMiddle;

                            switch (_editor.BlockEditingType)
                            {
                                case 0:
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                    break;
                                case 1:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    break;
                                case 2:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 3:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 4:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 5:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 6:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 7:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 8:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
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
                            qa = (int) BlockFaces.WestQA;
                            ws = (int) BlockFaces.WestWS;
                            ed = (int) BlockFaces.WestED;
                            rf = (int) BlockFaces.WestRF;
                            middle = (int) BlockFaces.WestMiddle;

                            switch (_editor.BlockEditingType)
                            {
                                case 0:
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                    break;
                                case 1:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 2:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 3:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 4:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 5:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 6:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 7:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 8:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
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
                            qa = (int) BlockFaces.NorthQA;
                            ws = (int) BlockFaces.NorthWS;
                            ed = (int) BlockFaces.NorthED;
                            rf = (int) BlockFaces.NorthRF;
                            middle = (int) BlockFaces.NorthMiddle;

                            switch (_editor.BlockEditingType)
                            {
                                case 0:
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                    break;
                                case 1:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 2:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 3:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 4:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 5:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 6:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 7:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 8:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
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
                            qa = (int) BlockFaces.EastQA;
                            ws = (int) BlockFaces.EastWS;
                            ed = (int) BlockFaces.EastED;
                            rf = (int) BlockFaces.EastRF;
                            middle = (int) BlockFaces.EastMiddle;

                            switch (_editor.BlockEditingType)
                            {
                                case 0:
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(false);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(true);
                                    break;
                                case 1:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 2:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_down"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_up"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 3:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 4:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 5:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 6:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_se"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_ne"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);

                                    break;
                                case 7:
                                    if (index == rf)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == ws)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_sw"]);
                                    if (index == qa)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    if (index == ed)
                                        _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["arrow_nw"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["SelectionEnabled"].SetValue(false);
                                    break;
                                case 8:
                                    _roomEffect.Parameters["Texture"].SetResource(_editor.Textures["cross"]);
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
                    if (room.Blocks[x, z].WallPortal != -1)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                if ((index == 25 || index == 26) && !noCollision)
                {
                    if (room.Blocks[x, z].FloorPortal != -1)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                if ((index == 27 || index == 28) && !noCollision)
                {
                    if (room.Blocks[x, z].CeilingPortal != -1)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                if (bucket.Face.Shape == BlockFaceShape.Rectangle)
                    _roomEffect.Parameters["Shape"].SetValue(0);
                else
                    _roomEffect.Parameters["Shape"].SetValue(1);

                _roomEffect.Parameters["SplitMode"].SetValue(bucket.Face.SplitMode);

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                _editor.GraphicsDevice.Draw(PrimitiveType.TriangleList, bucket.Face.Vertices.Length,
                    bucket.Face.StartVertex);

                Debug.NumVertices += bucket.Face.Vertices.Length;
                Debug.NumTriangles += bucket.Face.Vertices.Length / 3;

                _lastBucket = bucket;
            }
        }

        private float GetObjectHeight(Vector3 position)
        {
            int xBlock = (int) Math.Floor(position.X / 1024.0f);
            int zBlock = (int) Math.Floor(position.Z / 1024.0f);

            // Get the base floor height
            int floorHeight = _editor.SelectedRoom.GetLowestFloorCorner(xBlock, zBlock);

            // Get the distance between point and floor in units
            float height = position.Y - (float) floorHeight * 256.0f;

            return height;
        }

        private string GetObjectPositionString(Vector3 position)
        {
            int xBlock = (int) Math.Floor(position.X / 1024.0f);
            int zBlock = (int) Math.Floor(position.Z / 1024.0f);

            // Get the base floor height
            int floorHeight = _editor.SelectedRoom.GetLowestFloorCorner(xBlock, zBlock);

            // Get the distance between point and floor in units
            float height = position.Y - (float) floorHeight * 256.0f;

            string message = "Position: [" + position.X + ", " + position.Y + ", " + position.Z + "]";
            message += Environment.NewLine + "Height: " + Math.Round(height) + " units(" + (height / 256.0f) +
                       " clicks)";

            return message;
        }

        private void AddObjectHeightLine(Vector3 position, Matrix viewProjection)
        {
            int xBlock = (int) Math.Floor(position.X / 1024.0f);
            int zBlock = (int) Math.Floor(position.Z / 1024.0f);

            // Get the base floor height
            int floorHeight = _editor.SelectedRoom.GetLowestFloorCorner(xBlock, zBlock);

            // Get the distance between point and floor in clicks
            float height = position.Y / 256.0f - (float) floorHeight;

            // Prepare two vertices for the line
            EditorVertex v1 = new EditorVertex();
            v1.Position = new Vector4(position.X, floorHeight * 256.0f, position.Z, 1.0f);

            EditorVertex v2 = new EditorVertex();
            v2.Position = new Vector4(position.X, position.Y, position.Z, 1.0f);

            EditorVertex[] vertices = new EditorVertex[] {v1, v2};

            // Prepare the Vertex Buffer
            if (_objectHeightLineVertex != null) _objectHeightLineVertex.Dispose();
            _objectHeightLineVertex = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<EditorVertex>(_editor.GraphicsDevice,
                vertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);

            // Add the text description
            /*Vector3 meanPosition = new Vector3(position.X, position.Y / 2.0f, position.Z);
            Matrix modelViewProjection = Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position)) * viewProjection;
            Vector3 screenPos = Vector3.Project(meanPosition, 0, 0, Width, Height, _editor.GraphicsDevice.Viewport.MinDepth,
                            _editor.GraphicsDevice.Viewport.MaxDepth, modelViewProjection);

            //Debug.AddString("Height: " + Math.Round(height * 256.0f) + " units (" + height + " clicks)", screenPos);*/

            _drawHeightLine = true;
        }
    }
}
