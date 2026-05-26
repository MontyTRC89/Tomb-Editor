using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace TombLib.Icons;

/// WinForms / GDI+ entry point. Loads a pre-rendered PNG variant matching the current DPI.
public static class IconBitmaps
{
    private static readonly Assembly Asm = typeof(IconBitmaps).Assembly;
    private const string AsmPrefix = "TombLib.Icons.Png.";

    private static readonly ConcurrentDictionary<(IconId Id, int Scale), Bitmap> Cache = new();

    /// Returns the embedded PNG bitmap for `id` at the variant closest to `dpiScale`.
    /// dpiScale: 1.0 = 96 DPI, 1.5 = 144 DPI, 2.0 = 192 DPI, 2.25 = 216 DPI, etc.
    public static Bitmap Load(IconId id, float dpiScale)
    {
        var bucket = PickBucket(dpiScale);
        return LoadBucket(id, bucket);
    }

    /// Convenience overload using a string path like "Actions/Play".
    public static Bitmap Load(string path, float dpiScale) => Load(IconId.Parse(path), dpiScale);

    /// Convenience overload. Loads the 1x (16 px) bucket. The hosting process is
    /// DPI-unaware so Windows bitmap-stretches the toolbar at high DPI; loading a
    /// higher bucket here would only be re-stretched and waste memory.
    public static Bitmap Load(IconId id) => LoadBucket(id, 1);

    /// Convenience overload using a string path like "Actions/Play". Loads the 1x bucket.
    public static Bitmap Load(string path) => LoadBucket(IconId.Parse(path), 1);

    /// Loads at the DPI of the given control (uses Control.DeviceDpi). Falls back to process DPI.
    public static Bitmap LoadForControl(IconId id, System.Windows.Forms.Control? control)
        => Load(id, control is null ? CurrentDpiScale() : control.DeviceDpi / 96f);

    /// Loads a specific 1x/2x/3x/4x variant directly. Useful when caller knows the exact bucket.
    public static Bitmap LoadBucket(IconId id, int bucket)
    {
        if (bucket is < 1 or > 4) throw new ArgumentOutOfRangeException(nameof(bucket), "Bucket must be 1..4");
        return Cache.GetOrAdd((id, bucket), key =>
        {
            // Folder is "@Nx" on disk; MSBuild rewrites '@' to '_' in the manifest resource name.
            var resourceName = $"{AsmPrefix}_{key.Scale}x.{key.Id.Category}.{key.Id.Name}.png";
            using var stream = Asm.GetManifestResourceStream(resourceName)
                               ?? throw new FileNotFoundException($"Embedded icon not found: {resourceName}");
            var bmp = new Bitmap(stream);
            // Tag the bitmap with its IconId so controls can reload a different bucket
            // when DPI changes without having to track the mapping themselves.
            bmp.Tag = key.Id;
            return bmp;
        });
    }

    private static int PickBucket(float dpiScale) => dpiScale switch
    {
        <  1.30f => 1,
        <  1.85f => 2,
        <  2.85f => 3,
        _        => 4
    };

    /// Reads the current process DPI scale (relative to 96).
    public static float CurrentDpiScale()
    {
        using var g = Graphics.FromHwnd(IntPtr.Zero);
        return g.DpiX / 96f;
    }
}
