#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

public class Panel2DGrid : FrameworkElement, IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const double OutlineSectorColoringInfoWidth = 3.0;
    private const double BorderThickness = 1.0;
    private const double HatchPenThickness = 3.5;
    private const double HatchSpacing = 8.0;

    private static readonly Pen GridPen = WPFUtils.CreateFrozenPen(Color.FromArgb(140, 0, 0, 0), BorderThickness);
    private static readonly Pen BorderPen = WPFUtils.CreateFrozenPen(Colors.Black, BorderThickness);
    private static readonly Pen SelectedPortalPen = WPFUtils.CreateFrozenPen(Colors.YellowGreen, 2.0);
    private static readonly Pen SelectedTriggerPen = WPFUtils.CreateFrozenPen(Colors.White, 2.0);
    private static readonly Brush DesignPlaceholderBrush = WPFUtils.CreateFrozenBrush(Color.FromRgb(50, 50, 50));

    private readonly Editor? _editor;
    private Room? _room;
    private bool _doSectorSelection;
    private bool _disposed;
    private ToolTip? _selectionToolTip;

    public Room? Room
    {
        get => _room;
        set
        {
            if (_room == value)
                return;

            _room = value;
            InvalidateVisual();
        }
    }

    public Panel2DGrid()
    {
        ClipToBounds = true;
        Focusable = true;
        SnapsToDevicePixels = true;
        UseLayoutRounding = true;

        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        _editor = Editor.Instance;
        _editor.EditorEventRaised += EditorEventRaised;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_editor is not null)
            _editor.EditorEventRaised -= EditorEventRaised;

        CloseSelectionToolTip();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void EditorEventRaised(IEditorEvent obj)
    {
        if (_disposed)
            return;

        if (obj is SectorColoringManager.ChangeSectorColoringInfoEvent
            or Editor.GameVersionChangedEvent
            or Editor.SelectedSectorsChangedEvent
            or Editor.RoomSectorPropertiesChangedEvent
            or Editor.ObjectChangedEvent
            or Editor.RoomGeometryChangedEvent
            or Editor.ConfigurationChangedEvent
            || (obj is Editor.SelectedObjectChangedEvent e && IsObjectChangeRelevant(e)))
        {
            InvalidateVisual();
        }

        if (obj is Editor.ActionChangedEvent actionChanged)
        {
            bool isCameraRelocate = actionChanged.Current is EditorActionRelocateCamera;
            Cursor = isCameraRelocate ? Cursors.Cross : Cursors.Arrow;
        }
    }

    private static bool IsObjectChangeRelevant(Editor.SelectedObjectChangedEvent e)
        => e.Previous is SectorBasedObjectInstance || e.Current is SectorBasedObjectInstance;

    // Grid coordinate calculations.

    private VectorInt2 RoomSize => Room?.SectorSize ?? new VectorInt2(Room.DefaultRoomDimensions, Room.DefaultRoomDimensions);

    private VectorInt2 GetGridDimensions()
        => VectorInt2.Max(RoomSize, new VectorInt2(Room.DefaultRoomDimensions, Room.DefaultRoomDimensions));

    private double GetGridStep()
    {
        var gridDimensions = GetGridDimensions();
        double w = Math.Max(0.0, ActualWidth - BorderThickness);
        double h = Math.Max(0.0, ActualHeight - BorderThickness);

        if (w <= 0.0 || h <= 0.0)
            return 0.0;

        if (w * gridDimensions.Y < h * gridDimensions.X)
            return w / gridDimensions.X;
        else
            return h / gridDimensions.Y;
    }

    private Rect GetVisualAreaTotal()
    {
        double w = ActualWidth;
        double h = ActualHeight;
        var gridDimensions = GetGridDimensions();
        double gridStep = GetGridStep();
        double gridW = (gridDimensions.X * gridStep) + BorderThickness;
        double gridH = (gridDimensions.Y * gridStep) + BorderThickness;

        return new Rect(
            (w - gridW) * 0.5,
            (h - gridH) * 0.5,
            gridW,
            gridH);
    }

    private Rect GetVisualAreaRoom()
    {
        var totalArea = GetGridLineArea(GetVisualAreaTotal());
        double gridStep = GetGridStep();
        var gridDimensions = GetGridDimensions();
        var roomSize = RoomSize;

        return new Rect(
            totalArea.X + (gridStep * ((gridDimensions.X - roomSize.X) / 2)),
            totalArea.Y + (gridStep * ((gridDimensions.Y - roomSize.Y) / 2)),
            gridStep * roomSize.X,
            gridStep * roomSize.Y);
    }

    private Point ToVisualCoord(VectorInt2 sectorCoord)
    {
        var roomArea = GetVisualAreaRoom();
        double gridStep = GetGridStep();

        return new Point(
            (sectorCoord.X * gridStep) + roomArea.X,
            roomArea.Bottom - ((sectorCoord.Y + 1) * gridStep));
    }

    private Rect ToVisualCoord(RectangleInt2 sectorArea)
    {
        var p0 = ToVisualCoord(sectorArea.Start);
        var p1 = ToVisualCoord(sectorArea.End);
        double gridStep = GetGridStep();

        return new Rect(
            Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y),
            Math.Abs(p1.X - p0.X) + gridStep, Math.Abs(p1.Y - p0.Y) + gridStep);
    }

    private bool TryGetSectorFromVisualCoord(Point point, out VectorInt2 sectorCoord)
    {
        sectorCoord = default;

        var room = Room;

        if (room is null)
            return false;

        double gridStep = GetGridStep();

        if (gridStep <= 0.0)
            return false;

        var roomArea = GetVisualAreaRoom();
        var roomSize = room.SectorSize;

        sectorCoord = new VectorInt2(
            (int)Math.Max(0, Math.Min(roomSize.X - 1, (point.X - roomArea.X) / gridStep)),
            (int)Math.Max(0, Math.Min(roomSize.Y - 1, (roomArea.Bottom - point.Y) / gridStep)));

        return true;
    }

    // Mouse interaction.

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();

        var position = e.GetPosition(this);

        if (e.ClickCount == 2 && Keyboard.Modifiers == ModifierKeys.None)
        {
            HandleMouseDown(position, isRightButton: true);
            e.Handled = true;
            return;
        }

        CaptureMouse();
        HandleMouseDown(position, isRightButton: false);
        e.Handled = true;
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);
        Focus();

        HandleMouseDown(e.GetPosition(this), isRightButton: true);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        _doSectorSelection = false;

        if (IsMouseCaptured)
            ReleaseMouseCapture();
    }

    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonUp(e);
        _doSectorSelection = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var editor = _editor;

        if (editor?.SelectedRoom is null || editor.Action is EditorActionRelocateCamera)
            return;

        if (e.LeftButton == MouseButtonState.Pressed && _doSectorSelection && TryGetSectorFromVisualCoord(e.GetPosition(this), out var sectorPos))
            editor.SelectedSectors = new SectorSelection { Start = editor.SelectedSectors.Start, End = sectorPos };
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        CloseSelectionToolTip();
    }

    private void HandleMouseDown(Point position, bool isRightButton)
    {
        CloseSelectionToolTip();

        var editor = _editor;
        var room = Room;

        if (editor is null || room is null)
            return;

        if (!TryGetSectorFromVisualCoord(position, out var sectorPos))
            return;

        if (editor.Action is EditorActionRelocateCamera)
        {
            editor.MoveCameraToSector(sectorPos);
            return;
        }

        var selectedSectorObject = editor.SelectedObject as SectorBasedObjectInstance;
        bool isAltDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

        if (!isRightButton && !isAltDown)
        {
            if (selectedSectorObject is not null &&
                selectedSectorObject.Room == room &&
                selectedSectorObject.Area.Contains(sectorPos))
            {
                HandleSectorObjectClick(editor, room, selectedSectorObject);
            }
            else
            {
                editor.SelectedSectors = new SectorSelection { Start = sectorPos, End = sectorPos };

                if (selectedSectorObject is not null)
                    editor.SelectedObject = null;

                _doSectorSelection = true;
            }
        }
        else
        {
            SelectNextObjectAtSector(editor, room, sectorPos, selectedSectorObject, position);
        }
    }

    private static void HandleSectorObjectClick(Editor editor, Room room, SectorBasedObjectInstance selectedSectorObject)
    {
        if (selectedSectorObject is PortalInstance portal)
        {
            if (room.AlternateBaseRoom is not null && portal.AdjoiningRoom.Alternated)
            {
                editor.SelectRoom(portal.AdjoiningRoom.AlternateRoom);
                editor.SelectedObject = portal.FindOppositePortal(room).FindAlternatePortal(portal.AdjoiningRoom.AlternateRoom);
            }
            else
            {
                editor.SelectRoom(portal.AdjoiningRoom);
                editor.SelectedObject = portal.FindOppositePortal(room);
            }
        }
        else if (selectedSectorObject is TriggerInstance)
        {
            EditorActions.EditObject(selectedSectorObject, GetDialogOwner());
        }
    }

    private void SelectNextObjectAtSector(
        Editor editor,
        Room room,
        VectorInt2 sectorPos,
        SectorBasedObjectInstance? selectedSectorObject,
        Point tooltipPosition)
    {
        var portalsInRoom = room.Portals.Cast<SectorBasedObjectInstance>();
        var triggersInRoom = room.Triggers.Cast<SectorBasedObjectInstance>();
        var relevantObjects = portalsInRoom.Concat(triggersInRoom)
            .Where(obj => obj.Area.Contains(sectorPos));

        var nextObject = relevantObjects
            .FindFirstAfterWithWrapAround(obj => obj == selectedSectorObject, obj => true);

        if (nextObject is not null)
        {
            editor.SelectedObject = nextObject;
            ShowSelectionToolTip(nextObject, tooltipPosition);
        }
    }

    private void ShowSelectionToolTip(SectorBasedObjectInstance selectedObject, Point position)
    {
        CloseSelectionToolTip();

        _selectionToolTip = new ToolTip
        {
            Content = selectedObject.ToString(),
            HorizontalOffset = position.X + 5.0,
            Placement = PlacementMode.Relative,
            PlacementTarget = this,
            StaysOpen = false,
            VerticalOffset = position.Y + 5.0
        };

        ToolTip = _selectionToolTip;
        _selectionToolTip.IsOpen = true;
    }

    private void CloseSelectionToolTip()
    {
        if (_selectionToolTip is null)
            return;

        _selectionToolTip.IsOpen = false;
        _selectionToolTip = null;
        ToolTip = null;
    }

    private static System.Windows.Forms.IWin32Window? GetDialogOwner()
        => System.Windows.Forms.Application.OpenForms.Count > 0 ? System.Windows.Forms.Application.OpenForms[0] : null;

    // Rendering.

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
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

            DrawGridLines(dc, totalArea, gridDimensions, gridStep);
            DrawSelection(dc, editor, room, resources);
        }
        catch (Exception exc)
        {
            logger.Error(exc, "An exception occurred while drawing the 2D grid.");
        }
    }

    private static void DrawGridLines(DrawingContext dc, Rect totalArea, VectorInt2 gridDimensions, double gridStep)
    {
        var gridArea = GetGridLineArea(totalArea);
        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            for (int x = 1; x < gridDimensions.X; ++x)
            {
                double xPos = gridArea.X + (x * gridStep);
                AddLine(ctx, new Point(xPos, gridArea.Y), new Point(xPos, gridArea.Bottom));
            }

            for (int y = 1; y < gridDimensions.Y; ++y)
            {
                double yPos = gridArea.Y + (y * gridStep);
                AddLine(ctx, new Point(gridArea.X, yPos), new Point(gridArea.Right, yPos));
            }
        }

        geometry.Freeze();
        dc.DrawGeometry(null, GridPen, geometry);
        DrawRectangleInside(dc, BorderPen, totalArea);
    }

    private void DrawSelection(DrawingContext dc, Editor editor, Room room, RenderResources resources)
    {
        if (editor.SelectedSectors.Valid)
        {
            var selectionPen = resources.GetPen(editor.Configuration.UI_ColorScheme.ColorSelection, 2.0);
            DrawRectangleInside(dc, selectionPen, ToVisualCoord(editor.SelectedSectors.Area));
        }

        if (editor.SelectedObject is SectorBasedObjectInstance instance && instance.Room == room)
        {
            var pen = instance is PortalInstance ? SelectedPortalPen : SelectedTriggerPen;
            DrawRectangleInside(dc, pen, ToVisualCoord(instance.Area));
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
        DrawRectangleInside(dc, pen, sectorArea);
    }

    private static void DrawHatch(DrawingContext dc, RenderResources resources, Rect sectorArea, Vector4 color)
    {
        var pen = resources.GetPen(color, HatchPenThickness);
        var clip = new RectangleGeometry(sectorArea);
        clip.Freeze();

        dc.PushClip(clip);

        double startOffset = -sectorArea.Height / 2.0;
        double totalRange = sectorArea.Width + sectorArea.Height;
        double margin = (HatchPenThickness / 2.0) + 0.5;

        for (double offset = startOffset; offset < totalRange; offset += HatchSpacing)
        {
            dc.DrawLine(pen,
                new Point(sectorArea.X - sectorArea.Height + offset - margin, sectorArea.Bottom + margin),
                new Point(sectorArea.X + offset + margin, sectorArea.Top - margin));
        }

        dc.Pop();
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

    // Helper methods.

    private static void AddLine(StreamGeometryContext ctx, Point start, Point end)
    {
        ctx.BeginFigure(start, false, false);
        ctx.LineTo(end, true, false);
    }

    private static Rect GetGridLineArea(Rect totalArea)
    {
        return new Rect(
            totalArea.X + (BorderThickness / 2.0),
            totalArea.Y + (BorderThickness / 2.0),
            Math.Max(0.0, totalArea.Width - BorderThickness),
            Math.Max(0.0, totalArea.Height - BorderThickness));
    }

    private static void DrawRectangleInside(DrawingContext dc, Pen pen, Rect rect)
    {
        if (rect.Width > pen.Thickness && rect.Height > pen.Thickness)
            rect.Inflate(-pen.Thickness / 2.0, -pen.Thickness / 2.0);

        dc.DrawRectangle(null, pen, rect);
    }

    private void DrawDesignPlaceholder(DrawingContext dc)
    {
        dc.DrawRectangle(DesignPlaceholderBrush, BorderPen,
            new Rect(0.0, 0.0, ActualWidth, ActualHeight));

        var text = new FormattedText("2D Grid",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 12.0, Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(text, new Point(
            (ActualWidth - text.Width) / 2.0,
            (ActualHeight - text.Height) / 2.0));
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

            brush = WPFUtils.CreateFrozenBrush(color);
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

            pen = WPFUtils.CreateFrozenPen(color, thickness);
            _pens.Add(key, pen);
            return pen;
        }

        private readonly record struct PenKey(Color Color, double Thickness);
    }
}
