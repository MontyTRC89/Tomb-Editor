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

    public enum WadLinkOpcode : ushort
    {
        NotUseStack = 0,
        Push = 1,
        Pop = 2,
        Read = 3
    }

    public enum WadKeyFrameRotationAxis : short
    {
        ThreeAxes = 0,
        AxisX = 1,
        AxisY = 2,
        AxisZ = 3
    }

    public enum WadMeshShadeMode : short
    {
        Dynamic = 0,
        Static = 1
    }

    public enum WadAnimCommandType : short
    {
        PositionReference = 1,
        JumpReference = 2,
        EmptyHands = 3,
        KillEntity = 4,
        PlaySound = 5,
        FlipEffect = 6
    }

    public enum WadPolygonShape : ushort
    {
        Quad = 0,
        Triangle = 1
    }

    public enum WadChunkType : ushort
    {
        NoExtraChunk = 0xcdcd
    }

    public enum WadMeshLightingType : ushort
    {
        Normals,
        PrecalculatedGrayShades
    }
}
