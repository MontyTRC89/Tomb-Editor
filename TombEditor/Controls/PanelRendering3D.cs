using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using TombEditor.Geometry;
using SharpDX.Toolkit.Graphics;
using TombLib.Graphics;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using TombLib.IO;
using TombLib.Utils;

namespace TombEditor.Controls
{
    public partial class PanelRendering3D : Panel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private class BoundingBoxToDraw
        {
            public BoundingBox BoundingBox { get; set; }
            public Vector3 Position { get; set; }

            public BoundingBoxToDraw(BoundingBox box, Vector3 pos, bool half)
            {
                BoundingBox = box;
                Position = Editor.Instance.SelectedRoom.WorldPos + pos;

                if (half)
                {
                    float halfX = (BoundingBox.Maximum.X - BoundingBox.Minimum.X) / 2.0f;
                    float halfY = (BoundingBox.Maximum.Y - BoundingBox.Minimum.Y) / 2.0f;
                    float halfZ = (BoundingBox.Maximum.Z - BoundingBox.Minimum.Z) / 2.0f;

                    Position += new Vector3(halfX, halfY, halfZ);
                }
            }
        }

        private class RoomRenderBucket
        {
            public Room Room { get; set; }
            public int X { get; set; }
            public int Z { get; set; }
            public BlockFace Face { get; set; }
            public bool DoubleSided { get; set; }
            public float Distance { get; set; }
            public BlendMode BlendMode { get; set; }
            public Buffer<short> IndexBuffer { get; set; }
            public List<short> Indices { get; set; } = new List<short>();
        }

        private class Comparer : IComparer<RoomRenderBucket>, IComparer<ItemInstance>
        {
            private readonly Dictionary<Room, int> _rooms = new Dictionary<Room, int>();

            public Comparer(Level level)
            {
                for (int i = 0; i < level.Rooms.Length; ++i)
                    if (level.Rooms[i] != null)
                        _rooms.Add(level.Rooms[i], i);
            }

            public int Compare(RoomRenderBucket x, RoomRenderBucket y)
            {
                {
                    int result = x.DoubleSided.CompareTo(y.DoubleSided);
                    if (result != 0)
                        return result;
                }

                {
                    int result = _rooms[x.Room].CompareTo(_rooms[y.Room]);
                    if (result != 0)
                        return result;
                }

                return x.Distance.CompareTo(y.Distance);
            }

            public int Compare(ItemInstance x, ItemInstance y)
            {
                int result = x.WadObjectId.CompareTo(y.WadObjectId);
                if (result != 0)
                    return result;
                return _rooms[x.Room].CompareTo(_rooms[y.Room]);
            }
        }

        private class PickingResultBlock : PickingResult
        {
            public DrawingPoint Pos { get; set; }
            public BlockFace Face { get; set; }
            public bool IsFloor { get; private set; }
            public PickingResultBlock(float distance, DrawingPoint pos, BlockFace face)
            {
                Distance = distance;
                Pos = pos;
                Face = face;
                IsFloor = (Face == BlockFace.Floor || Face == BlockFace.FloorTriangle2 || Face <= BlockFace.DiagonalMiddle);
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
        private Texture2D _textureAtlas;
        private Vector2 _textureAtlasRemappingSize;
        private readonly List<ImageC> _textureAtlasImages = new List<ImageC>();
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
        private System.Drawing.Point _lastMousePosition;
        private bool _doSectorSelection = false;
        private static readonly Vector4 _selectionColor = new Vector4(3.0f, 0.2f, 0.2f, 1.0f);
        private Buffer<EditorVertex> _skyVertexBuffer;
        private Debug _debug;

        // Gizmo
        private Gizmo _gizmo;

        // Rooms to draw
        private List<Room> _roomsToDraw;

        // Geometry buckets to draw
        private List<RoomRenderBucket> _opaqueBuckets;
        private List<RoomRenderBucket> _solidBuckets;
        private List<RoomRenderBucket> _transparentBuckets;
        private List<RoomRenderBucket> _invisibleBuckets;

        // Items to draw
        private readonly List<MoveableInstance> _moveablesToDraw = new List<MoveableInstance>();
        private readonly List<StaticInstance> _staticsToDraw = new List<StaticInstance>();
        private readonly List<ImportedGeometryInstance> _roomGeometryToDraw = new List<ImportedGeometryInstance>();

        // Debug lines
        private Buffer<EditorVertex> _objectHeightLineVertexBuffer;
        private bool _drawHeightLine = false;

        private Buffer<EditorVertex> _flybyPathVertexBuffer;
        private bool _drawFlybyPath = false;
        private List<BoundingBoxToDraw> _boundingBoxesToDraw;

        private Effect _roomEffect;

        public PanelRendering3D()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            _textureAtlas?.Dispose();
            _presenter?.Dispose();
            _rasterizerWireframe?.Dispose();
            _objectHeightLineVertexBuffer?.Dispose();
            _flybyPathVertexBuffer?.Dispose();
            _gizmo?.Dispose();
            _skyVertexBuffer?.Dispose();
            _sphere?.Dispose();
            _cone?.Dispose();
            _linesCube?.Dispose();
            _littleCube?.Dispose();
            _littleSphere?.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update texture
            if ((obj is Editor.LevelChangedEvent) ||
                (obj is Editor.LoadedTexturesChangedEvent))
            {
                RebuildTextureAtlas();
            }

            // Update FOV
            if (obj is Editor.ConfigurationChangedEvent)
                Camera.FieldOfView = ((Editor.ConfigurationChangedEvent)obj).Current.Rendering3D_FieldOfView * (float)(Math.PI / 180);

            // Update drawing
            if ((obj is IEditorObjectChangedEvent) ||
                (obj is IEditorRoomChangedEvent) ||
                (obj is Editor.ConfigurationChangedEvent) ||
                (obj is Editor.SelectedObjectChangedEvent) ||
                (obj is Editor.SelectedSectorsChangedEvent) ||
                (obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.ModeChangedEvent) ||
                (obj is Editor.LoadedWadsChangedEvent) ||
                (obj is Editor.LoadedTexturesChangedEvent) ||
                (obj is Editor.LoadedImportedGeometriesChangedEvent))
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
                    case EditorActionType.PlaceImportedGeometry:
                    case EditorActionType.PlaceItem:
                    case EditorActionType.PlaceLight:
                    case EditorActionType.PlaceSink:
                    case EditorActionType.PlaceSoundSource:
                    case EditorActionType.Stamp:
                        hasCrossCurser = true;
                        break;
                }
                Cursor = hasCrossCurser ? Cursors.Cross : Cursors.Arrow;
            }

            // Center camera
            if (obj is Editor.ResetCameraEvent)
                ResetCamera();

            // Move camera to sector
            if (obj is Editor.MoveCameraToSectorEvent)
            {
                var e = (Editor.MoveCameraToSectorEvent)obj;

                Vector3 center = _editor.SelectedRoom.GetLocalCenter();
                Camera.Target = new Vector3(e.Sector.X * 1024.0f + 512.0f, center.Y, e.Sector.Y * 1024.0f + 512.0f) + _editor.SelectedRoom.WorldPos;
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

        public void ResetCamera()
        {
            Room room = _editor?.SelectedRoom;

            // Point the camera to the room's center
            Vector3 target = new Vector3();
            if (room != null)
                target = room.WorldPos + room.GetLocalCenter();

            // Initialize a new camera
            Camera = new ArcBallCamera(target, DefaultCameraAngleX, DefaultCameraAngleY, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, DefaultCameraDistance, 2750, 1000000, _editor.Configuration.Rendering3D_FieldOfView * (float)(Math.PI / 180));
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
                BackBufferWidth = ClientSize.Width,
                BackBufferHeight = ClientSize.Height,
                DepthStencilFormat = DepthFormat.Depth24Stencil8,
                DeviceWindowHandle = this,
                IsFullScreen = false,
                MultiSampleCount = MSAALevel.None,
                PresentationInterval = PresentInterval.Immediate,
                RenderTargetUsage = SharpDX.DXGI.Usage.RenderTargetOutput | SharpDX.DXGI.Usage.BackBuffer,
                Flags = SharpDX.DXGI.SwapChainFlags.None
            };
            _presenter = new SwapChainGraphicsPresenter(_device, pp);

            // Maybe I could use this as bounding box, scaling it properly before drawing
            _linesCube = GeometricPrimitive.LinesCube.New(_device, 128, 128, 128);

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
            _gizmo = new Gizmo(deviceManager.Device, deviceManager.Effects["Solid"]);

            ResetCamera();

            PrepareSky();

            logger.Info("Graphic Device ready");
        }

        private void PrepareSky()
        {
            var vertices = new List<EditorVertex>();

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var v1 = new EditorVertex
                    {
                        Position = new Vector3(x, 0.0f, z) * 1024.0f,
                        UV = new Vector2(0.0f, 1.0f)
                    };

                    var v2 = new EditorVertex
                    {
                        Position = new Vector3(x + 1, 0.0f, z) * 1024.0f,
                        UV = new Vector2(1.0f, 1.0f)
                    };

                    var v3 = new EditorVertex
                    {
                        Position = new Vector3(x + 1, 0.0f, z + 1) * 1024.0f,
                        UV = new Vector2(0.0f, 1.0f)
                    };

                    var v4 = new EditorVertex
                    {
                        Position = new Vector3(x, 0.0f, z + 1) * 1024.0f,
                        UV = new Vector2(0.0f, 0.0f)
                    };

                    vertices.Add(v2);
                    vertices.Add(v4);
                    vertices.Add(v1);
                    vertices.Add(v4);
                    vertices.Add(v2);
                    vertices.Add(v3);
                }
            }

            _skyVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<EditorVertex>(_device, vertices.ToArray<EditorVertex>(), SharpDX.Direct3D11.ResourceUsage.Default);

            //_skyTexture = Texture2D.Load(_device, "Editor\\sky.png");
        }

        private void RebuildTextureAtlas()
        {
            if (_device == null)
                return;
            var textures = _editor.Level.Settings.Textures;

            // Compare images (if there is no difference we exit)
            if (_textureAtlasImages.Count == textures.Count)
            {
                for (int i = 0; i < textures.Count; ++i)
                    if (_textureAtlasImages[i] != textures[i].Image)
                        goto NotIdentical;
                return; // All images are identical, we can exit
            }
            NotIdentical:

            // Update texture list
            _textureAtlasImages.Clear();
            for (int i = 0; i < textures.Count; ++i)
                _textureAtlasImages.Add(textures[i].Image);

            // Delete old texture list
            _textureAtlas?.Dispose();
            _textureAtlas = null;

            // Build texture atlas
            if (textures.Count > 0)
            {
                // TODO Support more than 1 texture
                ImageC texture = textures[0].Image;

                const int maxTextureSize = 8096;
                if (texture.Height > maxTextureSize)
                {
                    // HACK Split really high texture into multiple columns
                    const int texturePageHeight = maxTextureSize - 256; // Subtract maximum tile size
                    int pageCount = (texture.Height + texturePageHeight - 1) / texturePageHeight;
                    var remappedTexture = ImageC.CreateNew(texture.Width * pageCount, maxTextureSize);

                    for (int i = 0; i < pageCount; ++i)
                    {
                        int fromY = texturePageHeight * i;
                        int fromHeight = Math.Min(texture.Height - texturePageHeight * i, 8096);
                        remappedTexture.CopyFrom(texture.Width * i, 0, texture, 0, fromY, texture.Width, fromHeight);
                    }

                    _textureAtlas = TextureLoad.Load(_device, remappedTexture);
                    _textureAtlasRemappingSize = new Vector2(texture.Width, texturePageHeight);
                }
                else
                {
                    _textureAtlas = TextureLoad.Load(_device, texture);
                    _textureAtlasRemappingSize = new Vector2(float.MaxValue);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Don't paint the background
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_presenter != null && ClientSize.Width != 0 && ClientSize.Height != 0)
            {
                _presenter.Resize(ClientSize.Width, ClientSize.Height, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
                Invalidate();
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            // I intercept arrow keys here otherwise they would processed by the form and
            // camera would move only if Panel3D is focused
            switch (e.KeyCode)
            {
                case Keys.Up:
                    Camera.Rotate(0, -_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate);
                    Invalidate();
                    break;

                case Keys.Down:
                    Camera.Rotate(0, _editor.Configuration.Rendering3D_NavigationSpeedKeyRotate);
                    Invalidate();
                    break;

                case Keys.Left:
                    Camera.Rotate(_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate, 0);
                    Invalidate();
                    break;

                case Keys.Right:
                    Camera.Rotate(-_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate, 0);
                    Invalidate();
                    break;

                case Keys.PageUp:
                    Camera.Zoom(-_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom);
                    Invalidate();
                    break;

                case Keys.PageDown:
                    Camera.Zoom(_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom);
                    Invalidate();
                    break;
            }
        }
        
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * _editor.Configuration.Rendering3D_NavigationSpeedMouseWheelZoom);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _lastMousePosition = e.Location;
            _doSectorSelection = false;

            //https://stackoverflow.com/questions/14191219/receive-mouse-move-even-cursor-is-outside-control
            Capture = true; // Capture mouse for zoom and panning

            if (e.Button == MouseButtons.Left)
            {
                // Do picking on the scene
                PickingResult newPicking = DoPicking(GetRay(e.X, e.Y));

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
                if (newPicking is PickingResultGizmo)
                {
                    _gizmo.ActivateGizmo((PickingResultGizmo)newPicking);
                    return;
                }

                // Process editor actions
                switch (_editor.Action.Action)
                {
                    case EditorActionType.PlaceLight:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, new LightInstance(_editor.Action.LightType));
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceItem:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, ItemInstance.FromItemType(_editor.Action.ItemType));
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceSink:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, new SinkInstance());
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceCamera:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, new CameraInstance());
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceImportedGeometry:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, new ImportedGeometryInstance());
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceSoundSource:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, new SoundSourceInstance());
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.PlaceFlyByCamera:
                        if (newPicking is PickingResultBlock)
                        {
                            EditorActions.PlaceObject(_editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos, new FlybyCameraInstance());
                            _editor.Action = EditorAction.None;
                        }
                        break;
                    case EditorActionType.Paste:
                        if (newPicking is PickingResultBlock)
                        {
                            ObjectInstance instance = Clipboard.Paste(_editor.Level, _editor.SelectedRoom, ((PickingResultBlock)newPicking).Pos);
                            _editor.ObjectChange(instance);
                            _editor.SelectedObject = instance;
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
                                    bool isFloor = ((PickingResultBlock)newPicking).IsFloor;

                                    // Split the faces
                                    if (ModifierKeys.HasFlag(Keys.Alt))
                                    {
                                        if (isFloor)
                                            EditorActions.FlipFloorSplit(_editor.SelectedRoom, new SharpDX.Rectangle(pos.X, pos.Y, pos.X, pos.Y));
                                        else
                                            EditorActions.FlipCeilingSplit(_editor.SelectedRoom, new SharpDX.Rectangle(pos.X, pos.Y, pos.X, pos.Y));
                                        return;
                                    }
                                    else if (ModifierKeys.HasFlag(Keys.Shift))
                                    {
                                        EditorActions.RotateSectors(_editor.SelectedRoom, new SharpDX.Rectangle(pos.X, pos.Y, pos.X, pos.Y), isFloor);
                                        return;
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
                                    if (ModifierKeys == Keys.Control)
                                        EditorActions.MirrorTexture(_editor.SelectedRoom, newBlockPicking.Pos, newBlockPicking.Face);
                                    else if (ModifierKeys == Keys.Shift)
                                        EditorActions.RotateTexture(_editor.SelectedRoom, newBlockPicking.Pos, newBlockPicking.Face);
                                    else if (ModifierKeys == Keys.Alt)
                                        EditorActions.PickTexture(_editor.SelectedRoom, newBlockPicking.Pos, newBlockPicking.Face);
                                    else
                                        EditorActions.ApplyTextureAutomatically(_editor.SelectedRoom, newBlockPicking.Pos, newBlockPicking.Face, _editor.SelectedTexture);
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

            switch(e.Button)
            {
                case MouseButtons.Left:
                    PickingResult newPicking = DoPicking(GetRay(e.X, e.Y));
                    if (newPicking is PickingResultObject)
                        EditorActions.EditObject(((PickingResultObject)newPicking).ObjectInstance, this.Parent);
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

            // Hover effect on gizmo
            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(GetRay(e.X, e.Y))))
                Invalidate();

            // Process action
            switch (e.Button)
            {
                case MouseButtons.Middle:
                case MouseButtons.Right:
                    // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                    float relativeDeltaX = (e.X - _lastMousePosition.X) / (float)Height;
                    float relativeDeltaY = (e.Y - _lastMousePosition.Y) / (float)Height;
                    if (((ModifierKeys & Keys.Shift) == Keys.Shift) || (e.Button == MouseButtons.Middle))
                        Camera.MoveCameraPlane(new Vector3(relativeDeltaX, relativeDeltaY, 0) *
                            _editor.Configuration.Rendering3D_NavigationSpeedMouseTranslate);
                    else if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Camera.Zoom(-relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseZoom);
                    else
                        Camera.Rotate(
                            relativeDeltaX * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                            -relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);

                    _gizmo.MouseMoved(Camera.GetViewProjectionMatrix(Width, Height), e.X, e.Y); // Update gizmo
                    Invalidate();
                    break;

                case MouseButtons.Left:
                    if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(Width, Height), e.X, e.Y))
                    { // Process gizmo
                    }
                    else if (_doSectorSelection)
                    { // Calculate block selection
                        PickingResult newPicking = DoPicking(GetRay(e.X, e.Y));
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
                    }
                    else if ((_editor.Mode == EditorMode.FaceEdit) && (_editor.Action.Action == EditorActionType.None))
                    { // Texture editing
                        PickingResultBlock newPicking = DoPicking(GetRay(e.X, e.Y)) as PickingResultBlock;

                        if (newPicking != null)
                            EditorActions.ApplyTextureAutomatically(_editor.SelectedRoom, newPicking.Pos, newPicking.Face, _editor.SelectedTexture);
                    }
                    break;
            }

            _lastMousePosition = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _doSectorSelection = false;
            if (_gizmo.MouseUp())
                Invalidate();
            Capture = false;
            Invalidate();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ItemType)))
                e.Effect = DragDropEffects.Copy;
            else if (EditorActions.DragDropFileSupported(e, true))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            // Check if we are done with all common file tasks

            if (EditorActions.DragDropFile(e, FindForm()))
                return;

            // Now try to put data on pointed sector

            Point loc = PointToClient(new Point(e.X, e.Y));
            PickingResult newPicking = DoPicking(GetRay(loc.X, loc.Y));

            if (newPicking is PickingResultBlock)
            {
                if (e.Data.GetDataPresent(typeof(ItemType)))
                {
                    // Put item from object browser

                    EditorActions.PlaceObject(_editor.SelectedRoom,
                        ((PickingResultBlock)newPicking).Pos,
                        ItemInstance.FromItemType((ItemType)e.Data.GetData(typeof(ItemType))));
                }
                else if(e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Try to put custom geometry files

                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (!SupportedFormats.IsExtensionPresent(FileFormatType.Geometry, files[i]))
                            continue;

                        var info = ImportedGeometryInfo.Default;
                        info.Path = files[i];
                        info.Name = Path.GetFileNameWithoutExtension(files[i]);

                        var instance = new ImportedGeometryInstance();
                        var existingGeometry = _editor.Level.Settings.ImportedGeometries.Find(item => item.Info.Path == info.Path);

                        if(existingGeometry == null)
                        {
                            existingGeometry = new ImportedGeometry();
                            _editor.Level.Settings.ImportedGeometryUpdate(existingGeometry, info);
                            _editor.Level.Settings.ImportedGeometries.Add(existingGeometry);
                            _editor.LoadedImportedGeometriesChange();
                        }

                        instance.Model = existingGeometry;

                        EditorActions.PlaceObject(_editor.SelectedRoom,
                            ((PickingResultBlock)newPicking).Pos, instance);

                        _editor.ObjectChange(instance);
                    }
                }
            }
        }

        private static float TransformRayDistance(ref Ray sourceRay, ref Matrix transform, ref Ray destinationRay, float sourceDistance)
        {
            Vector3 sourcePos = sourceRay.Position + sourceDistance * sourceRay.Direction;
            Vector3 destinationPos = Vector3.TransformCoordinate(sourcePos, transform);
            float destinationDistance = (destinationPos - destinationRay.Position).Length();
            return destinationDistance;
        }

        private void DoMeshPicking<T>(ref PickingResult result, Ray ray, ObjectInstance objectPtr, Mesh<T> mesh, Matrix objectMatrix) where T : struct, IVertex
        {
            // Transform view ray to object space space
            Matrix inverseObjectMatrix = objectMatrix;
            inverseObjectMatrix.Invert();
            Vector3 transformedRayPos = Vector3.TransformCoordinate(ray.Position, inverseObjectMatrix);
            Vector3 transformedRayDestination = Vector3.TransformCoordinate(ray.Position + ray.Direction, inverseObjectMatrix);
            Ray transformedRay = new Ray(transformedRayPos, transformedRayDestination - transformedRayPos);
            transformedRay.Direction.Normalize();

            // Do a fast bounding box check
            float minDistance;
            {
                BoundingBox box = mesh.BoundingBox;
                float distance;
                if (!transformedRay.Intersects(ref box, out distance))
                    return;

                minDistance = result == null ? float.PositiveInfinity : TransformRayDistance(ref ray, ref inverseObjectMatrix, ref transformedRay, result.Distance);
                if (!(distance < minDistance))
                    return;
            }

            // Now do a ray - triangle intersection test
            bool hit = false;
            for (int k = 0; k < mesh.Indices.Count; k += 3)
            {
                Vector3 p1 = mesh.Vertices[mesh.Indices[k]].Position;
                Vector3 p2 = mesh.Vertices[mesh.Indices[k + 1]].Position;
                Vector3 p3 = mesh.Vertices[mesh.Indices[k + 2]].Position;

                float distance;
                if (transformedRay.Intersects(ref p1, ref p2, ref p3, out distance) && (distance < minDistance))
                {
                    minDistance = distance;
                    hit = true;
                }
            }

            if (hit)
                result = new PickingResultObject(TransformRayDistance(ref transformedRay, ref objectMatrix, ref ray, minDistance), objectPtr);
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
                if (instance is LightInstance)
                {
                    BoundingSphere sphere = new BoundingSphere(room.WorldPos + instance.Position, _littleSphereRadius);
                    if (ray.Intersects(ref sphere, out distance) && ((result == null) || (distance < result.Distance)))
                        result = new PickingResultObject(distance, instance);
                }
                else if (instance is MoveableInstance)
                {
                    MoveableInstance modelInfo = (MoveableInstance)instance;
                    if (_editor?.Level?.Wad?.DirectXMoveables?.ContainsKey(modelInfo.WadObjectId) ?? false)
                    {
                        SkinnedModel model = _editor.Level.Wad.DirectXMoveables[modelInfo.WadObjectId];

                        for (int j = 0; j < model.Meshes.Count; j++)
                        {
                            SkinnedMesh mesh = model.Meshes[j];
                            DoMeshPicking(ref result, ray, instance, mesh, model.AnimationTransforms[j] * instance.ObjectMatrix);
                        }
                    }
                    else
                    {
                        BoundingBox box = new BoundingBox(
                            room.WorldPos + modelInfo.Position - new Vector3(_littleCubeRadius),
                            room.WorldPos + modelInfo.Position + new Vector3(_littleCubeRadius));
                        if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                }
                else if (instance is StaticInstance)
                {
                    StaticInstance modelInfo = (StaticInstance)instance;
                    if (_editor?.Level?.Wad?.DirectXStatics?.ContainsKey(modelInfo.WadObjectId) ?? false)
                    {
                        StaticModel model = _editor.Level.Wad.DirectXStatics[modelInfo.WadObjectId];

                        StaticMesh mesh = model.Meshes[0];
                        DoMeshPicking(ref result, ray, instance, mesh, instance.ObjectMatrix);
                    }
                    else
                    {
                        BoundingBox box = new BoundingBox(
                            room.WorldPos + modelInfo.Position - new Vector3(_littleCubeRadius),
                            room.WorldPos + modelInfo.Position + new Vector3(_littleCubeRadius));
                        if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                }
                else if (instance is ImportedGeometryInstance)
                {
                    var geometry = (ImportedGeometryInstance)instance;

                    BoundingBox box = geometry.Model?.DirectXModel?.BoundingBox ?? new BoundingBox(new Vector3(-128), new Vector3(128));
                    box.Minimum += room.WorldPos + instance.Position;
                    box.Maximum += room.WorldPos + instance.Position;
                    if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                        result = new PickingResultObject(distance, instance);
                }
                else
                {
                    BoundingBox box = new BoundingBox(
                        room.WorldPos + instance.Position - new Vector3(_littleCubeRadius),
                        room.WorldPos + instance.Position + new Vector3(_littleCubeRadius));
                    if (ray.Intersects(ref box, out distance) && ((result == null) || (distance < result.Distance)))
                        result = new PickingResultObject(distance, instance);
                }

            // Check room geometry
            var roomIntersectInfo = room.RayIntersectsGeometry(new Ray(ray.Position - room.WorldPos, ray.Direction));
            if ((roomIntersectInfo != null) && ((result == null) || (roomIntersectInfo.Value.Distance < result.Distance)))
                result = new PickingResultBlock(roomIntersectInfo.Value.Distance, roomIntersectInfo.Value.Pos, roomIntersectInfo.Value.Face);

            return result;
        }

        private Ray GetRay(float x, float y)
        {
            // Get the current ViewProjection matrix
            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);

            // First get the ray in 3D space from X, Y mouse coordinates
            Ray ray = Ray.GetPickRay((int)Math.Round(x), (int)Math.Round(y), new ViewportF(0, 0, Width, Height), viewProjection);
            return ray;
        }

        private void AddRoomBoundingBox()
        {
            float height = (_editor.SelectedRoom.GetHighestCorner() - _editor.SelectedRoom.GetLowestCorner());

            var boundingBox = new BoundingBox(Vector3.Zero,
                                              new Vector3(_editor.SelectedRoom.NumXSectors * 1024.0f,
                                                          height * 256.0f,
                                                          _editor.SelectedRoom.NumZSectors * 1024.0f
                                                          ));

            _boundingBoxesToDraw.Add(new BoundingBoxToDraw(boundingBox, Vector3.Zero, true));
        }

        private void DrawDebugLines(Matrix viewProjection)
        {
            if (!_drawFlybyPath && !_drawHeightLine && _boundingBoxesToDraw.Count == 0) return;

            _device.SetRasterizerState(_rasterizerWireframe);

            Effect solidEffect = _deviceManager.Effects["Solid"];

            Matrix model = Matrix.Translation(_editor.SelectedRoom.WorldPos);
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

            if (_drawHeightLine)
            {
                _device.SetVertexBuffer(_objectHeightLineVertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _objectHeightLineVertexBuffer));

                Matrix model2 = Matrix.Translation(_editor.SelectedRoom.WorldPos);

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

            if (_boundingBoxesToDraw.Count != 0)
            {
                _device.SetVertexBuffer(_linesCube.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _linesCube.VertexBuffer));
                _device.SetIndexBuffer(_linesCube.IndexBuffer, false);
            }

            foreach (var boundingBox in _boundingBoxesToDraw)
            {
                float scaleX = (boundingBox.BoundingBox.Maximum.X - boundingBox.BoundingBox.Minimum.X) / 256.0f;
                float scaleY = (boundingBox.BoundingBox.Maximum.Y - boundingBox.BoundingBox.Minimum.Y) / 256.0f;
                float scaleZ = (boundingBox.BoundingBox.Maximum.Z - boundingBox.BoundingBox.Minimum.Z) / 256.0f;

                /*float halfX = (boundingBox.BoundingBox.Maximum.X - boundingBox.BoundingBox.Minimum.X) / 2.0f;
                float halfY = (boundingBox.BoundingBox.Maximum.Y - boundingBox.BoundingBox.Minimum.Y) / 2.0f;
                float halfZ = (boundingBox.BoundingBox.Maximum.Z - boundingBox.BoundingBox.Minimum.Z) / 2.0f;*/

                Matrix scaleMatrix = Matrix.Scaling(scaleX, scaleY, scaleZ);

                Matrix translateMatrix = Matrix.Translation(boundingBox.Position /*+ new Vector3(halfX, halfY, halfZ)*/);

                solidEffect.Parameters["ModelViewProjection"].SetValue(scaleMatrix *
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
                        message += "\nTriggered in Room " + trigger.Room + " on sectors [" + trigger.Area.X + ", " + trigger.Area.Y + " to " + trigger.Area.Right + ", " + trigger.Area.Bottom + "]";
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


            foreach (var light in room.Objects.OfType<LightInstance>())
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

                solidEffect.Parameters["ModelViewProjection"].SetValue(light.ObjectMatrix * viewProjection);

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
                    solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 0.2f, 0.2f, 1.0f));

                solidEffect.CurrentTechnique.Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
            }

            if (_editor.SelectedObject?.Room == room && _editor.SelectedObject is LightInstance)
            {
                LightInstance light = (LightInstance)_editor.SelectedObject;

                if (light.Type == LightType.Light || light.Type == LightType.Shadow || light.Type == LightType.FogBulb)
                {
                    _device.SetVertexBuffer(_sphere.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
                    _device.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);

                    if (light.Type == LightType.Light || light.Type == LightType.Shadow)
                    {
                        Matrix model = Matrix.Scaling(light.InnerRange * 2.0f) * light.ObjectMatrix;
                        solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                        solidEffect.CurrentTechnique.Passes[0].Apply();
                        _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
                    }

                    if (light.Type == LightType.Light || light.Type == LightType.Shadow ||
                        light.Type == LightType.FogBulb)
                    {
                        Matrix model = Matrix.Scaling(light.OuterRange * 2.0f) * light.ObjectMatrix;
                        solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                        solidEffect.CurrentTechnique.Passes[0].Apply();
                        _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
                    }
                }
                else if (light.Type == LightType.Spot)
                {
                    _device.SetVertexBuffer(_cone.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _device.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    // Inner cone
                    float coneAngle = (float)Math.Atan2(512, 1024);
                    float lenScaleH = light.InnerRange;
                    float lenScaleW = MathUtil.DegreesToRadians(light.InnerAngle) / coneAngle * lenScaleH;

                    Matrix Model = Matrix.Scaling(lenScaleW, lenScaleW, lenScaleH) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue(Model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);

                    // Outer cone
                    float cutoffScaleH = light.OuterRange;
                    float cutoffScaleW = MathUtil.DegreesToRadians(light.OuterAngle) / coneAngle * cutoffScaleH;

                    Matrix model2 = Matrix.Scaling(cutoffScaleW, cutoffScaleW, cutoffScaleH) * light.ObjectMatrix;
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

                    Matrix model = Matrix.Scaling(0.01f, 0.01f, 1.0f) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                string message = light.Type + " Light";

                // Object position
                message += "\n" + GetObjectPositionString(light.Room, light);

                Vector3 screenPos = Vector3.Project(new Vector3(), 0, 0, Width, Height,
                    _device.Viewport.MinDepth,
                    _device.Viewport.MaxDepth, light.ObjectMatrix * viewProjection);
                _debug.AddString(message, screenPos);

                // Add the line height of the object
                AddObjectHeightLine(viewProjection, light.Room, light.Position);
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="viewProjection"></param>
        /// <param name="room"></param>
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

                    string message = "Camera " + (instance.Fixed ? "(Fixed)" : "");

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);

                    Vector3 screenPos = Vector3.Project(new Vector3(), 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, instance.ObjectMatrix);

                    BuildTriggeredByMessage(ref message, instance);

                    _debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                effect.Parameters["ModelViewProjection"].SetValue(instance.ObjectMatrix * viewProjection);
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

                    FlybyCameraInstance flyby = (FlybyCameraInstance)instance;

                    string message = "Flyby camera (" + instance.Sequence + ":" + instance.Number + ")";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);

                    Vector3 screenPos = Vector3.Project(new Vector3(), 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, instance.ObjectMatrix);

                    BuildTriggeredByMessage(ref message, instance);

                    _debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);

                    // Add the path of the flyby
                    AddFlybyPath(flyby.Sequence);
                }

                effect.Parameters["ModelViewProjection"].SetValue(instance.ObjectMatrix * viewProjection);
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

                    var message = "Sink";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);

                    Vector3 screenPos = Vector3.Project(new Vector3(), 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, instance.ObjectMatrix * viewProjection);

                    BuildTriggeredByMessage(ref message, instance);

                    _debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                effect.Parameters["ModelViewProjection"].SetValue(instance.ObjectMatrix * viewProjection);
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

                    string message = "Sound source";
                    if ((instance.SoundId >= 0) && _editor.Level.Wad != null &&
                        _editor.Level.Wad.SoundInfo.ContainsKey(instance.SoundId))
                        message += " (" + _editor.Level.Wad.SoundInfo[instance.SoundId].Name + ") ";
                    else
                        message += " ( Invalid or missing sound ) ";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);

                    Vector3 screenPos = Vector3.Project(new Vector3(), 0, 0, Width, Height,
                        _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, instance.ObjectMatrix * viewProjection);

                    BuildTriggeredByMessage(ref message, instance);

                    _debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                effect.Parameters["ModelViewProjection"].SetValue(instance.ObjectMatrix * viewProjection);
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

                    Vector4 color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(1.0f, 0.4f, 0.4f, 1.0f);
                        _device.SetRasterizerState(_rasterizerWireframe);

                        string message = instance.ToString();
                        message += "\nUnavailable " + instance.ItemType.ToString();

                        // Object position
                        message += "\n" + GetObjectPositionString(room, instance);

                        Vector3 screenPos = Vector3.Project(new Vector3(), 0, 0, Width, Height,
                            _device.Viewport.MinDepth,
                            _device.Viewport.MaxDepth, instance.RotationPositionMatrix * viewProjection);

                        BuildTriggeredByMessage(ref message, instance);

                        _debug.AddString(message, screenPos);

                        // Add the line height of the object
                        AddObjectHeightLine(viewProjection, room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue(instance.RotationPositionMatrix * viewProjection);
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }

                foreach (var instance in room.Objects.OfType<StaticInstance>())
                {
                    if (_editor?.Level?.Wad?.DirectXStatics?.ContainsKey(instance.WadObjectId) ?? false)
                        continue;

                    _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                    Vector4 color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(1.0f, 0.4f, 0.4f, 1.0f);
                        _device.SetRasterizerState(_rasterizerWireframe);

                        string message = instance.ToString();
                        message += "\nUnavailable " + instance.ItemType.ToString();

                        Vector3 screenPos = Vector3.Project(new Vector3(), 0, 0, Width, Height,
                            _device.Viewport.MinDepth,
                            _device.Viewport.MaxDepth, instance.RotationPositionMatrix * viewProjection);
                        _debug.AddString(message, screenPos);

                        // Add the line height of the object
                        AddObjectHeightLine(viewProjection, room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue(instance.RotationPositionMatrix * viewProjection);
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }

                foreach (var instance in room.Objects.OfType<ImportedGeometryInstance>())
                {
                    if (instance.Model?.DirectXModel != null)
                        continue;

                    _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                    Vector4 color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                    if (_editor.SelectedObject == instance)
                    {
                        color = new Vector4(1.0f, 0.4f, 0.4f, 1.0f);
                        _device.SetRasterizerState(_rasterizerWireframe);

                        Vector3 screenPos = Vector3.Project(new Vector3(), 0, 0, Width, Height,
                            _device.Viewport.MinDepth,
                            _device.Viewport.MaxDepth, instance.RotationPositionMatrix * viewProjection);
                        _debug.AddString(instance.ToString(), screenPos);

                        // Add the line height of the object
                        AddObjectHeightLine(viewProjection, room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue(instance.RotationPositionMatrix * viewProjection);
                    effect.Parameters["Color"].SetValue(color);

                    effect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);
                }
            }

            _device.SetVertexBuffer(_cone.VertexBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
            _device.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);
            _device.SetRasterizerState(_rasterizerWireframe);

            foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
            {
                // Outer cone
                float coneAngle = (float)Math.Atan2(512, 1024);
                float cutoffScaleH = 1;
                float cutoffScaleW = MathUtil.DegreesToRadians(instance.Fov / 2) / coneAngle * cutoffScaleH;

                Matrix model = Matrix.Scaling(cutoffScaleW, cutoffScaleW, cutoffScaleH) * instance.ObjectMatrix;

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
                _debug.NumMoveables++;

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

                skinnedModelEffect.Parameters["Color"].SetValue(_editor.Mode == EditorMode.Lighting ? instance.Color : new Vector4(1.0f));
                if (_editor.SelectedObject == instance) // Selection
                    skinnedModelEffect.Parameters["Color"].SetValue(_selectionColor);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    SkinnedMesh mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    Matrix world = model.AnimationTransforms[i] * instance.ObjectMatrix;

                    skinnedModelEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);

                    skinnedModelEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                    _debug.NumVerticesObjects += mesh.NumIndices;
                    _debug.NumTrianglesObjects += mesh.NumIndices / 3;
                }

                if (_editor.SelectedObject == instance)
                {
                    Vector3 screenPos = Vector3.Project(512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, instance.ObjectMatrix * viewProjection);

                    string message = _editor.Level.Wad.Moveables[instance.WadObjectId].ToString();

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);
                    message += "\n" + "Rotation Y: " + Math.Round(instance.RotationY, 2);

                    // Add OCB
                    if (instance.Ocb != 0)
                        message += "\nOCB: " + instance.Ocb;

                    BuildTriggeredByMessage(ref message, instance);

                    _debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private void DrawRoomImportedGeometry(Matrix viewProjection)
        {
            _device.SetBlendState(_device.BlendStates.Opaque);

            Effect geometryEffect = _deviceManager.Effects["RoomGeometry"];

            ImportedGeometryInstance _lastObject = null;

            for (int k = 0; k < _roomGeometryToDraw.Count; k++)
            {
                ImportedGeometryInstance instance = _roomGeometryToDraw[k];
                if (instance.Model?.DirectXModel == null)
                    continue;

                ImportedGeometry.Model model = instance.Model.DirectXModel;

                //_debug.NumMoveables++;

                Room room = instance.Room;

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    ImportedGeometryMesh mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    if (k == 0 && i == 0)
                    {
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));
                    }

                    _device.SetVertexBuffer(0, mesh.VertexBuffer);
                    _device.SetIndexBuffer(mesh.IndexBuffer, true);

                    geometryEffect.Parameters["ModelViewProjection"].SetValue(instance.ObjectMatrix * viewProjection);

                    geometryEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                    if (_editor.SelectedObject == instance)
                        geometryEffect.Parameters["Color"].SetValue(_selectionColor);

                    if (mesh.Texture != null)
                    {
                        geometryEffect.Parameters["TextureEnabled"].SetValue(true);
                        if (mesh.Texture is ImportedGeometryTexture)
                        {
                            geometryEffect.Parameters["Texture"].SetResource(((ImportedGeometryTexture)mesh.Texture).DirectXTexture);
                            geometryEffect.Parameters["ReciprocalTextureSize"].SetValue(new Vector2(1.0f / mesh.Texture.Image.Width, 1.0f / mesh.Texture.Image.Height));
                        }
                        else
                        {
                            geometryEffect.Parameters["Texture"].SetResource(_textureAtlas);
                            geometryEffect.Parameters["ReciprocalTextureSize"].SetValue(new Vector2(1.0f / _textureAtlas.Width, 1.0f / _textureAtlas.Height));
                        }
                        geometryEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);
                    }
                    else
                    {
                        geometryEffect.Parameters["TextureEnabled"].SetValue(false);
                    }

                    geometryEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.Indices.Count, mesh.BaseIndex);

                    _debug.NumVerticesRooms += mesh.NumIndices;
                    _debug.NumTrianglesRooms += mesh.NumIndices / 3;
                }

                if (_editor.SelectedObject == instance)
                {
                    Vector3 screenPos = Vector3.Project(512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, instance.ObjectMatrix * viewProjection);

                    string message = instance.ToString();

                    // Object position
                    message += "\n" + GetObjectPositionString(_editor.SelectedRoom, instance);

                    _debug.AddString(message, screenPos);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, _editor.SelectedRoom, instance.Position);
                }

                _lastObject = instance;
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

                _debug.NumStaticMeshes++;

                staticMeshEffect.Parameters["Color"].SetValue(_editor.Mode == EditorMode.Lighting ? instance.Color : new Vector4(1.0f));
                if (_editor.SelectedObject == instance)
                    staticMeshEffect.Parameters["Color"].SetValue(_selectionColor);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    StaticMesh mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    staticMeshEffect.Parameters["ModelViewProjection"].SetValue(instance.ObjectMatrix * viewProjection);

                    staticMeshEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                    _debug.NumVerticesObjects += mesh.NumIndices;
                    _debug.NumTrianglesObjects += mesh.NumIndices / 3;
                }

                if (_editor.SelectedObject == instance)
                {
                    Vector3 screenPos = Vector3.Project(512.0f * Vector3.UnitY, 0, 0, Width,
                        Height, _device.Viewport.MinDepth,
                        _device.Viewport.MaxDepth, instance.ObjectMatrix * viewProjection);

                    string message = _editor.Level.Wad.Statics[instance.WadObjectId].ToString();

                    // Object position
                    message += "\n" + GetObjectPositionString(_editor.SelectedRoom, instance);
                    message += "\n" + "Rotation Y: " + Math.Round(instance.RotationY, 2);

                    BuildTriggeredByMessage(ref message, instance);

                    _debug.AddString(message, screenPos);

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
            if (!_editor.Level.Wad.Moveables.ContainsKey(459))
                return;

            _device.SetBlendState(_device.BlendStates.AlphaBlend);

            Effect skinnedModelEffect = _deviceManager.Effects["Model"];

            SkinnedModel skinnedModel = _editor.Level.Wad.DirectXMoveables[459];

            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<SkinnedVertex>(0, skinnedModel.VertexBuffer));

            /*_device.SetVertexBuffer(0, _skyVertexBuffer);

            Matrix skyMatrix = Matrix.Scaling(50.0f) * Matrix.Translation(new Vector3(-409600.0f, 20480.0f, -409600.0f)) * _editor.SelectedRoom.Transform;
            skinnedModelEffect.Parameters["ModelViewProjection"].SetValue(skyMatrix * viewProjection);

            skinnedModelEffect.Parameters["Texture"].SetResource(_skyTexture);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);

            skinnedModelEffect.Techniques[0].Passes[0].Apply();
            _device.Draw(PrimitiveType.TriangleList, _skyVertexBuffer.ElementCount);
            */

            _device.SetVertexBuffer(0, skinnedModel.VertexBuffer);
            _device.SetIndexBuffer(skinnedModel.IndexBuffer, true);

            skinnedModelEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

            for (int i = 0; i < skinnedModel.Meshes.Count; i++)
            {
                SkinnedMesh mesh = skinnedModel.Meshes[i];

                Matrix modelMatrix = Matrix.Scaling(20.0f) * skinnedModel.AnimationTransforms[i] * _editor.SelectedRoom.Transform;
                skinnedModelEffect.Parameters["ModelViewProjection"].SetValue(modelMatrix * viewProjection);

                skinnedModelEffect.Techniques[0].Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);

                _debug.NumVerticesObjects += mesh.NumIndices;
                _debug.NumTrianglesObjects += mesh.NumIndices / 3;
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
            _roomGeometryToDraw.Clear();

            for (int i = 0; i < _roomsToDraw.Count; i++)
            {
                _moveablesToDraw.AddRange(_roomsToDraw[i].Objects.OfType<MoveableInstance>());
                _staticsToDraw.AddRange(_roomsToDraw[i].Objects.OfType<StaticInstance>());
                _roomGeometryToDraw.AddRange(_roomsToDraw[i].Objects.OfType<ImportedGeometryInstance>());
            }

            var comparer = new Comparer(_editor.Level);
            _moveablesToDraw.Sort(comparer);
            _staticsToDraw.Sort(comparer);
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
            _opaqueBuckets = new List<RoomRenderBucket>();
            _transparentBuckets = new List<RoomRenderBucket>();
            _solidBuckets = new List<RoomRenderBucket>();
            _invisibleBuckets = new List<RoomRenderBucket>();

            Vector3 cameraPosition = Camera.GetPosition();

            // Build buckets
            for (int i = 0; i < _roomsToDraw.Count; i++)
            {
                var room = _roomsToDraw[i];

                for (int x = 0; x < room.NumXSectors; x++)
                    for (int z = 0; z < room.NumZSectors; z++)
                        for (BlockFace face = 0; face < Block.FaceCount; face++)
                        {
                            if (!room.IsFaceDefined(x, z, face))
                                continue;

                            TextureArea faceTexture = room.Blocks[x, z].GetFaceTexture(face);

                            if ((_editor.Mode == EditorMode.Geometry && room == _editor.SelectedRoom) || faceTexture.TextureIsUnavailable)
                            {
                                RoomRenderBucket bucket = new RoomRenderBucket
                                {
                                    Room = room,
                                    X = x,
                                    Z = z,
                                    Face = face,
                                    BlendMode = BlendMode.Normal,
                                    DoubleSided = faceTexture.DoubleSided
                                };

                                _solidBuckets.Add(bucket);
                            }
                            else if (faceTexture.Texture == TextureInvisible.Instance)
                            {
                                RoomRenderBucket bucket = new RoomRenderBucket
                                {
                                    Face = face,
                                    X = x,
                                    Z = z,
                                    Room = room,
                                    BlendMode = BlendMode.Normal,
                                    DoubleSided = faceTexture.DoubleSided
                                };

                                _invisibleBuckets.Add(bucket);
                            }
                            else if (faceTexture.BlendMode == BlendMode.Normal)
                            {
                                RoomRenderBucket foundBucket = null;
                                foreach (RoomRenderBucket bucket in _opaqueBuckets)
                                    if (bucket.Room == room && (bucket.DoubleSided == faceTexture.DoubleSided))
                                    {
                                        foundBucket = bucket;
                                        break;
                                    }

                                if (foundBucket == null)
                                {
                                    foundBucket = new RoomRenderBucket
                                    {
                                        Room = room,
                                        X = x,
                                        Z = z,
                                        Face = face,
                                        DoubleSided = faceTexture.DoubleSided,
                                        BlendMode = BlendMode.Normal
                                    };

                                    _opaqueBuckets.Add(foundBucket);
                                }

                                var vertexRange = room.GetFaceVertexRange(x, z, face);
                                for (int j = 0; j < vertexRange.Count; ++j)
                                    foundBucket.Indices.Add((short)(vertexRange.Start + j));
                            }
                            else
                            {
                                RoomRenderBucket bucket = new RoomRenderBucket
                                {
                                    Room = room,
                                    X = x,
                                    Z = z,
                                    Face = face,
                                    DoubleSided = faceTexture.DoubleSided,
                                    BlendMode = faceTexture.BlendMode
                                };

                                // calcolo il piano passante per la faccia

                                // calcolo il centro della faccia
                                Vector3 center = Vector3.Zero;
                                var vertexRange = room.GetFaceVertexRange(x, z, face);
                                for (int j = 0; j < vertexRange.Count; j++)
                                    center += room.GetRoomVertices()[j].Position;
                                center /= vertexRange.Count;

                                // calcolo la distanza
                                bucket.Distance = (center - cameraPosition).Length();

                                // aggiungo la struttura alla lista
                                _transparentBuckets.Add(bucket);
                            }
                        }
            }

            // Sort buckets
            var comparer = new Comparer(_editor.Level);
            _opaqueBuckets.Sort(comparer);
            _transparentBuckets.Sort(comparer);
            _invisibleBuckets.Sort(comparer);

            Parallel.ForEach(_opaqueBuckets, PrepareIndexBuffer);
        }

        private void PrepareIndexBuffer(RoomRenderBucket item)
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
            _debug = new Debug();
            _drawHeightLine = false;
            _drawFlybyPath = false;
            _boundingBoxesToDraw = new List<BoundingBoxToDraw>();

            if ((_editor.Mode == EditorMode.FaceEdit || _editor.Mode == EditorMode.Lighting) && DrawPortals)
                AddRoomBoundingBox();

            // Don't draw anything if device is not ready
            if (_device == null || _editor.SelectedRoom == null)
                return;

            // reset the backbuffer
            Vector4 clearColor = _editor?.SelectedRoom?.AlternateBaseRoom != null ? _editor.Configuration.Rendering3D_BackgroundColorFlipRoom : _editor.Configuration.Rendering3D_BackgroundColor;
            _device.Presenter = _presenter;
            _device.SetViewports(new ViewportF(0, 0, Width, Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer, _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, clearColor, 1.0f, 0);
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

            _debug.NumRooms = _roomsToDraw.Count;

            Task task1 = Task.Factory.StartNew(RenderTask1, viewProjection);
            Task task2 = Task.Factory.StartNew(RenderTask2, viewProjection);
            Task.WaitAll(task1, task2);

            // Draw the skybox if present
            if (_editor != null && _editor.Level != null && _editor.Level.Wad != null && DrawHorizon)
            {
                DrawSkyBox(viewProjection);
                _device.Clear(ClearOptions.DepthBuffer, SharpDX.Color.Transparent, 1.0f, 0);
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
                DrawRoomImportedGeometry(viewProjection);
            }

            // Set some common parameters of the shader
            _roomEffect = _deviceManager.Effects["Room"];
            _roomEffect.Parameters["UseVertexColors"].SetValue(false);
            _roomEffect.Parameters["TextureEnabled"].SetValue(false);
            _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(false);
            _roomEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);
            _roomEffect.Parameters["LineWidth"].SetValue(_editor.Configuration.Rendering3D_LineWidth);
            _roomEffect.Parameters["TextureAtlasRemappingSize"].SetValue(_textureAtlasRemappingSize);
            _roomEffect.Parameters["TextureCoordinateFactor"].SetValue(_textureAtlas == null ? new Vector2(0) : new Vector2(1.0f / _textureAtlas.Width, 1.0f / _textureAtlas.Height));

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
            _debug.Fps = 1.0 / _watch.Elapsed.TotalSeconds;

            // Draw debug info
            Effect solidEffect = _deviceManager.Effects["Solid"];
            solidEffect.Techniques[0].Passes[0].Apply();
            _debug.Draw(_deviceManager, _editor.SelectedObject?.ToString(), _editor.Configuration.Rendering3D_TextColor);

            _device.Present();

            //logger.Debug("Draw Call! " + _watch.Elapsed.TotalSeconds + "ms");
        }

        private void DrawSelectedFogBulb()
        {
            LightInstance light = _editor.SelectedObject as LightInstance;
            if (light != null)
            {
                if (light.Type == LightType.FogBulb)
                {
                    _roomEffect.Parameters["FogBulbEnabled"].SetValue(true);
                    _roomEffect.Parameters["FogBulbIntensity"].SetValue(light.Intensity);
                    _roomEffect.Parameters["FogBulbPosition"].SetValue(Vector3.Transform(light.Position, _editor.SelectedRoom.Transform));
                    _roomEffect.Parameters["FogBulbRadius"].SetValue(light.OuterRange * 1024.0f);
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
                string message = _roomsToDraw[i].Name;

                Vector3 pos = _roomsToDraw[i].WorldPos;
                Matrix wvp = Matrix.Translation(pos) * viewProjection;
                Vector3 screenPos = Vector3.Project(_roomsToDraw[i].GetLocalCenter(), 0, 0, Width, Height,
                    _device.Viewport.MinDepth,
                    _device.Viewport.MaxDepth, wvp);
                _debug.AddString(message, screenPos);
            }
        }

        private void AddDirectionsToDebug(Matrix viewProjection)
        {
            float xBlocks = _editor.SelectedRoom.NumXSectors / 2.0f * 1024.0f;
            float zBlocks = _editor.SelectedRoom.NumZSectors / 2.0f * 1024.0f;

            string[] messages = { "+Z (North)", "-Z (South)", "-X (East)", "+X (West)" };
            Vector3[] positions = new Vector3[4];

            Vector3 center = _editor.SelectedRoom.GetLocalCenter();
            Vector3 pos = _editor.SelectedRoom.WorldPos;

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
                _debug.AddString(messages[i], screenPos);
            }
        }

        private void DrawOpaqueBuckets(Matrix viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(true);
            _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(false);
            _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

            RoomRenderBucket _lastBucket = null;

            // Draw opaque faces
            for (int i = 0; i < _opaqueBuckets.Count; i++)
            {
                RoomRenderBucket bucket = _opaqueBuckets[i];

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
                    _roomEffect.Parameters["UseVertexColors"].SetValue(lights);

                    _roomEffect.Parameters["Model"].SetValue(room.Transform);
                }

                _device.SetIndexBuffer(bucket.IndexBuffer, false);

                // Enable or disable double sided textures
                if (_lastBucket == null || _lastBucket.DoubleSided != bucket.DoubleSided)
                {
                    if (bucket.DoubleSided)
                        _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                    else
                        _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                }

                // Change texture if needed
                if (_lastBucket == null)
                {
                    _roomEffect.Parameters["Texture"].SetResource(_textureAtlas);
                    _roomEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);
                }

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the face
                _device.DrawIndexed(PrimitiveType.TriangleList, bucket.IndexBuffer.ElementCount); // face.Vertices.Count, face.StartVertex);

                _debug.NumVerticesRooms += bucket.IndexBuffer.ElementCount;
                _debug.NumTrianglesRooms += bucket.IndexBuffer.ElementCount / 3;

                _lastBucket = bucket;
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawInvisibleBuckets(Matrix viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(false);
            _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(true);
            _roomEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            _roomEffect.Parameters["UseVertexColors"].SetValue(false);
            _device.SetBlendState(_device.BlendStates.AlphaBlend);

            RoomRenderBucket _lastBucket = null;

            // Draw opaque faces
            for (int i = 0; i < _invisibleBuckets.Count; i++)
            {
                RoomRenderBucket bucket = _invisibleBuckets[i];

                var room = bucket.Room;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _device.SetVertexBuffer(room.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
                    _roomEffect.Parameters["ModelViewProjection"].SetValue(room.Transform * viewProjection);
                }

                // Set shape
                _roomEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the face
                var vertexRange = room.GetFaceVertexRange(bucket.X, bucket.Z, bucket.Face);
                _device.Draw(PrimitiveType.TriangleList, vertexRange.Count, vertexRange.Start);

                _debug.NumVerticesRooms += vertexRange.Count;
                _debug.NumTrianglesRooms += vertexRange.Count / 3;

                _lastBucket = bucket;
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawTransparentBuckets(Matrix viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(true);
            _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(false);

            RoomRenderBucket _lastBucket = null;

            // Draw opaque faces
            for (int i = 0; i < _transparentBuckets.Count; i++)
            {
                RoomRenderBucket bucket = _transparentBuckets[i];

                var room = bucket.Room;
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
                    _roomEffect.Parameters["UseVertexColors"].SetValue(lights);

                    _roomEffect.Parameters["Model"].SetValue(room.Transform);
                }

                // Enable or disable double sided textures
                if (_lastBucket == null || _lastBucket.DoubleSided != bucket.DoubleSided)
                {
                    if (bucket.DoubleSided)
                        _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                    else
                        _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                }

                // Check if is alpha trasparency or magenta trasparency
                if (_lastBucket == null)
                {
                    if (bucket.BlendMode == BlendMode.Additive)
                        _device.SetBlendState(_device.BlendStates.Additive);
                    else
                        _device.SetBlendState(_device.BlendStates.AlphaBlend);
                }

                // Change texture if needed
                if (_lastBucket == null)
                {
                    _roomEffect.Parameters["Texture"].SetResource(_textureAtlas);
                    _roomEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);
                }

                // Set shape
                _roomEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the face
                var vertexRange = room.GetFaceVertexRange(bucket.X, bucket.Z, bucket.Face);
                _device.Draw(PrimitiveType.TriangleList, vertexRange.Count, vertexRange.Start);

                _debug.NumVerticesRooms += vertexRange.Count;
                _debug.NumTrianglesRooms += vertexRange.Count / 3;

                _lastBucket = bucket;
            }

            _device.SetBlendState(_device.BlendStates.AlphaBlend);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawSolidBuckets(Matrix viewProjection)
        {
            RoomRenderBucket _lastBucket = null;

            // Draw solid faces
            for (int i = 0; i < _solidBuckets.Count; i++)
            {
                RoomRenderBucket bucket = _solidBuckets[i];
                var room = bucket.Room;

                int x = bucket.X;
                int z = bucket.Z;
                BlockFace face = bucket.Face;

                // If room is changed, setup vertex buffers, world matrix and lighting
                if (_lastBucket == null || _lastBucket.Room != bucket.Room)
                {
                    // Load the vertex buffer in the GPU and set the world matrix
                    _device.SetVertexBuffer(room.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, room.VertexBuffer));
                    _roomEffect.Parameters["ModelViewProjection"].SetValue(room.Transform * viewProjection);
                    _roomEffect.Parameters["UseVertexColors"].SetValue(false);
                }

                // Calculate the bounds of the current selection
                int xMin = Math.Min(_editor.SelectedSectors.Start.X, _editor.SelectedSectors.End.X);
                int xMax = Math.Max(_editor.SelectedSectors.Start.X, _editor.SelectedSectors.End.X);
                int zMin = Math.Min(_editor.SelectedSectors.Start.Y, _editor.SelectedSectors.End.Y);
                int zMax = Math.Max(_editor.SelectedSectors.Start.Y, _editor.SelectedSectors.End.Y);

                if (face < (BlockFace)10)
                {
                    _roomEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 80.0f / 255.0f, 0.0f, 1.0f));
                }
                else if (face >= (BlockFace)10 && face < (BlockFace)15)
                {
                    _roomEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 160.0f / 255.0f, 0.0f, 1.0f));
                }
                else if (face >= (BlockFace)15 && face < (BlockFace)25)
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
                    if (room.Blocks[x, z].Triggers.Count != 0)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(Editor.ColorTrigger));
                }

                // Portals
                if (face < (BlockFace)25)
                {
                    if (room.Blocks[x, z].WallPortal != null)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                if (face == (BlockFace)25 || face == (BlockFace)26)
                {
                    if (room.Blocks[x, z].FloorPortal != null)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                if (face == (BlockFace)27 || face == (BlockFace)28)
                {
                    if (room.Blocks[x, z].CeilingPortal != null)
                        _roomEffect.Parameters["Color"].SetValue(GetSharpdDXColor(System.Drawing.Color.Yellow));
                }

                // Enable UV coordinates
                _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(true);
                _roomEffect.Parameters["TextureEnabled"].SetValue(false);

                if (x >= xMin && x <= xMax && xMin != -1 && zMin != -1 && xMax != -1 && zMax != -1 &&
                    z >= zMin && z <= zMax && room == _editor.SelectedRoom)
                {
                    // We are in a selected area, so enable selection color
                    _roomEffect.Parameters["UseVertexColors"].SetValue(false);
                    _roomEffect.Parameters["Color"].SetValue(new Vector4(0.998f, 0.0f, 0.0f, 1.0f)); // Selection color

                    // Apply arrows to floor and ceiling
                    if (face == (BlockFace)25 || face == (BlockFace)26)
                    {
                        switch (_editor.SelectedSectors.Arrow)
                        {
                            case EditorArrowType.EdgeN:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.EdgeE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_right"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.EdgeS:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.EdgeW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_left"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.CornerNW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.CornerNE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.CornerSE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.CornerSW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                        }
                    }
                    else if (face == (BlockFace)27 || face == (BlockFace)28)
                    {
                        switch (_editor.SelectedSectors.Arrow)
                        {
                            case EditorArrowType.EdgeN:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.EdgeE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_right"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.EdgeS:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.EdgeW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_left"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.CornerNW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.CornerNE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.CornerSE:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                            case EditorArrowType.CornerSW:
                                _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                break;
                        }
                    }
                    else if (!(face == (BlockFace)4 || face == (BlockFace)9 || face == (BlockFace)14 || face == (BlockFace)19 || face == (BlockFace)24 ||
                             (room.Blocks[x, z].Type != BlockType.Wall &&
                              room.Blocks[x, z].Type != BlockType.BorderWall)))
                    {
                        // South faces ------------------------------------------------------------------------------
                        if (face == BlockFace.NegativeZ_QA || face == BlockFace.NegativeZ_ED)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]); break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]); break;
                            }
                        }

                        if (face == BlockFace.NegativeZ_WS || face == BlockFace.NegativeZ_RF)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]); break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]); break;
                            }
                        }

                        if (face == BlockFace.NegativeZ_Middle)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up_down"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]); break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]); break;
                            }
                        }

                        // East faces ------------------------------------------------------------------------------
                        if (face == BlockFace.NegativeX_QA || face == BlockFace.NegativeX_ED)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]); break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]); break;
                            }
                        }

                        if (face == BlockFace.NegativeX_WS || face == BlockFace.NegativeX_RF)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]); break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]); break;
                            }
                        }

                        if (face == BlockFace.NegativeX_Middle)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up_down"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]); break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]); break;
                            }
                        }

                        // North faces ------------------------------------------------------------------------------
                        if (face == BlockFace.PositiveZ_QA || face == BlockFace.PositiveZ_ED)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]); break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]); break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                            }
                        }

                        if (face == BlockFace.PositiveZ_WS || face == BlockFace.PositiveZ_RF)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]); break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]); break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                            }
                        }

                        if (face == BlockFace.PositiveZ_Middle)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up_down"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]); break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]); break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]); break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                            }
                        }

                        // West faces ------------------------------------------------------------------------------
                        if (face == BlockFace.PositiveX_QA || face == BlockFace.PositiveX_ED)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]); break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]); break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                            }
                        }

                        if (face == BlockFace.PositiveX_WS || face == BlockFace.PositiveX_RF)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]); break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]); break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                            }
                        }

                        if (face == BlockFace.PositiveX_Middle)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]); break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up_down"]); break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]); break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]); break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]); break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                            }
                        }

                        if (_editor.SelectedSectors.Arrow != EditorArrowType.EntireFace)
                        {
                            _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                            _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                        }
                    }
                }

                _roomEffect.CurrentTechnique.Passes[0].Apply();

                var vertexRange = room.GetFaceVertexRange(bucket.X, bucket.Z, bucket.Face);
                _device.Draw(PrimitiveType.TriangleList, vertexRange.Count, vertexRange.Start);

                _debug.NumVerticesRooms += vertexRange.Count;
                _debug.NumTrianglesRooms += vertexRange.Count / 3;

                _lastBucket = bucket;
            }
        }

        private static float GetFloorHeight(Room room, Vector3 position)
        {
            int xBlock = (int)Math.Max(0, Math.Min(room.NumXSectors - 1, Math.Floor(position.X / 1024.0f)));
            int zBlock = (int)Math.Max(0, Math.Min(room.NumZSectors - 1, Math.Floor(position.Z / 1024.0f)));

            // Get the base floor height
            return room.Blocks[xBlock, zBlock].FloorMin * 256.0f;
        }

        private static string GetObjectPositionString(Room room, PositionBasedObjectInstance instance)
        {
            // Get the distance between point and floor in units
            float height = instance.Position.Y - GetFloorHeight(room, instance.Position);

            string message = "Position: [" + instance.Position.X + ", " + instance.Position.Y + ", " + instance.Position.Z + "]";
            message += "\nSector Position: [" + instance.SectorPosition.X + ", " + instance.SectorPosition.Y + ", " + instance.SectorPosition.Z + "]";
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
            EditorVertex[] vertices = new EditorVertex[]
            {
                new EditorVertex { Position = position },
                new EditorVertex { Position = new Vector3(position.X,floorHeight,position.Z) }
            };

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
            flybyCameras.Sort((x, y) => x.Number.CompareTo(y.Number));

            // Create a vertex array
            List<EditorVertex> vertices = new List<EditorVertex>();

            for (int i = 0; i < flybyCameras.Count - 1; i++)
            {
                Vector3 room1pos = flybyCameras[i].Room.WorldPos;
                Vector3 room2pos = flybyCameras[i + 1].Room.WorldPos;

                EditorVertex v1 = new EditorVertex();
                v1.Position = flybyCameras[i].Position + room1pos;

                EditorVertex v2 = new EditorVertex();
                v2.Position = flybyCameras[i + 1].Position + room2pos;

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
