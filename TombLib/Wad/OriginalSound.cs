using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public class OriginalSound
    {
        public string Name { get; set; }
        public List<string> Samples { get; set; }
        public short Volume { get; set; }
        public bool FlagP { get; set; }
        public bool FlagV { get; set; }
        public bool FlagR { get; set; }
        public bool FlagL { get; set; }
        public bool FlagN { get; set; }
        public ushort Flags { get; set; }
        public short Chance { get; set; }
        public short Pitch { get; set; }
        public short Range { get; set; }
        public List<string> WadLetters { get; set; }
        public bool MandatorySound { get; set; }
        public bool Unused { get; set; }

        public OriginalSound()
        {
            Samples = new List<string>();
            WadLetters = new List<string>();
        }
    }
}
