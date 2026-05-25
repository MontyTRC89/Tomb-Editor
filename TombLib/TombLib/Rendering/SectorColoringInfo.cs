using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;

namespace TombLib.Rendering
{
    public enum SectorColoringType
    {
        Wall,
        Trigger,
        NotWalkableFloor,
        Box,
        Monkey,
        Death,
        Climb,
        Portal,
        FloorPortal,
        CeilingPortal,
        BorderWall,
        Floor,
        Ceiling,
        ForceFloorSolid,
        PortalEffect,
        Beetle,
        TriggerTriggerer
    }

    public enum SectorColoringShape
    {
        Rectangle,
        TriangleXpZp,
        TriangleXpZn,
        TriangleXnZp,
        TriangleXnZn,
        Hatch,
        Frame,
        EdgeXn,
        EdgeXp,
        EdgeZp,
        EdgeZn,
    }

    public struct SectorColoringInfoColor
    {
        public SectorColoringShape Shape;
        public Vector4 Color;
    }

    public class ColorScheme
    {
        public Vector4 ColorSelection;
        public Vector4 ColorIllegalSlope;
        public Vector4 ColorSlideDirection;
        public Vector4 Color3DBackground;
        public Vector4 Color2DBackground;
        public Vector4 ColorFlipRoom;
        public Vector4 ColorPortal;
        public Vector4 ColorPortalFace;
        public Vector4 ColorPortalEffect;
        public Vector4 ColorFloor;
        public Vector4 ColorBorderWall;
        public Vector4 ColorWall;
        public Vector4 ColorWallLower;
        public Vector4 ColorWallUpper;
        public Vector4 ColorTrigger;
        public Vector4 ColorMonkey;
        public Vector4 ColorClimb;
        public Vector4 ColorBox;
        public Vector4 ColorDeath;
        public Vector4 ColorNotWalkable;
        public Vector4 ColorBeetle;
        public Vector4 ColorTriggerTriggerer;
        public Vector4 ColorForceSolidFloor;
        public Vector4 Color2DRoomsAbove;
        public Vector4 Color2DRoomsBelow;
        public Vector4 Color2DRoomsMoved;
        public Vector4[] CustomColors = new Vector4[16];

        public static readonly ColorScheme Default = new ColorScheme()
        {
            ColorSelection        = new Vector4(255, 0, 0, 255) / 255.0f,
            ColorIllegalSlope     = new Vector4(255, 132, 0, 255) / 255.0f,
            ColorSlideDirection   = new Vector4(160, 64, 190, 255) / 255.0f,
            Color3DBackground     = new Vector4(165, 165, 165, 255) / 255.0f,
            Color2DBackground     = new Vector4(255, 255, 255, 255) / 255.0f,
            ColorFlipRoom         = new Vector4(35, 35, 35, 255) / 255.0f,
            ColorPortal           = new Vector4(0, 0, 0, 255) / 255.0f,
            ColorPortalFace       = new Vector4(255, 255, 0, 255) / 255.0f,
            ColorPortalEffect     = new Vector4(100, 170, 255, 255) / 255.0f,
            ColorFloor            = new Vector4(0, 190, 190, 255) / 255.0f,
            ColorBorderWall       = new Vector4(128, 128, 128, 255) / 255.0f,
            ColorWall             = new Vector4(0, 160, 0, 255) / 255.0f,
            ColorWallLower        = new Vector4(0, 80, 0, 255) / 255.0f,
            ColorWallUpper        = new Vector4(0, 240, 0, 255) / 255.0f,
            ColorTrigger          = new Vector4(200, 0, 200, 255) / 255.0f,
            ColorMonkey           = new Vector4(255, 100, 100, 255) / 255.0f,
            ColorClimb            = new Vector4(255, 180, 180, 255) / 255.0f,
            ColorBox              = new Vector4(100, 100, 100, 255) / 255.0f,
            ColorDeath            = new Vector4(20, 240, 20, 255) / 255.0f,
            ColorNotWalkable      = new Vector4(0, 0, 150, 255) / 255.0f,
            ColorBeetle           = new Vector4(100, 100, 100, 255) / 255.0f,
            ColorTriggerTriggerer = new Vector4(0, 0, 252, 255) / 255.0f,
            ColorForceSolidFloor  = new Vector4(0, 170, 170, 255) / 255.0f,
            Color2DRoomsAbove     = new Vector4(50, 50, 200, 255) / 255.0f,
            Color2DRoomsBelow     = new Vector4(85, 85, 85, 255) / 255.0f,
            Color2DRoomsMoved     = new Vector4(230, 230, 20, 255) / 255.0f,
        };

        public static readonly ColorScheme Gray = new ColorScheme()
        {
            ColorSelection        = new Vector4(201, 173, 173, 255) / 255.0f,
            ColorIllegalSlope     = new Vector4(157, 150, 145, 255) / 255.0f,
            ColorSlideDirection   = new Vector4(185, 187, 196, 255) / 255.0f,
            Color3DBackground     = new Vector4(82, 82, 82, 255) / 255.0f,
            Color2DBackground     = new Vector4(120, 120, 120, 255) / 255.0f,
            ColorFlipRoom         = new Vector4(35, 35, 35, 255) / 255.0f,
            ColorPortal           = new Vector4(0, 0, 0, 255) / 255.0f,
            ColorPortalFace       = new Vector4(172, 172, 172, 255) / 255.0f,
            ColorPortalEffect     = new Vector4(180, 180, 180, 255) / 255.0f,
            ColorFloor            = new Vector4(144, 144, 144, 255) / 255.0f,
            ColorBorderWall       = new Vector4(128, 128, 128, 255) / 255.0f,
            ColorWall             = new Vector4(105, 105, 105, 255) / 255.0f,
            ColorWallLower        = new Vector4(77, 77, 77, 255) / 255.0f,
            ColorWallUpper        = new Vector4(78, 78, 78, 255) / 255.0f,
            ColorTrigger          = new Vector4(124, 101, 122, 255) / 255.0f,
            ColorMonkey           = new Vector4(209, 201, 197, 255) / 255.0f,
            ColorClimb            = new Vector4(205, 194, 194, 255) / 255.0f,
            ColorBox              = new Vector4(100, 100, 100, 255) / 255.0f,
            ColorDeath            = new Vector4(136, 123, 123, 255) / 255.0f,
            ColorNotWalkable      = new Vector4(169, 168, 179, 255) / 255.0f,
            ColorBeetle           = new Vector4(100, 100, 100, 255) / 255.0f,
            ColorTriggerTriggerer = new Vector4(217, 220, 221, 255) / 255.0f,
            ColorForceSolidFloor  = new Vector4(134, 145, 145, 255) / 255.0f,
            Color2DRoomsAbove     = new Vector4(122, 120, 131, 255) / 255.0f,
            Color2DRoomsBelow     = new Vector4(86, 86, 88, 255) / 255.0f,
            Color2DRoomsMoved     = new Vector4(216, 216, 216, 255) / 255.0f
        };

        public static readonly ColorScheme Pastel = new ColorScheme()
        {
            ColorSelection         = new Vector4(255, 120, 120, 255) / 255.0f,
            ColorIllegalSlope      = new Vector4(255, 171, 79, 255) / 255.0f,
            ColorSlideDirection    = new Vector4(206, 158, 222, 255) / 255.0f,
            Color3DBackground      = new Vector4(120, 120, 120, 255) / 255.0f,
            Color2DBackground      = new Vector4(172, 172, 172, 255) / 255.0f,
            ColorFlipRoom          = new Vector4(35, 35, 35, 255) / 255.0f,
            ColorPortal            = new Vector4(0, 0, 0, 255) / 255.0f,
            ColorPortalFace        = new Vector4(247, 236, 98, 255) / 255.0f,
            ColorPortalEffect      = new Vector4(170, 200, 255, 255) / 255.0f,
            ColorFloor             = new Vector4(121, 185, 219, 255) / 255.0f,
            ColorBorderWall        = new Vector4(128, 128, 128, 255) / 255.0f,
            ColorWall              = new Vector4(137, 189, 125, 255) / 255.0f,
            ColorWallLower         = new Vector4(91, 164, 81, 255) / 255.0f,
            ColorWallUpper         = new Vector4(158, 219, 155, 255) / 255.0f,
            ColorTrigger           = new Vector4(216, 175, 210, 255) / 255.0f,
            ColorMonkey            = new Vector4(255, 187, 151, 255) / 255.0f,
            ColorClimb             = new Vector4(255, 153, 145, 255) / 255.0f,
            ColorBox               = new Vector4(100, 100, 100, 255) / 255.0f,
            ColorDeath             = new Vector4(209, 50, 54, 255) / 255.0f,
            ColorNotWalkable       = new Vector4(91, 91, 255, 255) / 255.0f,
            ColorBeetle            = new Vector4(100, 100, 100, 255) / 255.0f,
            ColorTriggerTriggerer  = new Vector4(186, 228, 252, 255) / 255.0f,
            ColorForceSolidFloor   = new Vector4(0, 170, 170, 255) / 255.0f,
            Color2DRoomsAbove      = new Vector4(50, 50, 200, 255) / 255.0f,
            Color2DRoomsBelow      = new Vector4(85, 85, 85, 255) / 255.0f,
            Color2DRoomsMoved      = new Vector4(230, 230, 20, 255) / 255.0f
        };

        public static readonly ColorScheme Dark = new ColorScheme()
        {
            ColorSelection         = new Vector4(102, 38, 38, 255) / 255.0f,
            ColorIllegalSlope      = new Vector4(122, 86, 18, 255) / 255.0f,
            ColorSlideDirection    = new Vector4(88, 35, 105, 255) / 255.0f,
            Color3DBackground      = new Vector4(72, 72, 72, 255) / 255.0f,
            Color2DBackground      = new Vector4(82, 82, 82, 255) / 255.0f,
            ColorFlipRoom          = new Vector4(31, 31, 31, 255) / 255.0f,
            ColorPortal            = new Vector4(0, 0, 0, 255) / 255.0f,
            ColorPortalFace        = new Vector4(125, 115, 72, 255) / 255.0f,
            ColorPortalEffect      = new Vector4(10, 100, 200, 255) / 255.0f,
            ColorFloor             = new Vector4(0, 66, 66, 255) / 255.0f,
            ColorBorderWall        = new Vector4(78, 78, 78, 255) / 255.0f,
            ColorWall              = new Vector4(52, 88, 37, 255) / 255.0f,
            ColorWallLower         = new Vector4(58, 79, 53, 255) / 255.0f,
            ColorWallUpper         = new Vector4(40, 67, 35, 255) / 255.0f,
            ColorTrigger           = new Vector4(79, 54, 84, 255) / 255.0f,
            ColorMonkey            = new Vector4(126, 56, 56, 255) / 255.0f,
            ColorClimb             = new Vector4(132, 34, 34, 255) / 255.0f,
            ColorBox               = new Vector4(69, 69, 69, 255) / 255.0f,
            ColorDeath             = new Vector4(69, 120, 7, 255) / 255.0f,
            ColorNotWalkable       = new Vector4(38, 30, 68, 255) / 255.0f,
            ColorBeetle            = new Vector4(87, 87, 87, 255) / 255.0f,
            ColorTriggerTriggerer  = new Vector4(83, 79, 138, 255) / 255.0f,
            ColorForceSolidFloor   = new Vector4(0, 121, 121, 255) / 255.0f,
            Color2DRoomsAbove      = new Vector4(41, 41, 88, 255) / 255.0f,
            Color2DRoomsBelow      = new Vector4(44, 44, 44, 255) / 255.0f,
            Color2DRoomsMoved      = new Vector4(94, 92, 57, 255) / 255.0f,
        };
    }

    public class SectorColoringInfo
    {
        public static readonly SectorColoringInfo Default = new SectorColoringInfo(ColorScheme.Default);
        public ColorScheme SectorColorScheme;

        public List<SectorColoringType> CurrentPriority = Enum.GetValues(typeof(SectorColoringType)).Cast<SectorColoringType>().ToList();
        public List<SectorColoringType> PreviousPriority = Enum.GetValues(typeof(SectorColoringType)).Cast<SectorColoringType>().ToList();
        public float TransitionValue = 0.0f;

        private static readonly List<SectorColoringShape> _allShapes = Enum.GetValues(typeof(SectorColoringShape)).Cast<SectorColoringShape>().ToList();

        public SectorColoringInfo(ColorScheme colorScheme)
        {
            SectorColorScheme = colorScheme;
        }

        private Vector4? GetSectorColoringInfoColor(ColorScheme colorScheme, List<SectorColoringType> priorityList, Room room, int x, int z, bool probeThroughPortals, SectorColoringShape shape, HashSet<SectorColoringType> typesToIgnore = null)
        {
            Sector sector = room.GetSectorTry(x, z);
            if (sector == null)
                return null;

            bool checkIgnored = typesToIgnore != null;

            Sector bottomSector = room.ProbeLowestSector(x, z, probeThroughPortals).Sector ?? Sector.Empty;
            for (int i = 0; i < priorityList.Count; i++)
            {
                if (checkIgnored && typesToIgnore.Contains(priorityList[i]))
                    continue;

                switch (shape)
                {
                    case SectorColoringShape.Rectangle:
                        switch (priorityList[i])
                        {
                            case SectorColoringType.Trigger:
                                if (bottomSector.Triggers.Count != 0)
                                    return colorScheme.ColorTrigger;
                                break;
                            case SectorColoringType.NotWalkableFloor:
                                if (bottomSector.HasFlag(SectorFlags.NotWalkableFloor))
                                    return colorScheme.ColorNotWalkable;
                                break;
                            case SectorColoringType.Box:
                                if (bottomSector.HasFlag(SectorFlags.Box))
                                    return colorScheme.ColorBox;
                                break;
                            case SectorColoringType.Monkey:
                                if (bottomSector.HasFlag(SectorFlags.Monkey))
                                    return colorScheme.ColorMonkey;
                                break;
                            case SectorColoringType.Death:
                                if (bottomSector.HasFlag(SectorFlags.DeathFire) ||
                                    bottomSector.HasFlag(SectorFlags.DeathElectricity) ||
                                    bottomSector.HasFlag(SectorFlags.DeathLava))
                                    return colorScheme.ColorDeath;
                                break;
                            case SectorColoringType.BorderWall:
                                if (sector.Type == SectorType.BorderWall)
                                    return colorScheme.ColorBorderWall;
                                break;
                            case SectorColoringType.Wall:
                                if (sector.Type == SectorType.Wall && sector.Floor.DiagonalSplit == DiagonalSplit.None)
                                    return colorScheme.ColorWall;
                                break;
                            case SectorColoringType.Floor:
                                if (!(sector.Floor.DiagonalSplit == DiagonalSplit.None && sector.IsAnyWall))
                                    return colorScheme.ColorFloor;
                                break;
                            case SectorColoringType.Ceiling:
                                if (!(sector.Ceiling.DiagonalSplit == DiagonalSplit.None && sector.IsAnyWall))
                                    return colorScheme.ColorFloor;
                                break;
                            case SectorColoringType.Portal:
                                if((room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType != Room.RoomConnectionType.NoPortal) ||
                                   (room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType != Room.RoomConnectionType.NoPortal) ||
                                   (sector.WallPortal != null && sector.WallPortal.IsTraversable))
                                    return colorScheme.ColorPortal;
                                break;
                            case SectorColoringType.FloorPortal:
                                if (sector.FloorPortal != null && sector.FloorPortal.IsTraversable &&
                                    room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.FullPortal)
                                    return colorScheme.ColorPortalFace;
                                break;
                            case SectorColoringType.CeilingPortal:
                                if (sector.CeilingPortal != null && sector.CeilingPortal.IsTraversable &&
                                    room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.FullPortal)
                                    return colorScheme.ColorPortalFace;
                                break;
                        }
                        break;
                    case SectorColoringShape.Frame:
                        switch (priorityList[i])
                        {
                            case SectorColoringType.Climb:
                                if (bottomSector.HasFlag(SectorFlags.ClimbAny))
                                    return colorScheme.ColorClimb;
                                break;
                            case SectorColoringType.Beetle:
                                if (bottomSector.HasFlag(SectorFlags.Beetle))
                                    return colorScheme.ColorBeetle;
                                break;
                            case SectorColoringType.TriggerTriggerer:
                                if (bottomSector.HasFlag(SectorFlags.TriggerTriggerer))
                                    return colorScheme.ColorTriggerTriggerer;
                                break;
                            case SectorColoringType.PortalEffect:
                                if (!room.Level.IsTombEngine)
                                    break;
                                if ((sector.WallPortal != null && sector.WallPortal.Effect == PortalEffectType.ClassicMirror) ||
                                    (sector.FloorPortal != null && sector.FloorPortal.Effect == PortalEffectType.ClassicMirror) ||
                                    (sector.CeilingPortal != null && sector.CeilingPortal.Effect == PortalEffectType.ClassicMirror))
                                    return colorScheme.ColorPortalEffect;
                                break;
                        }
                        break;
                    case SectorColoringShape.Hatch:
                        switch (priorityList[i])
                        {
                            case SectorColoringType.ForceFloorSolid:
                                if (sector.ForceFloorSolid)
                                    return colorScheme.ColorForceSolidFloor;
                                break;

                            case SectorColoringType.Portal:
                                {
                                    var floorInfo = room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true);
                                    var ceilingInfo = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true);

                                    if ((floorInfo.Portal   != null && floorInfo.VisualType   == Room.RoomConnectionType.NoPortal) ||
                                        (ceilingInfo.Portal != null && ceilingInfo.VisualType == Room.RoomConnectionType.NoPortal) ||
                                        (sector.WallPortal != null && !sector.WallPortal.IsTraversable))
                                        return colorScheme.ColorPortal;
                                }
                                break;
                        }
                        break;
                    case SectorColoringShape.TriangleXnZn:
                        switch (priorityList[i])
                        {
                            case SectorColoringType.Wall:
                                if (sector.Type == SectorType.Wall && sector.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                                    return colorScheme.ColorWall;
                                break;
                            case SectorColoringType.FloorPortal:
                                if (sector.FloorPortal != null && sector.FloorPortal.IsTraversable &&
                                    room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.TriangularPortalXpZp)
                                    return colorScheme.ColorFloor;
                                break;
                            case SectorColoringType.CeilingPortal:
                                if (sector.CeilingPortal != null && sector.CeilingPortal.IsTraversable &&
                                    room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.TriangularPortalXpZp)
                                    return colorScheme.ColorFloor;
                                break;
                        }
                        break;
                    case SectorColoringShape.TriangleXnZp:
                        switch (priorityList[i])
                        {
                            case SectorColoringType.Wall:
                                if (sector.Type == SectorType.Wall && sector.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                                    return colorScheme.ColorWall;
                                break;
                            case SectorColoringType.FloorPortal:
                                if (sector.FloorPortal != null && sector.FloorPortal.IsTraversable &&
                                    room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.TriangularPortalXpZn)
                                    return colorScheme.ColorFloor;
                                break;
                            case SectorColoringType.CeilingPortal:
                                if (sector.CeilingPortal != null && sector.CeilingPortal.IsTraversable &&
                                    room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.TriangularPortalXpZn)
                                    return colorScheme.ColorFloor;
                                break;
                        }
                        break;
                    case SectorColoringShape.TriangleXpZn:
                        switch (priorityList[i])
                        {
                            case SectorColoringType.Wall:
                                if (sector.Type == SectorType.Wall && sector.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                                    return colorScheme.ColorWall;
                                break;
                            case SectorColoringType.FloorPortal:
                                if (sector.FloorPortal != null && sector.FloorPortal.IsTraversable &&
                                    room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.TriangularPortalXnZp)
                                    return colorScheme.ColorFloor;
                                break;
                            case SectorColoringType.CeilingPortal:
                                if (sector.CeilingPortal != null && sector.CeilingPortal.IsTraversable &&
                                    room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.TriangularPortalXnZp)
                                    return colorScheme.ColorFloor;
                                break;
                        }
                        break;
                    case SectorColoringShape.TriangleXpZp:
                        switch (priorityList[i])
                        {
                            case SectorColoringType.Wall:
                                if (sector.Type == SectorType.Wall && sector.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                                    return colorScheme.ColorWall;
                                break;
                            case SectorColoringType.FloorPortal:
                                if (sector.FloorPortal != null && sector.FloorPortal.IsTraversable &&
                                    room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.TriangularPortalXnZn)
                                    return colorScheme.ColorFloor;
                                break;
                            case SectorColoringType.CeilingPortal:
                                if (sector.CeilingPortal != null && sector.CeilingPortal.IsTraversable &&
                                    room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType == Room.RoomConnectionType.TriangularPortalXnZn)
                                    return colorScheme.ColorFloor;
                                break;
                        }
                        break;
                    case SectorColoringShape.EdgeXn:
                    case SectorColoringShape.EdgeXp:
                    case SectorColoringShape.EdgeZp:
                    case SectorColoringShape.EdgeZn:
                        if (!bottomSector.HasFlag(SectorFlags.ClimbAny))
                            if (shape == SectorColoringShape.EdgeXn && bottomSector.HasFlag(SectorFlags.ClimbNegativeX) ||
                                shape == SectorColoringShape.EdgeXp && bottomSector.HasFlag(SectorFlags.ClimbPositiveX) ||
                                shape == SectorColoringShape.EdgeZp && bottomSector.HasFlag(SectorFlags.ClimbPositiveZ) ||
                                shape == SectorColoringShape.EdgeZn && bottomSector.HasFlag(SectorFlags.ClimbNegativeZ))
                                return colorScheme.ColorClimb;
                        break;
                    default:
                        break;
                }
            }
            return null;
        }

        public List<SectorColoringInfoColor> GetColors(ColorScheme colorScheme, Room room, int x, int z, bool probeThroughPortals, HashSet<SectorColoringType> typesToIgnore = null, List<SectorColoringShape> shapesToList = null)
        {
            List<SectorColoringInfoColor> colors = null;
            if (shapesToList == null)
                shapesToList = _allShapes;
            for (int i = 0; i < shapesToList.Count; i++)
            {
                Vector4? currColor = GetSectorColoringInfoColor(colorScheme, CurrentPriority, room, x, z, probeThroughPortals, shapesToList[i], typesToIgnore);
                Vector4? prevColor = GetSectorColoringInfoColor(colorScheme, PreviousPriority, room, x, z, probeThroughPortals, shapesToList[i], typesToIgnore);
                if (!prevColor.HasValue || !currColor.HasValue)
                    continue;

                if (colors == null)
                    colors = new List<SectorColoringInfoColor>();
                colors.Add(new SectorColoringInfoColor() { Color = Vector4.Lerp(prevColor.Value, currColor.Value, TransitionValue), Shape = shapesToList[i] });
            }
            return colors;
        }

        public bool SetPriority(SectorColoringType type, bool fast = true)
        {
            if (!CurrentPriority.Contains(type) || CurrentPriority.First() == type)
                return false;
            PreviousPriority = CurrentPriority;
            CurrentPriority = new[] { type }.Concat(PreviousPriority.Where(c => c != type)).ToList();
            TransitionValue = fast ? 1.0f : 0.0f;
            return true;
        }
    };

}
