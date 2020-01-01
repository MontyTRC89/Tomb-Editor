﻿using System.Numerics;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public enum StaticMeshFlags : ushort
    {
        None = 0,
        DisableCollision = 4,
        GlassTrasparency = 8,
        IceTrasparency = 16,
        DamageLaraOnCollision = 32,
        BurnLaraOnCollision = 64,
        ExplodeKillingOnCollision = 128,
        PoisonLaraOnCollision = 256,
        HugeCollision = 512,
        HardShatter = 1024,
        EnableHeavyTriggerOnCollision = 2048,
        Scalable = 4096,
        SpecificShatter = 8192,
        All = 16380
    }

    public class StaticInstance : ItemInstance
    {
        // Don't use a reference here because the loaded Wad might not
        // contain each required object. Additionally the loaded Wads
        // can change. It would be unnecesary difficult to update all those references.
        public WadStaticId WadObjectId { get; set; }

        public Vector3 Color { get; set; } = new Vector3(1.0f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)

        public override ItemType ItemType => new ItemType(WadObjectId, Room?.Level?.Settings);
    }
}
