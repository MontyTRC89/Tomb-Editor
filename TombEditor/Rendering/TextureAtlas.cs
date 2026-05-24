using System;
using System.Collections.Concurrent;
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
using TombLib.Wad;

namespace TombEditor.Rendering;

/// <summary>
/// Single-page atlas owned by the renderer. Packs three classes of texel
/// data into one BGRA8 page:
///   1. A reserved white pixel (untextured / fallback).
///   2. All <see cref="SectorTexture"/> overlay sprites loaded from
///      TombLib.Rendering's embedded resources.
///   3. One entry per distinct texture <i>region</i> sampled by a face — the
///      rectangle a polygon actually uses, never the whole source page.
///
/// <para>Packing per region (instead of per page) is what keeps textured
/// polygons seam-free: every entry gets its own edge-replicated gutter, so a
/// face that samples right up to its tile edge can only ever reach a copy of
/// its own edge — never the neighbouring tile of the same source page. This
/// mirrors the legacy <c>RenderingTextureAllocator.GetForTriangle</c> /
/// <c>Dx11RenderingTextureAllocator</c> behaviour.</para>
///
/// <para>Build pipeline:
///   1. Collect the distinct (texture, region) rectangles every room face,
///      WAD polygon and imported-geometry submesh samples.
///   2. Pack them largest-first into the page (the shelf packer fragments
///      badly otherwise and would drop big entries).
///   3. Blit each region's pixels + replicate its gutter, in parallel via
///      <see cref="Parallel.ForEach"/> — each job writes a disjoint atlas
///      rectangle. Row copies use AVX2 / SSE2 stores when available.</para>
/// </summary>
public sealed class TextureAtlas : IDisposable
{
    public TextureHandle Texture { get; }
    public SamplerHandle Sampler { get; }
    public VectorInt2    Size    { get; }

    private readonly IRhiDevice _device;
    private readonly Dictionary<TexRegion, VectorInt2> _regions = new();
    private readonly Dictionary<SectorTexture, (VectorInt2 Origin, VectorInt2 Size)> _sectorOverlays = new();
    private readonly VectorInt2 _whitePixel;

    private static readonly Assembly RenderingAssembly = typeof(SectorTexture).Assembly;

    // Edge-replicated gutter around every packed entry. The legacy allocator
    // used 1 px and never showed seams; 4 px is a generous margin for the
    // anisotropic footprint at oblique angles without wasting much atlas area.
    private const int Gutter = 4;

    // A distinct rectangle of a source texture page, identified by the page
    // plus its integer bounding box. Two faces sampling the same tile resolve
    // to the same region and share one packed entry.
    private readonly record struct TexRegion(Texture Texture, int X, int Y, int W, int H);

    // One blit task: copy the W×H sub-rect at (SrcX, SrcY) of Src (stride
    // SrcStride bytes) into the atlas at InnerOrigin, then replicate gutters.
    private readonly record struct PackJob(
        VectorInt2 InnerOrigin, byte[] Src, int SrcStride, int SrcX, int SrcY, int W, int H);

    public TextureAtlas(IRhiDevice device, Level level, int atlasSize = 4096)
    {
        _device = device;
        Size = new VectorInt2(atlasSize, atlasSize);

        var atlasBytes = new byte[atlasSize * atlasSize * 4];
        var packer = new RectPackerSimpleStack(Size);

        // --- 1) Reserved white pixel — a 3×3 white block, sampled at its centre.
        var whiteBlock = packer.TryAdd(new VectorInt2(3, 3))
                         ?? throw new InvalidOperationException("Atlas too small for the reserved white pixel.");
        _whitePixel = new VectorInt2(whiteBlock.X + 1, whiteBlock.Y + 1);
        for (int yy = 0; yy < 3; yy++)
        for (int xx = 0; xx < 3; xx++)
        {
            int idx = ((whiteBlock.Y + yy) * atlasSize + (whiteBlock.X + xx)) * 4;
            atlasBytes[idx + 0] = 255;
            atlasBytes[idx + 1] = 255;
            atlasBytes[idx + 2] = 255;
            atlasBytes[idx + 3] = 255;
        }

        // --- 2) Load the sector-overlay sprites from embedded resources, in
        // parallel — each is an independent PNG decode.
        var overlayBag = new ConcurrentBag<(SectorTexture St, ImageC Img)>();
        Parallel.ForEach((SectorTexture[])Enum.GetValues(typeof(SectorTexture)), st =>
        {
            if (st == SectorTexture.None) return;

            string resourceName = "TombLib.Rendering.SectorTextures." + st + ".png";
            using Stream? stream = RenderingAssembly.GetManifestResourceStream(resourceName);
            if (stream == null) return;

            ImageC image;
            try { image = ImageC.FromStream(stream); }
            catch { return; }
            if (image != null && image.Width > 0 && image.Height > 0)
                overlayBag.Add((st, image));
        });
        var overlayImages = new List<(SectorTexture St, ImageC Img)>(overlayBag);

        // --- 3) Collect the texture regions every face / polygon samples.
        var regionSet = new HashSet<TexRegion>();

        void CollectMesh(WadMesh? mesh)
        {
            if (mesh == null) return;
            foreach (var poly in mesh.Polys)
            {
                var ta = poly.Texture;
                if (poly.Shape == WadPolygonShape.Triangle)
                    CollectFace(regionSet, ta.Texture, ta.TexCoord0, ta.TexCoord1, ta.TexCoord2);
                else
                    CollectFace(regionSet, ta.Texture, ta.TexCoord0, ta.TexCoord1, ta.TexCoord2, ta.TexCoord3);
            }
        }

        // Room geometry — every textured triangle. Parallelised over rooms:
        // each thread fills its own set (HashSet is not thread-safe), and the
        // sets are merged as each partition finishes.
        Parallel.ForEach(level.Rooms,
            () => new HashSet<TexRegion>(),
            (room, _, localSet) =>
            {
                if (room?.RoomGeometry != null)
                    foreach (var ta in room.RoomGeometry.TriangleTextureAreas)
                        CollectFace(localSet, ta.Texture, ta.TexCoord0, ta.TexCoord1, ta.TexCoord2);
                return localSet;
            },
            localSet => { lock (regionSet) regionSet.UnionWith(localSet); });

        // WAD static / moveable polygons.
        if (level.Settings?.Wads != null)
            foreach (var refWad in level.Settings.Wads)
            {
                if (refWad?.Wad == null) continue;
                foreach (var staticMesh in refWad.Wad.Statics.Values)
                    CollectMesh(staticMesh.Mesh);
                foreach (var moveable in refWad.Wad.Moveables.Values)
                    foreach (var bone in moveable.Bones)
                        CollectMesh(bone?.Mesh);
            }

        // Horizon (skybox) moveable — drawn every frame, referenced by nothing.
        if (level.Settings != null)
        {
            var horizonId = WadMoveableId.GetHorizon(level.Settings.GameVersion);
            if (horizonId.HasValue)
            {
                var horizon = level.Settings.WadTryGetMoveable(horizonId.Value);
                if (horizon != null)
                    foreach (var bone in horizon.Bones)
                        CollectMesh(bone?.Mesh);
            }
        }

        // Imported geometry — sampled with full-image UVs, so the region is
        // the whole texture.
        if (level.Settings?.ImportedGeometries != null)
            foreach (var ig in level.Settings.ImportedGeometries)
            {
                if (ig?.DirectXModel?.Meshes == null) continue;
                foreach (var mesh in ig.DirectXModel.Meshes)
                    foreach (var submesh in mesh.Submeshes.Values)
                        CollectWholeImage(regionSet, submesh.Material?.Texture);
            }

        // --- 4) Convert every distinct source page to raw BGRA bytes, in
        // parallel — one independent ImageC.ToByteArray per page.
        var uniquePages = new List<Texture>();
        var seenPages   = new HashSet<Texture>();
        foreach (var region in regionSet)
            if (seenPages.Add(region.Texture)) uniquePages.Add(region.Texture);

        var pageByteArrays = new byte[uniquePages.Count][];
        Parallel.For(0, uniquePages.Count, i => pageByteArrays[i] = uniquePages[i].Image.ToByteArray());

        var pageBytes = new Dictionary<Texture, byte[]>(uniquePages.Count);
        for (int i = 0; i < uniquePages.Count; i++)
            pageBytes[uniquePages[i]] = pageByteArrays[i];

        // --- 5) Pack regions largest-first, then the overlay sprites. Packing
        // itself stays single-threaded — the shelf packer is stateful.
        var packJobs = new List<PackJob>();

        var sortedRegions = new List<TexRegion>(regionSet);
        sortedRegions.Sort((a, b) => ((long)b.W * b.H).CompareTo((long)a.W * a.H));

        foreach (var region in sortedRegions)
        {
            if (!TryPack(packer, region.W, region.H, out var inner)) continue;
            _regions[region] = inner;
            packJobs.Add(new PackJob(inner, pageBytes[region.Texture], region.Texture.Image.Width * 4,
                                     region.X, region.Y, region.W, region.H));
        }

        foreach (var (st, image) in overlayImages)
        {
            if (!TryPack(packer, image.Width, image.Height, out var inner)) continue;
            _sectorOverlays[st] = (inner, new VectorInt2(image.Width, image.Height));
            packJobs.Add(new PackJob(inner, image.ToByteArray(), image.Width * 4,
                                     0, 0, image.Width, image.Height));
        }

        // --- 6) Blit + replicate gutters in parallel (disjoint atlas rects).
        Parallel.ForEach(packJobs, job =>
            BlitAndPad(atlasBytes, atlasSize, job.InnerOrigin,
                       job.Src, job.SrcStride, job.SrcX, job.SrcY, job.W, job.H));

        // No mip chain on purpose — the legacy room atlas runs MipLevels=1 +
        // anisotropic 4× too. With auto mips the gutter shrinks to sub-pixel
        // on lower levels and the anisotropic minification blends across
        // neighbouring atlas entries, which shows as dark lines at seams.
        Texture = device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, atlasSize, atlasSize,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            mipLevels: 1, debugName: "LevelAtlas"),
            atlasBytes);

        // Anisotropic 4× + Mirror address mode — the same combination the
        // legacy Dx11RenderingDevice.SamplerDefault uses.
        Sampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Anisotropic, AddressMode.Mirror, maxAnisotropy: 4));
    }

    // ===================================================== Region collection

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void CollectFace(HashSet<TexRegion> set, Texture? texture,
                                    Vector2 a, Vector2 b, Vector2 c)
    {
        Span<Vector2> coords = stackalloc Vector2[3] { a, b, c };
        if (TryComputeRegion(texture, coords, out var region))
            set.Add(region);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void CollectFace(HashSet<TexRegion> set, Texture? texture,
                                    Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Span<Vector2> coords = stackalloc Vector2[4] { a, b, c, d };
        if (TryComputeRegion(texture, coords, out var region))
            set.Add(region);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void CollectWholeImage(HashSet<TexRegion> set, Texture? texture)
    {
        if (texture?.Image == null || texture.Image.Width <= 0 || texture.Image.Height <= 0)
            return;
        CollectFace(set, texture, Vector2.Zero,
                    new Vector2(texture.Image.Width, texture.Image.Height), Vector2.Zero);
    }

    // Integer bounding box of a face's source texcoords, clamped to the source
    // image. Two faces sampling the same tile produce the same region, so they
    // share one packed entry. Build-time collection and render-time MapFace
    // both go through here, so the keys always agree.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool TryComputeRegion(Texture? texture, ReadOnlySpan<Vector2> faceCoords,
                                         out TexRegion region)
    {
        region = default;
        if (texture == null || texture is TextureInvisible || texture.IsUnavailable
            || texture.Image == null || faceCoords.Length == 0)
            return false;

        int imageW = texture.Image.Width;
        int imageH = texture.Image.Height;
        if (imageW <= 0 || imageH <= 0) return false;

        // SIMD min / max — System.Numerics.Vector2 lowers each step to a
        // single packed-min / packed-max instruction.
        Vector2 min = faceCoords[0], max = faceCoords[0];
        for (int i = 1; i < faceCoords.Length; i++)
        {
            min = Vector2.Min(min, faceCoords[i]);
            max = Vector2.Max(max, faceCoords[i]);
        }

        int x0 = Math.Clamp((int)MathF.Floor(min.X),   0, imageW);
        int y0 = Math.Clamp((int)MathF.Floor(min.Y),   0, imageH);
        int x1 = Math.Clamp((int)MathF.Ceiling(max.X), 0, imageW);
        int y1 = Math.Clamp((int)MathF.Ceiling(max.Y), 0, imageH);
        if (x1 <= x0) { x0 = Math.Min(x0, imageW - 1); x1 = x0 + 1; }
        if (y1 <= y0) { y0 = Math.Min(y0, imageH - 1); y1 = y0 + 1; }
        if (x0 < 0 || y0 < 0) return false;

        region = new TexRegion(texture, x0, y0, x1 - x0, y1 - y0);
        return true;
    }

    // ============================================================== Packing

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool TryPack(RectPackerSimpleStack packer, int w, int h, out VectorInt2 innerOrigin)
    {
        innerOrigin = default;
        var pos = packer.TryAdd(new VectorInt2(w + Gutter * 2, h + Gutter * 2));
        if (pos == null) return false;
        innerOrigin = new VectorInt2(pos.Value.X + Gutter, pos.Value.Y + Gutter);
        return true;
    }

    // Thread-safe: each call writes a region of the atlas disjoint from any
    // other call's (the packer guarantees it). Copies the W×H sub-rect of
    // `src` at (srcX, srcY), then replicates a Gutter-wide edge border so
    // bilinear / anisotropic sampling at the entry edge stays inside it.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe void BlitAndPad(byte[] atlasBytes, int atlasSize, VectorInt2 origin,
                                          byte[] src, int srcStride, int srcX, int srcY, int w, int h)
    {
        int rowBytes = w * 4;

        fixed (byte* atlasPtr = atlasBytes)
        fixed (byte* srcPtr   = src)
        {
            // Inner image — copy each row of the source sub-rect.
            for (int y = 0; y < h; y++)
            {
                byte* dst = atlasPtr + ((origin.Y + y) * atlasSize + origin.X) * 4;
                byte* s   = srcPtr   + (srcY + y) * srcStride + srcX * 4;
                SimdMemcpy(dst, s, rowBytes);
            }

            // Top + bottom gutters — replicate the adjacent in-image row.
            byte* topSrc = atlasPtr + ( origin.Y          * atlasSize + origin.X) * 4;
            byte* botSrc = atlasPtr + ((origin.Y + h - 1) * atlasSize + origin.X) * 4;
            for (int g = 1; g <= Gutter; g++)
            {
                SimdMemcpy(atlasPtr + ((origin.Y - g)         * atlasSize + origin.X) * 4, topSrc, rowBytes);
                SimdMemcpy(atlasPtr + ((origin.Y + h - 1 + g) * atlasSize + origin.X) * 4, botSrc, rowBytes);
            }

            // Left + right gutters (corners included). yClamped pulls the
            // edge pixel for rows above / below the inner area.
            for (int y = -Gutter; y < h + Gutter; y++)
            {
                int yClamped = y < 0 ? 0 : (y >= h ? h - 1 : y);
                byte* rowBase = atlasPtr + ((origin.Y + y) * atlasSize) * 4;
                uint  leftPx  = *(uint*)(atlasPtr + ((origin.Y + yClamped) * atlasSize + origin.X)         * 4);
                uint  rightPx = *(uint*)(atlasPtr + ((origin.Y + yClamped) * atlasSize + origin.X + w - 1) * 4);
                for (int g = 1; g <= Gutter; g++)
                {
                    *(uint*)(rowBase + (origin.X - g)         * 4) = leftPx;
                    *(uint*)(rowBase + (origin.X + w - 1 + g) * 4) = rightPx;
                }
            }
        }
    }

    /// <summary>
    /// Block-copy <paramref name="byteCount"/> bytes from <paramref name="src"/>
    /// to <paramref name="dst"/> using the widest SIMD path available:
    /// AVX2 (32-byte stores) → SSE2 (16-byte) → scalar.
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

    // ============================================================ Public API

    /// <summary>UV of the reserved 1×1 white pixel (the no-texture fallback).</summary>
    public Vector2 WhitePixelUv => new(
        (_whitePixel.X + 0.5f) / Size.X,
        (_whitePixel.Y + 0.5f) / Size.Y);

    /// <summary>
    /// Resolves the atlas placement for one textured face. Pass every source
    /// texcoord the face uses — together they identify the packed region.
    /// The returned <see cref="FaceUvMapper"/> converts each of those
    /// texcoords to an atlas UV; it falls back to the white pixel when the
    /// face's texture / region was not packed.
    /// </summary>
    public FaceUvMapper MapFace(Texture? texture, ReadOnlySpan<Vector2> faceCoords)
    {
        if (TryComputeRegion(texture, faceCoords, out var region)
            && _regions.TryGetValue(region, out var atlasOrigin))
            return new FaceUvMapper(atlasOrigin, new VectorInt2(region.X, region.Y), Size);
        return new FaceUvMapper(WhitePixelUv);
    }

    public FaceUvMapper MapFace(Texture? texture, Vector2 a, Vector2 b)
    {
        Span<Vector2> coords = stackalloc Vector2[2] { a, b };
        return MapFace(texture, coords);
    }

    public FaceUvMapper MapFace(Texture? texture, Vector2 a, Vector2 b, Vector2 c)
    {
        Span<Vector2> coords = stackalloc Vector2[3] { a, b, c };
        return MapFace(texture, coords);
    }

    public FaceUvMapper MapFace(Texture? texture, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Span<Vector2> coords = stackalloc Vector2[4] { a, b, c, d };
        return MapFace(texture, coords);
    }

    /// <summary>
    /// Atlas UV for a sector-overlay sprite (slope arrow, slide, cross, ...).
    /// <paramref name="faceUv"/> is the per-vertex (0..1) coordinate inside
    /// the sprite. Returns the white-pixel UV when the sprite is missing.
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

    /// <summary>
    /// Maps a single textured face's source texcoords to atlas UVs. Created by
    /// <see cref="TextureAtlas.MapFace(Texture, ReadOnlySpan{Vector2})"/> — one
    /// per face — then <see cref="Map"/> is called for each of the face's
    /// vertices.
    /// </summary>
    public readonly struct FaceUvMapper
    {
        private readonly Vector2 _atlasOrigin;   // packed region origin, atlas texels
        private readonly Vector2 _sourceStart;   // region origin in the source page
        private readonly Vector2 _invAtlasSize;
        private readonly Vector2 _fallbackUv;
        private readonly bool    _valid;

        internal FaceUvMapper(VectorInt2 atlasOrigin, VectorInt2 sourceStart, VectorInt2 atlasSize)
        {
            _atlasOrigin  = new Vector2(atlasOrigin.X, atlasOrigin.Y);
            _sourceStart  = new Vector2(sourceStart.X, sourceStart.Y);
            _invAtlasSize = new Vector2(1f / atlasSize.X, 1f / atlasSize.Y);
            _fallbackUv   = default;
            _valid        = true;
        }

        internal FaceUvMapper(Vector2 fallbackUv)
        {
            _atlasOrigin  = default;
            _sourceStart  = default;
            _invAtlasSize = default;
            _fallbackUv   = fallbackUv;
            _valid        = false;
        }

        /// <summary>Maps one source-page pixel coordinate to a normalized atlas UV.</summary>
        public Vector2 Map(Vector2 sourcePixelCoord)
            => _valid
               ? (_atlasOrigin + (sourcePixelCoord - _sourceStart)) * _invAtlasSize
               : _fallbackUv;
    }
}
