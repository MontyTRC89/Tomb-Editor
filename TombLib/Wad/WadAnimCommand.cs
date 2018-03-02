using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public struct WadAnimCommand
    {
        public WadAnimCommandType Type { get; set; }
        public WadSoundInfo SoundInfo { get; set; }
        public ushort Parameter1 { get; set; }
        public ushort Parameter2 { get; set; }
        public ushort Parameter3 { get; set; }
    }
}
