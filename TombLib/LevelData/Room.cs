using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.Graphics;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum Reverberation : byte
    {
        Outside, SmallRoom, MediumRoom, LargeRoom, Pipe
    }

    public enum RoomType : byte
    {
        Normal, Rain, Snow, Water, Quicksand
    }

    public enum RoomLightEffect : byte
    {
        None, Default, Reflection, Glow, Movement, GlowAndMovement, Mist
    }

    public class Room : ITriggerParameter
    {
        public delegate void RemovedFromRoomDelegate(Room instance);
        public event RemovedFromRoomDelegate DeletedEvent;

        public const short DefaultHeight = 12;
        public const short DefaultRoomDimensions = 20;
        public const short MaxRecommendedRoomDimensions = 31;

        public string Name { get; set; }
        public VectorInt3 Position { get; set; }
        public Block[,] Blocks { get; private set; }
        private List<PositionBasedObjectInstance> _objects = new List<PositionBasedObjectInstance>();

        public Room AlternateBaseRoom { get; set; }
        public Room AlternateRoom { get; set; }
        public short AlternateGroup { get; set; } = -1;

        public Vector3 AmbientLight { get; set; } = new Vector3(0.25f, 0.25f, 0.25f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)

        public RoomType Type { get; set; } = RoomType.Normal;
        public byte TypeStrength { get; set; } = 0;
        public RoomLightEffect LightEffect { get; set; } = RoomLightEffect.Default;
        public byte LightEffectStrength { get; set; } = 0;

        public bool FlagCold { get; set; }
        public bool FlagDamage { get; set; }
        public bool FlagOutside { get; set; }
        public bool FlagHorizon { get; set; }
        public bool FlagNoLensflare { get; set; }
        public bool FlagExcludeFromPathFinding { get; set; }
        public Reverberation Reverberation { get; set; }
        public bool Locked { get; set; }
        public ImportedGeometryMesh ExternalRoomMesh { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public Level Level { get; set; }

        // Internal data structures
        public RoomGeometry RoomGeometry { get; set; }

        public Room(Level level, int numXSectors, int numZSectors, Vector3 ambientLight, string name = "Unnamed", short ceiling = DefaultHeight)
        {
            Name = name;
            Level = level;
            AmbientLight = ambientLight;
            Resize(null, new RectangleInt2(0, 0, numXSectors - 1, numZSectors - 1), 0, ceiling, true);
            BuildGeometry();
        }

        public Room(Level level, VectorInt2 sectorSize, Vector3 ambientLight, string name = "Unnamed", short ceiling = DefaultHeight)
            : this(level, sectorSize.X, sectorSize.Y, ambientLight, name, ceiling)
        { }

        // Usually it's highly recommended to call FixupNeighborPortals afterwards, to fix neighboring portals.
        public void Resize(Level level, RectangleInt2 area, short floor = 0, short ceiling = DefaultHeight, bool? useFloor = false)
        {
            int numXSectors = area.Width + 1;
            int numZSectors = area.Height + 1;
            VectorInt2 offset = area.Start;

            if (numXSectors < 3 || numZSectors < 3)
                throw new ArgumentOutOfRangeException(nameof(area), area, "Provided area for resizing the room is too small. The area must span at least 3 sectors in X and Z dimension.");

            // Remove sector based objects if there are any
            var sectorObjects = Blocks != null ? SectorObjects.ToList() : new List<SectorBasedObjectInstance>();
            foreach (var instance in sectorObjects)
                RemoveObjectAndSingularPortalAndKeepAlive(level, instance);

            // Build new blocks
            Block[,] newBlocks = new Block[numXSectors, numZSectors];
            for (int x = 0; x < numXSectors; x++)
                for (int z = 0; z < numZSectors; z++)
                {
                    Block oldBlock = GetBlockTry(new VectorInt2(x, z) + offset);
                    newBlocks[x, z] = oldBlock ?? new Block(floor, ceiling);

                    if (oldBlock == null || (useFloor.HasValue && newBlocks[x, z].Type == BlockType.BorderWall))
                        newBlocks[x, z].Type = !useFloor.HasValue || useFloor.Value ? BlockType.Floor : BlockType.Wall;
                    
                    if(!useFloor.HasValue)
                    {
                        oldBlock.Raise(BlockVertical.Floor, floor);
                        oldBlock.Raise(BlockVertical.Ed, floor);
                        oldBlock.Raise(BlockVertical.Ceiling, ceiling);
                        oldBlock.Raise(BlockVertical.Rf, ceiling);
                    }

                    if (x == 0 || z == 0 || x == numXSectors - 1 || z == numZSectors - 1)
                    {
                        newBlocks[x, z].Type = BlockType.BorderWall;
                        newBlocks[x, z].Floor.DiagonalSplit = DiagonalSplit.None;
                        newBlocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.None;
                    }
                }

            // Update data structures
            RoomGeometry = null;
            Blocks = newBlocks;

            // Move objects
            SectorPos = SectorPos + offset;
            foreach (var instance in _objects)
                instance.Position -= new Vector3(offset.X * 1024, 0, offset.Y * 1024);

            // Add sector based objects again
            foreach (var instance in sectorObjects)
                AddObjectAndSingularPortalCutSectors(level, area, instance);
        }

        // Usually it's highly recommended to call FixupNeighborPortals on both rooms afterwards, to fix neighboring portals.
        public Room Split(Level level, RectangleInt2 area, Room alternateOppositeSplitRoom = null)
        {
            var newRoom = Clone(level, instance => !(instance is PositionBasedObjectInstance) && !(instance is PortalInstance));
            newRoom.Name = "Split from " + Name;
            newRoom.Resize(level, area);

            // Setup alternate rooms
            newRoom.AlternateGroup = AlternateGroup;
            if (alternateOppositeSplitRoom != null)
                if (AlternateRoom != null)
                {
                    alternateOppositeSplitRoom.AlternateBaseRoom = newRoom;
                    newRoom.AlternateRoom = alternateOppositeSplitRoom;
                }
                else if (AlternateBaseRoom != null)
                {
                    alternateOppositeSplitRoom.AlternateRoom = newRoom;
                    newRoom.AlternateBaseRoom = alternateOppositeSplitRoom;
                }

            // Detect if the room was split by a straight line
            // If this is the case, resize the original room
            List<PortalInstance> portals = Portals.ToList();
            if (area.X0 == 0 && area.Y0 == 0 && area.X1 == NumXSectors - 1 && area.Y1 < NumZSectors - 1)
            {
                Resize(level, new RectangleInt2(area.X0, area.Y1 - 1, area.X1, NumZSectors - 1));
                AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(area.X0 + 1, 0, area.X1 - 1, 0), PortalDirection.WallNegativeZ, newRoom));
                newRoom.AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(area.X0 + 1, newRoom.NumZSectors - 1, area.X1 - 1, newRoom.NumZSectors - 1), PortalDirection.WallPositiveZ, this));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z < 1024)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if (area.X0 == 0 && area.Y0 == 0 && area.X1 < NumXSectors - 1 && area.Y1 == NumZSectors - 1)
            {
                Resize(level, new RectangleInt2(area.X1 - 1, area.Y0, NumXSectors - 1, area.Y1));
                AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(0, area.Y0 + 1, 0, area.Y1 - 1), PortalDirection.WallNegativeX, newRoom));
                newRoom.AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(newRoom.NumXSectors - 1, area.Y0 + 1, newRoom.NumXSectors - 1, area.Y1 - 1), PortalDirection.WallPositiveX, this));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.X < 1024)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if (area.X0 == 0 && area.Y0 > 0 && area.X1 == NumXSectors - 1 && area.Y1 == NumZSectors - 1)
            {
                Resize(level, new RectangleInt2(area.X0, 0, area.X1, area.Y0 + 1));
                AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(area.X0 + 1, NumZSectors - 1, area.X1 - 1, NumZSectors - 1), PortalDirection.WallPositiveZ, newRoom));
                newRoom.AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(area.X0 + 1, 0, area.X1 - 1, 0), PortalDirection.WallNegativeZ, this));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z > (NumZSectors - 1) * 1024)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if (area.X0 > 0 && area.Y0 == 0 && area.X1 == NumXSectors - 1 && area.Y1 == NumZSectors - 1)
            {
                Resize(level, new RectangleInt2(0, area.Y0, area.X0 + 1, area.Y1));
                AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(NumXSectors - 1, area.Y0 + 1, NumXSectors - 1, area.Y1 - 1), PortalDirection.WallPositiveX, newRoom));
                newRoom.AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(0, area.Y0 + 1, 0, area.Y1 - 1), PortalDirection.WallNegativeX, this));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.X > (NumXSectors - 1) * 1024)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else
            {
                // Resize area
                for (int z = area.Y0 + 1; z < area.Y1; ++z)
                    for (int x = area.X0 + 1; x < area.X1; ++x)
                        Blocks[x, z].Type = BlockType.Wall;

                // Move objects
                Vector2 start = (area.Start + new Vector2(1, 1)) * 1024.0f;
                Vector2 end = (area.End - new Vector2(0, 0)) * 1024.0f;
                foreach (PortalInstance portal in portals)
                {
                    RemoveObjectAndSingularPortalAndKeepAlive(level, portal);
                    RectangleInt2 validPortalArea = portal.GetValidArea(area);
                    foreach (PortalInstance splitPortal in portal.SplitIntoRectangles(pos => !validPortalArea.Contains(pos), new VectorInt2()))
                        AddObjectAndSingularPortal(level, splitPortal);
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                }
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.X > start.X && instance.Position.Z > start.Y &&
                        instance.Position.X < end.X && instance.Position.Z < end.Y)
                        newRoom.MoveObjectFrom(level, this, instance);
            }

            newRoom.BuildGeometry();
            BuildGeometry();
            return newRoom;
        }

        public Room Clone(Level level, Predicate<ObjectInstance> decideToCopy, bool fullCopy = false)
        {
            // Copy most variables
            var result = (Room)MemberwiseClone();

            if(!fullCopy)
            {
                result.AlternateBaseRoom = null;
                result.AlternateRoom = null;
                result.AlternateGroup = -1;
            }

            result.RoomGeometry = null;

            // Copy blocks
            result.Blocks = new Block[NumXSectors, NumZSectors];
            for (int z = 0; z < NumZSectors; ++z)
                for (int x = 0; x < NumXSectors; ++x)
                    result.Blocks[x, z] = Blocks[x, z].Clone();

            if(decideToCopy != null)
            {
                // Copy objects
                result._objects = new List<PositionBasedObjectInstance>();
                foreach (var instance in AnyObjects)
                    if (decideToCopy(instance))
                        result.AddObjectAndSingularPortal(level, instance.Clone());
            }

            return result;
        }

        public Room Clone(Level level)
        {
            return Clone(level, instance => !(instance is PortalInstance));
        }

        /// <summary>
        /// This methode should be invoked after the object was added to a room and the object will go out of scope.
        /// </summary>
        public void Delete(Level level)
        {
            // Remove all objects in the room
            var objectsToRemove = AnyObjects.ToList();
            foreach (var instance in objectsToRemove)
                if (AlternateBaseRoom != null)
                    RemoveObjectAndSingularPortal(level, instance);
                else
                    RemoveObject(level, instance);

            // Inform of room removal
            DeletedEvent?.Invoke(this);
        }

        public List<Room> AndAdjoiningRooms => new List<Room>(Portals.ToList()
                                                                     .GroupBy(item => item.AdjoiningRoom)
                                                                     .Select(group => group.First())
                                                                     .Select(item => item.AdjoiningRoom)
                                                                     .ToList()) { this };

        public bool ExistsInLevel => Level != null && Array.IndexOf(Level.Rooms, this) != -1;
        public bool Alternated => AlternateRoom != null || AlternateBaseRoom != null;
        public Room AlternateOpposite => AlternateRoom ?? AlternateBaseRoom;
        public VectorInt2 SectorSize => new VectorInt2(NumXSectors, NumZSectors);
        public RectangleInt2 WorldArea => new RectangleInt2(Position.X, Position.Z, Position.X + NumXSectors - 1, Position.Z + NumZSectors - 1);
        public BoundingBox WorldBoundingBox => new BoundingBox(new Vector3(Position.X, Position.Y + GetLowestCorner(), Position.Z)*new Vector3(1024.0f,256.0f,1024.0f),
                                                               new Vector3(Position.X + NumXSectors - 1, Position.Y + GetHighestCorner(), Position.Z + NumZSectors - 1) * new Vector3(1024.0f, 256.0f, 1024.0f));
        public RectangleInt2 LocalArea => new RectangleInt2(0, 0, NumXSectors - 1, NumZSectors - 1);
        public bool CoordinateInvalid(int x, int z) => x < 0 || z < 0 || x >= NumXSectors || z >= NumZSectors;
        public IEnumerable<Room> Versions
        {
            get
            {
                yield return this;
                Room alternateVersion = AlternateOpposite;
                if (alternateVersion != null)
                    yield return alternateVersion;
            }
        }

        public VectorInt2 SectorPos
        {
            get { return new VectorInt2(Position.X, Position.Z); }
            set { Position = new VectorInt3(value.X, Position.Y, value.Y); }
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

        public void SetBlock(VectorInt2 pos, Block block)
        {
            Blocks[pos.X, pos.Y] = block;
        }

        public Block GetBlockTry(int x, int z)
        {
            if (Blocks == null)
                return null;
            if (x >= 0 && z >= 0 && x < NumXSectors && z < NumZSectors)
                return Blocks[x, z];
            return null;
        }

        public Block GetBlockTry(VectorInt2 pos)
        {
            return GetBlockTry(pos.X, pos.Y);
        }

        ///<param name="x">The X-coordinate. The point at room.Position is it at (0, 0)</param>
        ///<param name="z">The Z-coordinate. The point at room.Position is it at (0, 0)</param>
        public List<int> GetHeightsAtPoint(int x, int z, BlockVertical vertical)
        {
            List<int> values = new List<int>();
            for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
            {
                Block block = GetBlockTry(x - edge.DirectionX(), z - edge.DirectionZ());
                if (block != null && !block.IsAnyWall)
                    values.Add(block.GetHeight(vertical, edge));
            }
            return values;
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
            // We must also check if floor portal is solid or not
            if (!result.Block.FloorPortal.IsTraversable || GetFloorRoomConnectionInfo(pos).TraversableType != RoomConnectionType.FullPortal)
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

            if (sector.WallPortal == null)
                return new RoomBlockPair { Room = this, Block = sector, Pos = pos };

            Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;
            VectorInt2 adjoiningSectorCoordinate = pos + (SectorPos - adjoiningRoom.SectorPos);
            Block sector2 = adjoiningRoom.GetBlockTry(adjoiningSectorCoordinate);
            return new RoomBlockPair { Room = adjoiningRoom, Block = sector2, Pos = adjoiningSectorCoordinate };
        }
        public RoomBlockPair GetBlockTryThroughPortal(int x, int z) => GetBlockTryThroughPortal(new VectorInt2(x, z));

        public void ModifyHeightThroughPortal(int x, int z, BlockVertical vertical, int increment, RectangleInt2 area)
        {
            if (x <= 0 || z <= 0 || x >= NumXSectors || z >= NumZSectors)
                return;

            if (area.Contains(new VectorInt2(x, z)))
            {
                if (vertical.IsOnFloor() && Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsOnCeiling() && Blocks[x, z].Ceiling.DiagonalSplit == DiagonalSplit.None)
                {
                    Blocks[x, z].ChangeHeight(vertical, BlockEdge.XnZn, increment);
                    Blocks[x, z].FixHeights(vertical);
                }
            }
            if (area.Contains(new VectorInt2(x - 1, z)))
            {
                var adjacentLeftBlock = GetBlockTry(x - 1, z);
                if (adjacentLeftBlock != null && (vertical.IsOnFloor() && adjacentLeftBlock.Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsOnCeiling() && adjacentLeftBlock.Ceiling.DiagonalSplit == DiagonalSplit.None))
                {
                    adjacentLeftBlock.ChangeHeight(vertical, BlockEdge.XpZn, increment);
                    adjacentLeftBlock.FixHeights(vertical);
                }
            }
            if (area.Contains(new VectorInt2(x, z - 1)))
            {
                var adjacentBottomBlock = GetBlockTry(x, z - 1);
                if (adjacentBottomBlock != null && (vertical.IsOnFloor() && adjacentBottomBlock.Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsOnCeiling() && adjacentBottomBlock.Ceiling.DiagonalSplit == DiagonalSplit.None))
                {
                    adjacentBottomBlock.ChangeHeight(vertical, BlockEdge.XnZp, increment);
                    adjacentBottomBlock.FixHeights(vertical);
                }
            }
            if (area.Contains(new VectorInt2(x - 1, z - 1)))
            {
                var adjacentBottomLeftBlock = GetBlockTry(x - 1, z - 1);
                if (adjacentBottomLeftBlock != null && (vertical.IsOnFloor() && adjacentBottomLeftBlock.Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsOnCeiling() && adjacentBottomLeftBlock.Ceiling.DiagonalSplit == DiagonalSplit.None))
                {
                    adjacentBottomLeftBlock.ChangeHeight(vertical, BlockEdge.XpZp, increment);
                    adjacentBottomLeftBlock.FixHeights(vertical);
                }
            }
        }

        public bool IsIllegalSlope(int x, int z)
        {
            Block sector = GetBlockTry(x, z);

            if (Type == RoomType.Water || sector == null || sector.IsAnyWall || !sector.Floor.HasSlope)
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
                BlockEdge[] heightsToCompare;
                BlockEdge[] heightsToCheck;

                switch (slopeDirections[i])
                {
                    case Direction.PositiveZ:
                        lookupBlock = GetBlockTryThroughPortal(x, z + 1);
                        heightsToCompare = new BlockEdge[2] { BlockEdge.XnZp, BlockEdge.XpZp };
                        heightsToCheck = new BlockEdge[2] { BlockEdge.XnZn, BlockEdge.XpZn };
                        break;

                    case Direction.NegativeZ:
                        lookupBlock = GetBlockTryThroughPortal(x, z - 1);
                        heightsToCompare = new BlockEdge[2] { BlockEdge.XpZn, BlockEdge.XnZn };
                        heightsToCheck = new BlockEdge[2] { BlockEdge.XpZp, BlockEdge.XnZp };
                        break;

                    // We only need to override east and west diagonal split HeightsToCompare[1] cases, because
                    // slanted diagonal splits are always 45 degrees, hence these are only two slide directions possible.

                    case Direction.PositiveX:
                        lookupBlock = GetBlockTryThroughPortal(x + 1, z);
                        heightsToCompare = new BlockEdge[2] { BlockEdge.XpZp, sector.Floor.DiagonalSplit == DiagonalSplit.XnZp ? BlockEdge.XpZp : BlockEdge.XpZn };
                        heightsToCheck = new BlockEdge[2] { BlockEdge.XnZp, BlockEdge.XnZn };
                        break;

                    case Direction.NegativeX:
                        lookupBlock = GetBlockTryThroughPortal(x - 1, z);
                        heightsToCompare = new BlockEdge[2] { BlockEdge.XnZn, sector.Floor.DiagonalSplit == DiagonalSplit.XpZn ? BlockEdge.XnZn : BlockEdge.XnZp };
                        heightsToCheck = new BlockEdge[2] { BlockEdge.XpZn, BlockEdge.XpZp };
                        break;
                    default:
                        continue;
                }

                // Diagonal split resolver

                if (lookupBlock.Block.IsAnyWall && lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.None)
                {
                    // If lookup block contains non-diagonal wall, we mark slope as illegal in any case.

                    slopeIsIllegal = true;
                    continue;
                }
                else if (lookupBlock.Block.Floor.DiagonalSplit != DiagonalSplit.None)
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
                            if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XnZn ||
                                lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (lookupBlock.Block.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                                heightsToCheck[0] = BlockEdge.XpZn;
                            else
                                heightsToCheck[1] = BlockEdge.XnZn;
                            break;
                        case Direction.PositiveX:
                            if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XnZn ||
                                lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (lookupBlock.Block.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                                heightsToCheck[0] = BlockEdge.XnZn;
                            else
                                heightsToCheck[1] = BlockEdge.XnZp;
                            break;
                        case Direction.NegativeZ:
                            if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XpZp ||
                                lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (lookupBlock.Block.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                                heightsToCheck[0] = BlockEdge.XnZp;
                            else
                                heightsToCheck[1] = BlockEdge.XpZp;
                            break;
                        case Direction.NegativeX:
                            if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XpZp ||
                                lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (lookupBlock.Block.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                                heightsToCheck[0] = BlockEdge.XpZp;
                            else
                                heightsToCheck[1] = BlockEdge.XpZn;
                            break;
                    }
                }

                int heightAdjust = Position.Y - lookupBlock.Room.Position.Y;
                int absoluteLowestPassableHeight = lowestPassableHeight + heightAdjust;
                int absoluteLowestPassableStep = lowestPassableStep + heightAdjust;

                // Main comparer

                if (lookupBlock.Block.FloorPortal == null)
                {
                    if (lookupBlock.Block.Floor.GetHeight(heightsToCheck[0]) - sector.Floor.GetHeight(heightsToCompare[0]) > absoluteLowestPassableStep ||
                        lookupBlock.Block.Floor.GetHeight(heightsToCheck[1]) - sector.Floor.GetHeight(heightsToCompare[1]) > absoluteLowestPassableStep)
                        slopeIsIllegal = true;
                    else
                    {
                        var lookupBlockSlopeDirections = lookupBlock.Block.GetFloorTriangleSlopeDirections();

                        for (int j = 0; j < 2; j++)
                        {
                            // If both current and lookup sector triangle split direction is the same, ignore far opposite triangles.
                            // FIXME: works only for normal sectors. Diagonal steps are broken!

                            if (!sector.Floor.IsQuad && !lookupBlock.Block.Floor.IsQuad)
                            {
                                var sectorSplitDirection = sector.Floor.DiagonalSplit == DiagonalSplit.None ? sector.Floor.SplitDirectionIsXEqualsZ : (int)sector.Floor.DiagonalSplit % 2 != 0;
                                var lookupSplitDirection = lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.None ? lookupBlock.Block.Floor.SplitDirectionIsXEqualsZ : (int)lookupBlock.Block.Floor.DiagonalSplit % 2 != 0;

                                if (sectorSplitDirection == lookupSplitDirection)
                                {
                                    if (sector.Floor.DiagonalSplit != DiagonalSplit.None && lookupBlock.Block.Floor.DiagonalSplit != DiagonalSplit.None)
                                        continue;

                                    if (sectorSplitDirection)
                                    {
                                        if ((slopeDirections[i] == Direction.PositiveZ || slopeDirections[i] == Direction.NegativeX) && i == 0 && j == 1 ||
                                            (slopeDirections[i] == Direction.PositiveX || slopeDirections[i] == Direction.NegativeZ) && i == 1 && j == 0)
                                            continue;
                                    }
                                    else
                                    {
                                        if (slopeDirections[i] < Direction.NegativeZ && i == 1 && j == 0 ||
                                            slopeDirections[i] >= Direction.NegativeZ && i == 0 && j == 1)
                                            continue;
                                    }
                                }
                            }

                            // This code is needed to get REAL triangle index for diagonal step cases, because for some reason, triangle indices are inverted in this case.

                            var realTriangleIndex = j;

                            if (lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XpZn || lookupBlock.Block.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                                realTriangleIndex = realTriangleIndex == 0 ? 1 : 0;

                            // Triangle is considered illegal only if its lowest point lies lower than lowest passable step height compared to opposite triangle minimum point.
                            // Triangle is NOT considered illegal, if its slide direction is perpendicular to opposite triangle slide direction.

                            var heightDifference = sector.GetTriangleMinimumFloorPoint(i) - (lookupBlock.Block.GetTriangleMinimumFloorPoint(realTriangleIndex) - heightAdjust);

                            if (heightsToCheck[0] != heightsToCheck[1] && heightDifference <= lowestPassableStep || // Ordinary cases
                                heightsToCheck[0] == heightsToCheck[1] && heightDifference <= 0 && heightDifference > -lowestPassableStep)  // Diagonal step cases
                            {

                                if (lookupBlockSlopeDirections[j] != Direction.None && lookupBlockSlopeDirections[j] != slopeDirections[i])
                                    if ((int)lookupBlockSlopeDirections[j] % 2 == (int)slopeDirections[i] % 2)
                                        slopeIsIllegal = true;
                            }
                        }
                    }
                }

                if (lookupBlock.Block.CeilingPortal == null)
                {
                    if (lookupBlock.Block.Ceiling.GetHeight(heightsToCheck[0]) - sector.Floor.GetHeight(heightsToCompare[0]) < absoluteLowestPassableHeight ||
                        lookupBlock.Block.Ceiling.GetHeight(heightsToCheck[1]) - sector.Floor.GetHeight(heightsToCompare[1]) < absoluteLowestPassableHeight ||
                        lookupBlock.Block.Ceiling.GetHeight(heightsToCheck[0]) - lookupBlock.Block.Floor.GetHeight(heightsToCheck[0]) < lowestPassableHeight ||
                        lookupBlock.Block.Ceiling.GetHeight(heightsToCheck[1]) - lookupBlock.Block.Floor.GetHeight(heightsToCheck[1]) < lowestPassableHeight)
                        slopeIsIllegal = true;
                }
            }

            return slopeIsIllegal;
        }

        public bool IsAnyPortal(PortalDirection direction, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    switch(direction)
                    {
                        case PortalDirection.Floor:
                            if (Blocks[x, z].FloorPortal != null)
                                return true;
                            continue;
                        case PortalDirection.Ceiling:
                            if (Blocks[x, z].CeilingPortal != null)
                                return true;
                            continue;
                        default:
                            if (Blocks[x, z].WallPortal != null)
                                return true;
                            continue;
                    }
                }
            return false;
        }

        public bool CornerSelected(RectangleInt2 area)
        {
            // Check if one of the four corner is selected
            var cornerSelected = false;
            if (area.X0 == 0 && area.Y0 == 0 || area.X1 == 0 && area.Y1 == 0)
                cornerSelected = true;
            if (area.X0 == 0 && area.Y0 == NumZSectors - 1 || area.X1 == 0 && area.Y1 == NumZSectors - 1)
                cornerSelected = true;
            if (area.X0 == NumXSectors - 1 && area.Y0 == 0 || area.X1 == NumXSectors - 1 && area.Y1 == 0)
                cornerSelected = true;
            if (area.X0 == NumXSectors - 1 && area.Y0 == NumZSectors - 1 || area.X1 == NumXSectors - 1 && area.Y1 == NumZSectors - 1)
                cornerSelected = true;
            return cornerSelected;
        }

        public bool IsFaceDefined(int x, int z, BlockFace face)
        {
            return RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorInfo(x, z, face)).Count != 0;
        }

        public float GetFaceHighestPoint(int x, int z, BlockFace face)
        {
            var range = RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorInfo(x, z, face));
            float max = float.NegativeInfinity;
            for (int i = 0; i < range.Count; ++i)
                max = Math.Max(RoomGeometry.VertexPositions[i + range.Start].Y, max);
            return max;
        }

        public float GetFaceLowestPoint(int x, int z, BlockFace face)
        {
            var range = RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorInfo(x, z, face));
            float max = float.PositiveInfinity;
            for (int i = 0; i < range.Count; ++i)
                max = Math.Min(RoomGeometry.VertexPositions[i + range.Start].Y, max);
            return max;
        }

        public BlockFaceShape GetFaceShape(int x, int z, BlockFace face)
        {
            switch (RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorInfo(x, z, face)).Count)
            {
                case 3:
                    return BlockFaceShape.Triangle;
                case 6:
                    return BlockFaceShape.Quad;
                default:
                    return BlockFaceShape.Unknown;
            }
        }

        public void BuildGeometry()
        {
            RoomGeometry = new RoomGeometry(this);
        }

        public Matrix4x4 Transform => Matrix4x4.CreateTranslation(WorldPos);

        public int GetHighestCorner(RectangleInt2 area)
        {
            area = area.Intersect(LocalArea);

            int max = int.MinValue;
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!Blocks[x, z].IsAnyWall)
                        max = Math.Max(max, Blocks[x, z].Ceiling.Max);
            return (max == int.MinValue ? DefaultHeight : max);
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
                        min = Math.Min(min, Blocks[x, z].Floor.Min);
            return (min == int.MaxValue ? 0 : min);
        }

        public int GetLowestCorner()
        {
            return GetLowestCorner(new RectangleInt2(1, 1, NumXSectors - 2, NumZSectors - 2));
        }

        public VectorInt3 WorldPos
        {
            get { return new VectorInt3(Position.X * 1024, Position.Y * 256, Position.Z * 1024); }
            set { Position = new VectorInt3(value.X / 1024, value.Y / 256, value.Z / 1024); }
        }

        public Vector3 GetLocalCenter()
        {
            float ceilingHeight = GetHighestCorner();
            float floorHeight = GetLowestCorner();
            return new Vector3(
                NumXSectors * (0.5f * 1024.0f),
                (floorHeight + ceilingHeight) * (0.5f * 256.0f),
                NumZSectors * (0.5f * 1024.0f));
        }

        public int NumXSectors
        {
            get { return Blocks.GetLength(0); }
        }

        public int NumZSectors
        {
            get { return Blocks.GetLength(1); }
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
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                            lowest = Math.Min(lowest, b.Floor.GetHeight(edge));
                }

            // Move room to new position
            Position += new VectorInt3(0, lowest, 0);

            // Transform room content in such a way, their world position is identical to before even though the room position changed
            for (int z = 0; z < NumZSectors; z++)
                for (int x = 0; x < NumXSectors; x++)
                {
                    var b = Blocks[x, z];
                    for (BlockVertical vertical = 0; vertical < BlockVertical.Count; ++vertical)
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                            b.SetHeight(vertical, edge, b.GetHeight(vertical, edge) - lowest);
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

        private bool AddObjectAndSingularPortalCutSectors(Level level, RectangleInt2 newArea, SectorBasedObjectInstance instance)
        {
            // Determine area
            RectangleInt2 instanceNewAreaConstraint = instance.GetValidArea(newArea);
            if (!instance.Area.Intersects(instanceNewAreaConstraint))
                return false;
            RectangleInt2 instanceNewArea = instance.Area.Intersect(instanceNewAreaConstraint) - newArea.Start;

            // Add object
            AddObjectAndSingularPortal(level, instance.Clone(instanceNewArea));
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
                    if (AlternateOpposite != null)
                        addedObjects.Add(AlternateOpposite.AddObjectAndSingularPortal(level, (PortalInstance)portal.Clone()));

                    addedObjects.Add(portal.AdjoiningRoom.AddObjectAndSingularPortal(level, oppositePortal));
                    if (portal.AdjoiningRoom.AlternateOpposite != null)
                        addedObjects.Add(portal.AdjoiningRoom.AlternateOpposite.AddObjectAndSingularPortal(level, (PortalInstance)oppositePortal.Clone()));
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
            return new[] { instance };
        }

        public IEnumerable<ObjectInstance> RemoveObjectAndKeepAlive(Level level, ObjectInstance instance)
        {
            List<ObjectInstance> result = new List<ObjectInstance>();
            result.Add(RemoveObjectAndSingularPortalAndKeepAlive(level, instance));

            // Delete the corresponding other portals if necessary.
            var portal = instance as PortalInstance;
            if (portal != null)
            {
                var alternatePortal = portal.FindAlternatePortal(this.AlternateOpposite);
                if (alternatePortal != null)
                    result.Add(AlternateOpposite.RemoveObjectAndSingularPortal(level, alternatePortal));

                var oppositePortal = portal.FindOppositePortal(this);
                if (oppositePortal != null)
                    result.Add(portal.AdjoiningRoom.RemoveObjectAndSingularPortal(level, oppositePortal));

                var oppositeAlternatePortal = oppositePortal?.FindAlternatePortal(portal.AdjoiningRoom?.AlternateOpposite);
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

        // Fixes portals in neighboring rooms to match those in the current room.
        // This will preserve properties of the portals, i.e. the opacity.
        public static void FixupNeighborPortals(Level level, IEnumerable<Room> newRooms_, IEnumerable<Room> oldRooms_, ref HashSet<Room> relevantRooms)
        {
            // Required base room
            List<Room> newRooms = newRooms_.Select(room => room.AlternateBaseRoom ?? room).ToList();
            List<Room> oldRooms = oldRooms_.Select(room => room.AlternateBaseRoom ?? room).ToList();

            // Add other certainly relevant rooms
            foreach (Room room in newRooms)
                relevantRooms.Add(room);
            foreach (var relevantRoom in relevantRooms.ToArray())
                if (relevantRoom.AlternateOpposite != null)
                    relevantRooms.Add(relevantRoom.AlternateOpposite);

            // Remove old portals from those rooms
            var oldPortals = new List<KeyValuePair<Room, PortalInstance>>();
            foreach (Room relevantRoom in relevantRooms)
                if (!newRooms.Contains(relevantRoom) && !newRooms.Contains(relevantRoom.AlternateOpposite))
                    foreach (PortalInstance portal in relevantRoom.Portals.ToList())
                        if (oldRooms.Contains(portal.AdjoiningRoom))
                        {
                            relevantRoom.RemoveObjectAndSingularPortalAndKeepAlive(level, portal);
                            oldPortals.Add(new KeyValuePair<Room, PortalInstance>(relevantRoom, portal));
                        }

            // A helper function
            Func<Room, Room, PortalInstance, PortalInstance> findBestMatch = (room, roomForWhichToFindMatch, portal) =>
            {
                RectangleInt2 oppositeArea = PortalInstance.GetOppositePortalArea(portal.Direction, portal.Area) + room.SectorPos;
                PortalDirection oppositeDirection = PortalInstance.GetOppositeDirection(portal.Direction);

                // Find portal that is closest to the needed spot
                int bestMatchArea = 0;
                PortalInstance bestMatch = null;
                foreach (var oldPortal in oldPortals)
                    if (oldPortal.Key == roomForWhichToFindMatch &&
                        oldPortal.Value.Direction == oppositeDirection &&
                        oldPortal.Value.Area.Intersects(oppositeArea - oldPortal.Key.SectorPos))
                    {
                        RectangleInt2 area = oldPortal.Value.Area.Intersect(oppositeArea - oldPortal.Key.SectorPos);
                        int newMatchArea = (area.Width + 1) * (area.Height + 1);
                        if (newMatchArea > bestMatchArea)
                        {
                            bestMatchArea = newMatchArea;
                            bestMatch = oldPortal.Value;
                        }
                    }
                if (bestMatch != null)
                {
                    bestMatch.AdjoiningRoom = room;
                    return (PortalInstance)bestMatch.Clone(oppositeArea - portal.AdjoiningRoom.SectorPos);
                }
                return new PortalInstance(oppositeArea - portal.AdjoiningRoom.SectorPos, PortalInstance.GetOppositeDirection(portal.Direction), room);
            };

            // Add new portals where necessary
            foreach (Room room in newRooms)
                foreach (PortalInstance portal in room.Portals)
                    if (!newRooms.Contains(portal.AdjoiningRoom))
                    {
                        relevantRooms.Add(portal.AdjoiningRoom);
                        portal.AdjoiningRoom.AddObjectAndSingularPortal(level, findBestMatch(room, portal.AdjoiningRoom, portal));
                        if (portal.AdjoiningRoom.AlternateOpposite != null)
                        {
                            relevantRooms.Add(portal.AdjoiningRoom.AlternateOpposite);
                            portal.AdjoiningRoom.AlternateOpposite.AddObjectAndSingularPortal(level, findBestMatch(room, portal.AdjoiningRoom.AlternateOpposite, portal));
                        }
                    }

            // Cleanup
            foreach (var oldPortal in oldPortals)
                oldPortal.Value.Delete();
        }

        public enum RoomConnectionType
        {
            NoPortal,
            TriangularPortalXnZn,
            TriangularPortalXpZn,
            TriangularPortalXnZp,
            TriangularPortalXpZp,
            FullPortal
        }

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
        }

        public static IEnumerable<KeyValuePair<Room, Room>> GetPossibleAlternateRoomPairs(Room firstRoom, Room secondRoom, bool lookingFromSecond = false)
        {
            if (firstRoom.AlternateOpposite == null)
            {
                if (secondRoom.AlternateOpposite == null)
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                else
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom.AlternateOpposite);
                }
            }
            else
            {
                if (secondRoom.AlternateOpposite == null)
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateOpposite, secondRoom);
                }
                else if (firstRoom.AlternateGroup == secondRoom.AlternateGroup)
                {
                    bool isAlternateCurrently = lookingFromSecond ? secondRoom.AlternateBaseRoom != null : firstRoom.AlternateBaseRoom != null;
                    if (isAlternateCurrently)
                        yield return new KeyValuePair<Room, Room>(firstRoom.AlternateRoom ?? firstRoom, secondRoom.AlternateRoom ?? secondRoom);
                    else
                        yield return new KeyValuePair<Room, Room>(firstRoom.AlternateBaseRoom ?? firstRoom, secondRoom.AlternateBaseRoom ?? secondRoom);
                }
                else
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateOpposite, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom.AlternateOpposite);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateOpposite, secondRoom.AlternateOpposite);
                }
            }
        }

        public static RoomConnectionType CalculateRoomConnectionTypeWithoutAlternates(Room roomBelow, Room roomAbove, Block blockBelow, Block blockAbove)
        {
            // Evaluate force floor solid
            if (blockAbove.ForceFloorSolid)
                return RoomConnectionType.NoPortal;

            // Check walls
            DiagonalSplit aboveDiagonalSplit = blockAbove.Floor.DiagonalSplit;
            DiagonalSplit belowDiagonalSplit = blockBelow.Ceiling.DiagonalSplit;
            if (blockAbove.IsAnyWall && aboveDiagonalSplit == DiagonalSplit.None)
                return RoomConnectionType.NoPortal;
            if (blockBelow.IsAnyWall && belowDiagonalSplit == DiagonalSplit.None)
                return RoomConnectionType.NoPortal;
            if (blockBelow.IsAnyWall && blockAbove.IsAnyWall && aboveDiagonalSplit != belowDiagonalSplit)
                return RoomConnectionType.NoPortal;

            // Gather split data
            bool belowSplitXEqualsZ = blockBelow.Ceiling.SplitDirectionIsXEqualsZWithDiagonalSplit;
            bool aboveSplitXEqualsZ = blockAbove.Floor.SplitDirectionIsXEqualsZWithDiagonalSplit;
            bool belowIsQuad = blockBelow.Ceiling.IsQuad;
            bool aboveIsQuad = blockAbove.Floor.IsQuad;

            // Check where the geometry matches to create a portal
            if (belowIsQuad || aboveIsQuad || belowSplitXEqualsZ == aboveSplitXEqualsZ)
            {
                DiagonalSplit diagonalSplit = aboveDiagonalSplit != DiagonalSplit.None ? aboveDiagonalSplit : belowDiagonalSplit;
                bool splitXEqualsZ = belowIsQuad ? aboveSplitXEqualsZ : belowSplitXEqualsZ;

                bool matchesAtXnYn = roomBelow.Position.Y + blockBelow.Ceiling.GetActualMax(BlockEdge.XnZn) == roomAbove.Position.Y + blockAbove.Floor.GetActualMin(BlockEdge.XnZn);
                bool matchesAtXpYn = roomBelow.Position.Y + blockBelow.Ceiling.GetActualMax(BlockEdge.XpZn) == roomAbove.Position.Y + blockAbove.Floor.GetActualMin(BlockEdge.XpZn);
                bool matchesAtXnYp = roomBelow.Position.Y + blockBelow.Ceiling.GetActualMax(BlockEdge.XnZp) == roomAbove.Position.Y + blockAbove.Floor.GetActualMin(BlockEdge.XnZp);
                bool matchesAtXpYp = roomBelow.Position.Y + blockBelow.Ceiling.GetActualMax(BlockEdge.XpZp) == roomAbove.Position.Y + blockAbove.Floor.GetActualMin(BlockEdge.XpZp);

                if (matchesAtXnYn && matchesAtXpYn && matchesAtXnYp && matchesAtXpYp && !(blockAbove.IsAnyWall || blockBelow.IsAnyWall))
                    return RoomConnectionType.FullPortal;
                if ((belowIsQuad || belowSplitXEqualsZ) && (aboveIsQuad || splitXEqualsZ))
                { // Try to make a triangular portal split which is parallel to X=Z
                    if (matchesAtXnYn && matchesAtXnYp && matchesAtXpYp && (diagonalSplit == DiagonalSplit.XpZn || !(blockAbove.IsAnyWall || blockBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXnZp;
                    if (matchesAtXnYn && matchesAtXpYn && matchesAtXpYp && (diagonalSplit == DiagonalSplit.XnZp || !(blockAbove.IsAnyWall || blockBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXpZn;
                }
                if ((belowIsQuad || !belowSplitXEqualsZ) && (aboveIsQuad || !splitXEqualsZ))
                { // Try to make a triangular portal split which is perpendicular to X=Z
                    if (matchesAtXpYn && matchesAtXnYp && matchesAtXpYp && (diagonalSplit == DiagonalSplit.XnZn || !(blockAbove.IsAnyWall || blockBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXpZp;
                    if (matchesAtXnYn && matchesAtXpYn && matchesAtXnYp && (diagonalSplit == DiagonalSplit.XpZp || !(blockAbove.IsAnyWall || blockBelow.IsAnyWall)))
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
            Parallel.For(0, roomsToProcess.Count, index =>
                {
                    roomsToProcess[index].BuildGeometry();
                });
        }

        public class CopyDependentLevelSettingsArgs
        {
            public LevelSettings DestinationLevelSettings { get; }
            public LevelSettings SourceLevelSettings { get; }
            public bool UnifyData { get; }
            private Dictionary<ushort, ushort> FlyBySequenceReassociation { get; } = new Dictionary<ushort, ushort>();
            private readonly HashSet<ushort> UsedFlyBySequences = new HashSet<ushort>();
            public CopyDependentLevelSettingsArgs(IEnumerable<ObjectInstance> oldInstances, LevelSettings destinationLevelSettings, LevelSettings sourceLevelSettings, bool unifyData)
            {
                DestinationLevelSettings = destinationLevelSettings;
                SourceLevelSettings = sourceLevelSettings;
                UnifyData = unifyData;

                if (oldInstances != null)
                    foreach (var flyBys in oldInstances.OfType<FlybyCameraInstance>())
                        UsedFlyBySequences.Add(flyBys.Sequence);
            }

            public ushort ReassociateFlyBySequence(ushort sequenceId)
            {
                // Is that sequence already assigned to a new sequence Id?
                ushort result;
                if (FlyBySequenceReassociation.TryGetValue(sequenceId, out result))
                    return result;

                // Find unused sequence ID
                if (!UsedFlyBySequences.Contains(sequenceId))
                    result = sequenceId;
                else
                {
                    result = 0;
                    while (UsedFlyBySequences.Contains(result))
                        result = checked((ushort)(result + 1));
                }

                // Assign and return ID
                UsedFlyBySequences.Add(result);
                FlyBySequenceReassociation.Add(sequenceId, result);
                return result;
            }
        }

        public void CopyDependentLevelSettings(CopyDependentLevelSettingsArgs args)
        {
            foreach (ObjectInstance instance in AnyObjects)
                instance.CopyDependentLevelSettings(args);

            if (args.DestinationLevelSettings.Textures.Count == 0)
            {
                args.DestinationLevelSettings.Textures.AddRange(args.SourceLevelSettings.Textures);
                args.DestinationLevelSettings.AnimatedTextureSets.AddRange(args.SourceLevelSettings.AnimatedTextureSets);
            }
            else if (args.UnifyData)
            {
                bool LookForAnimatedSets = false;
                Cache<LevelTexture, LevelTexture> TextureRemap = null;
                TextureRemap = new Cache<LevelTexture, LevelTexture>(32,
                    sourceTexture =>
                    {
                        foreach (LevelTexture destinationTexture in args.DestinationLevelSettings.Textures)
                            if (destinationTexture == sourceTexture || destinationTexture.Equals(sourceTexture))
                                return destinationTexture;

                        // Copy the texture
                        args.DestinationLevelSettings.Textures.Add(sourceTexture);
                        LookForAnimatedSets = true;
                        return sourceTexture;
                    });

                for (int z = 0; z < NumZSectors; ++z)
                    for (int x = 0; x < NumXSectors; ++x)
                    {
                        Block block = Blocks[x, z];
                        for (BlockFace face = 0; face < BlockFace.Count; ++face)
                        {
                            TextureArea textureArea = block.GetFaceTexture(face);
                            var sourceTexture = textureArea.Texture as LevelTexture;
                            if (sourceTexture != null)
                            {
                                textureArea.Texture = TextureRemap[sourceTexture]; // Remap source texture to destination texture.
                                if (LookForAnimatedSets)
                                {
                                    // Copy animated texture sets
                                    var setsToCopy = args.SourceLevelSettings.AnimatedTextureSets.Where(set => set.Frames.Any(frame => frame.Texture == sourceTexture));
                                    foreach (AnimatedTextureSet setToCopy in setsToCopy)
                                    {
                                        AnimatedTextureSet newSet = setToCopy.Clone();
                                        foreach (AnimatedTextureFrame frame in newSet.Frames)
                                            frame.Texture = TextureRemap[frame.Texture]; // Remap source textures to destination textures.
                                        args.DestinationLevelSettings.AnimatedTextureSets.Add(newSet);
                                    }
                                    LookForAnimatedSets = false;
                                }
                                block.SetFaceTexture(face, textureArea);
                            }
                        }
                    }
            }
        }

        public HashSet<Texture> GetTextures()
        {
            HashSet<Texture> result = new HashSet<Texture>();
            for (int z = 0; z < NumZSectors; ++z)
                for (int x = 0; x < NumXSectors; ++x)
                {
                    Block block = Blocks[x, z];
                    for (BlockFace face = 0; face < BlockFace.Count; ++face)
                        result.Add(block.GetFaceTexture(face).Texture);
                }
            return result;
        }

        bool IEquatable<ITriggerParameter>.Equals(ITriggerParameter other) => this == other;

        /* public bool HasExternalRoomGeometry
         {
            / get
             {
                 foreach (var obj in Objects)
                     if (obj is ImportedGeometryInstance)
                     {
                         var imported = obj as ImportedGeometryInstance;
                         if (imported.Model.)
                     }
             }
         }*/
    }
}
