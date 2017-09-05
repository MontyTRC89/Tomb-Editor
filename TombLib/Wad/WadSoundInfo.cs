using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public class WadSoundInfo
    {
        private List<WadSound> _sound;

        public List<WadSound> WaveSounds { get { return _sound; } }
        public byte Volume { get; set; }
        public byte Range { get; set; }
        public byte Chance { get; set; }
        public byte Pitch { get; set; }
        public bool FlagN { get; set; }
        public bool RandomizePitch { get; set; }
        public bool RandomizeGain { get; set; }

        public WadSoundInfo()
        {
            _sound = new List<WadSound>();
        }
    }
}
