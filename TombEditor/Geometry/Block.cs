using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using TombLib.Utils;

namespace TombEditor.Geometry
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
        ClimbPositiveX = 256,
        ClimbNegativeX = 512,
        ClimbPositiveZ = 1024,
        ClimbNegativeZ = 2048,
        ClimbAny = ClimbPositiveX  | ClimbNegativeX | ClimbPositiveZ | ClimbNegativeZ
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum DiagonalSplit : byte
    {
        None = 0,
        NW = 1,
        NE = 2,
        SE = 3,
        SW = 4
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum BlockFace : byte
    {
        NorthQA = 0, SouthQA = 1, WestQA = 2, EastQA = 3, DiagonalQA = 4,
        NorthED = 5, SouthED = 6, WestED = 7, EastED = 8, DiagonalED = 9,
        NorthMiddle = 10, SouthMiddle = 11, WestMiddle = 12, EastMiddle = 13, DiagonalMiddle = 14,
        NorthWS = 15, SouthWS = 16, WestWS = 17, EastWS = 18, DiagonalWS = 19,
        NorthRF = 20, SouthRF = 21, WestRF = 22, EastRF = 23, DiagonalRF = 24,
        Floor = 25, FloorTriangle2 = 26, Ceiling = 27, CeilingTriangle2 = 28
    }

    public class Block
    {
        public const BlockFace FaceCount = (BlockFace)29;
        /// <summary> Index of faces on the negative X and positive Z direction </summary>
        public const int FaceXnZp = 0;
        /// <summary> Index of faces on the positive X and positive Z direction </summary>
        public const int FaceXpZp = 1;
        /// <summary> Index of faces on the positive X and negative Z direction </summary>
        public const int FaceXpZn = 2;
        /// <summary> Index of faces on the negative X and negative Z direction </summary>
        public const int FaceXnZn = 3;
        /// <summary> The x offset of each face index in [0, 4). </summary>
        public static readonly int[] FaceX = new int[] { 0, 1, 1, 0 };
        /// <summary> The x offset of each face index in [0, 4). </summary>
        public static readonly int[] FaceZ = new int[] { 1, 1, 0, 0 };

        public BlockType Type { get; set; } = BlockType.Floor;
        public BlockFlags Flags { get; set; } = BlockFlags.None;
        // ReSharper disable once InconsistentNaming
        public short[] EDFaces { get; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] QAFaces { get; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] WSFaces { get; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] RFFaces { get; } = new short[4];
        private TextureArea[] _faceTextures { get; } = new TextureArea[(int)FaceCount];
        public bool FloorSplitDirectionToggled { get; set; } = false;
        public bool CeilingSplitDirectionToggled { get; set; } = false;

        public DiagonalSplit FloorDiagonalSplit { get; set; }
        public DiagonalSplit CeilingDiagonalSplit { get; set; }

        public List<TriggerInstance> Triggers { get; } = new List<TriggerInstance>(); // This array is not supposed to be modified here.

        public PortalOpacity FloorOpacity { get; set; }
        public PortalOpacity CeilingOpacity { get; set; }
        public PortalOpacity WallOpacity { get; set; }
        public Portal FloorPortal { get; set; } = null; // This is not supposed to be modified here.
        public Portal WallPortal { get; set; } = null; // This is not supposed to be modified here.
        public Portal CeilingPortal { get; set; } = null; // This is not supposed to be modified here.
        public bool NoCollisionFloor { get; set; }
        public bool NoCollisionCeiling { get; set; }

        // Helper data for Prj2 loading
        internal PortalOpacity TempFloorOpacity { get; set; }
        internal PortalOpacity TempCeilingOpacity { get; set; }
        internal PortalOpacity TempWallOpacity { get; set; }

        public Block(int floor, int ceiling)
        {
            for (int i = 0; i < 4; i++)
            {
                QAFaces[i] = (short)floor;
                EDFaces[i] = (short)floor;
                WSFaces[i] = (short)ceiling;
                RFFaces[i] = (short)ceiling;
            }
        }

        public Block Clone()
        {
            var b = new Block(0, 0);

            for (int i = 0; i < 4; i++)
                b.QAFaces[i] = QAFaces[i];
            for (int i = 0; i < 4; i++)
                b.EDFaces[i] = EDFaces[i];
            for (int i = 0; i < 4; i++)
                b.WSFaces[i] = WSFaces[i];
            for (int i = 0; i < 4; i++)
                b.RFFaces[i] = RFFaces[i];

            b.Flags = Flags;
            b.Type = Type;
            b.FloorSplitDirectionToggled = FloorSplitDirectionToggled;
            b.CeilingSplitDirectionToggled = CeilingSplitDirectionToggled;
            
            return b;
        }

        public void SetFaceTexture(BlockFace face, TextureArea texture)
        {
            _faceTextures[(int)face] = texture;
        }

        public TextureArea GetFaceTexture(BlockFace face)
        {
            return _faceTextures[(int)face];
        }

        public bool IsFloor => Type == BlockType.Floor;

        public bool IsAnyWall => Type != BlockType.Floor;

        public IEnumerable<Portal> Portals
        {
            get
            {
                if (WallPortal != null) yield return WallPortal;
                if (CeilingPortal != null) yield return CeilingPortal;
                if (FloorPortal != null) yield return FloorPortal;
            }
        }

        public short[] GetVerticalSubdivision(int verticalSubdivision)
        {
            switch (verticalSubdivision)
            {
                case 0:
                    return QAFaces;
                case 1:
                    return WSFaces;
                case 2:
                    return EDFaces;
                case 3:
                    return RFFaces;
                default:
                    throw new NotSupportedException();
            }
        }

        public short GetEdge(int verticalSubdivision, int edge)
        {
            return GetVerticalSubdivision(verticalSubdivision)[edge];
        }

        public void SetEdge(int verticalSubdivision, int edge, short newValue)
        {
            GetVerticalSubdivision(verticalSubdivision)[edge] = newValue;
        }

        public void ChangeEdge(int verticalSubdivision, int edge, short increment)
        {
            GetVerticalSubdivision(verticalSubdivision)[edge] += increment;
        }

        private static int FindHorizontalTriangle(int h1, int h2, int h3, int h4)
        {
            var p1 = new Vector3(0, h1, 0);
            var p2 = new Vector3(1, h2, 0);
            var p3 = new Vector3(1, h3, 1);
            var p4 = new Vector3(0, h4, 1);

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

        public bool FloorSplitDirectionIsXEqualsY
        {
            get
            {
                int h1 = QAFaces[0],h2 = QAFaces[1], h3 = QAFaces[2], h4 = QAFaces[3];
                int horizontalTriangle = FindHorizontalTriangle(h1, h2, h3, h4);

                switch (horizontalTriangle)
                {
                    case 0:
                        return !FloorSplitDirectionToggled;
                    case 1:
                        return FloorSplitDirectionToggled;
                    case 2:
                        return !FloorSplitDirectionToggled;
                    case 3:
                        return FloorSplitDirectionToggled;
                    default:
                        int min = Math.Min(Math.Min(Math.Min(h1, h2), h3), h4);
                        int max = Math.Max(Math.Max(Math.Max(h1, h2), h3), h4);

                        if (max == h1 && max == h3)
                            return !FloorSplitDirectionToggled;
                        if (max == h2 && max == h4)
                            return FloorSplitDirectionToggled;

                        if (min == h1 && max == h3)
                            return !FloorSplitDirectionToggled;
                        if (min == h2 && max == h4)
                            return FloorSplitDirectionToggled;
                        if (min == h3 && max == h1)
                            return !FloorSplitDirectionToggled;
                        if (min == h4 && max == h2)
                            return FloorSplitDirectionToggled;

                        break;
                }

                return FloorSplitDirectionToggled;
            }
            set
            {
                if (value != FloorSplitDirectionIsXEqualsY)
                    FloorSplitDirectionToggled = !FloorSplitDirectionToggled;
            }
        }

        public bool CeilingSplitDirectionIsXEqualsY
        {
            get
            {
                int h1 = WSFaces[0], h2 = WSFaces[1], h3 = WSFaces[2], h4 = WSFaces[3];
                int horizontalTriangle = FindHorizontalTriangle(h1, h2, h3, h4);

                switch (horizontalTriangle)
                {
                    case 0:
                        return CeilingSplitDirectionToggled;
                    case 1:
                        return !CeilingSplitDirectionToggled;
                    case 2:
                        return CeilingSplitDirectionToggled;
                    case 3:
                        return !CeilingSplitDirectionToggled;
                    default:
                        int min = Math.Min(Math.Min(Math.Min(h1, h2), h3), h4);
                        int max = Math.Max(Math.Max(Math.Max(h1, h2), h3), h4);

                        if (max == h1 && max == h3)
                            return CeilingSplitDirectionToggled;
                        if (max == h2 && max == h4)
                            return !CeilingSplitDirectionToggled;

                        if (min == h1 && max == h3)
                            return CeilingSplitDirectionToggled;
                        if (min == h2 && max == h4)
                            return !CeilingSplitDirectionToggled;
                        if (min == h3 && max == h1)
                            return CeilingSplitDirectionToggled;
                        if (min == h4 && max == h2)
                            return !CeilingSplitDirectionToggled;

                        break;
                }

                return CeilingSplitDirectionToggled;
            }
            set
            {
                if (value != CeilingSplitDirectionIsXEqualsY)
                    CeilingSplitDirectionToggled = !CeilingSplitDirectionToggled;
            }
        }

        public static bool IsQuad(int hXnZp, int hXpZp, int hXpZn, int hXnZn)
        {
            return (hXpZp - hXpZn) == (hXnZp - hXnZn) &&
                (hXpZp - hXnZp) == (hXpZn - hXnZn);
        }

        public bool FloorIsQuad => IsQuad(QAFaces[FaceXnZp], QAFaces[FaceXpZp], QAFaces[FaceXpZn], QAFaces[FaceXnZn]);
        public bool CeilingIsQuad => IsQuad(WSFaces[FaceXnZp], WSFaces[FaceXpZp], WSFaces[FaceXpZn], WSFaces[FaceXnZn]);
        public int FloorIfQuadSlopeX => FloorIsQuad ? QAFaces[FaceXpZp] - QAFaces[FaceXnZp] : 0;
        public int FloorIfQuadSlopeZ => FloorIsQuad ? QAFaces[FaceXpZp] - QAFaces[FaceXpZn] : 0;
        public int CeilingIfQuadSlopeX => CeilingIsQuad ? WSFaces[FaceXpZp] - WSFaces[FaceXnZp] : 0;
        public int CeilingIfQuadSlopeZ => CeilingIsQuad ? WSFaces[FaceXpZp] - WSFaces[FaceXpZn] : 0;
        public short FloorMax => Math.Max(Math.Max(QAFaces[0], QAFaces[1]), Math.Max(QAFaces[2], QAFaces[3]));
        public short FloorMin => Math.Min(Math.Min(QAFaces[0], QAFaces[1]), Math.Min(QAFaces[2], QAFaces[3]));
        public short CeilingMax => Math.Max(Math.Max(WSFaces[0], WSFaces[1]), Math.Max(WSFaces[2], WSFaces[3]));
        public short CeilingMin => Math.Min(Math.Min(WSFaces[0], WSFaces[1]), Math.Min(WSFaces[2], WSFaces[3]));
    }
}
