using System.Numerics;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;

namespace TombLib.LevelData
{
    public class GhostBlockInstance : SectorBasedObjectInstance, ISpatial, ICopyable
    {
        public SectorSurface Floor;
        public SectorSurface Ceiling;

        public bool SelectedFloor = true;
        public SectorEdge? SelectedCorner { get; set; }

        public VectorInt2 SectorPosition
        {
            get { return new VectorInt2(Area.X0, Area.Y0); }
            set { Area = new RectangleInt2(value.X, value.Y, value.X, value.Y); }
        }

        public Sector Sector => Room.Sectors[Area.X0, Area.Y0];
        public bool FloorIsQuad => Sector.Floor.IsQuad;
        public bool CeilingIsQuad => Sector.Ceiling.IsQuad;
        public bool FloorSplitToggled => Sector.Floor.SplitDirectionIsXEqualsZ;
        public bool CeilingSplitToggled => Sector.Ceiling.SplitDirectionIsXEqualsZ;

        public bool Editable => !Sector.IsAnyWall;
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
            float
                floorXnZp = Floor.XnZp / (float)Level.FullClickHeight,
                floorXpZp = Floor.XpZp / (float)Level.FullClickHeight,
                floorXpZn = Floor.XpZn / (float)Level.FullClickHeight,
                floorXnZn = Floor.XnZn / (float)Level.FullClickHeight,
                ceilingXnZp = Ceiling.XnZp / (float)Level.FullClickHeight,
                ceilingXpZp = Ceiling.XpZp / (float)Level.FullClickHeight,
                ceilingXpZn = Ceiling.XpZn / (float)Level.FullClickHeight,
                ceilingXnZn = Ceiling.XnZn / (float)Level.FullClickHeight;

            return "floor: (" + floorXnZp + ", " + floorXpZp + ", " + floorXpZn + ", " + floorXnZn + ")\n" +
                   "ceiling: (" + ceilingXnZp + ", " + ceilingXpZp + ", " + ceilingXpZn + ", " + ceilingXnZn + ")";
        }

        public void Move(int delta, bool? forceSurface = null)
        {
            if (SelectedCorner.HasValue)
                Move(SelectedCorner.Value, delta, forceSurface.HasValue ? forceSurface.Value : SelectedFloor);
            else
                for (int i = 0; i < 4; i++)
                    Move((SectorEdge)(i), delta, forceSurface.HasValue ? forceSurface.Value : SelectedFloor);
        }

        private void Move (SectorEdge edge, int delta, bool floor)
        {
            if (floor)
                switch (edge)
                {
                    case SectorEdge.XnZn: Floor.XnZn += delta; break;
                    case SectorEdge.XnZp: Floor.XnZp += delta; break;
                    case SectorEdge.XpZn: Floor.XpZn += delta; break;
                    case SectorEdge.XpZp: Floor.XpZp += delta; break;
                }
            else
                switch (edge)
                {
                    case SectorEdge.XnZn: Ceiling.XnZn += delta; break;
                    case SectorEdge.XnZp: Ceiling.XnZp += delta; break;
                    case SectorEdge.XpZn: Ceiling.XpZn += delta; break;
                    case SectorEdge.XpZp: Ceiling.XpZp += delta; break;
                }
        }

        public Vector3[] ControlPositions(bool floor, bool original = false, float margin = -16.0f)
        {
            var result = new Vector3[4];

            var localCenter = new Vector3(SectorPosition.X * Level.SectorSizeUnit + Level.HalfSectorSizeUnit, 0, SectorPosition.Y * Level.SectorSizeUnit + Level.HalfSectorSizeUnit);
            var type = floor ? SectorVerticalPart.QA : SectorVerticalPart.WS;

            var hXnZn = Sector.GetHeight(type, SectorEdge.XnZn) + (original ? 0 : (floor ? Floor : Ceiling).XnZn) + (floor ? -margin : margin);
            var hXpZp = Sector.GetHeight(type, SectorEdge.XpZp) + (original ? 0 : (floor ? Floor : Ceiling).XpZp) + (floor ? -margin : margin);
            var hXnZp = Sector.GetHeight(type, SectorEdge.XnZp) + (original ? 0 : (floor ? Floor : Ceiling).XnZp) + (floor ? -margin : margin);
            var hXpZn = Sector.GetHeight(type, SectorEdge.XpZn) + (original ? 0 : (floor ? Floor : Ceiling).XpZn) + (floor ? -margin : margin);

            var shift = new Vector3();

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: shift.X = -Level.HalfSectorSizeUnit - margin; shift.Y = hXnZp; shift.Z =  Level.HalfSectorSizeUnit + margin; break;
                    case 1: shift.X =  Level.HalfSectorSizeUnit + margin; shift.Y = hXpZp; shift.Z =  Level.HalfSectorSizeUnit + margin; break;
                    case 2: shift.X =  Level.HalfSectorSizeUnit + margin; shift.Y = hXpZn; shift.Z = -Level.HalfSectorSizeUnit - margin; break;
                    case 3: shift.X = -Level.HalfSectorSizeUnit - margin; shift.Y = hXnZn; shift.Z = -Level.HalfSectorSizeUnit - margin; break;
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
