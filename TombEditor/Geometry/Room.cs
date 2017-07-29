using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombEditor.Geometry
{
    public class Room
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public short Ceiling { get; set; }
        public byte NumXSectors { get; set; }
        public byte NumZSectors { get; set; }
        public System.Drawing.Color AmbientLight { get; set; } = System.Drawing.Color.FromArgb(255, 32, 32, 32);
        public Block[,] Blocks { get; set; }
        public Buffer<EditorVertex> VertexBuffer { get; set; }
        public Buffer<EditorVertex> NormalsBuffer { get; set; }
        public List<EditorVertex> Vertices { get; set; }
        public List<int> Moveables { get; set; } = new List<int>();
        public List<int> StaticMeshes { get; set; } = new List<int>();
        public List<Light> Lights { get; set; } = new List<Light>();
        public List<int> SoundSources { get; set; } = new List<int>();
        public List<int> Sinks { get; set; } = new List<int>();
        public List<int> Cameras { get; set; } = new List<int>();
        public List<int> FlyByCameras { get; set; } = new List<int>();
        public List<int> Portals { get; set; } = new List<int>();
        public short BaseRoom { get; set; } = -1;
        public bool Flipped { get; set; }
        public bool Visited { get; set; }
        public List<EditorVertex> OptimizedVertices { get; set; }
        public short AlternateRoom { get; set; } = -1;
        public short AlternateGroup { get; set; } = 0;
        public short WaterLevel { get; set; }
        public short MistLevel { get; set; }
        public short ReflectionLevel { get; set; }
        public bool FlagWater { get; set; }
        public bool FlagReflection { get; set; }
        public bool FlagMist { get; set; }
        public bool FlagSnow { get; set; }
        public bool FlagRain { get; set; }
        public bool FlagCold { get; set; }
        public bool FlagDamage { get; set; }
        public bool FlagQuickSand { get; set; }
        public bool FlagOutside { get; set; }
        public bool FlagHorizon { get; set; }
        public Reverberation Reverberation { get; set; }
        public Level Level { get; set; }
        public Vector3 Centre { get; set; }
        public bool ExcludeFromPathFinding { get; set; }
        private Editor _editor;
        private EditorVertex[,,] _verticesGrid;
        private byte[,] _numVerticesInGrid;
        private byte[,,] _iterations;
                
        public Room(Level level)
        {
            Level = level;
            _editor = Editor.Instance;

            InitializeVerticesGrid();
        }

        public Room(Level level, int posx, int posy, int posz, byte numXSectors, byte numZSectors, short ceiling)
        {
            Position = new Vector3(posx, posy, posz);
            NumXSectors = numXSectors;
            NumZSectors = numZSectors;
            AmbientLight = System.Drawing.Color.FromArgb(255, 32, 32, 32);
            Moveables = new List<int>();
            StaticMeshes = new List<int>();
            Ceiling = ceiling;
            Lights = new List<Light>();
            FlyByCameras = new List<int>();
            Cameras = new List<int>();
            Sinks = new List<int>();
            SoundSources = new List<int>();
            Portals = new List<int>();
            Level = level;
            AlternateRoom = -1;
            BaseRoom = -1;

            Blocks = new Block[numXSectors, numZSectors];

            for (int x = 0; x < NumXSectors; x++)
            {
                for (int z = 0; z < NumZSectors; z++)
                {
                    Block block;
                    if (x == 0 || z == 0 || x == NumXSectors - 1 || z == NumZSectors - 1)
                    {
                        block = new Block(BlockType.BorderWall, BlockFlags.None, Ceiling);
                    }
                    else
                    {
                        block = new Block(BlockType.Floor, BlockFlags.None, Ceiling);
                    }

                    Blocks[x, z] = block;
                }
            }

            _editor = Editor.Instance;

            InitializeVerticesGrid();

            BuildGeometry();
            CalculateLightingForThisRoom();
            UpdateBuffers();
        }

        public EditorVertex[,,] VerticesGrid
        {
            get
            {
                return _verticesGrid;
            }
        }

        public byte[,] NumVerticesInGrid
        {
            get
            {
                return _numVerticesInGrid;
            }
        }
    
        public Vector2 SectorPos
        {
            get
            {
                return new Vector2(Position.X, Position.Z);
            }
            set
            {
                Position = new Vector3(value.X, Position.Y, value.Y);
            }
        }

        public void InitializeVerticesGrid()
        {
            _verticesGrid = new EditorVertex[NumXSectors, NumZSectors, 16];
            _numVerticesInGrid = new byte[NumXSectors, NumZSectors];
        }

        public void CalculateLightingForThisRoom()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // This is used for iterative mean
            _iterations = new byte[NumXSectors, NumZSectors, 16];

            for (int x = 0; x < NumXSectors; x++)
            {
                for (int z = 0; z < NumZSectors; z++)
                {
                    for (int f = 0; f < 29; f++)
                    {
                        if (Blocks[x, z].Faces[f].Defined)
                        {
                            // Calculate the lighting of vertices in X, Z of face f
                            CalculateLighting(x, z, f);
                        }
                    }
                }
            }

            // Apply the face color to editor vertices
            for (int i = 0; i < Vertices.Count; i++)
            {
                int x = (int)(Vertices[i].Position.X / 1024);
                int z = (int)(Vertices[i].Position.Z / 1024);

                for (int j = 0; j < _numVerticesInGrid[x, z]; j++)
                {
                    if (_verticesGrid[x, z, j].Position.Y == Vertices[i].Position.Y)
                    {
                        EditorVertex v = Vertices[i];
                        v.FaceColor = _verticesGrid[x, z, j].FaceColor;
                        Vertices[i] = v;

                        break;
                    }
                }
            }

            watch.Stop();
            long stop = watch.ElapsedMilliseconds;
        }

        private void CalculateCeilingSlope(int x, int z, int ws0, int ws1, int ws2, int ws3)
        {
            if (!(ws0 == ws1 && ws1 == ws2 && ws2 == ws3))
            {
                // Calculate the slope
                int topHeight = GetHighestCeilingCorner(x, z);
                int lowHeight = GetLowestCeilingCorner(x, z);

                if (ws0 == lowHeight && ws1 == lowHeight)
                {
                    Blocks[x, z].CeilingSlopeZ = (short)(topHeight - lowHeight);
                }

                if (ws1 == lowHeight && ws2 == lowHeight)
                {
                    Blocks[x, z].CeilingSlopeX = (short)-(topHeight - lowHeight);
                }

                if (ws2 == lowHeight && ws3 == lowHeight)
                {
                    Blocks[x, z].CeilingSlopeZ = (short)-(topHeight - lowHeight);
                }

                if (ws3 == lowHeight && ws0 == lowHeight)
                {
                    Blocks[x, z].CeilingSlopeX = (short)(topHeight - lowHeight);
                }

                if (ws0 == lowHeight && ws0 < ws1 && ws0 < ws3)
                {
                    Blocks[x, z].CeilingSlopeX = (short)((topHeight - lowHeight));
                    Blocks[x, z].CeilingSlopeZ = (short)-((topHeight - lowHeight));
                }

                if (ws1 == lowHeight && ws1 < ws2 && ws1 < ws0)
                {
                    Blocks[x, z].CeilingSlopeX = (short)-((topHeight - lowHeight));
                    Blocks[x, z].CeilingSlopeZ = (short)-((topHeight - lowHeight));
                }

                if (ws2 == lowHeight && ws2 < ws1 && ws2 < ws3)
                {
                    Blocks[x, z].CeilingSlopeX = (short)-((topHeight - lowHeight));
                    Blocks[x, z].CeilingSlopeZ = (short)((topHeight - lowHeight));
                }

                if (ws3 == lowHeight && ws3 < ws2 && ws3 < ws0)
                {
                    Blocks[x, z].CeilingSlopeX = (short)((topHeight - lowHeight));
                    Blocks[x, z].CeilingSlopeZ = (short)((topHeight - lowHeight));
                }
            }
        }

        private void CalculateFloorSlope(int x, int z, int qa0, int qa1, int qa2, int qa3)
        {
            if (!(qa0 == qa1 && qa1 == qa2 && qa2 == qa3))
            {
                // Calculate the slope
                int topHeight = GetHighestFloorCorner(x, z);
                int lowHeight = GetLowestFloorCorner(x, z);

                if (qa0 == topHeight && qa1 == topHeight)
                {
                    Blocks[x, z].FloorSlopeZ = (short)(topHeight - lowHeight);
                }

                if (qa1 == topHeight && qa2 == topHeight)
                {
                    Blocks[x, z].FloorSlopeX = (short)(topHeight - lowHeight);
                }

                if (qa2 == topHeight && qa3 == topHeight)
                {
                    Blocks[x, z].FloorSlopeZ = (short)-(topHeight - lowHeight);
                }

                if (qa3 == topHeight && qa0 == topHeight)
                {
                    Blocks[x, z].FloorSlopeX = (short)-(topHeight - lowHeight);
                }

                if (qa0 == topHeight && qa0 > qa1 && qa0 > qa3)
                {
                    Blocks[x, z].FloorSlopeX = (short)-((topHeight - qa1));
                    Blocks[x, z].FloorSlopeZ = (short)((topHeight - qa3));
                }

                if (qa1 == topHeight && qa1 > qa2 && qa1 > qa0)
                {
                    Blocks[x, z].FloorSlopeX = (short)((topHeight - qa0));
                    Blocks[x, z].FloorSlopeZ = (short)((topHeight - qa2));
                }

                if (qa2 == topHeight && qa2 > qa1 && qa2 > qa3)
                {
                    Blocks[x, z].FloorSlopeX = (short)((topHeight - qa3));
                    Blocks[x, z].FloorSlopeZ = (short)-((topHeight - qa1));
                }

                if (qa3 == topHeight && qa3 > qa2 && qa3 > qa0)
                {
                    Blocks[x, z].FloorSlopeX = (short)-((topHeight - qa2));
                    Blocks[x, z].FloorSlopeZ = (short)-((topHeight - qa0));
                }
            }
        }

        private short CheckIfVertexExists(Vector3 v)
        {
            for (short i = 0; i < OptimizedVertices.Count; i++)
            {
                if (OptimizedVertices[i].Position.X == v.X && OptimizedVertices[i].Position.Y == v.Y && OptimizedVertices[i].Position.Z == v.Z)
                    return i;
            }

            return -1;
        }

        public void BuildGeometry()
        {
            BuildGeometry(0, NumXSectors - 1, 0, NumZSectors - 1);
        }

        public void BuildGeometry(int xMin, int xMax, int zMin, int zMax)
        {
            Vertices = new List<EditorVertex>();
            OptimizedVertices = new List<EditorVertex>();
            Centre = new Vector3(0.0f, 0.0f, 0.0f);

            Vector2 e1 = new Vector2(0.0f, 0.0f);
            Vector2 e2 = new Vector2(1.0f, 0.0f);
            Vector2 e3 = new Vector2(1.0f, 1.0f);
            Vector2 e4 = new Vector2(0.0f, 1.0f);

            // Adjust ranges
            xMin = Math.Max(0, xMin);
            xMax = Math.Min(NumXSectors - 1, xMax);
            zMin = Math.Max(0, zMin);
            zMax = Math.Min(NumZSectors - 1, zMax);

            // Reset faces
            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    for (int f = 0; f < 29; f++)
                    {
                        Blocks[x, z].Faces[f].Defined = false;
                        if (Blocks[x, z].FloorPortal != -1)
                            Level.Portals[Blocks[x, z].FloorPortal].Vertices.Clear();
                        if (Blocks[x, z].CeilingPortal != -1)
                            Level.Portals[Blocks[x, z].CeilingPortal].Vertices.Clear();
                        if (Blocks[x, z].WallPortal != -1)
                            Level.Portals[Blocks[x, z].WallPortal].Vertices.Clear();
                    }

                    _numVerticesInGrid[x, z] = 0;
                }
            }

            // Build face polygons
            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    // Save the height of the faces
                    int qa0 = Blocks[x, z].QAFaces[0];
                    int qa1 = Blocks[x, z].QAFaces[1];
                    int qa2 = Blocks[x, z].QAFaces[2];
                    int qa3 = Blocks[x, z].QAFaces[3];
                    int ws0 = Blocks[x, z].WSFaces[0];
                    int ws1 = Blocks[x, z].WSFaces[1];
                    int ws2 = Blocks[x, z].WSFaces[2];
                    int ws3 = Blocks[x, z].WSFaces[3];
                    int ed0 = Blocks[x, z].EDFaces[0];
                    int ed1 = Blocks[x, z].EDFaces[1];
                    int ed2 = Blocks[x, z].EDFaces[2];
                    int ed3 = Blocks[x, z].EDFaces[3];
                    int rf0 = Blocks[x, z].RFFaces[0];
                    int rf1 = Blocks[x, z].RFFaces[1];
                    int rf2 = Blocks[x, z].RFFaces[2];
                    int rf3 = Blocks[x, z].RFFaces[3];

                    // Save portals
                    Portal floorPortal = FindPortal(x, z, PortalDirection.Floor);
                    Portal ceilingPortal = FindPortal(x, z, PortalDirection.Ceiling);
                    Portal northPortal = FindPortal(x, z, PortalDirection.North);
                    Portal southPortal = FindPortal(x, z, PortalDirection.South);
                    Portal eastPortal = FindPortal(x, z, PortalDirection.West);
                    Portal westPortal = FindPortal(x, z, PortalDirection.East);

                    // If x, z is one of the four corner then nothing has to be done
                    if ((x == 0 && z == 0) || (x == 0 && z == NumZSectors - 1) ||
                        (x == NumXSectors - 1 && z == NumZSectors - 1) || (x == NumXSectors - 1 && z == 0))
                        continue;

                    // Vertical polygons  ---------------------------------------------------------------------------------

                    // North
                    if (x > 0 && x < NumXSectors - 1 && z > 0 && z < NumZSectors - 2 &&
                        !(Blocks[x, z + 1].Type == BlockType.Wall &&
                         (Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.NW || Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.NE)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal != -1 &&
                            Blocks[x, z].WallOpacity != PortalOpacity.None)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NW || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NE))
                            AddVerticalFaces(x, z, FaceDirection.North, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.North, true, true, false);
                    }


                    // South
                    if (x > 0 && x < NumXSectors - 1 && z > 1 && z < NumZSectors - 1 &&
                        !(Blocks[x, z - 1].Type == BlockType.Wall &&
                         (Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.SW || Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.SE)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall ||
                            (Blocks[x, z].WallPortal != -1 &&
                            Blocks[x, z].WallOpacity != PortalOpacity.None)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SW || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SE))
                            AddVerticalFaces(x, z, FaceDirection.South, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.South, true, true, false);
                    }

                    // East
                    if (z > 0 && z < NumZSectors - 1 && x > 0 && x < NumXSectors - 2 &&
                        !(Blocks[x + 1, z].Type == BlockType.Wall &&
                        (Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.NE || Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.SE)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal != -1 &&
                             Blocks[x, z].WallOpacity != PortalOpacity.None)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NE || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SE))
                            AddVerticalFaces(x, z, FaceDirection.East, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.East, true, true, false);
                    }

                    // West
                    if (z > 0 && z < NumZSectors - 1 && x > 1 && x < NumXSectors - 1 &&
                        !(Blocks[x - 1, z].Type == BlockType.Wall &&
                        (Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.NW || Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.SW)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal != -1 &&
                             Blocks[x, z].WallOpacity != PortalOpacity.None)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NW || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SW))
                            AddVerticalFaces(x, z, FaceDirection.West, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.West, true, true, false);
                    }

                    // Diagonal faces
                    if (Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                    {
                        if (Blocks[x, z].Type == BlockType.Wall)
                        {
                            AddVerticalFaces(x, z, FaceDirection.DiagonalFloor, true, true, true);
                        }
                        else
                        {
                            AddVerticalFaces(x, z, FaceDirection.DiagonalFloor, true, false, false);
                        }
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit != DiagonalSplit.None)
                    {
                        if (Blocks[x, z].Type != BlockType.Wall)
                        {
                            AddVerticalFaces(x, z, FaceDirection.DiagonalCeiling, false, true, false);
                        }
                    }

                    // North border wall
                    if (z == 0 && x != 0 && x != NumXSectors - 1 &&
                        !(Blocks[x, 1].Type == BlockType.Wall &&
                         (Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.NW || Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.NE)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != -1)
                        {
                            Portal portal = FindPortal(x, z, PortalDirection.South);
                            Room adjoiningRoom = Level.Rooms[portal.AdjoiningRoom];
                            if (Flipped && BaseRoom != -1)
                            {
                                if (adjoiningRoom.Flipped)
                                    adjoiningRoom = Level.Rooms[adjoiningRoom.AlternateRoom];
                            }

                            int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                            if (adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].Type == BlockType.Wall &&
                                (adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.None ||
                                 adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.SE ||
                                 adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.SW))
                            {
                                addMiddle = true;
                            }
                        }


                        if (addMiddle || (Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == -1) || (Blocks[x, z].WallPortal >= 0 && Blocks[x, z].WallOpacity != PortalOpacity.None))
                            AddVerticalFaces(x, z, FaceDirection.North, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.North, true, true, false);
                    }

                    // South border wall
                    if (z == NumZSectors - 1 && x != 0 && x != NumXSectors - 1 &&
                        !(Blocks[x, NumZSectors - 2].Type == BlockType.Wall &&
                         (Blocks[x, NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.SW || Blocks[x, NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.SE)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != -1)
                        {
                            Portal portal = FindPortal(x, z, PortalDirection.North);
                            Room adjoiningRoom = Level.Rooms[portal.AdjoiningRoom];
                            if (Flipped && BaseRoom != -1)
                            {
                                if (adjoiningRoom.Flipped)
                                    adjoiningRoom = Level.Rooms[adjoiningRoom.AlternateRoom];
                            }

                            int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                            if (adjoiningRoom.Blocks[facingX, 1].Type == BlockType.Wall &&
                                (adjoiningRoom.Blocks[facingX, 1].FloorDiagonalSplit == DiagonalSplit.None ||
                                 adjoiningRoom.Blocks[facingX, 1].FloorDiagonalSplit == DiagonalSplit.NE ||
                                 adjoiningRoom.Blocks[facingX, 1].FloorDiagonalSplit == DiagonalSplit.NW))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || (Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == -1) || (Blocks[x, z].WallPortal >= 0 && Blocks[x, z].WallOpacity != PortalOpacity.None))
                            AddVerticalFaces(x, z, FaceDirection.South, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.South, true, true, false);
                    }

                    // West border wall
                    if (x == 0 && z != 0 && z != NumZSectors - 1 &&
                        !(Blocks[1, z].Type == BlockType.Wall &&
                         (Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.NE || Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.SE)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != -1)
                        {
                            Portal portal = FindPortal(x, z, PortalDirection.West);
                            Room adjoiningRoom = Level.Rooms[portal.AdjoiningRoom];
                            if (Flipped && BaseRoom != -1)
                            {
                                if (adjoiningRoom.Flipped)
                                    adjoiningRoom = Level.Rooms[adjoiningRoom.AlternateRoom];
                            }

                            int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);

                            if (adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].Type == BlockType.Wall &&
                                (adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].FloorDiagonalSplit == DiagonalSplit.None ||
                                 adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].FloorDiagonalSplit == DiagonalSplit.NW ||
                                 adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].FloorDiagonalSplit == DiagonalSplit.SW))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || (Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == -1) || (Blocks[x, z].WallPortal >= 0 && Blocks[x, z].WallOpacity != PortalOpacity.None))
                            AddVerticalFaces(x, z, FaceDirection.East, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.East, true, true, false);
                    }

                    // East border wall
                    if (x == NumXSectors - 1 && z != 0 && z != NumZSectors - 1 &&
                        !(Blocks[NumXSectors - 2, z].Type == BlockType.Wall &&
                         (Blocks[NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.NW || Blocks[NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.SW)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != -1)
                        {
                            Portal portal = FindPortal(x, z, PortalDirection.East);
                            Room adjoiningRoom = Level.Rooms[portal.AdjoiningRoom];
                            if (Flipped && BaseRoom != -1)
                            {
                                if (adjoiningRoom.Flipped)
                                    adjoiningRoom = Level.Rooms[adjoiningRoom.AlternateRoom];
                            }

                            int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);

                            if (adjoiningRoom.Blocks[1, facingZ].Type == BlockType.Wall &&
                                (adjoiningRoom.Blocks[1, facingZ].FloorDiagonalSplit == DiagonalSplit.None ||
                                 adjoiningRoom.Blocks[1, facingZ].FloorDiagonalSplit == DiagonalSplit.NE ||
                                 adjoiningRoom.Blocks[1, facingZ].FloorDiagonalSplit == DiagonalSplit.SE))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || (Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == -1) || (Blocks[x, z].WallPortal >= 0 && Blocks[x, z].WallOpacity != PortalOpacity.None))
                            AddVerticalFaces(x, z, FaceDirection.West, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.West, true, true, false);
                    }

                    // If is a non diagonal wall, then continue
                    if (Blocks[x, z].Type == BlockType.Wall && Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.None)
                        continue;

                    //
                    // 1----2    Split 0: 231 413  
                    // | \  |    Split 1: 124 342
                    // |  \ |
                    // 4----3
                    //

                    //
                    // 1----2    Split 0: 231 413  
                    // |  / |    Split 1: 124 342
                    // | /  |
                    // 4----3
                    //

                    // Floor polygons ---------------------------------------------------------------------------------
                    BlockFace face = Blocks[x, z].Faces[(int)BlockFaces.Floor];

                    // First, I reset the slope already calculated
                    Blocks[x, z].FloorSlopeX = 0;
                    Blocks[x, z].FloorSlopeZ = 0;

                    if (IsQuad(x, z, qa0, qa1, qa2, qa3) || (Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None))
                    {
                        if (!(qa0 == qa1 && qa1 == qa2 && qa2 == qa3) && Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.None)
                        {
                            CalculateFloorSlope(x, z, qa0, qa1, qa2, qa3);
                        }

                        if ((Blocks[x, z].Type == BlockType.Floor && Blocks[x, z].FloorPortal == -1) || /*Blocks[x, z].CeilingPortal != -1 ||*/
                            (Blocks[x, z].FloorPortal != -1 && Blocks[x, z].FloorOpacity != PortalOpacity.None) ||
                            Blocks[x, z].IsFloorSolid || Blocks[x, z].Type == BlockType.Wall)
                        {
                            if (Blocks[x, z].SplitFloor == false && Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.None)
                            {
                                AddRectangle(x, z, BlockFaces.Floor, new Vector3(x * 1024.0f, qa0 * 256.0f, (z + 1) * 1024.0f),
                                                                    new Vector3((x + 1) * 1024.0f, qa1 * 256.0f, (z + 1) * 1024.0f),
                                                                    new Vector3((x + 1) * 1024.0f, qa2 * 256.0f, z * 1024.0f),
                                                                    new Vector3(x * 1024.0f, qa3 * 256.0f, z * 1024.0f),
                                                                    face.RectangleUV[0], face.RectangleUV[1], face.RectangleUV[2], face.RectangleUV[3],
                                                                    e1, e2, e3, e4);
                            }
                            else
                            {
                                int split = GetBestFloorSplit(x, z, qa0, qa1, qa2, qa3);
                                if (Blocks[x, z].Type != BlockType.Wall)
                                {
                                    if (Blocks[x, z].SplitFoorType == 1)
                                        if (split == 0)
                                            split = 1;
                                        else
                                            split = 0;
                                }

                                bool addTriangle1 = true;
                                bool addTriangle2 = true;

                                int y1 = 0;
                                int y2 = 0;
                                int y3 = 0;
                                int y4 = 0;
                                int y5 = 0;
                                int y6 = 0;

                                if (Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                {
                                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NW)
                                    {
                                        split = 1;
                                        addTriangle1 = true;
                                        addTriangle2 = (Blocks[x, z].Type != BlockType.Wall);
                                        y1 = qa0;
                                        y2 = qa0;
                                        y3 = qa0;
                                        y4 = qa2;
                                        y5 = qa3;
                                        y6 = qa1;
                                    }

                                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NE)
                                    {
                                        split = 0;
                                        addTriangle1 = true;
                                        addTriangle2 = (Blocks[x, z].Type != BlockType.Wall);
                                        y1 = qa1;
                                        y2 = qa1;
                                        y3 = qa1;
                                        y4 = qa3;
                                        y5 = qa0;
                                        y6 = qa2;
                                    }

                                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SE)
                                    {
                                        split = 1;
                                        addTriangle1 = (Blocks[x, z].Type != BlockType.Wall);
                                        addTriangle2 = true;
                                        y1 = qa0;
                                        y2 = qa1;
                                        y3 = qa3;
                                        y4 = qa2;
                                        y5 = qa2;
                                        y6 = qa2;
                                    }

                                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SW)
                                    {
                                        split = 0;
                                        addTriangle1 = (Blocks[x, z].Type != BlockType.Wall);
                                        addTriangle2 = true;
                                        y1 = qa1;
                                        y2 = qa2;
                                        y3 = qa0;
                                        y4 = qa3;
                                        y5 = qa3;
                                        y6 = qa3;
                                    }
                                }
                                else
                                {
                                    if (split == 0)
                                    {
                                        addTriangle1 = true;
                                        addTriangle2 = true;
                                        y1 = qa1;
                                        y2 = qa2;
                                        y3 = qa0;
                                        y4 = qa3;
                                        y5 = qa0;
                                        y6 = qa2;
                                    }
                                    else
                                    {
                                        addTriangle1 = true;
                                        addTriangle2 = true;
                                        y1 = qa0;
                                        y2 = qa1;
                                        y3 = qa3;
                                        y4 = qa2;
                                        y5 = qa3;
                                        y6 = qa1;
                                    }
                                }

                                if (split == 0)
                                {
                                    if (addTriangle1)
                                    {
                                        AddTriangle(x, z, BlockFaces.Floor, new Vector3((x + 1) * 1024.0f, y1 * 256.0f, (z + 1) * 1024.0f),
                                                                            new Vector3((x + 1) * 1024.0f, y2 * 256.0f, z * 1024.0f),
                                                                            new Vector3(x * 1024.0f, y3 * 256.0f, (z + 1) * 1024.0f),
                                                                            face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e2, e3, e1);
                                    }

                                    if (addTriangle2)
                                    {
                                        face = Blocks[x, z].Faces[(int)BlockFaces.FloorTriangle2];
                                        AddTriangle(x, z, BlockFaces.FloorTriangle2, new Vector3(x * 1024.0f, y4 * 256.0f, z * 1024.0f),
                                                                                     new Vector3(x * 1024.0f, y5 * 256.0f, (z + 1) * 1024.0f),
                                                                                     new Vector3((x + 1) * 1024.0f, y6 * 256.0f, z * 1024.0f),
                                                                                     face.TriangleUV2[0], face.TriangleUV2[1], face.TriangleUV2[2], e4, e1, e3, 0);
                                    }
                                }
                                else
                                {
                                    if (addTriangle1)
                                    {
                                        AddTriangle(x, z, BlockFaces.Floor, new Vector3(x * 1024.0f, y1 * 256.0f, (z + 1) * 1024.0f),
                                                                            new Vector3((x + 1) * 1024.0f, y2 * 256.0f, (z + 1) * 1024.0f),
                                                                            new Vector3(x * 1024.0f, y3 * 256.0f, z * 1024.0f),
                                                                            face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                                    }

                                    if (addTriangle2)
                                    {
                                        face = Blocks[x, z].Faces[(int)BlockFaces.FloorTriangle2];
                                        AddTriangle(x, z, BlockFaces.FloorTriangle2, new Vector3((x + 1) * 1024.0f, y4 * 256.0f, z * 1024.0f),
                                                                                     new Vector3(x * 1024.0f, y5 * 256.0f, z * 1024.0f),
                                                                                     new Vector3((x + 1) * 1024.0f, y6 * 256.0f, (z + 1) * 1024.0f),
                                                                                     face.TriangleUV2[0], face.TriangleUV2[1], face.TriangleUV2[2], e3, e4, e2, 1);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((Blocks[x, z].Type == BlockType.Floor && Blocks[x, z].FloorPortal == -1) || /*Blocks[x, z].Type == BlockType.CeilingPortal ||*/
                            (Blocks[x, z].FloorPortal >= 0 && Blocks[x, z].FloorOpacity != PortalOpacity.None) ||
                            Blocks[x, z].IsFloorSolid)
                        {
                            int split = GetBestFloorSplit(x, z, qa0, qa1, qa2, qa3);
                            if (Blocks[x, z].SplitFoorType == 1)
                                if (split == 0)
                                    split = 1;
                                else
                                    split = 0;

                            Blocks[x, z].RealSplitFloor = (byte)split;

                            if (split == 0)
                            {
                                AddTriangle(x, z, BlockFaces.Floor, new Vector3((x + 1) * 1024.0f, qa1 * 256.0f, (z + 1) * 1024.0f),
                                                                    new Vector3((x + 1) * 1024.0f, qa2 * 256.0f, z * 1024.0f),
                                                                    new Vector3(x * 1024.0f, qa0 * 256.0f, (z + 1) * 1024.0f),
                                                                    face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e2, e3, e1);

                                face = Blocks[x, z].Faces[(int)BlockFaces.FloorTriangle2];
                                AddTriangle(x, z, BlockFaces.FloorTriangle2, new Vector3(x * 1024.0f, qa3 * 256.0f, z * 1024.0f),
                                                                             new Vector3(x * 1024.0f, qa0 * 256.0f, (z + 1) * 1024.0f),
                                                                             new Vector3((x + 1) * 1024.0f, qa2 * 256.0f, z * 1024.0f),
                                                                             face.TriangleUV2[0], face.TriangleUV2[1], face.TriangleUV2[2], e4, e1, e3);
                            }
                            else
                            {
                                AddTriangle(x, z, BlockFaces.Floor, new Vector3(x * 1024.0f, qa0 * 256.0f, (z + 1) * 1024.0f),
                                                                    new Vector3((x + 1) * 1024.0f, qa1 * 256.0f, (z + 1) * 1024.0f),
                                                                    new Vector3(x * 1024.0f, qa3 * 256.0f, z * 1024.0f),
                                                                    face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);

                                face = Blocks[x, z].Faces[(int)BlockFaces.FloorTriangle2];
                                AddTriangle(x, z, BlockFaces.FloorTriangle2, new Vector3((x + 1) * 1024.0f, qa2 * 256.0f, z * 1024.0f),
                                                                             new Vector3(x * 1024.0f, qa3 * 256.0f, z * 1024.0f),
                                                                             new Vector3((x + 1) * 1024.0f, qa1 * 256.0f, (z + 1) * 1024.0f),
                                                                             face.TriangleUV2[0], face.TriangleUV2[1], face.TriangleUV2[2], e3, e4, e2, 1);
                            }
                        }
                    }

                    // Ceiling polygons ---------------------------------------------------------------------------------
                    //
                    //  2----1    Split 0: 142 324  
                    //  | \  |    Split 1: 213 431
                    //  |  \ |
                    //  3----4
                    //

                    face = Blocks[x, z].Faces[(int)BlockFaces.Ceiling];

                    // First, I reset the slope already calculated
                    Blocks[x, z].CeilingSlopeX = 0;
                    Blocks[x, z].CeilingSlopeZ = 0;

                    if (IsQuad(x, z, ws0, ws1, ws2, ws3) || (Blocks[x, z].CeilingDiagonalSplit != DiagonalSplit.None))
                    {
                        if (!(ws0 == ws1 && ws1 == ws2 && ws2 == ws3) && Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.None)
                        {
                            CalculateCeilingSlope(x, z, ws0, ws1, ws2, ws3);
                        }

                        if ((Blocks[x, z].Type == BlockType.Floor && Blocks[x, z].CeilingPortal == -1) ||/* Blocks[x, z].Type == BlockType.CeilingPortal ||*/
                            (Blocks[x, z].CeilingPortal >= 0 && Blocks[x, z].CeilingOpacity != PortalOpacity.None) ||
                            Blocks[x, z].IsCeilingSolid || Blocks[x, z].Type == BlockType.Wall)
                        {
                            if (Blocks[x, z].SplitCeiling == false && Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.None)
                            {
                                AddRectangle(x, z, BlockFaces.Ceiling, new Vector3((x + 1) * 1024.0f, (Ceiling + ws1) * 256.0f, (z + 1) * 1024.0f),
                                                                       new Vector3((x) * 1024.0f, (Ceiling + ws0) * 256.0f, (z + 1) * 1024.0f),
                                                                       new Vector3((x) * 1024.0f, (Ceiling + ws3) * 256.0f, (z) * 1024.0f),
                                                                       new Vector3((x + 1) * 1024.0f, (Ceiling + ws2) * 256.0f, (z) * 1024.0f),
                                                                       face.RectangleUV[0], face.RectangleUV[1], face.RectangleUV[2], face.RectangleUV[3],
                                                                       e2, e1, e4, e3);
                            }
                            else
                            {
                                int split = GetBestCeilingSplit(x, z, ws0, ws1, ws2, ws3);
                                if (Blocks[x, z].SplitCeilingType == 1)
                                    if (split == 0)
                                        split = 1;
                                    else
                                        split = 0;

                                if (Blocks[x, z].Type != BlockType.Wall)
                                {
                                    if (Blocks[x, z].SplitFoorType == 1)
                                        if (split == 0)
                                            split = 1;
                                        else
                                            split = 0;
                                }

                                bool addTriangle1 = true;
                                bool addTriangle2 = true;

                                int y1 = 0;
                                int y2 = 0;
                                int y3 = 0;
                                int y4 = 0;
                                int y5 = 0;
                                int y6 = 0;

                                if (Blocks[x, z].CeilingDiagonalSplit != DiagonalSplit.None)
                                {
                                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.NW)
                                    {
                                        split = 0;
                                        addTriangle1 = true;
                                        addTriangle2 = (Blocks[x, z].Type != BlockType.Wall);
                                        y1 = Ceiling + ws0;
                                        y2 = Ceiling + ws0;
                                        y3 = Ceiling + ws0;
                                        y4 = Ceiling + ws2;
                                        y5 = Ceiling + ws1;
                                        y6 = Ceiling + ws3;
                                    }

                                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.NE)
                                    {
                                        split = 1;
                                        addTriangle1 = true;
                                        addTriangle2 = (Blocks[x, z].Type != BlockType.Wall);
                                        y1 = Ceiling + ws1;
                                        y2 = Ceiling + ws1;
                                        y3 = Ceiling + ws1;
                                        y4 = Ceiling + ws3;
                                        y5 = Ceiling + ws2;
                                        y6 = Ceiling + ws0;
                                    }

                                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.SE)
                                    {
                                        split = 0;
                                        addTriangle1 = (Blocks[x, z].Type != BlockType.Wall);
                                        addTriangle2 = true;
                                        y1 = Ceiling + ws0;
                                        y2 = Ceiling + ws3;
                                        y3 = Ceiling + ws1;
                                        y4 = Ceiling + ws2;
                                        y5 = Ceiling + ws2;
                                        y6 = Ceiling + ws2;
                                    }

                                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.SW)
                                    {
                                        split = 1;
                                        addTriangle1 = (Blocks[x, z].Type != BlockType.Wall);
                                        addTriangle2 = true;
                                        y1 = Ceiling + ws1;
                                        y2 = Ceiling + ws0;
                                        y3 = Ceiling + ws2;
                                        y4 = Ceiling + ws3;
                                        y5 = Ceiling + ws3;
                                        y6 = Ceiling + ws3;
                                    }
                                }
                                else
                                {
                                    if (split == 0)
                                    {
                                        addTriangle1 = true;
                                        addTriangle2 = true;
                                        y1 = Ceiling + ws0;
                                        y2 = Ceiling + ws3;
                                        y3 = Ceiling + ws1;
                                        y4 = Ceiling + ws2;
                                        y5 = Ceiling + ws1;
                                        y6 = Ceiling + ws3;
                                    }
                                    else
                                    {
                                        addTriangle1 = true;
                                        addTriangle2 = true;
                                        y1 = Ceiling + ws1;
                                        y2 = Ceiling + ws0;
                                        y3 = Ceiling + ws2;
                                        y4 = Ceiling + ws3;
                                        y5 = Ceiling + ws2;
                                        y6 = Ceiling + ws0;
                                    }
                                }

                                if (split == 0)
                                {
                                    if (addTriangle1)
                                    {
                                        AddTriangle(x, z, BlockFaces.Ceiling, new Vector3(x * 1024.0f, y1 * 256.0f, (z + 1) * 1024.0f),
                                                                            new Vector3(x * 1024.0f, y2 * 256.0f, z * 1024.0f),
                                                                            new Vector3((x + 1) * 1024.0f, y3 * 256.0f, (z + 1) * 1024.0f),
                                                                            face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e4, e2, 1);
                                    }

                                    if (addTriangle2)
                                    {
                                        face = Blocks[x, z].Faces[(int)BlockFaces.CeilingTriangle2];
                                        AddTriangle(x, z, BlockFaces.CeilingTriangle2, new Vector3((x + 1) * 1024.0f, y4 * 256.0f, (z) * 1024.0f),
                                                                                     new Vector3((x + 1) * 1024.0f, y5 * 256.0f, (z + 1) * 1024.0f),
                                                                                     new Vector3((x) * 1024.0f, y6 * 256.0f, z * 1024.0f),
                                                                                     face.TriangleUV2[0], face.TriangleUV2[1], face.TriangleUV2[2], e3, e2, e4, 1);
                                    }
                                }
                                else
                                {
                                    if (addTriangle1)
                                    {
                                        AddTriangle(x, z, BlockFaces.Ceiling, new Vector3((x + 1) * 1024.0f, y1 * 256.0f, (z + 1) * 1024.0f),
                                                                            new Vector3((x) * 1024.0f, y2 * 256.0f, (z + 1) * 1024.0f),
                                                                            new Vector3((x + 1) * 1024.0f, y3 * 256.0f, (z) * 1024.0f),
                                                                            face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e2, e1, e3);
                                    }

                                    if (addTriangle2)
                                    {
                                        face = Blocks[x, z].Faces[(int)BlockFaces.CeilingTriangle2];
                                        AddTriangle(x, z, BlockFaces.CeilingTriangle2, new Vector3(x * 1024.0f, y4 * 256.0f, (z) * 1024.0f),
                                                                                     new Vector3((x + 1) * 1024.0f, y5 * 256.0f, z * 1024.0f),
                                                                                     new Vector3((x) * 1024.0f, y6 * 256.0f, (z + 1) * 1024.0f),
                                                                                     face.TriangleUV2[0], face.TriangleUV2[1], face.TriangleUV2[2], e4, e3, e1);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((Blocks[x, z].Type == BlockType.Floor && Blocks[x, z].CeilingPortal == -1) || /*Blocks[x, z].FloorPortal != -1 ||*/
                            (Blocks[x, z].CeilingPortal != -1 && Blocks[x, z].CeilingOpacity != PortalOpacity.None) ||
                            Blocks[x, z].IsCeilingSolid)
                        {
                            int split = GetBestCeilingSplit(x, z, ws0, ws1, ws2, ws3);
                            if (Blocks[x, z].SplitCeilingType == 1)
                                if (split == 0)
                                    split = 1;
                                else
                                    split = 0;

                            Blocks[x, z].RealSplitCeiling = (byte)split;

                            if (split == 1)
                            {
                                AddTriangle(x, z, BlockFaces.Ceiling, new Vector3(x * 1024.0f, (Ceiling + ws0) * 256.0f, (z + 1) * 1024.0f),
                                                                    new Vector3(x * 1024.0f, (Ceiling + ws3) * 256.0f, z * 1024.0f),
                                                                    new Vector3((x + 1) * 1024.0f, (Ceiling + ws1) * 256.0f, (z + 1) * 1024.0f),
                                                                    face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e4, e2, 1);

                                face = Blocks[x, z].Faces[(int)BlockFaces.CeilingTriangle2];
                                AddTriangle(x, z, BlockFaces.CeilingTriangle2, new Vector3((x + 1) * 1024.0f, (Ceiling + ws2) * 256.0f, (z) * 1024.0f),
                                                                             new Vector3((x + 1) * 1024.0f, (Ceiling + ws1) * 256.0f, (z + 1) * 1024.0f),
                                                                             new Vector3((x) * 1024.0f, (Ceiling + ws3) * 256.0f, z * 1024.0f),
                                                                             face.TriangleUV2[0], face.TriangleUV2[1], face.TriangleUV2[2], e3, e2, e4, 1);
                            }
                            else
                            {
                                AddTriangle(x, z, BlockFaces.Ceiling, new Vector3((x + 1) * 1024.0f, (Ceiling + ws1) * 256.0f, (z + 1) * 1024.0f),
                                                                    new Vector3((x) * 1024.0f, (Ceiling + ws0) * 256.0f, (z + 1) * 1024.0f),
                                                                    new Vector3((x + 1) * 1024.0f, (Ceiling + ws2) * 256.0f, (z) * 1024.0f),
                                                                    face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e2, e1, e3);

                                face = Blocks[x, z].Faces[(int)BlockFaces.CeilingTriangle2];
                                AddTriangle(x, z, BlockFaces.CeilingTriangle2, new Vector3(x * 1024.0f, (Ceiling + ws3) * 256.0f, (z) * 1024.0f),
                                                                             new Vector3((x + 1) * 1024.0f, (Ceiling + ws2) * 256.0f, z * 1024.0f),
                                                                             new Vector3((x) * 1024.0f, (Ceiling + ws0) * 256.0f, (z + 1) * 1024.0f),
                                                                             face.TriangleUV2[0], face.TriangleUV2[1], face.TriangleUV2[2], e4, e3, e1);
                            }
                        }
                    }
                }
            }

            // After building the geometry subset, I have to rebuild the editor vertices array for the vertex buffer
            Vertices = new List<EditorVertex>();
            Vector4 tempCentre = Vector4.Zero;
            int totalVertices = 0;

            for (int x = 0; x < NumXSectors; x++)
            {
                for (int z = 0; z < NumZSectors; z++)
                {
                    for (int f = 0; f < 29; f++)
                    {
                        BlockFace face = Blocks[x, z].Faces[f];
                        if (face == null || !face.Defined)
                            continue;

                        if (face.Vertices != null && face.Vertices.Length != 0)
                        {
                            Blocks[x, z].Faces[f].StartVertex = (short)Vertices.Count;
                            int baseIndex = Vertices.Count;

                            Blocks[x, z].Faces[(int)f].IndicesForSolidBucketsRendering = new List<short>();

                            Blocks[x, z].Faces[(int)f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 0));
                            Blocks[x, z].Faces[(int)f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 1));
                            Blocks[x, z].Faces[(int)f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 2));

                            if (face.Shape == BlockFaceShape.Rectangle)
                            {
                                Blocks[x, z].Faces[(int)f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 3));
                                Blocks[x, z].Faces[(int)f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 4));
                                Blocks[x, z].Faces[(int)f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 5));
                            }

                            Vertices.AddRange(face.Vertices);
                        }
                    }

                    for (int i = 0; i < _numVerticesInGrid[x, z]; i++)
                    {
                        tempCentre += _verticesGrid[x, z, i].Position;
                        totalVertices++;
                    }
                }
            }

            Centre = new Vector3(tempCentre.X, tempCentre.Y, tempCentre.Z);
            Centre /= totalVertices;
        }

        private void AddVerticalFaces(int x, int z, FaceDirection direction, bool floor, bool ceiling, bool middle)
        {
            int xA = 0, xB = 0, zA = 0, zB = 0, yA = 0, yB = 0, yC = 0, yD = 0;

            Vector2 e1 = new Vector2(0.0f, 0.0f);
            Vector2 e2 = new Vector2(1.0f, 0.0f);
            Vector2 e3 = new Vector2(1.0f, 1.0f);
            Vector2 e4 = new Vector2(0.0f, 1.0f);

            Block otherBlock;
            BlockFace face;

            BlockFaces qaFace, edFace, wsFace, rfFace, middleFace;
            int qA, qB, eA, eB, rA, rB, wA, wB, fA, fB, cA, cB;

            switch (direction)
            {
                case FaceDirection.North:
                    xA = x + 1;
                    xB = x;
                    zA = z + 1;
                    zB = z + 1;
                    otherBlock = Blocks[x, z + 1];
                    qA = Blocks[x, z].QAFaces[1];
                    qB = Blocks[x, z].QAFaces[0];
                    eA = Blocks[x, z].EDFaces[1];
                    eB = Blocks[x, z].EDFaces[0];
                    rA = Blocks[x, z].RFFaces[1];
                    rB = Blocks[x, z].RFFaces[0];
                    wA = Blocks[x, z].WSFaces[1];
                    wB = Blocks[x, z].WSFaces[0];
                    fA = otherBlock.QAFaces[2];
                    fB = otherBlock.QAFaces[3];
                    cA = otherBlock.WSFaces[2];
                    cB = otherBlock.WSFaces[3];
                    qaFace = BlockFaces.NorthQA;
                    edFace = BlockFaces.NorthED;
                    middleFace = BlockFaces.NorthMiddle;
                    rfFace = BlockFaces.NorthRF;
                    wsFace = BlockFaces.NorthWS;

                    if (Blocks[x, z].WallPortal != -1)
                    {
                        Portal portal = FindPortal(x, z, PortalDirection.South);
                        Room adjoiningRoom = Level.Rooms[portal.AdjoiningRoom];
                        if (Flipped && BaseRoom != -1)
                        {
                            if (adjoiningRoom.Flipped)
                                adjoiningRoom = Level.Rooms[adjoiningRoom.AlternateRoom];
                        }

                        int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].QAFaces[1];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].QAFaces[0];
                        qA = (int)Position.Y + Blocks[x, 1].QAFaces[2];
                        qB = (int)Position.Y + Blocks[x, 1].QAFaces[3];
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].WSFaces[1];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].WSFaces[0];
                        wA = (int)Position.Y + Blocks[x, 1].WSFaces[2];
                        wB = (int)Position.Y + Blocks[x, 1].WSFaces[3];
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;

                        Blocks[x, z].QAFaces[1] = (short)qA;
                        Blocks[x, z].QAFaces[0] = (short)qB;
                        Blocks[x, z].WSFaces[1] = (short)wA;
                        Blocks[x, z].WSFaces[0] = (short)wB;
                    }

                    if (Blocks[x, z].Type == BlockType.BorderWall)
                    {
                        if (Blocks[x, 1].WSFaces[2] + Ceiling < Blocks[x, z].QAFaces[1])
                        {
                            qA = Blocks[x, 1].WSFaces[2] + Ceiling;
                            Blocks[x, z].QAFaces[1] = (short)qA;
                        }

                        if (Blocks[x, 1].WSFaces[3] + Ceiling < Blocks[x, z].QAFaces[0])
                        {
                            qB = Blocks[x, 1].WSFaces[3] + Ceiling;
                            Blocks[x, z].QAFaces[0] = (short)qB;
                        }
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NW)
                    {
                        qA = Blocks[x, z].QAFaces[0];
                        qB = Blocks[x, z].QAFaces[0];
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NE)
                    {
                        qA = Blocks[x, z].QAFaces[1];
                        qB = Blocks[x, z].QAFaces[1];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.SE)
                    {
                        fA = otherBlock.QAFaces[2];
                        fB = otherBlock.QAFaces[2];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.SW)
                    {
                        fA = otherBlock.QAFaces[3];
                        fB = otherBlock.QAFaces[3];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.NW)
                    {
                        wA = Blocks[x, z].WSFaces[0];
                        wB = Blocks[x, z].WSFaces[0];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.NE)
                    {
                        wA = Blocks[x, z].WSFaces[1];
                        wB = Blocks[x, z].WSFaces[1];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.SE)
                    {
                        cA = otherBlock.WSFaces[2];
                        cB = otherBlock.WSFaces[2];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.SW)
                    {
                        cA = otherBlock.WSFaces[3];
                        cB = otherBlock.WSFaces[3];
                    }

                    break;

                case FaceDirection.South:
                    xA = x;
                    xB = x + 1;
                    zA = z;
                    zB = z;
                    otherBlock = Blocks[x, z - 1];
                    qA = Blocks[x, z].QAFaces[3];
                    qB = Blocks[x, z].QAFaces[2];
                    eA = Blocks[x, z].EDFaces[3];
                    eB = Blocks[x, z].EDFaces[2];
                    rA = Blocks[x, z].RFFaces[3];
                    rB = Blocks[x, z].RFFaces[2];
                    wA = Blocks[x, z].WSFaces[3];
                    wB = Blocks[x, z].WSFaces[2];
                    fA = otherBlock.QAFaces[0];
                    fB = otherBlock.QAFaces[1];
                    cA = otherBlock.WSFaces[0];
                    cB = otherBlock.WSFaces[1];
                    qaFace = BlockFaces.SouthQA;
                    edFace = BlockFaces.SouthED;
                    middleFace = BlockFaces.SouthMiddle;
                    rfFace = BlockFaces.SouthRF;
                    wsFace = BlockFaces.SouthWS;

                    if (Blocks[x, z].WallPortal != -1)
                    {
                        Portal portal = FindPortal(x, z, PortalDirection.North);
                        Room adjoiningRoom = Level.Rooms[portal.AdjoiningRoom];
                        if (Flipped && BaseRoom != -1)
                        {
                            if (adjoiningRoom.Flipped)
                                adjoiningRoom = Level.Rooms[adjoiningRoom.AlternateRoom];
                        }

                        int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);
                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[facingX, 1].QAFaces[3];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[facingX, 1].QAFaces[2];
                        qA = (int)Position.Y + Blocks[x, NumZSectors - 2].QAFaces[0];
                        qB = (int)Position.Y + Blocks[x, NumZSectors - 2].QAFaces[1];
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[facingX, 1].WSFaces[3];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[facingX, 1].WSFaces[2];
                        wA = (int)Position.Y + Blocks[x, NumZSectors - 2].WSFaces[0];
                        wB = (int)Position.Y + Blocks[x, NumZSectors - 2].WSFaces[1];
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;

                        Blocks[x, z].QAFaces[3] = (short)qA;
                        Blocks[x, z].QAFaces[2] = (short)qB;
                        Blocks[x, z].WSFaces[3] = (short)wA;
                        Blocks[x, z].WSFaces[2] = (short)wB;
                    }

                    if (Blocks[x, z].Type == BlockType.BorderWall)
                    {
                        if (Blocks[x, NumZSectors - 2].WSFaces[0] + Ceiling < Blocks[x, z].QAFaces[3])
                        {
                            qA = Blocks[x, NumZSectors - 2].WSFaces[0] + Ceiling;
                            Blocks[x, z].QAFaces[3] = (short)qA;
                        }

                        if (Blocks[x, NumZSectors - 2].WSFaces[1] + Ceiling < Blocks[x, z].QAFaces[2])
                        {
                            qB = Blocks[x, NumZSectors - 2].WSFaces[1] + Ceiling;
                            Blocks[x, z].QAFaces[2] = (short)qB;
                        }
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SW)
                    {
                        qA = Blocks[x, z].QAFaces[3];
                        qB = Blocks[x, z].QAFaces[3];
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SE)
                    {
                        qA = Blocks[x, z].QAFaces[2];
                        qB = Blocks[x, z].QAFaces[2];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.NW)
                    {
                        fA = otherBlock.QAFaces[0];
                        fB = otherBlock.QAFaces[0];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.NE)
                    {
                        fA = otherBlock.QAFaces[1];
                        fB = otherBlock.QAFaces[1];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.SW)
                    {
                        wA = Blocks[x, z].WSFaces[3];
                        wB = Blocks[x, z].WSFaces[3];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.SE)
                    {
                        wA = Blocks[x, z].WSFaces[2];
                        wB = Blocks[x, z].WSFaces[2];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.NW)
                    {
                        cA = otherBlock.WSFaces[0];
                        cB = otherBlock.WSFaces[0];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.NE)
                    {
                        cA = otherBlock.WSFaces[1];
                        cB = otherBlock.WSFaces[1];
                    }

                    break;

                case FaceDirection.East:
                    xA = x + 1;
                    xB = x + 1;
                    zA = z;
                    zB = z + 1;
                    otherBlock = Blocks[x + 1, z];
                    qA = Blocks[x, z].QAFaces[2];
                    qB = Blocks[x, z].QAFaces[1];
                    eA = Blocks[x, z].EDFaces[2];
                    eB = Blocks[x, z].EDFaces[1];
                    rA = Blocks[x, z].RFFaces[2];
                    rB = Blocks[x, z].RFFaces[1];
                    wA = Blocks[x, z].WSFaces[2];
                    wB = Blocks[x, z].WSFaces[1];
                    fA = otherBlock.QAFaces[3];
                    fB = otherBlock.QAFaces[0];
                    cA = otherBlock.WSFaces[3];
                    cB = otherBlock.WSFaces[0];
                    qaFace = BlockFaces.EastQA;
                    edFace = BlockFaces.EastED;
                    middleFace = BlockFaces.EastMiddle;
                    rfFace = BlockFaces.EastRF;
                    wsFace = BlockFaces.EastWS;

                    if (Blocks[x, z].WallPortal != -1)
                    {
                        Portal portal = FindPortal(x, z, PortalDirection.West);
                        Room adjoiningRoom = Level.Rooms[portal.AdjoiningRoom];
                        if (Flipped && BaseRoom != -1)
                        {
                            if (adjoiningRoom.Flipped)
                                adjoiningRoom = Level.Rooms[adjoiningRoom.AlternateRoom];
                        }


                        int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);
                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].QAFaces[2];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].QAFaces[1];
                        qA = (int)Position.Y + Blocks[1, z].QAFaces[3];
                        qB = (int)Position.Y + Blocks[1, z].QAFaces[0];
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        wA = (int)Position.Y + Blocks[1, z].QAFaces[3];
                        wB = (int)Position.Y + Blocks[1, z].QAFaces[0];
                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].WSFaces[2];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].WSFaces[1];
                        wA = (int)Position.Y + Blocks[1, z].WSFaces[1];
                        wB = (int)Position.Y + Blocks[1, z].WSFaces[2];
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;

                        Blocks[x, z].QAFaces[2] = (short)qA;
                        Blocks[x, z].QAFaces[1] = (short)qB;
                        Blocks[x, z].WSFaces[2] = (short)wA;
                        Blocks[x, z].WSFaces[1] = (short)wB;
                    }

                    if (Blocks[x, z].Type == BlockType.BorderWall)
                    {
                        if (Blocks[1, z].WSFaces[3] + Ceiling < Blocks[x, z].QAFaces[2])
                        {
                            qA = Blocks[1, z].WSFaces[3] + Ceiling;
                            Blocks[x, z].QAFaces[2] = (short)qA;
                        }

                        if (Blocks[1, z].WSFaces[0] + Ceiling < Blocks[x, z].QAFaces[1])
                        {
                            qB = Blocks[1, z].WSFaces[0] + Ceiling;
                            Blocks[x, z].QAFaces[1] = (short)qB;
                        }
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NE)
                    {
                        qA = Blocks[x, z].QAFaces[1];
                        qB = Blocks[x, z].QAFaces[1];
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SE)
                    {
                        qA = Blocks[x, z].QAFaces[2];
                        qB = Blocks[x, z].QAFaces[2];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.NW)
                    {
                        fA = otherBlock.QAFaces[0];
                        fB = otherBlock.QAFaces[0];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.SW)
                    {
                        fA = otherBlock.QAFaces[3];
                        fB = otherBlock.QAFaces[3];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.NE)
                    {
                        wA = Blocks[x, z].WSFaces[1];
                        wB = Blocks[x, z].WSFaces[1];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.SE)
                    {
                        wA = Blocks[x, z].WSFaces[2];
                        wB = Blocks[x, z].WSFaces[2];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.NW)
                    {
                        cA = otherBlock.WSFaces[0];
                        cB = otherBlock.WSFaces[0];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.SW)
                    {
                        cA = otherBlock.WSFaces[3];
                        cB = otherBlock.WSFaces[3];
                    }

                    break;

                case FaceDirection.DiagonalFloor:
                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NW)
                    {
                        xA = x + 1;
                        xB = x;
                        zA = z + 1;
                        zB = z;
                        otherBlock = Blocks[x, z];
                        qA = Blocks[x, z].QAFaces[1];
                        qB = Blocks[x, z].QAFaces[3];
                        eA = Blocks[x, z].EDFaces[1];
                        eB = Blocks[x, z].EDFaces[3];
                        rA = Blocks[x, z].RFFaces[1];
                        rB = Blocks[x, z].RFFaces[3];
                        wA = Blocks[x, z].WSFaces[1];
                        wB = Blocks[x, z].WSFaces[3];
                        fA = Blocks[x, z].QAFaces[0];
                        fB = Blocks[x, z].QAFaces[0];
                        cA = Blocks[x, z].WSFaces[0];
                        cB = Blocks[x, z].WSFaces[0];
                        qaFace = BlockFaces.DiagonalQA;
                        edFace = BlockFaces.DiagonalED;
                        middleFace = BlockFaces.DiagonalMiddle;
                        rfFace = BlockFaces.DiagonalRF;
                        wsFace = BlockFaces.DiagonalWS;
                    }
                    else if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NE)
                    {
                        xA = x + 1;
                        xB = x;
                        zA = z;
                        zB = z + 1;
                        otherBlock = Blocks[x, z];
                        qA = Blocks[x, z].QAFaces[2];
                        qB = Blocks[x, z].QAFaces[0];
                        eA = Blocks[x, z].EDFaces[2];
                        eB = Blocks[x, z].EDFaces[0];
                        rA = Blocks[x, z].RFFaces[2];
                        rB = Blocks[x, z].RFFaces[0];
                        wA = Blocks[x, z].WSFaces[2];
                        wB = Blocks[x, z].WSFaces[0];
                        fA = Blocks[x, z].QAFaces[1];
                        fB = Blocks[x, z].QAFaces[1];
                        cA = Blocks[x, z].WSFaces[1];
                        cB = Blocks[x, z].WSFaces[1];
                        qaFace = BlockFaces.DiagonalQA;
                        edFace = BlockFaces.DiagonalED;
                        middleFace = BlockFaces.DiagonalMiddle;
                        rfFace = BlockFaces.DiagonalRF;
                        wsFace = BlockFaces.DiagonalWS;
                    }
                    else if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SE)
                    {
                        xA = x;
                        xB = x + 1;
                        zA = z;
                        zB = z + 1;
                        otherBlock = Blocks[x, z];
                        qA = Blocks[x, z].QAFaces[3];
                        qB = Blocks[x, z].QAFaces[1];
                        eA = Blocks[x, z].EDFaces[3];
                        eB = Blocks[x, z].EDFaces[1];
                        rA = Blocks[x, z].RFFaces[3];
                        rB = Blocks[x, z].RFFaces[1];
                        wA = Blocks[x, z].WSFaces[3];
                        wB = Blocks[x, z].WSFaces[1];
                        fA = Blocks[x, z].QAFaces[2];
                        fB = Blocks[x, z].QAFaces[2];
                        cA = Blocks[x, z].WSFaces[2];
                        cB = Blocks[x, z].WSFaces[2];
                        qaFace = BlockFaces.DiagonalQA;
                        edFace = BlockFaces.DiagonalED;
                        middleFace = BlockFaces.DiagonalMiddle;
                        rfFace = BlockFaces.DiagonalRF;
                        wsFace = BlockFaces.DiagonalWS;
                    }
                    else
                    {
                        xA = x;
                        xB = x + 1;
                        zA = z + 1;
                        zB = z;
                        otherBlock = Blocks[x, z];
                        qA = Blocks[x, z].QAFaces[0];
                        qB = Blocks[x, z].QAFaces[2];
                        eA = Blocks[x, z].EDFaces[0];
                        eB = Blocks[x, z].EDFaces[2];
                        rA = Blocks[x, z].RFFaces[0];
                        rB = Blocks[x, z].RFFaces[2];
                        wA = Blocks[x, z].WSFaces[0];
                        wB = Blocks[x, z].WSFaces[2];
                        fA = Blocks[x, z].QAFaces[3];
                        fB = Blocks[x, z].QAFaces[3];
                        cA = Blocks[x, z].WSFaces[3];
                        cB = Blocks[x, z].WSFaces[3];
                        qaFace = BlockFaces.DiagonalQA;
                        edFace = BlockFaces.DiagonalED;
                        middleFace = BlockFaces.DiagonalMiddle;
                        rfFace = BlockFaces.DiagonalRF;
                        wsFace = BlockFaces.DiagonalWS;
                    }

                    break;

                case FaceDirection.DiagonalCeiling:
                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.NW)
                    {
                        xA = x + 1;
                        xB = x;
                        zA = z + 1;
                        zB = z;
                        otherBlock = Blocks[x, z];
                        qA = Blocks[x, z].QAFaces[1];
                        qB = Blocks[x, z].QAFaces[3];
                        eA = Blocks[x, z].EDFaces[1];
                        eB = Blocks[x, z].EDFaces[3];
                        rA = Blocks[x, z].RFFaces[1];
                        rB = Blocks[x, z].RFFaces[3];
                        wA = Blocks[x, z].WSFaces[1];
                        wB = Blocks[x, z].WSFaces[3];
                        fA = Blocks[x, z].QAFaces[0];
                        fB = Blocks[x, z].QAFaces[0];
                        cA = Blocks[x, z].WSFaces[0];
                        cB = Blocks[x, z].WSFaces[0];
                        qaFace = BlockFaces.DiagonalQA;
                        edFace = BlockFaces.DiagonalED;
                        middleFace = BlockFaces.DiagonalMiddle;
                        rfFace = BlockFaces.DiagonalRF;
                        wsFace = BlockFaces.DiagonalWS;
                    }
                    else if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.NE)
                    {
                        xA = x + 1;
                        xB = x;
                        zA = z;
                        zB = z + 1;
                        otherBlock = Blocks[x, z];
                        qA = Blocks[x, z].QAFaces[2];
                        qB = Blocks[x, z].QAFaces[0];
                        eA = Blocks[x, z].EDFaces[2];
                        eB = Blocks[x, z].EDFaces[0];
                        rA = Blocks[x, z].RFFaces[2];
                        rB = Blocks[x, z].RFFaces[0];
                        wA = Blocks[x, z].WSFaces[2];
                        wB = Blocks[x, z].WSFaces[0];
                        fA = Blocks[x, z].QAFaces[1];
                        fB = Blocks[x, z].QAFaces[1];
                        cA = Blocks[x, z].WSFaces[1];
                        cB = Blocks[x, z].WSFaces[1];
                        qaFace = BlockFaces.DiagonalQA;
                        edFace = BlockFaces.DiagonalED;
                        middleFace = BlockFaces.DiagonalMiddle;
                        rfFace = BlockFaces.DiagonalRF;
                        wsFace = BlockFaces.DiagonalWS;
                    }
                    else if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.SE)
                    {
                        xA = x;
                        xB = x + 1;
                        zA = z;
                        zB = z + 1;
                        otherBlock = Blocks[x, z];
                        qA = Blocks[x, z].QAFaces[3];
                        qB = Blocks[x, z].QAFaces[1];
                        eA = Blocks[x, z].EDFaces[3];
                        eB = Blocks[x, z].EDFaces[1];
                        rA = Blocks[x, z].RFFaces[3];
                        rB = Blocks[x, z].RFFaces[1];
                        wA = Blocks[x, z].WSFaces[3];
                        wB = Blocks[x, z].WSFaces[1];
                        fA = Blocks[x, z].QAFaces[2];
                        fB = Blocks[x, z].QAFaces[2];
                        cA = Blocks[x, z].WSFaces[2];
                        cB = Blocks[x, z].WSFaces[2];
                        qaFace = BlockFaces.DiagonalQA;
                        edFace = BlockFaces.DiagonalED;
                        middleFace = BlockFaces.DiagonalMiddle;
                        rfFace = BlockFaces.DiagonalRF;
                        wsFace = BlockFaces.DiagonalWS;
                    }
                    else
                    {
                        xA = x;
                        xB = x + 1;
                        zA = z + 1;
                        zB = z;
                        otherBlock = Blocks[x, z];
                        qA = Blocks[x, z].QAFaces[0];
                        qB = Blocks[x, z].QAFaces[2];
                        eA = Blocks[x, z].EDFaces[0];
                        eB = Blocks[x, z].EDFaces[2];
                        rA = Blocks[x, z].RFFaces[0];
                        rB = Blocks[x, z].RFFaces[2];
                        wA = Blocks[x, z].WSFaces[0];
                        wB = Blocks[x, z].WSFaces[2];
                        fA = Blocks[x, z].QAFaces[3];
                        fB = Blocks[x, z].QAFaces[3];
                        cA = Blocks[x, z].WSFaces[3];
                        cB = Blocks[x, z].WSFaces[3];
                        qaFace = BlockFaces.DiagonalQA;
                        edFace = BlockFaces.DiagonalED;
                        middleFace = BlockFaces.DiagonalMiddle;
                        rfFace = BlockFaces.DiagonalRF;
                        wsFace = BlockFaces.DiagonalWS;
                    }

                    break;

                default:
                    xA = x;
                    xB = x;
                    zA = z + 1;
                    zB = z;
                    otherBlock = Blocks[x - 1, z];
                    qA = Blocks[x, z].QAFaces[0];
                    qB = Blocks[x, z].QAFaces[3];
                    eA = Blocks[x, z].EDFaces[0];
                    eB = Blocks[x, z].EDFaces[3];
                    rA = Blocks[x, z].RFFaces[0];
                    rB = Blocks[x, z].RFFaces[3];
                    wA = Blocks[x, z].WSFaces[0];
                    wB = Blocks[x, z].WSFaces[3];
                    fA = otherBlock.QAFaces[1];
                    fB = otherBlock.QAFaces[2];
                    cA = otherBlock.WSFaces[1];
                    cB = otherBlock.WSFaces[2];
                    qaFace = BlockFaces.WestQA;
                    edFace = BlockFaces.WestED;
                    middleFace = BlockFaces.WestMiddle;
                    rfFace = BlockFaces.WestRF;
                    wsFace = BlockFaces.WestWS;

                    if (Blocks[x, z].WallPortal != -1)
                    {
                        Portal portal = FindPortal(x, z, PortalDirection.East);
                        Room adjoiningRoom = Level.Rooms[portal.AdjoiningRoom];

                        if (Flipped && BaseRoom != -1)
                        {
                            if (adjoiningRoom.Flipped)
                                adjoiningRoom = Level.Rooms[adjoiningRoom.AlternateRoom];
                        }

                        int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);
                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[1, facingZ].QAFaces[0];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[1, facingZ].QAFaces[3];
                        qA = (int)Position.Y + Blocks[NumXSectors - 2, z].QAFaces[1];
                        qB = (int)Position.Y + Blocks[NumXSectors - 2, z].QAFaces[2];
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[1, facingZ].WSFaces[0];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[1, facingZ].WSFaces[3];
                        wA = (int)Position.Y + Blocks[NumXSectors - 2, z].WSFaces[1];
                        wB = (int)Position.Y + Blocks[NumXSectors - 2, z].WSFaces[2];
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;

                        Blocks[x, z].QAFaces[3] = (short)qA;
                        Blocks[x, z].QAFaces[0] = (short)qB;
                        Blocks[x, z].WSFaces[3] = (short)wA;
                        Blocks[x, z].WSFaces[0] = (short)wB;
                    }

                    if (Blocks[x, z].Type == BlockType.BorderWall)
                    {
                        if (Blocks[NumXSectors - 2, z].WSFaces[1] + Ceiling < Blocks[x, z].QAFaces[0])
                        {
                            qA = Blocks[NumXSectors - 2, z].WSFaces[1] + Ceiling;
                            Blocks[x, z].QAFaces[0] = (short)qA;
                        }

                        if (Blocks[NumXSectors - 2, z].WSFaces[2] + Ceiling < Blocks[x, z].QAFaces[3])
                        {
                            qB = Blocks[NumXSectors - 2, z].WSFaces[2] + Ceiling;
                            Blocks[x, z].QAFaces[3] = (short)qB;
                        }
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.NW)
                    {
                        qA = Blocks[x, z].QAFaces[0];
                        qB = Blocks[x, z].QAFaces[0];
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.SW)
                    {
                        qA = Blocks[x, z].QAFaces[3];
                        qB = Blocks[x, z].QAFaces[3];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.NE)
                    {
                        fA = otherBlock.QAFaces[1];
                        fB = otherBlock.QAFaces[1];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.SE)
                    {
                        fA = otherBlock.QAFaces[2];
                        fB = otherBlock.QAFaces[2];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.NW)
                    {
                        wA = Blocks[x, z].WSFaces[0];
                        wB = Blocks[x, z].WSFaces[0];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.SW)
                    {
                        wA = Blocks[x, z].WSFaces[3];
                        wB = Blocks[x, z].WSFaces[3];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.NE)
                    {
                        cA = otherBlock.WSFaces[1];
                        cB = otherBlock.WSFaces[1];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.SE)
                    {
                        cA = otherBlock.WSFaces[2];
                        cB = otherBlock.WSFaces[2];
                    }

                    break;
            }

            bool subdivide = false;

            if (qA >= fA && qB >= fB && !(qA == fA && qB == fB) && floor)
            {
                // verifico eventuali suddivisione
                yA = fA;
                yB = fB;

                if (eA >= yA && eB >= yB && qA >= eA && qB >= eB && !(eA == yA && eB == yB))
                {
                    subdivide = true;
                    yA = eA;
                    yB = eB;
                }

                // Poligoni QA e ED
                if (floor)
                {
                    face = Blocks[x, z].Faces[(int)qaFace];

                    // QA
                    if (qA > yA && qB > yB)
                        AddRectangle(x, z, qaFace, new Vector3(xA * 1024.0f, qA * 256.0f, zA * 1024.0f),
                                                               new Vector3(xB * 1024.0f, qB * 256.0f, zB * 1024.0f),
                                                               new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                               new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                               face.RectangleUV[0], face.RectangleUV[1], face.RectangleUV[2], face.RectangleUV[3],
                                                               e1, e2, e3, e4);
                    else if (qA > yA && qB == yB && qB >= (qA - yA))
                        AddTriangle(x, z, qaFace, new Vector3(xA * 1024.0f, qA * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                              new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                    else if (qA == yA && qB > yB && qA >= (qB - yB))
                        AddTriangle(x, z, qaFace, new Vector3(xB * 1024.0f, qB * 256.0f, zB * 1024.0f),
                                                              new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                              new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);
                    else if (qA > yA && qB == yB && qB < (qA - yA))
                        AddTriangle(x, z, qaFace, new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                              new Vector3(xA * 1024.0f, qA * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                    else if (qA == yA && qB > yB && qA < (qB - yB))
                        AddTriangle(x, z, qaFace, new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                              new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, qB * 256.0f, zB * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);

                    // ED
                    if (subdivide)
                    {
                        yA = fA;
                        yB = fB;

                        face = Blocks[x, z].Faces[(int)edFace];

                        if (eA > yA && eB > yB)
                            AddRectangle(x, z, edFace, new Vector3(xA * 1024.0f, eA * 256.0f, zA * 1024.0f),
                                                                   new Vector3(xB * 1024.0f, eB * 256.0f, zB * 1024.0f),
                                                                   new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                                   new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                                   face.RectangleUV[0], face.RectangleUV[1], face.RectangleUV[2], face.RectangleUV[3],
                                                                   e1, e2, e3, e4);
                        else if (eA > yA && eB == yB)
                            AddTriangle(x, z, edFace, new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                                  new Vector3(xA * 1024.0f, eA * 256.0f, zA * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, eB * 256.0f, zB * 1024.0f),
                                                                  face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                        else if (eA == yA && eB > yB)
                            AddTriangle(x, z, edFace, new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                                  new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, eB * 256.0f, zB * 1024.0f),
                                                                  face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);
                    }
                }
            }

            subdivide = false;

            if (cA >= wA && cB >= wB && !(wA == cA && wB == cB) && ceiling)
            {
                // verifico eventuali suddivisione
                yA = cA;
                yB = cB;

                if (rA <= yA && rB <= yB && wA <= rA && wB <= rB && !(rA == yA && rB == yB))
                {
                    subdivide = true;
                    yA = rA;
                    yB = rB;
                }

                // Poligoni WS e RF
                if (ceiling)
                {
                    face = Blocks[x, z].Faces[(int)wsFace];

                    // WS
                    if (wA < yA && wB < yB)
                        AddRectangle(x, z, wsFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                               new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                               new Vector3(xB * 1024.0f, (Ceiling + wB) * 256.0f, zB * 1024.0f),
                                                               new Vector3(xA * 1024.0f, (Ceiling + wA) * 256.0f, zA * 1024.0f),
                                                               face.RectangleUV[0], face.RectangleUV[1], face.RectangleUV[2], face.RectangleUV[3],
                                                               e1, e2, e3, e4);
                    else if (wA < yA && wB == yB && wB >= (yA + wA))
                        AddTriangle(x, z, wsFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                              new Vector3(xA * 1024.0f, (Ceiling + wA) * 256.0f, zA * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                    else if (wA == yA && wB < yB && wA >= (yB + wB))
                        AddTriangle(x, z, wsFace, new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                              new Vector3(xB * 1024.0f, (Ceiling + wB) * 256.0f, zB * 1024.0f),
                                                              new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);
                    else if (wA < yA && wB == yB && wB < (yA + wA))
                        AddTriangle(x, z, wsFace, new Vector3(xA * 1024.0f, (Ceiling + wA) * 256.0f, zA * 1024.0f),
                                                              new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                    else if (wA == yA && wB < yB && wA < (yB + wB))
                        AddTriangle(x, z, wsFace, new Vector3(xB * 1024.0f, (Ceiling + wB) * 256.0f, zB * 1024.0f),
                                                              new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);

                    // RF
                    if (subdivide)
                    {
                        yA = cA;
                        yB = cB;

                        face = Blocks[x, z].Faces[(int)rfFace];

                        if (rA < yA && rB < yB)
                            AddRectangle(x, z, rfFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                                   new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                                   new Vector3(xB * 1024.0f, (Ceiling + rB) * 256.0f, zB * 1024.0f),
                                                                   new Vector3(xA * 1024.0f, (Ceiling + rA) * 256.0f, zA * 1024.0f),
                                                                   face.RectangleUV[0], face.RectangleUV[1], face.RectangleUV[2], face.RectangleUV[3],
                                                                   e1, e2, e3, e4);
                        else if (rA < yA && rB == yB)
                            AddTriangle(x, z, rfFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                                  new Vector3(xA * 1024.0f, (Ceiling + rA) * 256.0f, zA * 1024.0f),
                                                                  face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                        else if (rA == yA && rB < yB)
                            AddTriangle(x, z, rfFace, new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, (Ceiling + rB) * 256.0f, zB * 1024.0f),
                                                                  new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                                  face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);
                    }
                }
            }

            // Poligoni WS e RF
            if (middle)
            {
                face = Blocks[x, z].Faces[(int)middleFace];

                if (wA > cA)
                    yA = cA;
                else
                    yA = wA;
                if (wB > cB)
                    yB = cB;
                else
                    yB = wB;
                if (qA < fA)
                    yD = fA;
                else
                    yD = qA;
                if (qB < fB)
                    yC = fB;
                else
                    yC = qB;

                // middle
                if (Ceiling + yA != yD && Ceiling + yB != yC)
                    AddRectangle(x, z, middleFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                           new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                           new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                                                           new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                                                           face.RectangleUV[0], face.RectangleUV[1], face.RectangleUV[2], face.RectangleUV[3],
                                                           e1, e2, e3, e4);

                else if (Ceiling + yA != yD && Ceiling + yB == yC && yC >= (Ceiling + yA + yD))
                    AddTriangle(x, z, middleFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                          new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                          new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                                                          face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);

                else if (Ceiling + yA == yD && Ceiling + yB != yC && yD >= (Ceiling + yB + yC))
                    AddTriangle(x, z, middleFace, new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                       new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                                                       new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                                                       face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);

                else if (Ceiling + yA != yD && Ceiling + yB == yC && yC < (Ceiling + yA + yD))
                    AddTriangle(x, z, middleFace, new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                                                         new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                         new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                         face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);

                else if (Ceiling + yA == yD && Ceiling + yB != yC && yD < (Ceiling + yB + yC))
                    AddTriangle(x, z, middleFace, new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                                                       new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                       new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                       face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);
            }
        }

        public static bool IsQuad(int x, int z, int h1, int h2, int h3, int h4, bool horizontal = false)
        {
            Vector3 p1 = new Vector3(x * 1024.0f, h1 * 256.0f, z * 1024.0f);
            Vector3 p2 = new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f);
            Vector3 p3 = new Vector3((x + 1) * 1024.0f, h3 * 256.0f, (z + 1) * 1024.0f);
            Vector3 p4 = new Vector3(x * 1024.0f, h4 * 256.0f, (z + 1) * 1024.0f);

            Plane plane1 = new Plane(p1, p2, p3);
            Plane plane2 = new Plane(p1, p2, p4);

            if (plane1.Normal != plane2.Normal)
                return false;
            if (horizontal && (plane1.Normal != Vector3.UnitY && plane1.Normal != -Vector3.UnitY))
                return false;

            return true;
        }

        public static bool IsHorizontal(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4)
        {
            Vector3 p1 = new Vector3(v1.X, v1.Y, v1.Z);
            Vector3 p2 = new Vector3(v2.X, v2.Y, v2.Z);
            Vector3 p3 = new Vector3(v3.X, v3.Y, v3.Z);
            Vector3 p4 = new Vector3(v4.X, v4.Y, v4.Z);

            Plane plane1 = new Plane(p1, p2, p3);
            Plane plane2 = new Plane(p1, p2, p4);

            if (plane1.Normal != plane2.Normal)
                return false;
            if (plane1.Normal == Vector3.UnitY || plane1.Normal == -Vector3.UnitY)
                return true;

            return false;
        }

        public static bool IsHorizontal(Vector4 v1, Vector4 v2, Vector4 v3)
        {
            Vector3 p1 = new Vector3(v1.X, v1.Y, v1.Z);
            Vector3 p2 = new Vector3(v2.X, v2.Y, v2.Z);
            Vector3 p3 = new Vector3(v3.X, v3.Y, v3.Z);

            Plane plane1 = new Plane(p1, p2, p3);
            if (plane1.Normal == Vector3.UnitY || plane1.Normal == -Vector3.UnitY)
                return true;

            return false;
        }

        public static int FindHorizontalTriangle(int x, int z, int h1, int h2, int h3, int h4)
        {
            Vector3 p1 = new Vector3(x * 1024.0f, h1 * 256.0f, z * 1024.0f);
            Vector3 p2 = new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f);
            Vector3 p3 = new Vector3((x + 1) * 1024.0f, h3 * 256.0f, (z + 1) * 1024.0f);
            Vector3 p4 = new Vector3(x * 1024.0f, h4 * 256.0f, (z + 1) * 1024.0f);

            Plane plane = new Plane(p1, p2, p4);
            if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                return 0;

            plane = new Plane(p1, p2, p3);
            if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                return 1;

            plane = new Plane(p2, p3, p4);
            if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                return 2;

            plane = new Plane(p3, p4, p1);
            if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                return 3;

            return -1;
        }

        private int GetBestFloorSplit(int x, int z, int h1, int h2, int h3, int h4)
        {
            int horizontalTriangle = FindHorizontalTriangle(x, z, h1, h2, h3, h4);

            switch (horizontalTriangle)
            {
                case 0:
                    return 1;
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 3:
                    return 0;
                default:
                    int min = Math.Min(Math.Min(Math.Min(h1, h2), h3), h4);
                    int max = Math.Max(Math.Max(Math.Max(h1, h2), h3), h4);

                    if (max == h1 && max == h3)
                        return 1;
                    if (max == h2 && max == h4)
                        return 0;

                    if (min == h1 && max == h3)
                        return 1;
                    if (min == h2 && max == h4)
                        return 0;
                    if (min == h3 && max == h1)
                        return 1;
                    if (min == h4 && max == h2)
                        return 0;

                    break;
            }

            return 0;
        }

        private int GetBestCeilingSplit(int x, int z, int h1, int h2, int h3, int h4)
        {
            int horizontalTriangle = FindHorizontalTriangle(x, z, h1, h2, h3, h4);

            switch (horizontalTriangle)
            {
                case 0:
                    return 1;
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 3:
                    return 0;
                default:
                    int min = Math.Min(Math.Min(Math.Min(h1, h2), h3), h4);
                    if (min == h1)
                        return 1;
                    if (min == h2)
                        return 0;
                    if (min == h3)
                        return 1;
                    break;
            }

            return 0;
        }

        private Portal FindPortal(int x, int z, PortalDirection type)
        {
            if (Blocks[x, z].WallPortal != -1)
                return Level.Portals[Blocks[x, z].WallPortal];
            if (Blocks[x, z].FloorPortal != -1 && type == PortalDirection.Floor)
                return Level.Portals[Blocks[x, z].FloorPortal];
            if (Blocks[x, z].CeilingPortal != -1 && type == PortalDirection.Ceiling)
                return Level.Portals[Blocks[x, z].CeilingPortal];

            return null;
        }

        private void AddRectangle(int x, int z, BlockFaces face, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector2 uv1, Vector2 uv2,
                                  Vector2 uv3, Vector2 uv4, Vector2 e1, Vector2 e2, Vector2 e3, Vector2 e4,
                                  byte splitMode = 0)
        {
            Plane plane = new Plane(p1, p2, p3);
            Vector3 normal = plane.Normal;

            EditorVertex v1 = new EditorVertex();
            v1.Position = new Vector4(p1, 1.0f);
            v1.UV = uv1;
            v1.EditorUV = e1;

            EditorVertex v2 = new EditorVertex();
            v2.Position = new Vector4(p2, 1.0f);
            v2.UV = uv2;
            v2.EditorUV = e2;

            EditorVertex v3 = new EditorVertex();
            v3.Position = new Vector4(p3, 1.0f);
            v3.UV = uv3;
            v3.EditorUV = e3;

            EditorVertex v4 = new EditorVertex();
            v4.Position = new Vector4(p4, 1.0f);
            v4.UV = uv4;
            v4.EditorUV = e4;

            Blocks[x, z].Faces[(int)face].Vertices = new EditorVertex[6];

            // creo una nuova lista dei vertici
            Blocks[x, z].Faces[(int)face].IndicesForSolidBucketsRendering = new List<short>();
            Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations = new List<short>();
            Blocks[x, z].Faces[(int)face].EditorUV = new byte[4];

            Blocks[x, z].Faces[(int)face].Shape = BlockFaceShape.Rectangle;
            Blocks[x, z].Faces[(int)face].Defined = true;
            Blocks[x, z].Faces[(int)face].SplitMode = splitMode;
            Blocks[x, z].Faces[(int)face].Plane = plane;

            //Blocks[x, z].Faces[(int)face].StartVertex = (short)Vertices.Count;
            //for (int i = 0; i < Blocks[x, z].Faces[(int)face].Vertices.Length; i++) Vertices.Add(Blocks[x, z].Faces[(int)face].Vertices[i]);

            // according to texture rotation
            short i1 = -1; // CheckIfVertexExists(p1);
            short i2 = -1; // CheckIfVertexExists(p2);
            short i3 = -1; // CheckIfVertexExists(p3);
            short i4 = -1; // CheckIfVertexExists(p4);

            int gridX1 = (int)(p1.X / 1024);
            int gridZ1 = (int)(p1.Z / 1024);

            for (short i = 0; i < _numVerticesInGrid[gridX1, gridZ1]; i++)
            {
                if (_verticesGrid[gridX1, gridZ1, i].Position.X == p1.X &&
                    _verticesGrid[gridX1, gridZ1, i].Position.Y == p1.Y &&
                    _verticesGrid[gridX1, gridZ1, i].Position.Z == p1.Z)
                {
                    i1 = i;
                    break;
                }
            }

            int gridX2 = (int)(p2.X / 1024);
            int gridZ2 = (int)(p2.Z / 1024);

            for (short i = 0; i < _numVerticesInGrid[gridX2, gridZ2]; i++)
            {
                if (_verticesGrid[gridX2, gridZ2, i].Position.X == p2.X &&
                    _verticesGrid[gridX2, gridZ2, i].Position.Y == p2.Y &&
                    _verticesGrid[gridX2, gridZ2, i].Position.Z == p2.Z)
                {
                    i2 = i;
                    break;
                }
            }

            int gridX3 = (int)(p3.X / 1024);
            int gridZ3 = (int)(p3.Z / 1024);

            for (short i = 0; i < _numVerticesInGrid[gridX3, gridZ3]; i++)
            {
                if (_verticesGrid[gridX3, gridZ3, i].Position.X == p3.X &&
                    _verticesGrid[gridX3, gridZ3, i].Position.Y == p3.Y &&
                    _verticesGrid[gridX3, gridZ3, i].Position.Z == p3.Z)
                {
                    i3 = i;
                    break;
                }
            }

            int gridX4 = (int)(p4.X / 1024);
            int gridZ4 = (int)(p4.Z / 1024);

            for (short i = 0; i < _numVerticesInGrid[gridX4, gridZ4]; i++)
            {
                if (_verticesGrid[gridX4, gridZ4, i].Position.X == p4.X &&
                    _verticesGrid[gridX4, gridZ4, i].Position.Y == p4.Y &&
                    _verticesGrid[gridX4, gridZ4, i].Position.Z == p4.Z)
                {
                    i4 = i;
                    break;
                }
            }

            Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Clear();

            bool isPortal = Blocks[x, z].FloorPortal != -1 || Blocks[x, z].CeilingPortal != -1 || Blocks[x, z].WallPortal != -1;
            Portal portal = null;
            if (isPortal)
            {
                if (Blocks[x, z].FloorPortal != -1)
                    portal = Level.Portals[Blocks[x, z].FloorPortal];
                if (Blocks[x, z].CeilingPortal != -1)
                    portal = Level.Portals[Blocks[x, z].CeilingPortal];
                if (Blocks[x, z].WallPortal != -1)
                    portal = Level.Portals[Blocks[x, z].WallPortal];
            }

            short base1 = (short)(((short)(p1.X / 1024.0f) << 9) + ((short)(p1.Z / 1024.0f) << 4));
            short base2 = (short)(((short)(p2.X / 1024.0f) << 9) + ((short)(p2.Z / 1024.0f) << 4));
            short base3 = (short)(((short)(p3.X / 1024.0f) << 9) + ((short)(p3.Z / 1024.0f) << 4));
            short base4 = (short)(((short)(p4.X / 1024.0f) << 9) + ((short)(p4.Z / 1024.0f) << 4));


            if (i1 == -1)
            {
                int lastVertex = _numVerticesInGrid[gridX1, gridZ1];
                _verticesGrid[gridX1, gridZ1, lastVertex] = v1;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base1 + _numVerticesInGrid[gridX1, gridZ1]));
                i1 = _numVerticesInGrid[gridX1, gridZ1];

                _numVerticesInGrid[gridX1, gridZ1]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base1 + i1));
            }

            if (i2 == -1)
            {
                int lastVertex = _numVerticesInGrid[gridX2, gridZ2];
                _verticesGrid[gridX2, gridZ2, lastVertex] = v2;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base2 + _numVerticesInGrid[gridX2, gridZ2]));
                i2 = _numVerticesInGrid[gridX2, gridZ2];

                _numVerticesInGrid[gridX2, gridZ2]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base2 + i2));
            }

            if (i3 == -1)
            {
                int lastVertex = _numVerticesInGrid[gridX3, gridZ3];
                _verticesGrid[gridX3, gridZ3, lastVertex] = v3;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base3 + _numVerticesInGrid[gridX3, gridZ3]));
                i3 = _numVerticesInGrid[gridX3, gridZ3];

                _numVerticesInGrid[gridX3, gridZ3]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base3 + i3));
            }

            if (i4 == -1)
            {
                int lastVertex = _numVerticesInGrid[gridX4, gridZ4];
                _verticesGrid[gridX4, gridZ4, lastVertex] = v4;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base4 + _numVerticesInGrid[gridX4, gridZ4]));
                i4 = _numVerticesInGrid[gridX4, gridZ4];

                _numVerticesInGrid[gridX4, gridZ4]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base4 + i4));
            }

            i1 = (short)(((short)(p1.X / 1024.0f) << 9) + ((short)(p1.Z / 1024.0f) << 4) + i1);
            i2 = (short)(((short)(p2.X / 1024.0f) << 9) + ((short)(p2.Z / 1024.0f) << 4) + i2);
            i3 = (short)(((short)(p3.X / 1024.0f) << 9) + ((short)(p3.Z / 1024.0f) << 4) + i3);
            i4 = (short)(((short)(p4.X / 1024.0f) << 9) + ((short)(p4.Z / 1024.0f) << 4) + i4);

            if (splitMode == 0)
            {
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i2);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i3);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i1);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i4);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i1);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i3);

                Blocks[x, z].Faces[(int)face].Vertices[0] = v2;
                Blocks[x, z].Faces[(int)face].Vertices[1] = v3;
                Blocks[x, z].Faces[(int)face].Vertices[2] = v1;
                Blocks[x, z].Faces[(int)face].Vertices[3] = v4;
                Blocks[x, z].Faces[(int)face].Vertices[4] = v1;
                Blocks[x, z].Faces[(int)face].Vertices[5] = v3;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i1);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i2);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i4);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i3);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i4);
                Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i2);

                Blocks[x, z].Faces[(int)face].Vertices[0] = v1;
                Blocks[x, z].Faces[(int)face].Vertices[1] = v2;
                Blocks[x, z].Faces[(int)face].Vertices[2] = v4;
                Blocks[x, z].Faces[(int)face].Vertices[3] = v3;
                Blocks[x, z].Faces[(int)face].Vertices[4] = v4;
                Blocks[x, z].Faces[(int)face].Vertices[5] = v2;
            }
        }

        private void AddTriangle(int x, int z, BlockFaces face, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv1, Vector2 uv2,
                                  Vector2 uv3, Vector2 e1, Vector2 e2, Vector2 e3, byte subdivision = 0)
        {
            Plane plane = new Plane(p1, p2, p3);
            Vector3 normal = plane.Normal;

            // creo una nuova lista dei vertici
            Blocks[x, z].Faces[(int)face].IndicesForSolidBucketsRendering = new List<short>();
            Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations = new List<short>();
            Blocks[x, z].Faces[(int)face].EditorUV = new byte[4];

            Blocks[x, z].Faces[(int)face].Shape = BlockFaceShape.Triangle;
            Blocks[x, z].Faces[(int)face].Defined = true;
            Blocks[x, z].Faces[(int)face].SplitMode = subdivision;
            Blocks[x, z].Faces[(int)face].Plane = plane;

            //  Blocks[x, z].Faces[(int)face].StartVertex = (short)Vertices.Count;
            //  for (int i = 0; i < Blocks[x, z].Faces[(int)face].Vertices.Count; i++) Vertices.Add(Blocks[x, z].Faces[(int)face].Vertices[i]);

            Blocks[x, z].Faces[(int)face].Vertices = new EditorVertex[3];

            EditorVertex v1 = new EditorVertex();
            v1.Position = new Vector4(p1, 1.0f);
            v1.UV = uv1;
            v1.EditorUV = e1;

            EditorVertex v2 = new EditorVertex();
            v2.Position = new Vector4(p2, 1.0f);
            v2.UV = uv2;
            v2.EditorUV = e2;

            EditorVertex v3 = new EditorVertex();
            v3.Position = new Vector4(p3, 1.0f);
            v3.UV = uv3;
            v3.EditorUV = e3;

            // according to texture rotation
            short i1 = -1;
            short i2 = -1;
            short i3 = -1;

            int gridX1 = (int)(p1.X / 1024);
            int gridZ1 = (int)(p1.Z / 1024);

            for (short i = 0; i < _numVerticesInGrid[gridX1, gridZ1]; i++)
            {
                if (_verticesGrid[gridX1, gridZ1, i].Position.X == p1.X &&
                    _verticesGrid[gridX1, gridZ1, i].Position.Y == p1.Y &&
                    _verticesGrid[gridX1, gridZ1, i].Position.Z == p1.Z)
                {
                    i1 = i;
                    break;
                }
            }

            int gridX2 = (int)(p2.X / 1024);
            int gridZ2 = (int)(p2.Z / 1024);

            for (short i = 0; i < _numVerticesInGrid[gridX2, gridZ2]; i++)
            {
                if (_verticesGrid[gridX2, gridZ2, i].Position.X == p2.X &&
                    _verticesGrid[gridX2, gridZ2, i].Position.Y == p2.Y &&
                    _verticesGrid[gridX2, gridZ2, i].Position.Z == p2.Z)
                {
                    i2 = i;
                    break;
                }
            }

            int gridX3 = (int)(p3.X / 1024);
            int gridZ3 = (int)(p3.Z / 1024);

            for (short i = 0; i < _numVerticesInGrid[gridX3, gridZ3]; i++)
            {
                if (_verticesGrid[gridX3, gridZ3, i].Position.X == p3.X &&
                    _verticesGrid[gridX3, gridZ3, i].Position.Y == p3.Y &&
                    _verticesGrid[gridX3, gridZ3, i].Position.Z == p3.Z)
                {
                    i3 = i;
                    break;
                }
            }

            Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Clear();

            bool isPortal = Blocks[x, z].FloorPortal != -1 || Blocks[x, z].CeilingPortal != -1 || Blocks[x, z].WallPortal != -1;
            Portal portal = null;
            if (isPortal)
            {
                if (Blocks[x, z].FloorPortal != -1)
                    portal = Level.Portals[Blocks[x, z].FloorPortal];
                if (Blocks[x, z].CeilingPortal != -1)
                    portal = Level.Portals[Blocks[x, z].CeilingPortal];
                if (Blocks[x, z].WallPortal != -1)
                    portal = Level.Portals[Blocks[x, z].WallPortal];
            }

            short base1 = (short)(((short)(p1.X / 1024.0f) << 9) + ((short)(p1.Z / 1024.0f) << 4));
            short base2 = (short)(((short)(p2.X / 1024.0f) << 9) + ((short)(p2.Z / 1024.0f) << 4));
            short base3 = (short)(((short)(p3.X / 1024.0f) << 9) + ((short)(p3.Z / 1024.0f) << 4));

            if (i1 == -1)
            {
                int lastVertex = _numVerticesInGrid[gridX1, gridZ1];
                _verticesGrid[gridX1, gridZ1, lastVertex] = v1;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base1 + _numVerticesInGrid[gridX1, gridZ1]));
                i1 = _numVerticesInGrid[gridX1, gridZ1];

                _numVerticesInGrid[gridX1, gridZ1]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base1 + i1));
            }

            if (i2 == -1)
            {
                int lastVertex = _numVerticesInGrid[gridX2, gridZ2];
                _verticesGrid[gridX2, gridZ2, lastVertex] = v2;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base2 + _numVerticesInGrid[gridX2, gridZ2]));
                i2 = _numVerticesInGrid[gridX2, gridZ2];

                _numVerticesInGrid[gridX2, gridZ2]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base2 + i2));
            }

            if (i3 == -1)
            {
                int lastVertex = _numVerticesInGrid[gridX3, gridZ3];
                _verticesGrid[gridX3, gridZ3, lastVertex] = v3;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base3 + _numVerticesInGrid[gridX3, gridZ3]));
                i3 = _numVerticesInGrid[gridX3, gridZ3];

                _numVerticesInGrid[gridX3, gridZ3]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base3 + i3));
            }

            i1 = (short)(((short)(p1.X / 1024.0f) << 9) + ((short)(p1.Z / 1024.0f) << 4) + i1);
            i2 = (short)(((short)(p2.X / 1024.0f) << 9) + ((short)(p2.Z / 1024.0f) << 4) + i2);
            i3 = (short)(((short)(p3.X / 1024.0f) << 9) + ((short)(p3.Z / 1024.0f) << 4) + i3);

            Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i1);
            Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i2);
            Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations.Add(i3);

            Blocks[x, z].Faces[(int)face].Vertices[0] = v1;
            Blocks[x, z].Faces[(int)face].Vertices[1] = v2;
            Blocks[x, z].Faces[(int)face].Vertices[2] = v3;
        }

        public bool RayIntersectsFace(ref Ray ray, ref BlockFace face, out float distance)
        {
            double epsilon = Math.Cos(MathUtil.DegreesToRadians(90));

            if (face.Shape == BlockFaceShape.Rectangle)
            {
                EditorVertex v1 = Vertices[face.StartVertex + 0];
                Vector3 p1 = new Vector3(v1.Position.X, v1.Position.Y, v1.Position.Z);
                EditorVertex v2 = Vertices[face.StartVertex + 1];
                Vector3 p2 = new Vector3(v2.Position.X, v2.Position.Y, v2.Position.Z);
                EditorVertex v3 = Vertices[face.StartVertex + 2];
                Vector3 p3 = new Vector3(v3.Position.X, v3.Position.Y, v3.Position.Z);
                EditorVertex v4 = Vertices[face.StartVertex + 3];
                Vector3 p4 = new Vector3(v4.Position.X, v4.Position.Y, v4.Position.Z);
                EditorVertex v5 = Vertices[face.StartVertex + 4];
                Vector3 p5 = new Vector3(v5.Position.X, v5.Position.Y, v5.Position.Z);
                EditorVertex v6 = Vertices[face.StartVertex + 5];
                Vector3 p6 = new Vector3(v6.Position.X, v6.Position.Y, v6.Position.Z);

                Plane pl = new Plane(p1, p2, p3);

                if (ray.Intersects(ref p1, ref p2, ref p3, out distance) && Vector3.Dot(-ray.Direction, pl.Normal) >= epsilon)
                    return true;
                if (ray.Intersects(ref p4, ref p5, ref p6, out distance) && Vector3.Dot(-ray.Direction, pl.Normal) >= epsilon)
                    return true;

                return false;
            }
            else
            {
                EditorVertex v1 = Vertices[face.StartVertex + 0];
                Vector3 p1 = new Vector3(v1.Position.X, v1.Position.Y, v1.Position.Z);
                EditorVertex v2 = Vertices[face.StartVertex + 1];
                Vector3 p2 = new Vector3(v2.Position.X, v2.Position.Y, v2.Position.Z);
                EditorVertex v3 = Vertices[face.StartVertex + 2];
                Vector3 p3 = new Vector3(v3.Position.X, v3.Position.Y, v3.Position.Z);

                Plane pl = new Plane(p1, p2, p3);

                if (ray.Intersects(ref p1, ref p2, ref p3, out distance) && Vector3.Dot(-ray.Direction, pl.Normal) >= epsilon)
                    return true;

                return false;
            }
        }

        private List<RayPathPoint> GetRayPath(Vector3 lightPosition, Vector3 vertex)
        {
            List<RayPathPoint> path = new List<RayPathPoint>();

            Vector3 direction = vertex - lightPosition;
            float distance = direction.Length();
            direction.Normalize();
            float currentDistance = 1024.0f;

            int lastX = (int)Math.Floor(lightPosition.X);
            int lastZ = (int)Math.Floor(lightPosition.Z);

            RayPathPoint pnt = new RayPathPoint();
            pnt.X = lastX;
            pnt.Z = lastZ;
            path.Add(pnt);

            while (currentDistance < distance)
            {
                Vector3 point = lightPosition + direction * currentDistance;
                int x = (int)Math.Floor(point.X);
                int z = (int)Math.Floor(point.Z);

                if (x != lastX && z != lastZ)
                {
                    lastX = x;
                    lastZ = z;
                    pnt = new RayPathPoint();
                    pnt.X = lastX;
                    pnt.Z = lastZ;
                    path.Add(pnt);
                }

                currentDistance += 512.0f;
            }

            return path;
        }

        /*   private bool RaytraceLightZ(ref Light light, ref Vector3 p, ref float distance)
           {
               distance = 0;

               float deltaX = 0;
               float deltaY = 0;
               float deltaZ = 0;

               float fractionZ;

               float currentX;
               float currentY;
               float currentZ;

               float minZ = 0;



               if (p.Z>=light.Position.Z)
               {
                   deltaX = p.X - light.Position.X;
                   deltaY = p.Y - light.Position.Y;
                   deltaZ = p.Z - light.Position.Z;


                   //minZ = p.Z;
               }
               else
               {
                   deltaX = light.Position.X - p.X;
                   deltaY = light.Position.Y - p.Y;
                   deltaZ = light.Position.Z - p.Z;

                 //  minZ = light.Position.Z;
               }

               if (deltaZ != 0)
               {
                   // find the two equations of the 3D ray
                   float mXZ = deltaX / deltaZ;
                   float mYZ = deltaY / deltaZ;

                   float qXZ = p.X - p.Z * mXZ;
                   float qYZ = p.Y - p.Z * mYZ;

                   float startX = 0; //= Math.Ceiling()
                   float startY = 0;
                   float startZ = 0;



                   fractionZ = (minZ / 1024 + 1) * 1024 - minZ;

                   currentX = deltaX / (deltaZ + 1) * fractionZ + p.X;
                   currentY = deltaY / (deltaZ + 1) * fractionZ + p.Y;
                   currentZ = (minZ / 1024 + 1) * 1024;

                   if (currentZ<=light.Position.Z)
                   {
                       do
                       {
                           int currentXblock = (int)(currentX / 1024);
                           int currentZblock = (int)(currentZ / 1024);

                           if (currentXblock < 0 || currentZblock >= NumZSectors || currentXblock >= NumXSectors)
                           {
                               if (currentZ == light.Position.Z)
                                   return true;
                           }
                           else
                           {
                               int currentYclick = (int)(currentY / 256);

                               if (currentZblock > 0)
                               {
                                   v17 = *((_DWORD*)pRoom + 29);
                                   pCurrentBlock = v17 + 142 * (currentZblock + currentXblock * numRoomZblocks) - 142;
                                   if (*(_WORD*)(pCurrentBlock + 6)
                                      + ((*(_BYTE*)(v17 + 142 * (currentZblock + currentXblock * numRoomZblocks) - 132)
                                        + (signed int)*(_BYTE*)(pCurrentBlock + 11)) >> 1) > currentYclick
                                    || *(_WORD*)(pCurrentBlock + 8)
                                     + ((*(_BYTE*)(pCurrentBlock + 16) + (signed int)*(_BYTE*)(pCurrentBlock + 17)) >> 1) < currentYclick
                                  || *(_BYTE*)pCurrentBlock & 8 )
                 return false;
                               }
                               if (currentZ == zLightCopy)
                                   return 1;

                           }

                       } while (currentZ <= light.Position.Z);
                   }
               }


               }
               */

        private bool IsPointInShadow(ref Light light, ref Vector3 p, ref float distance)
        {
            return false;
            // Get the path traveled by the ray
            /*List<RayPathPoint> path = GetRayPath(light.Position, p);

            // Get the light direction
            Vector3 direction = p - light.Position;
            direction.Normalize();

            // Initialize the ray
            Ray ray = new Ray(light.Position, direction);
            
            // Now iterate the path, finding the closest intersection to the light.
            float maxDistance = 32768;
            for (int l = 0; l < path.Count; l++)
            {
                int x = path[l].X / 1024;
                int z = path[l].Z / 1024;

                if (x < 0 || z < 0 || x > NumXSectors - 1 || z > NumZSectors - 1) continue;

                int q0 = Blocks[x, z].QAFaces[0];
                int q1 = Blocks[x, z].QAFaces[1];
                int q2 = Blocks[x, z].QAFaces[2];
                int q3 = Blocks[x, z].QAFaces[3];

                int ws0 = Blocks[x, z].QAFaces[0];
                int ws1 = Blocks[x, z].QAFaces[1];
                int ws2 = Blocks[x, z].QAFaces[2];
                int ws3 = Blocks[x, z].QAFaces[3];

                // For each face, text ray - triangle intersection
                for (int f = 0; f < Blocks[x, z].Faces.Length; f++)
                {
                    BlockFace face = Blocks[x, z].Faces[f];

                    if (!face.Defined) continue;

                    EditorVertex v1 = face.Vertices[0];
                    Vector3 p1 = new Vector3(v1.Position.X, v1.Position.Y, v1.Position.Z);
                    EditorVertex v2 = face.Vertices[1];
                    Vector3 p2 = new Vector3(v2.Position.X, v2.Position.Y, v2.Position.Z);
                    EditorVertex v3 = face.Vertices[2];
                    Vector3 p3 = new Vector3(v3.Position.X, v3.Position.Y, v3.Position.Z);

                    // The output distance of the current triangle
                    float outDist = 0;

                    // Now, it's time to inteserct ray with triangles. 
                    // To avoid border failures, before to test the intersection 
                    // I expand the triangle a little along edge normals so it doesn't fail the test
                    float deltaTriangle = 32.0f;
                    
                    if (face.Shape == BlockFaceShape.Triangle)
                    {
                        outDist = 0;

                        Vector3 outPoint = Vector3.Zero;

                        Plane pl = new Plane(p1, p2, p3);
                        Vector3 n12 = Vector3.Cross(p2 - p1, pl.Normal);
                        Vector3 n23 = Vector3.Cross(p3 - p2, pl.Normal);
                        Vector3 n31 = Vector3.Cross(p1 - p3, pl.Normal);

                        n12.Normalize();
                        n23.Normalize();
                        n31.Normalize();

                        p1 = p1 + deltaTriangle * (n12 + n31);
                        p2 = p2 + deltaTriangle * (n12 + n23);
                        p3 = p3 + deltaTriangle * (n23 + n31);

                        if (ray.Intersects(ref p1, ref p2, ref p3, out outPoint))
                        {
                            outPoint.X = (float)Math.Round(outPoint.X);
                            outPoint.Y = (float)Math.Round(outPoint.Y);
                            outPoint.Z = (float)Math.Round(outPoint.Z);

                            outDist = (outPoint - light.Position).Length();
                            if (outDist < maxDistance) maxDistance = outDist;
                        }
                    }
                    else
                    {
                        EditorVertex v4 = face.Vertices[3];
                        Vector3 p4 = new Vector3(v4.Position.X, v4.Position.Y, v4.Position.Z);
                        EditorVertex v5 = face.Vertices[4];
                        Vector3 p5 = new Vector3(v5.Position.X, v5.Position.Y, v5.Position.Z);
                        EditorVertex v6 = face.Vertices[5];
                        Vector3 p6 = new Vector3(v6.Position.X, v6.Position.Y, v6.Position.Z);

                        outDist = 0;

                        Vector3 outPoint = Vector3.Zero;

                        Plane pl = new Plane(p1, p2, p3);
                        Vector3 n12 = Vector3.Cross(p2 - p1, pl.Normal);
                        Vector3 n23 = Vector3.Cross(p3 - p2, pl.Normal);
                        Vector3 n31 = Vector3.Cross(p1 - p3, pl.Normal);

                        n12.Normalize();
                        n23.Normalize();
                        n31.Normalize();

                      p1 = p1 + deltaTriangle * (n12 + n31);
                        p2 = p2 + deltaTriangle * (n12 + n23);
                        p3 = p3 + deltaTriangle * (n23 + n31);

                        if (ray.Intersects(ref p1, ref p2, ref p3, out outPoint))
                        {
                            outPoint.X = (float)Math.Round(outPoint.X);
                            outPoint.Y = (float)Math.Round(outPoint.Y);
                            outPoint.Z = (float)Math.Round(outPoint.Z);

                            outDist = (outPoint - light.Position).Length();
                            if (outDist < maxDistance) maxDistance = outDist;
                        }

                        outDist = 0;
                        outPoint = Vector3.Zero;

                        pl = new Plane(p4, p5, p6);

                        n12 = Vector3.Cross(p2 - p1, pl.Normal);
                        n23 = Vector3.Cross(p3 - p2, pl.Normal);
                        n31 = Vector3.Cross(p1 - p3, pl.Normal);

                        n12.Normalize();
                        n23.Normalize();
                        n31.Normalize();


                       p4 = p4 + deltaTriangle * (n12 + n31);
                        p5 = p5 + deltaTriangle * (n12 + n23);
                        p6 = p6 + deltaTriangle * (n23 + n31);

                        if (ray.Intersects(ref p4, ref p5, ref p6, out outPoint))
                        {
                            outDist = (outPoint - light.Position).Length();
                            if (outDist < maxDistance) maxDistance = outDist;
                        }
                    }
                }
            }

            if ((maxDistance - distance) < 0.0f) return true;

            return false;*/
        }

        private bool RayTraceCheckFloorCeiling(int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int currentX = (x / 1024) - (x > xLight ? 1 : 0);
            int currentZ = (z / 1024) - (z > zLight ? 1 : 0);

            int floorMin = GetLowestFloorCorner(currentX, currentZ);
            int ceilingMax = Ceiling + GetHighestCeilingCorner(currentX, currentZ);

            return (floorMin <= y / 256 && ceilingMax >= y / 256);
        }

        private bool RayTraceX(int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int deltaX;
            int deltaY;
            int deltaZ;

            int minX;
            int maxX;

            int fracX;

            int currentX;
            int currentY;
            int currentZ;

            int currentXblock;
            int currentZblock;
            int currentYclick;

            yLight = -yLight;
            y = -y;

            int yPoint = y;
            int zPoint = z;

            if (x <= xLight)
            {
                deltaX = xLight - x;
                deltaY = yLight - y;
                deltaZ = zLight - z;

                minX = x;
                maxX = xLight;
            }
            else
            {
                deltaX = x - xLight;
                deltaY = y - yLight;
                deltaZ = z - zLight;

                minX = xLight;
                maxX = x;

                yPoint = yLight;
                zPoint = zLight;
            }

            // deltaY *= -1;

            if (deltaX != 0)
            {
                fracX = (((minX >> 10) + 1) << 10) - minX;
                currentX = ((minX >> 10) + 1) << 10;
                currentZ = deltaZ * fracX / (deltaX + 1) + zPoint;
                currentY = deltaY * fracX / (deltaX + 1) + yPoint;

                if (currentX <= maxX)
                {
                    do
                    {
                        currentXblock = currentX / 1024;
                        currentZblock = currentZ / 1024;

                        if (currentZblock < 0 || currentXblock >= NumXSectors || currentZblock >= NumZSectors)
                        {
                            if (currentX == maxX)
                                return true;
                        }
                        else
                        {
                            currentYclick = currentY / -256;

                            if (currentXblock > 0)
                            {
                                Block currentBlock = Blocks[currentXblock - 1, currentZblock];

                                if (((currentBlock.QAFaces[0] + currentBlock.QAFaces[3]) / 2 > currentYclick) ||
                                    (Ceiling + (currentBlock.WSFaces[0] + currentBlock.WSFaces[3]) / 2 < currentYclick) ||
                                    currentBlock.Type == BlockType.Wall)
                                {
                                    return false;
                                }
                            }

                            if (currentX == maxX)
                            {
                                return true;
                            }

                            if (currentXblock >= 0)
                            {
                                Block currentBlock = Blocks[currentXblock - 1, currentZblock];
                                Block nextBlock = Blocks[currentXblock, currentZblock];

                                if (((currentBlock.QAFaces[2] + currentBlock.QAFaces[1]) / 2 > currentYclick) ||
                                    (Ceiling + (currentBlock.WSFaces[2] + currentBlock.WSFaces[1]) / 2 < currentYclick) ||
                                    currentBlock.Type == BlockType.Wall ||
                                    ((nextBlock.QAFaces[0] + nextBlock.QAFaces[3]) / 2 > currentYclick) ||
                                    (Ceiling + (nextBlock.WSFaces[0] + nextBlock.WSFaces[3]) / 2 < currentYclick) ||
                                    nextBlock.Type == BlockType.Wall)
                                {
                                    return false;
                                }
                            }
                        }

                        currentX += 1024;
                        currentZ += (deltaZ << 10) / (deltaX + 1);
                        currentY += (deltaY << 10) / (deltaX + 1);
                    }
                    while (currentX <= maxX);
                }
            }

            return true;
        }

        private bool RayTraceZ(int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int deltaX;
            int deltaY;
            int deltaZ;

            int minZ;
            int maxZ;

            int fracZ;

            int currentX;
            int currentY;
            int currentZ;

            int currentXblock;
            int currentZblock;
            int currentYclick;

            yLight = -yLight;
            y = -y;

            int yPoint = y;
            int xPoint = x;

            if (z <= zLight)
            {
                deltaX = xLight - x;
                deltaY = yLight - y;
                deltaZ = zLight - z;

                minZ = z;
                maxZ = zLight;
            }
            else
            {
                deltaX = x - xLight;
                deltaY = y - yLight;
                deltaZ = z - zLight;

                minZ = zLight;
                maxZ = z;

                xPoint = xLight;
                yPoint = yLight;
            }

            //deltaY *= -1;

            if (deltaZ != 0)
            {
                fracZ = (((minZ >> 10) + 1) << 10) - minZ;
                currentZ = ((minZ >> 10) + 1) << 10;
                currentX = deltaX * fracZ / (deltaZ + 1) + xPoint;
                currentY = deltaY * fracZ / (deltaZ + 1) + yPoint;

                if (currentZ <= maxZ)
                {
                    do
                    {
                        currentXblock = currentX / 1024;
                        currentZblock = currentZ / 1024;

                        if (currentXblock < 0 || currentZblock >= NumZSectors || currentXblock >= NumXSectors)
                        {
                            if (currentZ == maxZ)
                                return true;
                        }
                        else
                        {
                            currentYclick = currentY / -256;

                            if (currentZblock > 0)
                            {
                                Block currentBlock = Blocks[currentXblock, currentZblock - 1];

                                if (((currentBlock.QAFaces[2] + currentBlock.QAFaces[3]) / 2 > currentYclick) ||
                                    (Ceiling + (currentBlock.WSFaces[2] + currentBlock.WSFaces[3]) / 2 < currentYclick) ||
                                    currentBlock.Type == BlockType.Wall)
                                {
                                    return false;
                                }
                            }

                            if (currentZ == maxZ)
                            {
                                return true;
                            }

                            if (currentZblock >= 0)
                            {
                                Block currentBlock = Blocks[currentXblock, currentZblock - 1];
                                Block nextBlock = Blocks[currentXblock, currentZblock];

                                if (((currentBlock.QAFaces[0] + currentBlock.QAFaces[1]) / 2 > currentYclick) ||
                                    (Ceiling + (currentBlock.WSFaces[0] + currentBlock.WSFaces[1]) / 2 < currentYclick) ||
                                    currentBlock.Type == BlockType.Wall ||
                                    ((nextBlock.QAFaces[2] + nextBlock.QAFaces[3]) / 2 > currentYclick) ||
                                    (Ceiling + (nextBlock.WSFaces[2] + nextBlock.WSFaces[3]) / 2 < currentYclick) ||
                                    nextBlock.Type == BlockType.Wall)
                                {
                                    return false;
                                }
                            }
                        }

                        currentZ += 1024;
                        currentX += (deltaX << 10) / (deltaZ + 1);
                        currentY += (deltaY << 10) / (deltaZ + 1);
                    }
                    while (currentZ <= maxZ);
                }
            }

            return true;
        }

        public void CalculateLighting(int x, int z, int f)
        {
            BlockFace face = Blocks[x, z].Faces[f];

            // Initialize the vertex color with ambiental light
            int r = AmbientLight.R;
            int g = AmbientLight.G;
            int b = AmbientLight.B;

            Vector3 n = face.Plane.Normal;

            for (int v = 0; v < face.IndicesForLightingCalculations.Count; v++)
            {
                int theX = (face.IndicesForLightingCalculations[v] >> 9) & 0x1f;
                int theZ = (face.IndicesForLightingCalculations[v] >> 4) & 0x1f;
                int theIndex = face.IndicesForLightingCalculations[v] & 0x0f;

                Vector3 p = new Vector3(_verticesGrid[theX, theZ, theIndex].Position.X,
                                        _verticesGrid[theX, theZ, theIndex].Position.Y,
                                        _verticesGrid[theX, theZ, theIndex].Position.Z);

                r = AmbientLight.R;
                g = AmbientLight.G;
                b = AmbientLight.B;

                // Get all nearest lights. Maybe in the future limit to max number of lights?
                List<Light> lights = GetNearestLights(p);

                for (int i = 0; i < lights.Count; i++)
                {
                    Light light = lights[i];

                    if (light.Type == LightType.Light || light.Type == LightType.Shadow)
                    {
                        // Get the distance between light and vertex
                        float distance = (float)Math.Abs((p - lights[i].Position).Length());

                        // If distance is greater than light out radius, then skip this light
                        if (distance > light.Out * 1024.0f)
                            continue;

                        // Calculate light diffuse value
                        int diffuse = (int)(light.Intensity * 8192);

                        // Calculate the length squared of the normal vector
                        float dotN = Vector3.Dot(n, n);

                        // Do raytracing
                        if (dotN <= 0 ||
                            !RayTraceCheckFloorCeiling((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceX((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceZ((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                        {
                            continue;
                        }

                        // Calculate the attenuation
                        float attenuaton = 1.0f;
                        attenuaton = (float)(light.Out * 1024.0f - distance) / (light.Out * 1024.0f - light.In * 1024.0f);
                        if (attenuaton > 1.0f)
                            attenuaton = 1.0f;
                        if (attenuaton <= 0.0f)
                            continue;

                        // Calculate final light color
                        int finalIntensity = (int)(dotN * attenuaton * diffuse);

                        r += finalIntensity * light.Color.R / 8192;
                        g += finalIntensity * light.Color.G / 8192;
                        b += finalIntensity * light.Color.B / 8192;
                    }
                    else if (light.Type == LightType.Effect)
                    {
                        float diffuse = light.Intensity;

                        int x1 = (int)(Math.Floor(light.Position.X / 1024.0f) * 1024);
                        int z1 = (int)(Math.Floor(light.Position.Z / 1024.0f) * 1024);
                        int x2 = (int)(Math.Ceiling(light.Position.X / 1024.0f) * 1024);
                        int z2 = (int)(Math.Ceiling(light.Position.Z / 1024.0f) * 1024);

                        // TODO: winroomedit was supporting effect lights placed on vertical faces and effects light was applied to owning face
                        if (((p.X == x1 && p.Z == z1) || (p.X == x1 && p.Z == z2) || (p.X == x2 && p.Z == z1) ||
                            (p.X == x2 && p.Z == z2)) && p.Y <= light.Position.Y)
                        {
                            int finalIntensity = (int)(light.Intensity * 8192 * 0.25f);

                            r += finalIntensity * light.Color.R / 8192;
                            g += finalIntensity * light.Color.G / 8192;
                            b += finalIntensity * light.Color.B / 8192;
                        }
                    }
                    else if (light.Type == LightType.Sun)
                    {
                        // Do raytracing now for saving CPU later
                        if (!RayTraceCheckFloorCeiling((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceX((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceZ((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                        {
                            continue;
                        }

                        // Calculate the light direction
                        Vector3 lightDirection = Vector3.Zero;

                        lightDirection.X = (float)(Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                        lightDirection.Y = (float)(Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                        lightDirection.Z = (float)(Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));

                        lightDirection.Normalize();

                        // calcolo la luce diffusa
                        float diffuse = -Vector3.Dot(lightDirection, n);

                        if (diffuse <= 0)
                            continue;

                        if (diffuse > 1)
                            diffuse = 1.0f;


                        int finalIntensity = (int)(diffuse * light.Intensity * 8192);
                        if (finalIntensity < 0)
                            continue;

                        r += finalIntensity * light.Color.R / 8192;
                        g += finalIntensity * light.Color.G / 8192;
                        b += finalIntensity * light.Color.B / 8192;
                    }
                    else if (light.Type == LightType.Spot)
                    {
                        // Calculate the ray from light to vertex
                        Vector3 lightVector = (p - lights[i].Position);
                        lightVector.Y = -lightVector.Y;
                        lightVector.Normalize();

                        // Get the distance between light and vertex
                        float distance = (float)Math.Abs((p - lights[i].Position).Length());

                        // If distance is greater than light length, then skip this light
                        if (distance > light.Cutoff * 1024.0f)
                            continue;

                        // Calculate the light direction
                        Vector3 lightDirection = Vector3.Zero;

                        lightDirection.X = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                        lightDirection.Y = (float)(Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                        lightDirection.Z = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));

                        lightDirection.Normalize();

                        // Calculate the cosines values for In, Out
                        double d = -Vector3.Dot(lightVector, lightDirection);
                        double cosI2 = (double)Math.Cos(MathUtil.DegreesToRadians(light.In));
                        double cosO2 = (double)Math.Cos(MathUtil.DegreesToRadians(light.Out));

                        if (d < cosO2)
                            continue;

                        if (!RayTraceCheckFloorCeiling((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceX((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceZ((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                        {
                            continue;
                        }

                        // Calculate light diffuse value
                        int diffuse = (int)(light.Intensity * 8192);


                        float range = light.Out - light.In;
                        range *= 1024.0f;

                        float factor = (float)(1.0f - (d - cosI2) / (cosO2 - cosI2));
                        if (factor > 1.0f)
                            factor = 1.0f;
                        if (factor <= 0.0f)
                            continue;

                        float attenuation = 1.0f;
                        if (distance >= light.Len * 1024.0f)
                            attenuation = (float)(1.0f - (distance - light.Len * 1024.0f) / (light.Cutoff * 1024.0f - light.Len * 1024.0f));

                        if (attenuation > 1.0f)
                            attenuation = 1.0f;
                        if (attenuation < 0.0f)
                            continue;

                        n.Y = -n.Y;

                        float dot1 = Vector3.Dot(lightDirection, n);
                        if (dot1 < 0.0f)
                            continue;
                        if (dot1 > 1.0f)
                            dot1 = 1.0f;

                        int finalIntensity = (int)(attenuation * dot1 * factor * light.Intensity * 8192);

                        r += finalIntensity * light.Color.R / 8192;
                        g += finalIntensity * light.Color.G / 8192;
                        b += finalIntensity * light.Color.B / 8192;
                    }
                }

                // Normalization
                int ind = theIndex; // v; // face.Indices[v];
                EditorVertex vertex = _verticesGrid[theX, theZ, ind];

                if (r < 0)
                    r = 0;
                if (g < 0)
                    g = 0;
                if (b < 0)
                    b = 0;

                vertex.FaceColor.X = (r + _iterations[theX, theZ, ind] * vertex.FaceColor.X) / (_iterations[theX, theZ, ind] + 1);
                vertex.FaceColor.Y = (g + _iterations[theX, theZ, ind] * vertex.FaceColor.Y) / (_iterations[theX, theZ, ind] + 1);
                vertex.FaceColor.Z = (b + _iterations[theX, theZ, ind] * vertex.FaceColor.Z) / (_iterations[theX, theZ, ind] + 1);
                vertex.FaceColor.W = 255.0f;

                _iterations[theX, theZ, ind]++;

                _verticesGrid[theX, theZ, ind] = vertex;
            }

            return;
        }

        private List<Light> GetNearestLights(Vector3 p)
        {
            List<Light> lights = new List<Light>();

            for (int i = 0; i < Lights.Count; i++)
            {
                Light light = Lights[i];

                if ((light.Type == LightType.Light || light.Type == LightType.Shadow || light.Type == LightType.Effect) &&
                      Math.Abs((p - light.Position).Length()) + 64.0f <= light.Out * 1024.0f)
                {
                    lights.Add(light);
                }

                if (light.Type == LightType.Spot && Math.Abs((p - light.Position).Length()) + 64.0f <= light.Cutoff * 1024.0f)
                {
                    lights.Add(light);
                }

                if (light.Type == LightType.Sun)
                {
                    lights.Add(light);
                }
            }

            return lights;
        }

        public void UpdateBuffers()
        {
            if (Vertices.Count == 0)
                return;

            if (VertexBuffer == null)
            {
                VertexBuffer = Buffer.New(Editor.Instance.GraphicsDevice, Vertices.ToArray(), BufferFlags.VertexBuffer);
            }
            else
            {
                if (VertexBuffer.ElementCount < Vertices.Count)
                {
                    VertexBuffer.Dispose();
                    VertexBuffer = Buffer.New(Editor.Instance.GraphicsDevice, Vertices.ToArray(), BufferFlags.VertexBuffer);
                }

                VertexBuffer.SetData<EditorVertex>(Vertices.ToArray());
            }
        }

        public Matrix Transform
        {
            get
            {
                return Matrix.Translation(new Vector3(Position.X * 1024.0f, Position.Y * 256.0f, Position.Z * 1024.0f));
            }
        }

        public void Resize(byte numXsectors, byte numZsectors)
        {
            NumXSectors = (byte)numXsectors;
            NumZSectors = (byte)NumZSectors;

        }

        public int GetHighestCorner()
        {
            int max = int.MinValue;

            for (int x = 1; x < NumXSectors - 1; x++)
            {
                for (int z = 1; z < NumZSectors - 1; z++)
                {
                    if (Blocks[x, z].Type == BlockType.Wall)
                        continue;

                    if (Blocks[x, z].WSFaces[0] > max)
                        max = Blocks[x, z].WSFaces[0];
                    if (Blocks[x, z].WSFaces[1] > max)
                        max = Blocks[x, z].WSFaces[1];
                    if (Blocks[x, z].WSFaces[2] > max)
                        max = Blocks[x, z].WSFaces[2];
                    if (Blocks[x, z].WSFaces[3] > max)
                        max = Blocks[x, z].WSFaces[3];
                }
            }

            return (Ceiling + max);
        }

        public int GetLowestCorner()
        {
            int min = int.MaxValue;

            for (int x = 1; x < NumXSectors - 1; x++)
            {
                for (int z = 1; z < NumZSectors - 1; z++)
                {
                    if (Blocks[x, z].Type == BlockType.Wall)
                        continue;

                    if (Blocks[x, z].QAFaces[0] < min)
                        min = Blocks[x, z].QAFaces[0];
                    if (Blocks[x, z].QAFaces[1] < min)
                        min = Blocks[x, z].QAFaces[1];
                    if (Blocks[x, z].QAFaces[2] < min)
                        min = Blocks[x, z].QAFaces[2];
                    if (Blocks[x, z].QAFaces[3] < min)
                        min = Blocks[x, z].QAFaces[3];
                }
            }

            return min;
        }

        public int GetHighestCorner(int x1, int z1, int x2, int z2)
        {
            int max = int.MinValue;

            for (int x = x1; x < x2; x++)
            {
                for (int z = z1; z < z2; z++)
                {
                    if (Blocks[x, z].Type == BlockType.Wall)
                        continue;

                    if (Blocks[x, z].WSFaces[0] > max)
                        max = Blocks[x, z].WSFaces[0];
                    if (Blocks[x, z].WSFaces[1] > max)
                        max = Blocks[x, z].WSFaces[1];
                    if (Blocks[x, z].WSFaces[2] > max)
                        max = Blocks[x, z].WSFaces[2];
                    if (Blocks[x, z].WSFaces[3] > max)
                        max = Blocks[x, z].WSFaces[3];
                }
            }

            return (Ceiling + max);
        }

        public int GetLowestCorner(int x1, int z1, int x2, int z2)
        {
            int min = int.MaxValue;

            for (int x = x1; x < x2; x++)
            {
                for (int z = z1; z < z2; z++)
                {
                    if (Blocks[x, z].Type == BlockType.Wall)
                        continue;

                    if (Blocks[x, z].QAFaces[0] < min)
                        min = Blocks[x, z].QAFaces[0];
                    if (Blocks[x, z].QAFaces[1] < min)
                        min = Blocks[x, z].QAFaces[1];
                    if (Blocks[x, z].QAFaces[2] < min)
                        min = Blocks[x, z].QAFaces[2];
                    if (Blocks[x, z].QAFaces[3] < min)
                        min = Blocks[x, z].QAFaces[3];
                }
            }

            return min;
        }

        public int GetHighestCorner(int x, int z)
        {
            int max = int.MinValue;

            if (Blocks[x, z].WSFaces[0] > max)
                max = Blocks[x, z].WSFaces[0];
            if (Blocks[x, z].WSFaces[1] > max)
                max = Blocks[x, z].WSFaces[1];
            if (Blocks[x, z].WSFaces[2] > max)
                max = Blocks[x, z].WSFaces[2];
            if (Blocks[x, z].WSFaces[3] > max)
                max = Blocks[x, z].WSFaces[3];

            return (Ceiling + max);
        }

        public int GetLowestCorner(int x, int z)
        {
            int min = int.MaxValue;

            if (Blocks[x, z].QAFaces[0] < min)
                min = Blocks[x, z].QAFaces[0];
            if (Blocks[x, z].QAFaces[1] < min)
                min = Blocks[x, z].QAFaces[1];
            if (Blocks[x, z].QAFaces[2] < min)
                min = Blocks[x, z].QAFaces[2];
            if (Blocks[x, z].QAFaces[3] < min)
                min = Blocks[x, z].QAFaces[3];

            return min;
        }

        public int GetHighestFloorCorner(int x, int z)
        {
            int max = int.MinValue;

            if (Blocks[x, z].QAFaces[0] > max)
                max = Blocks[x, z].QAFaces[0];
            if (Blocks[x, z].QAFaces[1] > max)
                max = Blocks[x, z].QAFaces[1];
            if (Blocks[x, z].QAFaces[2] > max)
                max = Blocks[x, z].QAFaces[2];
            if (Blocks[x, z].QAFaces[3] > max)
                max = Blocks[x, z].QAFaces[3];

            return max;
        }

        public int GetLowestFloorCorner(int x, int z)
        {
            int min = int.MaxValue;

            if (Blocks[x, z].QAFaces[0] < min)
                min = Blocks[x, z].QAFaces[0];
            if (Blocks[x, z].QAFaces[1] < min)
                min = Blocks[x, z].QAFaces[1];
            if (Blocks[x, z].QAFaces[2] < min)
                min = Blocks[x, z].QAFaces[2];
            if (Blocks[x, z].QAFaces[3] < min)
                min = Blocks[x, z].QAFaces[3];

            return min;
        }

        public int GetMeanFloorHeight(int x, int z)
        {
            int sum = 0;

            for (int i = 0; i < 4; i++)
                sum += Blocks[x, z].QAFaces[i];

            sum *= 256;
            sum /= 4;

            return sum;
        }

        public int GetLowestCeilingCorner(int x, int z)
        {
            int min = int.MaxValue;

            if (Blocks[x, z].WSFaces[0] < min)
                min = Blocks[x, z].WSFaces[0];
            if (Blocks[x, z].WSFaces[1] < min)
                min = Blocks[x, z].WSFaces[1];
            if (Blocks[x, z].WSFaces[2] < min)
                min = Blocks[x, z].WSFaces[2];
            if (Blocks[x, z].WSFaces[3] < min)
                min = Blocks[x, z].WSFaces[3];

            return min;
        }

        public int GetMeanCeilingHeight(int x, int z)
        {
            int sum = 0;

            for (int i = 0; i < 4; i++)
                sum += Blocks[x, z].WSFaces[i];

            sum *= 256;
            sum /= 4;

            return sum;
        }

        public int GetHighestCeilingCorner(int x, int z)
        {
            int max = int.MinValue;

            if (Blocks[x, z].WSFaces[0] > max)
                max = Blocks[x, z].WSFaces[0];
            if (Blocks[x, z].WSFaces[1] > max)
                max = Blocks[x, z].WSFaces[1];
            if (Blocks[x, z].WSFaces[2] > max)
                max = Blocks[x, z].WSFaces[2];
            if (Blocks[x, z].WSFaces[3] > max)
                max = Blocks[x, z].WSFaces[3];

            return max;
        }

        // TODO: check if can be safely removed
        public void AdjustObjectsHeight()
        {
            return;
            /*
            for (int z = 0; z < NumZSectors; z++)
            {
                for (int x = 0; x < NumXSectors; x++)
                {
                    int highest = GetHighestFloorCorner(x, z);
                    highest *= 256;

                    int lowest = Ceiling + GetLowestCeilingCorner(x, z);
                    lowest *= 256;

                    int meanFloor = GetMeanFloorHeight(x, z);
                    int meanCeiling = GetMeanCeilingHeight(x, z);

                    for (int i = 0; i < Lights.Count; i++)
                    {
                        if (Lights[i].Position.Y < highest && (int)Math.Floor(Lights[i].Position.X / 1024.0f) == x &&
                            (int)Math.Floor(Lights[i].Position.Z / 1024.0f) == z)
                        {
                            Light light = Lights[i];
                            light.Position = new Vector3(light.Position.X, meanFloor, light.Position.Z);
                            Lights[i] = light;
                        }

                        if (Lights[i].Position.Y > lowest && (int)Math.Floor(Lights[i].Position.X / 1024.0f) == x &&
                            (int)Math.Floor(Lights[i].Position.Z / 1024.0f) == z)
                        {
                            Light light = Lights[i];
                            light.Position = new Vector3(light.Position.X, meanCeiling, light.Position.Z);
                            Lights[i] = light;
                        }
                    }

                    for (int i = 0; i < Moveables.Count; i++)
                    {
                        IObjectInstance instance = Level.Objects[Moveables[i]];

                        int meanFloor2 = GetLowestFloorCorner((int)(instance.Position.X / 1024.0f), 
                                                           (int)(instance.Position.Z / 1024.0f));
                        int meanCeiling2 = GetHighestCeilingCorner((int)(instance.Position.X / 1024.0f),
                                                           (int)(instance.Position.Z / 1024.0f));

                        if (instance.Position.Y < meanFloor2 && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, meanFloor2, instance.Position.Z);
                            Level.Objects[Moveables[i]] = (MoveableInstance)instance;
                        }

                        //if (instance.Position.Y > meanCeiling2 && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                        //   (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        //{
                        //    instance.Position = new Vector3(instance.Position.X, meanCeiling, instance.Position.Z);
                        //    Level.Objects[Moveables[i]] = (MoveableInstance)instance;
                        //}
                        //if (instance.Position.Y < highest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                        //    (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        //{
                        //    instance.Position = new Vector3(instance.Position.X, meanFloor, instance.Position.Z);
                        //    Level.Objects[Moveables[i]] = (MoveableInstance)instance;
                        //}

                        //if (instance.Position.Y > lowest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                        //   (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        //{
                        //    instance.Position = new Vector3(instance.Position.X, meanCeiling, instance.Position.Z);
                        //    Level.Objects[Moveables[i]] = (MoveableInstance)instance;
                        //}
                    }

                    for (int i = 0; i < StaticMeshes.Count; i++)
                    {
                        IObjectInstance instance = Level.Objects[StaticMeshes[i]];

                        int meanFloor2 = GetLowestFloorCorner((int)(instance.Position.X / 1024.0f),
                                                           (int)(instance.Position.Z / 1024.0f));
                        int meanCeiling2 = GetHighestCeilingCorner((int)(instance.Position.X / 1024.0f),
                                                           (int)(instance.Position.Z / 1024.0f));

                        if (instance.Position.Y < meanFloor2 && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, meanFloor, instance.Position.Z);
                            Level.Objects[StaticMeshes[i]] = (StaticMeshInstance)instance;
                        }

                        //if (instance.Position.Y > GetHighestCeilingCorner && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                        //   (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        //{
                        //    instance.Position = new Vector3(instance.Position.X, meanCeiling, instance.Position.Z);
                        //    Level.Objects[StaticMeshes[i]] = (StaticMeshInstance)instance;
                        //}
                    }

                    for (int i = 0; i < Sinks.Count; i++)
                    {
                        IObjectInstance instance = Level.Objects[Sinks[i]];

                        if (instance.Position.Y < highest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, meanFloor, instance.Position.Z);
                            Level.Objects[Sinks[i]] = instance;
                        }

                        if (instance.Position.Y > lowest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, meanCeiling, instance.Position.Z);
                            Level.Objects[Sinks[i]] = instance;
                        }
                    }

                    for (int i = 0; i < Cameras.Count; i++)
                    {
                        IObjectInstance instance = Level.Objects[Cameras[i]];

                        if (instance.Position.Y < highest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, highest, instance.Position.Z);
                            Level.Objects[Cameras[i]] = instance;
                        }

                        if (instance.Position.Y > lowest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, lowest, instance.Position.Z);
                            Level.Objects[Cameras[i]] = instance;
                        }
                    }

                    for (int i = 0; i < FlyByCameras.Count; i++)
                    {
                        IObjectInstance instance = Level.Objects[FlyByCameras[i]];

                        if (instance.Position.Y < highest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, highest, instance.Position.Z);
                            Level.Objects[FlyByCameras[i]] = instance;
                        }

                        if (instance.Position.Y > lowest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, lowest, instance.Position.Z);
                            Level.Objects[FlyByCameras[i]] = instance;
                        }
                    }

                    for (int i = 0; i < SoundSources.Count; i++)
                    {
                        IObjectInstance instance = Level.Objects[SoundSources[i]];

                        if (instance.Position.Y < highest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                            (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, highest, instance.Position.Z);
                            Level.Objects[SoundSources[i]] = instance;
                        }

                        if (instance.Position.Y > lowest && (int)Math.Floor(instance.Position.X / 1024.0f) == x &&
                           (int)Math.Floor(instance.Position.Z / 1024.0f) == z)
                        {
                            instance.Position = new Vector3(instance.Position.X, lowest, instance.Position.Z);
                            Level.Objects[SoundSources[i]] = instance;
                        }
                    }
                }
            }*/
        }
    }
}
