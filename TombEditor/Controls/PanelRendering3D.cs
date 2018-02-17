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
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Controls.ContextMenus;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
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
            public float VerticalCoord { get; set; }
            public VectorInt2 Pos { get; set; }
            public BlockFace Face { get; set; }

            public bool IsFloorHorizontalPlane => (Face == BlockFace.Floor || Face == BlockFace.FloorTriangle2);
            public bool IsCeilingHorizontalPlane => (Face == BlockFace.Ceiling || Face == BlockFace.CeilingTriangle2);
            public bool IsVerticalPlane => (!IsFloorHorizontalPlane && !IsCeilingHorizontalPlane);
            public bool BelongsToFloor => (IsFloorHorizontalPlane || Face <= BlockFace.DiagonalMiddle);
            public bool BelongsToCeiling => (IsCeilingHorizontalPlane || Face > BlockFace.DiagonalMiddle);
            public PickingResultBlock(float distance, float verticalCoord, VectorInt2 pos, BlockFace face)
            {
                Distance = distance;
                VerticalCoord = verticalCoord;
                Pos = pos;
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
                public short[,] Heights;
                public bool Processed;

                public ReferenceCell()
                {
                    Heights = new short[2, 4] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
                    Processed = false;
                }
            }

            private PanelRendering3D _parent;
            private ReferenceCell[,] _actionGrid = new ReferenceCell[Room.MaxRoomDimensions, Room.MaxRoomDimensions];
            private PickingResultBlock _referencePicking;
            private Point _referencePosition;
            private Point _newPosition;
            private Room _referenceRoom;

            // Terrain map resolution must be ALWAYS POWER OF 2 PLUS 1 - this is the requirement of diamond square algorithm.
            private const int TerrainMapResolution = 32 + 1;
            public float[,] RandomHeightMap = new float[TerrainMapResolution, TerrainMapResolution];

            public bool Engaged { get; private set; }
            public bool Dragged { get; private set; }
            public Block ReferenceBlock => _parent._editor.SelectedRoom.GetBlockTry(_referencePicking.Pos.X, _referencePicking.Pos.Y);
            public bool ReferenceIsFloor => _referencePicking.BelongsToFloor;
            public bool ReferenceIsDiagonalStep => (_referencePicking.BelongsToFloor ? ReferenceBlock.FloorDiagonalSplit != DiagonalSplit.None : ReferenceBlock.CeilingDiagonalSplit != DiagonalSplit.None);
            public bool ReferenceIsOppositeDiagonalStep
            {
                get
                {
                    if (ReferenceIsDiagonalStep)
                    {
                        if (_referencePicking.BelongsToFloor)
                        {
                            switch (ReferenceBlock.FloorDiagonalSplit)
                            {
                                case DiagonalSplit.XnZp:
                                    if (_referencePicking.Face == BlockFace.FloorTriangle2 ||
                                        _referencePicking.Face == BlockFace.NegativeZ_QA ||
                                        _referencePicking.Face == BlockFace.NegativeZ_ED ||
                                        _referencePicking.Face == BlockFace.PositiveX_QA ||
                                        _referencePicking.Face == BlockFace.PositiveX_ED)
                                        return true;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (_referencePicking.Face == BlockFace.Floor ||
                                        _referencePicking.Face == BlockFace.NegativeX_QA ||
                                        _referencePicking.Face == BlockFace.NegativeX_ED ||
                                        _referencePicking.Face == BlockFace.PositiveZ_QA ||
                                        _referencePicking.Face == BlockFace.PositiveZ_ED)
                                        return true;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (_referencePicking.Face == BlockFace.FloorTriangle2 ||
                                        _referencePicking.Face == BlockFace.NegativeX_QA ||
                                        _referencePicking.Face == BlockFace.NegativeX_ED ||
                                        _referencePicking.Face == BlockFace.NegativeZ_QA ||
                                        _referencePicking.Face == BlockFace.NegativeZ_ED)
                                        return true;
                                    break;
                                case DiagonalSplit.XnZn:
                                    if (_referencePicking.Face == BlockFace.Floor ||
                                        _referencePicking.Face == BlockFace.PositiveX_QA ||
                                        _referencePicking.Face == BlockFace.PositiveX_ED ||
                                        _referencePicking.Face == BlockFace.PositiveZ_QA ||
                                        _referencePicking.Face == BlockFace.PositiveZ_ED)
                                        return true;
                                    break;
                            }
                        }
                        else
                        {
                            switch (ReferenceBlock.CeilingDiagonalSplit)
                            {
                                case DiagonalSplit.XnZp:
                                    if (_referencePicking.Face == BlockFace.CeilingTriangle2 ||
                                        _referencePicking.Face == BlockFace.NegativeZ_WS ||
                                        _referencePicking.Face == BlockFace.NegativeZ_RF ||
                                        _referencePicking.Face == BlockFace.PositiveX_WS ||
                                        _referencePicking.Face == BlockFace.PositiveX_RF)
                                        return true;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (_referencePicking.Face == BlockFace.Ceiling ||
                                        _referencePicking.Face == BlockFace.NegativeX_WS ||
                                        _referencePicking.Face == BlockFace.NegativeX_RF ||
                                        _referencePicking.Face == BlockFace.PositiveZ_WS ||
                                        _referencePicking.Face == BlockFace.PositiveZ_RF)
                                        return true;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (_referencePicking.Face == BlockFace.CeilingTriangle2 ||
                                        _referencePicking.Face == BlockFace.NegativeX_WS ||
                                        _referencePicking.Face == BlockFace.NegativeX_RF ||
                                        _referencePicking.Face == BlockFace.NegativeZ_WS ||
                                        _referencePicking.Face == BlockFace.NegativeZ_RF)
                                        return true;
                                    break;
                                case DiagonalSplit.XnZn:
                                    if (_referencePicking.Face == BlockFace.Ceiling ||
                                        _referencePicking.Face == BlockFace.PositiveX_WS ||
                                        _referencePicking.Face == BlockFace.PositiveX_RF ||
                                        _referencePicking.Face == BlockFace.PositiveZ_WS ||
                                        _referencePicking.Face == BlockFace.PositiveZ_RF)
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

                for (int i = 0; i < Room.MaxRoomDimensions; i++)
                    for (int j = 0; j < Room.MaxRoomDimensions; j++)
                        _actionGrid[i, j] = new ReferenceCell();
            }

            private void PrepareActionGrid()
            {
                for (int x = 0; x < _parent._editor.SelectedRoom.NumXSectors; x++)
                    for (int z = 0; z < _parent._editor.SelectedRoom.NumZSectors; z++)
                    {
                        _actionGrid[x, z].Processed = false;

                        for (int i = 0; i < 4; i++)
                        {
                            if (_referencePicking.BelongsToFloor)
                            {
                                _actionGrid[x, z].Heights[0, i] = _referenceRoom.Blocks[x, z].QA[i];
                                _actionGrid[x, z].Heights[1, i] = _referenceRoom.Blocks[x, z].ED[i];
                            }
                            else
                            {
                                _actionGrid[x, z].Heights[0, i] = _referenceRoom.Blocks[x, z].WS[i];
                                _actionGrid[x, z].Heights[1, i] = _referenceRoom.Blocks[x, z].RF[i];
                            }
                        }
                    }
            }

            private void GenerateNewTerrain()
            {
                // Algorithm used here is naive Diamond-Square, which should be enough for low-res TR geometry.

                int s = TerrainMapResolution - 1;

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
                            average = RandomHeightMap[(x - halfSide + s) % (s), y];
                            average += RandomHeightMap[(x + halfSide) % (s), y];
                            average += RandomHeightMap[x, (y + halfSide) % (s)];
                            average += RandomHeightMap[x, (y - halfSide + s) % (s)];
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

                if (_referencePicking.Face == BlockFace.DiagonalED ||
                    _referencePicking.Face == BlockFace.DiagonalQA)
                {
                    switch (ReferenceBlock.FloorDiagonalSplit)
                    {
                        case DiagonalSplit.XnZp:
                        case DiagonalSplit.XpZp:
                            _referencePicking.Face = BlockFace.Floor;
                            break;
                        case DiagonalSplit.XpZn:
                        case DiagonalSplit.XnZn:
                            _referencePicking.Face = BlockFace.FloorTriangle2;
                            break;
                    }
                }
                else if (_referencePicking.Face == BlockFace.DiagonalWS ||
                         _referencePicking.Face == BlockFace.DiagonalRF)
                {
                    switch (ReferenceBlock.CeilingDiagonalSplit)
                    {
                        case DiagonalSplit.XnZp:
                        case DiagonalSplit.XpZp:
                            _referencePicking.Face = BlockFace.Ceiling;
                            break;
                        case DiagonalSplit.XpZn:
                        case DiagonalSplit.XnZn:
                            _referencePicking.Face = BlockFace.CeilingTriangle2;
                            break;
                    }
                }
                else if (_referencePicking.Face == BlockFace.NegativeX_Middle ||
                         _referencePicking.Face == BlockFace.NegativeZ_Middle ||
                         _referencePicking.Face == BlockFace.PositiveX_Middle ||
                         _referencePicking.Face == BlockFace.PositiveZ_Middle ||
                         _referencePicking.Face == BlockFace.DiagonalMiddle)
                {
                    Direction direction;
                    switch (_referencePicking.Face)
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

                    var face = EditorActions.GetFaces(_parent._editor.SelectedRoom, _referencePicking.Pos, direction, BlockFaceType.Wall).First((item) => item.Key == _referencePicking.Face);

                    if (face.Value[0] - _referencePicking.VerticalCoord > _referencePicking.VerticalCoord - face.Value[1])
                        switch (ReferenceBlock.FloorDiagonalSplit)
                        {
                            default:
                            case DiagonalSplit.XnZp:
                            case DiagonalSplit.XpZp:
                                _referencePicking.Face = BlockFace.Floor;
                                break;
                            case DiagonalSplit.XpZn:
                            case DiagonalSplit.XnZn:
                                _referencePicking.Face = BlockFace.FloorTriangle2;
                                break;
                        }
                    else
                        switch (ReferenceBlock.CeilingDiagonalSplit)
                        {
                            default:
                            case DiagonalSplit.XnZp:
                            case DiagonalSplit.XpZp:
                                _referencePicking.Face = BlockFace.Ceiling;
                                break;
                            case DiagonalSplit.XpZn:
                            case DiagonalSplit.XnZn:
                                _referencePicking.Face = BlockFace.CeilingTriangle2;
                                break;
                        }
                }
            }

            public void Engage(int refX, int refY, PickingResultBlock refPicking)
            {
                if (!Engaged)
                {
                    Engaged = true;
                    _referencePosition = new Point((int)(refX * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity), (int)(refY * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity));
                    _newPosition = _referencePosition;
                    _referencePicking = refPicking;
                    _referenceRoom = _parent._editor.SelectedRoom;
                    RelocatePicking();
                    PrepareActionGrid();

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
                }
            }

            public bool Process(int x, int y)
            {
                if (((_parent._editor.SelectedSectors.Valid && _parent._editor.SelectedSectors.Area.Contains(new VectorInt2(x, y))) || _parent._editor.SelectedSectors == SectorSelection.None) && !_actionGrid[x, y].Processed)
                {
                    _actionGrid[x, y].Processed = true;
                    return true;
                }
                else
                    return false;
            }

            public Point? UpdateDragState(int newX, int newY, bool relative)
            {
                var newPosition = new Point((int)(newX * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity), (int)(newY * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity));

                if (newPosition != _newPosition)
                {
                    Point delta;
                    if (relative)
                        delta = new Point(Math.Sign(_newPosition.X - newPosition.X), Math.Sign(_newPosition.Y - newPosition.Y));
                    else
                        delta = new Point(_referencePosition.X - newPosition.X, _referencePosition.Y - newPosition.Y);
                    _newPosition = newPosition;
                    Dragged = true;
                    return (delta);
                }
                else
                    return null;
            }

            public void DiscardEditedGeometry(bool autoUpdate = false)
            {
                for (int x = 0; x < _referenceRoom.NumXSectors; x++)
                    for (int z = 0; z < _referenceRoom.NumZSectors; z++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (_referencePicking.BelongsToFloor)
                            {
                                _referenceRoom.Blocks[x, z].QA[i] = _actionGrid[x, z].Heights[0, i];
                                _referenceRoom.Blocks[x, z].ED[i] = _actionGrid[x, z].Heights[1, i];
                            }
                            else
                            {
                                _referenceRoom.Blocks[x, z].WS[i] = _actionGrid[x, z].Heights[0, i];
                                _referenceRoom.Blocks[x, z].RF[i] = _actionGrid[x, z].Heights[1, i];
                            }
                        }
                    }

                if (autoUpdate)
                    EditorActions.SmartBuildGeometry(_referenceRoom, _referenceRoom.LocalArea);
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawIllegalSlopes { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowMoveables { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowStatics { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowImportedGeometry { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowLightMeshes { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowOtherObjects { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawSlideDirections { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisablePickingForImportedGeometry { get; set; }

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
        private System.Drawing.Point _startMousePosition;
        private MovementTimer _movementTimer;
        private bool _doSectorSelection = false;
        private bool _noSelectionConfirm = false;
        private static readonly Vector4 _selectionColor = new Vector4(3.0f, 0.2f, 0.2f, 1.0f);
        private Buffer<EditorVertex> _skyVertexBuffer;
        private Debug _debug;
        private RasterizerState _rasterizerStateDepthBias;

        // Context menus
        private BaseContextMenu _currentContextMenu;

        private static readonly HashSet<HighlightType> _ignoredHighlights = new HashSet<HighlightType>
        {
            HighlightType.Portal,
            HighlightType.BorderWall,
            HighlightType.Wall,
            HighlightType.Beetle,
            HighlightType.TriggerTriggerer
        };

        private static readonly List<HighlightShape> _usedShapes = new List<HighlightShape>
        {
            HighlightShape.Rectangle
        };

        // Current room's last position
        private Vector3? _currentRoomLastPos = null;

        // Gizmo
        private Gizmo _gizmo;

        // Tool handler
        private ToolHandler _toolHandler;

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
        private Buffer<SolidVertex> _objectHeightLineVertexBuffer;
        private bool _drawHeightLine = false;

        private Buffer<SolidVertex> _flybyPathVertexBuffer;
        private bool _drawFlybyPath = false;
        private List<BoundingBoxToDraw> _boundingBoxesToDraw;

        private Effect _roomEffect;

        public PanelRendering3D()
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

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
            _movementTimer?.Dispose();
            _rasterizerStateDepthBias?.Dispose();
            _currentContextMenu?.Dispose();
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

            // Move camera position with room movements
            if ((obj is Editor.RoomGeometryChangedEvent) && (_editor.Mode == EditorMode.Map2D) && _currentRoomLastPos.HasValue)
            {
                Camera.MoveCameraLinear(_editor.SelectedRoom.WorldPos - _currentRoomLastPos.Value);
                _currentRoomLastPos = _editor.SelectedRoom.WorldPos;
            }
            else if ((obj is Editor.SelectedRoomChangedEvent) || (obj is Editor.ModeChangedEvent))
                _currentRoomLastPos = _editor.SelectedRoom.WorldPos;

            // Reset tool handler state
            if ((obj is Editor.SelectedRoomChangedEvent) ||
               (obj is Editor.ModeChangedEvent) ||
               (obj is Editor.ToolChangedEvent))
            {
                _toolHandler?.Disengage();
            }

            // Update drawing
            if ((obj is IEditorObjectChangedEvent) ||
                (obj is IEditorRoomChangedEvent) ||
                (obj is HighlightManager.ChangeHighlightEvent) ||
                (obj is Editor.ConfigurationChangedEvent) ||
                (obj is Editor.SelectedObjectChangedEvent) ||
                (obj is Editor.SelectedSectorsChangedEvent) ||
                (obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.ModeChangedEvent) ||
                (obj is Editor.LoadedWadsChangedEvent) ||
                (obj is Editor.LoadedTexturesChangedEvent) ||
                (obj is Editor.LoadedImportedGeometriesChangedEvent))
            {
                //logger.Debug("Event Processed! " + obj.ToString());

                if (_editor.Mode != EditorMode.Map2D)
                    Invalidate();
            }

            // Update cursor
            if (obj is Editor.ActionChangedEvent)
            {
                IEditorAction currentAction = ((Editor.ActionChangedEvent)obj).Current;
                bool hasCrossCurser = (currentAction is EditorActionPlace) || (currentAction is EditorActionRelocateCamera);
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
            get { return 0.6f; }
        }
        private float DefaultCameraAngleY
        {
            get { return (float)Math.PI; }
        }

        public void ResetCamera()
        {
            Room room = _editor?.SelectedRoom;

            // Point the camera to the room's center
            Vector3 target = new Vector3();
            if (room != null)
                target = room.WorldPos + room.GetLocalCenter();

            // Initialize a new camera
            Camera = new ArcBallCamera(target, DefaultCameraAngleX, DefaultCameraAngleY, -(float)Math.PI / 2, (float)Math.PI / 2, DefaultCameraDistance, 2750, 1000000, _editor.Configuration.Rendering3D_FieldOfView * (float)(Math.PI / 180));
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
            _rasterizerWireframe = RasterizerState.New(_device, renderStateDesc);

            _rasterizerStateDepthBias = RasterizerState.New(_device, new SharpDX.Direct3D11.RasterizerStateDescription
            {
                CullMode = SharpDX.Direct3D11.CullMode.Back,
                FillMode = SharpDX.Direct3D11.FillMode.Solid,
                DepthBias = -2,
                SlopeScaledDepthBias = -2
            });

            _gizmo = new Gizmo(deviceManager.Device, deviceManager.Effects["Solid"]);
            _toolHandler = new ToolHandler(this);
            _movementTimer = new MovementTimer(MoveTimerTick);

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
            if ((LicenseManager.UsageMode != LicenseUsageMode.Runtime) || (_editor == null) || (_editor.Level == null))
            {
                e.Graphics.Clear(Parent.BackColor);
                e.Graphics.DrawString("3D Room Rendering: Not Available!", Font, Brushes.DarkGray, ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            else
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

            if ((ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.None)
                _movementTimer.Engage(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _movementTimer.Stop();
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

                if (newPicking is PickingResultBlock)
                {
                    PickingResultBlock newBlockPicking = (PickingResultBlock)newPicking;

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
                        if (!action.ShouldBeActive)
                            _editor.Action = null;
                        return;
                    }

                    // Act based on editor mode
                    VectorInt2 pos = newBlockPicking.Pos;
                    bool belongsToFloor = newBlockPicking.BelongsToFloor;
                    switch (_editor.Mode)
                    {
                        case EditorMode.Geometry:
                            if (_editor.Tool.Tool != EditorToolType.Selection)
                            {
                                _toolHandler.Engage(e.X, e.Y, newBlockPicking);

                                if (!ModifierKeys.HasFlag(Keys.Alt) && !ModifierKeys.HasFlag(Keys.Shift) && _toolHandler.Process(pos.X, pos.Y))
                                {
                                    if (_editor.Tool.Tool == EditorToolType.Smooth)
                                        EditorActions.SmoothSector(_editor.SelectedRoom, pos.X, pos.Y, (belongsToFloor ? 0 : 1));
                                    else if (_editor.Tool.Tool < EditorToolType.Flatten)
                                        EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                            new RectangleInt2(pos, pos),
                                            EditorArrowType.EntireFace,
                                            (belongsToFloor ? 0 : 1),
                                            (short)((_editor.Tool.Tool == EditorToolType.Shovel || (_editor.Tool.Tool == EditorToolType.Pencil && ModifierKeys.HasFlag(Keys.Control))) ^ belongsToFloor ? 1 : -1),
                                            (_editor.Tool.Tool == EditorToolType.Brush || _editor.Tool.Tool == EditorToolType.Shovel),
                                            false);
                                }
                            }
                            break;

                        case EditorMode.FaceEdit:
                            // Do texturing
                            if (_editor.Tool.Tool != EditorToolType.Group)
                            {
                                if (ModifierKeys.HasFlag(Keys.Control))
                                {
                                    EditorActions.MirrorTexture(_editor.SelectedRoom, pos, newBlockPicking.Face);
                                    break;
                                }
                                else if (ModifierKeys.HasFlag(Keys.Shift))
                                {
                                    EditorActions.RotateTexture(_editor.SelectedRoom, pos, newBlockPicking.Face);
                                    break;
                                }
                            }

                            if (ModifierKeys.HasFlag(Keys.Alt))
                            {
                                EditorActions.PickTexture(_editor.SelectedRoom, pos, newBlockPicking.Face);
                                break;
                            }
                            else if ((_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos)) || _editor.SelectedSectors == SectorSelection.None)
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
                                            EditorActions.TexturizeGroup(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, newBlockPicking.Face, ModifierKeys.HasFlag(Keys.Control), ModifierKeys.HasFlag(Keys.Shift));
                                        break;

                                    case EditorToolType.Brush:
                                    case EditorToolType.Pencil:
                                        EditorActions.ApplyTextureAutomatically(_editor.SelectedRoom, pos, newBlockPicking.Face, _editor.SelectedTexture);
                                        break;

                                    default:
                                        break;
                                }

                            }
                            break;
                    }

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
                        }
                    }

                }
                else if (newPicking is PickingResultObject)
                {
                    // Select new object
                    _editor.SelectedObject = ((PickingResultObject)newPicking).ObjectInstance;
                }
                else if (newPicking is PickingResultGizmo)
                {
                    // Set gizmo axis
                    _gizmo.ActivateGizmo((PickingResultGizmo)newPicking);
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

            switch (e.Button)
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

            bool redrawWindow = false;

            // Reset internal bool for deselection
            _noSelectionConfirm = false;

            // Hover effect on gizmo
            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(GetRay(e.X, e.Y))))
                redrawWindow = true;

            // Process action
            switch (e.Button)
            {
                case MouseButtons.Middle:
                case MouseButtons.Right:
                    // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                    float relativeDeltaX = (e.X - _lastMousePosition.X) / (float)Height;
                    float relativeDeltaY = (e.Y - _lastMousePosition.Y) / (float)Height;
                    if (ModifierKeys.HasFlag(Keys.Shift) || (e.Button == MouseButtons.Middle))
                        Camera.MoveCameraPlane(new Vector3(relativeDeltaX, relativeDeltaY, 0) *
                            _editor.Configuration.Rendering3D_NavigationSpeedMouseTranslate);
                    else if (ModifierKeys.HasFlag(Keys.Control))
                        Camera.Zoom((_editor.Configuration.Rendering3D_InvertMouseZoom ? relativeDeltaY : -relativeDeltaY) * _editor.Configuration.Rendering3D_NavigationSpeedMouseZoom);
                    else
                        Camera.Rotate(
                            relativeDeltaX * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                            -relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);

                    _gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), e.X, e.Y); // Update gizmo
                    redrawWindow = true;
                    break;

                case MouseButtons.Left:
                    if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), e.X, e.Y))
                    {
                        // Process gizmo
                        redrawWindow = true;
                    }
                    else if (_editor.Tool.Tool >= EditorToolType.Drag && _toolHandler.Engaged && !_doSectorSelection && _editor.SelectedSectors.Valid)
                    {
                        var dragValue = _toolHandler.UpdateDragState(e.X, e.Y, _editor.Tool.Tool == EditorToolType.Drag);
                        if (dragValue.HasValue)
                        {
                            int subdivisionToEdit = (_toolHandler.ReferenceIsFloor ? (ModifierKeys.HasFlag(Keys.Control) ? 2 : 0) : (ModifierKeys.HasFlag(Keys.Control) ? 3 : 1));

                            switch (_editor.Tool.Tool)
                            {
                                case EditorToolType.Drag:
                                    EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                        _editor.SelectedSectors.Area,
                                        _editor.SelectedSectors.Arrow,
                                        subdivisionToEdit,
                                        (short)Math.Sign(dragValue.Value.Y),
                                        ModifierKeys.HasFlag(Keys.Alt),
                                        _toolHandler.ReferenceIsOppositeDiagonalStep, true);
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
                                                for (int i = 0; i < 4; i++)
                                                {
                                                    if (belongsToFloor && _toolHandler.ReferenceIsFloor)
                                                    {
                                                        _editor.SelectedRoom.Blocks[pos.X, pos.Y].QA[i] = _toolHandler.ReferenceBlock.QA.Min();
                                                        _editor.SelectedRoom.Blocks[pos.X, pos.Y].ED[i] = _toolHandler.ReferenceBlock.ED.Min();
                                                    }
                                                    else if (!belongsToFloor && !_toolHandler.ReferenceIsFloor)
                                                    {
                                                        _editor.SelectedRoom.Blocks[pos.X, pos.Y].WS[i] = _toolHandler.ReferenceBlock.WS.Min();
                                                        _editor.SelectedRoom.Blocks[pos.X, pos.Y].RF[i] = _toolHandler.ReferenceBlock.RF.Min();
                                                    }
                                                }
                                                EditorActions.SmartBuildGeometry(_editor.SelectedRoom, new RectangleInt2(pos, pos));
                                                break;

                                            case EditorToolType.Smooth:
                                                EditorActions.SmoothSector(_editor.SelectedRoom, pos.X, pos.Y, (belongsToFloor ? 0 : 1));
                                                break;

                                            case EditorToolType.Drag:
                                            case EditorToolType.Terrain:
                                                break;

                                            default:
                                                EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                                    new RectangleInt2(pos, pos),
                                                    EditorArrowType.EntireFace,
                                                    (belongsToFloor ? 0 : 1),
                                                    (short)((_editor.Tool.Tool == EditorToolType.Shovel || (_editor.Tool.Tool == EditorToolType.Pencil && ModifierKeys.HasFlag(Keys.Control))) ^ belongsToFloor ? 1 : -1),
                                                    (_editor.Tool.Tool == EditorToolType.Brush || _editor.Tool.Tool == EditorToolType.Shovel),
                                                    false);
                                                break;
                                        }
                                        redrawWindow = true;
                                    }
                                }
                            }
                            else if (_editor.Mode == EditorMode.FaceEdit && _editor.Action == null && ModifierKeys == Keys.None)
                            {
                                if (_editor.Tool.Tool == EditorToolType.Brush)
                                {
                                    if ((_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos) ||
                                         _editor.SelectedSectors == SectorSelection.None))
                                        redrawWindow = EditorActions.ApplyTextureAutomatically(_editor.SelectedRoom, pos, newBlockPicking.Face, _editor.SelectedTexture);
                                }
                            }
                        }
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
                    if (_editor.Mode == EditorMode.Geometry)
                    {
                        PickingResultBlock newBlockPicking = DoPicking(GetRay(e.X, e.Y)) as PickingResultBlock;
                        if (newBlockPicking != null && !_toolHandler.Dragged)
                        {
                            VectorInt2 pos = newBlockPicking.Pos;
                            bool belongsToFloor = newBlockPicking.BelongsToFloor;

                            if (ModifierKeys.HasFlag(Keys.Alt))
                            {
                                // Split the faces
                                if (belongsToFloor)
                                    EditorActions.FlipFloorSplit(_editor.SelectedRoom, new RectangleInt2(pos, pos));
                                else
                                    EditorActions.FlipCeilingSplit(_editor.SelectedRoom, new RectangleInt2(pos, pos));
                                return;
                            }
                            else if (ModifierKeys.HasFlag(Keys.Shift))
                            {
                                // Rotate sector
                                EditorActions.RotateSectors(_editor.SelectedRoom, new RectangleInt2(pos, pos), belongsToFloor);
                                return;
                            }
                            else if (_editor.Tool.Tool == EditorToolType.Selection || _editor.Tool.Tool >= EditorToolType.Drag)
                                if (!_doSectorSelection && _editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos))
                                    // Rotate the arrows
                                    if (ModifierKeys.HasFlag(Keys.Control))
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
                    }
                    break;

                case MouseButtons.Right:
                    var distance = new Vector2(_startMousePosition.X, _startMousePosition.Y) - new Vector2(e.Location.X, e.Location.Y);
                    // EXPERIMENTAL: show context menus here
                    if (distance.Length() < 4.0f)
                    {
                        _currentContextMenu?.Dispose();
                        _currentContextMenu = null;

                        PickingResult newPicking = DoPicking(GetRay(e.X, e.Y));
                        if (newPicking is PickingResultObject)
                        {
                            ObjectInstance target = ((PickingResultObject)newPicking).ObjectInstance;
                            if (target is PositionBasedObjectInstance)
                                _currentContextMenu = new PositionBasedObjectContextMenu(_editor, (PositionBasedObjectInstance)target);
                        }
                        else if (newPicking is PickingResultBlock)
                        {
                            var pickedBlock = (newPicking as PickingResultBlock);
                            if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pickedBlock.Pos))
                                _currentContextMenu = new SelectedGeometryContextMenu(_editor, _editor.SelectedRoom, _editor.SelectedSectors.Area);
                            else
                                _currentContextMenu = new BlockContextMenu(_editor, _editor.SelectedRoom, pickedBlock.Pos);
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
                e.Effect = DragDropEffects.Move;
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
            PickingResult newPicking = DoPicking(GetRay(loc.X, loc.Y));

            if (newPicking is PickingResultBlock)
            {
                var newBlockPicking = (PickingResultBlock)newPicking;

                // Disallow dropping objects and geometry on non-floor faces
                if (!newBlockPicking.IsFloorHorizontalPlane)
                    return;

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

        private void MoveTimerTick(object sender, EventArgs e)
        {
            switch (_movementTimer.MoveDirection)
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
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, out distance) && (distance < minDistance))
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
                if (instance is MoveableInstance && ShowMoveables)
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
                        if (Collision.RayIntersectsBox(ray, box, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                }
                else if (instance is StaticInstance && ShowStatics)
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
                        if (Collision.RayIntersectsBox(ray, box, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                }
                else if (instance is ImportedGeometryInstance && ShowImportedGeometry && !DisablePickingForImportedGeometry)
                {
                    var geometry = (ImportedGeometryInstance)instance;

                    if (geometry?.Model?.DirectXModel?.Meshes?.ElementAt(0) != null)
                        foreach (ImportedGeometryMesh mesh in geometry.Model.DirectXModel.Meshes)
                            DoMeshPicking(ref result, ray, instance, mesh, geometry.ObjectMatrix);
                    else
                    {
                        BoundingBox box = new BoundingBox(
                            room.WorldPos + geometry.Position - new Vector3(_littleCubeRadius),
                            room.WorldPos + geometry.Position + new Vector3(_littleCubeRadius));
                        if (Collision.RayIntersectsBox(ray, box, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                }
                else if (ShowOtherObjects)
                {
                    if (instance is LightInstance)
                    {
                        BoundingSphere sphere = new BoundingSphere(room.WorldPos + instance.Position, _littleSphereRadius);
                        if (Collision.RayIntersectsSphere(ray, sphere, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                    else
                    {
                        BoundingBox box = new BoundingBox(
                            room.WorldPos + instance.Position - new Vector3(_littleCubeRadius),
                            room.WorldPos + instance.Position + new Vector3(_littleCubeRadius));
                        if (Collision.RayIntersectsBox(ray, box, out distance) && ((result == null) || (distance < result.Distance)))
                            result = new PickingResultObject(distance, instance);
                    }
                }

            // Check room geometry
            var roomIntersectInfo = room.RayIntersectsGeometry(new Ray(ray.Position - room.WorldPos, ray.Direction));
            if ((roomIntersectInfo != null) && ((result == null) || (roomIntersectInfo.Value.Distance < result.Distance)))
                result = new PickingResultBlock(roomIntersectInfo.Value.Distance, roomIntersectInfo.Value.VerticalCoord, roomIntersectInfo.Value.Pos, roomIntersectInfo.Value.Face);

            return result;
        }

        private Ray GetRay(float x, float y)
        {
            // Get the current ViewProjection matrix
            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);

            // First get the ray in 3D space from X, Y mouse coordinates
            Ray ray = _device.Viewport.GetPickRay(new Vector2(x, y), viewProjection);
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

        private void DrawDebugLines(Matrix4x4 viewProjection)
        {
            if (!_drawFlybyPath && !_drawHeightLine && _boundingBoxesToDraw.Count == 0)
                return;

            _device.SetRasterizerState(_rasterizerWireframe);

            Effect solidEffect = _deviceManager.Effects["Solid"];

            Matrix4x4 model = Matrix4x4.CreateTranslation(_editor.SelectedRoom.WorldPos);
            solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
            solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

            if (_drawHeightLine)
            {
                _device.SetVertexBuffer(_objectHeightLineVertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _objectHeightLineVertexBuffer));

                Matrix4x4 model2 = Matrix4x4.CreateTranslation(_editor.SelectedRoom.WorldPos);

                solidEffect.Parameters["ModelViewProjection"].SetValue((model2 * viewProjection).ToSharpDX());
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, 2);
            }

            if (_drawFlybyPath)
            {
                _device.SetVertexBuffer(_flybyPathVertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _flybyPathVertexBuffer));

                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
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

                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scaleX, scaleY, scaleZ);

                Matrix4x4 translateMatrix = Matrix4x4.CreateTranslation(boundingBox.Position /*+ new Vector3(halfX, halfY, halfZ)*/);

                solidEffect.Parameters["ModelViewProjection"].SetValue((scaleMatrix *
                                                                       translateMatrix *
                                                                       viewProjection).ToSharpDX());
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.DrawIndexed(PrimitiveType.LineList, _linesCube.IndexBuffer.ElementCount);
            }
        }

        private void BuildTriggeredByMessage(ref string message, ObjectInstance instance)
        {
            foreach (var room in _editor.Level.Rooms.Where(room => room != null))
                foreach (var trigger in room.Triggers)
                    if (trigger.Target == instance)
                        message += "\nTriggered in Room " + trigger.Room + " on sectors " + trigger.Area;
        }

        private void DrawLights(Matrix4x4 viewProjection, Room room)
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

                    Matrix4x4 model = Matrix4x4.CreateTranslation(light.Position) * Matrix4x4.CreateTranslation(Utils.PositionInWorldCoordinates(_editor.Level.Rooms[room].Position));
                    effect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());

                    effect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.LineList, 49);

                    continue;
                }*/

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
                _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
            }

            if (_editor.SelectedObject?.Room == room && _editor.SelectedObject is LightInstance)
            {
                LightInstance light = (LightInstance)_editor.SelectedObject;

                if (light.Type == LightType.Point || light.Type == LightType.Shadow || light.Type == LightType.FogBulb)
                {
                    _device.SetVertexBuffer(_sphere.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
                    _device.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);

                    Matrix4x4 model;

                    if (light.Type == LightType.Point || light.Type == LightType.Shadow)
                    {
                        model = Matrix4x4.CreateScale(light.InnerRange * 2.0f) * light.ObjectMatrix;
                        solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                        solidEffect.CurrentTechnique.Passes[0].Apply();
                        _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
                    }

                    model = Matrix4x4.CreateScale(light.OuterRange * 2.0f) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
                }
                else if (light.Type == LightType.Spot)
                {
                    _device.SetVertexBuffer(_cone.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _device.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    // Inner cone
                    float coneAngle = (float)Math.Atan2(512, 1024);
                    float lenScaleH = light.InnerRange;
                    float lenScaleW = (light.InnerAngle * (float)(Math.PI / 180)) / coneAngle * lenScaleH;

                    Matrix4x4 Model = Matrix4x4.CreateScale(lenScaleW, lenScaleW, lenScaleH) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue((Model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);

                    // Outer cone
                    float cutoffScaleH = light.OuterRange;
                    float cutoffScaleW = (light.OuterAngle * (float)(Math.PI / 180)) / coneAngle * cutoffScaleH;

                    Matrix4x4 model2 = Matrix4x4.CreateScale(cutoffScaleW, cutoffScaleW, cutoffScaleH) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model2 * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }
                else if (light.Type == LightType.Sun)
                {
                    _device.SetVertexBuffer(_cone.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cone.VertexBuffer));
                    _device.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                    Matrix4x4 model = Matrix4x4.CreateScale(0.01f, 0.01f, 1.0f) * light.ObjectMatrix;
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                string message = light.Type + " Light";

                // Object position
                message += "\n" + GetObjectPositionString(light.Room, light);

                DrawDebugString(message, light.ObjectMatrix * viewProjection);

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
        private void DrawObjects(Matrix4x4 viewProjection, Room room)
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

                    string message = "Camera " + (instance.Fixed ? "(Fixed)" : "") + " [ID = " + instance.ScriptId + "]";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);

                    BuildTriggeredByMessage(ref message, instance);
                    DrawDebugString(message, instance.ObjectMatrix * viewProjection);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                effect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());
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

                    string message = "Flyby camera (" + instance.Sequence + ":" + instance.Number + ") [ID = " + instance.ScriptId + "]";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);

                    BuildTriggeredByMessage(ref message, instance);
                    DrawDebugString(message, instance.ObjectMatrix * viewProjection);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);

                    // Add the path of the flyby
                    AddFlybyPath(flyby.Sequence);
                }

                effect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());
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

                    var message = "Sink [ID = " + instance.ScriptId + "]";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);

                    BuildTriggeredByMessage(ref message, instance);
                    DrawDebugString(message, instance.ObjectMatrix * viewProjection);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                effect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());
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

                    string message = "Sound source [ID = " + instance.ScriptId + "]";
                    if (_editor.Level.Wad != null &&
                        _editor.Level.Wad.Sounds.ContainsKey(instance.SoundId))
                        message += " (" + _editor.Level.Wad.Sounds[instance.SoundId].Name + ") ";
                    else
                        message += " ( Invalid or missing sound ) ";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);

                    BuildTriggeredByMessage(ref message, instance);
                    DrawDebugString(message, instance.ObjectMatrix * viewProjection);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                effect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());
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

                        Vector3 screenPos = _device.Viewport.Project(new Vector3(), instance.RotationPositionMatrix * viewProjection);

                        BuildTriggeredByMessage(ref message, instance);

                        _debug.AddString(message, screenPos);

                        // Add the line height of the object
                        AddObjectHeightLine(viewProjection, room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue((instance.RotationPositionMatrix * viewProjection).ToSharpDX());
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

                        DrawDebugString(message, instance.RotationPositionMatrix * viewProjection);

                        // Add the line height of the object
                        AddObjectHeightLine(viewProjection, room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue((instance.RotationPositionMatrix * viewProjection).ToSharpDX());
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

                        DrawDebugString(instance.ToString(), instance.RotationPositionMatrix * viewProjection);

                        // Add the line height of the object
                        AddObjectHeightLine(viewProjection, room, instance.Position);
                    }

                    effect.Parameters["ModelViewProjection"].SetValue((instance.RotationPositionMatrix * viewProjection).ToSharpDX());
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
                float cutoffScaleW = (instance.Fov * (float)(Math.PI / 360)) / coneAngle * cutoffScaleH;

                Matrix4x4 model = Matrix4x4.CreateScale(cutoffScaleW, cutoffScaleW, cutoffScaleH) * instance.ObjectMatrix;

                effect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                effect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                effect.CurrentTechnique.Passes[0].Apply();
                _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
        }

        private void DrawMoveables(Matrix4x4 viewProjection)
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
                SkinnedModel skin = ((instance.WadObjectId == 0 && _editor.Level.Wad.DirectXMoveables.ContainsKey(8)) ? _editor.Level.Wad.DirectXMoveables[8] : model);

                _debug.NumMoveables++;

                Room room = instance.Room;

                if (_lastObject == null || instance.WadObjectId != _lastObject.WadObjectId)
                {
                    _device.SetVertexBuffer(0, skin.VertexBuffer);
                    _device.SetIndexBuffer(skin.IndexBuffer, true);
                }

                if (_lastObject == null)
                {
                    _device.SetVertexInputLayout(
                        VertexInputLayout.FromBuffer<SkinnedVertex>(0, skin.VertexBuffer));
                }

                skinnedModelEffect.Parameters["Color"].SetValue(_editor.Mode == EditorMode.Lighting ? instance.Color : new Vector4(1.0f));
                if (_editor.SelectedObject == instance) // Selection
                    skinnedModelEffect.Parameters["Color"].SetValue(_selectionColor);

                for (int i = 0; i < skin.Meshes.Count; i++)
                {
                    SkinnedMesh mesh = skin.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    Matrix4x4 world = model.AnimationTransforms[i] * instance.ObjectMatrix;

                    skinnedModelEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());

                    skinnedModelEffect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                    {
                        _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);

                        _debug.NumVerticesObjects += submesh.Value.NumIndices;
                        _debug.NumTrianglesObjects += submesh.Value.NumIndices / 3;
                    }
                }

                if (_editor.SelectedObject == instance)
                {
                    string message = _editor.Level.Wad.Moveables[instance.WadObjectId].ToString() + " [ID = " + instance.ScriptId + "]";

                    // Object position
                    message += "\n" + GetObjectPositionString(room, instance);
                    message += "\n" + "Rotation Y: " + Math.Round(instance.RotationY, 2);

                    // Add OCB
                    if (instance.Ocb != 0)
                        message += "\nOCB: " + instance.Ocb;

                    BuildTriggeredByMessage(ref message, instance);
                    DrawDebugString(message, instance.ObjectMatrix * viewProjection);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, room, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private void DrawRoomImportedGeometry(Matrix4x4 viewProjection)
        {
            var geometryEffect = _deviceManager.Effects["RoomGeometry"];

            ImportedGeometryInstance _lastObject = null;

            for (var k = 0; k < _roomGeometryToDraw.Count; k++)
            {
                var instance = _roomGeometryToDraw[k];
                if (instance.Model?.DirectXModel == null)
                    continue;

                var model = instance.Model.DirectXModel;
                var room = instance.Room;
                var roomIndex = _editor.Level.Rooms.ReferenceIndexOf(room);

                var meshes = new List<ImportedGeometryMesh>();
                if (model.HasMultipleRooms && model.RoomMeshes.ContainsKey(roomIndex))
                    meshes.Add(model.RoomMeshes[roomIndex]);
                else
                    meshes.AddRange(model.Meshes);

                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, model.VertexBuffer));
                _device.SetVertexBuffer(0, model.VertexBuffer);
                _device.SetIndexBuffer(model.IndexBuffer, true);

                for (var i = 0; i < meshes.Count; i++)
                {
                    var mesh = meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    geometryEffect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());

                    geometryEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                    if (_editor.SelectedObject == instance)
                        geometryEffect.Parameters["Color"].SetValue(_selectionColor);

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
                        _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);

                        _debug.NumVerticesRooms += submesh.Value.NumIndices;
                        _debug.NumTrianglesRooms += submesh.Value.NumIndices / 3;
                    }
                }

                if (_editor.SelectedObject == instance)
                {
                    // Object position
                    DrawDebugString(instance.ToString() + "\n" +
                        GetObjectPositionString(_editor.SelectedRoom, instance), instance.ObjectMatrix * viewProjection);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, _editor.SelectedRoom, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private void DrawStatics(Matrix4x4 viewProjection)
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

                    staticMeshEffect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * viewProjection).ToSharpDX());

                    staticMeshEffect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                    {
                        _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);

                        _debug.NumVerticesObjects += submesh.Value.NumIndices;
                        _debug.NumTrianglesObjects += submesh.Value.NumIndices / 3;
                    }
                }

                if (_editor.SelectedObject == instance)
                {
                    string message = _editor.Level.Wad.Statics[instance.WadObjectId].ToString() + " [ID = " + instance.ScriptId + "]";

                    // Object position
                    message += "\n" + GetObjectPositionString(_editor.SelectedRoom, instance);
                    message += "\n" + "Rotation Y: " + Math.Round(instance.RotationY, 2);

                    BuildTriggeredByMessage(ref message, instance);
                    DrawDebugString(message, instance.ObjectMatrix * viewProjection);

                    // Add the line height of the object
                    AddObjectHeightLine(viewProjection, _editor.SelectedRoom, instance.Position);
                }

                _lastObject = instance;
            }
        }

        private void DrawSkyBox(Matrix4x4 viewProjection)
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

            Matrix4x4 skyMatrix = Matrix4x4.CreateScale(50.0f) * Matrix4x4.CreateTranslation(new Vector3(-409600.0f, 20480.0f, -409600.0f)) * _editor.SelectedRoom.Transform;
            skinnedModelEffect.Parameters["ModelViewProjection"].SetValue((skyMatrix * viewProjection).ToSharpDX());

            skinnedModelEffect.Parameters["Texture"].SetResource(_skyTexture);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);

            skinnedModelEffect.Techniques[0].Passes[0].Apply();
            _device.Draw(PrimitiveType.TriangleList, _skyVertexBuffer.ElementCount);
            */

            _device.SetVertexBuffer(0, skinnedModel.VertexBuffer);
            _device.SetIndexBuffer(skinnedModel.IndexBuffer, true);

            skinnedModelEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);
            skinnedModelEffect.Parameters["Color"].SetValue(Vector4.One);

            for (int i = 0; i < skinnedModel.Meshes.Count; i++)
            {
                SkinnedMesh mesh = skinnedModel.Meshes[i];

                Matrix4x4 modelMatrix = Matrix4x4.CreateScale(20.0f) * skinnedModel.AnimationTransforms[i] * _editor.SelectedRoom.Transform;
                skinnedModelEffect.Parameters["ModelViewProjection"].SetValue((modelMatrix * viewProjection).ToSharpDX());

                skinnedModelEffect.Techniques[0].Passes[0].Apply();

                foreach (var submesh in mesh.Submeshes)
                {
                    _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);

                    _debug.NumVerticesObjects += submesh.Value.NumIndices;
                    _debug.NumTrianglesObjects += submesh.Value.NumIndices / 3;
                }
            }
        }

        public void DrawDebugString(string message, Matrix4x4 transformation, Vector3 offset = new Vector3())
        {
            Vector3 screenPos = _device.Viewport.Project(Vector3.Zero, transformation);
            screenPos += offset; // Offset text a little bit
            _debug.AddString(message, screenPos);
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
                        if (!_roomsToDraw.Contains(theRoom))
                            _roomsToDraw.Add(theRoom);
                    }
                    else
                    {
                        if (theRoom.AlternateRoom != null)
                        {
                            visitedRooms.Add(theRoom);
                            if (!_roomsToDraw.Contains(theRoom.AlternateRoom))
                                _roomsToDraw.Add(theRoom.AlternateRoom);
                        }
                        else
                        {
                            visitedRooms.Add(theRoom);
                            if (!_roomsToDraw.Contains(theRoom))
                                _roomsToDraw.Add(theRoom);
                        }
                    }
                }
                else
                {
                    visitedRooms.Add(theRoom);
                    if (!_roomsToDraw.Contains(theRoom))
                        _roomsToDraw.Add(theRoom);
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

        private void PrepareRenderBuckets(Matrix4x4 viewProjection)
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

                                // Get the distance of the face from the camera
                                bucket.Distance = GetFaceDistance(x, z, room, face, cameraPosition);

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

        private float GetFaceDistance(int x, int z, Room room, BlockFace face, Vector3 cameraPosition)
        {
            Vector3 center = Vector3.Zero;
            var vertexRange = room.GetFaceVertexRange(x, z, face);
            for (int j = 0; j < vertexRange.Count; j++)
                center += room.GetRoomVertices()[j].Position;
            center /= vertexRange.Count;
            return (center - cameraPosition).Length();
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
            // Verify that editor is ready
            if (_editor == null ||
                _editor.Level == null ||
                _editor.SelectedRoom == null ||
                _editor.SelectedRoom.VertexBuffer == null ||
                _device == null)
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

            // reset the backbuffer
            Vector4 clearColor = _editor?.SelectedRoom?.AlternateBaseRoom != null ? _editor.Configuration.Rendering3D_BackgroundColorFlipRoom : _editor.Configuration.Rendering3D_BackgroundColor;
            _device.Presenter = _presenter;
            _device.SetViewports(new SharpDX.ViewportF(0, 0, ClientSize.Width, ClientSize.Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer, _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, clearColor.ToSharpDX(), 1.0f, 0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);

            //Precalculate the view projection matrix
            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);

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
            if (_editor.Level.Wad != null && DrawHorizon)
            {
                DrawSkyBox(viewProjection);
                _device.Clear(ClearOptions.DepthBuffer, SharpDX.Color.Transparent, 1.0f, 0);
            }

            // Set some common parameters of the shader
            _roomEffect = _deviceManager.Effects["Room"];
            _roomEffect.Parameters["Highlight"].SetValue(false);
            _roomEffect.Parameters["Dim"].SetValue(false);
            _roomEffect.Parameters["UseVertexColors"].SetValue(false);
            _roomEffect.Parameters["TextureEnabled"].SetValue(false);
            _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(false);
            _roomEffect.Parameters["Texture"].SetResource((Texture2D)null);
            _roomEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);
            _roomEffect.Parameters["LineWidth"].SetValue(_editor.Configuration.Rendering3D_LineWidth);
            _roomEffect.Parameters["TextureAtlasRemappingSize"].SetValue(_textureAtlasRemappingSize);
            _roomEffect.Parameters["TextureCoordinateFactor"].SetValue(_textureAtlas == null ? new Vector2(0) : new Vector2(1.0f / _textureAtlas.Width, 1.0f / _textureAtlas.Height));

            // Draw buckets
            DrawSolidBuckets(viewProjection);
            DrawSelectedFogBulb();
            DrawOpaqueBuckets(viewProjection);

            // Draw moveables and static meshes
            if (_editor.Level.Wad != null)
            {
                // Before drawing custom geometry, apply a depth bias for reducing Z fighting
                _device.SetRasterizerState(_rasterizerStateDepthBias);

                if (ShowMoveables)
                    DrawMoveables(viewProjection);
                if (ShowStatics)
                    DrawStatics(viewProjection);

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            }

            DrawTransparentBuckets(viewProjection);
            DrawInvisibleBuckets(viewProjection);

            // Draw room imported geometry
            if (_roomGeometryToDraw.Count != 0)
            {
                // If picking for imported geometry is disabled, then draw geometry translucent
                if (DisablePickingForImportedGeometry)
                    _device.SetBlendState(_device.BlendStates.Additive);

                // Before drawing custom geometry, apply a depth bias for reducing Z fighting
                _device.SetRasterizerState(_rasterizerStateDepthBias);

                // Draw imported geometry
                DrawRoomImportedGeometry(viewProjection);

                // Reset GPU states
                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                if (DisablePickingForImportedGeometry)
                    _device.SetBlendState(_device.BlendStates.Opaque);
            }

            if (ShowOtherObjects)
            {
                // Draw objects (sinks, cameras, fly-by cameras and sound sources) only for current room
                DrawObjects(viewProjection, _editor.SelectedRoom);
                // Draw light objects and bounding volumes only for current room
                DrawLights(viewProjection, _editor.SelectedRoom);
            }

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

            logger.Debug("Draw Call! " + _watch.Elapsed.TotalSeconds + "ms");
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
                    _roomEffect.Parameters["FogBulbPosition"].SetValue(MathC.HomogenousTransform(light.Position, _editor.SelectedRoom.Transform));
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
            Matrix4x4 viewProjection = (Matrix4x4)viewProjection_;

            // Collect objects to draw
            CollectObjectsToDraw();

            // Now group faces to render based on various things
            PrepareRenderBuckets(viewProjection);
        }

        private void RenderTask2(object viewProjection_)
        {
            Matrix4x4 viewProjection = (Matrix4x4)viewProjection_;

            // Add room names
            if (DrawRoomNames)
            {
                AddRoomNamesToDebug(viewProjection);
            }

            // Draw North, South, East and West
            AddDirectionsToDebug(viewProjection);
        }

        private void AddRoomNamesToDebug(Matrix4x4 viewProjection)
        {
            for (int i = 0; i < _roomsToDraw.Count; i++)
            {
                string message = _roomsToDraw[i].Name;

                var pos = _roomsToDraw[i].WorldPos;
                var world = Matrix4x4.CreateTranslation(pos);
                Matrix4x4 wvp = world * viewProjection;
                Vector3 screenPos = _device.Viewport.Project(_roomsToDraw[i].GetLocalCenter(), wvp);
                _debug.AddString(message, screenPos);
            }
        }

        private void AddDirectionsToDebug(Matrix4x4 viewProjection)
        {
            float xBlocks = _editor.SelectedRoom.NumXSectors / 2.0f * 1024.0f;
            float zBlocks = _editor.SelectedRoom.NumZSectors / 2.0f * 1024.0f;

            string[] messages = { "+Z (North)", "-Z (South)", "+X (East)", "-X (West)" };
            Vector3[] positions = new Vector3[4];

            Vector3 center = _editor.SelectedRoom.GetLocalCenter();
            Vector3 pos = _editor.SelectedRoom.WorldPos;

            positions[0] = center + new Vector3(0, 0, zBlocks);
            positions[1] = center + new Vector3(0, 0, -zBlocks);
            positions[2] = center + new Vector3(xBlocks, 0, 0);
            positions[3] = center + new Vector3(-xBlocks, 0, 0);

            Matrix4x4 wvp = Matrix4x4.CreateTranslation(pos) * viewProjection;
            for (int i = 0; i < 4; i++)
            {
                Vector3 screenPos = _device.Viewport.Project(positions[i], wvp);
                _debug.AddString(messages[i], screenPos - new Vector3(45, 0, 0));
            }
        }

        private void DrawOpaqueBuckets(Matrix4x4 viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(true);
            _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(false);
            _roomEffect.Parameters["Highlight"].SetValue(false);
            _roomEffect.Parameters["Dim"].SetValue(false);
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
                    _roomEffect.Parameters["ModelViewProjection"].SetValue((room.Transform * viewProjection).ToSharpDX());

                    // Enable or disable static lighting
                    _roomEffect.Parameters["UseVertexColors"].SetValue(_editor.Mode == EditorMode.Lighting);

                    // Set blend mode
                    if (_lastBucket == null || _lastBucket.BlendMode != bucket.BlendMode)
                    {
                        if (bucket.BlendMode == BlendMode.Normal)
                            _device.SetBlendState(_device.BlendStates.Opaque);
                        else if (bucket.BlendMode == BlendMode.Additive)
                            _device.SetBlendState(_device.BlendStates.Additive);
                        else if (bucket.BlendMode == BlendMode.AlphaTest)
                            _device.SetBlendState(_device.BlendStates.AlphaBlend);
                        else
                            _device.SetBlendState(_device.BlendStates.Opaque);
                    }

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

        private void DrawInvisibleBuckets(Matrix4x4 viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(false);
            _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(true);
            _roomEffect.Parameters["Highlight"].SetValue(false);
            _roomEffect.Parameters["Dim"].SetValue(false);
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
                    _roomEffect.Parameters["ModelViewProjection"].SetValue((room.Transform * viewProjection).ToSharpDX());
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

        private void DrawTransparentBuckets(Matrix4x4 viewProjection)
        {
            // Setup shader
            _roomEffect.Parameters["TextureEnabled"].SetValue(true);
            _roomEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(false);
            _roomEffect.Parameters["Highlight"].SetValue(false);
            _roomEffect.Parameters["Dim"].SetValue(false);

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
                    _roomEffect.Parameters["ModelViewProjection"].SetValue((room.Transform * viewProjection).ToSharpDX());

                    // Enable or disable static lighting
                    bool lights = (room != _editor.SelectedRoom || _editor.Mode == EditorMode.Lighting);
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

        private void DrawSolidBuckets(Matrix4x4 viewProjection)
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
                    _roomEffect.Parameters["ModelViewProjection"].SetValue((room.Transform * viewProjection).ToSharpDX());
                    _roomEffect.Parameters["UseVertexColors"].SetValue(false);
                }

                // Reset selection highlight / dim for all faces
                _roomEffect.Parameters["Highlight"].SetValue(false);
                _roomEffect.Parameters["Dim"].SetValue(false);

                // Calculate the bounds of the current selection
                int xMin = Math.Min(_editor.SelectedSectors.Start.X, _editor.SelectedSectors.End.X);
                int xMax = Math.Max(_editor.SelectedSectors.Start.X, _editor.SelectedSectors.End.X);
                int zMin = Math.Min(_editor.SelectedSectors.Start.Y, _editor.SelectedSectors.End.Y);
                int zMax = Math.Max(_editor.SelectedSectors.Start.Y, _editor.SelectedSectors.End.Y);

                if (face < (BlockFace)25)
                {
                    BlockFlags climbDirection;
                    Room.RoomBlockPair lookupBlock;

                    // To highlight desired wall, we need to look into appropriate adjacent block

                    switch (face)
                    {
                        case BlockFace.PositiveX_ED:
                        case BlockFace.PositiveX_Middle:
                        case BlockFace.PositiveX_QA:
                        case BlockFace.PositiveX_RF:
                        case BlockFace.PositiveX_WS:
                            climbDirection = BlockFlags.ClimbNegativeX;
                            lookupBlock = room.ProbeLowestBlock(x + 1, z, _editor.Configuration.Editor_ProbeAttributesThroughPortals);
                            break;
                        case BlockFace.NegativeX_ED:
                        case BlockFace.NegativeX_Middle:
                        case BlockFace.NegativeX_QA:
                        case BlockFace.NegativeX_RF:
                        case BlockFace.NegativeX_WS:
                            climbDirection = BlockFlags.ClimbPositiveX;
                            lookupBlock = room.ProbeLowestBlock(x - 1, z, _editor.Configuration.Editor_ProbeAttributesThroughPortals);
                            break;
                        case BlockFace.NegativeZ_ED:
                        case BlockFace.NegativeZ_Middle:
                        case BlockFace.NegativeZ_QA:
                        case BlockFace.NegativeZ_RF:
                        case BlockFace.NegativeZ_WS:
                            climbDirection = BlockFlags.ClimbPositiveZ;
                            lookupBlock = room.ProbeLowestBlock(x, z - 1, _editor.Configuration.Editor_ProbeAttributesThroughPortals);
                            break;
                        case BlockFace.PositiveZ_ED:
                        case BlockFace.PositiveZ_Middle:
                        case BlockFace.PositiveZ_QA:
                        case BlockFace.PositiveZ_RF:
                        case BlockFace.PositiveZ_WS:
                            climbDirection = BlockFlags.ClimbNegativeZ;
                            lookupBlock = room.ProbeLowestBlock(x, z + 1, _editor.Configuration.Editor_ProbeAttributesThroughPortals);
                            break;
                        default:
                            climbDirection = BlockFlags.None;
                            lookupBlock = new Room.RoomBlockPair();
                            break;
                    }

                    if (lookupBlock.Block != null && lookupBlock.Block.HasFlag(climbDirection))
                        _roomEffect.Parameters["Color"].SetValue(HighlightState.ColorClimb);
                    else
                    {
                        if (room.Blocks[x, z].WallPortal == null)
                        {
                            if (face < (BlockFace)10)
                            {
                                _roomEffect.Parameters["Color"].SetValue(HighlightState.ColorWallUpper);
                            }
                            else if (face >= (BlockFace)10 && face < (BlockFace)15)
                            {
                                _roomEffect.Parameters["Color"].SetValue(HighlightState.ColorWall);
                            }
                            else if (face >= (BlockFace)15 && face < (BlockFace)25)
                            {
                                _roomEffect.Parameters["Color"].SetValue(HighlightState.ColorWallMiddle);
                            }
                        }
                        else
                        {
                            // Wall sections on wall portals rendered yellow.
                            _roomEffect.Parameters["Color"].SetValue(HighlightState.ColorPortalFace);
                        }
                    }
                }
                else
                {
                    // For now, we only render rectangular solid highlights, so use single rectangle solid shape in _usedShapes list, and use first and only entry in returned highlight list.
                    var currentHighlights = _editor.HighlightManager.GetColors(room, x, z, _editor.Configuration.Editor_ProbeAttributesThroughPortals, _ignoredHighlights, _usedShapes);
                    if (currentHighlights != null)
                        _roomEffect.Parameters["Color"].SetValue(currentHighlights[0].Color);

                    // Floor-specific highlights
                    if (face == (BlockFace)25 || face == (BlockFace)26)
                    {
                        if (room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                        {
                            if ((room.Blocks[x, z].FloorDiagonalSplit > DiagonalSplit.XpZp && face == BlockFace.Floor) ||
                                (room.Blocks[x, z].FloorDiagonalSplit <= DiagonalSplit.XpZp && face == BlockFace.FloorTriangle2))
                                _roomEffect.Parameters["Dim"].SetValue(true);
                        }

                        if (room.Blocks[x, z].FloorPortal != null)
                            _roomEffect.Parameters["Color"].SetValue(HighlightState.ColorPortalFace);
                    }


                    // Ceiling-specific highlights
                    if (face == (BlockFace)27 || face == (BlockFace)28)
                    {
                        if (room.Blocks[x, z].CeilingDiagonalSplit != DiagonalSplit.None)
                        {
                            if ((room.Blocks[x, z].CeilingDiagonalSplit > DiagonalSplit.XpZp && face == BlockFace.Ceiling) ||
                                (room.Blocks[x, z].CeilingDiagonalSplit <= DiagonalSplit.XpZp && face == BlockFace.CeilingTriangle2))
                                _roomEffect.Parameters["Dim"].SetValue(true);
                        }

                        if (room.Blocks[x, z].CeilingPortal != null)
                            _roomEffect.Parameters["Color"].SetValue(HighlightState.ColorPortalFace);
                    }
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

                    //Highlight selection, if current tool is dragging
                    _roomEffect.Parameters["Highlight"].SetValue(_toolHandler.Dragged);

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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up_down"]);
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]);
                                    break;
                                case EditorArrowType.CornerSW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]);
                                    break;
                            }
                        }

                        // East faces ------------------------------------------------------------------------------
                        if (face == BlockFace.NegativeX_QA || face == BlockFace.NegativeX_ED)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    break;
                            }
                        }

                        if (face == BlockFace.NegativeX_WS || face == BlockFace.NegativeX_RF)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    break;
                            }
                        }

                        if (face == BlockFace.NegativeX_Middle)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]);
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up_down"]);
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]);
                                    break;
                            }
                        }

                        // North faces ------------------------------------------------------------------------------
                        if (face == BlockFace.PositiveZ_QA || face == BlockFace.PositiveZ_ED)
                        {
                            switch (_editor.SelectedSectors.Arrow)
                            {
                                case EditorArrowType.EdgeN:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up_down"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["cross"]);
                                    _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                    _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                                    break;
                                case EditorArrowType.EdgeW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]);
                                    break;
                                case EditorArrowType.CornerNW:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]);
                                    break;
                                case EditorArrowType.CornerNE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne"]);
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_down"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_se"]);
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]);
                                    break;
                                case EditorArrowType.EdgeE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_up_down"]);
                                    break;
                                case EditorArrowType.EdgeS:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]);
                                    break;
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
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_ne_se"]);
                                    break;
                                case EditorArrowType.CornerSE:
                                    _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["arrow_nw_sw"]);
                                    break;
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
                else
                {
                    if (face == (BlockFace)25 || face == (BlockFace)26)
                    {
                        if (DrawIllegalSlopes && room.IsIllegalSlope(x, z))
                        {
                            _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["illegal_slope"]);
                            _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                            _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                        }
                        else if (DrawSlideDirections)
                        {
                            var slopeDirection = room.Blocks[x, z].GetFloorTriangleSlopeDirections()[(face == (BlockFace)25 ? 0 : 1)];

                            if (slopeDirection != Direction.None)
                            {
                                string flipSplit = (room.Blocks[x, z].FloorSplitDirectionIsXEqualsZ ? "_flip" : "");

                                switch (slopeDirection)
                                {
                                    case Direction.PositiveX:
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["slide_east" + flipSplit]);
                                        break;
                                    case Direction.NegativeX:
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["slide_west" + flipSplit]);
                                        break;
                                    case Direction.PositiveZ:
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["slide_north" + flipSplit]);
                                        break;
                                    case Direction.NegativeZ:
                                        _roomEffect.Parameters["Texture"].SetResource(_deviceManager.Textures["slide_south" + flipSplit]);
                                        break;
                                }
                                _roomEffect.Parameters["TextureEnabled"].SetValue(true);
                                _roomEffect.Parameters["Color"].SetValue(new Vector4(1.0f));
                            }
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

        private void AddObjectHeightLine(Matrix4x4 viewProjection, Room room, Vector3 position)
        {
            float floorHeight = GetFloorHeight(room, position);

            // Get the distance between point and floor in units
            float height = position.Y - floorHeight;

            // Prepare two vertices for the line
            var vertices = new SolidVertex[]
            {
                new SolidVertex { Position = position, Color = Vector4.One },
                new SolidVertex { Position = new Vector3(position.X, floorHeight, position.Z), Color = Vector4.One }
            };

            // Prepare the Vertex Buffer
            if (_objectHeightLineVertexBuffer != null)
                _objectHeightLineVertexBuffer.Dispose();
            _objectHeightLineVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<SolidVertex>(_device,
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
            _flybyPathVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<SolidVertex>(_device, vertices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _drawFlybyPath = true;
        }
    }
}
