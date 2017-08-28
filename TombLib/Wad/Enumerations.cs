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

    public enum WadTexturePageType : byte
    {
        Shared = 0,
        Private = 1
    }

    public enum WadLinkOpcode
    {
        NotUseStack = 0,
        Push = 1,
        Pop = 2,
        Read = 3
    }

    public enum WadChunks : short
    {

    }
}
