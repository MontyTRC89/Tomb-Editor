using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.LevelData;

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
        public Vector4 ColorSelection = new Vector4(255, 0, 0, 255) / 255.0f;
        public Vector4 ColorIllegalSlope = new Vector4(255, 132, 0, 255) / 255.0f;
        public Vector4 ColorSlideDirection = new Vector4(160, 64, 190, 255) / 255.0f;
        public Vector4 Color3DBackground = new Vector4(165, 165, 165, 255) / 255.0f;
        public Vector4 Color2DBackground = new Vector4(255, 255, 255, 255) / 255.0f;
        public Vector4 ColorFlipRoom = new Vector4(35, 35, 35, 255) / 255.0f;
        public Vector4 ColorPortal = new Vector4(0, 0, 0, 255) / 255.0f;
        public Vector4 ColorPortalFace = new Vector4(255, 255, 0, 255) / 255.0f;
        public Vector4 ColorFloor = new Vector4(0, 190, 190, 255) / 255.0f;
        public Vector4 ColorBorderWall = new Vector4(128, 128, 128, 255) / 255.0f;
        public Vector4 ColorWall = new Vector4(0, 160, 0, 255) / 255.0f;
        public Vector4 ColorWallLower = new Vector4(0, 80, 0, 255) / 255.0f;
        public Vector4 ColorWallUpper = new Vector4(0, 240, 0, 255) / 255.0f;
        public Vector4 ColorTrigger = new Vector4(200, 0, 200, 255) / 255.0f;
        public Vector4 ColorMonkey = new Vector4(255, 100, 100, 255) / 255.0f;
        public Vector4 ColorClimb = new Vector4(255, 180, 180, 255) / 255.0f;
        public Vector4 ColorBox = new Vector4(100, 100, 100, 255) / 255.0f;
        public Vector4 ColorDeath = new Vector4(20, 240, 20, 255) / 255.0f;
        public Vector4 ColorNotWalkable = new Vector4(0, 0, 150, 255) / 255.0f;
        public Vector4 ColorBeetle = new Vector4(100, 100, 100, 255) / 255.0f;
        public Vector4 ColorTriggerTriggerer = new Vector4(0, 0, 252, 255) / 255.0f;
        public Vector4 ColorForceSolidFloor = new Vector4(0, 170, 170, 255) / 255.0f;
        public Vector4 Color2DRoomsAbove = new Vector4(50, 50, 200, 255) / 255.0f;
        public Vector4 Color2DRoomsBelow = new Vector4(85, 85, 85, 255) / 255.0f;
        public Vector4 Color2DRoomsMoved = new Vector4(230, 230, 20, 255) / 255.0f;
    }

    public class SectorColoringInfo
    {
        public static readonly SectorColoringInfo Default = new SectorColoringInfo(new ColorScheme());
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
            Block block = room.GetBlockTry(x, z);
            if (block == null)
                return null;

            bool checkIgnored = typesToIgnore != null;

            Block bottomBlock = room.ProbeLowestBlock(x, z, probeThroughPortals).Block ?? Block.Empty;
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
                                if (bottomBlock.Triggers.Count != 0)
                                    return colorScheme.ColorTrigger;
                                break;
                            case SectorColoringType.NotWalkableFloor:
                                if (bottomBlock.HasFlag(BlockFlags.NotWalkableFloor))
                                    return colorScheme.ColorNotWalkable;
                                break;
                            case SectorColoringType.Box:
                                if (bottomBlock.HasFlag(BlockFlags.Box))
                                    return colorScheme.ColorBox;
                                break;
                            case SectorColoringType.Monkey:
                                if (bottomBlock.HasFlag(BlockFlags.Monkey))
                                    return colorScheme.ColorMonkey;
                                break;
                            case SectorColoringType.Death:
                                if (bottomBlock.HasFlag(BlockFlags.DeathFire) ||
                                    bottomBlock.HasFlag(BlockFlags.DeathElectricity) ||
                                    bottomBlock.HasFlag(BlockFlags.DeathLava))
                                    return colorScheme.ColorDeath;
                                break;
                            case SectorColoringType.BorderWall:
                                if (block.Type == BlockType.BorderWall)
                                    return colorScheme.ColorBorderWall;
                                break;
                            case SectorColoringType.Wall:
                                if (block.Type == BlockType.Wall && block.Floor.DiagonalSplit == DiagonalSplit.None)
                                    return colorScheme.ColorWall;
                                break;
                            case SectorColoringType.Floor:
                                if (!(block.Floor.DiagonalSplit == DiagonalSplit.None && block.IsAnyWall) && block.FloorPortal == null)
                                    return colorScheme.ColorFloor;
                                break;
                            case SectorColoringType.Ceiling:
                                if (!(block.Ceiling.DiagonalSplit == DiagonalSplit.None && block.IsAnyWall) && block.CeilingPortal == null)
                                    return colorScheme.ColorFloor;
                                break;
                            case SectorColoringType.Portal:
                                if((room.GetFloorRoomConnectionInfo(new VectorInt2(x, z)).TraversableType != Room.RoomConnectionType.NoPortal) ||
                                   (room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z)).TraversableType != Room.RoomConnectionType.NoPortal) ||
                                   (block.WallPortal != null && block.WallPortal.IsTraversable))
                                    return colorScheme.ColorPortal;
                                break;
                            case SectorColoringType.FloorPortal:
                                if (block.FloorPortal != null && block.FloorPortal.IsTraversable)
                                    return colorScheme.ColorPortalFace;
                                break;
                            case SectorColoringType.CeilingPortal:
                                if (block.CeilingPortal != null && block.CeilingPortal.IsTraversable)
                                    return colorScheme.ColorPortalFace;
                                break;
                        }
                        break;
                    case SectorColoringShape.Frame:
                        switch (priorityList[i])
                        {
                            case SectorColoringType.Climb:
                                if (bottomBlock.HasFlag(BlockFlags.ClimbAny))
                                    return colorScheme.ColorClimb;
                                break;
                            case SectorColoringType.Beetle:
                                if (bottomBlock.HasFlag(BlockFlags.Beetle))
                                    return colorScheme.ColorBeetle;
                                break;
                            case SectorColoringType.TriggerTriggerer:
                                if (bottomBlock.HasFlag(BlockFlags.TriggerTriggerer))
                                    return colorScheme.ColorTriggerTriggerer;
                                break;
                        }
                        break;
                    case SectorColoringShape.Hatch:
                        if (block.ForceFloorSolid)
                            return colorScheme.ColorForceSolidFloor;
                        break;
                    case SectorColoringShape.TriangleXnZn:
                        if (block.Type == BlockType.Wall && block.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                            return colorScheme.ColorWall;
                        break;
                    case SectorColoringShape.TriangleXnZp:
                        if (block.Type == BlockType.Wall && block.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            return colorScheme.ColorWall;
                        break;
                    case SectorColoringShape.TriangleXpZn:
                        if (block.Type == BlockType.Wall && block.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            return colorScheme.ColorWall;
                        break;
                    case SectorColoringShape.TriangleXpZp:
                        if (block.Type == BlockType.Wall && block.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                            return colorScheme.ColorWall;
                        break;
                    case SectorColoringShape.EdgeXn:
                    case SectorColoringShape.EdgeXp:
                    case SectorColoringShape.EdgeZp:
                    case SectorColoringShape.EdgeZn:
                        if (!bottomBlock.HasFlag(BlockFlags.ClimbAny))
                            if (shape == SectorColoringShape.EdgeXn && bottomBlock.HasFlag(BlockFlags.ClimbNegativeX) ||
                                shape == SectorColoringShape.EdgeXp && bottomBlock.HasFlag(BlockFlags.ClimbPositiveX) ||
                                shape == SectorColoringShape.EdgeZp && bottomBlock.HasFlag(BlockFlags.ClimbPositiveZ) ||
                                shape == SectorColoringShape.EdgeZn && bottomBlock.HasFlag(BlockFlags.ClimbNegativeZ))
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
