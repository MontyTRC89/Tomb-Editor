using System;
using System.Diagnostics.CodeAnalysis;

namespace TombEditor.Geometry
{
    public enum PickingElementType : int
    {
        None = 0,
        Block = 1,
        Moveable = 2,
        StaticMesh = 3,
        Light = 4,
        Camera = 5,
        SoundSource = 6,
        FogBulb = 7,
        CollisionBlock = 8,
        Path = 9,
        Sink = 10,
        Portal = 11,
        Trigger = 12,
        FlyByCamera = 13
    }

    public struct PickingResult
    {
        public PickingElementType ElementType { get; set; }
        public int SubElementType { get; set; }
        public int Element { get; set; }
        public int SubElement { get; set; }
        public bool Gizmo { get; set; }
        public GizmoAxis GizmoAxis { get; set; }
    }

    public enum GizmoAxis : byte
    {
        None,
        X,
        Y,
        Z
    }

    public enum BlockType : byte
    {
        Floor, Wall, BorderWall
    }

    public enum LightType : byte
    {
        Light, Shadow, Spot, Effect, Sun, FogBulb
    }

    public enum PortalDirection : byte
    {
        Floor, Ceiling, North, South, East, West
    }

    public enum Reverberation : byte
    {
        Outside, SmallRoom, MediumRoom, LargeRoom, Pipe
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

    public enum TriggerType : byte
    {
        Trigger = 0,
        Pad = 1,
        Key = 2,
        Pickup = 3,
        Condition = 4,
        Heavy = 5,
        Dummy = 6,
        Switch = 7,
        Antipad = 8,
        Combat = 9,
        Antitrigger = 10,
        HeavySwitch = 11,
        HeavyAntritrigger = 12,
        Monkey = 13
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

    public enum TriggerTargetType : byte
    {
        Object = 0,
        Camera = 1,
        Sink = 2,
        FlipEffect = 3,
        FlipOn = 4,
        FlipOff = 5,
        Target = 6,
        FlipMap = 7,
        FinishLevel = 8,
        Secret = 9,
        Variable = 10,
        PlayAudio = 11,
        FlyByCamera = 12,
        CutsceneOrParameterNg = 13,
        Fmv = 14
    }

    [Flags]
    public enum BlockFlags : short
    {
        None = 0,
        Monkey = 1,
        Opacity2 = 2,
        Trigger = 4,
        Box = 8,
        Death = 16,
        Lava = 32,
        Electricity = 64,
        Opacity = 128,
        Beetle = 256,
        TriggerTriggerer = 512,
        NotWalkableFloor = 1024
    }

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
    public enum SplitType : byte
    {
        None, NWtoSE, NEtoSW
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum DiagonalSplit : byte
    {
        None = 0,
        NW = 1,
        NE = 2,
        SE = 3,
        SW = 4
    }

    public enum DiagonalSplitType : byte
    {
        None = 0,
        Floor = 1,
        Ceiling = 2,
        FloorAndCeiling = 3,
        Wall = 4
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

    public enum PortalOpacity : byte
    {
        None, Opacity1, Opacity2, Water
    }

    public enum FaceDirection : byte
    {
        North, South, East, West, DiagonalFloor, DiagonalCeiling, DiagonalWall
    }
}
