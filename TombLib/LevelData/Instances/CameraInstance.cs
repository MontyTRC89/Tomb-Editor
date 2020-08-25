namespace TombLib.LevelData
{
    public class CameraInstance : PositionAndScriptBasedObjectInstance
    {
        public bool Fixed { get; set; }
        public byte MoveTimer { get; set; }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Camera " + (Fixed ? "Locked" : "") +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                (ScriptId.HasValue ? ", ScriptId = " + ScriptId.Value : "");
        }

        public string ShortName() => (Fixed ? "Locked camera" : "Camera") + " (Room " + Room?.ToString() ?? "NULL" + ")" + 
            (ScriptId.HasValue ? " <" + ScriptId.Value + ">" : "");
    }
}
