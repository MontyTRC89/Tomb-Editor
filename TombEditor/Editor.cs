using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System.Windows.Forms;
using System.IO;
using TombLib.Graphics;
using TombEditor.Geometry;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using TombEditor.Controls;
using SharpDX.Direct3D;

namespace TombEditor
{
    public enum EditorMode
    {
        None, Geometry, Map2D, FaceEdit, Lighting
    }

    public enum EditorAction
    {
        None, PlaceItem, PlaceLight, PlaceCamera, PlaceFlyByCamera, PlaceSound, PlaceSink, PlaceNoCollision
    }

    public enum EditorSubAction
    {
        None, GeometryBeginSelection, GeometrySelecting, GeometrySelected
    }

    public enum EditorItemType
    {
        Moveable, StaticMesh
    }

    public class Editor
    {
        // istanza dell'editor
        private static Editor _instance;

        public Dictionary<string, Texture2D> Textures;
        public Dictionary<string, Effect> Effects;
        public Level Level { get; set; }
        public LightType LightType { get; set; }
        public bool PlaceLight { get; set; }
        public Control RenderControl { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public SpriteFont Font { get; set; }
        public SpriteBatch DebugSprites { get; set; }
        public EditorAction Action { get; set; }
        public EditorMode Mode { get; set; }
        public EditorSubAction SubAction { get; set; }
        public EditorItemType ItemType { get; set; }
        public int NewGeometryType { get; set; }
        public PickingResult StartPickingResult { get; set; }
        public PickingResult PickingResult { get; set; }
        public int BlockSelectionStartX { get; set; }
        public int BlockSelectionStartZ { get; set; }
        public int BlockSelectionEndX { get; set; }
        public int BlockSelectionEndZ { get; set; }
        public int BlockEditingType { get; set; }
        public Dictionary<int, string> MoveablesObjectIds { get; set; }
        public Dictionary<int, string> StaticMeshesObjectIds { get; set; }
        public int SelectedItem { get; set; }
        public short RoomIndex { get; set; } = -1;
        public bool IsFlipMap { get; set; }
        public System.Drawing.Color FloorColor { get; set; }
        public System.Drawing.Color WallColor { get; set; }
        public System.Drawing.Color TriggerColor { get; set; } = System.Drawing.Color.FromArgb(255, 200, 0, 200);
        public System.Drawing.Color MonkeyColor { get; set; }
        public System.Drawing.Color BoxColor { get; set; }
        public System.Drawing.Color DeathColor { get; set; }
        public System.Drawing.Color ClimbColor { get; set; }
        public System.Drawing.Color NoCollisionColor { get; set; } = System.Drawing.Color.FromArgb(255, 128, 0, 0);
        public System.Drawing.Color NotWalkableColor { get; set; }
        public int FlipMap { get; set; } = -1;
        public bool DrawPortals { get; set; }
        public int SelectedTexture { get; set; } = -1;
        public bool Stamp { get; set; }
        public Vector2[] UV { get; set; }
        public TextureTileType TextureTriangle { get; set; }
        public int LightIndex { get; set; } = -1;
        public List<System.Drawing.Color> Palette { get; set; }
        public bool NoCollision { get; set; }
        public bool InvisiblePolygon { get; set; }
        public bool DoubleSided { get; set; }
        public bool Transparent { get; set; }
        public bool DrawRoomNames { get; set; }
        public bool DrawHorizon { get; set; }
        public float FPS { get; set; }
        //public bool TriangleFaceEdit { get; set; }
        public int XSave;
        public int ZSave;
        public short SoundID { get; set; }
        public static int MaxNumberOfRooms = 512;

        private Panel2DGrid _panelGrid;
        private PanelRendering3D _panel3D;
        private FormMain _formEditor;

        // le griglie XYZ e la griglia free
        private Buffer<VertexPositionColor>[] _grids;
        private VertexInputLayout _gridLayout;

        private Editor()
        {
            _instance = this;
        }

        public void Initialize(PanelRendering3D renderControl, Panel2DGrid grid, FormMain formEditor)
        {
            Palette = new List<System.Drawing.Color>();
            try
            {
                BinaryReader readerPalette = new BinaryReader(File.OpenRead("Editor\\Palette.bin"));
                while (readerPalette.BaseStream.Position < readerPalette.BaseStream.Length)
                {
                    Palette.Add(System.Drawing.Color.FromArgb(255, readerPalette.ReadByte(), readerPalette.ReadByte(), readerPalette.ReadByte()));
                }

                readerPalette.Close();
            }
            catch (Exception)
            { }

            _panel3D = renderControl;
            _panelGrid = grid;
            _formEditor = formEditor;

            GraphicsDevice = GraphicsDevice.New(DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.None,
                                                FeatureLevel.Level_10_0);

            PresentationParameters pp = new PresentationParameters();
            pp.BackBufferFormat = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            pp.BackBufferWidth = _panel3D.Width;
            pp.BackBufferHeight = _panel3D.Height;
            pp.DepthStencilFormat = DepthFormat.Depth24Stencil8;
            pp.DeviceWindowHandle = _panel3D;
            pp.IsFullScreen = false;
            pp.MultiSampleCount = MSAALevel.None;
            pp.PresentationInterval = PresentInterval.Immediate;
            pp.RenderTargetUsage = SharpDX.DXGI.Usage.RenderTargetOutput | SharpDX.DXGI.Usage.BackBuffer;
            pp.Flags = SharpDX.DXGI.SwapChainFlags.None;

            SwapChainGraphicsPresenter presenter = new SwapChainGraphicsPresenter(GraphicsDevice, pp);
            GraphicsDevice.Presenter = presenter;

            // inizializzo le griglie
            _grids = new Buffer<VertexPositionColor>[4];

            // compilo gli effetti
            Effects = new Dictionary<string, Effect>();

            IEnumerable<string> effectFiles = Directory.EnumerateFiles(EditorPath + "\\Editor", "*.fx");
            foreach (string fileName in effectFiles)
            {
                string effectName = Path.GetFileNameWithoutExtension(fileName);
                Effects.Add(effectName, LoadEffect(fileName));
            }

            // aggiungo il BasicEffect
            BasicEffect bEffect = new BasicEffect(GraphicsDevice);
            Effects.Add("Toolkit.BasicEffect", bEffect);

            // carico le texture dell'editor
            Textures = new Dictionary<string, Texture2D>();

            IEnumerable<string> textureFiles = Directory.EnumerateFiles(EditorPath + "\\Editor", "*.png");
            foreach (string fileName in textureFiles)
            {
                string textureName = Path.GetFileNameWithoutExtension(fileName);
                Textures.Add(textureName, TombLib.Graphics.TextureLoad.FromStream(GraphicsDevice, new FileStream(fileName, FileMode.Open, FileAccess.Read)));
            }

            // carico il Editor
            SpriteFontData fontData = SpriteFontData.Load("Editor\\Font.bin");
            Font = SpriteFont.New(GraphicsDevice, fontData);

            DebugSprites = new SpriteBatch(GraphicsDevice);

            BlockSelectionStartX = -1;
            BlockSelectionStartZ = -1;
            BlockSelectionEndX = -1;
            BlockSelectionEndX = -1;

            // carico gli id degli oggetti
            StreamReader reader = new StreamReader("Editor\\Objects.txt");
            MoveablesObjectIds = new Dictionary<int, string>();
            while (reader.EndOfStream == false)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(';');
                MoveablesObjectIds.Add(Int32.Parse(tokens[0]), tokens[1]);
            }

            StaticMeshesObjectIds = new Dictionary<int, string>();
            for (int i = 0; i < 100; i++)
            {
                StaticMeshesObjectIds.Add(i, "Static Mesh #" + i);
            }

            RoomIndex = -1;
            SelectedItem = -1;
            SelectedTexture = -1;

            UV = new Vector2[4];

            PickingResultEmpty = new PickingResult();
            PickingResultEmpty.Element = (int)PickingElementType.None;

            Debug.Initialize();


        }


        public void ChangeLightColorFromPalette()
        {
            _formEditor.ChangeLightColorFromPalette();
        }

        public void ResetPanel3DCursor()
        {
            _formEditor.ResetPanel3DCursor();
        }

        private Effect LoadEffect(string fileName)
        {
            EffectCompilerResult result = EffectCompiler.CompileFromFile(fileName);

            if (result.HasErrors)
            {
                string errors = "";

                foreach (SharpDX.Toolkit.Diagnostics.LogMessage err in result.Logger.Messages)
                    errors += err + Environment.NewLine;

                MessageBox.Show("Could not compile effect '" + fileName + "'" + Environment.NewLine + errors, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            Effect effect = new SharpDX.Toolkit.Graphics.Effect(GraphicsDevice, result.EffectData);
            return effect;
        }

        public void LoadTriggersInUI()
        {
            _formEditor.LoadTriggersInUI();
        }

        public static string EditorPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            }
        }

        public static Editor Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                else
                    return new Editor();
            }
        }

        public void PrepareXGrid(int rows, int columns, float pass)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            for (int x = -rows / 2; x < rows / 2; x++)
            {
                VertexPositionColor vertex = new VertexPositionColor(new Vector3(x * pass, 0, -columns / 2 * pass), Color.White);
                vertices.Add(vertex);
                vertex = new VertexPositionColor(new Vector3(x * pass, 0, columns / 2 * pass), Color.White);
                vertices.Add(vertex);
            }

            for (int z = -columns / 2; z < columns / 2; z++)
            {
                VertexPositionColor vertex = new VertexPositionColor(new Vector3(-rows / 2 * pass, 0, z * pass), Color.White);
                vertices.Add(vertex);
                vertex = new VertexPositionColor(new Vector3(rows / 2 * pass, 0, z * pass), Color.White);
                vertices.Add(vertex);
            }

            // creo il vertex buffer
            if (_grids[0] == null)
                _grids[0] = Buffer<VertexPositionColor>.New<VertexPositionColor>(GraphicsDevice, vertices.ToArray(), BufferFlags.VertexBuffer);

            // creo il vertex input layout
            if (_gridLayout == null)
                _gridLayout = VertexInputLayout.FromBuffer<VertexPositionColor>(0, _grids[0]);
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
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

        public void ResetCamera()
        {
            _panel3D.ResetCamera();
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

        public string UpdateStatistics()
        {
            if (RoomIndex == -1 || Level.Rooms[RoomIndex] == null)
                return "";

            string stats = "Room X: " + Level.Rooms[RoomIndex].Position.X.ToString();
            stats += " Y floor: " + (Level.Rooms[RoomIndex].Position.Y + Level.Rooms[RoomIndex].GetLowestCorner());
            stats += " Y ceiling: " + (Level.Rooms[RoomIndex].Position.Y + Level.Rooms[RoomIndex].GetHighestCorner());
            stats += " Z: " + Level.Rooms[RoomIndex].Position.Z.ToString();
            stats += "    Size: " + (Level.Rooms[RoomIndex].NumXSectors - 2).ToString();
            stats += "x";
            stats += (Level.Rooms[RoomIndex].NumZSectors - 2).ToString();
            stats += "    ";
            stats += "Portals: " + Level.Portals.Count;
            stats += "    ";
            stats += "Triggers: " + Level.Triggers.Count;

            _formEditor.UpdateStatistics();

            return stats;
        }

        public static PickingResult PickingResultEmpty;

        public void ResetSelection()
        {
            _formEditor.ResetSelection();
        }

        public void SelectRoom(int index)
        {
            LightIndex = -1;
            BlockSelectionStartX = -1;
            BlockSelectionStartZ = -1;
            BlockSelectionEndX = -1;
            BlockSelectionEndX = -1;
            LoadTriggersInUI();

            _formEditor.SelectRoom(index);
        }

        public void UpdateRoomName()
        {
            if (RoomIndex == -1)
                return;
            //_formEditor.UpdateLabelRoom(Level.Rooms[RoomIndex].Name);
        }

        public void DrawPanel2DMessage(string message)
        {
            // _formEditor.DrawPanel2DMessage(message);
        }

        public void ResetPanel2DMessage()
        {
            // _formEditor.ResetPanel2DMessage();
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
    }
}
