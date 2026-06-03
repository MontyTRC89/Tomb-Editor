#nullable enable

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Controls;
using TombLib.WPF.Features.AnimatedTextures;

namespace TombEditor.Controls;

/// <summary>
/// Pure-WPF clone of the WinForms <c>PanelTextureMapForAnimations</c>: draws every animated-texture
/// set's frame outlines on the atlas, highlighting the selected set and numbering its frames.
/// </summary>
public class WpfAnimatedTextureMapView : WpfTextureMapView, IAnimatedTextureMap
{
    protected override float MaxTextureSize => float.PositiveInfinity;
    protected override bool DrawTriangle => false;

    private static readonly Pen _outlinePen = CreatePen(Color.FromArgb(80, 192, 192, 192));
    private static readonly Pen _activeOutlinePen = CreatePen(Color.FromArgb(200, 238, 82, 238));
    private static readonly Brush _textBrush = Freeze(new SolidColorBrush(Colors.Violet));
    private static readonly Brush _textShadowBrush = Freeze(new SolidColorBrush(Colors.Black));
    private static readonly Typeface _typeface = new("Segoe UI");

    private AnimatedTextureSet? _selectedSet;
    public AnimatedTextureSet? SelectedSet
    {
        get => _selectedSet;
        set
        {
            _selectedSet = value;
            InvalidateVisual();
        }
    }

    private static Pen CreatePen(Color color)
    {
        var pen = new Pen(new SolidColorBrush(color), 2);
        pen.Freeze();
        return pen;
    }

    private static Brush Freeze(Brush brush)
    {
        brush.Freeze();
        return brush;
    }

    protected override void OnPaintSelection(DrawingContext drawingContext)
    {
        var sets = _editor.Level.Settings.AnimatedTextureSets;

        if (sets.Count > 0)
        {
            foreach (AnimatedTextureSet set in sets)
                if (set != _selectedSet)
                    DrawSetOutlines(drawingContext, set, false);

            DrawSetOutlines(drawingContext, _selectedSet, true);
        }

        // Current selection (handles, quad) on top.
        base.OnPaintSelection(drawingContext);
    }

    private void DrawSetOutlines(DrawingContext drawingContext, AnimatedTextureSet? set, bool current)
    {
        if (set is null)
            return;

        Texture? visible = ((TextureMapBase)this).VisibleTexture;
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        for (int i = 0; i < set.Frames.Count; ++i)
        {
            AnimatedTextureFrame frame = set.Frames[i];

            if (visible is null || !Equals(frame.Texture, visible))
                continue;

            Point[] edges =
            {
                ToVisualCoord(frame.TexCoord0),
                ToVisualCoord(frame.TexCoord1),
                ToVisualCoord(frame.TexCoord2),
                ToVisualCoord(frame.TexCoord3)
            };

            double minX = Math.Min(Math.Min(edges[0].X, edges[1].X), Math.Min(edges[2].X, edges[3].X));
            double minY = Math.Min(Math.Min(edges[0].Y, edges[1].Y), Math.Min(edges[2].Y, edges[3].Y));
            double maxX = Math.Max(Math.Max(edges[0].X, edges[1].X), Math.Max(edges[2].X, edges[3].X));
            double maxY = Math.Max(Math.Max(edges[0].Y, edges[1].Y), Math.Max(edges[2].Y, edges[3].Y));

            if (current)
            {
                string counter = (i + 1) + "/" + set.Frames.Count;
                var text = new FormattedText(counter, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    _typeface, 12.0, _textBrush, dpi) { TextAlignment = TextAlignment.Left };
                var shadow = new FormattedText(counter, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    _typeface, 12.0, _textShadowBrush, dpi) { TextAlignment = TextAlignment.Left };

                if (maxX - minX > text.Width && maxY - minY > text.Height)
                {
                    var origin = new Point((minX + maxX) / 2.0 - text.Width / 2.0, (minY + maxY) / 2.0 - text.Height / 2.0);
                    drawingContext.DrawText(shadow, new Point(origin.X + 1, origin.Y + 1));
                    drawingContext.DrawText(text, origin);
                }
            }

            DrawClosedPolyline(drawingContext, current ? _activeOutlinePen : _outlinePen, edges);
        }
    }

    private static void DrawClosedPolyline(DrawingContext drawingContext, Pen pen, Point[] points)
    {
        if (points.Length == 0)
            return;

        var geometry = new StreamGeometry();
        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], false, true);
            for (int i = 1; i < points.Length; i++)
                context.LineTo(points[i], true, false);
        }
        geometry.Freeze();
        drawingContext.DrawGeometry(null, pen, geometry);
    }
}
