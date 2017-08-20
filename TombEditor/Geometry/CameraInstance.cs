namespace TombEditor.Geometry
{
    public class CameraInstance : PositionBasedObjectInstance, IHasScriptID
    {
        public ushort? ScriptId { get; set; }
        public short Sequence { get; set; }
        public short Timer { get; set; }
        public short Roll { get; set; }
        public short Number { get; set; }
        public short Speed { get; set; }
        public short Fov { get; set; }
        public short Flags { get; set; }
        public bool Fixed { get; set; }
        
        public override bool CopyToFlipRooms => false;

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
        
        public override string ToString()
        {
            return "Camera " + (Fixed ? "Fixed" : "") +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Y = " + SectorPosition.Y +
                ", Z = " + SectorPosition.Z;
        }
    }
}
