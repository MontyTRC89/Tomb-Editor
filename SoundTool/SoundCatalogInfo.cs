using System.Collections.Generic;

namespace SoundTool
{
    public class SoundCatalogInfo
    {
        public string Name { get; set; }
        public List<string> Samples { get; set; }
        public short Volume { get; set; }
        public bool FlagP { get; set; }
        public bool FlagV { get; set; }
        public bool FlagR { get; set; }
        public bool FlagL { get; set; }
        public bool FlagN { get; set; }
        public bool FlagW { get; set; }
        public ushort Flags { get; set; }
        public short Chance { get; set; }
        public short Pitch { get; set; }
        public short Range { get; set; }
        public bool MandatorySound { get; set; }
        public bool Unused { get; set; }
        public bool NgLocked { get; set; }

        // Used internally
        internal List<string> WadLetters { get; private set; }

        public SoundCatalogInfo()
        {
            Samples = new List<string>();
            WadLetters = new List<string>();
        }
    }
}
