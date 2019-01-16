using System;
using System.Xml.Serialization;

namespace TombLib.Wad
{
    public class WadAnimCommand : ICloneable
    {
        public class WadAnimCommandSoundXml
        {
            public WadSoundInfo SoundInfo { get; set; }
        }

        public WadAnimCommandType Type { get; set; }
        public short Parameter1 { get; set; }
        public short Parameter2 { get; set; }
        public short Parameter3 { get; set; }

        [XmlElement("SoundInfoName")]
        public string XmlSerializer_SoundInfoName { get; set; }
        [XmlElement("Sound")]
        public string XmlSerializer_Sound { get; set; }

        [XmlIgnore]
        public WadSoundInfo SoundInfo { get; set; }

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
                    if ((Parameter1 & 0x8000) != 0)
                        return "Play Sound ID = " + (SoundInfo != null ? "\"" + SoundInfo.Name + "\"" : (Parameter2 & 0x3FFF).ToString()) + " (water) on Frame = " + Parameter1;
                    else if ((Parameter1 & 0x8000) != 0)
                        return "Play Sound ID = " + (SoundInfo != null ? "\"" + SoundInfo.Name + "\"" : (Parameter2 & 0x3FFF).ToString()) + " (land) on Frame = " + Parameter1;
                    else
                        return "Play Sound ID = " + (SoundInfo != null ? "\"" + SoundInfo.Name + "\"" : (Parameter2 & 0x3FFF).ToString()) + " on Frame = " + Parameter1;
                case WadAnimCommandType.FlipEffect:
                    return "Play FlipEffect ID = " + Parameter2 + " on Frame = " + Parameter1;
            }

            return "";
        }

        public WadAnimCommand Clone() => (WadAnimCommand)MemberwiseClone();
        object ICloneable.Clone() => Clone();
    }
}
