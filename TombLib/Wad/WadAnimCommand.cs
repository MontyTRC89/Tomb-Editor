using System;
using System.Xml.Serialization;

namespace TombLib.Wad
{
    public class WadAnimCommand : ICloneable
    {
        public WadAnimCommandType Type { get; set; }
        public short Parameter1 { get; set; }
        public short Parameter2 { get; set; }
        public short Parameter3 { get; set; }

        public override string ToString()
        {
            switch (Type)
            {
                case WadAnimCommandType.EmptyHands:
                    return "Remove guns from hands";
                case WadAnimCommandType.SetJumpDistance:
                    return "Set jump reference <V, H> = <" + Parameter1 + ", " + Parameter2 + ">";
                case WadAnimCommandType.KillEntity:
                    return "Kill entity";
                case WadAnimCommandType.SetPosition:
                    return "Set position reference <X, Y, Z> = <" + Parameter1 + ", " + Parameter2 + ", " + Parameter3 + ">";
                case WadAnimCommandType.PlaySound:
                    int soundId = Parameter2 & 0x3FFF;
                    if ((Parameter1 & 0x8000) != 0)
                        return "Play Sound ID = " + soundId + " (water) on Frame = " + Parameter1;
                    else if ((Parameter1 & 0x8000) != 0)
                        return "Play Sound ID = " + soundId + " (land) on Frame = " + Parameter1;
                    else
                        return "Play Sound ID = " + soundId + " on Frame = " + Parameter1;
                case WadAnimCommandType.FlipEffect:
                    return "Play FlipEffect ID = " + Parameter2 + " on Frame = " + Parameter1;
            }

            return "";
        }

        public WadAnimCommand Clone() => (WadAnimCommand)MemberwiseClone();
        object ICloneable.Clone() => Clone();
    }
}
