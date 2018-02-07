using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public enum WadTombRaiderVersion : short
    {
        TR1,
        TR2,
        TR3,
        TR4,
        TRNG,
        TR5
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

    public enum WadSoundLoopType : byte
    {
        None = 0,
        W = 1,
        R = 2,
        L = 3
    }
}
