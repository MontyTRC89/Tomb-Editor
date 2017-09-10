namespace TombEditor.Geometry
{
    public class CameraInstance : PositionBasedObjectInstance, IHasScriptID
    {
        public ushort? ScriptId { get; set; }
        public ushort Sequence { get; set; }
        public short Timer { get; set; }
        public float Roll { get; set; }
        public ushort Number { get; set; }
        public float Speed { get; set; }
        public float Fov { get; set; }
        public ushort Flags { get; set; }
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
