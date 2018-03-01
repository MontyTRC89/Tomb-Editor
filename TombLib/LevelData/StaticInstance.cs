using System;
using System.Numerics;

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
        Scalable = 4096
    }

    public class StaticInstance : ItemInstance
    {
        public ushort Ocb { get; set; } = 0;
        public Vector4 Color { get; set; } = new Vector4(1.0f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)

        public override ItemType ItemType => new ItemType(true, WadObjectId, WadVersion);
    }
}
