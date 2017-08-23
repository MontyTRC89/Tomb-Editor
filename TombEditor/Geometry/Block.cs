using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

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

    public class Block
    {
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
        public BlockFace[] Faces { get; } = new BlockFace[29];
        public bool FloorSplitDirectionToggled { get; set; } = false;
        public bool CeilingSplitDirectionToggled { get; set; } = false;
        public bool FloorIsSplit { get; set; } = false;
        public bool CeilingIsSplit { get; set; } = false;
        public PortalOpacity FloorOpacity { get; set; }
        public PortalOpacity CeilingOpacity { get; set; }
        public PortalOpacity WallOpacity { get; set; }
        public Portal FloorPortal { get; set; } = null; // This array is not supposed to be modified here.
        public Portal WallPortal { get; set; } = null; // This array is not supposed to be modified here.
        public Portal CeilingPortal { get; set; } = null; // This array is not supposed to be modified here.
        public short FloorSlopeX { get; set; } // To remove since information is confusing and redundant
        public short FloorSlopeZ { get; set; } // To remove since information is confusing and redundant
        public short CeilingSlopeX { get; set; } // To remove since information is confusing and redundant
        public short CeilingSlopeZ { get; set; } // To remove since information is confusing and redundant
        public bool NoCollisionFloor { get; set; }
        public bool NoCollisionCeiling { get; set; }
        public List<TriggerInstance> Triggers { get; } = new List<TriggerInstance>(); // This array is not supposed to be modified here.
        public DiagonalSplit FloorDiagonalSplit { get; set; }
        public DiagonalSplit CeilingDiagonalSplit { get; set; }

        public Block(int floor, int ceiling)
        {
            for (int i = 0; i < 29; i++)
                Faces[i] = new BlockFace();

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
            
            b.FloorIsSplit = FloorIsSplit;
            b.FloorSplitDirectionToggled = FloorSplitDirectionToggled;
            
            b.CeilingIsSplit = CeilingIsSplit;
            b.CeilingSplitDirectionToggled = CeilingSplitDirectionToggled;

            b.FloorSlopeX = FloorSlopeX;
            b.FloorSlopeZ = FloorSlopeZ;

            b.CeilingSlopeX = CeilingSlopeX;
            b.CeilingSlopeZ = CeilingSlopeZ;
            
            return b;
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
            const int x = 0;
            const int z = 0;
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

        public bool FloorSplitRealDirection
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
        }

        public bool CeilingSplitRealDirection
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
        }
    }
}
