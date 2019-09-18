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

        public bool FrameBased => Type >= WadAnimCommandType.PlaySound;
        public bool PositionBased => Type == WadAnimCommandType.SetPosition;
        public bool VelocityBased => Type == WadAnimCommandType.SetJumpDistance;

        // Only for old Wad2 importing
        public WadSoundInfo SoundInfoObsolete { get; set; }

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
                    if ((Parameter2 & 0x8000) != 0)
                        return "Play Sound ID = " + soundId + " (water) on Frame = " + Parameter1;
                    else if ((Parameter2 & 0x4000) != 0)
                        return "Play Sound ID = " + soundId + " (land) on Frame = " + Parameter1;
                    else
                        return "Play Sound ID = " + soundId + " on Frame = " + Parameter1;
                case WadAnimCommandType.FlipEffect:
                    int flipeffectId = Parameter2 & 0x3FFF;
                    if ((Parameter2 & 0x8000) != 0)
                        return "Play FlipEffect ID = " + flipeffectId + " (right foot) on Frame = " + Parameter1;
                    else if ((Parameter2 & 0x4000) != 0)
                        return "Play FlipEffect ID = " + flipeffectId + " (left foot) on Frame = " + Parameter1;
                    else
                        return "Play FlipEffect ID = " + flipeffectId + " on Frame = " + Parameter1;
            }

            return "";
        }

        public WadAnimCommand Clone() => (WadAnimCommand)MemberwiseClone();
        object ICloneable.Clone() => Clone();

        public static bool DistinctiveEquals(WadAnimCommand first, WadAnimCommand second, bool considerFrames)
        {
            if (ReferenceEquals(first, null) != ReferenceEquals(second, null))
                return false;
            else if ((ReferenceEquals(first, null) == true) && (ReferenceEquals(second, null) == true))
                return true;

            if (first.Type != second.Type) return false;

            if (first.FrameBased)
            {
                return ((!considerFrames || first.Parameter1 == second.Parameter1) &&
                                            first.Parameter2 == second.Parameter2);
            }
            else if (first.VelocityBased)
            {
                return (first.Parameter1 == second.Parameter1 &&
                        first.Parameter2 == second.Parameter2);
            }
            else if (first.PositionBased)
            {
                return (first.Parameter1 == second.Parameter1 &&
                        first.Parameter2 == second.Parameter2 &&
                        first.Parameter3 == second.Parameter3);
            }
            else
                return true; // Equal or unknown command
        }

        public static bool operator ==(WadAnimCommand first, WadAnimCommand second) => DistinctiveEquals(first, second, true);
        public static bool operator !=(WadAnimCommand first, WadAnimCommand second) => !(first == second);

        public bool Equals(WadAnimCommand other) => this == other;
        public override bool Equals(object other) => other is WadAnimCommand && this == (WadAnimCommand)other;
        public override int GetHashCode() => ("t" + Type + "p1" + Parameter1 + "p2" + Parameter2 + "p3" + Parameter3).GetHashCode();
    }
}
