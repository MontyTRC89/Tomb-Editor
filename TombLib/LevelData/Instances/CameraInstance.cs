namespace TombLib.LevelData
{
    public class CameraInstance : PositionAndScriptBasedObjectInstance
    {
        public bool Fixed { get; set; }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Camera " + (Fixed ? "Fixed" : "") +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                (ScriptId.HasValue ? ", ScriptId = " + ScriptId.Value : "");
        }

        public string ShortName() => (Fixed ? "Fixed camera" : "Camera") + (ScriptId.HasValue ? " <" + ScriptId.Value + ">" : "");
    }
}
