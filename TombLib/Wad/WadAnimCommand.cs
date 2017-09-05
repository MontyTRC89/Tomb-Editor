using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public class WadAnimCommand
    {
        public WadAnimCommandType Type { get { return _type; } }
        public ushort Parameter1 { get { return _parameter1; } set { _parameter1 = value; } }
        public ushort Parameter2 { get { return _parameter2; } set { _parameter2 = value; } }
        public ushort Parameter3 { get { return _parameter3; } set { _parameter3 = value; } }

        private WadAnimCommandType _type;
        private ushort _parameter1;
        private ushort _parameter2;
        private ushort _parameter3;

        public WadAnimCommand(WadAnimCommandType type)
        {
            _type = type;
        }
    }
}
