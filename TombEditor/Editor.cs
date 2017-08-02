using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using TombLib.Graphics;
using TombEditor.Geometry;
using TombEditor.Controls;

namespace TombEditor
{
    public enum EditorMode
    {
        Geometry, Map2D, FaceEdit, Lighting
    }

    public enum EditorAction
    {
        None, PlaceItem, PlaceLight, PlaceCamera, PlaceFlyByCamera, PlaceSound, PlaceSink, PlaceNoCollision, Paste, Stamp
    }

    public enum EditorArrowType
    {
        EntireFace,
        EdgeN,
        EdgeE,
        EdgeS,
        EdgeW,
        CornerNW,
        CornerNE,
        CornerSE,
        CornerSW,
        DiagonalFloorCorner,
        DiagonalCeilingCorner
    }

    public class Editor
    {
        public static readonly System.Drawing.Color ColorFloor = System.Drawing.Color.FromArgb(0, 190, 190);
        public static readonly System.Drawing.Color ColorWall = System.Drawing.Color.FromArgb(0, 160, 0);
        public static readonly System.Drawing.Color ColorTrigger = System.Drawing.Color.FromArgb(200, 0, 200);
        public static readonly System.Drawing.Color ColorMonkey = System.Drawing.Color.FromArgb(255, 100, 100);
        public static readonly System.Drawing.Color ColorBox = System.Drawing.Color.FromArgb(100, 100, 100);
        public static readonly System.Drawing.Color ColorDeath = System.Drawing.Color.FromArgb(20, 240, 20);
        public static readonly System.Drawing.Color ColorClimb = System.Drawing.Color.FromArgb(0, 100, 0);
        public static readonly System.Drawing.Color ColorNoCollision = System.Drawing.Color.FromArgb(255, 128, 0, 0);
        public static readonly System.Drawing.Color ColorNotWalkable = System.Drawing.Color.FromArgb(0, 0, 150);

        // istanza dell'editor
        public Level Level { get; set; }
        public EditorAction Action { get; set; } = EditorAction.None;
        public int ActionPlaceSound_SoundID { get; set; }
        public LightType ActionPlaceLight_LightType { get; set; }
        public int ActionPlaceItem_ItemID { get; set; }
        public bool ActionPlaceItem_IsStatic { get; set; }
        public EditorMode Mode { get; set; } = EditorMode.Geometry;
        public Room SelectedRoom { get; set; }
        public ObjectPtr? SelectedObject { get; set; } = null;
        public System.Drawing.Point BlockSelectionStart { get; set; } = new System.Drawing.Point(-1, -1);
        public System.Drawing.Point BlockSelectionEnd { get; set; } = new System.Drawing.Point(-1, -1);
        public EditorArrowType BlockSelectionArrow { get; set; } = EditorArrowType.EntireFace;
        public bool RelocateCameraActive { get; set; }
        public bool InvisiblePolygon { get; set; }
        public bool DoubleSided { get; set; }
        public bool Transparent { get; set; }
        public int SelectedTextureIndex { get; set; } = -1;
        public Vector2[] SeletedTextureUV { get; } = new Vector2[4];
        public TextureTileType SeletedTextureTriangle { get; set; }
        public Configuration Configuration { get; set; } = TombEditor.Configuration.LoadFrom(Configuration.GetDefaultPath());

        private Panel2DGrid _panelGrid;
        private PanelRendering3D _panel3D;
        private FormMain _formEditor;

        // To be removed
        private DeviceManager _deviceManager;

        public void Initialize(PanelRendering3D renderControl, Panel2DGrid grid, FormMain formEditor, DeviceManager deviceManager)
        {
            _panel3D = renderControl;
            _panelGrid = grid;
            _formEditor = formEditor;
            _deviceManager = deviceManager;
        }
        
        public void MoveCameraToSector(DrawingPoint pos)
        {
            _panel3D.MoveCameraToSector(pos);
        }
        public void LoadTriggersInUI()
        {
            _formEditor.LoadTriggersInUI();
        }

        private static Editor _instance;

        public static Editor Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                else
                    return _instance = new Editor();
            }
        }

        public void LoadStaticMeshColorInUI()
        {
            _formEditor.LoadStaticMeshColorInUI();
        }

        public void DrawPanelGrid()
        {
            _panelGrid.Invalidate();
        }

        public void DrawPanel3D()
        {
            _panel3D.Draw();
        }

        public void DrawPanelMap2D()
        {
            _formEditor.DrawPanelMap2D();
        }

        public void ResetCamera()
        {
            _panel3D.ResetCamera();
        }
        
        // To be removed
        public SharpDX.Toolkit.Graphics.GraphicsDevice GetDevice()
        {
            return _deviceManager.Device;
        }

        public Camera Camera
        {
            get
            {
                return _panel3D.Camera;
            }
        }

        public void LoadTextureMapInEditor(Level level)
        {
            _formEditor.LoadTextureMapInEditor(level);
        }

        public void UpdateStatusStrip()
        {
            _formEditor.UpdateStatusStrip();
        }
        
        public void ResetSelection()
        {
            _formEditor.ResetSelection();
        }

        public void SelectRoom(Room room)
        {
            BlockSelectionReset();
            LoadTriggersInUI();
    
            _formEditor.SelectRoom(room);
        }
        
        public void UpdateRoomName()
        {
            if (SelectedRoom == null)
                return;
            //_formEditor.UpdateLabelRoom(Level.Rooms[SelectedRoom].Name);
        }
        
        public void CenterCamera()
        {
            _formEditor.CenterCamera();
        }

        public void LoadWadInInterface()
        {
            _formEditor.LoadWadInInterface();
        }

        public void EditLight()
        {
            _formEditor.EditLight();
        }

        // The rectangle is (-1, -1, -1, 1) when nothing is selected.
        // The "Right" and "Bottom" point of the rectangle is inclusive.
        public Rectangle BlockSelection
        {
            get
            {
                return new SharpDX.Rectangle(
                    Math.Min(BlockSelectionStart.X, BlockSelectionEnd.X), Math.Min(BlockSelectionStart.Y, BlockSelectionEnd.Y),
                    Math.Max(BlockSelectionStart.X, BlockSelectionEnd.X), Math.Max(BlockSelectionStart.Y, BlockSelectionEnd.Y));
            }
            set
            {
                BlockSelectionStart = new System.Drawing.Point(value.X, value.Y);
                BlockSelectionEnd = new System.Drawing.Point(value.Right, value.Bottom);
            }
        }

        public void BlockSelectionReset()
        {
            BlockSelectionStart = new System.Drawing.Point(-1, -1);
            BlockSelectionEnd = new System.Drawing.Point(-1, -1);
        }

        public bool BlockSelectionAvailable
        {
           get { return (BlockSelectionStart.X != -1) && (BlockSelectionStart.Y != -1) && (BlockSelectionEnd.X != -1) && (BlockSelectionEnd.Y != -1); }
        }

        public int LightIndex
        {
            get
            {
                if (SelectedObject == null)
                    return -1;
                if (SelectedObject.Value.Type != ObjectInstanceType.Light)
                    return -1;
                return SelectedObject.Value.Index;
            }
        }
    }
}
