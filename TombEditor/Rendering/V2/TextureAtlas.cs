using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.RenderingV2.Rhi;
using TombLib.Utils;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Single-page atlas owned by the V2 renderer. Packs three classes of texel
/// data into one BGRA8 page:
///   1. A reserved white pixel (untextured / fallback).
///   2. All <see cref="SectorTexture"/> overlay sprites loaded from
///      TombLib.Rendering's embedded resources.
///   3. Every loaded level <see cref="Texture"/> (plus any face-referenced
///      texture that isn't in the level texture list).
///
/// <para>Entries get a 1-pixel edge-replicated gutter so bilinear /
/// anisotropic / mipmap sampling can't bleed pixels from neighbouring
/// packed textures.</para>
///
/// <para>Build pipeline:
///   1. Pack all sources sequentially (the shelf packer is not thread-safe).
///   2. Blit + replicate the gutter in parallel via <see cref="Parallel.ForEach"/>
///      — each job writes to a disjoint atlas region, so the work scales
///      with core count.
///   3. Row copies inside each job use AVX2 (32-byte) or SSE2 (16-byte)
///      stores when supported, scalar fallback otherwise.
/// </para>
/// </summary>
public sealed class TextureAtlas : IDisposable
{
    public TextureHandle Texture  { get; }
    public SamplerHandle Sampler  { get; }
    public VectorInt2    Size     { get; }

    private readonly IRhiDevice _device;
    private readonly Dictionary<Texture, VectorInt2> _origins = new();
    private readonly Dictionary<SectorTexture, (VectorInt2 Origin, VectorInt2 Size)> _sectorOverlays = new();
    private readonly VectorInt2 _whitePixel;

    private static readonly Assembly RenderingAssembly = typeof(SectorTexture).Assembly;

    private readonly record struct PackJob(VectorInt2 InnerOrigin, ImageC Image);

    public TextureAtlas(IRhiDevice device, Level level, int atlasSize = 4096)
    {
        _device = device;
        Size = new VectorInt2(atlasSize, atlasSize);

        var atlasBytes = new byte[atlasSize * atlasSize * 4];
        var packer = new RectPackerSimpleStack(Size);

        // 1) White pixel + 1-pixel gutter (3×3 white block).
        var wpPadded = packer.TryAdd(new VectorInt2(3, 3))
                       ?? throw new InvalidOperationException("Atlas too small for the reserved white pixel.");
        _whitePixel = new VectorInt2(wpPadded.X + 1, wpPadded.Y + 1);
        for (int yy = 0; yy < 3; yy++)
        for (int xx = 0; xx < 3; xx++)
        {
            int idx = ((wpPadded.Y + yy) * atlasSize + (wpPadded.X + xx)) * 4;
            atlasBytes[idx + 0] = 255;
            atlasBytes[idx + 1] = 255;
            atlasBytes[idx + 2] = 255;
            atlasBytes[idx + 3] = 255;
        }

        // 2 + 3) Pack everything sequentially (RectPacker is not thread-safe).
        var overlayJobs = new List<(SectorTexture St, PackJob Job)>();
        foreach (SectorTexture st in Enum.GetValues(typeof(SectorTexture)))
        {
            if (st == SectorTexture.None) continue;

            string resName = "TombLib.Rendering.SectorTextures." + st.ToString() + ".png";
            using Stream? stream = RenderingAssembly.GetManifestResourceStream(resName);
            if (stream == null) continue;

            ImageC img;
            try { img = ImageC.FromStream(stream); }
            catch { continue; }
            if (img == null || img.Width <= 0 || img.Height <= 0) continue;

            if (TryPack(packer, img, out var inner))
            {
                _sectorOverlays[st] = (inner, new VectorInt2(img.Width, img.Height));
                overlayJobs.Add((st, new PackJob(inner, img)));
            }
        }

        var unique = new HashSet<Texture>();
        void TryAdd(Texture? t)
        {
            if (t == null || t is TextureInvisible) return;
            if (t.IsUnavailable || t.Image == null) return;
            if (t.Image.Width <= 0 || t.Image.Height <= 0) return;
            unique.Add(t);
        }

        if (level.Settings?.Textures != null)
            foreach (var t in level.Settings.Textures) TryAdd(t);

        foreach (var room in level.Rooms)
        {
            if (room?.RoomGeometry == null) continue;
            foreach (var ta in room.RoomGeometry.TriangleTextureAreas)
                TryAdd(ta.Texture);
        }

        // WAD textures: every static / moveable polygon's texture, so the
        // object pass can sample them.
        if (level.Settings?.Wads != null)
        {
            foreach (var refWad in level.Settings.Wads)
            {
                if (refWad?.Wad == null) continue;
                foreach (var s in refWad.Wad.Statics.Values)
                    if (s.Mesh != null)
                        foreach (var poly in s.Mesh.Polys) TryAdd(poly.Texture.Texture);
                foreach (var m in refWad.Wad.Moveables.Values)
                    foreach (var bone in m.Bones)
                        if (bone?.Mesh != null)
                            foreach (var poly in bone.Mesh.Polys) TryAdd(poly.Texture.Texture);
            }
        }

        // ImportedGeometry textures.
        if (level.Settings?.ImportedGeometries != null)
        {
            foreach (var ig in level.Settings.ImportedGeometries)
            {
                if (ig?.DirectXModel?.Meshes == null) continue;
                foreach (var mesh in ig.DirectXModel.Meshes)
                    foreach (var submesh in mesh.Submeshes.Values)
                        TryAdd(submesh.Material?.Texture);
            }
        }

        var textureJobs = new List<PackJob>(unique.Count);
        foreach (var tex in unique)
        {
            if (TryPack(packer, tex.Image, out var inner))
            {
                _origins[tex] = inner;
                textureJobs.Add(new PackJob(inner, tex.Image));
            }
        }

        // Phase 2 — parallel blit + padding. Each job writes to a disjoint
        // (W+2)×(H+2) rectangle so there are no overlapping writes.
        var allJobs = new List<PackJob>(overlayJobs.Count + textureJobs.Count);
        foreach (var (_, job) in overlayJobs) allJobs.Add(job);
        allJobs.AddRange(textureJobs);

        Parallel.ForEach(allJobs, job =>
        {
            BlitAndPad(atlasBytes, atlasSize, job.InnerOrigin, job.Image);
        });

        // No mip chain on purpose. The legacy Dx11RenderingTextureAllocator
        // (room atlas) uses MipLevels=1 + anisotropic 4× — the small gutter
        // is enough because the sampler footprint at mip 0 stays inside the
        // 4-pixel edge replication. With auto-generated mips, lower mip
        // levels shrink the gutter to sub-pixel (gutter / 2^mip) and the
        // anisotropic minification blends across neighbouring atlas entries,
        // which shows up as visible dark lines at sector seams. Trade-off:
        // textures viewed at extreme distance can alias — the legacy lives
        // with that and it's never been a reported issue.
        Texture = device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, atlasSize, atlasSize,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            mipLevels: 1,
                            debugName: "LevelAtlas"),
            atlasBytes);

        // Mirror address mode + anisotropic 4× — the legacy
        // Dx11RenderingDevice.SamplerDefault uses exactly this combination.
        // Wrap was reading "around the atlas" on the anisotropic footprint
        // at oblique angles and showed up as thin lines at polygon seams.
        Sampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Anisotropic, AddressMode.Mirror, maxAnisotropy: 4));
    }

    // 8-pixel gutter on each side. With anisotropic 4× the sampler footprint
    // at oblique floor angles can extend ~6-8 texels in the elongated
    // direction. 4 px was insufficient to contain that footprint at sector
    // seams (visible thin dark/light lines on tiled floors). 8 px costs more
    // atlas area but covers the worst case without artifacts.
    private const int Gutter = 8;

    private static bool TryPack(RectPackerSimpleStack packer, ImageC image, out VectorInt2 innerOrigin)
    {
        innerOrigin = default;
        var padded = new VectorInt2(image.Width + Gutter * 2, image.Height + Gutter * 2);
        var pos = packer.TryAdd(padded);
        if (pos == null) return false;
        innerOrigin = new VectorInt2(pos.Value.X + Gutter, pos.Value.Y + Gutter);
        return true;
    }

    // Thread-safe: each call writes to a region of the atlas that is
    // disjoint from any other call's region (the packer guarantees it).
    // Uses SIMD memcpy for the inner rows; gutters are 4 px on each side.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe void BlitAndPad(byte[] atlasBytes, int atlasSize, VectorInt2 origin, ImageC image)
    {
        int w = image.Width;
        int h = image.Height;
        byte[] src = image.ToByteArray();
        int rowBytes = w * 4;

        fixed (byte* atlasPtr = atlasBytes)
        fixed (byte* srcPtr   = src)
        {
            // Inner image — N rows of `rowBytes` each.
            for (int y = 0; y < h; y++)
            {
                byte* dst = atlasPtr + ((origin.Y + y) * atlasSize + origin.X) * 4;
                byte* s   = srcPtr   + y * rowBytes;
                SimdMemcpy(dst, s, rowBytes);
            }

            // Top + bottom gutters — replicate the adjacent in-image row
            // Gutter times. This pre-computes the same value we'd otherwise
            // sample at runtime via Address.Clamp, but works inside the atlas.
            byte* topSrc = atlasPtr + ( origin.Y          * atlasSize + origin.X) * 4;
            byte* botSrc = atlasPtr + ((origin.Y + h - 1) * atlasSize + origin.X) * 4;
            for (int g = 1; g <= Gutter; g++)
            {
                byte* topDst = atlasPtr + ((origin.Y - g)         * atlasSize + origin.X) * 4;
                byte* botDst = atlasPtr + ((origin.Y + h - 1 + g) * atlasSize + origin.X) * 4;
                SimdMemcpy(topDst, topSrc, rowBytes);
                SimdMemcpy(botDst, botSrc, rowBytes);
            }

            // Left + right gutters (including the corners). yClamped pulls
            // the sampled in-image pixel for rows above/below the inner area
            // so corner pixels still get filled with edge colour.
            for (int y = -Gutter; y < h + Gutter; y++)
            {
                int yClamped = y < 0 ? 0 : (y >= h ? h - 1 : y);
                byte* rowBase = atlasPtr + ((origin.Y + y) * atlasSize) * 4;
                uint  leftPx  = *(uint*)(atlasPtr + ((origin.Y + yClamped) * atlasSize + origin.X)         * 4);
                uint  rightPx = *(uint*)(atlasPtr + ((origin.Y + yClamped) * atlasSize + origin.X + w - 1) * 4);
                for (int g = 1; g <= Gutter; g++)
                {
                    *(uint*)(rowBase + (origin.X - g)        * 4) = leftPx;
                    *(uint*)(rowBase + (origin.X + w - 1 + g)* 4) = rightPx;
                }
            }
        }
    }

    /// <summary>
    /// Block-copy <paramref name="byteCount"/> bytes from <paramref name="src"/>
    /// to <paramref name="dst"/> using the widest SIMD path available:
    /// AVX2 (32-byte unaligned stores) → SSE2 (16-byte) → scalar.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe void SimdMemcpy(byte* dst, byte* src, int byteCount)
    {
        int i = 0;
        if (Avx2.IsSupported)
        {
            int n = byteCount & ~31;
            for (; i < n; i += 32)
                Avx.Store(dst + i, Avx.LoadVector256(src + i));
        }
        else if (Sse2.IsSupported)
        {
            int n = byteCount & ~15;
            for (; i < n; i += 16)
                Sse2.Store(dst + i, Sse2.LoadVector128(src + i));
        }
        for (; i < byteCount; i++) dst[i] = src[i];
    }

    /// <summary>UV of the reserved 1×1 white pixel (used as a no-texture fallback).</summary>
    public Vector2 WhitePixelUv => new(
        (_whitePixel.X + 0.5f) / Size.X,
        (_whitePixel.Y + 0.5f) / Size.Y);

    /// <summary>
    /// Translate a source-texture pixel coordinate into a normalized atlas UV.
    /// Returns the centre of the reserved white pixel for untextured tris.
    /// </summary>
    public Vector2 GetAtlasUv(Texture? texture, Vector2 sourcePixelCoord)
    {
        if (texture != null && _origins.TryGetValue(texture, out var origin))
            return new Vector2(
                (origin.X + sourcePixelCoord.X) / Size.X,
                (origin.Y + sourcePixelCoord.Y) / Size.Y);
        return WhitePixelUv;
    }

    /// <summary>
    /// Atlas UV for a sector-overlay sprite (slope arrow, slide, cross, ...).
    /// <paramref name="faceUv"/> is the per-vertex (0..1) coordinate inside
    /// the sprite as produced by <see cref="RoomGeometry.VertexEditorUVs"/>.
    /// Returns the white-pixel UV when the sprite is missing from the atlas.
    /// </summary>
    public Vector2 GetSectorOverlayUv(SectorTexture st, Vector2 faceUv)
    {
        if (st == SectorTexture.None || !_sectorOverlays.TryGetValue(st, out var rect))
            return WhitePixelUv;
        return new Vector2(
            (rect.Origin.X + faceUv.X * rect.Size.X) / Size.X,
            (rect.Origin.Y + faceUv.Y * rect.Size.Y) / Size.Y);
    }

    public void Dispose()
    {
        if (Sampler.IsValid) _device.Destroy(Sampler);
        if (Texture.IsValid) _device.Destroy(Texture);
    }
}
