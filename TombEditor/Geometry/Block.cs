using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class Block
    {
        public BlockType Type { get; set; }
        public BlockFlags Flags { get; set; }
        public short[] EDFaces { get; set; } = new short[4];
        public short[] QAFaces { get; set; } = new short[4];
        public short[] WSFaces { get; set; } = new short[4];
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
        public Room Room { get; set; }
        public Level Level { get; set; }
        public DiagonalSplit FloorDiagonalSplit { get; set; }
        public DiagonalSplitType FloorDiagonalSplitType { get; set; }
        public DiagonalSplit CeilingDiagonalSplit { get; set; }
        public DiagonalSplitType CeilingDiagonalSplitType { get; set; }

        public Block(Level level, Room room, BlockType type, BlockFlags flags, short height)
        {
            Type = type;
            Flags = flags;
            Level = level;
            Room = room;

            for (int i = 0; i < 29; i++)
            {
                BlockFace face = new BlockFace();
                Faces[i] = face;
            }
        }

        public bool IsFloorSplit
        {
            get
            {
                int x = 0;
                int z = 0;

                Vector3 p1 = new Vector3(x * 1024.0f, QAFaces[0] * 256.0f, z * 1024.0f);
                Vector3 p2 = new Vector3((x + 1) * 1024.0f, QAFaces[1] * 256.0f, z * 1024.0f);
                Vector3 p3 = new Vector3((x + 1) * 1024.0f, QAFaces[2] * 256.0f, (z + 1) * 1024.0f);
                Vector3 p4 = new Vector3(x * 1024.0f, QAFaces[3] * 256.0f, (z + 1) * 1024.0f);

                Plane plane1 = new Plane(p1, p2, p3);
                Plane plane2 = new Plane(p1, p2, p4);

                if (plane1.Normal != plane2.Normal)
                    return false;

                return true;
            }
        }

        public bool IsCeilingSplit
        {
            get
            {
                int x = 0;
                int z = 0;

                Vector3 p1 = new Vector3(x * 1024.0f, WSFaces[0] * 256.0f, z * 1024.0f);
                Vector3 p2 = new Vector3((x + 1) * 1024.0f, WSFaces[1] * 256.0f, z * 1024.0f);
                Vector3 p3 = new Vector3((x + 1) * 1024.0f, WSFaces[2] * 256.0f, (z + 1) * 1024.0f);
                Vector3 p4 = new Vector3(x * 1024.0f, WSFaces[3] * 256.0f, (z + 1) * 1024.0f);

                Plane plane1 = new Plane(p1, p2, p3);
                Plane plane2 = new Plane(p1, p2, p4);

                if (plane1.Normal != plane2.Normal)
                    return false;

                return true;
            }
        }

        public Block Clone()
        {
            Block b = new Geometry.Block(Level, Room, Type, Flags, 0);

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

            for (int i = 0; i < 29; i++)
            {
                BlockFace f = Faces[i];
                BlockFace newFace = new Geometry.BlockFace();

                newFace.Defined = f.Defined;
                newFace.DoubleSided = f.DoubleSided;
                newFace.Flipped = f.Flipped;
                newFace.Invisible = f.Invisible;
                newFace.Rotation = f.Rotation;
                newFace.Shape = f.Shape;
                newFace.SplitMode = f.SplitMode;
                newFace.Texture = f.Texture;
                newFace.TextureTriangle = f.TextureTriangle;

                //if (f.Texture != -1) Level.TextureSamples[f.Texture].UsageCount++;
            }

            return b;
        }
    }
}
