using System;

namespace TombLib.LevelData
{
    public class SinkInstance : PositionAndScriptBasedObjectInstance, IReplaceable
    {
        public short Strength { get; set; }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Sink with strength " + (Strength + 1) +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                GetScriptIDOrName(false);
        }

        public string ShortName() => "Sink (Strength = " + (Strength + 1 ) + ")" + GetScriptIDOrName();

        public string PrimaryAttribDesc => "Strength";
        public string SecondaryAttribDesc => string.Empty;

        public bool ReplaceableEquals(IReplaceable other, bool withProperties = false)
        {
            return (other is SinkInstance && (other as SinkInstance)?.Strength == Strength);
        }

        public bool Replace(IReplaceable other, bool withProperties)
        {
            var thatSink = (SinkInstance)other;
            if (!ReplaceableEquals(other) && Strength != thatSink?.Strength)
            {
                Strength = thatSink.Strength;
                return true;
            }
            else
                return false;
        }
    }
}
