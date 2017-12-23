using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using TombLib.LevelData.Compilers;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using TombLib.Utils;
using System.Threading.Tasks;
using TombLib.Graphics;

namespace TombLib.LevelData
{
    public enum Reverberation : byte
    {
        Outside, SmallRoom, MediumRoom, LargeRoom, Pipe
    }

    public enum BlockFaceShape : byte
    {
        RectangleInt2, Triangle
    }

    public enum BlockFaceType
    {
        Floor, Ceiling, Wall
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
        public bool Locked { get; set; }

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
        private List<int>[,,] _sectorFaceIndices;

        public Room(int numXSectors, int numZSectors, string name = "Unnamed", short ceiling = DefaultHeight)
        {
            Name = name;
            Resize(null, new RectangleInt2(0, 0, numXSectors - 1, numZSectors - 1), 0, ceiling);
        }

        public Room(VectorInt2 sectorSize, string name = "Unnamed", short ceiling = DefaultHeight)
            : this(sectorSize.X, sectorSize.Y, name, ceiling)
        {}

        public void Resize(Level level, RectangleInt2 area, short floor = 0, short ceiling = DefaultHeight)
        {
            int numXSectors = area.Width + 1;
            int numZSectors = area.Height + 1;
            VectorInt2 offset = area.Start;

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
                    Block oldBlock = GetBlockTry(new VectorInt2(x, z) + (offset));
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
            _sectorFaceIndices = new List<int>[numXSectors, numZSectors, (int)Block.FaceCount];

            Blocks = newBlocks;

            // Move objects
            SectorPos = SectorPos + offset;
            foreach (var instance in _objects)
                instance.Position -= new Vector3(offset.X * 1024, 0, offset.Y * 1024);

            // Add sector based objects again
            RectangleInt2 newArea = new RectangleInt2(offset.X, offset.Y, offset.X + numXSectors - 1, offset.Y + numZSectors - 1);
            foreach (var instance in sectorObjects)
                AddObjectCutSectors(level, newArea, instance);

            // Update state
            UpdateCompletely();
        }

        public Room Split(Level level, RectangleInt2 area)
        {
            var newRoom = Clone(level, (instance) => !(instance is PositionBasedObjectInstance) && !(instance is PortalInstance));
            newRoom.Name = "Split from " + Name;
            newRoom.Resize(level, area);
            List<PortalInstance> portals = Portals.ToList();

            // Detect if the room was split by a straight line
            // If this is the case, resize the original room
            if ((area.X0 == 0) && (area.Y0 == 0) && (area.X1 == (NumXSectors - 1)) && (area.Y1 < (NumZSectors - 1)))
            {
                Resize(level, new RectangleInt2(area.X0, area.Y1 - 1, area.X1, NumZSectors - 1));
                AddObject(level, new PortalInstance(new RectangleInt2(area.X0 + 1, 0, area.X1 - 1, 0), PortalDirection.WallNegativeZ, newRoom));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z < 1024)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if ((area.X0 == 0) && (area.Y0 == 0) && (area.X1 < (NumXSectors - 1)) && (area.Y1 == (NumZSectors - 1)))
            {
                Resize(level, new RectangleInt2(area.X1 - 1, area.Y0, NumXSectors - 1, area.Y1));
                AddObject(level, new PortalInstance(new RectangleInt2(0, area.Y0 + 1, 0, area.Y1 - 1), PortalDirection.WallNegativeX, newRoom));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.X < 1024)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if ((area.X0 == 0) && (area.Y0 > 0) && (area.X1 == (NumXSectors - 1)) && (area.Y1 == (NumZSectors - 1)))
            {
                Resize(level, new RectangleInt2(area.X0, 0, area.X1, area.Y0 + 1));
                AddObject(level, new PortalInstance(new RectangleInt2(area.X0 + 1, NumZSectors - 1, area.X1 - 1, NumZSectors - 1), PortalDirection.WallPositiveZ, newRoom));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z > ((NumZSectors - 2) * 1024))
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if ((area.X0 > 0) && (area.Y0 == 0) && (area.X1 == (NumXSectors - 1)) && (area.Y1 == (NumZSectors - 1)))
            {
                Resize(level, new RectangleInt2(0, area.Y0, area.X0 + 1, area.Y1));
                AddObject(level, new PortalInstance(new RectangleInt2(NumXSectors - 1, area.Y0 + 1, NumXSectors - 1, area.Y1 - 1), PortalDirection.WallPositiveX, newRoom));

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
                for (int z = area.Y0 + 1; z < area.Y1; ++z)
                    for (int x = area.X0 + 1; x < area.X1; ++x)
                        Blocks[x, z].Type = BlockType.Wall;

                // Move objects
                Vector2 start = new Vector2(area.X0, area.Y0) * 1024.0f;
                Vector2 end = new Vector2(area.X0 + 1, area.Y1 + 1) * 1024.0f;
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
            for (int x = 0; x < NumXSectors; x++)
                for (int z = 0; z < NumZSectors; z++)
                    for (int f = 0; f < 29; f++)
                        result._sectorFaceIndices[x, z, 4] = new List<int>();
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
            return result;
        }

        public Room Clone(Level level)
        {
            return Clone(level, instance => !(instance is PortalInstance));
        }

        public bool Flipped => (AlternateRoom != null) || (AlternateBaseRoom != null);
        public Room AlternateVersion => AlternateRoom ?? AlternateBaseRoom;
        public VectorInt2 SectorSize => new VectorInt2(NumXSectors, NumZSectors);
        public RectangleInt2 WorldArea => new RectangleInt2((int)Position.X, (int)Position.Z, (int)Position.X + NumXSectors - 1, (int)Position.Z + NumZSectors - 1);
        public RectangleInt2 LocalArea => new RectangleInt2(0, 0, NumXSectors - 1, NumZSectors - 1);
        public IEnumerable<Room> Versions
        {
            get
            {
                yield return this;
                Room alternateVersion = AlternateVersion;
                if (alternateVersion != null)
                    yield return alternateVersion;
            }
        }

        public VectorInt2 SectorPos
        {
            get { return new VectorInt2((int)Position.X, (int)Position.Z); }
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

        public Block GetBlock(VectorInt2 pos)
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

        public Block GetBlockTry(VectorInt2 pos)
        {
            return GetBlockTry(pos.X, pos.Y);
        }

        public struct RoomBlockPair
        {
            public Room Room { get; set; }
            public Block Block { get; set; }
            public VectorInt2 Pos { get; set; }
        }

        public RoomBlockPair ProbeLowestBlock(VectorInt2 pos, bool doProbe = true)
        {
            Block block = GetBlockTry(pos);
            if (block == null)
                return new RoomBlockPair();
            else if (!doProbe || block.WallPortal != null)
                return new RoomBlockPair { Room = this, Block = block, Pos = pos };

            RoomBlockPair result = GetBlockTryThroughPortal(pos);
            if (result.Block?.FloorPortal == null)
                return result;

            Room adjoiningRoom = result.Block.FloorPortal.AdjoiningRoom;
            VectorInt2 adjoiningSectorCoordinate = pos + (SectorPos - adjoiningRoom.SectorPos);
            return adjoiningRoom.ProbeLowestBlock(adjoiningSectorCoordinate);
        }
        public RoomBlockPair ProbeLowestBlock(int x, int z, bool doProbe = true) => ProbeLowestBlock(new VectorInt2(x, z), doProbe);

        public RoomBlockPair GetBlockTryThroughPortal(VectorInt2 pos)
        {
            Block sector = GetBlockTry(pos);
            if (sector == null)
                return new RoomBlockPair();

            if (sector?.WallPortal == null)
                return new RoomBlockPair { Room = this, Block = sector, Pos = pos };

            Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;
            VectorInt2 adjoiningSectorCoordinate = pos + (SectorPos - adjoiningRoom.SectorPos);
            Block sector2 = adjoiningRoom.GetBlockTry(adjoiningSectorCoordinate);
            return new RoomBlockPair { Room = adjoiningRoom, Block = sector2, Pos = adjoiningSectorCoordinate };
        }
        public RoomBlockPair GetBlockTryThroughPortal(int x, int z) => GetBlockTryThroughPortal(new VectorInt2(x, z));

        public void ModifyPoint(int x, int z, int verticalSubdivision, short increment, RectangleInt2 area)
        {
            bool floor = (verticalSubdivision % 2 == 0);

            if (x <= 0 || z <= 0 || x >= NumXSectors || z >= NumZSectors)
                return;
            {
                if (area.Contains(new VectorInt2(x, z)))
                {
                    if ((floor && Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.None) || (!floor && Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.None))
                    {
                        Blocks[x, z].GetVerticalSubdivision(verticalSubdivision)[3] += increment;
                        Blocks[x, z].FixHeights(verticalSubdivision);
                    }
                }
                if (area.Contains(new VectorInt2(x - 1, z)))
                {
                    var adjacentLeftBlock = GetBlockTry(x - 1, z);
                    if (adjacentLeftBlock != null && ((floor && adjacentLeftBlock.FloorDiagonalSplit == DiagonalSplit.None) || (!floor && adjacentLeftBlock.CeilingDiagonalSplit == DiagonalSplit.None)))
                    {
                        adjacentLeftBlock.GetVerticalSubdivision(verticalSubdivision)[2] += increment;
                        adjacentLeftBlock.FixHeights(verticalSubdivision);
                    }
                }
                if (area.Contains(new VectorInt2(x, z - 1)))
                {
                    var adjacentBottomBlock = GetBlockTry(x, z - 1);
                    if (adjacentBottomBlock != null && ((floor && adjacentBottomBlock.FloorDiagonalSplit == DiagonalSplit.None) || (!floor && adjacentBottomBlock.CeilingDiagonalSplit == DiagonalSplit.None)))
                    {
                        adjacentBottomBlock.GetVerticalSubdivision(verticalSubdivision)[0] += increment;
                        adjacentBottomBlock.FixHeights(verticalSubdivision);
                    }
                }
                if (area.Contains(new VectorInt2(x - 1, z - 1)))
                {
                    var adjacentBottomLeftBlock = GetBlockTry(x - 1, z - 1);
                    if (adjacentBottomLeftBlock != null && ((floor && adjacentBottomLeftBlock.FloorDiagonalSplit == DiagonalSplit.None) || (!floor && adjacentBottomLeftBlock.CeilingDiagonalSplit == DiagonalSplit.None)))
                    {
                        adjacentBottomLeftBlock.GetVerticalSubdivision(verticalSubdivision)[1] += increment;
                        adjacentBottomLeftBlock.FixHeights(verticalSubdivision);
                    }
                }
            }
        }

        public short GetMinHeight(RectangleInt2 area, int verticalSubdivision)
        {
            short minimum = short.MaxValue;

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    minimum = Math.Min(minimum, Blocks[x, z].GetFaceMin(verticalSubdivision));

            return minimum;
        }

        public short GetMaxHeight(RectangleInt2 area, int verticalSubdivision)
        {
            short maximum = short.MinValue;

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    maximum = Math.Max(maximum, Blocks[x, z].GetFaceMin(verticalSubdivision));

            return maximum;
        }

        public bool IsIllegalSlope(int x, int z)
        {
            Block sector = GetBlockTry(x, z);

            if (WaterLevel > 0 || sector == null || sector.IsAnyWall || !sector.FloorHasSlope)
                return false;

            const int lowestPassableHeight = 4;
            const int lowestPassableStep = 2;  // Lara still can bug out of 2-click step heights

            var normals = sector.GetFloorTriangleNormals();
            var slopeDirections = sector.GetFloorTriangleSlopeDirections();

            if (slopeDirections[0] != Direction.None && slopeDirections[1] != Direction.None &&
                slopeDirections[0] != slopeDirections[1])
            {
                var diff = normals[0] - normals[1];
                if (Math.Atan2(diff.X, diff.Z) < 0)
                    return true;
            }

            // Main illegal slope calculation takes two adjacent corner heights (heightsToCheck[0-1]) from lookup block
            // and looks if they are higher than any slope's lower heights (heightsToCompare[0-1]). Also it checks if
            // lookup block's adjacent ceiling heights (heightsToCheck[2-3]) is lower than minimum passable height.
            // If lookup block contains floor or ceiling diagonal splits, resolve algorighm is diverted (look further).

            bool slopeIsIllegal = false;

            for (int i = 0; i < 2; i++)
            {
                if (slopeIsIllegal)
                    continue;

                RoomBlockPair lookupBlock;
                short[] heightsToCompare;
                short[] heightsToCheck;

                switch (slopeDirections[i])
                {
                    case Direction.PositiveZ:
                        lookupBlock = GetBlockTryThroughPortal(x, z + 1);
                        heightsToCompare = new short[2] { 0, 1 };
                        heightsToCheck = new short[2] { 3, 2 };
                        break;

                    case Direction.NegativeZ:
                        lookupBlock = GetBlockTryThroughPortal(x, z - 1);
                        heightsToCompare = new short[2] { 2, 3 };
                        heightsToCheck = new short[2] { 1, 0 };
                        break;

                    // We only need to override east and west diagonal split HeightsToCompare[1] cases, because
                    // slanted diagonal splits are always 45 degrees, hence these are only two slide directions possible.

                    case Direction.PositiveX:
                        lookupBlock = GetBlockTryThroughPortal(x + 1, z);
                        heightsToCompare = new short[2] { 1, (short)(sector.FloorDiagonalSplit == DiagonalSplit.XnZp ? 1 : 2) };
                        heightsToCheck = new short[2] { 0, 3 };
                        break;

                    case Direction.NegativeX:
                        lookupBlock = GetBlockTryThroughPortal(x - 1, z);
                        heightsToCompare = new short[2] { 3, (short)(sector.FloorDiagonalSplit == DiagonalSplit.XpZn ? 3 : 0) };
                        heightsToCheck = new short[2] { 2, 1 };
                        break;
                    default:
                        continue;
                }

                // Diagonal split resolver

                if (lookupBlock.Block.IsAnyWall && lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.None)
                {
                    // If lookup block contains non-diagonal wall, we mark slope as illegal in any case.

                    slopeIsIllegal = true;
                    continue;
                }
                else if (lookupBlock.Block.FloorDiagonalSplit != DiagonalSplit.None)
                {
                    // If lookup block has diagonal splits...
                    // For floor diagonal steps and diagonal walls, only steps/walls that are facing away
                    // from split are always treated as potentially illegal, because steps facing towards
                    // split are resolved by engine position corrector nicely.

                    // Since in Tomb Editor diagonal steps are made that way so only single corner height
                    // is used for setting step height, we copy one of lookup block's heightsToCheck to
                    // another.

                    switch (slopeDirections[i])
                    {
                        case Direction.PositiveZ:
                            if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (lookupBlock.Block.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XnZp)
                                heightsToCheck[0] = 2;
                            else
                                heightsToCheck[1] = 3;
                            break;
                        case Direction.PositiveX:
                            if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (lookupBlock.Block.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XpZp)
                                heightsToCheck[0] = 3;
                            else
                                heightsToCheck[1] = 0;
                            break;
                        case Direction.NegativeZ:
                            if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XpZp ||
                                lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (lookupBlock.Block.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XpZn)
                                heightsToCheck[0] = 0;
                            else
                                heightsToCheck[1] = 1;
                            break;
                        case Direction.NegativeX:
                            if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XpZp ||
                                lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (lookupBlock.Block.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XnZn)
                                heightsToCheck[0] = 1;
                            else
                                heightsToCheck[1] = 2;
                            break;
                    }
                }

                int heightAdjust = (int)Math.Round(Position.Y - lookupBlock.Room.Position.Y);
                int absoluteLowestPassableHeight = lowestPassableHeight + heightAdjust;
                int absoluteLowestPassableStep = lowestPassableStep + heightAdjust;

                // Main comparer

                if (lookupBlock.Block.FloorPortal == null)
                {
                    if (lookupBlock.Block.QA[heightsToCheck[0]] - sector.QA[heightsToCompare[0]] > absoluteLowestPassableStep ||
                        lookupBlock.Block.QA[heightsToCheck[1]] - sector.QA[heightsToCompare[1]] > absoluteLowestPassableStep)
                        slopeIsIllegal = true;
                    else
                    {
                        var lookupBlockSlopeDirections = lookupBlock.Block.GetFloorTriangleSlopeDirections();

                        for (int j = 0; j < 2; j++)
                        {
                            // If both current and lookup sector triangle split direction is the same, ignore far opposite triangles.
                            // FIXME: works only for normal sectors. Diagonal steps are broken!

                            if (!sector.FloorIsQuad && !lookupBlock.Block.FloorIsQuad)
                            {
                                var sectorSplitDirection = (sector.FloorDiagonalSplit == DiagonalSplit.None ? sector.FloorSplitDirectionIsXEqualsZ : ((int)sector.FloorDiagonalSplit % 2 != 0));
                                var lookupSplitDirection = (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.None ? lookupBlock.Block.FloorSplitDirectionIsXEqualsZ : ((int)lookupBlock.Block.FloorDiagonalSplit % 2 != 0));

                                if (sectorSplitDirection == lookupSplitDirection)
                                {
                                    if (sector.FloorDiagonalSplit != DiagonalSplit.None && lookupBlock.Block.FloorDiagonalSplit != DiagonalSplit.None)
                                        continue;

                                    if (sectorSplitDirection)
                                    {
                                        if (((slopeDirections[i] == Direction.PositiveZ || slopeDirections[i] == Direction.NegativeX) && i == 0 && j == 1) ||
                                            ((slopeDirections[i] == Direction.PositiveX || slopeDirections[i] == Direction.NegativeZ) && i == 1 && j == 0))
                                            continue;
                                    }
                                    else
                                    {
                                        if ((slopeDirections[i] < Direction.NegativeZ && i == 1 && j == 0) ||
                                            (slopeDirections[i] >= Direction.NegativeZ && i == 0 && j == 1))
                                            continue;
                                    }
                                }
                            }

                            // This code is needed to get REAL triangle index for diagonal step cases, because for some reason, triangle indices are inverted in this case.

                            var realTriangleIndex = j;

                            if (lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XpZn || lookupBlock.Block.FloorDiagonalSplit == DiagonalSplit.XnZp)
                                realTriangleIndex = (realTriangleIndex == 0 ? 1 : 0);

                            // Triangle is considered illegal only if its lowest point lies lower than lowest passable step height compared to opposite triangle minimum point.
                            // Triangle is NOT considered illegal, if its slide direction is perpendicular to opposite triangle slide direction.

                            var heightDifference = sector.GetTriangleMinimumFloorPoint(i) - (lookupBlock.Block.GetTriangleMinimumFloorPoint(realTriangleIndex) - heightAdjust);

                            if ((heightsToCheck[0] != heightsToCheck[1] && heightDifference <= lowestPassableStep) || // Ordinary cases
                                (heightsToCheck[0] == heightsToCheck[1] && heightDifference <= 0 && heightDifference > -lowestPassableStep))  // Diagonal step cases
                            {

                                if (lookupBlockSlopeDirections[j] != Direction.None && lookupBlockSlopeDirections[j] != slopeDirections[i])
                                    if (((int)lookupBlockSlopeDirections[j] % 2) == ((int)slopeDirections[i] % 2))
                                        slopeIsIllegal = true;
                            }
                        }
                    }
                }

                if(lookupBlock.Block.CeilingPortal == null)
                {
                    if (lookupBlock.Block.WS[heightsToCheck[0]] - sector.QA[heightsToCompare[0]] < absoluteLowestPassableHeight ||
                        lookupBlock.Block.WS[heightsToCheck[1]] - sector.QA[heightsToCompare[1]] < absoluteLowestPassableHeight ||
                        lookupBlock.Block.WS[heightsToCheck[0]] - lookupBlock.Block.QA[heightsToCheck[0]] < lowestPassableHeight ||
                        lookupBlock.Block.WS[heightsToCheck[1]] - lookupBlock.Block.QA[heightsToCheck[1]] < lowestPassableHeight)
                        slopeIsIllegal = true;
                }
            }

            return slopeIsIllegal;
        }

        public bool IsFaceDefined(int x, int z, BlockFace face)
        {
            return _sectorFaceVertexVertexRange[x, z, (int)face].Count != 0;
        }

        public float GetFaceHeight(int x, int z, BlockFace face)
        {
            if(IsFaceDefined(x, z, face))
            {
                float min = float.MaxValue;
                float max = float.MinValue;

                var indices = GetFaceIndices(x, z, face);
                foreach(var index in indices)
                {
                    min = Math.Min(_allVertices[index].Position.Y, min);
                    max = Math.Max(_allVertices[index].Position.Y, max);
                }
                return (max - min);
            }
            else
            {
                return 0;
            }
        }

        public float GetFaceHighestPoint(int x, int z, BlockFace face)
        {
            float max = float.MinValue;

            if (IsFaceDefined(x, z, face))
            {
                var indices = GetFaceIndices(x, z, face);
                foreach (var index in indices)
                    max = Math.Max(_allVertices[index].Position.Y, max);
            }

            return max;
        }

        public float GetFaceLowestPoint(int x, int z, BlockFace face)
        {
            float min = float.MaxValue;

            if (IsFaceDefined(x, z, face))
            {
                var indices = GetFaceIndices(x, z, face);
                foreach (var index in indices)
                    min = Math.Min(_allVertices[index].Position.Y, min);
            }

            return min;
        }

        public VertexRange GetFaceVertexRange(int x, int z, BlockFace face)
        {
            VertexRange range = _sectorFaceVertexVertexRange[x, z, (int)face];
            int offset = _sectorAllVerticesOffset[x, z];
            return new VertexRange { Start = range.Start + offset, Count = range.Count };
        }

        public Block.FaceShape GetFaceShape(int x, int z, BlockFace face)
        {
            switch (GetFaceVertexRange(x, z, face).Count)
            {
                case 3:
                    return Block.FaceShape.Triangle;
                case 6:
                    return Block.FaceShape.Quad;
                default:
                    return Block.FaceShape.Unknown;
            }
        }

        public List<int> GetFaceIndices(int x, int z, BlockFace face)
        {
            var range = _sectorFaceIndices[x, z, (int)face];
            int offset = _sectorAllVerticesOffset[x, z];
            var indices = new List<int>();
            foreach (var index in range)
                indices.Add(index + offset);

            if ((face == BlockFace.Ceiling || face == BlockFace.CeilingTriangle2) && indices.Count == 4)
            {

                /*var temp = indices[0];
                indices[0] = indices[1];
                indices[1] = temp;

                temp = indices[3];
                indices[3] = indices[2];
                indices[2] = temp;*/
            }

            return indices;
        }

        public void BuildGeometry()
        {
            BuildGeometry(new RectangleInt2(0, 0, NumXSectors - 1, NumZSectors - 1));
        }

        public void BuildGeometry(RectangleInt2 area)
        {
            // Adjust ranges
            int xMin = Math.Max(0, area.X0);
            int xMax = Math.Min(NumXSectors - 1, area.X1);
            int zMin = Math.Max(0, area.Y0);
            int zMax = Math.Min(NumZSectors - 1, area.Y1);

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
                        _sectorFaceIndices[x, z, (int)f] = new List<int>();
                    }

                    // Save the height of the faces
                    int qa0 = Blocks[x, z].QA[0];
                    int qa1 = Blocks[x, z].QA[1];
                    int qa2 = Blocks[x, z].QA[2];
                    int qa3 = Blocks[x, z].QA[3];
                    int ws0 = Blocks[x, z].WS[0];
                    int ws1 = Blocks[x, z].WS[1];
                    int ws2 = Blocks[x, z].WS[2];
                    int ws3 = Blocks[x, z].WS[3];

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
                            var block = adjoiningRoom.GetBlockTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.FloorDiagonalSplit == DiagonalSplit.None ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XnZp ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XpZp))
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
                            var block = adjoiningRoom.GetBlockTry(facingX, 1) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.FloorDiagonalSplit == DiagonalSplit.None ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XpZn))
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
                            var block = adjoiningRoom.GetBlockTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.FloorDiagonalSplit == DiagonalSplit.None ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XpZn ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XpZp))
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
                            var block = adjoiningRoom.GetBlockTry(1, facingZ) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.FloorDiagonalSplit == DiagonalSplit.None ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XnZp))
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
                    RoomConnectionInfo floorPortalInfo = GetFloorRoomConnectionInfo(new VectorInt2(x, z));
                    BuildFloorOrCeilingFace(x, z, qa0, qa1, qa2, qa3, Blocks[x, z].FloorDiagonalSplit, Blocks[x, z].FloorSplitDirectionIsXEqualsZ,
                        BlockFace.Floor, BlockFace.FloorTriangle2, floorPortalInfo.VisualType, false);

                    // Ceiling polygons
                    var sectorVertices = _sectorVertices[x, z];
                    int startCeilingPolygons = sectorVertices.Count;

                    RoomConnectionInfo ceilingPortalInfo = GetCeilingRoomConnectionInfo(new VectorInt2(x, z));
                    BuildFloorOrCeilingFace(x, z, ws0, ws1, ws2, ws3, Blocks[x, z].CeilingDiagonalSplit, Blocks[x, z].CeilingSplitDirectionIsXEqualsZ,
                        BlockFace.Ceiling, BlockFace.CeilingTriangle2, ceilingPortalInfo.VisualType, true);

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

        private void BuildFloorOrCeilingFace(int x, int z, int h0, int h1, int h2, int h3, DiagonalSplit splitType, bool diagonalSplitXEqualsY,
                                             BlockFace face1, BlockFace face2, RoomConnectionType portalMode,
                                             bool isForCeiling)
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
                AddQuad(x, z, face1,
                    new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                    new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                    new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                    new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                    Blocks[x, z].GetFaceTexture(face1), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
                    isForCeiling);
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
            int qA, qB, eA, eB, rA, rB, wA, wB, fA, fB, cA, cB;

            Block b = Blocks[x, z];
            Block ob;

            BlockFace qaFace, edFace, wsFace, rfFace, middleFace;

            TextureArea face;

            switch (direction)
            {
                case FaceDirection.PositiveZ:
                    xA = x + 1;
                    xB = x;
                    zA = z + 1;
                    zB = z + 1;
                    ob = Blocks[x, z + 1];
                    qA = b.QA[1];
                    qB = b.QA[0];
                    eA = b.ED[1];
                    eB = b.ED[0];
                    rA = b.RF[1];
                    rB = b.RF[0];
                    wA = b.WS[1];
                    wB = b.WS[0];
                    fA = ob.QA[2];
                    fB = ob.QA[3];
                    cA = ob.WS[2];
                    cB = ob.WS[3];
                    qaFace = BlockFace.PositiveZ_QA;
                    edFace = BlockFace.PositiveZ_ED;
                    middleFace = BlockFace.PositiveZ_Middle;
                    rfFace = BlockFace.PositiveZ_RF;
                    wsFace = BlockFace.PositiveZ_WS;

                    if (b.WallPortal != null)
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

                        var qaNearA = nearBlock.QA[2];
                        var qaNearB = nearBlock.QA[3];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZp) qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZp) qaNearB = qaNearA;

                        var wsNearA = nearBlock.WS[2];
                        var wsNearB = nearBlock.WS[3];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Block.Empty;

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QA[1];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QA[0];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZn) qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZn) qBportal = qAportal;

                        qA = (int)Position.Y + qaNearA;
                        qB = (int)Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WS[1];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WS[0];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wBportal = wAportal;

                        wA = (int)Position.Y + wsNearA;
                        wB = (int)Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;

                        eA = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.ED[1];
                        eB = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.ED[0];
                        rA = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.RF[1];
                        rB = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.RF[0];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        qA = b.QA[0];
                        qB = b.QA[0];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        qA = b.QA[1];
                        qB = b.QA[1];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        fA = ob.QA[2];
                        fB = ob.QA[2];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        fA = ob.QA[3];
                        fB = ob.QA[3];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        wA = b.WS[0];
                        wB = b.WS[0];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        wA = b.WS[1];
                        wB = b.WS[1];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        cA = ob.WS[2];
                        cB = ob.WS[2];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        cA = ob.WS[3];
                        cB = ob.WS[3];
                    }

                    break;

                case FaceDirection.NegativeZ:
                    xA = x;
                    xB = x + 1;
                    zA = z;
                    zB = z;
                    ob = Blocks[x, z - 1];
                    qA = b.QA[3];
                    qB = b.QA[2];
                    eA = b.ED[3];
                    eB = b.ED[2];
                    rA = b.RF[3];
                    rB = b.RF[2];
                    wA = b.WS[3];
                    wB = b.WS[2];
                    fA = ob.QA[0];
                    fB = ob.QA[1];
                    cA = ob.WS[0];
                    cB = ob.WS[1];
                    qaFace = BlockFace.NegativeZ_QA;
                    edFace = BlockFace.NegativeZ_ED;
                    middleFace = BlockFace.NegativeZ_Middle;
                    rfFace = BlockFace.NegativeZ_RF;
                    wsFace = BlockFace.NegativeZ_WS;

                    if (b.WallPortal != null)
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

                        var qaNearA = nearBlock.QA[0];
                        var qaNearB = nearBlock.QA[1];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZn) qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZn) qaNearB = qaNearA;

                        var wsNearA = nearBlock.WS[0];
                        var wsNearB = nearBlock.WS[1];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingX = x + (int)(Position.X - adjoiningRoom.Position.X);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, 1) ?? Block.Empty;

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QA[3];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QA[2];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZp) qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZp) qBportal = qAportal;

                        qA = (int)Position.Y + qaNearA;
                        qB = (int)Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WS[3];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WS[2];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp) wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp) wBportal = wAportal;

                        wA = (int)Position.Y + wsNearA;
                        wB = (int)Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;

                        eA = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.ED[3];
                        eB = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.ED[2];
                        rA = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.RF[3];
                        rB = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.RF[2];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        qA = b.QA[3];
                        qB = b.QA[3];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        qA = b.QA[2];
                        qB = b.QA[2];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        fA = ob.QA[0];
                        fB = ob.QA[0];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        fA = ob.QA[1];
                        fB = ob.QA[1];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        wA = b.WS[3];
                        wB = b.WS[3];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        wA = b.WS[2];
                        wB = b.WS[2];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        cA = ob.WS[0];
                        cB = ob.WS[0];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        cA = ob.WS[1];
                        cB = ob.WS[1];
                    }

                    break;

                case FaceDirection.PositiveX:
                    xA = x + 1;
                    xB = x + 1;
                    zA = z;
                    zB = z + 1;
                    ob = Blocks[x + 1, z];
                    qA = b.QA[2];
                    qB = b.QA[1];
                    eA = b.ED[2];
                    eB = b.ED[1];
                    rA = b.RF[2];
                    rB = b.RF[1];
                    wA = b.WS[2];
                    wB = b.WS[1];
                    fA = ob.QA[3];
                    fB = ob.QA[0];
                    cA = ob.WS[3];
                    cB = ob.WS[0];
                    qaFace = BlockFace.PositiveX_QA;
                    edFace = BlockFace.PositiveX_ED;
                    middleFace = BlockFace.PositiveX_Middle;
                    rfFace = BlockFace.PositiveX_RF;
                    wsFace = BlockFace.PositiveX_WS;

                    if (b.WallPortal != null)
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

                        var qaNearA = nearBlock.QA[3];
                        var qaNearB = nearBlock.QA[0];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZn) qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZp) qaNearB = qaNearA;

                        var wsNearA = nearBlock.WS[3];
                        var wsNearB = nearBlock.WS[0];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp) wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Block.Empty;

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QA[2];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QA[1];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZn) qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZp) qBportal = qAportal;

                        qA = (int)Position.Y + qaNearA;
                        qB = (int)Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WS[2];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WS[1];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp) wBportal = wAportal;

                        wA = (int)Position.Y + wsNearA;
                        wB = (int)Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;

                        eA = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.ED[2];
                        eB = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.ED[1];
                        rA = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.RF[2];
                        rB = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.RF[1];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        qA = b.QA[1];
                        qB = b.QA[1];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        qA = b.QA[2];
                        qB = b.QA[2];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        fA = ob.QA[0];
                        fB = ob.QA[0];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        fA = ob.QA[3];
                        fB = ob.QA[3];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        wA = b.WS[1];
                        wB = b.WS[1];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        wA = b.WS[2];
                        wB = b.WS[2];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        cA = ob.WS[0];
                        cB = ob.WS[0];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        cA = ob.WS[3];
                        cB = ob.WS[3];
                    }

                    break;

                case FaceDirection.DiagonalFloor:
                    switch (b.FloorDiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
                            qA = b.QA[1];
                            qB = b.QA[3];
                            eA = b.ED[1];
                            eB = b.ED[3];
                            rA = b.RF[1];
                            rB = b.RF[3];
                            wA = b.WS[1];
                            wB = b.WS[3];
                            fA = b.QA[0];
                            fB = b.QA[0];
                            cA = b.WS[0];
                            cB = b.WS[0];
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
                            qA = b.QA[2];
                            qB = b.QA[0];
                            eA = b.ED[2];
                            eB = b.ED[0];
                            rA = b.RF[2];
                            rB = b.RF[0];
                            wA = b.WS[2];
                            wB = b.WS[0];
                            fA = b.QA[1];
                            fB = b.QA[1];
                            cA = b.WS[1];
                            cB = b.WS[1];
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
                            qA = b.QA[3];
                            qB = b.QA[1];
                            eA = b.ED[3];
                            eB = b.ED[1];
                            rA = b.RF[3];
                            rB = b.RF[1];
                            wA = b.WS[3];
                            wB = b.WS[1];
                            fA = b.QA[2];
                            fB = b.QA[2];
                            cA = b.WS[2];
                            cB = b.WS[2];
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
                            qA = b.QA[0];
                            qB = b.QA[2];
                            eA = b.ED[0];
                            eB = b.ED[2];
                            rA = b.RF[0];
                            rB = b.RF[2];
                            wA = b.WS[0];
                            wB = b.WS[2];
                            fA = b.QA[3];
                            fB = b.QA[3];
                            cA = b.WS[3];
                            cB = b.WS[3];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                    }

                    break;

                case FaceDirection.DiagonalCeiling:
                    switch (b.CeilingDiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
                            qA = b.QA[1];
                            qB = b.QA[3];
                            eA = b.ED[1];
                            eB = b.ED[3];
                            rA = b.RF[1];
                            rB = b.RF[3];
                            wA = b.WS[1];
                            wB = b.WS[3];
                            fA = b.QA[0];
                            fB = b.QA[0];
                            cA = b.WS[0];
                            cB = b.WS[0];
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
                            qA = b.QA[2];
                            qB = b.QA[0];
                            eA = b.ED[2];
                            eB = b.ED[0];
                            rA = b.RF[2];
                            rB = b.RF[0];
                            wA = b.WS[2];
                            wB = b.WS[0];
                            fA = b.QA[1];
                            fB = b.QA[1];
                            cA = b.WS[1];
                            cB = b.WS[1];
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
                            qA = b.QA[3];
                            qB = b.QA[1];
                            eA = b.ED[3];
                            eB = b.ED[1];
                            rA = b.RF[3];
                            rB = b.RF[1];
                            wA = b.WS[3];
                            wB = b.WS[1];
                            fA = b.QA[2];
                            fB = b.QA[2];
                            cA = b.WS[2];
                            cB = b.WS[2];
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
                            qA = b.QA[0];
                            qB = b.QA[2];
                            eA = b.ED[0];
                            eB = b.ED[2];
                            rA = b.RF[0];
                            rB = b.RF[2];
                            wA = b.WS[0];
                            wB = b.WS[2];
                            fA = b.QA[3];
                            fB = b.QA[3];
                            cA = b.WS[3];
                            cB = b.WS[3];
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
                    ob = Blocks[x - 1, z];
                    qA = b.QA[0];
                    qB = b.QA[3];
                    eA = b.ED[0];
                    eB = b.ED[3];
                    rA = b.RF[0];
                    rB = b.RF[3];
                    wA = b.WS[0];
                    wB = b.WS[3];
                    fA = ob.QA[1];
                    fB = ob.QA[2];
                    cA = ob.WS[1];
                    cB = ob.WS[2];
                    qaFace = BlockFace.NegativeX_QA;
                    edFace = BlockFace.NegativeX_ED;
                    middleFace = BlockFace.NegativeX_Middle;
                    rfFace = BlockFace.NegativeX_RF;
                    wsFace = BlockFace.NegativeX_WS;

                    if (b.WallPortal != null)
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

                        var qaNearA = nearBlock.QA[1];
                        var qaNearB = nearBlock.QA[2];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZp) qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZn) qaNearB = qaNearA;

                        var wsNearA = nearBlock.WS[1];
                        var wsNearB = nearBlock.WS[2];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp) wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn) wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingZ = z + (int)(Position.Z - adjoiningRoom.Position.Z);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(1, facingZ) ?? Block.Empty;

                        int qAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QA[0];
                        int qBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.QA[3];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZp) qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZn) qBportal = qAportal;

                        qA = (int)Position.Y + qaNearA;
                        qB = (int)Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - (int)Position.Y;
                        qB = Math.Max(qB, qBportal) - (int)Position.Y;

                        int wAportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WS[0];
                        int wBportal = (int)adjoiningRoom.Position.Y + adjoiningBlock.WS[3];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp) wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn) wBportal = wAportal;

                        wA = (int)Position.Y + wsNearA;
                        wB = (int)Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - (int)Position.Y;
                        wB = Math.Min(wB, wBportal) - (int)Position.Y;

                        eA = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.ED[1];
                        eB = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.ED[2];
                        rA = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.RF[1];
                        rB = (int)adjoiningRoom.Position.Y - (int)Position.Y + adjoiningBlock.RF[2];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        qA = b.QA[0];
                        qB = b.QA[0];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        qA = b.QA[3];
                        qB = b.QA[3];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        fA = ob.QA[1];
                        fB = ob.QA[1];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        fA = ob.QA[2];
                        fB = ob.QA[2];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        wA = b.WS[0];
                        wB = b.WS[0];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        wA = b.WS[3];
                        wB = b.WS[3];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        cA = ob.WS[1];
                        cB = ob.WS[1];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        cA = ob.WS[2];
                        cB = ob.WS[2];
                    }

                    break;
            }

            var subdivide = false;

            // Always check these
            if (qA >= cA && qB >= cB)
            {
                qA = cA;
                qB = cB;
            }

            if (wA <= fA && wB <= fB)
            {
                wA = fA;
                wB = fB;
            }

            // Following checks are only for wall's faces
            if (b.IsAnyWall)
            {

                if (qA > fA && qB < fB || qA < fA && qB > fB)
                {
                    qA = fA;
                    qB = fB;
                }

                if (qA > cA && qB < cB || qA < cA && qB > cB)
                {
                    qA = cA;
                    qB = cB;
                }
                
                if (wA > cA && wB < cB || wA < cA && wB > cB)
                {
                    wA = cA;
                    wB = cB;
                }

                if (wA > fA && wB < fB || wA < fA && wB > fB)
                {
                    wA = fA;
                    wB = fB;
                }
            }

            if (!(qA == fA && qB == fB) && floor)
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
                face = b.GetFaceTexture(qaFace);

                // QA
                if (qA > yA && qB > yB)
                    AddQuad(x, z, qaFace,
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

                // ED
                if (subdivide)
                {
                    yA = fA;
                    yB = fB;

                    face = b.GetFaceTexture(edFace);

                    if (eA > yA && eB > yB)
                        AddQuad(x, z, edFace,
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

            if (!(wA == cA && wB == cB) && ceiling)
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
                face = b.GetFaceTexture(wsFace);

                // WS
                if (wA < yA && wB < yB)
                    AddQuad(x, z, wsFace,
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

                // RF
                if (subdivide)
                {
                    yA = cA;
                    yB = cB;

                    face = b.GetFaceTexture(rfFace);

                    if (rA < yA && rB < yB)
                        AddQuad(x, z, rfFace,
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

            if (!middle)
                return;

            face = b.GetFaceTexture(middleFace);

            yA = wA >= cA ? cA : wA;
            yB = wB >= cB ? cB : wB;
            int yD = qA <= fA ? fA : qA;
            int yC = qB <= fB ? fB : qB;

            // Middle
            if (yA != yD && yB != yC)
                AddQuad(x, z, middleFace,
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

        private void AddQuad(int x, int z, BlockFace face, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
                             TextureArea texture, Vector2 editorUV0, Vector2 editorUV1, Vector2 editorUV2, Vector2 editorUV3,
                             bool isForCeiling = false)
        {
            var sectorVertices = _sectorVertices[x, z];
            int sectorVerticesStart = sectorVertices.Count;

            sectorVertices.Add(new EditorVertex { Position = p1, UV = texture.TexCoord2, EditorUV = editorUV1 });  // 1
            sectorVertices.Add(new EditorVertex { Position = p2, UV = texture.TexCoord3, EditorUV = editorUV2 });  // 2
            sectorVertices.Add(new EditorVertex { Position = p0, UV = texture.TexCoord1, EditorUV = editorUV0 });  // 0
            sectorVertices.Add(new EditorVertex { Position = p3, UV = texture.TexCoord0, EditorUV = editorUV3 });  // 3
            sectorVertices.Add(new EditorVertex { Position = p0, UV = texture.TexCoord1, EditorUV = editorUV0 });
            sectorVertices.Add(new EditorVertex { Position = p2, UV = texture.TexCoord3, EditorUV = editorUV2 });

            _sectorFaceVertexVertexRange[x, z, (int)face] = new VertexRange { Start = sectorVerticesStart, Count = 6 };

            if (!isForCeiling)
            {
                _sectorFaceIndices[x, z, (int)face].AddRange(new int[] { sectorVerticesStart + 2,
                                                                         sectorVerticesStart + 0,
                                                                         sectorVerticesStart + 1,
                                                                         sectorVerticesStart + 3 });
            }
            else
            {
                _sectorFaceIndices[x, z, (int)face].AddRange(new int[] { sectorVerticesStart + 5,
                                                                         sectorVerticesStart + 1,
                                                                         sectorVerticesStart + 2,
                                                                         sectorVerticesStart + 0 });
            }
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
            _sectorFaceIndices[x, z, (int)face].AddRange(new int[] { sectorVerticesStart + 0,
                                                                     sectorVerticesStart + 1,
                                                                     sectorVerticesStart + 2 });
        }

        public struct IntersectionInfo
        {
            public VectorInt2 Pos;
            public BlockFace Face;
            public float Distance;
            public float VerticalCoord;
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
                                Vector3 position;
                                ray.Intersects(ref p0, ref p1, ref p2, out position);

                                var normal = Vector3.Cross(p1 - p0, p2 - p0);
                                if (Vector3.Dot(ray.Direction, normal) <= 0)
                                    if (!(distance > result.Distance))
                                        result = new IntersectionInfo() { Distance = distance, Face = face, Pos = new VectorInt2(x, z), VerticalCoord = position.Y };
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

                        if (((currentBlock.QA[0] + currentBlock.QA[3]) / 2 > currentYclick) ||
                            ((currentBlock.WS[0] + currentBlock.WS[3]) / 2 < currentYclick) ||
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

                        if (((currentBlock.QA[2] + currentBlock.QA[1]) / 2 > currentYclick) ||
                            ((currentBlock.WS[2] + currentBlock.WS[1]) / 2 < currentYclick) ||
                            currentBlock.Type == BlockType.Wall ||
                            ((nextBlock.QA[0] + nextBlock.QA[3]) / 2 > currentYclick) ||
                            ((nextBlock.WS[0] + nextBlock.WS[3]) / 2 < currentYclick) ||
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

                        if (((currentBlock.QA[2] + currentBlock.QA[3]) / 2 > currentYclick) ||
                            ((currentBlock.WS[2] + currentBlock.WS[3]) / 2 < currentYclick) ||
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

                        if (((currentBlock.QA[0] + currentBlock.QA[1]) / 2 > currentYclick) ||
                            ((currentBlock.WS[0] + currentBlock.WS[1]) / 2 < currentYclick) ||
                            currentBlock.Type == BlockType.Wall ||
                            ((nextBlock.QA[2] + nextBlock.QA[3]) / 2 > currentYclick) ||
                            ((nextBlock.WS[2] + nextBlock.WS[3]) / 2 < currentYclick) ||
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
                vertex.Color = AmbientLight;
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
                    faceColorSum += _allVertices[vertexIndex].Color;
                faceColorSum /= pair.Value.Count;
                foreach (var vertexIndex in pair.Value)
                {
                    var vertex = _allVertices[vertexIndex];
                    vertex.Color = faceColorSum;
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
            normal = normal.Normalize_();

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
                        case LightType.Point:
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
                                    if (light.IsObstructedByRoomGeometry)
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
                                    if (light.IsObstructedByRoomGeometry)
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
                                lightVector = lightVector.Normalize_();

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
                                    if (light.IsObstructedByRoomGeometry)
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

                vertex.Color.X = r * (1.0f / 128.0f);
                vertex.Color.Y = g * (1.0f / 128.0f);
                vertex.Color.Z = b * (1.0f / 128.0f);
                vertex.Color.W = 255.0f;

                _allVertices[range.Start + i] = vertex;
            }
        }

    /*    private Vector4 CalculateLightingForVertex(Vector3 position, Vector3 normal)
        {
            // No Linq here because it's slow
            List<LightInstance> lights = new List<LightInstance>();
            foreach (var instance in _objects)
            {
                LightInstance light = instance as LightInstance;
                if (light != null)
                    lights.Add(light);
            }

            int r = (int)(AmbientLight.X * 128);
            int g = (int)(AmbientLight.Y * 128);
            int b = (int)(AmbientLight.Z * 128);

            foreach (var light in lights) // No Linq here because it's slow
            {
                if ((!light.Enabled) || (!light.IsStaticallyUsed))
                    continue;

                switch (light.Type)
                {
                    case LightType.Point:
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
                            lightVector = lightVector.Normalize_();

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

            var color = new Vector4();

            color.X = r * (1.0f / 128.0f);
            color.Y = g * (1.0f / 128.0f);
            color.Z = b * (1.0f / 128.0f);
            color.W = 255.0f;

            return color;
        }*/

        public List<EditorVertex> GetRoomVertices()
        {
            return _allVertices;
        }

        public void UpdateBuffers()
        {
            // HACK
            if (_allVertices.Count == 0)
                return;

            if (_vertexBuffer != null) _vertexBuffer.Dispose();
            _vertexBuffer = Buffer.New(DeviceManager.DefaultDeviceManager.Device, _allVertices.ToArray(), BufferFlags.VertexBuffer);
            _vertexBuffer.SetData<EditorVertex>(_allVertices.ToArray());
        }

        public Buffer<EditorVertex> VertexBuffer => _vertexBuffer;

        public Matrix Transform => Matrix.Translation(new Vector3(Position.X * 1024.0f, Position.Y * 256.0f, Position.Z * 1024.0f));

        public int GetHighestCorner(RectangleInt2 area)
        {
            area = area.Intersect(LocalArea);

            int max = int.MinValue;
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!Blocks[x, z].IsAnyWall)
                        max = Math.Max(max, Blocks[x, z].CeilingMax);
            return max;
        }

        public int GetHighestCorner()
        {
            return GetHighestCorner(new RectangleInt2(1, 1, NumXSectors - 2, NumZSectors - 2));
        }

        public int GetLowestCorner(RectangleInt2 area)
        {
            area = area.Intersect(LocalArea);

            int min = int.MaxValue;
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!Blocks[x, z].IsAnyWall)
                        min = Math.Min(min, Blocks[x, z].FloorMin);
            return min;
        }

        public int GetLowestCorner()
        {
            return GetLowestCorner(new RectangleInt2(1, 1, NumXSectors - 2, NumZSectors - 2));
        }

        public Vector3 WorldPos
        {
            get { return new Vector3(Position.X * 1024.0f, Position.Y * 256.0f, Position.Z * 1024.0f); }
            set { Position = new Vector3(value.X * (1.0f / 1024.0f), value.Y * (1.0f / 256.0f), value.Z * (1.0f / 1024.0f)); }
        }

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
                    blockXnZn?.QA[Block.FaceXpZp],
                    blockXnZp?.QA[Block.FaceXpZn],
                    blockXpZn?.QA[Block.FaceXnZp],
                    blockXpZp?.QA[Block.FaceXnZn]),
                CeilingY = combineCeiling(
                    blockXnZn?.WS[Block.FaceXpZp],
                    blockXnZp?.WS[Block.FaceXpZn],
                    blockXpZn?.WS[Block.FaceXnZp],
                    blockXpZp?.WS[Block.FaceXnZn])
            };
        }

        public VerticalSpace? GetHeightInArea(RectangleInt2 area, Func<float?, float?, float?, float?, float> combineFloor, Func<float?, float?, float?, float?, float> combineCeiling)
        {
            VerticalSpace? result = null;
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
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

        public VerticalSpace? GetHeightInAreaAverage(RectangleInt2 area)
        {
            return GetHeightInArea(area, Average, Average);
        }

        public VerticalSpace? GetHeightInAreaMinSpace(RectangleInt2 area)
        {
            return GetHeightInArea(area, Max, Min);
        }

        public VerticalSpace? GetHeightInAreaMaxSpace(RectangleInt2 area)
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
                            lowest = Math.Min(lowest, b.QA[i]);
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
                        b.QA[i] -= lowest;
                        b.ED[i] -= lowest;
                        b.WS[i] -= lowest;
                        b.RF[i] -= lowest;
                    }
                }

            foreach (var instance in _objects)
                instance.Position -= new Vector3(0, lowest * 256, 0);
        }

        public static bool RemoveOutsidePortals(Level level, IEnumerable<Room> rooms, Func<IReadOnlyList<PortalInstance>, bool> beforeRemovePortals)
        {
            HashSet<Room> roomLookup = new HashSet<Room>(rooms);

            List<PortalInstance> portalsToRemove = new List<PortalInstance>();
            foreach (Room room in roomLookup)
                foreach (PortalInstance portal in room.Portals)
                    if (!roomLookup.Contains(portal.AdjoiningRoom))
                        portalsToRemove.Add(portal);

            if (portalsToRemove.Count == 0)
                return true;
            if (!beforeRemovePortals(portalsToRemove))
                return false;

            foreach (PortalInstance instance in portalsToRemove)
                if (instance.Room != null)
                    instance.Room.RemoveObject(level, instance);
            return true;
        }

        public bool AddObjectCutSectors(Level level, RectangleInt2 newArea, SectorBasedObjectInstance instance)
        {
            // Determine area
            RectangleInt2 instanceNewAreaConstraint = newArea.Inflate(-1);
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
            RectangleInt2 instanceNewArea = instance.Area.Intersect(instanceNewAreaConstraint) - newArea.Start;

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

        public ObjectInstance RemoveObjectAndSingularPortalAndKeepAlive(Level level, ObjectInstance instance)
        {
            instance.RemoveFromRoom(level, this);
            if (instance is PositionBasedObjectInstance)
                _objects.Remove((PositionBasedObjectInstance)instance);
            return instance;
        }

        public ObjectInstance RemoveObjectAndSingularPortal(Level level, ObjectInstance instance)
        {
            instance = RemoveObjectAndSingularPortalAndKeepAlive(level, instance);
            instance.Delete();
            return instance;
        }

        public IEnumerable<ObjectInstance> AddObject(Level level, ObjectInstance instance)
        {
            // Add portals and opposite portals
            var portal = instance as PortalInstance;
            if (portal != null)
            {
                RectangleInt2 oppositeArea = PortalInstance.GetOppositePortalArea(portal.Direction, portal.Area) + (SectorPos - portal.AdjoiningRoom.SectorPos);
                var oppositePortal = new PortalInstance(oppositeArea, PortalInstance.GetOppositeDirection(portal.Direction), this);

                // Add portals
                var addedObjects = new List<ObjectInstance>();
                try
                {
                    addedObjects.Add(AddObjectAndSingularPortal(level, portal));
                    if (AlternateVersion != null)
                        addedObjects.Add(AlternateVersion.AddObjectAndSingularPortal(level, portal.Clone()));

                    addedObjects.Add(portal.AdjoiningRoom.AddObjectAndSingularPortal(level, oppositePortal));
                    if (portal.AdjoiningRoom.AlternateVersion != null)
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

        public IEnumerable<ObjectInstance> RemoveObjectAndKeepAlive(Level level, ObjectInstance instance)
        {
            List<ObjectInstance> result = new List<ObjectInstance>();
            result.Add(RemoveObjectAndSingularPortal(level, instance));

            // Delete the corresponding other portals if necessary.
            var portal = instance as PortalInstance;
            if (portal != null)
            {
                var alternatePortal = portal.FindAlternatePortal(this?.AlternateVersion);
                if (alternatePortal != null)
                    result.Add(AlternateVersion.RemoveObjectAndSingularPortal(level, alternatePortal));

                var oppositePortal = portal.FindOppositePortal(this);
                if (oppositePortal != null)
                    result.Add(portal.AdjoiningRoom.RemoveObjectAndSingularPortal(level, oppositePortal));

                var oppositeAlternatePortal = oppositePortal?.FindAlternatePortal(portal.AdjoiningRoom?.AlternateVersion);
                if (oppositeAlternatePortal != null)
                    result.Add(oppositeAlternatePortal.Room.RemoveObjectAndSingularPortal(level, oppositeAlternatePortal));
            }
            return result;
        }

        public IEnumerable<ObjectInstance> RemoveObject(Level level, ObjectInstance instance)
        {
            IEnumerable<ObjectInstance> result = RemoveObjectAndKeepAlive(level, instance);
            foreach (ObjectInstance resultInstance in result)
                resultInstance.Delete();
            return result;
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
                bool matchesAtXnYn = (roomBelow.Position.Y + blockBelow.WS[Block.FaceXnZn]) == (roomAbove.Position.Y + blockAbove.QA[Block.FaceXnZn]);
                bool matchesAtXpYn = (roomBelow.Position.Y + blockBelow.WS[Block.FaceXpZn]) == (roomAbove.Position.Y + blockAbove.QA[Block.FaceXpZn]);
                bool matchesAtXnYp = (roomBelow.Position.Y + blockBelow.WS[Block.FaceXnZp]) == (roomAbove.Position.Y + blockAbove.QA[Block.FaceXnZp]);
                bool matchesAtXpYp = (roomBelow.Position.Y + blockBelow.WS[Block.FaceXpZp]) == (roomAbove.Position.Y + blockAbove.QA[Block.FaceXpZp]);

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

        public static RoomConnectionType CalculateRoomConnectionType(Room roomBelow, Room roomAbove, VectorInt2 posBelow, VectorInt2 posAbove, bool lookingFromAbove)
        {
            // Checkout all possible alternate room combinations and combine the results
            RoomConnectionType result = RoomConnectionType.FullPortal;
            foreach (var roomPair in GetPossibleAlternateRoomPairs(roomBelow, roomAbove, lookingFromAbove))
                result = Combine(result,
                    CalculateRoomConnectionTypeWithoutAlternates(roomPair.Key, roomPair.Value,
                        roomPair.Key.GetBlock(posBelow), roomPair.Value.GetBlock(posAbove)));
            return result;
        }

        public RoomConnectionInfo GetFloorRoomConnectionInfo(VectorInt2 pos)
        {
            Block block = GetBlock(pos);
            if (block.FloorPortal != null)
            {
                Room adjoiningRoom = block.FloorPortal.AdjoiningRoom;
                VectorInt2 adjoiningPos = pos + (SectorPos - adjoiningRoom.SectorPos);
                Block adjoiningBlock = adjoiningRoom.GetBlock(adjoiningPos);
                if (adjoiningBlock.CeilingPortal != null)
                    return new RoomConnectionInfo(block.FloorPortal, CalculateRoomConnectionType(adjoiningRoom, this, adjoiningPos, pos, true));
            }
            return new RoomConnectionInfo();
        }

        public RoomConnectionInfo GetCeilingRoomConnectionInfo(VectorInt2 pos)
        {
            Block block = GetBlock(pos);
            if (block.CeilingPortal != null)
            {
                Room adjoiningRoom = block.CeilingPortal.AdjoiningRoom;
                VectorInt2 adjoiningPos = pos + (SectorPos - adjoiningRoom.SectorPos);
                Block adjoiningBlock = adjoiningRoom.GetBlock(adjoiningPos);
                if (adjoiningBlock.FloorPortal != null)
                    return new RoomConnectionInfo(block.CeilingPortal, CalculateRoomConnectionType(this, adjoiningRoom, pos, adjoiningPos, false));
            }
            return new RoomConnectionInfo();
        }

        public void SmartBuildGeometry(RectangleInt2 area)
        {
            area = area.Inflate(1); // Add margin

            // Update adjoining rooms
            var roomsToProcess = new List<Room> { this };
            var areaToProcess = new List<RectangleInt2> { area };
            List<PortalInstance> listOfPortals = Portals.ToList();
            foreach (var portal in listOfPortals)
            {
                if (!portal.Area.Intersects(area))
                    continue; // This portal is irrelavant since no changes happend in its area

                RectangleInt2 portalArea = portal.Area.Intersect(area);
                RectangleInt2 otherRoomPortalArea = PortalInstance.GetOppositePortalArea(portal.Direction, portalArea) + (SectorPos - portal.AdjoiningRoom.SectorPos);

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

        public void CopyDependentLevelSettings(LevelSettings destinationLevelSettings, LevelSettings sourceLevelSettings, bool unifyData)
        {
            foreach (ObjectInstance instance in AnyObjects)
                instance.CopyDependentLevelSettings(destinationLevelSettings, sourceLevelSettings, unifyData);

            if (destinationLevelSettings.Textures.Count == 0)
                destinationLevelSettings.Textures.AddRange(sourceLevelSettings.Textures);
            else
            {
                int TODO_Merge_Textures_Once_We_Support_More_Than_One_Texture;
                if (unifyData)
                {
                    for (int z = 0; z < NumZSectors; ++z)
                        for (int x = 0; x < NumXSectors; ++x)
                        {
                            Block block = Blocks[x, z];
                            for (BlockFace face = 0; face < Block.FaceCount; ++face)
                            {
                                TextureArea textureArea = block.GetFaceTexture(face);
                                if (textureArea.Texture is LevelTexture)
                                {
                                    Vector2 maxSize = destinationLevelSettings.Textures[0].Image.Size;
                                    textureArea.Texture = destinationLevelSettings.Textures[0];
                                    textureArea.TexCoord0 = Vector2.Min(textureArea.TexCoord0, maxSize);
                                    textureArea.TexCoord1 = Vector2.Min(textureArea.TexCoord1, maxSize);
                                    textureArea.TexCoord2 = Vector2.Min(textureArea.TexCoord2, maxSize);
                                    textureArea.TexCoord3 = Vector2.Min(textureArea.TexCoord3, maxSize);
                                    block.SetFaceTexture(face, textureArea);
                                }
                            }
                        }
                }
            }
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