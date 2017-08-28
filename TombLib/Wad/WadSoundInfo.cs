using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public struct WadSoundInfo
    {
        public string WaveSound;
        public byte Volume;
        public byte Range;
        public byte Chance;
        public byte Pitch;
        public ushort Characteristics;
    }
}
