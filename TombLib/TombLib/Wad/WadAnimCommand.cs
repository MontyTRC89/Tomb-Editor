using System;

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
        public bool VelocityBased => Type == WadAnimCommandType.SetJumpVelocity;

        public string Description => ToString();
        public override string ToString()
        {
            switch (Type)
            {
                case WadAnimCommandType.EmptyHands:
                    return "Remove guns from hands";
                case WadAnimCommandType.SetJumpVelocity:
                    return "Set jump reference <H, V> = <" + Parameter1 + ", " + Parameter2 + ">";
                case WadAnimCommandType.KillEntity:
                    return "Kill entity";
                case WadAnimCommandType.SetPosition:
                    return "Set position reference <X, Y, Z> = <" + Parameter1 + ", " + Parameter2 + ", " + Parameter3 + ">";
                case WadAnimCommandType.PlaySound:
                    return "Play Sound ID = " + Parameter2 + " (" + ((WadSoundEnvironmentType)Parameter3).ToString() + ") on Frame = " + Parameter1;

                case WadAnimCommandType.Flipeffect:
                    int flipeffectId = Parameter2 & 0x3FFF;
                    if ((Parameter2 & 0x8000) != 0)
                        return "Play FlipEffect ID = " + flipeffectId + " (right foot) on Frame = " + Parameter1;
                    else if ((Parameter2 & 0x4000) != 0)
                        return "Play FlipEffect ID = " + flipeffectId + " (left foot) on Frame = " + Parameter1;
                    else
                        return "Play FlipEffect ID = " + flipeffectId + " on Frame = " + Parameter1;

                case WadAnimCommandType.DisableInterpolation:
                    return "Disable interpolation on Frame = " + Parameter1;
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
                                            first.Parameter2 == second.Parameter2 &&
                                            first.Parameter3 == second.Parameter3);
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

        public void ConvertEnvironmentType()
        {
            if (Type != WadAnimCommandType.PlaySound)
                throw new Exception("Attempt to convert sound environment type on a non-PlaySound animcommand");

            switch (Parameter2 & 0xF000)
            {
                default:
                case 0:
                    Parameter3 = (short)WadSoundEnvironmentType.Always;
                    break;

                case (1 << 14):
                    Parameter3 = (short)WadSoundEnvironmentType.Land;
                    break;

                case (1 << 15):
                    Parameter3 = (short)WadSoundEnvironmentType.Water;
                    break;

                case (1 << 12):
                    Parameter3 = (short)WadSoundEnvironmentType.Quicksand;
                    break;

                case (1 << 13):
                    Parameter3 = (short)WadSoundEnvironmentType.Underwater;
                    break;
            }

            Parameter2 = (short)(Parameter2 & 0x0FFF);
        }

        public static bool operator ==(WadAnimCommand first, WadAnimCommand second) => DistinctiveEquals(first, second, true);
        public static bool operator !=(WadAnimCommand first, WadAnimCommand second) => !(first == second);

        public bool Equals(WadAnimCommand other) => this == other;
        public override bool Equals(object other) => other is WadAnimCommand && this == (WadAnimCommand)other;
        public override int GetHashCode() => ("t" + Type + "p1" + Parameter1 + "p2" + Parameter2 + "p3" + Parameter3).GetHashCode();
    }
}
