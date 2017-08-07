using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry
{
    public abstract class SectorBasedObjectInstance : ObjectInstance
    {
        public byte X { get; set; }
        public byte Z { get; set; }
        public byte NumXBlocks { get; set; }
        public byte NumZBlocks { get; set; }

        public SectorBasedObjectInstance(int id, Room room)
            : base(id, room)
        { }

        public Rectangle Area
        {
            get { return new Rectangle(X, Z, X + NumXBlocks - 1, Z + NumZBlocks - 1); }
        }
    }
}
