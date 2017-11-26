using System;
using TombLib.Wad;

namespace TombEditor.Geometry
{
    public class MoveableInstance : ItemInstance
    {
        public short Ocb { get; set; } = 0;
        public bool Invisible { get; set; } = false;
        public bool ClearBody { get; set; } = false;
        public byte CodeBits { get; set; } = 0; // Only the lower 5 bits are used.

        public override bool CopyToFlipRooms => false;
        public override ItemType ItemType => new ItemType(false, WadObjectId);
    }
}
