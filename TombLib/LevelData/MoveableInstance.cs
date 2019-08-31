using TombLib.Wad;

namespace TombLib.LevelData
{
    public class MoveableInstance : ItemInstance
    {
        // Don't use a reference here because the loaded Wad might not
        // contain each required object. Additionally the loaded Wads
        // can change. It would be unnecesary difficult to update all those references.
        public WadMoveableId WadObjectId { get; set; }

        public short Ocb { get; set; } = 0;
        public bool Invisible { get; set; } = false;
        public bool ClearBody { get; set; } = false;
        public byte CodeBits { get; set; } = 0; // Only the lower 5 bits are used.

        public override bool CopyToAlternateRooms => false;
        public override ItemType ItemType => new ItemType(WadObjectId, Room?.Level?.Settings);
    }
}
