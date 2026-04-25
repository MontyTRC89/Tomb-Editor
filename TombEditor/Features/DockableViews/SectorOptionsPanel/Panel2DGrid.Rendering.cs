#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

// Rendering.
public partial class Panel2DGrid
{
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        CoerceViewOffset();
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        var editor = _editor;
        var room = Room;

        if (editor is null || room is null)
        {
            DrawDesignPlaceholder(dc);
            return;
        }

        try
        {
            var totalArea = GetVisualAreaTotal();
            var roomArea = GetVisualAreaRoom();
            var gridDimensions = GetGridDimensions();
            double gridStep = GetGridStep();
            var roomSize = room.SectorSize;

            if (gridStep <= 0.0)
                return;

            var resources = new RenderResources();
            dc.DrawRectangle(resources.GetBrush(editor.Configuration.UI_ColorScheme.Color2DBackground), null, totalArea);

            for (int x = 0; x < roomSize.X; x++)
            {
                for (int z = 0; z < roomSize.Y; z++)
                {
                    var tileRect = new Rect(
                        roomArea.X + (x * gridStep),
                        roomArea.Y + ((roomSize.Y - 1 - z) * gridStep),
                        gridStep,
                        gridStep);

                    PaintSectorTile(dc, editor, room, resources, tileRect, x, z);
                }
            }

            DrawGridLines(dc, totalArea, gridDimensions);
            DrawSelection(dc, editor, room, resources);
        }
        catch (Exception exc)
        {
            logger.Error(exc, "An exception occurred while drawing the 2D grid.");
        }
    }

    private static void DrawGridLines(DrawingContext dc, Rect totalArea, VectorInt2 gridDimensions)
    {
        var gridArea = GetGridLineArea(totalArea);
        dc.DrawGridLines(GridPen, gridArea, gridDimensions.X, gridDimensions.Y);
        dc.DrawRectangleInside(BorderPen, totalArea);
    }

    private void DrawSelection(DrawingContext dc, Editor editor, Room room, RenderResources resources)
    {
        if (editor.SelectedSectors.Valid)
        {
            var selectionPen = resources.GetPen(editor.Configuration.UI_ColorScheme.ColorSelection, 2.0);
            dc.DrawRectangleInside(selectionPen, ToVisualCoord(editor.SelectedSectors.Area));
        }

        if (editor.SelectedObject is SectorBasedObjectInstance instance && instance.Room == room)
        {
            var pen = instance is PortalInstance ? SelectedPortalPen : SelectedTriggerPen;
            dc.DrawRectangleInside(pen, ToVisualCoord(instance.Area));
        }
    }

    private static void PaintSectorTile(DrawingContext dc, Editor editor, Room room, RenderResources resources, Rect sectorArea, int x, int z)
    {
        var coloringInfos = editor.SectorColoringManager.ColoringInfo.GetColors(
            editor.Configuration.UI_ColorScheme, room, x, z,
            editor.Configuration.UI_ProbeAttributesThroughPortals);

        if (coloringInfos is null)
            return;

        for (int i = 0; i < coloringInfos.Count; i++)
        {
            var info = coloringInfos[i];

            switch (info.Shape)
            {
                case SectorColoringShape.Rectangle:
                    dc.DrawRectangle(resources.GetBrush(info.Color), null, sectorArea);
                    break;

                case SectorColoringShape.Frame:
                    DrawFrame(dc, resources, sectorArea, info.Color);
                    break;

                case SectorColoringShape.Hatch:
                    DrawHatch(dc, resources, sectorArea, info.Color);
                    break;

                case SectorColoringShape.EdgeZp:
                case SectorColoringShape.EdgeZn:
                case SectorColoringShape.EdgeXp:
                case SectorColoringShape.EdgeXn:
                    DrawEdge(dc, sectorArea, info.Shape, resources.GetBrush(info.Color));
                    break;

                case SectorColoringShape.TriangleXnZn:
                case SectorColoringShape.TriangleXnZp:
                case SectorColoringShape.TriangleXpZn:
                case SectorColoringShape.TriangleXpZp:
                    DrawTriangle(dc, sectorArea, info.Shape, resources.GetBrush(info.Color));
                    break;
            }
        }
    }

    private static void DrawFrame(DrawingContext dc, RenderResources resources, Rect sectorArea, Vector4 color)
    {
        if (sectorArea.Width <= OutlineSectorColoringInfoWidth || sectorArea.Height <= OutlineSectorColoringInfoWidth)
            return;

        var pen = resources.GetPen(color, OutlineSectorColoringInfoWidth);
        dc.DrawRectangleInside(pen, sectorArea);
    }

    private static void DrawHatch(DrawingContext dc, RenderResources resources, Rect sectorArea, Vector4 color)
    {
        var pen = resources.GetPen(color, HatchPenThickness);
        var clip = new RectangleGeometry(sectorArea);
        clip.Freeze();

        dc.PushClip(clip);

        try
        {
            double startOffset = -sectorArea.Height / 2.0;
            double totalRange = sectorArea.Width + sectorArea.Height;
            double margin = (HatchPenThickness / 2.0) + 0.5;

            for (double offset = startOffset; offset < totalRange; offset += HatchSpacing)
            {
                dc.DrawLine(pen,
                    new Point(sectorArea.X - sectorArea.Height + offset - margin, sectorArea.Bottom + margin),
                    new Point(sectorArea.X + offset + margin, sectorArea.Top - margin));
            }
        }
        finally
        {
            dc.Pop();
        }
    }

    private static void DrawEdge(DrawingContext dc, Rect sectorArea, SectorColoringShape shape, Brush brush)
    {
        const double w = OutlineSectorColoringInfoWidth;

        var edgeRect = shape switch
        {
            SectorColoringShape.EdgeZp => new Rect(sectorArea.X, sectorArea.Y, sectorArea.Width, w),
            SectorColoringShape.EdgeZn => new Rect(sectorArea.X, sectorArea.Bottom - w, sectorArea.Width, w),
            SectorColoringShape.EdgeXp => new Rect(sectorArea.Right - w, sectorArea.Y, w, sectorArea.Height),
            _ => new Rect(sectorArea.X, sectorArea.Y, w, sectorArea.Height)
        };

        dc.DrawRectangle(brush, null, edgeRect);
    }

    private static void DrawTriangle(DrawingContext dc, Rect sectorArea, SectorColoringShape shape, Brush brush)
    {
        var (p0, p1, p2) = shape switch
        {
            SectorColoringShape.TriangleXnZn => (
                new Point(sectorArea.Left, sectorArea.Top),
                new Point(sectorArea.Left, sectorArea.Bottom),
                new Point(sectorArea.Right, sectorArea.Bottom)),
            SectorColoringShape.TriangleXnZp => (
                new Point(sectorArea.Left, sectorArea.Bottom),
                new Point(sectorArea.Left, sectorArea.Top),
                new Point(sectorArea.Right, sectorArea.Top)),
            SectorColoringShape.TriangleXpZn => (
                new Point(sectorArea.Left, sectorArea.Bottom),
                new Point(sectorArea.Right, sectorArea.Top),
                new Point(sectorArea.Right, sectorArea.Bottom)),
            _ => (
                new Point(sectorArea.Left, sectorArea.Top),
                new Point(sectorArea.Right, sectorArea.Top),
                new Point(sectorArea.Right, sectorArea.Bottom))
        };

        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(p0, true, true);
            ctx.LineTo(p1, false, false);
            ctx.LineTo(p2, false, false);
        }

        geometry.Freeze();
        dc.DrawGeometry(brush, null, geometry);
    }

    private void DrawDesignPlaceholder(DrawingContext dc)
    {
        dc.DrawRectangle(DesignPlaceholderBrush, BorderPen,
            new Rect(0.0, 0.0, ActualWidth, ActualHeight));

        dc.DrawCenteredText(
            "2D Grid",
            new Rect(0.0, 0.0, ActualWidth, ActualHeight),
            new Typeface("Segoe UI"),
            12.0,
            Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip,
            CultureInfo.InvariantCulture);
    }

    private sealed class RenderResources
    {
        private readonly Dictionary<Color, Brush> _brushes = [];
        private readonly Dictionary<PenKey, Pen> _pens = [];

        public Brush GetBrush(Vector4 color)
            => GetBrush(color.ToWPFColor());

        public Brush GetBrush(Color color)
        {
            if (_brushes.TryGetValue(color, out var brush))
                return brush;

            brush = BrushHelpers.CreateFrozenBrush(color);
            _brushes.Add(color, brush);
            return brush;
        }

        public Pen GetPen(Vector4 color, double thickness)
            => GetPen(color.ToWPFColor(), thickness);

        private Pen GetPen(Color color, double thickness)
        {
            var key = new PenKey(color, thickness);

            if (_pens.TryGetValue(key, out var pen))
                return pen;

            pen = BrushHelpers.CreateFrozenPen(color, thickness);
            _pens.Add(key, pen);
            return pen;
        }

        private readonly record struct PenKey(Color Color, double Thickness);
    }
}
