namespace TombLib.Wad
{
    public enum WadGameVersion : long
    {
        TR1 = 1,
        TR2 = 2,
        TR3 = 3,
        TR4_TRNG = 4,
        TR5 = 5
    }

    public enum WadAnimCommandType : short
    {
        SetPosition = 1,
        SetJumpDistance = 2,
        EmptyHands = 3,
        KillEntity = 4,
        PlaySound = 5,
        FlipEffect = 6
    }

    public enum WadMeshLightingType : ushort
    {
        Normals,
        PrecalculatedGrayShades
    }
}
