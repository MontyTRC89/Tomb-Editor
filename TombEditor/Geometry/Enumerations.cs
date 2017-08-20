using System;
using System.Diagnostics.CodeAnalysis;

namespace TombEditor.Geometry
{
    public enum LightParameter
    {
        Intensity,
        In,
        Out,
        Len,
        CutOff,
        DirectionX,
        DirectionY
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TextureTileType : byte
    {
        Rectangle,
        TriangleNW,
        TriangleNE,
        TriangleSE,
        TriangleSW
    }

    public enum AnimatexTextureSetEffect : byte
    {
        Normal,
        HalfRotate,
        FullRotate
    }

    public enum TextureSounds : byte
    {
        Mud = 0,
        Snow = 1,
        Sand = 2,
        Gravel = 3,
        Ice = 4,
        Water = 5,
        Stone = 6,
        Wood = 7,
        Metal = 8,
        Marble = 9,
        Grass = 10,
        Concrete = 11,
        OldWood = 12,
        OldMetal = 13
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum BlockFaces : byte
    {
        NorthQA = 0, SouthQA = 1, WestQA = 2, EastQA = 3, DiagonalQA = 4,
        NorthED = 5, SouthED = 6, WestED = 7, EastED = 8, DiagonalED = 9,
        NorthMiddle = 10, SouthMiddle = 11, WestMiddle = 12, EastMiddle = 13, DiagonalMiddle = 14,
        NorthWS = 15, SouthWS = 16, WestWS = 17, EastWS = 18, DiagonalWS = 19,
        NorthRF = 20, SouthRF = 21, WestRF = 22, EastRF = 23, DiagonalRF = 24,
        Floor = 25, FloorTriangle2 = 26, Ceiling = 27, CeilingTriangle2 = 28
    }

    public enum BlockFaceShape : byte
    {
        Rectangle, Triangle
    }
}
