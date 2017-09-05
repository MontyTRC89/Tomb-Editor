using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public enum TombRaiderVersion : short
    {
        TR1,
        TR2,
        TR3,
        TR4,
        TR5,
        OpenLara,
        EdisonEngine,
        OpenTomb
    }

    public enum WadObjectType : short
    {
        OldMoveable,
        OldStaticMesh,
        Moveable,
        StaticMesh,
        Sprite
    }

    public enum WadLinkOpcode
    {
        NotUseStack = 0,
        Push = 1,
        Pop = 2,
        Read = 3
    }

    public enum WadKeyFrameRotationAxis : byte
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

    public enum WadPolygonShape : short
    {
        Rectangle = 0,
        Triangle = 1
    }
}
