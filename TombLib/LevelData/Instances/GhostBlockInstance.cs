using System.Numerics;

namespace TombLib.LevelData
{
    public class GhostBlockInstance : SectorBasedObjectInstance, ISpatial
    {
        public BlockSurface Floor;
        public BlockSurface Ceiling;

        public bool SelectedFloor = true;
        public BlockEdge? SelectedCorner { get; set; }

        public VectorInt2 SectorPosition
        {
            get { return new VectorInt2(Area.X0, Area.Y0); }
            set { Area = new RectangleInt2(value.X, value.Y, value.X, value.Y); }
        }

        public Block Block => Room.Blocks[Area.X0, Area.Y0];
        public bool FloorIsQuad => Block.Floor.IsQuad;
        public bool CeilingIsQuad => Block.Ceiling.IsQuad;
        public bool FloorSplitToggled => Block.Floor.SplitDirectionIsXEqualsZ;
        public bool CeilingSplitToggled => Block.Ceiling.SplitDirectionIsXEqualsZ;

        public bool Editable => !Block.IsAnyWall;
        public bool Valid => Editable && (ValidFloor || ValidCeiling);
        public bool ValidFloor => Floor.XnZn != 0 || Floor.XpZn != 0 || Floor.XnZp != 0 || Floor.XpZp != 0;
        public bool ValidCeiling => Ceiling.XnZn != 0 || Ceiling.XpZn != 0 || Ceiling.XnZp != 0 || Ceiling.XpZp != 0;

        public GhostBlockInstance() : base(new RectangleInt2()) { }

        public override string ToString()
        {
            string text = "Ghost block in room '" + (Room?.ToString() ?? "NULL") + "' " +
                          "on sector [" + SectorPosition.X + ", " + SectorPosition.Y + "] ";
            return text;
        }

        public string InfoMessage()
        {
            return "floor: (" + Floor.XnZp + ", " + Floor.XpZp + ", " + Floor.XpZn + ", " + Floor.XnZn + ")\n" +
                   "ceiling: (" + Ceiling.XnZp + ", " + Ceiling.XpZp + ", " + Ceiling.XpZn + ", " + Ceiling.XnZn + ")";
        }

        public void Move(int delta, bool? forceSurface = null)
        {
            if (SelectedCorner.HasValue)
                Move(SelectedCorner.Value, delta, forceSurface.HasValue ? forceSurface.Value : SelectedFloor);
            else
                for (int i = 0; i < 4; i++)
                    Move((BlockEdge)(i), delta, forceSurface.HasValue ? forceSurface.Value : SelectedFloor);
        }

        private void Move (BlockEdge edge, int delta, bool floor)
        {
            if (floor)
                switch (edge)
                {
                    case BlockEdge.XnZn: Floor.XnZn += (short)delta; break;
                    case BlockEdge.XnZp: Floor.XnZp += (short)delta; break;
                    case BlockEdge.XpZn: Floor.XpZn += (short)delta; break;
                    case BlockEdge.XpZp: Floor.XpZp += (short)delta; break;
                }
            else
                switch (edge)
                {
                    case BlockEdge.XnZn: Ceiling.XnZn += (short)delta; break;
                    case BlockEdge.XnZp: Ceiling.XnZp += (short)delta; break;
                    case BlockEdge.XpZn: Ceiling.XpZn += (short)delta; break;
                    case BlockEdge.XpZp: Ceiling.XpZp += (short)delta; break;
                }
        }

        public Vector3[] ControlPositions(bool floor, bool original = false, float margin = -16.0f)
        {
            var result = new Vector3[4];

            var localCenter = new Vector3(SectorPosition.X * 1024.0f + 512.0f, 0, SectorPosition.Y * 1024.0f + 512.0f);
            var type = floor ? BlockVertical.Floor : BlockVertical.Ceiling;

            var hXnZn = (Block.GetHeight(type, BlockEdge.XnZn) + (original ? 0 : (floor ? Floor : Ceiling).XnZn)) * 256.0f + (floor ? -margin : margin);
            var hXpZp = (Block.GetHeight(type, BlockEdge.XpZp) + (original ? 0 : (floor ? Floor : Ceiling).XpZp)) * 256.0f + (floor ? -margin : margin);
            var hXnZp = (Block.GetHeight(type, BlockEdge.XnZp) + (original ? 0 : (floor ? Floor : Ceiling).XnZp)) * 256.0f + (floor ? -margin : margin);
            var hXpZn = (Block.GetHeight(type, BlockEdge.XpZn) + (original ? 0 : (floor ? Floor : Ceiling).XpZn)) * 256.0f + (floor ? -margin : margin);

            var shift = new Vector3();

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: shift.X = -512.0f - margin; shift.Y = hXnZp; shift.Z =  512.0f + margin; break;
                    case 1: shift.X =  512.0f + margin; shift.Y = hXpZp; shift.Z =  512.0f + margin; break;
                    case 2: shift.X =  512.0f + margin; shift.Y = hXpZn; shift.Z = -512.0f - margin; break;
                    case 3: shift.X = -512.0f - margin; shift.Y = hXnZn; shift.Z = -512.0f - margin; break;
                }
                result[i] = (Room?.WorldPos ?? new Vector3()) + (localCenter + shift);
            }
            return result;
        }

        public Vector3 Center(bool floor)
        {
            var pos = ControlPositions(floor);
            return (pos[0] + pos[1] + pos[2] + pos[3]) / 4.0f + new Vector3(0, 96.0f, 0);
        }

        public Matrix4x4[] ControlMatrixes(bool floor)
        {
            var rotIdentity = Matrix4x4.CreateFromYawPitchRoll(0f, 0f, 0f);
            var scIdentity = Matrix4x4.CreateScale(1.0f);
            var pos = ControlPositions(floor);

            return new Matrix4x4[4]
            {
                rotIdentity * scIdentity * Matrix4x4.CreateTranslation(pos[0]),
                rotIdentity * scIdentity * Matrix4x4.CreateTranslation(pos[1]),
                rotIdentity * scIdentity * Matrix4x4.CreateTranslation(pos[2]),
                rotIdentity * scIdentity * Matrix4x4.CreateTranslation(pos[3])
            };
        }

        public Matrix4x4 CenterMatrix(bool floor)
        {
            var rotIdentity = Matrix4x4.CreateFromYawPitchRoll(0f, 0f, 0f);
            var scIdentity  = Matrix4x4.CreateScale(1.0f);
            return rotIdentity * scIdentity * Matrix4x4.CreateTranslation(Center(floor));
        }
    }
}
