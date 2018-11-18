namespace TombLib.LevelData
{
    public class SinkInstance : PositionAndScriptBasedObjectInstance
    {
        public short Strength { get; set; }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Sink with strength " + Strength +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y;
        }

        public string ShortName() => "Sink" + (ScriptId.HasValue ? " <" + ScriptId.Value + ">" : "");
    }
}
