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

    public enum DiagonalSplitType : byte
    {
        None = 0,
        Floor = 1,
        Ceiling = 2,
        FloorAndCeiling = 3,
        Wall = 4
    }

    [Flags]
    public enum BlockFlags : short
    {
        None = 0,
        Monkey = 1,
        Opacity2 = 2,
        Trigger = 4,
        Box = 8,
        Death = 16,
        Lava = 32,
        Electricity = 64,
        Opacity = 128,
        Beetle = 256,
        TriggerTriggerer = 512,
        NotWalkableFloor = 1024
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
        public byte SplitFoorType { get; set; } = 0;
        public byte SplitCeilingType { get; set; } = 0;
        public bool SplitFloor { get; set; } = false;
        public byte RealSplitFloor { get; set; }
        public byte RealSplitCeiling { get; set; }
        public bool SplitCeiling { get; set; } = false;
        public bool[] Climb { get; set; } = new bool[4];
        public PortalOpacity FloorOpacity { get; set; }
        public PortalOpacity CeilingOpacity { get; set; }
        public PortalOpacity WallOpacity { get; set; }
        public Portal FloorPortal { get; set; } = null;
        public Portal WallPortal { get; set; } = null;
        public Portal CeilingPortal { get; set; } = null;
        public short FloorSlopeX { get; set; } // To remove since information is confusing and redundant
        public short FloorSlopeZ { get; set; } // To remove since information is confusing and redundant
        public short CeilingSlopeX { get; set; } // To remove since information is confusing and redundant
        public short CeilingSlopeZ { get; set; } // To remove since information is confusing and redundant
        public bool NoCollisionFloor { get; set; }
        public bool NoCollisionCeiling { get; set; }
        public List<TriggerInstance> Triggers { get; } = new List<TriggerInstance>(); // This array is not supposed to be modified.
        public DiagonalSplit FloorDiagonalSplit { get; set; }
        public DiagonalSplitType FloorDiagonalSplitType { get; set; }
        public DiagonalSplit CeilingDiagonalSplit { get; set; }
        public DiagonalSplitType CeilingDiagonalSplitType { get; set; }

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

            b.RealSplitFloor = RealSplitFloor;
            b.SplitFloor = SplitFloor;
            b.SplitFoorType = SplitFoorType;

            b.RealSplitCeiling = RealSplitCeiling;
            b.SplitCeiling = SplitCeiling;
            b.SplitCeilingType = SplitCeilingType;

            b.FloorSlopeX = FloorSlopeX;
            b.FloorSlopeZ = FloorSlopeZ;

            b.CeilingSlopeX = CeilingSlopeX;
            b.CeilingSlopeZ = CeilingSlopeZ;

            for (int i = 0; i < 4; i++)
                b.Climb[i] = Climb[i];

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
    }
}
