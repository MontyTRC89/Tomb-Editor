namespace TombLib.LevelData
{
    public enum CameraInstanceMode : byte
    {
        Default = 0,
        Locked = 1,
        Sniper = 2
    }

    public class CameraInstance : PositionAndScriptBasedObjectInstance
    {
        public CameraInstanceMode CameraMode { get; set; }
        public byte MoveTimer { get; set; }
        public bool GlideOut { get; set; }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Camera " +
                (CameraMode == CameraInstanceMode.Locked ? "Locked" : (CameraMode == CameraInstanceMode.Sniper ? "Sniper" : "")) +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                GetScriptIDOrName(false);
        }

        public string ShortName() => (CameraMode == CameraInstanceMode.Locked ? "Locked camera" : (CameraMode == CameraInstanceMode.Sniper ? "Sniper camera" : "Camera")) + " (Room " + (Room?.ToString() ?? "NULL") + ")" + GetScriptIDOrName();
    }
}
