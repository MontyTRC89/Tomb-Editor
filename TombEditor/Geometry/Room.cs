using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using TombEditor.Compilers;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombEditor.Geometry
{
    public class Room
    {
        public tr_room _compiled;

        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public short Ceiling { get; set; }
        public byte NumXSectors { get; set; }
        public byte NumZSectors { get; set; }
        public System.Drawing.Color AmbientLight { get; set; } = System.Drawing.Color.FromArgb(255, 32, 32, 32);
        public Block[,] Blocks { get; set; }
        public Buffer<EditorVertex> VertexBuffer { get; private set; }
        private List<EditorVertex> Vertices { get; set; }
        public List<int> Moveables { get; private set; } = new List<int>();
        public List<int> StaticMeshes { get; private set; } = new List<int>();
        public List<Light> Lights { get; private set; } = new List<Light>();
        public List<int> SoundSources { get; private set; } = new List<int>();
        public List<int> Sinks { get; private set; } = new List<int>();
        public List<int> Cameras { get; private set; } = new List<int>();
        public List<int> FlyByCameras { get; private set; } = new List<int>();
        public List<int> Portals { get; private set; } = new List<int>();
        public Room BaseRoom { get; set; }
        public bool Flipped { get; set; }
        public bool Visited { get; set; }
        public List<EditorVertex> OptimizedVertices { get; set; }
        public Room AlternateRoom { get; set; }
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
        private Level Level { get; }
        public Vector3 Centre { get; private set; }
        public bool ExcludeFromPathFinding { get; set; }

        public Room(Level level)
        {
            Level = level;

            InitializeVerticesGrid();
        }

        public void Init(int posx, int posy, int posz, byte numXSectors, byte numZSectors, short ceiling)
        {
            System.Diagnostics.Debug.Assert(numXSectors > 0);
            System.Diagnostics.Debug.Assert(numZSectors > 0);
            
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
            AlternateRoom = null;
            BaseRoom = null;

            Blocks = new Block[numXSectors, numZSectors];

            for (int x = 0; x < NumXSectors; x++)
            {
                for (int z = 0; z < NumZSectors; z++)
                {
                    Block block;
                    if (x == 0 || z == 0 || x == NumXSectors - 1 || z == NumZSectors - 1)
                    {
                        block = new Block(BlockType.BorderWall, BlockFlags.None);
                    }
                    else
                    {
                        block = new Block(BlockType.Floor, BlockFlags.None);
                    }

                    Blocks[x, z] = block;
                }
            }

            InitializeVerticesGrid();

            BuildGeometry();
            CalculateLightingForThisRoom();
            UpdateBuffers();
        }

        public EditorVertex[,,] VerticesGrid { get; private set; }

        public byte[,] NumVerticesInGrid { get; private set; }

        public Vector2 SectorPos
        {
            get { return new Vector2(Position.X, Position.Z); }
            set { Position = new Vector3(value.X, Position.Y, value.Y); }
        }

        public void InitializeVerticesGrid()
        {
            VerticesGrid = new EditorVertex[NumXSectors, NumZSectors, 16];
            NumVerticesInGrid = new byte[NumXSectors, NumZSectors];
        }

        public void CalculateLightingForThisRoom()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // This is used for iterative mean
            byte[,,] iterations = new byte[NumXSectors, NumZSectors, 16];

            for (int x = 0; x < NumXSectors; x++)
            {
                for (int z = 0; z < NumZSectors; z++)
                {
                    for (int f = 0; f < 29; f++)
                    {
                        if (Blocks[x, z].Faces[f].Defined)
                        {
                            // Calculate the lighting of vertices in X, Z of face f
                            CalculateLighting(iterations, x, z, f);
                        }
                    }
                }
            }

            // Apply the face color to editor vertices
            for (int i = 0; i < Vertices.Count; i++)
            {
                int x = (int)(Vertices[i].Position.X / 1024);
                int z = (int)(Vertices[i].Position.Z / 1024);

                for (int j = 0; j < NumVerticesInGrid[x, z]; j++)
                {
                    if (VerticesGrid[x, z, j].Position.Y != Vertices[i].Position.Y)
                        continue;

                    var v = Vertices[i];
                    v.FaceColor = VerticesGrid[x, z, j].FaceColor;
                    Vertices[i] = v;

                    break;
                }
            }

            watch.Stop();
        }

        private void CalculateCeilingSlope(int x, int z, int ws0, int ws1, int ws2, int ws3)
        {
            if (ws0 == ws1 && ws1 == ws2 && ws2 == ws3)
                return;

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

        private void CalculateFloorSlope(int x, int z, int qa0, int qa1, int qa2, int qa3)
        {
            if (qa0 == qa1 && qa1 == qa2 && qa2 == qa3)
                return;

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

        public void BuildGeometry()
        {
            BuildGeometry(0, NumXSectors - 1, 0, NumZSectors - 1);
        }

        public void BuildGeometry(int xMin, int xMax, int zMin, int zMax)
        {
            Vertices = new List<EditorVertex>();
            OptimizedVertices = new List<EditorVertex>();
            Centre = new Vector3(0.0f, 0.0f, 0.0f);

            var e1 = new Vector2(0.0f, 0.0f);
            var e2 = new Vector2(1.0f, 0.0f);
            var e3 = new Vector2(1.0f, 1.0f);
            var e4 = new Vector2(0.0f, 1.0f);

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
                    }

                    NumVerticesInGrid[x, z] = 0;
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
                            var portal = FindPortal(x, z, PortalDirection.South);
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (Flipped && BaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
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
                            var portal = FindPortal(x, z, PortalDirection.North);
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (Flipped && BaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
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
                            var portal = FindPortal(x, z, PortalDirection.West);
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (Flipped && BaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
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
                            var portal = FindPortal(x, z, PortalDirection.East);
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (Flipped && BaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
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
                    var face = Blocks[x, z].Faces[(int)BlockFaces.Floor];

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
                                        split = split == 0 ? 1 : 0;
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
                                        addTriangle2 = Blocks[x, z].Type != BlockType.Wall;
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
                                        addTriangle2 = Blocks[x, z].Type != BlockType.Wall;
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
                                        addTriangle1 = Blocks[x, z].Type != BlockType.Wall;
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
                                        addTriangle1 = Blocks[x, z].Type != BlockType.Wall;
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
                                split = split == 0 ? 1 : 0;

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

                        if ((Blocks[x, z].Type != BlockType.Floor || Blocks[x, z].CeilingPortal != -1) &&
                            (Blocks[x, z].CeilingPortal < 0 || Blocks[x, z].CeilingOpacity == PortalOpacity.None) &&
                            !Blocks[x, z].IsCeilingSolid && Blocks[x, z].Type != BlockType.Wall)
                            continue;

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
                                split = split == 0 ? 1 : 0;

                            if (Blocks[x, z].Type != BlockType.Wall)
                            {
                                if (Blocks[x, z].SplitFoorType == 1)
                                    split = split == 0 ? 1 : 0;
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
                    else
                    {
                        if ((Blocks[x, z].Type != BlockType.Floor || Blocks[x, z].CeilingPortal != -1) &&
                            (Blocks[x, z].CeilingPortal == -1 || Blocks[x, z].CeilingOpacity == PortalOpacity.None) &&
                            !Blocks[x, z].IsCeilingSolid)
                            continue;

                        int split = GetBestCeilingSplit(x, z, ws0, ws1, ws2, ws3);
                        if (Blocks[x, z].SplitCeilingType == 1)
                            split = split == 0 ? 1 : 0;

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

            // After building the geometry subset, I have to rebuild the editor vertices array for the vertex buffer
            Vertices = new List<EditorVertex>();
            var tempCentre = Vector4.Zero;
            int totalVertices = 0;

            for (int x = 0; x < NumXSectors; x++)
            {
                for (int z = 0; z < NumZSectors; z++)
                {
                    for (int f = 0; f < 29; f++)
                    {
                        var face = Blocks[x, z].Faces[f];
                        if (face == null || !face.Defined)
                            continue;

                        if (face.Vertices == null || face.Vertices.Length == 0)
                            continue;

                        Blocks[x, z].Faces[f].StartVertex = (short)Vertices.Count;
                        int baseIndex = Vertices.Count;

                        Blocks[x, z].Faces[f].IndicesForSolidBucketsRendering = new List<short>
                        {
                            (short) (baseIndex + 0),
                            (short) (baseIndex + 1),
                            (short) (baseIndex + 2)
                        };


                        if (face.Shape == BlockFaceShape.Rectangle)
                        {
                            Blocks[x, z].Faces[f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 3));
                            Blocks[x, z].Faces[f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 4));
                            Blocks[x, z].Faces[f].IndicesForSolidBucketsRendering.Add((short)(baseIndex + 5));
                        }

                        Vertices.AddRange(face.Vertices);
                    }

                    for (int i = 0; i < NumVerticesInGrid[x, z]; i++)
                    {
                        tempCentre += VerticesGrid[x, z, i].Position;
                        totalVertices++;
                    }
                }
            }

            Centre = new Vector3(tempCentre.X, tempCentre.Y, tempCentre.Z);
            Centre /= totalVertices;
        }

        private void AddVerticalFaces(int x, int z, FaceDirection direction, bool floor, bool ceiling, bool middle)
        {
            int xA, xB, zA, zB, yA, yB;

            var e1 = new Vector2(0.0f, 0.0f);
            var e2 = new Vector2(1.0f, 0.0f);
            var e3 = new Vector2(1.0f, 1.0f);
            var e4 = new Vector2(0.0f, 1.0f);

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
                        var portal = FindPortal(x, z, PortalDirection.South);
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (Flipped && BaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
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
                        var portal = FindPortal(x, z, PortalDirection.North);
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (Flipped && BaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
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
                        var portal = FindPortal(x, z, PortalDirection.West);
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (Flipped && BaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }


                        int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);
                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].QAFaces[2];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].QAFaces[1];
                        qA = (int)Position.Y + Blocks[1, z].QAFaces[3];
                        qB = (int)Position.Y + Blocks[1, z].QAFaces[0];
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

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
                    switch (Blocks[x, z].FloorDiagonalSplit)
                    {
                        case DiagonalSplit.NW:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
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
                            break;
                        case DiagonalSplit.NE:
                            xA = x + 1;
                            xB = x;
                            zA = z;
                            zB = z + 1;
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
                            break;
                        case DiagonalSplit.SE:
                            xA = x;
                            xB = x + 1;
                            zA = z;
                            zB = z + 1;
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
                            break;
                        default:
                            xA = x;
                            xB = x + 1;
                            zA = z + 1;
                            zB = z;
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
                            break;
                    }

                    break;

                case FaceDirection.DiagonalCeiling:
                    switch (Blocks[x, z].CeilingDiagonalSplit)
                    {
                        case DiagonalSplit.NW:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
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
                            break;
                        case DiagonalSplit.NE:
                            xA = x + 1;
                            xB = x;
                            zA = z;
                            zB = z + 1;
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
                            break;
                        case DiagonalSplit.SE:
                            xA = x;
                            xB = x + 1;
                            zA = z;
                            zB = z + 1;
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
                            break;
                        default:
                            xA = x;
                            xB = x + 1;
                            zA = z + 1;
                            zB = z;
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
                            break;
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
                        var portal = FindPortal(x, z, PortalDirection.East);
                        var adjoiningRoom = portal.AdjoiningRoom;

                        if (Flipped && BaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
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
                    else if (qA == yA && qB > yB)
                        AddTriangle(x, z, qaFace, new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, qB * 256.0f, zB * 1024.0f),
                                                              new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);
                    else if (qA > yA && qB == yB)
                        AddTriangle(x, z, qaFace, new Vector3(xA * 1024.0f, qA * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                              new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);

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
                            AddTriangle(x, z, edFace, new Vector3(xA * 1024.0f, eA * 256.0f, zA * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                                                  new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                                  face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                        else if (eA == yA && eB > yB)
                            AddTriangle(x, z, edFace, new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, eB * 256.0f, zB * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
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
                    else if (wA < yA && wB == yB)
                        AddTriangle(x, z, wsFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                              new Vector3(xA * 1024.0f, (Ceiling + wA) * 256.0f, zA * 1024.0f),
                                                              face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);
                    else if (wA == yA && wB < yB)
                        AddTriangle(x, z, wsFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                              new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                              new Vector3(xB * 1024.0f, (Ceiling + wB) * 256.0f, zB * 1024.0f),
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
                            AddTriangle(x, z, rfFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                                  new Vector3(xB * 1024.0f, (Ceiling + rB) * 256.0f, zB * 1024.0f),
                                                                  face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);
                    }
                }
            }

            // Poligoni WS e RF
            if (!middle)
                return;

            face = Blocks[x, z].Faces[(int)middleFace];

            yA = wA > cA ? cA : wA;
            yB = wB > cB ? cB : wB;
            int yD = qA < fA ? fA : qA;
            int yC = qB < fB ? fB : qB;
            // middle
            if (Ceiling + yA != yD && Ceiling + yB != yC)
                AddRectangle(x, z, middleFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                       new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                       new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                                                       new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                                                       face.RectangleUV[0], face.RectangleUV[1], face.RectangleUV[2], face.RectangleUV[3],
                                                       e1, e2, e3, e4);

            else if (Ceiling + yA != yD && Ceiling + yB == yC)
                AddTriangle(x, z, middleFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                      new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                      new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                                                      face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e4, 1);

            else if (Ceiling + yA == yD && Ceiling + yB != yC)
                AddTriangle(x, z, middleFace, new Vector3(xA * 1024.0f, (Ceiling + yA) * 256.0f, zA * 1024.0f),
                                                   new Vector3(xB * 1024.0f, (Ceiling + yB) * 256.0f, zB * 1024.0f),
                                                   new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                                            face.TriangleUV[0], face.TriangleUV[1], face.TriangleUV[2], e1, e2, e3);
        }
        public static bool IsQuad(int x, int z, int h1, int h2, int h3, int h4, bool horizontal = false)
        {
            var p1 = new Vector3(x * 1024.0f, h1 * 256.0f, z * 1024.0f);
            var p2 = new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f);
            var p3 = new Vector3((x + 1) * 1024.0f, h3 * 256.0f, (z + 1) * 1024.0f);
            var p4 = new Vector3(x * 1024.0f, h4 * 256.0f, (z + 1) * 1024.0f);

            var plane1 = new Plane(p1, p2, p3);
            var plane2 = new Plane(p1, p2, p4);

            if (plane1.Normal != plane2.Normal)
                return false;

            return !horizontal || (plane1.Normal == Vector3.UnitY || plane1.Normal == -Vector3.UnitY);
        }

        private static int FindHorizontalTriangle(int x, int z, int h1, int h2, int h3, int h4)
        {
            var p1 = new Vector3(x * 1024.0f, h1 * 256.0f, z * 1024.0f);
            var p2 = new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f);
            var p3 = new Vector3((x + 1) * 1024.0f, h3 * 256.0f, (z + 1) * 1024.0f);
            var p4 = new Vector3(x * 1024.0f, h4 * 256.0f, (z + 1) * 1024.0f);

            var plane = new Plane(p1, p2, p4);
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

        private static int GetBestFloorSplit(int x, int z, int h1, int h2, int h3, int h4)
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

        private static int GetBestCeilingSplit(int x, int z, int h1, int h2, int h3, int h4)
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
            var plane = new Plane(p1, p2, p3);

            var v1 = new EditorVertex
            {
                Position = new Vector4(p1, 1.0f),
                UV = uv1,
                EditorUV = e1
            };

            var v2 = new EditorVertex
            {
                Position = new Vector4(p2, 1.0f),
                UV = uv2,
                EditorUV = e2
            };

            var v3 = new EditorVertex
            {
                Position = new Vector4(p3, 1.0f),
                UV = uv3,
                EditorUV = e3
            };

            var v4 = new EditorVertex
            {
                Position = new Vector4(p4, 1.0f),
                UV = uv4,
                EditorUV = e4
            };

            Blocks[x, z].Faces[(int)face].Vertices = new EditorVertex[6];

            // creo una nuova lista dei vertici
            Blocks[x, z].Faces[(int)face].IndicesForSolidBucketsRendering = new List<short>();
            Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations = new List<short>();
            Blocks[x, z].Faces[(int)face].EditorUv = new byte[4];

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

            for (short i = 0; i < NumVerticesInGrid[gridX1, gridZ1]; i++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (VerticesGrid[gridX1, gridZ1, i].Position.X == p1.X &&
                    VerticesGrid[gridX1, gridZ1, i].Position.Y == p1.Y &&
                    VerticesGrid[gridX1, gridZ1, i].Position.Z == p1.Z)
                {
                    i1 = i;
                    break;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            int gridX2 = (int)(p2.X / 1024);
            int gridZ2 = (int)(p2.Z / 1024);

            for (short i = 0; i < NumVerticesInGrid[gridX2, gridZ2]; i++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (VerticesGrid[gridX2, gridZ2, i].Position.X == p2.X &&
                    VerticesGrid[gridX2, gridZ2, i].Position.Y == p2.Y &&
                    VerticesGrid[gridX2, gridZ2, i].Position.Z == p2.Z)
                {
                    i2 = i;
                    break;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            int gridX3 = (int)(p3.X / 1024);
            int gridZ3 = (int)(p3.Z / 1024);

            for (short i = 0; i < NumVerticesInGrid[gridX3, gridZ3]; i++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (VerticesGrid[gridX3, gridZ3, i].Position.X == p3.X &&
                    VerticesGrid[gridX3, gridZ3, i].Position.Y == p3.Y &&
                    VerticesGrid[gridX3, gridZ3, i].Position.Z == p3.Z)
                {
                    i3 = i;
                    break;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            int gridX4 = (int)(p4.X / 1024);
            int gridZ4 = (int)(p4.Z / 1024);

            for (short i = 0; i < NumVerticesInGrid[gridX4, gridZ4]; i++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (VerticesGrid[gridX4, gridZ4, i].Position.X == p4.X &&
                    VerticesGrid[gridX4, gridZ4, i].Position.Y == p4.Y &&
                    VerticesGrid[gridX4, gridZ4, i].Position.Z == p4.Z)
                {
                    i4 = i;
                    break;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Clear();

            short base1 = (short)(((short)(p1.X / 1024.0f) << 9) + ((short)(p1.Z / 1024.0f) << 4));
            short base2 = (short)(((short)(p2.X / 1024.0f) << 9) + ((short)(p2.Z / 1024.0f) << 4));
            short base3 = (short)(((short)(p3.X / 1024.0f) << 9) + ((short)(p3.Z / 1024.0f) << 4));
            short base4 = (short)(((short)(p4.X / 1024.0f) << 9) + ((short)(p4.Z / 1024.0f) << 4));


            if (i1 == -1)
            {
                int lastVertex = NumVerticesInGrid[gridX1, gridZ1];
                VerticesGrid[gridX1, gridZ1, lastVertex] = v1;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base1 + NumVerticesInGrid[gridX1, gridZ1]));
                i1 = NumVerticesInGrid[gridX1, gridZ1];

                NumVerticesInGrid[gridX1, gridZ1]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base1 + i1));
            }

            if (i2 == -1)
            {
                int lastVertex = NumVerticesInGrid[gridX2, gridZ2];
                VerticesGrid[gridX2, gridZ2, lastVertex] = v2;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base2 + NumVerticesInGrid[gridX2, gridZ2]));
                i2 = NumVerticesInGrid[gridX2, gridZ2];

                NumVerticesInGrid[gridX2, gridZ2]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base2 + i2));
            }

            if (i3 == -1)
            {
                int lastVertex = NumVerticesInGrid[gridX3, gridZ3];
                VerticesGrid[gridX3, gridZ3, lastVertex] = v3;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base3 + NumVerticesInGrid[gridX3, gridZ3]));
                i3 = NumVerticesInGrid[gridX3, gridZ3];

                NumVerticesInGrid[gridX3, gridZ3]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base3 + i3));
            }

            if (i4 == -1)
            {
                int lastVertex = NumVerticesInGrid[gridX4, gridZ4];
                VerticesGrid[gridX4, gridZ4, lastVertex] = v4;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base4 + NumVerticesInGrid[gridX4, gridZ4]));
                i4 = NumVerticesInGrid[gridX4, gridZ4];

                NumVerticesInGrid[gridX4, gridZ4]++;
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
            var plane = new Plane(p1, p2, p3);

            // creo una nuova lista dei vertici
            Blocks[x, z].Faces[(int)face].IndicesForSolidBucketsRendering = new List<short>();
            Blocks[x, z].Faces[(int)face].IndicesForLightingCalculations = new List<short>();
            Blocks[x, z].Faces[(int)face].EditorUv = new byte[4];

            Blocks[x, z].Faces[(int)face].Shape = BlockFaceShape.Triangle;
            Blocks[x, z].Faces[(int)face].Defined = true;
            Blocks[x, z].Faces[(int)face].SplitMode = subdivision;
            Blocks[x, z].Faces[(int)face].Plane = plane;

            //  Blocks[x, z].Faces[(int)face].StartVertex = (short)Vertices.Count;
            //  for (int i = 0; i < Blocks[x, z].Faces[(int)face].Vertices.Count; i++) Vertices.Add(Blocks[x, z].Faces[(int)face].Vertices[i]);

            Blocks[x, z].Faces[(int)face].Vertices = new EditorVertex[3];

            var v1 = new EditorVertex
            {
                Position = new Vector4(p1, 1.0f),
                UV = uv1,
                EditorUV = e1
            };

            var v2 = new EditorVertex
            {
                Position = new Vector4(p2, 1.0f),
                UV = uv2,
                EditorUV = e2
            };

            var v3 = new EditorVertex
            {
                Position = new Vector4(p3, 1.0f),
                UV = uv3,
                EditorUV = e3
            };

            // according to texture rotation
            short i1 = -1;
            short i2 = -1;
            short i3 = -1;

            int gridX1 = (int)(p1.X / 1024);
            int gridZ1 = (int)(p1.Z / 1024);

            for (short i = 0; i < NumVerticesInGrid[gridX1, gridZ1]; i++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (VerticesGrid[gridX1, gridZ1, i].Position.X == p1.X &&
                    VerticesGrid[gridX1, gridZ1, i].Position.Y == p1.Y &&
                    VerticesGrid[gridX1, gridZ1, i].Position.Z == p1.Z)
                {
                    i1 = i;
                    break;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            int gridX2 = (int)(p2.X / 1024);
            int gridZ2 = (int)(p2.Z / 1024);

            for (short i = 0; i < NumVerticesInGrid[gridX2, gridZ2]; i++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (VerticesGrid[gridX2, gridZ2, i].Position.X == p2.X &&
                    VerticesGrid[gridX2, gridZ2, i].Position.Y == p2.Y &&
                    VerticesGrid[gridX2, gridZ2, i].Position.Z == p2.Z)
                {
                    i2 = i;
                    break;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            int gridX3 = (int)(p3.X / 1024);
            int gridZ3 = (int)(p3.Z / 1024);

            for (short i = 0; i < NumVerticesInGrid[gridX3, gridZ3]; i++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (VerticesGrid[gridX3, gridZ3, i].Position.X == p3.X &&
                    VerticesGrid[gridX3, gridZ3, i].Position.Y == p3.Y &&
                    VerticesGrid[gridX3, gridZ3, i].Position.Z == p3.Z)
                {
                    i3 = i;
                    break;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Clear();

            short base1 = (short)(((short)(p1.X / 1024.0f) << 9) + ((short)(p1.Z / 1024.0f) << 4));
            short base2 = (short)(((short)(p2.X / 1024.0f) << 9) + ((short)(p2.Z / 1024.0f) << 4));
            short base3 = (short)(((short)(p3.X / 1024.0f) << 9) + ((short)(p3.Z / 1024.0f) << 4));

            if (i1 == -1)
            {
                int lastVertex = NumVerticesInGrid[gridX1, gridZ1];
                VerticesGrid[gridX1, gridZ1, lastVertex] = v1;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base1 + NumVerticesInGrid[gridX1, gridZ1]));
                i1 = NumVerticesInGrid[gridX1, gridZ1];

                NumVerticesInGrid[gridX1, gridZ1]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base1 + i1));
            }

            if (i2 == -1)
            {
                int lastVertex = NumVerticesInGrid[gridX2, gridZ2];
                VerticesGrid[gridX2, gridZ2, lastVertex] = v2;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base2 + NumVerticesInGrid[gridX2, gridZ2]));
                i2 = NumVerticesInGrid[gridX2, gridZ2];

                NumVerticesInGrid[gridX2, gridZ2]++;
            }
            else
            {
                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base2 + i2));
            }

            if (i3 == -1)
            {
                int lastVertex = NumVerticesInGrid[gridX3, gridZ3];
                VerticesGrid[gridX3, gridZ3, lastVertex] = v3;

                Blocks[x, z].Faces[(int)face].IndicesForFinalLevel.Add((short)(base3 + NumVerticesInGrid[gridX3, gridZ3]));
                i3 = NumVerticesInGrid[gridX3, gridZ3];

                NumVerticesInGrid[gridX3, gridZ3]++;
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
                var v1 = Vertices[face.StartVertex + 0];
                var p1 = new Vector3(v1.Position.X, v1.Position.Y, v1.Position.Z);
                var v2 = Vertices[face.StartVertex + 1];
                var p2 = new Vector3(v2.Position.X, v2.Position.Y, v2.Position.Z);
                var v3 = Vertices[face.StartVertex + 2];
                var p3 = new Vector3(v3.Position.X, v3.Position.Y, v3.Position.Z);
                var v4 = Vertices[face.StartVertex + 3];
                var p4 = new Vector3(v4.Position.X, v4.Position.Y, v4.Position.Z);
                var v5 = Vertices[face.StartVertex + 4];
                var p5 = new Vector3(v5.Position.X, v5.Position.Y, v5.Position.Z);
                var v6 = Vertices[face.StartVertex + 5];
                var p6 = new Vector3(v6.Position.X, v6.Position.Y, v6.Position.Z);

                var pl = new Plane(p1, p2, p3);

                if (ray.Intersects(ref p1, ref p2, ref p3, out distance) && Vector3.Dot(-ray.Direction, pl.Normal) >= epsilon)
                    return true;
                if (ray.Intersects(ref p4, ref p5, ref p6, out distance) && Vector3.Dot(-ray.Direction, pl.Normal) >= epsilon)
                    return true;

                return false;
            }
            else
            {
                var v1 = Vertices[face.StartVertex + 0];
                var p1 = new Vector3(v1.Position.X, v1.Position.Y, v1.Position.Z);
                var v2 = Vertices[face.StartVertex + 1];
                var p2 = new Vector3(v2.Position.X, v2.Position.Y, v2.Position.Z);
                var v3 = Vertices[face.StartVertex + 2];
                var p3 = new Vector3(v3.Position.X, v3.Position.Y, v3.Position.Z);

                var pl = new Plane(p1, p2, p3);

                return ray.Intersects(ref p1, ref p2, ref p3, out distance) && Vector3.Dot(-ray.Direction, pl.Normal) >= epsilon;
            }
        }

        private bool RayTraceCheckFloorCeiling(int x, int y, int z, int xLight, int zLight)
        {
            int currentX = (x / 1024) - (x > xLight ? 1 : 0);
            int currentZ = (z / 1024) - (z > zLight ? 1 : 0);

            int floorMin = GetLowestFloorCorner(currentX, currentZ);
            int ceilingMax = Ceiling + GetHighestCeilingCorner(currentX, currentZ);

            return floorMin <= y / 256 && ceilingMax >= y / 256;
        }

        private bool RayTraceX(int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int deltaX;
            int deltaY;
            int deltaZ;

            int minX;
            int maxX;

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

            if (deltaX == 0)
                return true;

            int fracX = (((minX >> 10) + 1) << 10) - minX;
            int currentX = ((minX >> 10) + 1) << 10;
            int currentZ = deltaZ * fracX / (deltaX + 1) + zPoint;
            int currentY = deltaY * fracX / (deltaX + 1) + yPoint;

            if (currentX > maxX)
                return true;

            do
            {
                int currentXblock = currentX / 1024;
                int currentZblock = currentZ / 1024;

                if (currentZblock < 0 || currentXblock >= NumXSectors || currentZblock >= NumZSectors)
                {
                    if (currentX == maxX)
                        return true;
                }
                else
                {
                    int currentYclick = currentY / -256;

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
                        var currentBlock = Blocks[currentXblock - 1, currentZblock];
                        var nextBlock = Blocks[currentXblock, currentZblock];

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

            return true;
        }

        private bool RayTraceZ(int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int deltaX;
            int deltaY;
            int deltaZ;

            int minZ;
            int maxZ;

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

            if (deltaZ == 0)
                return true;

            int fracZ = (((minZ >> 10) + 1) << 10) - minZ;
            int currentZ = ((minZ >> 10) + 1) << 10;
            int currentX = deltaX * fracZ / (deltaZ + 1) + xPoint;
            int currentY = deltaY * fracZ / (deltaZ + 1) + yPoint;

            if (currentZ > maxZ)
                return true;

            do
            {
                int currentXblock = currentX / 1024;
                int currentZblock = currentZ / 1024;

                if (currentXblock < 0 || currentZblock >= NumZSectors || currentXblock >= NumXSectors)
                {
                    if (currentZ == maxZ)
                        return true;
                }
                else
                {
                    int currentYclick = currentY / -256;

                    if (currentZblock > 0)
                    {
                        var currentBlock = Blocks[currentXblock, currentZblock - 1];

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
                        var currentBlock = Blocks[currentXblock, currentZblock - 1];
                        var nextBlock = Blocks[currentXblock, currentZblock];

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

            return true;
        }

        private void CalculateLighting(byte[,,] iterations, int x, int z, int f)
        {
            var face = Blocks[x, z].Faces[f];

            var n = face.Plane.Normal;

            foreach (short index in face.IndicesForLightingCalculations)
            {
                int theX = (index >> 9) & 0x1f;
                int theZ = (index >> 4) & 0x1f;
                int theIndex = index & 0x0f;

                var p = new Vector3(VerticesGrid[theX, theZ, theIndex].Position.X,
                    VerticesGrid[theX, theZ, theIndex].Position.Y,
                    VerticesGrid[theX, theZ, theIndex].Position.Z);

                int r = AmbientLight.R;
                int g = AmbientLight.G;
                int b = AmbientLight.B;

                // Get all nearest lights. Maybe in the future limit to max number of lights?
                var lights = GetNearestLights(p);

                foreach (var light in lights)
                {
                    switch (light.Type)
                    {
                        case LightType.Light:
                        case LightType.Shadow:
                            {
                                // Get the distance between light and vertex
                                float distance = Math.Abs((p - light.Position).Length());

                                // If distance is greater than light out radius, then skip this light
                                if (distance > light.Out * 1024.0f)
                                    continue;

                                // Calculate light diffuse value
                                int diffuse = (int)(light.Intensity * 8192);

                                // Calculate the length squared of the normal vector
                                float dotN = Vector3.Dot(n, n);

                                // Do raytracing
                                if (dotN <= 0 ||
                                    !RayTraceCheckFloorCeiling((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                    !RayTraceX((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                    !RayTraceZ((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                {
                                    continue;
                                }

                                // Calculate the attenuation
                                var attenuaton = (light.Out * 1024.0f - distance) / (light.Out * 1024.0f - light.In * 1024.0f);
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
                            break;
                        case LightType.Effect:
                            {
                                int x1 = (int)(Math.Floor(light.Position.X / 1024.0f) * 1024);
                                int z1 = (int)(Math.Floor(light.Position.Z / 1024.0f) * 1024);
                                int x2 = (int)(Math.Ceiling(light.Position.X / 1024.0f) * 1024);
                                int z2 = (int)(Math.Ceiling(light.Position.Z / 1024.0f) * 1024);

                                // TODO: winroomedit was supporting effect lights placed on vertical faces and effects light was applied to owning face
                                // ReSharper disable CompareOfFloatsByEqualityOperator
                                if (((p.X == x1 && p.Z == z1) || (p.X == x1 && p.Z == z2) || (p.X == x2 && p.Z == z1) ||
                                     (p.X == x2 && p.Z == z2)) && p.Y <= light.Position.Y)
                                {
                                    int finalIntensity = (int)(light.Intensity * 8192 * 0.25f);

                                    r += finalIntensity * light.Color.R / 8192;
                                    g += finalIntensity * light.Color.G / 8192;
                                    b += finalIntensity * light.Color.B / 8192;
                                }
                                // ReSharper restore CompareOfFloatsByEqualityOperator
                            }
                            break;
                        case LightType.Sun:
                            {
                                // Do raytracing now for saving CPU later
                                if (!RayTraceCheckFloorCeiling((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                    !RayTraceX((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                    !RayTraceZ((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                {
                                    continue;
                                }

                                // Calculate the light direction
                                var lightDirection = Vector3.Zero;

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
                            break;
                        case LightType.Spot:
                            {
                                // Calculate the ray from light to vertex
                                var lightVector = p - light.Position;
                                lightVector.Y = -lightVector.Y;
                                lightVector.Normalize();

                                // Get the distance between light and vertex
                                float distance = Math.Abs((p - light.Position).Length());

                                // If distance is greater than light length, then skip this light
                                if (distance > light.Cutoff * 1024.0f)
                                    continue;

                                // Calculate the light direction
                                var lightDirection = Vector3.Zero;

                                lightDirection.X = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                                lightDirection.Y = (float)(Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                                lightDirection.Z = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));

                                lightDirection.Normalize();

                                // Calculate the cosines values for In, Out
                                double d = -Vector3.Dot(lightVector, lightDirection);
                                double cosI2 = Math.Cos(MathUtil.DegreesToRadians(light.In));
                                double cosO2 = Math.Cos(MathUtil.DegreesToRadians(light.Out));

                                if (d < cosO2)
                                    continue;

                                if (!RayTraceCheckFloorCeiling((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                    !RayTraceX((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                    !RayTraceZ((int)p.X, (int)p.Y, (int)p.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                {
                                    continue;
                                }

                                // Calculate light diffuse value
                                float factor = (float)(1.0f - (d - cosI2) / (cosO2 - cosI2));
                                if (factor > 1.0f)
                                    factor = 1.0f;
                                if (factor <= 0.0f)
                                    continue;

                                float attenuation = 1.0f;
                                if (distance >= light.Len * 1024.0f)
                                    attenuation = 1.0f - (distance - light.Len * 1024.0f) / (light.Cutoff * 1024.0f - light.Len * 1024.0f);

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
                            break;
                    }
                }

                // Normalization
                int ind = theIndex; // v; // face.Indices[v];
                EditorVertex vertex = VerticesGrid[theX, theZ, ind];

                if (r < 0)
                    r = 0;
                if (g < 0)
                    g = 0;
                if (b < 0)
                    b = 0;

                vertex.FaceColor.X = (r + iterations[theX, theZ, ind] * vertex.FaceColor.X) / (iterations[theX, theZ, ind] + 1);
                vertex.FaceColor.Y = (g + iterations[theX, theZ, ind] * vertex.FaceColor.Y) / (iterations[theX, theZ, ind] + 1);
                vertex.FaceColor.Z = (b + iterations[theX, theZ, ind] * vertex.FaceColor.Z) / (iterations[theX, theZ, ind] + 1);
                vertex.FaceColor.W = 255.0f;

                iterations[theX, theZ, ind]++;

                VerticesGrid[theX, theZ, ind] = vertex;
            }
        }

        private List<Light> GetNearestLights(Vector3 p)
        {
            var lights = new List<Light>();

            foreach (var light in Lights)
            {
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

        public Matrix Transform => Matrix.Translation(new Vector3(Position.X * 1024.0f, Position.Y * 256.0f, Position.Z * 1024.0f));

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
    }
}
