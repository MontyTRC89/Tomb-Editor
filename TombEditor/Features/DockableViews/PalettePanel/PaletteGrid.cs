#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.PalettePanel;

/// <summary>
/// Displays and edits the active editor palette.
/// </summary>
public class PaletteGrid : FrameworkElement
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const double CellWidth = 10.0;
    private const double CellHeight = 10.0;
    private const double BorderThickness = 1.0;

    private static readonly ColorC FallbackSelectedColor = new(128, 128, 128);
    private static readonly Pen GridPen = WPFUtils.CreateFrozenPen(Color.FromArgb(140, 0, 0, 0), BorderThickness);
    private static readonly Pen BorderPen = WPFUtils.CreateFrozenPen(Colors.Black, BorderThickness);
    private static readonly Pen SelectionPen = WPFUtils.CreateFrozenPen(Colors.White, BorderThickness);

    private readonly Editor? _editor;
    private List<ColorC> _palette = [];
    private List<Brush> _paletteBrushes = [];
    private int _selectedIndex = -1;

    public bool Editable { get; set; } = true;

    public Color SelectedColor => ToWPFColor(SelectedColorC);

    public ColorC SelectedColorC => HasSelectedPaletteColor ? _palette[_selectedIndex] : FallbackSelectedColor;

    public List<ColorC> Palette => [.. _palette];

    private bool HasSelectedPaletteColor => _selectedIndex >= 0 && _selectedIndex < _palette.Count;
    private int ColumnCount => Math.Max(0, (int)((ActualWidth - BorderThickness) / CellWidth));
    private int RowCount => Math.Max(0, (int)((ActualHeight - BorderThickness) / CellHeight));

    public PaletteGrid()
    {
        ClipToBounds = true;
        Focusable = true;
        SnapsToDevicePixels = true;
        UseLayoutRounding = true;

        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        _editor = Editor.Instance;
    }

    public void LoadPalette(IReadOnlyList<ColorC> palette)
    {
        _palette = [.. palette.Select(CloneColor)];
        _paletteBrushes = [.. _palette.Select(CreateBrush)];

        if (_palette.Count == 0)
            _selectedIndex = -1;
        else if (_selectedIndex >= _palette.Count)
            _selectedIndex = _palette.Count - 1;

        PickColor();
        InvalidateVisual();
    }

    public void PickColor()
    {
        var editor = _editor;

        if (editor is null || !editor.Configuration.Palette_PickColorFromSelectedObject || editor.SelectedObject is null)
            return;

        if (editor.SelectedObject is not IColorable instance)
            return;

        var normalizedColor = instance.Color / 2.0f;
        var color = new ColorC(
            (byte)(normalizedColor.X * 255.0f),
            (byte)(normalizedColor.Y * 255.0f),
            (byte)(normalizedColor.Z * 255.0f));

        for (int i = 0; i < _palette.Count; i++)
        {
            if (_palette[i] == color)
            {
                _selectedIndex = i;
                editor.LastUsedPaletteColourChange(SelectedColorC);

                InvalidateVisual();
                return;
            }
        }
    }

    public void SetColorAtSelection(ColorC color)
    {
        if (_selectedIndex < 0)
            return;

        SetPaletteColor(_selectedIndex, color);
        InvalidateVisual();
    }

    // Mouse interaction.

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        var editor = _editor;

        if (editor is null)
            return;

        Focus();

        if (e.ClickCount == 2 && Editable)
        {
            PickColourFromDialog();
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            if (Editable)
                PickColourFromDialog(false);

            e.Handled = true;
            return;
        }

        CaptureMouse();

        if (editor.SelectedObject is PositionBasedObjectInstance selectedObject && selectedObject is IColorable)
            editor.UndoManager.PushObjectPropertyChanged(selectedObject);

        editor.ToggleHiddenSelection(true);
        ChangeColorByMouse(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        _editor?.ToggleHiddenSelection(false);

        if (IsMouseCaptured)
            ReleaseMouseCapture();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            ChangeColorByMouse(e.GetPosition(this));
    }

    private void ChangeColorByMouse(Point position)
    {
        var editor = _editor;

        if (editor is null)
            return;

        int columns = ColumnCount;
        int rows = RowCount;

        if (columns <= 0 || rows <= 0)
            return;

        int x = position.X < 0.0 ? 0 : (int)(position.X / CellWidth);
        int y = position.Y < 0.0 ? 0 : (int)(position.Y / CellHeight);
        x = Math.Min(x, columns - 1);
        y = Math.Min(y, rows - 1);

        _selectedIndex = (y * columns) + x;

        if (HasSelectedPaletteColor)
        {
            ApplySelectedColorToObject(editor);
            editor.LastUsedPaletteColourChange(SelectedColorC);
        }

        InvalidateVisual();
    }

    private void ApplySelectedColorToObject(Editor editor)
    {
        var selectedObject = editor.SelectedObject;

        if (selectedObject is null || !selectedObject.CanBeColored())
            return;

        editor.ToggleHiddenSelection(true);

        if (selectedObject is IColorable instance)
        {
            instance.Color = SelectedColor.ToFloat3Color() * 2.0f;

            if (selectedObject is LightInstance)
                selectedObject.Room.RebuildLighting(editor.Configuration.Rendering3D_HighQualityLightPreview);

            editor.ObjectChange(selectedObject, ObjectChangeType.Change);
        }
    }

    private void PickColourFromDialog(bool onlyFromPalette = true)
    {
        var editor = _editor;

        if (editor is null || !Editable)
            return;

        using var colorDialog = new RealtimeColorDialog(
            editor.Configuration.ColorDialog_Position.X,
            editor.Configuration.ColorDialog_Position.Y,
            null,
            editor.Configuration.UI_ColorScheme);

        var currentColor = SelectedColor;
        colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);

        if (!onlyFromPalette)
        {
            var obj = editor.SelectedObject;

            if (obj is LightInstance light)
                colorDialog.Color = (light.Color * 0.5f).ToWinFormsColor();
            else if (obj is StaticInstance stat)
                colorDialog.Color = (stat.Color * 0.5f).ToWinFormsColor();
            else if (editor.Level.IsTombEngine && obj is MoveableInstance moveable)
                colorDialog.Color = moveable.Color.ToWinFormsColor();
        }

        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var picked = colorDialog.Color;
            SetColorAtSelection(new ColorC(picked.R, picked.G, picked.B));
            editor.Level.Settings.Palette = Palette;
        }

        editor.Configuration.ColorDialog_Position = colorDialog.Position;
    }

    // Rendering.

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (_editor is null)
        {
            DrawDesignPlaceholder(dc);
            return;
        }

        try
        {
            int columns = ColumnCount;
            int rows = RowCount;

            if (columns <= 0 || rows <= 0)
                return;

            DrawCells(dc, columns, rows);
            DrawGridLines(dc, columns, rows);
            DrawOuterBorder(dc, columns, rows);
            DrawSelectionRect(dc, columns, rows);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An exception occurred while drawing the palette grid.");
        }
    }

    private void DrawCells(DrawingContext dc, int columns, int rows)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = (y * columns) + x;

                double xPos = (x * CellWidth) + BorderThickness;
                double yPos = (y * CellHeight) + BorderThickness;
                var rect = new Rect(xPos, yPos, CellWidth, CellHeight);

                dc.DrawRectangle(GetBrushAtIndex(index), null, rect);
            }
        }
    }

    private static void DrawGridLines(DrawingContext dc, int columns, int rows)
    {
        double totalWidth = columns * CellWidth;
        double totalHeight = rows * CellHeight;

        for (int x = 1; x < columns; x++)
        {
            double xPos = (x * CellWidth) + BorderThickness;
            dc.DrawLine(GridPen, new Point(xPos, 0.0), new Point(xPos, totalHeight));
        }

        for (int y = 1; y < rows; y++)
        {
            double yPos = (y * CellHeight) + BorderThickness;
            dc.DrawLine(GridPen, new Point(0.0, yPos), new Point(totalWidth, yPos));
        }
    }

    private static void DrawOuterBorder(DrawingContext dc, int columns, int rows)
    {
        double totalWidth = columns * CellWidth;
        double totalHeight = rows * CellHeight;
        var borderRect = new Rect(BorderThickness, BorderThickness, totalWidth, totalHeight);
        dc.DrawRectangle(null, BorderPen, borderRect);
    }

    private void DrawSelectionRect(DrawingContext dc, int columns, int rows)
    {
        if (_selectedIndex < 0 || columns <= 0)
            return;

        int x = _selectedIndex % columns;
        int y = _selectedIndex / columns;

        if (y >= rows)
            return;

        var selectionRect = new Rect(
            (x * CellWidth) + BorderThickness, (y * CellHeight) + BorderThickness,
            CellWidth, CellHeight);

        dc.DrawRectangle(null, SelectionPen, selectionRect);
    }

    private Brush GetBrushAtIndex(int index)
    {
        if (index < 0 || index >= _paletteBrushes.Count)
            return Brushes.Transparent;

        return _paletteBrushes[index];
    }

    // Helper methods.

    private void SetPaletteColor(int index, ColorC color)
    {
        while (_palette.Count < index)
        {
            _palette.Add(FallbackSelectedColor);
            _paletteBrushes.Add(CreateBrush(FallbackSelectedColor));
        }

        if (index == _palette.Count)
        {
            _palette.Add(CloneColor(color));
            _paletteBrushes.Add(CreateBrush(color));
            return;
        }

        _palette[index] = CloneColor(color);
        _paletteBrushes[index] = CreateBrush(color);
    }

    private static ColorC CloneColor(ColorC color) => new(color.R, color.G, color.B);

    private static Brush CreateBrush(ColorC color) => WPFUtils.CreateFrozenBrush(ToWPFColor(color));

    private static Color ToWPFColor(ColorC color) => Color.FromRgb(color.R, color.G, color.B);

    private void DrawDesignPlaceholder(DrawingContext dc)
    {
        dc.DrawRectangle(WPFUtils.CreateFrozenBrush(Color.FromRgb(50, 50, 50)), BorderPen,
            new Rect(0.0, 0.0, ActualWidth, ActualHeight));

        var text = new FormattedText("Palette",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 12.0, Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(text, new Point(
            (ActualWidth - text.Width) / 2.0,
            (ActualHeight - text.Height) / 2.0));
    }
}
