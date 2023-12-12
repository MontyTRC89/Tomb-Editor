using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum BlockType : byte
    {
        Floor, Wall, BorderWall
    }

    [Flags]
    public enum BlockFlags : short
    {
        None = 0,
        Monkey = 1,
        Box = 2,
        DeathFire = 4,
        DeathLava = 8,
        DeathElectricity = 16,
        Beetle = 32,
        TriggerTriggerer = 64,
        NotWalkableFloor = 128,
        ClimbPositiveZ = 256,
        ClimbNegativeZ = 512,
        ClimbPositiveX = 1024,
        ClimbNegativeX = 2048,
        ClimbAny = ClimbPositiveZ | ClimbNegativeZ | ClimbPositiveX | ClimbNegativeX
    }

    public enum DiagonalSplit : byte
    {
        None = 0,
        XnZp = 1,
        XpZp = 2,
        XpZn = 3,
        XnZn = 4
    }

    public enum DiagonalType : byte
    {
        None = 0,
        XnZnToXpZp = 1,
        XnZpToXpZn = 2
    }

    public enum BlockEdge : byte
    {
        /// <summary> Index of edges on the negative X and positive Z direction </summary>
        XnZp,
        /// <summary> Index of edges on the positive X and positive Z direction </summary>
        XpZp,
        /// <summary> Index of edges on the positive X and negative Z direction </summary>
        XpZn,
        /// <summary> Index of edges on the negative X and negative Z direction </summary>
        XnZn,

        Count
    }

    public static class BlockEdgeExtensions
    {
        public static int DirectionX(this BlockEdge edge) => (edge == BlockEdge.XpZn || edge == BlockEdge.XpZp) ? 1 : 0;
        public static int DirectionZ(this BlockEdge edge) => (edge == BlockEdge.XnZp || edge == BlockEdge.XpZp) ? 1 : 0;
    }

    public enum BlockVertical : byte
    {
        Floor, // FloorSubdivision1
        Ceiling, // CeilingSubdivision1
        FloorSubdivision2,
        CeilingSubdivision2,

        FloorSubdivision3,
        CeilingSubdivision3,

        FloorSubdivision4,
        CeilingSubdivision4,

        FloorSubdivision5,
        CeilingSubdivision5,

        FloorSubdivision6,
        CeilingSubdivision6,

        FloorSubdivision7,
        CeilingSubdivision7,

        FloorSubdivision8,
        CeilingSubdivision8,

        FloorSubdivision9,
        CeilingSubdivision9
    }

    public static class BlockVerticalExtensions
    {
        public static bool IsOnFloor(this BlockVertical vertical)
            => vertical is BlockVertical.Floor || vertical.IsExtraFloorSubdivision();

        public static bool IsOnCeiling(this BlockVertical vertical)
            => vertical is BlockVertical.Ceiling || vertical.IsExtraCeilingSubdivision();

        public static bool IsExtraFloorSubdivision(this BlockVertical vertical)
            => vertical.ToString().StartsWith("FloorSubdivision");

        public static bool IsExtraCeilingSubdivision(this BlockVertical vertical)
            => vertical.ToString().StartsWith("CeilingSubdivision");

        public static bool IsExtraSubdivision(this BlockVertical vertical)
           => vertical.ToString().Contains("Subdivision");

        public static BlockVertical GetExtraFloorSubdivision(int subdivisionIndex)
        {
            string enumName = $"FloorSubdivision{subdivisionIndex + 2}";
            return Enum.Parse<BlockVertical>(enumName);
        }

        public static BlockVertical GetExtraCeilingSubdivision(int subdivisionIndex)
        {
            string enumName = $"CeilingSubdivision{subdivisionIndex + 2}";
            return Enum.Parse<BlockVertical>(enumName);
        }

        public static int GetExtraSubdivisionIndex(this BlockVertical vertical)
        {
            if (vertical.IsExtraFloorSubdivision())
                return int.Parse(vertical.ToString()["FloorSubdivision".Length..]) - 2;
            else if (vertical.IsExtraCeilingSubdivision())
                return int.Parse(vertical.ToString()["CeilingSubdivision".Length..]) - 2;
            else
                return -1;
        }
    }

    public enum BlockFace : byte
    {
        Wall_PositiveZ_QA = 0, //
        Wall_NegativeZ_QA = 1, //
        Wall_NegativeX_QA = 2, // FloorSubdivision1
        Wall_PositiveX_QA = 3, //
        Wall_Diagonal_QA = 4,  //

        Wall_PositiveZ_FloorSubdivision2 = 5,
        Wall_NegativeZ_FloorSubdivision2 = 6,
        Wall_NegativeX_FloorSubdivision2 = 7,
        Wall_PositiveX_FloorSubdivision2 = 8,
        Wall_Diagonal_FloorSubdivision2 = 9,

        Wall_PositiveZ_Middle = 10,
        Wall_NegativeZ_Middle = 11,
        Wall_NegativeX_Middle = 12,
        Wall_PositiveX_Middle = 13,
        Wall_Diagonal_Middle = 14,

        Wall_PositiveZ_WS = 15, //
        Wall_NegativeZ_WS = 16, //
        Wall_NegativeX_WS = 17, // CeilingSubdivision1
        Wall_PositiveX_WS = 18, //
        Wall_Diagonal_WS = 19,  //

        Wall_PositiveZ_CeilingSubdivision2 = 20,
        Wall_NegativeZ_CeilingSubdivision2 = 21,
        Wall_NegativeX_CeilingSubdivision2 = 22,
        Wall_PositiveX_CeilingSubdivision2 = 23,
        Wall_Diagonal_CeilingSubdivision2 = 24,

        Floor = 25,
        Floor_Triangle2 = 26,
        Ceiling = 27,
        Ceiling_Triangle2 = 28,

        Wall_PositiveZ_FloorSubdivision3 = 29,
        Wall_NegativeZ_FloorSubdivision3 = 30,
        Wall_NegativeX_FloorSubdivision3 = 31,
        Wall_PositiveX_FloorSubdivision3 = 32,
        Wall_Diagonal_FloorSubdivision3 = 33,

        Wall_PositiveZ_CeilingSubdivision3 = 34,
        Wall_NegativeZ_CeilingSubdivision3 = 35,
        Wall_NegativeX_CeilingSubdivision3 = 36,
        Wall_PositiveX_CeilingSubdivision3 = 37,
        Wall_Diagonal_CeilingSubdivision3 = 38,

        Wall_PositiveZ_FloorSubdivision4 = 39,
        Wall_NegativeZ_FloorSubdivision4 = 40,
        Wall_NegativeX_FloorSubdivision4 = 41,
        Wall_PositiveX_FloorSubdivision4 = 42,
        Wall_Diagonal_FloorSubdivision4 = 43,

        Wall_PositiveZ_CeilingSubdivision4 = 44,
        Wall_NegativeZ_CeilingSubdivision4 = 45,
        Wall_NegativeX_CeilingSubdivision4 = 46,
        Wall_PositiveX_CeilingSubdivision4 = 47,
        Wall_Diagonal_CeilingSubdivision4 = 48,

        Wall_PositiveZ_FloorSubdivision5 = 49,
        Wall_NegativeZ_FloorSubdivision5 = 50,
        Wall_NegativeX_FloorSubdivision5 = 51,
        Wall_PositiveX_FloorSubdivision5 = 52,
        Wall_Diagonal_FloorSubdivision5 = 53,

        Wall_PositiveZ_CeilingSubdivision5 = 54,
        Wall_NegativeZ_CeilingSubdivision5 = 55,
        Wall_NegativeX_CeilingSubdivision5 = 56,
        Wall_PositiveX_CeilingSubdivision5 = 57,
        Wall_Diagonal_CeilingSubdivision5 = 58,

        Wall_PositiveZ_FloorSubdivision6 = 59,
        Wall_NegativeZ_FloorSubdivision6 = 60,
        Wall_NegativeX_FloorSubdivision6 = 61,
        Wall_PositiveX_FloorSubdivision6 = 62,
        Wall_Diagonal_FloorSubdivision6 = 63,

        Wall_PositiveZ_CeilingSubdivision6 = 64,
        Wall_NegativeZ_CeilingSubdivision6 = 65,
        Wall_NegativeX_CeilingSubdivision6 = 66,
        Wall_PositiveX_CeilingSubdivision6 = 67,
        Wall_Diagonal_CeilingSubdivision6 = 68,

        Wall_PositiveZ_FloorSubdivision7 = 69,
        Wall_NegativeZ_FloorSubdivision7 = 70,
        Wall_NegativeX_FloorSubdivision7 = 71,
        Wall_PositiveX_FloorSubdivision7 = 72,
        Wall_Diagonal_FloorSubdivision7 = 73,

        Wall_PositiveZ_CeilingSubdivision7 = 74,
        Wall_NegativeZ_CeilingSubdivision7 = 75,
        Wall_NegativeX_CeilingSubdivision7 = 76,
        Wall_PositiveX_CeilingSubdivision7 = 77,
        Wall_Diagonal_CeilingSubdivision7 = 78,

        Wall_PositiveZ_FloorSubdivision8 = 79,
        Wall_NegativeZ_FloorSubdivision8 = 80,
        Wall_NegativeX_FloorSubdivision8 = 81,
        Wall_PositiveX_FloorSubdivision8 = 82,
        Wall_Diagonal_FloorSubdivision8 = 83,

        Wall_PositiveZ_CeilingSubdivision8 = 84,
        Wall_NegativeZ_CeilingSubdivision8 = 85,
        Wall_NegativeX_CeilingSubdivision8 = 86,
        Wall_PositiveX_CeilingSubdivision8 = 87,
        Wall_Diagonal_CeilingSubdivision8 = 88,

        Wall_PositiveZ_FloorSubdivision9 = 89,
        Wall_NegativeZ_FloorSubdivision9 = 90,
        Wall_NegativeX_FloorSubdivision9 = 91,
        Wall_PositiveX_FloorSubdivision9 = 92,
        Wall_Diagonal_FloorSubdivision9 = 93,

        Wall_PositiveZ_CeilingSubdivision9 = 94,
        Wall_NegativeZ_CeilingSubdivision9 = 95,
        Wall_NegativeX_CeilingSubdivision9 = 96,
        Wall_PositiveX_CeilingSubdivision9 = 97,
        Wall_Diagonal_CeilingSubdivision9 = 98,

        Count
    }

    public enum BlockFaceType
    {
        Floor, Ceiling, Wall
    }

    public enum BlockFaceShape
    {
        Quad, Triangle, Unknown
    }

    public static class BlockFaceExtensions
    {
        public static BlockVertical GetVertical(this BlockFace face)
        {
            string enumName = face.ToString();
            string verticalName = enumName.Split('_').ElementAtOrDefault(2);

            if (verticalName == "QA")
                verticalName = "Floor";

            if (verticalName == "WS")
                verticalName = "Ceiling";

            if (Enum.TryParse(verticalName, out BlockVertical vertical))
                return vertical;
            else
                throw new ArgumentException();
        }

        public static BlockFaceType GetFaceType(this BlockFace face)
        {
            if (face <= BlockFace.Wall_Diagonal_FloorSubdivision2 || face.IsExtraFloorSubdivision())
                return BlockFaceType.Floor;
            else if (face is >= BlockFace.Wall_PositiveZ_WS and <= BlockFace.Wall_Diagonal_CeilingSubdivision2 || face.IsExtraCeilingSubdivision())
                return BlockFaceType.Ceiling;
            else if (face is >= BlockFace.Wall_PositiveZ_Middle and <= BlockFace.Wall_Diagonal_Middle)
                return BlockFaceType.Wall;
            else
                throw new ArgumentException();
        }

        public static Direction GetDirection(this BlockFace face)
        {
            string enumName = face.ToString();
            string directionName = enumName.Split('_').ElementAtOrDefault(1);

            if (Enum.TryParse(directionName, out Direction direction))
                return direction;
            else
                return Direction.None;
        }

        public static IEnumerable<BlockFace> GetWalls()
            => Enum.GetValues<BlockFace>().Where(face => face.IsWall());

        public static bool IsWall(this BlockFace face)
            => face.ToString().StartsWith("Wall");

        public static bool IsNonWall(this BlockFace face)
            => face is >= BlockFace.Floor and <= BlockFace.Ceiling_Triangle2;

        public static bool IsNonDiagonalWall(this BlockFace face)
            => face.ToString().StartsWith("Wall") && !face.ToString().Contains("Diagonal");

        public static bool IsPositiveX(this BlockFace face)
            => face.ToString().Contains("PositiveX");

        public static bool IsNegativeX(this BlockFace face)
            => face.ToString().Contains("NegativeX");

        public static bool IsPositiveZ(this BlockFace face)
            => face.ToString().Contains("PositiveZ");

        public static bool IsNegativeZ(this BlockFace face)
            => face.ToString().Contains("NegativeZ");

        public static bool IsDiagonal(this BlockFace face)
            => face.ToString().Contains("Diagonal");

        public static bool IsFloorWall(this BlockFace face)
            => face <= BlockFace.Wall_Diagonal_FloorSubdivision2 || face.IsExtraFloorSubdivision();

        public static bool IsCeilingWall(this BlockFace face)
            => face is >= BlockFace.Wall_PositiveZ_WS and <= BlockFace.Wall_Diagonal_CeilingSubdivision2 || face.IsExtraCeilingSubdivision();

        public static bool IsMiddleWall(this BlockFace face)
            => face is >= BlockFace.Wall_PositiveZ_Middle and <= BlockFace.Wall_Diagonal_Middle;

        public static bool IsFloor(this BlockFace face)
            => face is BlockFace.Floor or BlockFace.Floor_Triangle2;

        public static bool IsCeiling(this BlockFace face)
            => face is BlockFace.Ceiling or BlockFace.Ceiling_Triangle2;

        public static bool IsExtraFloorSubdivision(this BlockFace face)
            => face.ToString().Contains("FloorSubdivision");

        public static bool IsExtraCeilingSubdivision(this BlockFace face)
            => face.ToString().Contains("CeilingSubdivision");

        public static bool IsSpecificFloorSubdivision(this BlockFace face, Direction direction)
            => face.ToString().Contains($"{direction}_FloorSubdivision");

        public static bool IsSpecificCeilingSubdivision(this BlockFace face, Direction direction)
            => face.ToString().Contains($"{direction}_CeilingSubdivision");

        public static BlockFace GetExtraFloorSubdivisionFace(Direction direction, int subdivisionIndex)
        {
            string enumName = $"Wall_{direction}_FloorSubdivision{subdivisionIndex + 2}";
            return Enum.Parse<BlockFace>(enumName);
        }

        public static BlockFace GetExtraCeilingSubdivisionFace(Direction direction, int subdivisionIndex)
        {
            string enumName = $"Wall_{direction}_CeilingSubdivision{subdivisionIndex + 2}";
            return Enum.Parse<BlockFace>(enumName);
        }
    }

    public enum Direction : byte
    {
        None = 0, PositiveZ = 1, PositiveX = 2, NegativeZ = 3, NegativeX = 4, Diagonal = 5
    }

    public struct RoomBlockPair
    {
        public Room Room { get; set; }
        public Block Block { get; set; }
        public VectorInt2 Pos { get; set; }
    }

    [Serializable]
    public struct BlockSurface
    {
        public bool SplitDirectionToggled;
        public DiagonalSplit DiagonalSplit;
        public short XnZp;
        public short XpZp;
        public short XpZn;
        public short XnZn;

        public bool IsQuad => DiagonalSplit == DiagonalSplit.None && IsQuad2(XnZp, XpZp, XpZn, XnZn);
        public bool HasSlope => Max - Min > 2;
        public int IfQuadSlopeX => IsQuad ? XpZp - XnZp : 0;
        public int IfQuadSlopeZ => IsQuad ? XpZp - XpZn : 0;
        public short Max => Math.Max(Math.Max(XnZp, XpZp), Math.Max(XpZn, XnZn));
        public short Min => Math.Min(Math.Min(XnZp, XpZp), Math.Min(XpZn, XnZn));

        public short GetHeight(BlockEdge edge)
        {
            switch (edge)
            {
                case BlockEdge.XnZp:
                    return XnZp;
                case BlockEdge.XpZp:
                    return XpZp;
                case BlockEdge.XpZn:
                    return XpZn;
                case BlockEdge.XnZn:
                    return XnZn;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetHeight(BlockEdge edge, int value)
        {
            switch (edge)
            {
                case BlockEdge.XnZp:
                    XnZp = checked((short)value);
                    return;
                case BlockEdge.XpZp:
                    XpZp = checked((short)value);
                    return;
                case BlockEdge.XpZn:
                    XpZn = checked((short)value);
                    return;
                case BlockEdge.XnZn:
                    XnZn = checked((short)value);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetHeight(int value)
        {
            SetHeight(BlockEdge.XnZn, value);
            SetHeight(BlockEdge.XnZp, value);
            SetHeight(BlockEdge.XpZn, value);
            SetHeight(BlockEdge.XpZp, value);
        }

        public static bool IsQuad2(int hXnZp, int hXpZp, int hXpZn, int hXnZn)
        {
            return hXpZp - hXpZn == hXnZp - hXnZn &&
                hXpZp - hXnZp == hXpZn - hXnZn;
        }

        public bool SplitDirectionIsXEqualsZ
        {
            get
            {
                var p1 = new Vector3(0, XnZp, 0);
                var p2 = new Vector3(1, XpZp, 0);
                var p3 = new Vector3(1, XpZn, 1);
                var p4 = new Vector3(0, XnZn, 1);

                var plane = Plane.CreateFromVertices(p1, p2, p4);
                if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                    return !SplitDirectionToggled;

                plane = Plane.CreateFromVertices(p1, p2, p3);
                if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                    return SplitDirectionToggled;

                plane = Plane.CreateFromVertices(p2, p3, p4);
                if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                    return !SplitDirectionToggled;

                plane = Plane.CreateFromVertices(p3, p4, p1);
                if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                    return SplitDirectionToggled;

                // Otherwise
                int min = Math.Min(Math.Min(Math.Min(XnZp, XpZp), XpZn), XnZn);
                int max = Math.Max(Math.Max(Math.Max(XnZp, XpZp), XpZn), XnZn);

                if (XnZp == XpZn && XpZp == XnZn && XpZp != XpZn)
                    return SplitDirectionToggled;

                if (min == XnZp && min == XpZn)
                    return SplitDirectionToggled;
                if (min == XpZp && min == XnZn)
                    return !SplitDirectionToggled;

                if (min == XnZp && max == XpZn)
                    return !SplitDirectionToggled;
                if (min == XpZp && max == XnZn)
                    return SplitDirectionToggled;
                if (min == XpZn && max == XnZp)
                    return !SplitDirectionToggled;
                if (min == XnZn && max == XpZp)
                    return SplitDirectionToggled;

                return !SplitDirectionToggled;
            }
            set
            {
                if (value != SplitDirectionIsXEqualsZ)
                    SplitDirectionToggled = !SplitDirectionToggled;
            }
        }

        /// <summary>Checks for DiagonalSplit and takes priority</summary>
        public bool SplitDirectionIsXEqualsZWithDiagonalSplit
        {
            get
            {
                switch (DiagonalSplit)
                {
                    case DiagonalSplit.XnZn:
                    case DiagonalSplit.XpZp:
                        return false;
                    case DiagonalSplit.XpZn:
                    case DiagonalSplit.XnZp:
                        return true;
                    case DiagonalSplit.None:
                        return SplitDirectionIsXEqualsZ;
                    default:
                        throw new ApplicationException("\"DiagonalSplit\" in unknown state.");
                }
            }
        }

        /// <summary>Returns the height of the 4 edges if the sector is split</summary>
        public int GetActualMax(BlockEdge edge)
        {
            switch (DiagonalSplit)
            {
                case DiagonalSplit.None:
                    return GetHeight(edge);
                case DiagonalSplit.XnZn:
                    if (edge == BlockEdge.XnZp || edge == BlockEdge.XpZn)
                        return Math.Max(GetHeight(edge), XpZp);
                    return GetHeight(edge);
                case DiagonalSplit.XpZp:
                    if (edge == BlockEdge.XnZp || edge == BlockEdge.XpZn)
                        return Math.Max(GetHeight(edge), XnZn);
                    return GetHeight(edge);
                case DiagonalSplit.XpZn:
                    if (edge == BlockEdge.XpZp || edge == BlockEdge.XnZn)
                        return Math.Max(GetHeight(edge), XnZp);
                    return GetHeight(edge);
                case DiagonalSplit.XnZp:
                    if (edge == BlockEdge.XpZp || edge == BlockEdge.XnZn)
                        return Math.Max(GetHeight(edge), XpZn);
                    return GetHeight(edge);
                default:
                    throw new ApplicationException("\"splitType\" in unknown state.");
            }
        }

        /// <summary>Returns the height of the 4 edges if the sector is split</summary>
        public int GetActualMin(BlockEdge edge)
        {
            switch (DiagonalSplit)
            {
                case DiagonalSplit.None:
                    return GetHeight(edge);
                case DiagonalSplit.XnZn:
                    if (edge == BlockEdge.XnZp || edge == BlockEdge.XpZn)
                        return Math.Min(GetHeight(edge), XpZp);
                    return GetHeight(edge);
                case DiagonalSplit.XpZp:
                    if (edge == BlockEdge.XnZp || edge == BlockEdge.XpZn)
                        return Math.Min(GetHeight(edge), XnZn);
                    return GetHeight(edge);
                case DiagonalSplit.XpZn:
                    if (edge == BlockEdge.XpZp || edge == BlockEdge.XnZn)
                        return Math.Min(GetHeight(edge), XnZp);
                    return GetHeight(edge);
                case DiagonalSplit.XnZp:
                    if (edge == BlockEdge.XpZp || edge == BlockEdge.XnZn)
                        return Math.Min(GetHeight(edge), XpZn);
                    return GetHeight(edge);
                default:
                    throw new ApplicationException("\"splitType\" in unknown state.");
            }
        }

        public static BlockSurface operator +(BlockSurface first, BlockSurface second) 
            => new BlockSurface() { XpZp = (short)(first.XpZp + second.XpZp), XpZn = (short)(first.XpZn + second.XpZn), XnZp = (short)(first.XnZp + second.XnZp), XnZn = (short)(first.XnZn + second.XnZn) };
        public static BlockSurface operator -(BlockSurface first, BlockSurface second)
            => new BlockSurface() { XpZp = (short)(first.XpZp - second.XpZp), XpZn = (short)(first.XpZn - second.XpZn), XnZp = (short)(first.XnZp - second.XnZp), XnZn = (short)(first.XnZn - second.XnZn) };

    }

    public class Subdivision : ICloneable
    {
        public short[] Edges { get; } = new short[4];

        public Subdivision()
        { }

        public Subdivision(short uniformEdgeY)
            => Edges[0] = Edges[1] = Edges[2] = Edges[3] = uniformEdgeY;

        public object Clone()
        {
            var result = new Subdivision();

            for (int i = 0; i < 4; i++)
                result.Edges[i] = Edges[i];

            return result;
        }
    }

    [Serializable]
    public class Block : ICloneable
    {
        public const int MAX_EXTRA_SUBDIVISIONS = 8;
        public static Block Empty { get; } = new Block();

        public BlockType Type { get; set; } = BlockType.Floor;
        public BlockFlags Flags { get; set; } = BlockFlags.None;
        public bool ForceFloorSolid { get; set; } // If this is set to true, portals are overwritten for this sector.
        public List<Subdivision> ExtraFloorSubdivisions { get; } = new();
        public List<Subdivision> ExtraCeilingSubdivisions { get; } = new();
        private Dictionary<BlockFace, TextureArea> _faceTextures { get; } = new();

        public BlockSurface Floor;
        public BlockSurface Ceiling;

        public List<TriggerInstance> Triggers { get; } = new List<TriggerInstance>(); // This array is not supposed to be modified here.
        public PortalInstance FloorPortal { get; internal set; } = null; // This is not supposed to be modified here.
        public PortalInstance WallPortal { get; internal set; } = null; // This is not supposed to be modified here.
        public PortalInstance CeilingPortal { get; internal set; } = null; // This is not supposed to be modified here.

        public GhostBlockInstance GhostBlock { get; internal set; } = null; // If exists, adds invisible geometry to sector.

        private Block()
        { }

        public Block(int floor, int ceiling)
        {
            Floor.XnZp = Floor.XpZp = Floor.XpZn = Floor.XnZn = (short)floor;
            Ceiling.XnZp = Ceiling.XpZp = Ceiling.XpZn = Ceiling.XnZn = (short)ceiling;
        }

        public Block Clone()
        {
            var result = new Block();
            result.Type = Type;
            result.Flags = Flags;
            result.ForceFloorSolid = ForceFloorSolid;
            foreach (KeyValuePair<BlockFace, TextureArea> entry in _faceTextures)
                result._faceTextures[entry.Key] = entry.Value;
            foreach (Subdivision subdivision in ExtraFloorSubdivisions)
                result.ExtraFloorSubdivisions.Add((Subdivision)subdivision.Clone());
            foreach (Subdivision subdivision in ExtraCeilingSubdivisions)
                result.ExtraCeilingSubdivisions.Add((Subdivision)subdivision.Clone());
            result.Floor = Floor;
            result.Ceiling = Ceiling;
            return result;
        }
        object ICloneable.Clone() => Clone();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasFlag(BlockFlags flag)
        {
            return (Flags & flag) == flag;
        }

        public bool IsFullWall => Type == BlockType.BorderWall || (Type == BlockType.Wall && Floor.DiagonalSplit == DiagonalSplit.None);
        public bool IsAnyWall => Type != BlockType.Floor;
        public bool IsAnyPortal => FloorPortal != null || CeilingPortal != null || WallPortal != null;
        public bool HasGhostBlock => GhostBlock != null;

        public bool SetFaceTexture(BlockFace face, TextureArea texture)
        {
            if (texture == TextureArea.None)
            {
                if (_faceTextures.ContainsKey(face))
                    _faceTextures.Remove(face);

                return true;
            }

            if (texture.TextureIsDegenerate)
                texture = TextureArea.Invisible;

            if (!_faceTextures.ContainsKey(face) || _faceTextures[face] != texture)
            {
                _faceTextures[face] = texture;
                return true;
            }
            else
                return false;
        }

        public TextureArea GetFaceTexture(BlockFace face)
        {
            return _faceTextures.GetValueOrDefault(face);
        }

        public Dictionary<BlockFace, TextureArea> GetFaceTextures()
        {
            return _faceTextures;
        }

        public IEnumerable<BlockVertical> GetVerticals()
        {
            yield return BlockVertical.Floor;
            yield return BlockVertical.Ceiling;

            for (int i = 0; i < ExtraFloorSubdivisions.Count; i++)
                yield return BlockVerticalExtensions.GetExtraFloorSubdivision(i);

            for (int i = 0; i < ExtraCeilingSubdivisions.Count; i++)
                yield return BlockVerticalExtensions.GetExtraCeilingSubdivision(i);
        }

        public IEnumerable<PortalInstance> Portals
        {
            get
            {
                if (WallPortal != null)
                    yield return WallPortal;
                if (CeilingPortal != null)
                    yield return CeilingPortal;
                if (FloorPortal != null)
                    yield return FloorPortal;
            }
        }

        public void ReplaceGeometry(Level level, Block replacement)
        {
            if (Type != BlockType.BorderWall) Type = replacement.Type;

            Flags = replacement.Flags;
            ForceFloorSolid = replacement.ForceFloorSolid;

            foreach (BlockFace face in replacement.GetFaceTextures().Keys.Union(_faceTextures.Keys))
            {
                var texture = replacement.GetFaceTexture(face);
                if (texture.TextureIsInvisible || level.Settings.Textures.Contains(texture.Texture))
                    SetFaceTexture(face, texture);
            }

            Floor = replacement.Floor;
            Ceiling = replacement.Ceiling;

            ExtraFloorSubdivisions.Clear();
            ExtraFloorSubdivisions.AddRange(replacement.ExtraFloorSubdivisions);

            ExtraCeilingSubdivisions.Clear();
            ExtraCeilingSubdivisions.AddRange(replacement.ExtraCeilingSubdivisions);
        }

        public short GetHeight(BlockVertical vertical, BlockEdge edge)
        {
            switch (vertical)
            {
                case BlockVertical.Floor:
                    return Floor.GetHeight(edge);
                case BlockVertical.Ceiling:
                    return Ceiling.GetHeight(edge);
            }

            if (vertical.IsExtraFloorSubdivision())
            {
                int index = vertical.GetExtraSubdivisionIndex();
                Subdivision subdivision = ExtraFloorSubdivisions.ElementAtOrDefault(index);

                if (subdivision == null)
                    return short.MinValue;

                return subdivision.Edges[(int)edge];
            }
            
            if (vertical.IsExtraCeilingSubdivision())
            {
                int index = vertical.GetExtraSubdivisionIndex();
                Subdivision subdivision = ExtraCeilingSubdivisions.ElementAtOrDefault(index);

                if (subdivision == null)
                    return short.MaxValue;

                return subdivision.Edges[(int)edge];
            }

            throw new ArgumentOutOfRangeException();
        }

        public void SetHeight(BlockVertical vertical, BlockEdge edge, int newValue)
        {
            if (newValue is short.MinValue or short.MaxValue)
                return;

            switch (vertical)
            {
                case BlockVertical.Floor:
                    Floor.SetHeight(edge, newValue);
                    return;
                case BlockVertical.Ceiling:
                    Ceiling.SetHeight(edge, newValue);
                    return;
            }

            if (vertical.IsExtraFloorSubdivision())
            {
                Subdivision existingSubdivision = ExtraFloorSubdivisions.ElementAtOrDefault(vertical.GetExtraSubdivisionIndex());

                if (existingSubdivision == null)
                {
                    if (!IsValidNextSubdivision(vertical))
                        return;

                    existingSubdivision = ExtraFloorSubdivisions.AddAndReturn(new Subdivision(Floor.Min));
                }

                existingSubdivision.Edges[(int)edge] = checked((short)newValue);
            }
            else if (vertical.IsExtraCeilingSubdivision())
            {
                Subdivision existingSubdivision = ExtraCeilingSubdivisions.ElementAtOrDefault(vertical.GetExtraSubdivisionIndex());

                if (existingSubdivision == null)
                {
                    if (!IsValidNextSubdivision(vertical))
                        return;

                    existingSubdivision = ExtraCeilingSubdivisions.AddAndReturn(new Subdivision(Ceiling.Max));
                }
                
                existingSubdivision.Edges[(int)edge] = checked((short)newValue);
            }
        }

        public bool IsValidNextSubdivision(BlockVertical vertical)
        {
            if (vertical.IsExtraFloorSubdivision())
                return vertical.GetExtraSubdivisionIndex() == ExtraFloorSubdivisions.Count;
            else if (vertical.IsExtraCeilingSubdivision())
                return vertical.GetExtraSubdivisionIndex() == ExtraCeilingSubdivisions.Count;
            else
                return false;
        }

        public bool SubdivisionExists(BlockVertical vertical)
        {
            if (vertical.IsExtraFloorSubdivision())
                return ExtraFloorSubdivisions.ElementAtOrDefault(vertical.GetExtraSubdivisionIndex()) != null;
            else if (vertical.IsExtraCeilingSubdivision())
                return ExtraCeilingSubdivisions.ElementAtOrDefault(vertical.GetExtraSubdivisionIndex()) != null;
            else
                return false;
        }

        private static DiagonalSplit TransformDiagonalSplit(DiagonalSplit split, RectTransformation transformation)
        {
            if (transformation.MirrorX)
                switch (split)
                {
                    case DiagonalSplit.XnZn:
                        split = DiagonalSplit.XpZn;
                        break;
                    case DiagonalSplit.XpZn:
                        split = DiagonalSplit.XnZn;
                        break;
                    case DiagonalSplit.XnZp:
                        split = DiagonalSplit.XpZp;
                        break;
                    case DiagonalSplit.XpZp:
                        split = DiagonalSplit.XnZp;
                        break;
                }

            for (int i = 0; i < transformation.QuadrantRotation; ++i)
                switch (split)
                {
                    case DiagonalSplit.XpZp:
                        split = DiagonalSplit.XnZp;
                        break;
                    case DiagonalSplit.XnZp:
                        split = DiagonalSplit.XnZn;
                        break;
                    case DiagonalSplit.XnZn:
                        split = DiagonalSplit.XpZn;
                        break;
                    case DiagonalSplit.XpZn:
                        split = DiagonalSplit.XpZp;
                        break;
                }

            return split;
        }

        private void MirrorWallTexture(BlockFace oldFace, Func<BlockFace, BlockFaceShape> oldFaceIsTriangle)
        {
            if (!_faceTextures.TryGetValue(oldFace, out TextureArea area))
                return;

            switch (oldFaceIsTriangle(oldFace))
            {
                case BlockFaceShape.Triangle:
                    Swap.Do(ref area.TexCoord0, ref area.TexCoord1);
                    break;
                case BlockFaceShape.Quad:
                    Swap.Do(ref area.TexCoord0, ref area.TexCoord3);
                    Swap.Do(ref area.TexCoord1, ref area.TexCoord2);
                    break;
            }

            _faceTextures[oldFace] = area;
        }

        // Rotates and mirrors a sector according to a given transformation. The transformation can be combination of rotations and mirrors
        // but all possible transformation essentially are boiled down to a mirror on the x axis and a rotation afterwards.
        // Thus all that needs to be handled is mirroring on x and a single counterclockwise rotation (perhaps multiple times in a row).
        // When mirroring is done, a oldFaceIsTriangle must be provided that can return the previous shape of texture faces.
        // Set "onlyFloor" to true, to only modify the floor.
        // Set "onlyFloor" to false, to only modify the ceiling.
        public void Transform(RectTransformation transformation, bool? onlyFloor = null, Func<BlockFace, BlockFaceShape> oldFaceIsTriangle = null)
        {
            // Rotate sector flags
            if (transformation.MirrorX)
                Flags =
                    (Flags & ~(BlockFlags.ClimbPositiveX | BlockFlags.ClimbNegativeX)) |
                    ((Flags & BlockFlags.ClimbPositiveX) != BlockFlags.None ? BlockFlags.ClimbNegativeX : BlockFlags.None) |
                    ((Flags & BlockFlags.ClimbNegativeX) != BlockFlags.None ? BlockFlags.ClimbPositiveX : BlockFlags.None);

            for (int i = 0; i < transformation.QuadrantRotation; ++i)
                Flags =
                    (Flags & ~(BlockFlags.ClimbPositiveX | BlockFlags.ClimbPositiveZ | BlockFlags.ClimbNegativeX | BlockFlags.ClimbNegativeZ)) |
                    ((Flags & BlockFlags.ClimbPositiveX) != BlockFlags.None ? BlockFlags.ClimbPositiveZ : BlockFlags.None) |
                    ((Flags & BlockFlags.ClimbPositiveZ) != BlockFlags.None ? BlockFlags.ClimbNegativeX : BlockFlags.None) |
                    ((Flags & BlockFlags.ClimbNegativeX) != BlockFlags.None ? BlockFlags.ClimbNegativeZ : BlockFlags.None) |
                    ((Flags & BlockFlags.ClimbNegativeZ) != BlockFlags.None ? BlockFlags.ClimbPositiveX : BlockFlags.None);

            // Rotate sector geometry
            bool diagonalChange = transformation.MirrorX != (transformation.QuadrantRotation % 2 != 0);
            bool oldFloorSplitDirectionIsXEqualsZReal = Floor.SplitDirectionIsXEqualsZWithDiagonalSplit;
            if (onlyFloor != false)
            {
                bool requiredFloorSplitDirectionIsXEqualsZ = Floor.SplitDirectionIsXEqualsZ != diagonalChange;
                Floor.DiagonalSplit = TransformDiagonalSplit(Floor.DiagonalSplit, transformation);
                transformation.TransformValueDiagonalQuad(ref Floor.XpZp, ref Floor.XnZp, ref Floor.XnZn, ref Floor.XpZn);

                foreach (Subdivision subdivision in ExtraFloorSubdivisions)
                    transformation.TransformValueDiagonalQuad(ref subdivision.Edges[(int)BlockEdge.XpZp], ref subdivision.Edges[(int)BlockEdge.XnZp], ref subdivision.Edges[(int)BlockEdge.XnZn], ref subdivision.Edges[(int)BlockEdge.XpZn]);

                if (requiredFloorSplitDirectionIsXEqualsZ != Floor.SplitDirectionIsXEqualsZ)
                    Floor.SplitDirectionToggled = !Floor.SplitDirectionToggled;

                if (HasGhostBlock)
                    transformation.TransformValueDiagonalQuad(ref GhostBlock.Floor.XpZp, ref GhostBlock.Floor.XnZp, ref GhostBlock.Floor.XnZn, ref GhostBlock.Floor.XpZn);

            }

            bool oldCeilingSplitDirectionIsXEqualsZReal = Ceiling.SplitDirectionIsXEqualsZWithDiagonalSplit;
            if (onlyFloor != true)
            {
                bool requiredCeilingSplitDirectionIsXEqualsZ = Ceiling.SplitDirectionIsXEqualsZ != diagonalChange;
                Ceiling.DiagonalSplit = TransformDiagonalSplit(Ceiling.DiagonalSplit, transformation);
                transformation.TransformValueDiagonalQuad(ref Ceiling.XpZp, ref Ceiling.XnZp, ref Ceiling.XnZn, ref Ceiling.XpZn);

                foreach (Subdivision subdivision in ExtraCeilingSubdivisions)
                    transformation.TransformValueDiagonalQuad(ref subdivision.Edges[(int)BlockEdge.XpZp], ref subdivision.Edges[(int)BlockEdge.XnZp], ref subdivision.Edges[(int)BlockEdge.XnZn], ref subdivision.Edges[(int)BlockEdge.XpZn]);

                if (requiredCeilingSplitDirectionIsXEqualsZ != Ceiling.SplitDirectionIsXEqualsZ)
                    Ceiling.SplitDirectionToggled = !Ceiling.SplitDirectionToggled;

                if (HasGhostBlock)
                    transformation.TransformValueDiagonalQuad(ref GhostBlock.Ceiling.XpZp, ref GhostBlock.Ceiling.XnZp, ref GhostBlock.Ceiling.XnZn, ref GhostBlock.Ceiling.XpZn);
            }

            // Rotate applied textures
            if (onlyFloor != false)
            {
                // Fix lower wall textures
                if (transformation.MirrorX)
                {
                    MirrorWallTexture(BlockFace.Wall_PositiveX_QA, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_PositiveZ_QA, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_NegativeX_QA, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_NegativeZ_QA, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_Diagonal_QA, oldFaceIsTriangle);

                    for (int i = 0; i < ExtraFloorSubdivisions.Count; i++)
                    {
                        MirrorWallTexture(BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, i), oldFaceIsTriangle);
                        MirrorWallTexture(BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, i), oldFaceIsTriangle);
                        MirrorWallTexture(BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, i), oldFaceIsTriangle);
                        MirrorWallTexture(BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, i), oldFaceIsTriangle);
                        MirrorWallTexture(BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, i), oldFaceIsTriangle);
                    }
                }

                transformation.TransformValueQuad(_faceTextures, BlockFace.Wall_PositiveX_QA, BlockFace.Wall_PositiveZ_QA, BlockFace.Wall_NegativeX_QA, BlockFace.Wall_NegativeZ_QA);

                for (int i = 0; i < ExtraFloorSubdivisions.Count; i++)
                {
                    transformation.TransformValueQuad(_faceTextures,
                        BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, i),
                        BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, i),
                        BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, i),
                        BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, i));
                }      

                // Fix floor textures
                if (Floor.IsQuad)
                {
                    if (_faceTextures.ContainsKey(BlockFace.Floor))
                        _faceTextures[BlockFace.Floor] = _faceTextures[BlockFace.Floor].Transform(transformation * new RectTransformation { QuadrantRotation = 2 });
                }
                else
                {
                    // Mirror
                    if (transformation.MirrorX)
                    {
                        _faceTextures.TrySwap(BlockFace.Floor, BlockFace.Floor_Triangle2);

                        if (_faceTextures.ContainsKey(BlockFace.Floor))
                        {
                            TextureArea floor = _faceTextures[BlockFace.Floor];
                            Swap.Do(ref floor.TexCoord0, ref floor.TexCoord2);
                            _faceTextures[BlockFace.Floor] = floor;
                        }

                        if (_faceTextures.ContainsKey(BlockFace.Floor_Triangle2))
                        {
                            TextureArea floorTriangle2 = _faceTextures[BlockFace.Floor_Triangle2];
                            Swap.Do(ref floorTriangle2.TexCoord0, ref floorTriangle2.TexCoord2);
                            _faceTextures[BlockFace.Floor_Triangle2] = floorTriangle2;
                        }

                        if (Floor.DiagonalSplit != DiagonalSplit.None) // REMOVE this when we have better diaognal steps.
                            _faceTextures.TrySwap(BlockFace.Floor, BlockFace.Floor_Triangle2);
                    }

                    // Rotation
                    for (int i = 0; i < transformation.QuadrantRotation; ++i)
                    {
                        if (!oldFloorSplitDirectionIsXEqualsZReal)
                            _faceTextures.TrySwap(BlockFace.Floor, BlockFace.Floor_Triangle2);
                        if (Floor.DiagonalSplit != DiagonalSplit.None) // REMOVE this when we have better diaognal steps.
                            _faceTextures.TrySwap(BlockFace.Floor, BlockFace.Floor_Triangle2);

                        oldFloorSplitDirectionIsXEqualsZReal = !oldFloorSplitDirectionIsXEqualsZReal;
                    }
                }
            }
            if (onlyFloor != true)
            {
                // Fix upper wall textures
                if (transformation.MirrorX)
                {
                    MirrorWallTexture(BlockFace.Wall_PositiveX_WS, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_PositiveZ_WS, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_NegativeX_WS, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_NegativeZ_WS, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_Diagonal_WS, oldFaceIsTriangle);

                    for (int i = 0; i < ExtraCeilingSubdivisions.Count; i++)
                    {
                        MirrorWallTexture(BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, i), oldFaceIsTriangle);
                        MirrorWallTexture(BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, i), oldFaceIsTriangle);
                        MirrorWallTexture(BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, i), oldFaceIsTriangle);
                        MirrorWallTexture(BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, i), oldFaceIsTriangle);
                        MirrorWallTexture(BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, i), oldFaceIsTriangle);
                    }
                }

                transformation.TransformValueQuad(_faceTextures, BlockFace.Wall_PositiveX_WS, BlockFace.Wall_PositiveZ_WS, BlockFace.Wall_NegativeX_WS, BlockFace.Wall_NegativeZ_WS);

                for (int i = 0; i < ExtraCeilingSubdivisions.Count; i++)
                {
                    transformation.TransformValueQuad(_faceTextures,
                        BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, i),
                        BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, i),
                        BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, i),
                        BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, i));
                }   

                // Fix ceiling textures
                if (Ceiling.IsQuad)
                {
                    if (_faceTextures.ContainsKey(BlockFace.Ceiling))
                        _faceTextures[BlockFace.Ceiling] = _faceTextures[BlockFace.Ceiling].Transform(transformation * new RectTransformation { QuadrantRotation = 2 });
                }
                else
                {
                    // Mirror
                    if (transformation.MirrorX)
                    {
                        _faceTextures.TrySwap(BlockFace.Ceiling, BlockFace.Ceiling_Triangle2);

                        if (_faceTextures.ContainsKey(BlockFace.Ceiling))
                        {
                            TextureArea ceiling = _faceTextures[BlockFace.Ceiling];
                            Swap.Do(ref ceiling.TexCoord0, ref ceiling.TexCoord2);
                            _faceTextures[BlockFace.Ceiling] = ceiling;
                        }

                        if (_faceTextures.ContainsKey(BlockFace.Ceiling_Triangle2))
                        {
                            TextureArea ceilingTriangle2 = _faceTextures[BlockFace.Ceiling_Triangle2];
                            Swap.Do(ref ceilingTriangle2.TexCoord0, ref ceilingTriangle2.TexCoord2);
                            _faceTextures[BlockFace.Ceiling_Triangle2] = ceilingTriangle2;
                        }

                        if (Ceiling.DiagonalSplit != DiagonalSplit.None) // REMOVE this when we have better diaognal steps.
                            _faceTextures.TrySwap(BlockFace.Ceiling, BlockFace.Ceiling_Triangle2);
                    }

                    // Rotation
                    for (int i = 0; i < transformation.QuadrantRotation; ++i)
                    {
                        if (!oldCeilingSplitDirectionIsXEqualsZReal)
                            _faceTextures.TrySwap(BlockFace.Ceiling, BlockFace.Ceiling_Triangle2);
                        if (Ceiling.DiagonalSplit != DiagonalSplit.None) // REMOVE this when we have better diaognal steps.
                            _faceTextures.TrySwap(BlockFace.Ceiling, BlockFace.Ceiling_Triangle2);

                        oldCeilingSplitDirectionIsXEqualsZReal = !oldCeilingSplitDirectionIsXEqualsZReal;
                    }
                }
            }
            if (onlyFloor == null)
            {
                if (transformation.MirrorX)
                {
                    MirrorWallTexture(BlockFace.Wall_PositiveX_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_PositiveZ_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_NegativeX_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_NegativeZ_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_Diagonal_Middle, oldFaceIsTriangle);
                }

                transformation.TransformValueQuad(_faceTextures, BlockFace.Wall_PositiveX_Middle, BlockFace.Wall_PositiveZ_Middle, BlockFace.Wall_NegativeX_Middle, BlockFace.Wall_NegativeZ_Middle);
            }
        }

        public void FixHeights(BlockVertical? vertical = null)
        {
            for (BlockEdge i = 0; i < BlockEdge.Count; i++)
            {
                for (int j = 0; j < ExtraFloorSubdivisions.Count; j++)
                {
                    BlockVertical subdivisionVertical = BlockVerticalExtensions.GetExtraFloorSubdivision(j);
                    BlockVertical lastBlockVertical = j == 0 ? BlockVertical.Floor : BlockVerticalExtensions.GetExtraFloorSubdivision(j - 1); 
                    SetHeight(subdivisionVertical, i, Math.Min(GetHeight(subdivisionVertical, i), GetHeight(lastBlockVertical, i)));
                }

                for (int j = 0; j < ExtraCeilingSubdivisions.Count; j++)
                {
                    BlockVertical subdivisionVertical = BlockVerticalExtensions.GetExtraCeilingSubdivision(j);
                    BlockVertical lastBlockVertical = j == 0 ? BlockVertical.Ceiling : BlockVerticalExtensions.GetExtraCeilingSubdivision(j - 1);
                    SetHeight(subdivisionVertical, i, Math.Max(GetHeight(subdivisionVertical, i), GetHeight(lastBlockVertical, i)));
                }

                if (vertical == null || vertical.Value.IsOnFloor())
                    if (Floor.DiagonalSplit != DiagonalSplit.None)
                        Floor.SetHeight(i, Math.Min(Floor.GetHeight(i), Ceiling.Min));
                    else
                        Floor.SetHeight(i, Math.Min(Floor.GetHeight(i), Ceiling.GetHeight(i)));

                if (vertical == null || vertical.Value.IsOnCeiling())
                    if (Ceiling.DiagonalSplit != DiagonalSplit.None)
                        Ceiling.SetHeight(i, Math.Max(Ceiling.GetHeight(i), Floor.Max));
                    else
                        Ceiling.SetHeight(i, Math.Max(Ceiling.GetHeight(i), Floor.GetHeight(i)));
            }
        }

        public Vector3[] GetFloorTriangleNormals()
        {
            Plane[] tri = new Plane[2];

            var p0 = new Vector3(0, Floor.XnZp, 0);
            var p1 = new Vector3(4, Floor.XpZp, 0);
            var p2 = new Vector3(4, Floor.XpZn, -4);
            var p3 = new Vector3(0, Floor.XnZn, -4);

            // Create planes based on floor split direction

            if (Floor.SplitDirectionIsXEqualsZ)
            {
                tri[0] = Plane.CreateFromVertices(p1, p2, p3);
                tri[1] = Plane.CreateFromVertices(p1, p3, p0);
            }
            else
            {
                tri[0] = Plane.CreateFromVertices(p0, p1, p2);
                tri[1] = Plane.CreateFromVertices(p0, p2, p3);
            }

            return new Vector3[2] { tri[0].Normal, tri[1].Normal };
        }

        public short GetTriangleMinimumFloorPoint(int triangle)
        {
            if (triangle != 0 && triangle != 1)
                return 0;

            if (Floor.SplitDirectionIsXEqualsZ)
            {
                if (triangle == 0)
                    return Math.Min(Math.Min(Floor.XpZp, Floor.XpZn), Floor.XnZn);
                else
                    return Math.Min(Math.Min(Floor.XpZp, Floor.XnZn), Floor.XnZp);
            }
            else
            {
                if (triangle == 0)
                    return Math.Min(Math.Min(Floor.XnZp, Floor.XpZp), Floor.XpZn);
                else
                    return Math.Min(Math.Min(Floor.XnZp, Floor.XpZn), Floor.XnZn);
            }
        }

        public Direction[] GetFloorTriangleSlopeDirections()
        {
            const float CriticalSlantYComponent = 0.8f;

            var normals = GetFloorTriangleNormals();

            // Initialize slope directions as unslidable by default (EntireFace means unslidable in our case).

            Direction[] slopeDirections = new Direction[2] { Direction.None, Direction.None };

            if (Floor.HasSlope)
            {
                for (int i = 0; i < (Floor.IsQuad ? 1 : 2); i++) // If floor is quad, we don't solve second triangle
                {
                    if (Math.Abs(normals[i].Y) <= CriticalSlantYComponent) // Triangle is slidable
                    {
                        bool angleNotDefined = true;
                        var angle = (float)(Math.Atan2(normals[i].X, normals[i].Z) * (180 / Math.PI));
                        angle = angle < 0 ? angle + 360.0f : angle;

                        // Note about 45, 135, 225 and 315 degree steps:
                        // Core Design has used override instead of rounding for triangular slopes angled under
                        // 45-degree stride, to produce either east or west-oriented slide.

                        while (angleNotDefined)
                        {
                            switch ((int)angle)
                            {
                                case 0:
                                case 360:
                                    slopeDirections[i] = Direction.PositiveZ;
                                    angleNotDefined = false;
                                    break;
                                case 45:
                                case 90:
                                case 135:
                                    slopeDirections[i] = Direction.PositiveX;
                                    angleNotDefined = false;
                                    break;
                                case 180:
                                    slopeDirections[i] = Direction.NegativeZ;
                                    angleNotDefined = false;
                                    break;
                                case 225:
                                case 270:
                                case 315:
                                    slopeDirections[i] = Direction.NegativeX;
                                    angleNotDefined = false;
                                    break;
                                default:
                                    angle = (int)Math.Round(angle / 90.0f, MidpointRounding.AwayFromZero) * 90;
                                    break;
                            }
                        }
                    }
                }

                // We swap triangle directions for XpZn and XnZp cases, because in these cases
                // triangle indices are inverted.
                // For other cases, we move slide direction triangle to proper one accordingly to
                // step slant value encoded in corner heights.

                if (Floor.DiagonalSplit != DiagonalSplit.None)
                {
                    switch (Floor.DiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            slopeDirections[1] = slopeDirections[0];
                            slopeDirections[0] = Direction.None;
                            break;

                        case DiagonalSplit.XnZp:
                            if (!Floor.IsQuad)
                            {
                                slopeDirections[0] = slopeDirections[1];
                                slopeDirections[1] = Direction.None;
                            }
                            break;

                        case DiagonalSplit.XnZn:
                            if (Floor.IsQuad)
                            {
                                slopeDirections[1] = slopeDirections[0];
                                slopeDirections[0] = Direction.None;
                            }
                            else
                                slopeDirections[0] = Direction.None;
                            break;

                        case DiagonalSplit.XpZp:
                            if (!Floor.IsQuad)
                                slopeDirections[1] = Direction.None;
                            break;
                    }
                }
            }

            return slopeDirections;
        }
    }
}
