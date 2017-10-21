﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using TombEditor.Compilers;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using TombLib.Utils;
using System.Threading.Tasks;

namespace TombEditor.Geometry
{
    public enum Reverberation : byte
    {
        Outside, SmallRoom, MediumRoom, LargeRoom, Pipe
    }

    public enum BlockFaceShape : byte
    {
        Rectangle, Triangle
    }

    public class Room
    {
        public const short DefaultHeight = 12;
        public const short MaxRoomDimensions = 20;

        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Block[,] Blocks { get; private set; }
        private List<PositionBasedObjectInstance> _objects = new List<PositionBasedObjectInstance>();

        public Room AlternateBaseRoom { get; set; } = null;
        public Room AlternateRoom { get; set; } = null;
        public short AlternateGroup { get; set; } = -1;

        public Vector4 AmbientLight { get; set; } = new Vector4(0.25f, 0.25f, 0.25f, 1.0f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)
        public byte WaterLevel { get; set; }
        public byte MistLevel { get; set; }
        public byte ReflectionLevel { get; set; }
        public bool FlagSnow { get; set; }
        public bool FlagRain { get; set; }
        public bool FlagCold { get; set; }
        public bool FlagDamage { get; set; }
        public bool FlagQuickSand { get; set; }
        public bool FlagOutside { get; set; }
        public bool FlagHorizon { get; set; }
        public bool FlagNoLensflare { get; set; }
        public bool FlagExcludeFromPathFinding { get; set; }
        public Reverberation Reverberation { get; set; }

        // Internal data structures
        private Buffer<EditorVertex> _vertexBuffer;
        private List<EditorVertex>[,] _sectorVertices;
        public struct VertexRange
        {
            public int Start;
            public int Count;
        };
        private VertexRange[,,] _sectorFaceVertexVertexRange;
        private int[,] _sectorAllVerticesOffset;
        private List<EditorVertex> _allVertices = new List<EditorVertex>();

        public Room(Level level, int numXSectors, int numZSectors, string name = "Unnamed", short ceiling = DefaultHeight)
        {
            Name = name;
            Resize(level, new Rectangle(0, 0, numXSectors - 1, numZSectors - 1), 0, ceiling);
        }

        public void Resize(Level level, Rectangle area, short floor = 0, short ceiling = DefaultHeight)
        {
            int numXSectors = area.Width + 1;
            int numZSectors = area.Height + 1;
            DrawingPoint offset = new DrawingPoint(area.X, area.Y);

            if ((numXSectors < 3) || (numZSectors < 3))
                throw new ArgumentOutOfRangeException("area", area, "Provided area for resizing the room is too small. The area must span at least 3 sectors in X and Z dimension.");

            // Remove sector based objects if there are any
            var sectorObjects = Blocks != null ? SectorObjects.ToList() : new List<SectorBasedObjectInstance>();
            foreach (var instance in sectorObjects)
                RemoveObject(level, instance);

            // Build new blocks
            Block[,] newBlocks = new Block[numXSectors, numZSectors];
            for (int x = 0; x < numXSectors; x++)
                for (int z = 0; z < numZSectors; z++)
                {
                    Block oldBlock = GetBlockTry(new DrawingPoint(x, z).Offset(offset));
                    newBlocks[x, z] = oldBlock ?? new Block(floor, ceiling);
                    if (newBlocks[x, z].Type == BlockType.BorderWall)
                        newBlocks[x, z].Type = BlockType.Wall;
                    if (x == 0 || z == 0 || x == numXSectors - 1 || z == numZSectors - 1)
                        newBlocks[x, z].Type = BlockType.BorderWall;
                }

            // Update data structures
            _sectorVertices = new List<EditorVertex>[numXSectors, numZSectors];
            for (int x = 0; x < numXSectors; x++)
                for (int z = 0; z < numZSectors; z++)
                    _sectorVertices[x, z] = new List<EditorVertex>();
            _sectorFaceVertexVertexRange = new VertexRange[numXSectors, numZSectors, (int)Block.FaceCount];
            _sectorAllVerticesOffset = new int[numXSectors, numZSectors];

            Blocks = newBlocks;

            // Move objects
            SectorPos = SectorPos.Offset(offset);
            foreach (var instance in _objects)
                instance.Position -= new Vector3(offset.X * 1024, 0, offset.Y * 1024);

            // Add sector based objects again
            Rectangle newArea = new Rectangle(offset.X, offset.Y, offset.X + numXSectors - 1, offset.Y + numZSectors - 1);
            foreach (var instance in sectorObjects)
                AddObjectCutSectors(level, newArea, instance);

            // Update state
            UpdateCompletely();
        }

        public Room Split(Level level, Rectangle area)
        {
            var newRoom = Clone(level, (instance) => !(instance is PositionBasedObjectInstance) && !(instance is PortalInstance));
            newRoom.Name = "Split from " + Name;
            newRoom.Resize(level, area);
            List<PortalInstance> portals = Portals.ToList();

            // Detect if the room was split by a straight line
            // If this is the case, resize the original room
            if ((area.X == 0) && (area.Y == 0) && (area.Right == (NumXSectors - 1)) && (area.Bottom < (NumZSectors - 1)))
            {
                Resize(level, new Rectangle(area.X, area.Bottom - 1, area.Right, NumZSectors - 1));
                AddObject(level, new PortalInstance(new Rectangle(area.X + 1, 0, area.Right - 1, 0), PortalDirection.WallNegativeZ, newRoom));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z < 1024)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if ((area.X == 0) && (area.Y == 0) && (area.Right < (NumXSectors - 1)) && (area.Bottom == (NumZSectors - 1)))
            {
                Resize(level, new Rectangle(area.Right - 1, area.Y, NumXSectors - 1, area.Bottom));
                AddObject(level, new PortalInstance(new Rectangle(0, area.Y + 1, 0, area.Bottom - 1), PortalDirection.WallNegativeX, newRoom));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.X < 1024)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if ((area.X == 0) && (area.Y > 0) && (area.Right == (NumXSectors - 1)) && (area.Bottom == (NumZSectors - 1)))
            {
                Resize(level, new Rectangle(area.X, 0, area.Right, area.Y + 1));
                AddObject(level, new PortalInstance(new Rectangle(area.X + 1, NumZSectors - 1, area.Right - 1, NumZSectors - 1), PortalDirection.WallPositiveZ, newRoom));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z > ((NumZSectors - 2) * 1024))
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if ((area.X > 0) && (area.Y == 0) && (area.Right == (NumXSectors - 1)) && (area.Bottom == (NumZSectors - 1)))
            {
                Resize(level, new Rectangle(0, area.Y, area.X + 1, area.Bottom));
                AddObject(level, new PortalInstance(new Rectangle(NumXSectors - 1, area.Y + 1, NumXSectors - 1, area.Bottom - 1), PortalDirection.WallPositiveX, newRoom));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z > ((NumXSectors - 2) * 1024))
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else
            {
                // Resize area
                for (int z = area.Y + 1; z < area.Bottom; ++z)
                    for (int x = area.X + 1; x < area.Right; ++x)
                        Blocks[x, z].Type = BlockType.Wall;

                // Move objects
                Vector2 start = new Vector2(area.X, area.Y) * 1024.0f;
                Vector2 end = new Vector2(area.Left + 1, area.Bottom + 1) * 1024.0f;
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if ((instance.Position.X > start.X) && (instance.Position.Z > start.Y) &&
                        (instance.Position.X < end.X) && (instance.Position.Z < end.Y))
                        newRoom.MoveObjectFrom(level, this, instance);
            }

            newRoom.UpdateCompletely();
            UpdateCompletely();
            return newRoom;
        }

        public Room Clone(Level level, Predicate<ObjectInstance> decideToCopy)
        {
            // Copy most variables
            var result = (Room)MemberwiseClone();
            result.AlternateBaseRoom = null;
            result.AlternateRoom = null;
            result.AlternateGroup = -1;

            result._sectorVertices = new List<EditorVertex>[NumXSectors, NumZSectors];
            for (int x = 0; x < NumXSectors; x++)
                for (int z = 0; z < NumZSectors; z++)
                    result._sectorVertices[x, z] = new List<EditorVertex>();
            result._sectorFaceVertexVertexRange = new VertexRange[NumXSectors, NumZSectors, (int)Block.FaceCount];
            result._sectorAllVerticesOffset = new int[NumXSectors, NumZSectors];
            result._allVertices = new List<EditorVertex>();
            result._vertexBuffer = null;

            // Copy blocks
            result.Blocks = new Block[NumXSectors, NumZSectors];
            for (int z = 0; z < NumZSectors; ++z)
                for (int x = 0; x < NumXSectors; ++x)
                    result.Blocks[x, z] = Blocks[x, z].Clone();

            // Copy objects
            result._objects = new List<PositionBasedObjectInstance>();
            foreach (var instance in AnyObjects)
                if (decideToCopy(instance))
                    result.AddObjectAndSingularPortal(level, instance.Clone());

            result.UpdateCompletely();
            return result;
        }

        public Room Clone(Level level)
        {
            return Clone(level, instance => !(instance is PortalInstance));
        }

        public bool Flipped => (AlternateRoom != null) || (AlternateBaseRoom != null);
        public Room AlternateVersion => AlternateRoom ?? AlternateBaseRoom;
        public DrawingPoint SectorSize => new DrawingPoint(NumXSectors, NumZSectors);
        public Rectangle WorldArea => new Rectangle((int)Position.X, (int)Position.Z, (int)Position.X + NumXSectors - 1, (int)Position.Z + NumZSectors - 1);
        public Rectangle LocalArea => new Rectangle(0, 0, NumXSectors - 1, NumZSectors - 1);

        public DrawingPoint SectorPos
        {
            get { return new DrawingPoint((int)Position.X, (int)Position.Z); }
            set { Position = new Vector3(value.X, Position.Y, value.Y); }
        }

        public IEnumerable<PortalInstance> Portals
        {
            get
            { // No LINQ because it is really slow.
                var portals = new HashSet<PortalInstance>();
                foreach (var block in Blocks)
                    foreach (var portal in block.Portals)
                        portals.Add(portal);
                return portals;
            }
        }

        public IEnumerable<TriggerInstance> Triggers
        {
            get
            { // No LINQ because it is really slow.
                var triggers = new HashSet<TriggerInstance>();
                foreach (var block in Blocks)
                    foreach (var trigger in block.Triggers)
                        triggers.Add(trigger);
                return triggers;
            }
        }

        public IEnumerable<SectorBasedObjectInstance> SectorObjects
        {
            get
            {
                foreach (var instance in Portals)
                    yield return instance;
                foreach (var instance in Triggers)
                    yield return instance;
            }
        }

        public IReadOnlyList<PositionBasedObjectInstance> Objects => _objects;

        public IEnumerable<ObjectInstance> AnyObjects
        {
            get
            {
                foreach (var instance in Portals)
                    yield return instance;
                foreach (var instance in Triggers)
                    yield return instance;
                foreach (var instance in _objects)
                    yield return instance;
            }
        }

        public Block GetBlock(DrawingPoint pos)
        {
            return Blocks[pos.X, pos.Y];
        }

        public Block GetBlockTry(int x, int z)
        {
            if (Blocks == null)
                return null;
            if ((x >= 0) && (z >= 0) && (x < NumXSectors) && (z < NumZSectors))
                return Blocks[x, z];
            return null;
        }

        public Block GetBlockTry(DrawingPoint pos)
        {
            return GetBlockTry(pos.X, pos.Y);
        }

        public Block GetBlockTryThroughPortal(int x, int z)
        {
            Block sector = GetBlockTry(x, z);

            if (sector?.WallPortal != null)
            {
                Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;
                DrawingPoint adjoiningSectorCoordinate = new DrawingPoint(x, z).Offset(SectorPos).OffsetNeg(adjoiningRoom.SectorPos);
                sector = adjoiningRoom.GetBlockTry(adjoiningSectorCoordinate);
            }
            return sector;
        }

        public bool IsIllegalSlope(int x, int z)
        {
            Block sector = GetBlockTry(x, z);

            if (sector == null || sector.IsAnyWall || sector.FloorDiagonalSplit != DiagonalSplit.None)
                return false;

            const float criticalSlantComponent = 0.8f;
            const int lowestPassableStep = 2;  // Lara still can bug out of 2-click step heights
            const int lowestPassableHeight = 3;

            Plane[] tri = new Plane[2];

            var p0 = new Vector3(0, sector.QAFaces[0], 0);
            var p1 = new Vector3(4, sector.QAFaces[1], 0);
            var p2 = new Vector3(4, sector.QAFaces[2], -4);
            var p3 = new Vector3(0, sector.QAFaces[3], -4);

            if ( true ) /// WE'RE MISSING REAL TRIANGLE DIRECTION HERE
            {
                tri[0] = new Plane(p0, p1, p2);
                tri[1] = new Plane(p0, p2, p3);
            }
            else
            {
                tri[0] = new Plane(p0, p1, p3);
                tri[1] = new Plane(p1, p2, p3);
            }
            
            EditorArrowType[] slopeDirections = new EditorArrowType[2] { EditorArrowType.EntireFace, EditorArrowType.EntireFace };

            if (Math.Abs(tri[0].Normal.Y) <= criticalSlantComponent)
            {
                int tri1Angle = (int)Math.Floor(Math.Atan2(tri[0].Normal.Z, tri[0].Normal.X) * (180.0f / Math.PI) / 90.0f) * 90;

                switch (tri1Angle)
                {
                    case 0:
                        slopeDirections[0] = EditorArrowType.EdgeE;
                        break;
                    case -90:
                        slopeDirections[0] = EditorArrowType.EdgeS;
                        break;
                    case 90:
                        slopeDirections[0] = EditorArrowType.EdgeN;
                        break;
                    case 180:
                        slopeDirections[0] = EditorArrowType.EdgeW;
                        break;
                }
            }

            if (!sector.FloorIsQuad)
            {
                if (Math.Abs(tri[1].Normal.Y) <= criticalSlantComponent)
                {
                    int tri2Angle = (int)Math.Floor(Math.Atan2(tri[1].Normal.Z, tri[1].Normal.X) * (180.0f / Math.PI) / 90.0f) * 90;

                    switch (tri2Angle)
                    {
                        case 0:
                            slopeDirections[1] = EditorArrowType.EdgeE;
                            break;
                        case -90:
                            slopeDirections[1] = EditorArrowType.EdgeS;
                            break;
                        case 90:
                            slopeDirections[1] = EditorArrowType.EdgeN;
                            break;
                        case 180:
                            slopeDirections[1] = EditorArrowType.EdgeW;
                            break;
                    }
                }
            }
            else
                slopeDirections[1] = slopeDirections[0];

            if (slopeDirections[0] == EditorArrowType.EntireFace && slopeDirections[1] == EditorArrowType.EntireFace)
                // Both triangles are unslidable
                return false;
            else if(slopeDirections[0] == slopeDirections[1])
            {
                // Second triangle pointing to the same direction, treat as quad
                slopeDirections[1] = EditorArrowType.EntireFace;
            }
            else if(slopeDirections[0] == EditorArrowType.EntireFace || slopeDirections[1] == EditorArrowType.EntireFace)
            {
                // One of the triangles is unslidable
                if(slopeDirections[0] == EditorArrowType.EntireFace &&
                    (slopeDirections[1] == EditorArrowType.EdgeN || slopeDirections[1] == EditorArrowType.EdgeE))
                        return false; // Case resolved by engine

                if (slopeDirections[1] == EditorArrowType.EntireFace &&
                    (slopeDirections[0] == EditorArrowType.EdgeW || slopeDirections[0] == EditorArrowType.EdgeS))
                        return false; // Case resolved by engine
            }
            else 
            {
                // Both triangles are slidable
                var diff = tri[0].Normal - tri[1].Normal;
                var angle = Math.Atan2(diff.X, diff.Z);

                if (angle < 0)
                    return true; // Slants are pointing to each other, hence engine can't resolve this situation
            }

            bool slopeIsIllegal = false;

            for (int i = 0; i < 2; i++)
            {
                if (slopeDirections[i] == EditorArrowType.EntireFace || slopeIsIllegal)
                    continue;

                Block lookupBlock = null;
                short[] facesToCheck = new short[2];
                short[] heightsToCompare = new short[2];

                switch (slopeDirections[i])
                {
                    case EditorArrowType.EdgeN:
                        lookupBlock = GetBlockTryThroughPortal(x, z + 1);
                        heightsToCompare[0] = 0;
                        heightsToCompare[1] = 1;
                        facesToCheck[0] = 2;
                        facesToCheck[1] = 3;
                        break;

                    case EditorArrowType.EdgeE:
                        lookupBlock = GetBlockTryThroughPortal(x + 1, z);
                        heightsToCompare[0] = 1;
                        heightsToCompare[1] = 2;
                        facesToCheck[0] = 3;
                        facesToCheck[1] = 0;
                        break;

                    case EditorArrowType.EdgeS:
                        lookupBlock = GetBlockTryThroughPortal(x, z - 1);
                        heightsToCompare[0] = 2;
                        heightsToCompare[1] = 3;
                        facesToCheck[0] = 0;
                        facesToCheck[1] = 1;
                        break;

                    case EditorArrowType.EdgeW:
                        lookupBlock = GetBlockTryThroughPortal(x - 1, z);
                        heightsToCompare[0] = 3;
                        heightsToCompare[1] = 0;
                        facesToCheck[0] = 1;
                        facesToCheck[1] = 2;
                        break;
                }

                if (lookupBlock.IsAnyWall && lookupBlock.FloorDiagonalSplit == DiagonalSplit.None)
                {
                    slopeIsIllegal = true;
                    continue;
                }
                else if(lookupBlock.FloorDiagonalSplit != DiagonalSplit.None)
                {
                    switch (slopeDirections[i])
                    {
                        case EditorArrowType.EdgeN:
                            if (lookupBlock.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                lookupBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (lookupBlock.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                                facesToCheck[1] = 2;
                            else
                                facesToCheck[0] = 3;
                            break;
                        case EditorArrowType.EdgeE:
                            if (lookupBlock.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                lookupBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (lookupBlock.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.FloorDiagonalSplit == DiagonalSplit.XpZp)
                                facesToCheck[1] = 3;
                            else
                                facesToCheck[0] = 0;
                            break;
                        case EditorArrowType.EdgeS:
                            if (lookupBlock.FloorDiagonalSplit == DiagonalSplit.XpZp ||
                                lookupBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (lookupBlock.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                                facesToCheck[1] = 0;
                            else
                                facesToCheck[0] = 1;
                            break;
                        case EditorArrowType.EdgeW:
                            if (lookupBlock.FloorDiagonalSplit == DiagonalSplit.XpZp ||
                                lookupBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (lookupBlock.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                                facesToCheck[0] = 2;
                            else
                                facesToCheck[1] = 1;
                            break;
                    }
                }

                if (Math.Max(lookupBlock.QAFaces[facesToCheck[0]], lookupBlock.QAFaces[facesToCheck[1]]) - Math.Min(sector.QAFaces[heightsToCompare[0]], sector.QAFaces[heightsToCompare[1]]) > lowestPassableStep ||
                    Math.Min(lookupBlock.WSFaces[facesToCheck[0]], lookupBlock.WSFaces[facesToCheck[1]]) - Math.Max(sector.QAFaces[heightsToCompare[0]], sector.QAFaces[heightsToCompare[1]]) < lowestPassableHeight ||
                    Math.Min(lookupBlock.WSFaces[facesToCheck[0]], lookupBlock.WSFaces[facesToCheck[1]]) - Math.Max(lookupBlock.QAFaces[facesToCheck[1]], lookupBlock.QAFaces[facesToCheck[1]]) < lowestPassableHeight)
                    slopeIsIllegal = true;
            }

            return slopeIsIllegal;
        }

        public bool IsFaceDefined(int x, int z, BlockFace face)
        {
            return _sectorFaceVertexVertexRange[x, z, (int)face].Count != 0;
        }

        public VertexRange GetFaceVertexRange(int x, int z, BlockFace face)
        {
            VertexRange range = _sectorFaceVertexVertexRange[x, z, (int)face];
            int offset = _sectorAllVerticesOffset[x, z];
            return new VertexRange { Start = range.Start + offset, Count = range.Count };
        }

        public void BuildGeometry()
        {
            BuildGeometry(new Rectangle(0, 0, NumXSectors - 1, NumZSectors - 1));
        }

        public void BuildGeometry(Rectangle area)
        {
            // Adjust ranges
            int xMin = Math.Max(0, area.X);
            int xMax = Math.Min(NumXSectors - 1, area.Right);
            int zMin = Math.Max(0, area.Y);
            int zMax = Math.Min(NumZSectors - 1, area.Bottom);

            // Build face polygons
            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    // Reset sector
                    for (BlockFace f = 0; f < Block.FaceCount; f++)
                    {
                        _sectorFaceVertexVertexRange[x, z, (int)f] = new VertexRange();
                        _sectorVertices[x, z].Clear();
                    }

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

                    // +Z direction
                    if (x > 0 && x < NumXSectors - 1 && z > 0 && z < NumZSectors - 2 &&
                        !(Blocks[x, z + 1].Type == BlockType.Wall &&
                         (Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.XnZn)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZn))
                            AddVerticalFaces(x, z, FaceDirection.PositiveZ, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.PositiveZ, true, true, false);
                    }


                    // -Z direction
                    if (x > 0 && x < NumXSectors - 1 && z > 1 && z < NumZSectors - 1 &&
                        !(Blocks[x, z - 1].Type == BlockType.Wall &&
                         (Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.XpZp || Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall ||
                            (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZp || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZp))
                            AddVerticalFaces(x, z, FaceDirection.NegativeZ, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.NegativeZ, true, true, false);
                    }

                    // +X direction
                    if (z > 0 && z < NumZSectors - 1 && x > 0 && x < NumXSectors - 2 &&
                        !(Blocks[x + 1, z].Type == BlockType.Wall &&
                        (Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.XnZn || Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZn || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZp))
                            AddVerticalFaces(x, z, FaceDirection.PositiveX, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.PositiveX, true, true, false);
                    }

                    // -X direction
                    if (z > 0 && z < NumZSectors - 1 && x > 1 && x < NumXSectors - 1 &&
                        !(Blocks[x - 1, z].Type == BlockType.Wall &&
                        (Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.XpZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZp))
                            AddVerticalFaces(x, z, FaceDirection.NegativeX, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.NegativeX, true, true, false);
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

                    // +Z directed border wall
                    if (z == 0 && x != 0 && x != NumXSectors - 1 &&
                        !(Blocks[x, 1].Type == BlockType.Wall &&
                         (Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.XnZn)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = FindPortal(x, z, PortalDirection.WallNegativeZ);
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (Flipped && AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                            if (adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].Type == BlockType.Wall &&
                                (adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.None ||
                                 adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.XnZp ||
                                 adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.XpZp))
                            {
                                addMiddle = true;
                            }
                        }


                        if (addMiddle || (Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == null) || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false))
                            AddVerticalFaces(x, z, FaceDirection.PositiveZ, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.PositiveZ, true, true, false);
                    }

                    // -Z directed border wall
                    if (z == NumZSectors - 1 && x != 0 && x != NumXSectors - 1 &&
                        !(Blocks[x, NumZSectors - 2].Type == BlockType.Wall &&
                         (Blocks[x, NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.XpZp || Blocks[x, NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = FindPortal(x, z, PortalDirection.WallPositiveZ);
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (Flipped && AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                            if (adjoiningRoom.Blocks[facingX, 1].Type == BlockType.Wall &&
                                (adjoiningRoom.Blocks[facingX, 1].FloorDiagonalSplit == DiagonalSplit.None ||
                                 adjoiningRoom.Blocks[facingX, 1].FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                 adjoiningRoom.Blocks[facingX, 1].FloorDiagonalSplit == DiagonalSplit.XpZn))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || (Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == null) || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false))
                            AddVerticalFaces(x, z, FaceDirection.NegativeZ, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.NegativeZ, true, true, false);
                    }

                    // -X directed border wall
                    if (x == 0 && z != 0 && z != NumZSectors - 1 &&
                        !(Blocks[1, z].Type == BlockType.Wall &&
                         (Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.XnZn || Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = FindPortal(x, z, PortalDirection.WallNegativeX);
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (Flipped && AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);

                            if (adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].Type == BlockType.Wall &&
                                (adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].FloorDiagonalSplit == DiagonalSplit.None ||
                                 adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].FloorDiagonalSplit == DiagonalSplit.XpZn ||
                                 adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ].FloorDiagonalSplit == DiagonalSplit.XpZp))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || (Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == null) || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false))
                            AddVerticalFaces(x, z, FaceDirection.PositiveX, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.PositiveX, true, true, false);
                    }

                    // +X directed border wall
                    if (x == NumXSectors - 1 && z != 0 && z != NumZSectors - 1 &&
                        !(Blocks[NumXSectors - 2, z].Type == BlockType.Wall &&
                         (Blocks[NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.XpZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = FindPortal(x, z, PortalDirection.WallPositiveX);
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (Flipped && AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);

                            if (adjoiningRoom.Blocks[1, facingZ].Type == BlockType.Wall &&
                                (adjoiningRoom.Blocks[1, facingZ].FloorDiagonalSplit == DiagonalSplit.None ||
                                 adjoiningRoom.Blocks[1, facingZ].FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                 adjoiningRoom.Blocks[1, facingZ].FloorDiagonalSplit == DiagonalSplit.XnZp))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || (Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == null) || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false))
                            AddVerticalFaces(x, z, FaceDirection.NegativeX, true, true, true);
                        else
                            AddVerticalFaces(x, z, FaceDirection.NegativeX, true, true, false);
                    }

                    // Floor polygons
                    RoomConnectionInfo floorPortalInfo = GetFloorRoomConnectionInfo(new DrawingPoint(x, z));
                    BuildFloorOrCeilingFace(x, z, qa0, qa1, qa2, qa3, Blocks[x, z].FloorDiagonalSplit, Blocks[x, z].FloorSplitDirectionIsXEqualsZ,
                        BlockFace.Floor, BlockFace.FloorTriangle2, floorPortalInfo.VisualType);

                    // Ceiling polygons
                    var sectorVertices = _sectorVertices[x, z];
                    int startCeilingPolygons = sectorVertices.Count;

                    RoomConnectionInfo ceilingPortalInfo = GetCeilingRoomConnectionInfo(new DrawingPoint(x, z));
                    BuildFloorOrCeilingFace(x, z, ws0, ws1, ws2, ws3, Blocks[x, z].CeilingDiagonalSplit, Blocks[x, z].CeilingSplitDirectionIsXEqualsZ,
                        BlockFace.Ceiling, BlockFace.CeilingTriangle2, ceilingPortalInfo.VisualType);

                    // Change vertices order for ceiling polygons
                    for (int i = startCeilingPolygons; i < sectorVertices.Count; i += 3)
                    {
                        var tempVertex = sectorVertices[i + 2];
                        sectorVertices[i + 2] = sectorVertices[i];
                        sectorVertices[i] = tempVertex;
                    }
                }
            }

            // Collect all vertices
            _allVertices.Clear();
            for (int x = 0; x < NumXSectors; x++)
                for (int z = 0; z < NumZSectors; z++)
                {
                    _sectorAllVerticesOffset[x, z] = _allVertices.Count;
                    _allVertices.AddRange(_sectorVertices[x, z]);
                }
        }

        private void BuildFloorOrCeilingFace(int x, int z, int h0, int h1, int h2, int h3, DiagonalSplit splitType, bool diagonalSplitXEqualsY, BlockFace face1, BlockFace face2, RoomConnectionType portalMode)
        {
            BlockType blockType = Blocks[x, z].Type;

            // Exit function if the sector is a complete wall or portal
            if (portalMode == RoomConnectionType.FullPortal)
                return;

            switch (blockType)
            {
                case BlockType.BorderWall:
                    return;
                case BlockType.Wall:
                    if (splitType == DiagonalSplit.None)
                        return;
                    break;
            }

            // Process relevant sectors for portals
            switch (portalMode)
            {
                case RoomConnectionType.FullPortal:
                    return;
                case RoomConnectionType.TriangularPortalXnZp:
                case RoomConnectionType.TriangularPortalXpZn:
                    if (Block.IsQuad(h0, h1, h2, h3))
                        diagonalSplitXEqualsY = true;
                    break;
                case RoomConnectionType.TriangularPortalXpZp:
                case RoomConnectionType.TriangularPortalXnZn:
                    if (Block.IsQuad(h0, h1, h2, h3))
                        diagonalSplitXEqualsY = false;
                    break;
            }

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

            // Build sector
            if (splitType != DiagonalSplit.None)
            {
                switch (splitType)
                {
                    case DiagonalSplit.XpZn:
                        if (portalMode != RoomConnectionType.TriangularPortalXnZp)
                            AddTriangle(x, z, face1,
                                new Vector3(x * 1024.0f, h0 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                Blocks[x, z].GetFaceTexture(face1), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), true);

                        if (portalMode != RoomConnectionType.TriangularPortalXpZn && blockType != BlockType.Wall)
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                Blocks[x, z].GetFaceTexture(face2), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), true);
                        break;
                    case DiagonalSplit.XnZn:
                        if (portalMode != RoomConnectionType.TriangularPortalXpZp)
                            AddTriangle(x, z, face1,
                                new Vector3(x * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, z * 1024.0f),
                                Blocks[x, z].GetFaceTexture(face1), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), false);

                        if (portalMode != RoomConnectionType.TriangularPortalXnZn && blockType != BlockType.Wall)
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                Blocks[x, z].GetFaceTexture(face2), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), false);
                        break;
                    case DiagonalSplit.XnZp:
                        if (portalMode != RoomConnectionType.TriangularPortalXpZn)
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                Blocks[x, z].GetFaceTexture(face2), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), true);

                        if (portalMode != RoomConnectionType.TriangularPortalXnZp && blockType != BlockType.Wall)
                            AddTriangle(x, z, face1,
                                new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                Blocks[x, z].GetFaceTexture(face1), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), true);

                        break;
                    case DiagonalSplit.XpZp:
                        if (portalMode != RoomConnectionType.TriangularPortalXnZn)
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h3 * 256.0f, (z + 1) * 1024.0f),
                                Blocks[x, z].GetFaceTexture(face2), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), false);

                        if (portalMode != RoomConnectionType.TriangularPortalXpZp && blockType != BlockType.Wall)
                            AddTriangle(x, z, face1,
                                new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                Blocks[x, z].GetFaceTexture(face1), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), false);
                        break;
                    default:
                        throw new NotSupportedException("Unknown FloorDiagonalSplit");
                }
            }
            else if (Block.IsQuad(h0, h1, h2, h3) && (portalMode == RoomConnectionType.NoPortal))
            {
                AddRectangle(x, z, face1,
                    new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                    new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                    new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                    new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                    Blocks[x, z].GetFaceTexture(face1), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f));
            }
            else if (diagonalSplitXEqualsY || (portalMode == RoomConnectionType.TriangularPortalXnZp) || (portalMode == RoomConnectionType.TriangularPortalXpZn))
            {
                if (portalMode != RoomConnectionType.TriangularPortalXnZp)
                    AddTriangle(x, z, face2,
                        new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                        new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                        new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                        Blocks[x, z].GetFaceTexture(face2), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), true);

                if (portalMode != RoomConnectionType.TriangularPortalXpZn)
                    AddTriangle(x, z, face1,
                        new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                        new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                        new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                        Blocks[x, z].GetFaceTexture(face1), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), true);
            }
            else
            {
                if (portalMode != RoomConnectionType.TriangularPortalXpZp)
                    AddTriangle(x, z, face1,
                        new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                        new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                        new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                        Blocks[x, z].GetFaceTexture(face1), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), false);

                if (portalMode != RoomConnectionType.TriangularPortalXnZn)
                    AddTriangle(x, z, face2,
                        new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                        new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                        new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                        Blocks[x, z].GetFaceTexture(face2), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), false);
            }
        }

        public void UpdateCompletely()
        {
            BuildGeometry();
            CalculateLightingForThisRoom();
            UpdateBuffers();
        }

        public void UpdateOnlyGeometry()
        {
            BuildGeometry();
            CalculateLightingForThisRoom();
        }

        private enum FaceDirection
        {
            PositiveZ, NegativeZ, PositiveX, NegativeX, DiagonalFloor, DiagonalCeiling, DiagonalWall
        }

        private void AddVerticalFaces(int x, int z, FaceDirection direction, bool floor, bool ceiling, bool middle)
        {
            int xA, xB, zA, zB, yA, yB;

            Block otherBlock;
            TextureArea face;

            BlockFace qaFace, edFace, wsFace, rfFace, middleFace;
            int qA, qB, eA, eB, rA, rB, wA, wB, fA, fB, cA, cB;

            bool isOtherFloorDiagonal = false;
            bool isOtherCeilingDiagonal = false;

            switch (direction)
            {
                case FaceDirection.PositiveZ:
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
                    qaFace = BlockFace.PositiveZ_QA;
                    edFace = BlockFace.PositiveZ_ED;
                    middleFace = BlockFace.PositiveZ_Middle;
                    rfFace = BlockFace.PositiveZ_RF;
                    wsFace = BlockFace.PositiveZ_WS;

                    isOtherFloorDiagonal = otherBlock.FloorDiagonalSplit == DiagonalSplit.XnZp || otherBlock.FloorDiagonalSplit == DiagonalSplit.XpZp;
                    isOtherCeilingDiagonal = otherBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp || otherBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp;

                    // Try to adjust illegal combinations of heights
                    if (!isOtherFloorDiagonal)
                    {
                        if (qA < fA && qB > fB || qA > fA && qB < fB)
                        {
                            qA = fA;
                            qB = fB;
                        }

                        if (qA < cA && qB > cB || qA > cA && qB < cB || qA > cA && qB > cB)
                        {
                            qA = cA;
                            qB = cB;
                        }

                        if (eA < qA && eB > qB || eA > qA && eB < qB)
                        {
                            eA = qA;
                            eB = qB;
                        }

                        if (eA < cA && eB > cB || eA > cA && eB < cB)
                        {
                            eA = cA;
                            eB = cB;
                        }
                    }

                    if (!isOtherCeilingDiagonal)
                    {
                        if (wA < cA && wB > cB || wA > cA && wB < cB)
                        {
                            wA = cA;
                            wB = cB;
                        }

                        if (wA < fA && wB > fB || wA > fA && wB < fB || wA < fA && wB < fB)
                        {
                            wA = fA;
                            wB = fB;
                        }

                        if (rA < wA && rB > wB || rA > wA && rB < wB)
                        {
                            rA = wA;
                            rB = wB;
                        }

                        if (rA < fA && rB > fB || rA > fA && rB < fB)
                        {
                            rA = fA;
                            rB = fB;
                        }
                    }

                    if (!isOtherFloorDiagonal)
                    {
                        if (qA < fA) qA = fA;
                        if (qB < fB) qB = fB;

                        if (wA < fA) wA = fA;
                        if (wB < fB) wB = fB;
                    }

                    if (!isOtherCeilingDiagonal)
                    {
                        if (qA > cA) qA = cA;
                        if (qB > cB) qB = cB;

                        if (wA > cA) wA = cA;
                        if (wB > cB) wB = cB;
                    }

                    if (Blocks[x, z].WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = FindPortal(x, z, PortalDirection.WallNegativeZ);
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (Flipped && AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = Blocks[x, 1];

                        var qaNearA = nearBlock.QAFaces[2];
                        var qaNearB = nearBlock.QAFaces[3];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZp) qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZp) qaNearB = qaNearA;

                        var wsNearA = nearBlock.WSFaces[2];
                        var wsNearB = nearBlock.WSFaces[3];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                        var adjoiningBlock = adjoiningRoom.Blocks[facingX, adjoiningRoom.NumZSectors - 2];

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QAFaces[1];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QAFaces[0];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZn) qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZn) qBportal = qAportal;

                        qA = (int)Position.Y + qaNearA; 
                        qB = (int)Position.Y + qaNearB; 
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WSFaces[1];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WSFaces[0];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wBportal = wAportal;

                        wA = (int)Position.Y + wsNearA;
                        wB = (int)Position.Y + wsNearB; 
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;
                    }

                    if (Blocks[x, z].Type == BlockType.BorderWall)
                    {
                        if (Blocks[x, 1].WSFaces[2] < Blocks[x, z].QAFaces[1])
                        {
                            qA = Blocks[x, 1].WSFaces[2];
                        }

                        if (Blocks[x, 1].WSFaces[3] < Blocks[x, z].QAFaces[0])
                        {
                            qB = Blocks[x, 1].WSFaces[3];
                        }
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        qA = Blocks[x, z].QAFaces[0];
                        qB = Blocks[x, z].QAFaces[0];
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        qA = Blocks[x, z].QAFaces[1];
                        qB = Blocks[x, z].QAFaces[1];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        fA = otherBlock.QAFaces[2];
                        fB = otherBlock.QAFaces[2];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        fA = otherBlock.QAFaces[3];
                        fB = otherBlock.QAFaces[3];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        wA = Blocks[x, z].WSFaces[0];
                        wB = Blocks[x, z].WSFaces[0];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        wA = Blocks[x, z].WSFaces[1];
                        wB = Blocks[x, z].WSFaces[1];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        cA = otherBlock.WSFaces[2];
                        cB = otherBlock.WSFaces[2];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        cA = otherBlock.WSFaces[3];
                        cB = otherBlock.WSFaces[3];
                    }

                    break;

                case FaceDirection.NegativeZ:
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
                    qaFace = BlockFace.NegativeZ_QA;
                    edFace = BlockFace.NegativeZ_ED;
                    middleFace = BlockFace.NegativeZ_Middle;
                    rfFace = BlockFace.NegativeZ_RF;
                    wsFace = BlockFace.NegativeZ_WS;

                    isOtherFloorDiagonal = otherBlock.FloorDiagonalSplit == DiagonalSplit.XnZn || otherBlock.FloorDiagonalSplit == DiagonalSplit.XpZn;
                    isOtherCeilingDiagonal = otherBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn || otherBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn;

                    // Try to adjust illegal combinations of heights
                    if (!isOtherFloorDiagonal)
                    {
                        if (qA < fA && qB > fB || qA > fA && qB < fB)
                        {
                            qA = fA;
                            qB = fB;
                        }

                        if (qA < cA && qB > cB || qA > cA && qB < cB || qA > cA && qB > cB)
                        {
                            qA = cA;
                            qB = cB;
                        }

                        if (eA < qA && eB > qB || eA > qA && eB < qB)
                        {
                            eA = qA;
                            eB = qB;
                        }

                        if (eA < cA && eB > cB || eA > cA && eB < cB)
                        {
                            eA = cA;
                            eB = cB;
                        }
                    }

                    if (!isOtherCeilingDiagonal)
                    {
                        if (wA < cA && wB > cB || wA > cA && wB < cB)
                        {
                            wA = cA;
                            wB = cB;
                        }

                        if (wA < fA && wB > fB || wA > fA && wB < fB || wA < fA && wB < fB)
                        {
                            wA = fA;
                            wB = fB;
                        }

                        if (rA < wA && rB > wB || rA > wA && rB < wB)
                        {
                            rA = wA;
                            rB = wB;
                        }

                        if (rA < fA && rB > fB || rA > fA && rB < fB)
                        {
                            rA = fA;
                            rB = fB;
                        }
                    }

                    if (!isOtherFloorDiagonal)
                    {
                        if (qA < fA) qA = fA;
                        if (qB < fB) qB = fB;

                        if (wA < fA) wA = fA;
                        if (wB < fB) wB = fB;
                    }

                    if (!isOtherCeilingDiagonal)
                    {
                        if (qA > cA) qA = cA;
                        if (qB > cB) qB = cB;

                        if (wA > cA) wA = cA;
                        if (wB > cB) wB = cB;
                    }

                    if (Blocks[x, z].WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = FindPortal(x, z, PortalDirection.WallPositiveZ);
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (Flipped && AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = Blocks[x, NumZSectors - 2];

                        var qaNearA = nearBlock.QAFaces[0];
                        var qaNearB = nearBlock.QAFaces[1];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZn) qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZn) qaNearB = qaNearA;

                        var wsNearA = nearBlock.WSFaces[0];
                        var wsNearB = nearBlock.WSFaces[1];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                        var adjoiningBlock = adjoiningRoom.Blocks[facingX, 1];

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QAFaces[3];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QAFaces[2];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZp) qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZp) qBportal = qAportal;

                        qA = (int)Position.Y + qaNearA;  
                        qB = (int)Position.Y + qaNearB;  
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WSFaces[3];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WSFaces[2];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp) wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp) wBportal = wAportal;

                        wA = (int)Position.Y + wsNearA;  
                        wB = (int)Position.Y + wsNearB;  
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;
                    }

                    if (Blocks[x, z].Type == BlockType.BorderWall)
                    {
                        if (Blocks[x, NumZSectors - 2].WSFaces[0] < Blocks[x, z].QAFaces[3])
                        {
                            qA = Blocks[x, NumZSectors - 2].WSFaces[0];
                        }

                        if (Blocks[x, NumZSectors - 2].WSFaces[1] < Blocks[x, z].QAFaces[2])
                        {
                            qB = Blocks[x, NumZSectors - 2].WSFaces[1];
                        }
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        qA = Blocks[x, z].QAFaces[3];
                        qB = Blocks[x, z].QAFaces[3];
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        qA = Blocks[x, z].QAFaces[2];
                        qB = Blocks[x, z].QAFaces[2];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        fA = otherBlock.QAFaces[0];
                        fB = otherBlock.QAFaces[0];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        fA = otherBlock.QAFaces[1];
                        fB = otherBlock.QAFaces[1];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        wA = Blocks[x, z].WSFaces[3];
                        wB = Blocks[x, z].WSFaces[3];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        wA = Blocks[x, z].WSFaces[2];
                        wB = Blocks[x, z].WSFaces[2];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        cA = otherBlock.WSFaces[0];
                        cB = otherBlock.WSFaces[0];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        cA = otherBlock.WSFaces[1];
                        cB = otherBlock.WSFaces[1];
                    }

                    break;

                case FaceDirection.PositiveX:
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
                    qaFace = BlockFace.PositiveX_QA;
                    edFace = BlockFace.PositiveX_ED;
                    middleFace = BlockFace.PositiveX_Middle;
                    rfFace = BlockFace.PositiveX_RF;
                    wsFace = BlockFace.PositiveX_WS;

                    isOtherFloorDiagonal = otherBlock.FloorDiagonalSplit == DiagonalSplit.XpZp || otherBlock.FloorDiagonalSplit == DiagonalSplit.XpZn;
                    isOtherCeilingDiagonal = otherBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp || otherBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn;

                    // Try to adjust illegal combinations of heights
                    if (!isOtherFloorDiagonal)
                    {
                        if (qA < fA && qB > fB || qA > fA && qB < fB)
                        {
                            qA = fA;
                            qB = fB;
                        }

                        if (qA < cA && qB > cB || qA > cA && qB < cB || qA > cA && qB > cB)
                        {
                            qA = cA;
                            qB = cB;
                        }

                        if (eA < qA && eB > qB || eA > qA && eB < qB)
                        {
                            eA = qA;
                            eB = qB;
                        }

                        if (eA < cA && eB > cB || eA > cA && eB < cB)
                        {
                            eA = cA;
                            eB = cB;
                        }
                    }

                    if (!isOtherCeilingDiagonal)
                    {
                        if (wA < cA && wB > cB || wA > cA && wB < cB)
                        {
                            wA = cA;
                            wB = cB;
                        }

                        if (wA < fA && wB > fB || wA > fA && wB < fB || wA < fA && wB < fB)
                        {
                            wA = fA;
                            wB = fB;
                        }

                        if (rA < wA && rB > wB || rA > wA && rB < wB)
                        {
                            rA = wA;
                            rB = wB;
                        }

                        if (rA < fA && rB > fB || rA > fA && rB < fB)
                        {
                            rA = fA;
                            rB = fB;
                        }
                    }

                    if (!isOtherFloorDiagonal)
                    {
                        if (qA < fA) qA = fA;
                        if (qB < fB) qB = fB;

                        if (wA < fA) wA = fA;
                        if (wB < fB) wB = fB;
                    }

                    if (!isOtherCeilingDiagonal)
                    {
                        if (qA > cA) qA = cA;
                        if (qB > cB) qB = cB;

                        if (wA > cA) wA = cA;
                        if (wB > cB) wB = cB;
                    }

                    if (Blocks[x, z].WallPortal != null)
                    {                        
                        // Get the adjoining room of the portal
                        var portal = FindPortal(x, z, PortalDirection.WallNegativeX);
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (Flipped && AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = Blocks[1, z];

                        var qaNearA = nearBlock.QAFaces[3];
                        var qaNearB = nearBlock.QAFaces[0];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZn) qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZp) qaNearB = qaNearA;

                        var wsNearA = nearBlock.WSFaces[3];
                        var wsNearB = nearBlock.WSFaces[0];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp) wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);

                        var adjoiningBlock = adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 2, facingZ];

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QAFaces[2];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QAFaces[1];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZn) qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZp) qBportal = qAportal;

                        qA = (int)Position.Y + qaNearA;
                        qB = (int)Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WSFaces[2];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WSFaces[1];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp) wBportal = wAportal;

                        wA = (int)Position.Y + wsNearA;
                        wB = (int)Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;
                    }

                    if (Blocks[x, z].Type == BlockType.BorderWall)
                    {
                        if (Blocks[1, z].WSFaces[3] < Blocks[x, z].QAFaces[2])
                        {
                            qA = Blocks[1, z].WSFaces[3];
                        }

                        if (Blocks[1, z].WSFaces[0] < Blocks[x, z].QAFaces[1])
                        {
                            qB = Blocks[1, z].WSFaces[0];
                        }
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        qA = Blocks[x, z].QAFaces[1];
                        qB = Blocks[x, z].QAFaces[1];
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        qA = Blocks[x, z].QAFaces[2];
                        qB = Blocks[x, z].QAFaces[2];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        fA = otherBlock.QAFaces[0];
                        fB = otherBlock.QAFaces[0];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        fA = otherBlock.QAFaces[3];
                        fB = otherBlock.QAFaces[3];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        wA = Blocks[x, z].WSFaces[1];
                        wB = Blocks[x, z].WSFaces[1];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        wA = Blocks[x, z].WSFaces[2];
                        wB = Blocks[x, z].WSFaces[2];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        cA = otherBlock.WSFaces[0];
                        cB = otherBlock.WSFaces[0];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        cA = otherBlock.WSFaces[3];
                        cB = otherBlock.WSFaces[3];
                    }

                    break;

                case FaceDirection.DiagonalFloor:
                    switch (Blocks[x, z].FloorDiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
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
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        case DiagonalSplit.XnZn:
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
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        case DiagonalSplit.XnZp:
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
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
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
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                    }

                    break;

                case FaceDirection.DiagonalCeiling:
                    switch (Blocks[x, z].CeilingDiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
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
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        case DiagonalSplit.XnZn:
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
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        case DiagonalSplit.XnZp:
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
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
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
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
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
                    qaFace = BlockFace.NegativeX_QA;
                    edFace = BlockFace.NegativeX_ED;
                    middleFace = BlockFace.NegativeX_Middle;
                    rfFace = BlockFace.NegativeX_RF;
                    wsFace = BlockFace.NegativeX_WS;

                    isOtherFloorDiagonal = otherBlock.FloorDiagonalSplit == DiagonalSplit.XnZp || otherBlock.FloorDiagonalSplit == DiagonalSplit.XnZn;
                    isOtherCeilingDiagonal = otherBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp || otherBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn;

                    // Try to adjust illegal combinations of heights
                    if (!isOtherFloorDiagonal)
                    {
                        if (qA < fA && qB > fB || qA > fA && qB < fB)
                        {
                            qA = fA;
                            qB = fB;
                        }

                        if (qA < cA && qB > cB || qA > cA && qB < cB || qA > cA && qB > cB)
                        {
                            qA = cA;
                            qB = cB;
                        }

                        if (eA < qA && eB > qB || eA > qA && eB < qB)
                        {
                            eA = qA;
                            eB = qB;
                        }

                        if (eA < cA && eB > cB || eA > cA && eB < cB)
                        {
                            eA = cA;
                            eB = cB;
                        }
                    }

                    if (!isOtherCeilingDiagonal)
                    {
                        if (wA < cA && wB > cB || wA > cA && wB < cB)
                        {
                            wA = cA;
                            wB = cB;
                        }

                        if (wA < fA && wB > fB || wA > fA && wB < fB || wA < fA && wB < fB)
                        {
                            wA = fA;
                            wB = fB;
                        }

                        if (rA < wA && rB > wB || rA > wA && rB < wB)
                        {
                            rA = wA;
                            rB = wB;
                        }

                        if (rA < fA && rB > fB || rA > fA && rB < fB)
                        {
                            rA = fA;
                            rB = fB;
                        }
                    }

                    if (!isOtherFloorDiagonal)
                    {
                        if (qA < fA) qA = fA;
                        if (qB < fB) qB = fB;

                        if (wA < fA) wA = fA;
                        if (wB < fB) wB = fB;
                    }

                    if (!isOtherCeilingDiagonal)
                    {
                        if (qA > cA) qA = cA;
                        if (qB > cB) qB = cB;

                        if (wA > cA) wA = cA;
                        if (wB > cB) wB = cB;
                    }

                    if (Blocks[x, z].WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = FindPortal(x, z, PortalDirection.WallNegativeX);
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (Flipped && AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = Blocks[NumXSectors - 2, z];

                        var qaNearA = nearBlock.QAFaces[1];
                        var qaNearB = nearBlock.QAFaces[2];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZp) qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZn) qaNearB = qaNearA;

                        var wsNearA = nearBlock.WSFaces[1];
                        var wsNearB = nearBlock.WSFaces[2];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp) wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);

                        var adjoiningBlock = adjoiningRoom.Blocks[1, facingZ];

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QAFaces[0];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QAFaces[3];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZp) qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZn) qBportal = qAportal;

                        qA = (int)Position.Y + qaNearA;
                        qB = (int)Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WSFaces[0];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WSFaces[3];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp) wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wBportal = wAportal;

                        wA = (int)Position.Y + wsNearA;
                        wB = (int)Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;
                    }

                    if (Blocks[x, z].Type == BlockType.BorderWall)
                    {
                        if (Blocks[NumXSectors - 2, z].WSFaces[1] < Blocks[x, z].QAFaces[0])
                        {
                            qA = Blocks[NumXSectors - 2, z].WSFaces[1];
                        }

                        if (Blocks[NumXSectors - 2, z].WSFaces[2] < Blocks[x, z].QAFaces[3])
                        {
                            qB = Blocks[NumXSectors - 2, z].WSFaces[2];
                        }
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        qA = Blocks[x, z].QAFaces[0];
                        qB = Blocks[x, z].QAFaces[0];
                    }

                    if (Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        qA = Blocks[x, z].QAFaces[3];
                        qB = Blocks[x, z].QAFaces[3];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        fA = otherBlock.QAFaces[1];
                        fB = otherBlock.QAFaces[1];
                    }

                    if (otherBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        fA = otherBlock.QAFaces[2];
                        fB = otherBlock.QAFaces[2];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        wA = Blocks[x, z].WSFaces[0];
                        wB = Blocks[x, z].WSFaces[0];
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        wA = Blocks[x, z].WSFaces[3];
                        wB = Blocks[x, z].WSFaces[3];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        cA = otherBlock.WSFaces[1];
                        cB = otherBlock.WSFaces[1];
                    }

                    if (otherBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        cA = otherBlock.WSFaces[2];
                        cB = otherBlock.WSFaces[2];
                    }

                    break;
            }

            bool subdivide = false;

            if (qA >= fA && qB >= fB && !(qA == fA && qB == fB) && floor)
            {
                // Check for subdivision
                yA = fA;
                yB = fB;

                if (eA >= yA && eB >= yB && qA >= eA && qB >= eB && !(eA == yA && eB == yB))
                {
                    subdivide = true;
                    yA = eA;
                    yB = eB;
                }

                // QA and ED
                face = Blocks[x, z].GetFaceTexture(qaFace);

                // QA
                if (qA <= cA && qB <= cB)
                {
                    if (qA > yA && qB > yB)
                        AddRectangle(x, z, qaFace,
                            new Vector3(xA * 1024.0f, qA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, qB * 256.0f, zB * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            face, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f));
                    else if (qA == yA && qB > yB)
                        AddTriangle(x, z, qaFace,
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, qB * 256.0f, zB * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            face, new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), false);
                    else if (qA > yA && qB == yB)
                        AddTriangle(x, z, qaFace,
                            new Vector3(xA * 1024.0f, qA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            face, new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), true);
                }

                // ED
                if (subdivide)
                {
                    yA = fA;
                    yB = fB;

                    face = Blocks[x, z].GetFaceTexture(edFace);

                    if (eA > yA && eB > yB)
                        AddRectangle(x, z, edFace,
                            new Vector3(xA * 1024.0f, eA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, eB * 256.0f, zB * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            face, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f));
                    else if (eA > yA && eB == yB)
                        AddTriangle(x, z, edFace,
                            new Vector3(xA * 1024.0f, eA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            face, new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), true);
                    else if (eA == yA && eB > yB)
                        AddTriangle(x, z, edFace,
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, eB * 256.0f, zB * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            face, new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), false);
                }
            }

            subdivide = false;

            if (cA >= wA && cB >= wB && !(wA == cA && wB == cB) && ceiling)
            {
                // Check for subdivision
                yA = cA;
                yB = cB;

                if (rA <= yA && rB <= yB && wA <= rA && wB <= rB && !(rA == yA && rB == yB))
                {
                    subdivide = true;
                    yA = rA;
                    yB = rB;
                }

                // WS and RF
                if (ceiling)
                {
                    face = Blocks[x, z].GetFaceTexture(wsFace);

                    // WS
                    if (wA >= fA && wB >= fB)
                    {
                        if (wA < yA && wB < yB)
                            AddRectangle(x, z, wsFace,
                                new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                new Vector3(xB * 1024.0f, wB * 256.0f, zB * 1024.0f),
                                new Vector3(xA * 1024.0f, wA * 256.0f, zA * 1024.0f),
                                face, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f));
                        else if (wA < yA && wB == yB)
                            AddTriangle(x, z, wsFace,
                                new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                new Vector3(xA * 1024.0f, wA * 256.0f, zA * 1024.0f),
                                face, new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), true);
                        else if (wA == yA && wB < yB)
                            AddTriangle(x, z, wsFace,
                                new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                new Vector3(xB * 1024.0f, wB * 256.0f, zB * 1024.0f),
                                face, new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), false);
                    }

                    // RF
                    if (subdivide)
                    {
                        yA = cA;
                        yB = cB;

                        face = Blocks[x, z].GetFaceTexture(rfFace);

                        if (rA < yA && rB < yB)
                            AddRectangle(x, z, rfFace,
                                new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                new Vector3(xB * 1024.0f, rB * 256.0f, zB * 1024.0f),
                                new Vector3(xA * 1024.0f, rA * 256.0f, zA * 1024.0f),
                                face, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f));
                        else if (rA < yA && rB == yB)
                            AddTriangle(x, z, rfFace,
                                new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                new Vector3(xA * 1024.0f, rA * 256.0f, zA * 1024.0f),
                                face, new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), true);
                        else if (rA == yA && rB < yB)
                            AddTriangle(x, z, rfFace,
                                new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                                new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                                new Vector3(xB * 1024.0f, rB * 256.0f, zB * 1024.0f),
                                face, new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), false);
                    }
                }
            }

            if (!middle)
                return;

            face = Blocks[x, z].GetFaceTexture(middleFace);

            yA = wA > cA ? cA : wA;
            yB = wB > cB ? cB : wB;
            int yD = qA < fA ? fA : qA;
            int yC = qB < fB ? fB : qB;
            // middle
            if (yA != yD && yB != yC)
                AddRectangle(x, z, middleFace,
                    new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                    new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                    new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                    new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                    face, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f));

            else if (yA != yD && yB == yC)
                AddTriangle(x, z, middleFace,
                    new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                    new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                    new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                    face, new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), true);

            else if (yA == yD && yB != yC)
                AddTriangle(x, z, middleFace,
                    new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                    new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                    new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                    face, new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), false);
        }

        private PortalInstance FindPortal(int x, int z, PortalDirection type)
        {
            if (Blocks[x, z].WallPortal != null)
                return Blocks[x, z].WallPortal;
            if (Blocks[x, z].FloorPortal != null && type == PortalDirection.Floor)
                return Blocks[x, z].FloorPortal;
            if (Blocks[x, z].CeilingPortal != null && type == PortalDirection.Ceiling)
                return Blocks[x, z].CeilingPortal;

            return null;
        }

        private void AddRectangle(int x, int z, BlockFace face, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, TextureArea texture, Vector2 editorUV0, Vector2 editorUV1, Vector2 editorUV2, Vector2 editorUV3)
        {
            var sectorVertices = _sectorVertices[x, z];
            int sectorVerticesStart = sectorVertices.Count;

            sectorVertices.Add(new EditorVertex { Position = p1, UV = texture.TexCoord1, EditorUV = editorUV1 });
            sectorVertices.Add(new EditorVertex { Position = p2, UV = texture.TexCoord2, EditorUV = editorUV2 });
            sectorVertices.Add(new EditorVertex { Position = p0, UV = texture.TexCoord0, EditorUV = editorUV0 });
            sectorVertices.Add(new EditorVertex { Position = p3, UV = texture.TexCoord3, EditorUV = editorUV3 });
            sectorVertices.Add(new EditorVertex { Position = p0, UV = texture.TexCoord0, EditorUV = editorUV0 });
            sectorVertices.Add(new EditorVertex { Position = p2, UV = texture.TexCoord2, EditorUV = editorUV2 });

            _sectorFaceVertexVertexRange[x, z, (int)face] = new VertexRange { Start = sectorVerticesStart, Count = 6 };
        }

        private void AddTriangle(int x, int z, BlockFace face, Vector3 p0, Vector3 p1, Vector3 p2, TextureArea texture, Vector2 editorUV0, Vector2 editorUV1, Vector2 editorUV2, bool IsXEqualYDiagonal)
        {
            var sectorVertices = _sectorVertices[x, z];
            int sectorVerticesStart = sectorVertices.Count;

            Vector2 editorUvFactor = new Vector2(IsXEqualYDiagonal ? -1.0f : 1.0f, -1.0f);
            sectorVertices.Add(new EditorVertex { Position = p0, UV = texture.TexCoord0, EditorUV = editorUV0 * editorUvFactor });
            sectorVertices.Add(new EditorVertex { Position = p1, UV = texture.TexCoord1, EditorUV = editorUV1 * editorUvFactor });
            sectorVertices.Add(new EditorVertex { Position = p2, UV = texture.TexCoord2, EditorUV = editorUV2 * editorUvFactor });

            _sectorFaceVertexVertexRange[x, z, (int)face] = new VertexRange { Start = sectorVerticesStart, Count = 3 };
        }

        public struct IntersectionInfo
        {
            public DrawingPoint Pos;
            public BlockFace Face;
            public float Distance;
        };

        public IntersectionInfo? RayIntersectsGeometry(Ray ray)
        {
            IntersectionInfo result = new IntersectionInfo { Distance = float.NaN };
            for (int x = 0; x < NumXSectors; x++)
                for (int z = 0; z < NumZSectors; z++)
                    for (BlockFace face = 0; face < Block.FaceCount; face++)
                    {
                        // Check for intersection on the correct side
                        var sectorVertices = _sectorVertices[x, z];
                        VertexRange vertexRange = _sectorFaceVertexVertexRange[x, z, (int)face];
                        for (int i = 0; i < vertexRange.Count; i += 3)
                        {
                            var p0 = sectorVertices[vertexRange.Start + i].Position;
                            var p1 = sectorVertices[vertexRange.Start + i + 1].Position;
                            var p2 = sectorVertices[vertexRange.Start + i + 2].Position;

                            float distance;
                            if (ray.Intersects(ref p0, ref p1, ref p2, out distance))
                            {
                                var normal = Vector3.Cross(p1 - p0, p2 - p0);
                                if (Vector3.Dot(ray.Direction, normal) <= 0)
                                    if (!(distance > result.Distance))
                                        result = new IntersectionInfo() { Distance = distance, Face = face, Pos = new DrawingPoint(x, z) };
                            }
                        }
                    }

            if (float.IsNaN(result.Distance))
                return null;
            return result;
        }



        private bool RayTraceCheckFloorCeiling(int x, int y, int z, int xLight, int zLight)
        {
            int currentX = (x / 1024) - (x > xLight ? 1 : 0);
            int currentZ = (z / 1024) - (z > zLight ? 1 : 0);

            Block block = Blocks[currentX, currentZ];
            int floorMin = block.FloorMin;
            int ceilingMax = block.CeilingMax;

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
                            ((currentBlock.WSFaces[0] + currentBlock.WSFaces[3]) / 2 < currentYclick) ||
                            currentBlock.Type == BlockType.Wall)
                        {
                            return false;
                        }
                    }

                    if (currentX == maxX)
                    {
                        return true;
                    }

                    if (currentXblock > 0)
                    {
                        var currentBlock = Blocks[currentXblock - 1, currentZblock];
                        var nextBlock = Blocks[currentXblock, currentZblock];

                        if (((currentBlock.QAFaces[2] + currentBlock.QAFaces[1]) / 2 > currentYclick) ||
                            ((currentBlock.WSFaces[2] + currentBlock.WSFaces[1]) / 2 < currentYclick) ||
                            currentBlock.Type == BlockType.Wall ||
                            ((nextBlock.QAFaces[0] + nextBlock.QAFaces[3]) / 2 > currentYclick) ||
                            ((nextBlock.WSFaces[0] + nextBlock.WSFaces[3]) / 2 < currentYclick) ||
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
                            ((currentBlock.WSFaces[2] + currentBlock.WSFaces[3]) / 2 < currentYclick) ||
                            currentBlock.Type == BlockType.Wall)
                        {
                            return false;
                        }
                    }

                    if (currentZ == maxZ)
                    {
                        return true;
                    }

                    if (currentZblock > 0)
                    {
                        var currentBlock = Blocks[currentXblock, currentZblock - 1];
                        var nextBlock = Blocks[currentXblock, currentZblock];

                        if (((currentBlock.QAFaces[0] + currentBlock.QAFaces[1]) / 2 > currentYclick) ||
                            ((currentBlock.WSFaces[0] + currentBlock.WSFaces[1]) / 2 < currentYclick) ||
                            currentBlock.Type == BlockType.Wall ||
                            ((nextBlock.QAFaces[2] + nextBlock.QAFaces[3]) / 2 > currentYclick) ||
                            ((nextBlock.WSFaces[2] + nextBlock.WSFaces[3]) / 2 < currentYclick) ||
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

        public void CalculateLightingForThisRoom()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // Reset all lighting
            for (int i = 0; i < _allVertices.Count; ++i)
            {
                var vertex = _allVertices[i];
                vertex.FaceColor = AmbientLight;
                _allVertices[i] = vertex;
            }

            // Calculate lighting
            for (int x = 0; x < NumXSectors; x++)
                for (int z = 0; z < NumZSectors; z++)
                    for (BlockFace f = 0; f < Block.FaceCount; f++)
                        if (IsFaceDefined(x, z, f))
                            CalculateLighting(x, z, f);

            // Calculate average of shared vertices
            Dictionary<Vector3, List<int>> sharedVertices = new Dictionary<Vector3, List<int>>();
            for (int i = 0; i < _allVertices.Count; ++i)
            {
                Vector3 position = _allVertices[i].Position;
                List<int> list;
                if (!sharedVertices.TryGetValue(position, out list))
                    sharedVertices.Add(position, list = new List<int>());
                list.Add(i);
            }

            foreach (var pair in sharedVertices)
            {
                Vector4 faceColorSum = new Vector4(0);
                foreach (var vertexIndex in pair.Value)
                    faceColorSum += _allVertices[vertexIndex].FaceColor;
                faceColorSum /= pair.Value.Count;
                foreach (var vertexIndex in pair.Value)
                {
                    var vertex = _allVertices[vertexIndex];
                    vertex.FaceColor = faceColorSum;
                    _allVertices[vertexIndex] = vertex;
                }
            }

            watch.Stop();
        }

        private void CalculateLighting(int x, int z, BlockFace face)
        {
            // No Linq here because it's slow
            List<LightInstance> lights = new List<LightInstance>();
            foreach (var instance in _objects)
            {
                LightInstance light = instance as LightInstance;
                if (light != null)
                    lights.Add(light);
            }

            VertexRange range = GetFaceVertexRange(x, z, face);
            if (range.Count == 0)
                return;

            var normal = Vector3.Cross(
                _allVertices[range.Start + 1].Position - _allVertices[range.Start].Position,
                _allVertices[range.Start + 2].Position - _allVertices[range.Start].Position);
            normal.Normalize();

            for (int i = 0; i < range.Count; ++i)
            {
                var position = _allVertices[range.Start + i].Position;

                int r = (int)(AmbientLight.X * 128);
                int g = (int)(AmbientLight.Y * 128);
                int b = (int)(AmbientLight.Z * 128);

                foreach (var light in lights) // No Linq here because it's slow
                {
                    if ((!light.Enabled) || (!light.IsStaticallyUsed))
                        continue;

                    switch (light.Type)
                    {
                        case LightType.Light:
                        case LightType.Shadow:
                            if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * 1024.0f)
                            {
                                // Get the distance between light and vertex
                                float distance = Math.Abs((position - light.Position).Length());

                                // If distance is greater than light out radius, then skip this light
                                if (distance > light.OuterRange * 1024.0f)
                                    continue;

                                // Calculate light diffuse value
                                int diffuse = (int)(light.Intensity * 8192);

                                // Calculate the length squared of the normal vector
                                float dotN = Vector3.Dot(normal, normal);

                                // Do raytracing
                                if (dotN <= 0 ||
                                    !RayTraceCheckFloorCeiling((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                    !RayTraceX((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                    !RayTraceZ((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                {
                                    if (light.CastsShadows)
                                        continue;
                                }

                                // Calculate the attenuation
                                var attenuaton = (light.OuterRange * 1024.0f - distance) / (light.OuterRange * 1024.0f - light.InnerRange * 1024.0f);
                                if (attenuaton > 1.0f)
                                    attenuaton = 1.0f;
                                if (attenuaton <= 0.0f)
                                    continue;

                                // Calculate final light color
                                int finalIntensity = (int)(dotN * attenuaton * diffuse);

                                r += (int)(finalIntensity * light.Color.X / 64.0f);
                                g += (int)(finalIntensity * light.Color.Y / 64.0f);
                                b += (int)(finalIntensity * light.Color.Z / 64.0f);
                            }
                            break;
                        case LightType.Effect:
                            if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * 1024.0f)
                            {
                                int x1 = (int)(Math.Floor(light.Position.X / 1024.0f) * 1024);
                                int z1 = (int)(Math.Floor(light.Position.Z / 1024.0f) * 1024);
                                int x2 = (int)(Math.Ceiling(light.Position.X / 1024.0f) * 1024);
                                int z2 = (int)(Math.Ceiling(light.Position.Z / 1024.0f) * 1024);

                                // TODO: winroomedit was supporting effect lights placed on vertical faces and effects light was applied to owning face
                                // ReSharper disable CompareOfFloatsByEqualityOperator
                                if (((position.X == x1 && position.Z == z1) || (position.X == x1 && position.Z == z2) || (position.X == x2 && position.Z == z1) ||
                                     (position.X == x2 && position.Z == z2)) && position.Y <= light.Position.Y)
                                {
                                    int finalIntensity = (int)(light.Intensity * 8192 * 0.25f);

                                    r += (int)(finalIntensity * light.Color.X / 64.0f);
                                    g += (int)(finalIntensity * light.Color.Y / 64.0f);
                                    b += (int)(finalIntensity * light.Color.Z / 64.0f);
                                }
                                // ReSharper restore CompareOfFloatsByEqualityOperator
                            }
                            break;
                        case LightType.Sun:
                            {
                                // Do raytracing now for saving CPU later
                                if (!RayTraceCheckFloorCeiling((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                    !RayTraceX((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                    !RayTraceZ((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                {
                                    if (light.CastsShadows)
                                        continue;
                                }

                                // Calculate the light direction
                                var lightDirection = light.GetDirection();

                                // calcolo la luce diffusa
                                float diffuse = -Vector3.Dot(lightDirection, normal);

                                if (diffuse <= 0)
                                    continue;

                                if (diffuse > 1)
                                    diffuse = 1.0f;


                                int finalIntensity = (int)(diffuse * light.Intensity * 8192);
                                if (finalIntensity < 0)
                                    continue;

                                r += (int)(finalIntensity * light.Color.X / 64.0f);
                                g += (int)(finalIntensity * light.Color.Y / 64.0f);
                                b += (int)(finalIntensity * light.Color.Z / 64.0f);
                            }
                            break;
                        case LightType.Spot:
                            if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * 1024.0f)
                            {
                                // Calculate the ray from light to vertex
                                var lightVector = position - light.Position;
                                lightVector.Normalize();

                                // Get the distance between light and vertex
                                float distance = Math.Abs((position - light.Position).Length());

                                // If distance is greater than light length, then skip this light
                                if (distance > light.OuterRange * 1024.0f)
                                    continue;

                                // Calculate the light direction
                                var lightDirection = light.GetDirection();

                                // Calculate the cosines values for In, Out
                                double d = Vector3.Dot(lightVector, lightDirection);
                                double cosI2 = Math.Cos(MathUtil.DegreesToRadians(light.InnerAngle));
                                double cosO2 = Math.Cos(MathUtil.DegreesToRadians(light.OuterAngle));

                                if (d < cosO2)
                                    continue;

                                if (!RayTraceCheckFloorCeiling((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                    !RayTraceX((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                    !RayTraceZ((int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                {
                                    if (light.CastsShadows)
                                        continue;
                                }

                                // Calculate light diffuse value
                                float factor = (float)(1.0f - (d - cosI2) / (cosO2 - cosI2));
                                if (factor > 1.0f)
                                    factor = 1.0f;
                                if (factor <= 0.0f)
                                    continue;

                                float attenuation = 1.0f;
                                if (distance >= light.InnerRange * 1024.0f)
                                    attenuation = 1.0f - (distance - light.InnerRange * 1024.0f) / (light.OuterRange * 1024.0f - light.InnerRange * 1024.0f);

                                if (attenuation > 1.0f)
                                    attenuation = 1.0f;
                                if (attenuation < 0.0f)
                                    continue;

                                float dot1 = -Vector3.Dot(lightDirection, normal);
                                if (dot1 < 0.0f)
                                    continue;
                                if (dot1 > 1.0f)
                                    dot1 = 1.0f;

                                int finalIntensity = (int)(attenuation * dot1 * factor * light.Intensity * 8192);

                                r += (int)(finalIntensity * light.Color.X / 64.0f);
                                g += (int)(finalIntensity * light.Color.Y / 64.0f);
                                b += (int)(finalIntensity * light.Color.Z / 64.0f);
                            }
                            break;
                    }
                }

                if (r < 0)
                    r = 0;
                if (g < 0)
                    g = 0;
                if (b < 0)
                    b = 0;

                // Apply color
                EditorVertex vertex = _allVertices[range.Start + i];

                vertex.FaceColor.X = r * (1.0f / 128.0f);
                vertex.FaceColor.Y = g * (1.0f / 128.0f);
                vertex.FaceColor.Z = b * (1.0f / 128.0f);
                vertex.FaceColor.W = 255.0f;

                _allVertices[range.Start + i] = vertex;
            }
        }

        public List<EditorVertex> GetRoomVertices()
        {
            return _allVertices;
        }

        public void UpdateBuffers()
        {
            // HACK
            if (_allVertices.Count == 0)
                return;

            // HACK
            if ((_vertexBuffer == null) || (_vertexBuffer.ElementCount < _allVertices.Count))
            {
                _vertexBuffer?.Dispose();
                _vertexBuffer = Buffer.New(DeviceManager.DefaultDeviceManager.Device, _allVertices.ToArray(), BufferFlags.VertexBuffer);
            }

            _vertexBuffer.SetData<EditorVertex>(_allVertices.ToArray());
        }

        public Buffer<EditorVertex> VertexBuffer => _vertexBuffer;

        public Matrix Transform => Matrix.Translation(new Vector3(Position.X * 1024.0f, Position.Y * 256.0f, Position.Z * 1024.0f));

        public int GetHighestCorner(Rectangle area)
        {
            area = area.Intersect(LocalArea);

            int max = int.MinValue;
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    if (!Blocks[x, z].IsAnyWall)
                        max = Math.Max(max, Blocks[x, z].CeilingMax);
            return max;
        }

        public int GetHighestCorner()
        {
            return GetHighestCorner(new Rectangle(1, 1, NumXSectors - 2, NumZSectors - 2));
        }

        public int GetLowestCorner(Rectangle area)
        {
            area = area.Intersect(LocalArea);

            int min = int.MaxValue;
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    if (!Blocks[x, z].IsAnyWall)
                        min = Math.Min(min, Blocks[x, z].FloorMin);
            return min;
        }

        public int GetLowestCorner()
        {
            return GetLowestCorner(new Rectangle(1, 1, NumXSectors - 2, NumZSectors - 2));
        }

        public Vector3 WorldPos => new Vector3(Position.X * 1024.0f, Position.Y * 256.0f, Position.Z * 1024.0f);

        public Vector3 GetLocalCenter()
        {
            float ceilingHeight = GetHighestCorner();
            float floorHeight = GetLowestCorner();
            float posX = NumXSectors * (0.5f * 1024.0f);
            float posY = (floorHeight + ceilingHeight) * (0.5f * 256.0f);
            float posZ = NumZSectors * (0.5f * 1024.0f);
            return new Vector3(posX, posY, posZ);
        }

        private Block GetBlockIfFloor(int x, int z)
        {
            Block block = Blocks.TryGet(x, z);
            if ((block != null) && block.IsAnyWall)
                return null;
            return block;
        }

        ///<param name="x">The X-coordinate. The point at room.Position it at (0, 0)</param>
        ///<param name="z">The Z-coordinate. The point at room.Position it at (0, 0)</param>
        public VerticalSpace? GetHeightAtPoint(int x, int z, Func<float?, float?, float?, float?, float> combineFloor, Func<float?, float?, float?, float?, float> combineCeiling)
        {
            Block blockXnZn = GetBlockIfFloor(x - 1, z - 1);
            Block blockXnZp = GetBlockIfFloor(x - 1, z);
            Block blockXpZn = GetBlockIfFloor(x, z - 1);
            Block blockXpZp = GetBlockIfFloor(x, z);
            if ((blockXnZn == null) && (blockXnZp == null) && (blockXpZn == null) && (blockXpZp == null))
                return null;

            return new VerticalSpace
            {
                FloorY = combineFloor(
                    blockXnZn?.QAFaces[Block.FaceXpZp],
                    blockXnZp?.QAFaces[Block.FaceXpZn],
                    blockXpZn?.QAFaces[Block.FaceXnZp],
                    blockXpZp?.QAFaces[Block.FaceXnZn]),
                CeilingY = combineCeiling(
                    blockXnZn?.WSFaces[Block.FaceXpZp],
                    blockXnZp?.WSFaces[Block.FaceXpZn],
                    blockXpZn?.WSFaces[Block.FaceXnZp],
                    blockXpZp?.WSFaces[Block.FaceXnZn])
            };
        }

        public VerticalSpace? GetHeightInArea(Rectangle area, Func<float?, float?, float?, float?, float> combineFloor, Func<float?, float?, float?, float?, float> combineCeiling)
        {
            VerticalSpace? result = null;
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    VerticalSpace? verticalSpace = GetHeightAtPoint(x, z, combineFloor, combineCeiling);
                    if (verticalSpace == null)
                        continue;
                    result = new VerticalSpace
                    {
                        FloorY = combineFloor(verticalSpace?.FloorY, result?.FloorY, null, null),
                        CeilingY = combineCeiling(verticalSpace?.CeilingY, result?.CeilingY, null, null)
                    };
                }
            return result;
        }


        private static float Average(float? Height0, float? Height1, float? Height2, float? Height3)
        {
            int Count = (Height0.HasValue ? 1 : 0) + (Height1.HasValue ? 1 : 0) + (Height2.HasValue ? 1 : 0) + (Height3.HasValue ? 1 : 0);
            float Sum = (Height0 ?? 0) + (Height1 ?? 0) + (Height2 ?? 0) + (Height3 ?? 0);
            return Sum / Count;
        }

        private static float Max(float? Height0, float? Height1, float? Height2, float? Height3)
        {
            return Math.Max(Math.Max(Height0 ?? float.NegativeInfinity, Height1 ?? float.NegativeInfinity),
                Math.Max(Height2 ?? float.NegativeInfinity, Height3 ?? float.NegativeInfinity));
        }

        private static float Min(float? Height0, float? Height1, float? Height2, float? Height3)
        {
            return Math.Min(Math.Min(Height0 ?? float.PositiveInfinity, Height1 ?? float.PositiveInfinity),
                Math.Min(Height2 ?? float.PositiveInfinity, Height3 ?? float.PositiveInfinity));
        }

        public VerticalSpace? GetHeightAtPointAverage(int x, int z)
        {
            return GetHeightAtPoint(x, z, Average, Average);
        }

        public VerticalSpace? GetHeightAtPointMinSpace(int x, int z)
        {
            return GetHeightAtPoint(x, z, Max, Min);
        }

        public VerticalSpace? GetHeightAtPointMaxSpace(int x, int z)
        {
            return GetHeightAtPoint(x, z, Min, Max);
        }

        public VerticalSpace? GetHeightInAreaAverage(Rectangle area)
        {
            return GetHeightInArea(area, Average, Average);
        }

        public VerticalSpace? GetHeightInAreaMinSpace(Rectangle area)
        {
            return GetHeightInArea(area, Max, Min);
        }

        public VerticalSpace? GetHeightInAreaMaxSpace(Rectangle area)
        {
            return GetHeightInArea(area, Min, Max);
        }

        public byte NumXSectors
        {
            get { return (byte)(Blocks.GetLength(0)); }
        }

        public byte NumZSectors
        {
            get { return (byte)(Blocks.GetLength(1)); }
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>Transforms the coordinates of QAFaces in such a way that the lowest one falls on Y = 0</summary>
        public void NormalizeRoomY()
        {
            // Determine lowest QAFace
            short lowest = short.MaxValue;
            for (int z = 0; z < NumZSectors; z++)
                for (int x = 0; x < NumXSectors; x++)
                {
                    var b = Blocks[x, z];
                    if (!b.IsAnyWall)
                        for (int i = 0; i < 4; i++)
                            lowest = Math.Min(lowest, b.QAFaces[i]);
                }

            // Move room to new position
            Position += new Vector3(0, lowest, 0);

            // Transform room content in such a way, their world position is identical to before even though the room position changed
            for (int z = 0; z < NumZSectors; z++)
                for (int x = 0; x < NumXSectors; x++)
                {
                    var b = Blocks[x, z];
                    for (int i = 0; i < 4; i++)
                    {
                        b.QAFaces[i] -= lowest;
                        b.EDFaces[i] -= lowest;
                        b.WSFaces[i] -= lowest;
                        b.RFFaces[i] -= lowest;
                    }
                }

            foreach (var instance in _objects)
                instance.Position -= new Vector3(0, lowest * 256, 0);
        }

        public bool AddObjectCutSectors(Level level, Rectangle newArea, SectorBasedObjectInstance instance)
        {
            // Determine area
            Rectangle instanceNewAreaConstraint = newArea.Inflate(-1);
            if (instance is PortalInstance)
                switch (((PortalInstance)instance).Direction) // Special constraints for portals on walls
                {
                    case PortalDirection.WallPositiveZ:
                        instanceNewAreaConstraint = newArea.Inflate(-1, 0);
                        break;
                    case PortalDirection.WallNegativeZ:
                        instanceNewAreaConstraint = newArea.Inflate(-1, 0);
                        break;
                    case PortalDirection.WallPositiveX:
                        instanceNewAreaConstraint = newArea.Inflate(0, -1);
                        break;
                    case PortalDirection.WallNegativeX:
                        instanceNewAreaConstraint = newArea.Inflate(0, -1);
                        break;
                }
            if (!instance.Area.Intersects(instanceNewAreaConstraint))
                return false;
            Rectangle instanceNewArea = instance.Area.Intersect(instanceNewAreaConstraint).OffsetNeg(new DrawingPoint(newArea.X, newArea.Y));

            // Add object
            AddObject(level, instance.Clone(instanceNewArea));
            return true;
        }

        public ObjectInstance AddObjectAndSingularPortal(Level level, ObjectInstance instance)
        {
            if (instance is PositionBasedObjectInstance)
                _objects.Add((PositionBasedObjectInstance)instance);
            try
            {
                instance.AddToRoom(level, this);
            }
            catch
            { // If we fail, remove the object from the list
                if (instance is PositionBasedObjectInstance)
                    _objects.Remove((PositionBasedObjectInstance)instance);
                throw;
            }
            return instance;
        }

        public void RemoveObjectAndSingularPortal(Level level, ObjectInstance instance)
        {
            instance.RemoveFromRoom(level, this);
            if (instance is PositionBasedObjectInstance)
                _objects.Remove((PositionBasedObjectInstance)instance);
        }

        public IEnumerable<ObjectInstance> AddObject(Level level, ObjectInstance instance)
        {
            // Add portals and opposite portals
            var portal = instance as PortalInstance;
            if (portal != null)
            {
                Rectangle oppositeArea = PortalInstance.GetOppositePortalArea(portal.Direction, portal.Area).Offset(SectorPos).OffsetNeg(portal.AdjoiningRoom.SectorPos);
                var oppositePortal = new PortalInstance(oppositeArea, PortalInstance.GetOppositeDirection(portal.Direction), this);

                // Add portals
                var addedObjects = new List<ObjectInstance>();
                try
                {
                    addedObjects.Add(AddObjectAndSingularPortal(level, portal));
                    if (AlternateVersion != null)
                        addedObjects.Add(AlternateVersion.AddObjectAndSingularPortal(level, portal.Clone()));

                    addedObjects.Add(portal.AdjoiningRoom.AddObjectAndSingularPortal(level, oppositePortal));
                    if (AlternateVersion != null)
                        addedObjects.Add(portal.AdjoiningRoom.AlternateVersion.AddObjectAndSingularPortal(level, oppositePortal.Clone()));
                }
                catch
                {
                    foreach (ObjectInstance instanceToRemove in addedObjects)
                        instanceToRemove.Room.RemoveObjectAndSingularPortal(level, instanceToRemove);
                    throw;
                }
                return addedObjects;
            }

            // Add normal object
            AddObjectAndSingularPortal(level, instance);
            return new ObjectInstance[] { instance };
        }

        public void RemoveObject(Level level, ObjectInstance instance)
        {
            RemoveObjectAndSingularPortal(level, instance);

            // Delete the corresponding other portals if necessary.
            var portal = instance as PortalInstance;
            if (portal != null)
            {
                var alternatePortal = portal.FindAlternatePortal(this?.AlternateVersion);
                if (alternatePortal != null)
                    AlternateVersion.RemoveObjectAndSingularPortal(level, alternatePortal);

                var oppositePortal = portal.FindOppositePortal(this);
                if (oppositePortal != null)
                    portal.AdjoiningRoom.RemoveObjectAndSingularPortal(level, oppositePortal);

                var oppositeAlternatePortal = oppositePortal?.FindAlternatePortal(portal.AdjoiningRoom?.AlternateVersion);
                if (oppositeAlternatePortal != null)
                    oppositeAlternatePortal.Room.RemoveObjectAndSingularPortal(level, oppositeAlternatePortal);
            }
        }

        public void MoveObjectFrom(Level level, Room from, PositionBasedObjectInstance instance)
        {
            from.RemoveObject(level, instance);
            instance.Position += from.WorldPos - WorldPos;
            AddObject(level, instance);
        }

        public enum RoomConnectionType
        {
            NoPortal,
            TriangularPortalXnZn,
            TriangularPortalXpZn,
            TriangularPortalXnZp,
            TriangularPortalXpZp,
            FullPortal
        };

        public static RoomConnectionType Combine(RoomConnectionType first, RoomConnectionType second)
        {
            if (first == second || first == RoomConnectionType.FullPortal)
                return second;
            else
                return RoomConnectionType.NoPortal;
        }

        public struct RoomConnectionInfo
        {
            public PortalInstance Portal { get; }
            public RoomConnectionType AnyType { get; }

            public RoomConnectionInfo(PortalInstance portal, RoomConnectionType anyType)
            {
                Portal = portal;
                AnyType = anyType;
            }

            ///<summary>Gives how the block visually appears regarding portals</summary>
            public RoomConnectionType VisualType => (Portal?.HasTexturedFaces ?? true) ? RoomConnectionType.NoPortal : AnyType;
            ///<summary>Gives how the block geometrically behaves regarding portals</summary>
            public RoomConnectionType TraversableType => (Portal?.IsTraversable ?? false) ? AnyType : RoomConnectionType.NoPortal;
        };

        public static IEnumerable<KeyValuePair<Room, Room>> GetPossibleAlternateRoomPairs(Room firstRoom, Room secondRoom, bool lookingFromSecond = false)
        {
            if (firstRoom.AlternateVersion == null)
            {
                if (secondRoom.AlternateVersion == null)
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                else
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom.AlternateVersion);
                }
            }
            else
            {
                if (secondRoom.AlternateVersion == null)
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateVersion, secondRoom);
                }
                else if (firstRoom.AlternateGroup == secondRoom.AlternateGroup)
                {
                    bool isAlternateCurrently = lookingFromSecond ? (secondRoom.AlternateBaseRoom != null) : (firstRoom.AlternateBaseRoom != null);
                    if (isAlternateCurrently)
                        yield return new KeyValuePair<Room, Room>(firstRoom.AlternateRoom ?? firstRoom, secondRoom.AlternateRoom ?? secondRoom);
                    else
                        yield return new KeyValuePair<Room, Room>(firstRoom.AlternateBaseRoom ?? firstRoom, secondRoom.AlternateBaseRoom ?? secondRoom);
                }
                else
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateVersion, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom.AlternateVersion);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateVersion, secondRoom.AlternateVersion);
                }
            }
        }

        public static RoomConnectionType CalculateRoomConnectionTypeWithoutAlternates(Room roomBelow, Room roomAbove, Block blockBelow, Block blockAbove)
        {
            // Evaluate force floor solid
            if (blockAbove.ForceFloorSolid)
                return RoomConnectionType.NoPortal;

            // Check walls
            DiagonalSplit aboveDiagonalSplit = blockAbove.FloorDiagonalSplit;
            DiagonalSplit belowDiagonalSplit = blockBelow.CeilingDiagonalSplit;
            if (blockAbove.IsAnyWall && (aboveDiagonalSplit == DiagonalSplit.None))
                return RoomConnectionType.NoPortal;
            if (blockBelow.IsAnyWall && (belowDiagonalSplit == DiagonalSplit.None))
                return RoomConnectionType.NoPortal;
            if (blockBelow.IsAnyWall && blockAbove.IsAnyWall && (aboveDiagonalSplit != belowDiagonalSplit))
                return RoomConnectionType.NoPortal;

            // Gather split data
            bool belowSplitXEqualsZ = blockBelow.CeilingSplitDirectionIsXEqualsZReal;
            bool aboveSplitXEqualsZ = blockAbove.FloorSplitDirectionIsXEqualsZReal;
            bool belowIsQuad = blockBelow.CeilingIsQuad;
            bool aboveIsQuad = blockAbove.FloorIsQuad;

            // Check where the geometry matches to create a portal
            if (belowIsQuad || aboveIsQuad || (belowSplitXEqualsZ == aboveSplitXEqualsZ))
            {
                bool matchesAtXnYn = (roomBelow.Position.Y + blockBelow.WSFaces[Block.FaceXnZn]) == (roomAbove.Position.Y + blockAbove.QAFaces[Block.FaceXnZn]);
                bool matchesAtXpYn = (roomBelow.Position.Y + blockBelow.WSFaces[Block.FaceXpZn]) == (roomAbove.Position.Y + blockAbove.QAFaces[Block.FaceXpZn]);
                bool matchesAtXnYp = (roomBelow.Position.Y + blockBelow.WSFaces[Block.FaceXnZp]) == (roomAbove.Position.Y + blockAbove.QAFaces[Block.FaceXnZp]);
                bool matchesAtXpYp = (roomBelow.Position.Y + blockBelow.WSFaces[Block.FaceXpZp]) == (roomAbove.Position.Y + blockAbove.QAFaces[Block.FaceXpZp]);

                if (matchesAtXnYn && matchesAtXpYn && matchesAtXnYp && matchesAtXpYp && !(blockAbove.IsAnyWall || blockBelow.IsAnyWall))
                    return RoomConnectionType.FullPortal;
                if ((belowIsQuad || belowSplitXEqualsZ) && (aboveIsQuad || aboveSplitXEqualsZ))
                {
                    if (matchesAtXnYn && matchesAtXnYp && matchesAtXpYp && (aboveDiagonalSplit == DiagonalSplit.XpZn || !(blockAbove.IsAnyWall || blockBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXnZp;
                    if (matchesAtXnYn && matchesAtXpYn && matchesAtXpYp && (aboveDiagonalSplit == DiagonalSplit.XnZp || !(blockAbove.IsAnyWall || blockBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXpZn;
                }
                if ((belowIsQuad || !belowSplitXEqualsZ) && (aboveIsQuad || !aboveSplitXEqualsZ))
                {
                    if (matchesAtXpYn && matchesAtXnYp && matchesAtXpYp && (aboveDiagonalSplit == DiagonalSplit.XnZn || !(blockAbove.IsAnyWall || blockBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXpZp;
                    if (matchesAtXnYn && matchesAtXpYn && matchesAtXnYp && (aboveDiagonalSplit == DiagonalSplit.XpZp || !(blockAbove.IsAnyWall || blockBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXnZn;
                }
            }
            return RoomConnectionType.NoPortal;
        }

        public static RoomConnectionType CalculateRoomConnectionType(Room roomBelow, Room roomAbove, DrawingPoint posBelow, DrawingPoint posAbove, bool lookingFromAbove)
        {
            // Checkout all possible alternate room combinations and combine the results
            RoomConnectionType result = RoomConnectionType.FullPortal;
            foreach (var roomPair in GetPossibleAlternateRoomPairs(roomBelow, roomAbove, lookingFromAbove))
                result = Combine(result,
                    CalculateRoomConnectionTypeWithoutAlternates(roomPair.Key, roomPair.Value,
                        roomPair.Key.GetBlock(posBelow), roomPair.Value.GetBlock(posAbove)));
            return result;
        }

        public RoomConnectionInfo GetFloorRoomConnectionInfo(DrawingPoint pos)
        {
            Block block = GetBlock(pos);
            if (block.FloorPortal != null)
            {
                Room adjoiningRoom = block.FloorPortal.AdjoiningRoom;
                DrawingPoint adjoiningPos = pos.Offset(SectorPos).OffsetNeg(adjoiningRoom.SectorPos);
                Block adjoiningBlock = adjoiningRoom.GetBlock(adjoiningPos);
                if (adjoiningBlock.CeilingPortal != null)
                    return new RoomConnectionInfo(block.FloorPortal, CalculateRoomConnectionType(adjoiningRoom, this, adjoiningPos, pos, true));
            }
            return new RoomConnectionInfo();
        }

        public RoomConnectionInfo GetCeilingRoomConnectionInfo(DrawingPoint pos)
        {
            Block block = GetBlock(pos);
            if (block.CeilingPortal != null)
            {
                Room adjoiningRoom = block.CeilingPortal.AdjoiningRoom;
                DrawingPoint adjoiningPos = pos.Offset(SectorPos).OffsetNeg(adjoiningRoom.SectorPos);
                Block adjoiningBlock = adjoiningRoom.GetBlock(adjoiningPos);
                if (adjoiningBlock.FloorPortal != null)
                    return new RoomConnectionInfo(block.CeilingPortal, CalculateRoomConnectionType(this, adjoiningRoom, pos, adjoiningPos, false));
            }
            return new RoomConnectionInfo();
        }

        public void SmartBuildGeometry(Rectangle area)
        {
            area = area.Inflate(1); // Add margin

            // Update adjoining rooms
            var roomsToProcess = new List<Room> { this };
            var areaToProcess = new List<Rectangle> { area };
            List<PortalInstance> listOfPortals = Portals.ToList();
            foreach (var portal in listOfPortals)
            {
                if (!portal.Area.Intersects(area))
                    continue; // This portal is irrelavant since no changes happend in its area

                Rectangle portalArea = portal.Area.Intersect(area);
                Rectangle otherRoomPortalArea = PortalInstance.GetOppositePortalArea(portal.Direction, portalArea)
                    .Offset(SectorPos).OffsetNeg(portal.AdjoiningRoom.SectorPos);

                // Find all related rooms or alternate rooms around the portals
                foreach (var roomPairs in GetPossibleAlternateRoomPairs(this, portal.AdjoiningRoom))
                {
                    // Add itself if necessary
                    int thisRoomIndex = roomsToProcess.IndexOf(roomPairs.Key);
                    if (thisRoomIndex == -1)
                    {
                        roomsToProcess.Add(roomPairs.Key);
                        areaToProcess.Add(area);
                    }
                    else
                        areaToProcess[thisRoomIndex] = areaToProcess[thisRoomIndex].Union(area);

                    // Add other if necessary
                    int adjoiningRoomIndex = roomsToProcess.IndexOf(roomPairs.Value);
                    if (adjoiningRoomIndex == -1)
                    {
                        roomsToProcess.Add(roomPairs.Value);
                        areaToProcess.Add(otherRoomPortalArea);
                    }
                    else
                        areaToProcess[adjoiningRoomIndex] = areaToProcess[adjoiningRoomIndex].Union(otherRoomPortalArea);
                }
            }
            System.Diagnostics.Debug.Assert(roomsToProcess.Count == areaToProcess.Count);

            // Update the collected stuff now
            Parallel.For(0, roomsToProcess.Count, (index) =>
                {
                    roomsToProcess[index].BuildGeometry(areaToProcess[index]);
                    roomsToProcess[index].CalculateLightingForThisRoom();
                });
            foreach (var room in roomsToProcess)
                room.UpdateBuffers();
        }
    }

    public struct VerticalSpace
    {
        public float FloorY;
        public float CeilingY;

        public static VerticalSpace operator +(VerticalSpace old, float offset)
        {
            return new VerticalSpace { FloorY = old.FloorY + offset, CeilingY = old.CeilingY + offset };
        }
    };
}