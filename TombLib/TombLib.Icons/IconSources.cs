using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace TombLib.Icons;

/// WPF entry point. Loads the SVG as a vector DrawingImage that scales perfectly.
public static class IconSources
{
    private static readonly Assembly Asm = typeof(IconSources).Assembly;
    private const string AsmPrefix = "TombLib.Icons.Svg.";

    private static readonly ConcurrentDictionary<IconId, DrawingImage> Cache = new();

    public static DrawingImage Load(IconId id)
    {
        return Cache.GetOrAdd(id, key =>
        {
            var resourceName = $"{AsmPrefix}{key.Category}.{key.Name}.svg";
            using var stream = Asm.GetManifestResourceStream(resourceName)
                               ?? throw new FileNotFoundException($"Embedded SVG not found: {resourceName}");

            var settings  = new WpfDrawingSettings { IncludeRuntime = false, TextAsGeometry = true };
            using var reader = new FileSvgReader(settings);
            var drawing = reader.Read(stream);
            var img = new DrawingImage(drawing);
            img.Freeze();
            return img;
        });
    }

    public static DrawingImage Load(string path) => Load(IconId.Parse(path));
}
