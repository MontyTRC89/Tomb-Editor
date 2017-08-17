using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
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
        
        public BlockType Type { get; set; }
        public BlockFlags Flags { get; set; }
        // ReSharper disable once InconsistentNaming
        public short[] EDFaces { get; set; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] QAFaces { get; set; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] WSFaces { get; set; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] RFFaces { get; set; } = new short[4];
        public BlockFace[] Faces { get; set; } = new BlockFace[29];
        public byte SplitFoorType { get; set; }
        public byte SplitCeilingType { get; set; }
        public bool SplitFloor { get; set; } = false;
        public byte RealSplitFloor { get; set; }
        public byte RealSplitCeiling { get; set; }
        public bool SplitCeiling { get; set; } = false;
        public bool[] Climb { get; set; } = new bool[4];
        public PortalOpacity FloorOpacity { get; set; }
        public PortalOpacity CeilingOpacity { get; set; }
        public PortalOpacity WallOpacity { get; set; }
        public int FloorPortal { get; set; } = -1;
        public int WallPortal { get; set; } = -1;
        public int CeilingPortal { get; set; } = -1;
        public short FloorSlopeX { get; set; }
        public short FloorSlopeZ { get; set; }
        public short CeilingSlopeX { get; set; }
        public short CeilingSlopeZ { get; set; }
        public bool IsFloorSolid { get; set; }
        public bool IsCeilingSolid { get; set; }
        public bool NoCollisionFloor { get; set; }
        public bool NoCollisionCeiling { get; set; }
        public List<int> Triggers { get; set; } = new List<int>();
        public DiagonalSplit FloorDiagonalSplit { get; set; }
        public DiagonalSplitType FloorDiagonalSplitType { get; set; }
        public DiagonalSplit CeilingDiagonalSplit { get; set; }
        public DiagonalSplitType CeilingDiagonalSplitType { get; set; }

        public Block(BlockType type, BlockFlags flags)
        {
            Type = type;
            Flags = flags;

            for (int i = 0; i < 29; i++)
            {
                var face = new BlockFace();
                Faces[i] = face;
            }
        }

        public bool IsFloorSplit
        {
            get
            {
                const int x = 0;
                const int z = 0;

                var p1 = new Vector3(x * 1024.0f, QAFaces[0] * 256.0f, z * 1024.0f);
                var p2 = new Vector3((x + 1) * 1024.0f, QAFaces[1] * 256.0f, z * 1024.0f);
                var p3 = new Vector3((x + 1) * 1024.0f, QAFaces[2] * 256.0f, (z + 1) * 1024.0f);
                var p4 = new Vector3(x * 1024.0f, QAFaces[3] * 256.0f, (z + 1) * 1024.0f);

                var plane1 = new Plane(p1, p2, p3);
                var plane2 = new Plane(p1, p2, p4);

                return plane1.Normal == plane2.Normal;
            }
        }

        public bool IsCeilingSplit
        {
            get
            {
                const int x = 0;
                const int z = 0;

                var p1 = new Vector3(x * 1024.0f, WSFaces[0] * 256.0f, z * 1024.0f);
                var p2 = new Vector3((x + 1) * 1024.0f, WSFaces[1] * 256.0f, z * 1024.0f);
                var p3 = new Vector3((x + 1) * 1024.0f, WSFaces[2] * 256.0f, (z + 1) * 1024.0f);
                var p4 = new Vector3(x * 1024.0f, WSFaces[3] * 256.0f, (z + 1) * 1024.0f);

                var plane1 = new Plane(p1, p2, p3);
                var plane2 = new Plane(p1, p2, p4);

                return plane1.Normal == plane2.Normal;
            }
        }

        public Block Clone()
        {
            var b = new Block(Type, Flags);

            for (int i = 0; i < 4; i++)
                b.QAFaces[i] = QAFaces[i];
            for (int i = 0; i < 4; i++)
                b.EDFaces[i] = EDFaces[i];
            for (int i = 0; i < 4; i++)
                b.WSFaces[i] = WSFaces[i];
            for (int i = 0; i < 4; i++)
                b.RFFaces[i] = RFFaces[i];

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

        public bool IsFloor
        {
            get { return Type == BlockType.Floor; }
        }

        public bool IsAnyWall
        {
            get { return Type != BlockType.Floor; }
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
