namespace TombLib.Wad
{
    public enum SoundSystem : long
    {
        Dynamic = 0,
        Xml = 1
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

    public enum WadLinkOpcode : ushort
    {
        NotUseStack = 0,
        Pop = 1,
        Push = 2,
        Read = 3
    }
}
