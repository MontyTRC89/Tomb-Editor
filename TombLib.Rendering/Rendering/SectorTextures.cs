﻿using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.LevelData;

namespace TombLib.Rendering
{
    public enum SectorTexture
    {
        None,
        arrow_up,
        arrow_down,
        arrow_up_down,
        arrow_left,
        arrow_right,
        arrow_ne,
        arrow_nw,
        arrow_se,
        arrow_sw,
        arrow_ne_se,
        arrow_nw_se,
        arrow_nw_sw,
        arrow_sw_ne,
        cross,
        illegal_slope,
        slide_east,
        slide_east_flip,
        slide_north,
        slide_north_flip,
        slide_south,
        slide_south_flip,
        slide_west,
        slide_west_flip,
        texture_coord_out_of_bounds,
        texture_unavailable
    }

    public enum SelectionType
    {
        None,
        Selected,
        Highlight
    }

    public delegate SectorTextureResult SectorTextureGetDelegate(Room room, int x, int z, BlockFace face);

    public struct SectorTextureResult
    {
        public Vector4 Color;
        public Vector4 Overlay;
        public SectorTexture SectorTexture;
        public bool Selected;
        public bool Highlighted;
        public bool Dimmed;
        public bool Hidden;
    }

    public class SectorTextureDefault
    {
        public static SectorTextureDefault Default { get; } = new SectorTextureDefault();
        public SectorColoringInfo ColoringInfo = SectorColoringInfo.Default;
        public RectangleInt2 SelectionArea = new RectangleInt2(-1, -1, -1, -1);
        public RectangleInt2 HighlightArea = new RectangleInt2(-1, -1, -1, -1);
        public ArrowType SelectionArrow = ArrowType.EntireFace;
        public bool ProbeAttributesThroughPortals = true;
        public bool DrawIllegalSlopes = true;
        public bool DrawSlideDirections = true;
        public bool HideHiddenRooms = true;

        public static readonly HashSet<SectorColoringType> IgnoredHighlights = new HashSet<SectorColoringType>
        {
            SectorColoringType.Portal,
            SectorColoringType.BorderWall,
            SectorColoringType.Wall,
            SectorColoringType.Beetle,
            SectorColoringType.TriggerTriggerer
        };

        public static readonly HashSet<SectorColoringType> IgnoredHighlightsForFloor = new HashSet<SectorColoringType>
        {
            SectorColoringType.Portal,
            SectorColoringType.CeilingPortal,
            SectorColoringType.BorderWall,
            SectorColoringType.Wall,
            SectorColoringType.Beetle,
            SectorColoringType.TriggerTriggerer
        };

        public static readonly HashSet<SectorColoringType> IgnoredHighlightsForCeiling = new HashSet<SectorColoringType>
        {
            SectorColoringType.Portal,
            SectorColoringType.FloorPortal,
            SectorColoringType.BorderWall,
            SectorColoringType.Wall,
            SectorColoringType.Beetle,
            SectorColoringType.TriggerTriggerer
        };

        public static readonly List<SectorColoringShape> UsedShapes = new List<SectorColoringShape>
        {
            SectorColoringShape.Rectangle
        };

        public SectorTextureResult Get(Room room, int x, int z, BlockFace face)
        {
            SectorTexture SectorTexture = SectorTexture.None;
            Vector4 Color;
            Vector4 Overlay = new Vector4();
            bool Dimmed = false;

            // Choose base color
            switch (face)
            {
                case BlockFace.PositiveZ_QA:
                case BlockFace.NegativeZ_QA:
                case BlockFace.NegativeX_QA:
                case BlockFace.PositiveX_QA:
                case BlockFace.DiagonalQA:
                case BlockFace.PositiveZ_ED:
                case BlockFace.NegativeZ_ED:
                case BlockFace.NegativeX_ED:
                case BlockFace.PositiveX_ED:
                case BlockFace.DiagonalED:
                    Color = ColoringInfo.SectorColorScheme.ColorWallLower;
                    if (room.Blocks[x, z].WallPortal != null)
                        Color = ColoringInfo.SectorColorScheme.ColorPortalFace;
                    break;

                case BlockFace.PositiveZ_Middle:
                case BlockFace.NegativeZ_Middle:
                case BlockFace.NegativeX_Middle:
                case BlockFace.PositiveX_Middle:
                case BlockFace.DiagonalMiddle:
                    Color = ColoringInfo.SectorColorScheme.ColorWall;
                    if (room.Blocks[x, z].WallPortal != null)
                        Color = ColoringInfo.SectorColorScheme.ColorPortalFace;
                    break;

                case BlockFace.PositiveZ_WS:
                case BlockFace.NegativeZ_WS:
                case BlockFace.NegativeX_WS:
                case BlockFace.PositiveX_WS:
                case BlockFace.DiagonalWS:
                case BlockFace.PositiveZ_RF:
                case BlockFace.NegativeZ_RF:
                case BlockFace.NegativeX_RF:
                case BlockFace.PositiveX_RF:
                case BlockFace.DiagonalRF:
                    Color = ColoringInfo.SectorColorScheme.ColorWallUpper;
                    if (room.Blocks[x, z].WallPortal != null)
                        Color = ColoringInfo.SectorColorScheme.ColorPortalFace;
                    break;

                case BlockFace.Floor:
                case BlockFace.FloorTriangle2:
                    // For now, we only render rectangular solid highlights, so use single rectangle solid shape in UsedShapes list, and use first and only entry in returned highlight list.
                    var currentHighlights = ColoringInfo.GetColors(ColoringInfo.SectorColorScheme, room, x, z, ProbeAttributesThroughPortals, IgnoredHighlightsForFloor, UsedShapes);
                    if (currentHighlights != null)
                        Color = currentHighlights[0].Color;
                    else
                        Color = ColoringInfo.SectorColorScheme.ColorFloor;

                    if (room.Blocks[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
                    {
                        if ((room.Blocks[x, z].Floor.DiagonalSplit > DiagonalSplit.XpZp && face == BlockFace.Floor) ||
                            (room.Blocks[x, z].Floor.DiagonalSplit <= DiagonalSplit.XpZp && face == BlockFace.FloorTriangle2))
                            Dimmed = true;
                    }
                    break;

                case BlockFace.Ceiling:
                case BlockFace.CeilingTriangle2:
                    // For now, we only render rectangular solid highlights, so use single rectangle solid shape in UsedShapes list, and use first and only entry in returned highlight list.
                    var currentHighlights2 = ColoringInfo.GetColors(ColoringInfo.SectorColorScheme, room, x, z, ProbeAttributesThroughPortals, IgnoredHighlightsForCeiling, UsedShapes);
                    if (currentHighlights2 != null)
                        Color = currentHighlights2[0].Color;
                    else
                        Color = ColoringInfo.SectorColorScheme.ColorFloor;

                    if (room.Blocks[x, z].Ceiling.DiagonalSplit != DiagonalSplit.None)
                    {
                        if ((room.Blocks[x, z].Ceiling.DiagonalSplit > DiagonalSplit.XpZp && face == BlockFace.Ceiling) ||
                            (room.Blocks[x, z].Ceiling.DiagonalSplit <= DiagonalSplit.XpZp && face == BlockFace.CeilingTriangle2))
                            Dimmed = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown BlockFlag encountered.");
            }

            // Draw climbable walls
            switch (face)
            {
                case BlockFace.PositiveX_ED:
                case BlockFace.PositiveX_Middle:
                case BlockFace.PositiveX_QA:
                case BlockFace.PositiveX_RF:
                case BlockFace.PositiveX_WS:
                    {
                        var lookupBlock = room.ProbeLowestBlock(x + 1, z, ProbeAttributesThroughPortals);
                        if (lookupBlock.Block != null && lookupBlock.Block.HasFlag(BlockFlags.ClimbNegativeX))
                            Color = ColoringInfo.SectorColorScheme.ColorClimb;
                        break;
                    }
                case BlockFace.NegativeX_ED:
                case BlockFace.NegativeX_Middle:
                case BlockFace.NegativeX_QA:
                case BlockFace.NegativeX_RF:
                case BlockFace.NegativeX_WS:
                    {
                        var lookupBlock = room.ProbeLowestBlock(x - 1, z, ProbeAttributesThroughPortals);
                        if (lookupBlock.Block != null && lookupBlock.Block.HasFlag(BlockFlags.ClimbPositiveX))
                            Color = ColoringInfo.SectorColorScheme.ColorClimb;
                        break;
                    }
                case BlockFace.NegativeZ_ED:
                case BlockFace.NegativeZ_Middle:
                case BlockFace.NegativeZ_QA:
                case BlockFace.NegativeZ_RF:
                case BlockFace.NegativeZ_WS:
                    {
                        var lookupBlock = room.ProbeLowestBlock(x, z - 1, ProbeAttributesThroughPortals);
                        if (lookupBlock.Block != null && lookupBlock.Block.HasFlag(BlockFlags.ClimbPositiveZ))
                            Color = ColoringInfo.SectorColorScheme.ColorClimb;
                        break;
                    }
                case BlockFace.PositiveZ_ED:
                case BlockFace.PositiveZ_Middle:
                case BlockFace.PositiveZ_QA:
                case BlockFace.PositiveZ_RF:
                case BlockFace.PositiveZ_WS:
                    {
                        var lookupBlock = room.ProbeLowestBlock(x, z + 1, ProbeAttributesThroughPortals);
                        if (lookupBlock.Block != null && lookupBlock.Block.HasFlag(BlockFlags.ClimbNegativeZ))
                            Color = ColoringInfo.SectorColorScheme.ColorClimb;
                        break;
                    }
            }

            // Draw slopes
            if (DrawSlideDirections)
                if (face == BlockFace.Floor || face == BlockFace.FloorTriangle2)
                {
                    var slopeDirection = room.Blocks[x, z].GetFloorTriangleSlopeDirections()[face == BlockFace.Floor ? 0 : 1];
                    bool flipped = room.Blocks[x, z].Floor.SplitDirectionIsXEqualsZ;
                    switch (slopeDirection)
                    {
                        case Direction.PositiveX:
                            SectorTexture = flipped ? SectorTexture.slide_east_flip : SectorTexture.slide_east;
                            break;
                        case Direction.NegativeX:
                            SectorTexture = flipped ? SectorTexture.slide_west_flip : SectorTexture.slide_west;
                            break;
                        case Direction.PositiveZ:
                            SectorTexture = flipped ? SectorTexture.slide_north_flip : SectorTexture.slide_north;
                            break;
                        case Direction.NegativeZ:
                            SectorTexture = flipped ? SectorTexture.slide_south_flip : SectorTexture.slide_south;
                            break;
                    }

                    if(slopeDirection != Direction.None)
                        Overlay = ColoringInfo.SectorColorScheme.ColorSlideDirection;
                }

            // Draw illegal slopes
            if (DrawIllegalSlopes)
                if (face == BlockFace.Floor || face == BlockFace.FloorTriangle2)
                    if (room.IsIllegalSlope(x, z))
                    {
                        SectorTexture = SectorTexture.illegal_slope;
                        Overlay = ColoringInfo.SectorColorScheme.ColorIllegalSlope;
                    }

            // Draw selection
            if (SelectionArea.Contains(new VectorInt2(x, z)))
            {
                SectorTexture = SectorTexture.None;
                Color = ColoringInfo.SectorColorScheme.ColorSelection; // Selection color
                Overlay = Color; // Overlay is the same as color if sector is selected

                switch (face)
                {
                    case BlockFace.Floor:
                    case BlockFace.FloorTriangle2:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_up; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_right; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_down; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_left; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_sw; break;
                        }
                        break;

                    case BlockFace.Ceiling:
                    case BlockFace.CeilingTriangle2:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_up; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_right; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_down; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_left; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_sw; break;
                        }
                        break;

                    // South faces ------------------------------------------------------------------------------
                    case BlockFace.NegativeZ_QA:
                    case BlockFace.NegativeZ_ED:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_up; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_nw; break;
                        }
                        break;

                    case BlockFace.NegativeZ_WS:
                    case BlockFace.NegativeZ_RF:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_down; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_sw; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_sw; break;
                        }
                        break;

                    case BlockFace.NegativeZ_Middle:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_ne_se; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_up_down; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_ne_se; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                        }
                        break;

                    // East faces ------------------------------------------------------------------------------
                    case BlockFace.NegativeX_QA:
                    case BlockFace.NegativeX_ED:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_up; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_ne; break;
                        }
                        break;

                    case BlockFace.NegativeX_WS:
                    case BlockFace.NegativeX_RF:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_sw; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_down; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_sw; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_se; break;
                        }
                        break;

                    case BlockFace.NegativeX_Middle:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_nw_sw; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_ne_se; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_up_down; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_ne_se; break;
                        }
                        break;

                    // North faces ------------------------------------------------------------------------------
                    case BlockFace.PositiveZ_QA:
                    case BlockFace.PositiveZ_ED:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_up; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                        }
                        break;

                    case BlockFace.PositiveZ_WS:
                    case BlockFace.PositiveZ_RF:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_down; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_sw; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_sw; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                        }
                        break;

                    case BlockFace.PositiveZ_Middle:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_up_down; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_ne_se; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_ne_se; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                        }
                        break;

                    // West faces ------------------------------------------------------------------------------
                    case BlockFace.PositiveX_QA:
                    case BlockFace.PositiveX_ED:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_up; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_nw; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                        }
                        break;

                    case BlockFace.PositiveX_WS:
                    case BlockFace.PositiveX_RF:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_down; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_sw; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_se; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_sw; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                        }
                        break;

                    case BlockFace.PositiveX_Middle:
                        switch (SelectionArrow)
                        {
                            case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_ne_se; break;
                            case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_up_down; break;
                            case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_nw_sw; break;
                            case ArrowType.EdgeW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                            case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne_se; break;
                            case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                            case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                        }
                        break;
                }
            }

            return new SectorTextureResult
            {
                Color = Color,
                Overlay = Overlay,
                SectorTexture = SectorTexture,
                Dimmed = Dimmed,
                Hidden = room.Hidden && HideHiddenRooms,
                Selected = (SelectionArea.Contains(new VectorInt2(x, z))),
                Highlighted = (HighlightArea.Contains(new VectorInt2(x, z)))
            };
        }
    }
}
