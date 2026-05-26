using NLog;
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
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Rendering.Graphics.Rhi;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Rendering;

/// <summary>
/// Pooled Texture2DArray atlas owned by the renderer. Packs three classes of
/// texel data across up to <see cref="MaxLayers"/> 4096-square layers of one
/// array texture:
///   1. A reserved white pixel (untextured / fallback) in layer 0.
///   2. All <see cref="SectorTexture"/> overlay sprites loaded from
///      TombLib.Rendering's embedded resources.
///   3. One entry per distinct texture <i>region</i> sampled by a face -- the
///      rectangle a polygon actually uses, never the whole source page.
///
/// <para>Packing per region (instead of per page) is what keeps textured
/// polygons seam-free: every entry gets its own edge-replicated gutter, so a
/// face that samples right up to its tile edge can only ever reach a copy of
/// its own edge -- never the neighbouring tile of the same source page.</para>
///
/// <para>The pool spills to a new layer when the current one runs out of
/// room. A huge level with imported geometries used to saturate the single
/// 4096^2 page and fall back to the white pixel (textures disappeared in face-
/// edit mode); spreading across multiple array layers absorbs that case --
/// the array texture binds once, the layer index lives in a per-vertex
/// attribute, so the shader still only sees one Texture2DArray.</para>
/// </summary>
public sealed class TextureAtlas : IDisposable
{
    public TextureHandle Texture    { get; }
    public SamplerHandle Sampler    { get; }
    public VectorInt2    Size       { get; }
    public int           LayerCount { get; }

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    // 16 * 4096^2 * 4 bytes = 1 GB worst-case GPU residency. Real levels
    // typically need 1-2 layers; level + heavy imported-geometry maps can
    // push 4-8. The GPU only commits the layers we actually upload to, and
    // CPU mirrors are lazy-allocated, so the resident cost scales with the
    // populated layer count rather than MaxLayers.
    public const int MaxLayers = 16;

    private readonly IRhiDevice _device;
    private readonly Dictionary<TexRegion, (int Layer, VectorInt2 Origin)> _regions = new();
    private readonly Dictionary<SectorTexture, (int Layer, VectorInt2 Origin, VectorInt2 Size)> _sectorOverlays = new();
    private readonly (int Layer, VectorInt2 Origin) _whitePixel;
    // Legacy V1 fallback patterns -- packed once into the atlas so the room
    // renderer can route faces with unavailable source files / out-of-range
    // tex coords to a distinctive checker pattern instead of the white pixel.
    private (int Layer, VectorInt2 Origin, VectorInt2 Size) _unavailablePattern;
    private (int Layer, VectorInt2 Origin, VectorInt2 Size) _outOfBoundsPattern;
    // CPU mirror of each populated layer, retained for the debug "dump atlas"
    // command (PNG write). Lazy-allocated -- a level that fills only layer 0
    // never pays the (MaxLayers - 1) * 64 MB CPU pressure.
    private readonly byte[]?[] _layerBytes;

    private static readonly Assembly RenderingAssembly = typeof(SectorTexture).Assembly;

    // Edge-replicated gutter around every packed entry. The legacy allocator
    // used 1 px and never showed seams; 4 px is a generous margin for the
    // anisotropic footprint at oblique angles without wasting much atlas area.
    private const int Gutter = 4;

    private readonly record struct TexRegion(Texture Texture, int X, int Y, int W, int H);

    private readonly record struct PackJob(
        int Layer, VectorInt2 InnerOrigin, byte[] Src, int SrcStride, int SrcX, int SrcY, int W, int H);

    public TextureAtlas(IRhiDevice device, Level level, int atlasSize = 4096)
    {
        _device = device;
        Size    = new VectorInt2(atlasSize, atlasSize);

        // Lazy-allocate layer mirrors so a level that only fills the first
        // layer doesn't pay (MaxLayers - 1) * 64 MB of CPU pressure for
        // nothing. EnsureLayerBytes promotes a slot on first write.
        _layerBytes = new byte[MaxLayers][];

        var packers = new RectPackerSimpleStack[MaxLayers];
        for (int i = 0; i < MaxLayers; i++)
            packers[i] = new RectPackerSimpleStack(Size);
        int usedLayers = 1;

        byte[] EnsureLayerBytes(int layer)
        {
            var bytes = _layerBytes[layer];
            if (bytes == null)
            {
                bytes = new byte[atlasSize * atlasSize * 4];
                _layerBytes[layer] = bytes;
            }
            return bytes;
        }
        // Layer 0 always exists (white pixel + overlays seed it).
        EnsureLayerBytes(0);

        // --- 1) Reserved white pixel -- a 3x3 white block in layer 0.
        var whiteBlock = packers[0].TryAdd(new VectorInt2(3, 3))
                         ?? throw new InvalidOperationException("Atlas too small for the reserved white pixel.");
        _whitePixel = (0, new VectorInt2(whiteBlock.X + 1, whiteBlock.Y + 1));
        for (int yy = 0; yy < 3; yy++)
        for (int xx = 0; xx < 3; xx++)
        {
            int idx = ((whiteBlock.Y + yy) * atlasSize + (whiteBlock.X + xx)) * 4;
            _layerBytes[0][idx + 0] = 255;
            _layerBytes[0][idx + 1] = 255;
            _layerBytes[0][idx + 2] = 255;
            _layerBytes[0][idx + 3] = 255;
        }

        // --- 2) Load the sector-overlay sprites from embedded resources, in
        // parallel -- each is an independent PNG decode.
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

        // --- 2b) Load the two legacy fallback patterns alongside the sector
        // overlays. They live in the same SectorTextures resource folder but
        // are not part of the SectorTexture enum -- they are sampled
        // explicitly by the room renderer for IsUnavailable / out-of-bounds
        // faces (see GetUnavailableUv / GetOutOfBoundsUv below). ImageC is a
        // struct, so we use a bool + by-ref out for the "could load" flag.
        static bool TryLoadPattern(string fileName, out ImageC image)
        {
            image = default;
            string resourceName = "TombLib.Rendering.SectorTextures." + fileName;
            using Stream? stream = RenderingAssembly.GetManifestResourceStream(resourceName);
            if (stream == null) return false;
            try { image = ImageC.FromStream(stream); return image.Width > 0 && image.Height > 0; }
            catch { return false; }
        }
        bool hasUnavailable = TryLoadPattern("texture_unavailable.png", out ImageC unavailableImage);
        bool hasOutOfBounds = TryLoadPattern("texture_coord_out_of_bounds.png", out ImageC outOfBoundsImage);

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

        // Imported geometry -- pack the whole source page once per material
        // texture (deduped via the HashSet). Per-triangle packing fragments
        // the atlas badly when an OBJ has thousands of polys sampling
        // overlapping sub-regions of the same page, and large levels with
        // many imported geos saturate the pool well before per-image would.
        if (level.Settings?.ImportedGeometries != null)
            foreach (var ig in level.Settings.ImportedGeometries)
            {
                if (ig?.DirectXModel?.Meshes == null) continue;
                foreach (var mesh in ig.DirectXModel.Meshes)
                    foreach (var submesh in mesh.Submeshes.Values)
                        CollectWholeImage(regionSet, submesh.Material?.Texture);
            }

        if (level.Settings != null &&
            level.Settings.GameVersion.Native() <= TRVersion.Game.TR2)
        {
            foreach (var kv in level.Settings.WadGetAllSpriteSequences())
            {
                var seq = kv.Value;
                if (seq?.Sprites == null) continue;
                foreach (var sprite in seq.Sprites)
                    CollectWholeImage(regionSet, sprite.Texture);
            }
        }

        // --- 4) Convert every distinct source page to raw BGRA bytes, in
        // parallel -- one independent ImageC.ToByteArray per page.
        var uniquePages = new List<Texture>();
        var seenPages   = new HashSet<Texture>();
        foreach (var region in regionSet)
            if (seenPages.Add(region.Texture)) uniquePages.Add(region.Texture);

        var pageByteArrays = new byte[uniquePages.Count][];
        Parallel.For(0, uniquePages.Count, i => pageByteArrays[i] = uniquePages[i].Image.ToByteArray());

        var pageBytes = new Dictionary<Texture, byte[]>(uniquePages.Count);
        for (int i = 0; i < uniquePages.Count; i++)
            pageBytes[uniquePages[i]] = pageByteArrays[i];

        // --- 5) Pack regions largest-first across the layer pool. The packer
        // is stateful (sequential), but spilling to the next layer is just a
        // retry with a fresh packer.
        var packJobs = new List<PackJob>();

        var sortedRegions = new List<TexRegion>(regionSet);
        sortedRegions.Sort((a, b) => ((long)b.W * b.H).CompareTo((long)a.W * a.H));

        bool overflowReported = false;
        foreach (var region in sortedRegions)
        {
            if (!TryPackPool(packers, region.W, region.H, ref usedLayers, out int layer, out var inner))
            {
                if (!overflowReported)
                {
                    overflowReported = true;
                    _logger.Warn("Level atlas pool exhausted ({0} layers, {1}x{1} each). " +
                                 "Some textures will render as plain white. Consider raising MaxLayers " +
                                 "or downscaling source textures.", MaxLayers, atlasSize);
                }
                continue;
            }
            _regions[region] = (layer, inner);
            packJobs.Add(new PackJob(layer, inner, pageBytes[region.Texture],
                                     region.Texture.Image.Width * 4,
                                     region.X, region.Y, region.W, region.H));
        }

        foreach (var (st, image) in overlayImages)
        {
            if (!TryPackPool(packers, image.Width, image.Height, ref usedLayers, out int layer, out var inner))
                continue;
            _sectorOverlays[st] = (layer, inner, new VectorInt2(image.Width, image.Height));
            packJobs.Add(new PackJob(layer, inner, image.ToByteArray(), image.Width * 4,
                                     0, 0, image.Width, image.Height));
        }

        // Pack the two fallback patterns. If either can't fit (extremely
        // unlikely -- they are small PNGs), the consumer falls back to the
        // white pixel via the default-constructed tuple's Size == 0 check.
        if (hasUnavailable
            && TryPackPool(packers, unavailableImage.Width, unavailableImage.Height,
                           ref usedLayers, out int unavLayer, out var unavInner))
        {
            _unavailablePattern = (unavLayer, unavInner,
                                   new VectorInt2(unavailableImage.Width, unavailableImage.Height));
            packJobs.Add(new PackJob(unavLayer, unavInner, unavailableImage.ToByteArray(),
                                     unavailableImage.Width * 4,
                                     0, 0, unavailableImage.Width, unavailableImage.Height));
        }
        if (hasOutOfBounds
            && TryPackPool(packers, outOfBoundsImage.Width, outOfBoundsImage.Height,
                           ref usedLayers, out int oobLayer, out var oobInner))
        {
            _outOfBoundsPattern = (oobLayer, oobInner,
                                   new VectorInt2(outOfBoundsImage.Width, outOfBoundsImage.Height));
            packJobs.Add(new PackJob(oobLayer, oobInner, outOfBoundsImage.ToByteArray(),
                                     outOfBoundsImage.Width * 4,
                                     0, 0, outOfBoundsImage.Width, outOfBoundsImage.Height));
        }

        // --- 6) Pre-allocate the byte mirrors for every layer we actually
        // packed into. The parallel blit relies on the destination buffer
        // being a stable reference -- letting threads race on EnsureLayerBytes
        // would risk double-allocation. After this loop the lazy field stays
        // null for any layer the packer never touched.
        for (int i = 0; i < usedLayers; i++)
            EnsureLayerBytes(i);

        // --- 7) Blit + replicate gutters in parallel (disjoint atlas rects).
        Parallel.ForEach(packJobs, job =>
            BlitAndPad(_layerBytes[job.Layer]!, atlasSize, job.InnerOrigin,
                       job.Src, job.SrcStride, job.SrcX, job.SrcY, job.W, job.H));

        // --- 8) Create the array texture sized to the layers we actually
        // packed into, then upload each layer separately. UpdateTexture uses
        // the DX-style subresource = mip + layer * mipLevels encoding, so
        // mipLevels = 1 means subresource == layer.
        //
        // Always allocate AT LEAST 2 array layers so every backend picks a
        // Texture2DArray view: DX11 auto-selects D3D11_SRV_DIMENSION_TEXTURE2D
        // when ArraySize == 1 (mismatching the shader's Texture2DArray
        // declaration); VK / GL do the same. The unused layer stays zero,
        // which only the white-pixel fallback would ever sample.
        LayerCount = usedLayers;
        int allocatedLayers = Math.Max(usedLayers, 2);
        if (usedLayers > 1)
            _logger.Info("Level atlas pool: {0}/{1} layers populated.", usedLayers, MaxLayers);

        Texture = device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, atlasSize, atlasSize,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            mipLevels: 1, arrayLayers: allocatedLayers, debugName: "LevelAtlas"),
            ReadOnlySpan<byte>.Empty);
        for (int layer = 0; layer < usedLayers; layer++)
        {
            device.UpdateTexture(Texture, layer,
                                 0, 0, atlasSize, atlasSize,
                                 atlasSize * 4, _layerBytes[layer]!);
        }

        // Anisotropic 4x + Mirror address mode -- the same combination the
        // legacy Dx11RenderingDevice.SamplerDefault uses.
        Sampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Anisotropic, AddressMode.Mirror, maxAnisotropy: 4));
    }

    // ===================================================== Region collection

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void CollectFace(HashSet<TexRegion> set, Texture? texture,
                                    Vector2 a, Vector2 b)
    {
        Span<Vector2> coords = stackalloc Vector2[2] { a, b };
        if (TryComputeRegion(texture, coords, out var region))
            set.Add(region);
    }

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

    // Spilling pool packer: try each layer in order; allocate the next one on
    // demand (which is free since all packers + layer mirrors are pre-built).
    // Updates usedLayers so the array texture only allocates the layers we
    // actually touched.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool TryPackPool(RectPackerSimpleStack[] packers, int w, int h,
                                    ref int usedLayers, out int layer, out VectorInt2 innerOrigin)
    {
        for (int i = 0; i < packers.Length; i++)
        {
            if (!TryPack(packers[i], w, h, out innerOrigin))
                continue;
            layer = i;
            if (i + 1 > usedLayers) usedLayers = i + 1;
            return true;
        }
        layer = 0;
        innerOrigin = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool TryPack(RectPackerSimpleStack packer, int w, int h, out VectorInt2 innerOrigin)
    {
        innerOrigin = default;
        var pos = packer.TryAdd(new VectorInt2(w + Gutter * 2, h + Gutter * 2));
        if (pos == null) return false;
        innerOrigin = new VectorInt2(pos.Value.X + Gutter, pos.Value.Y + Gutter);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe void BlitAndPad(byte[] atlasBytes, int atlasSize, VectorInt2 origin,
                                          byte[] src, int srcStride, int srcX, int srcY, int w, int h)
    {
        int rowBytes = w * 4;

        fixed (byte* atlasPtr = atlasBytes)
        fixed (byte* srcPtr   = src)
        {
            for (int y = 0; y < h; y++)
            {
                byte* dst = atlasPtr + ((origin.Y + y) * atlasSize + origin.X) * 4;
                byte* s   = srcPtr   + (srcY + y) * srcStride + srcX * 4;
                SimdMemcpy(dst, s, rowBytes);
            }

            byte* topSrc = atlasPtr + ( origin.Y          * atlasSize + origin.X) * 4;
            byte* botSrc = atlasPtr + ((origin.Y + h - 1) * atlasSize + origin.X) * 4;
            for (int g = 1; g <= Gutter; g++)
            {
                SimdMemcpy(atlasPtr + ((origin.Y - g)         * atlasSize + origin.X) * 4, topSrc, rowBytes);
                SimdMemcpy(atlasPtr + ((origin.Y + h - 1 + g) * atlasSize + origin.X) * 4, botSrc, rowBytes);
            }

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

    /// <summary>UV of the reserved 1x1 white pixel (the no-texture fallback).</summary>
    public Vector2 WhitePixelUv => new(
        (_whitePixel.Origin.X + 0.5f) / Size.X,
        (_whitePixel.Origin.Y + 0.5f) / Size.Y);

    /// <summary>Array layer of the reserved white pixel (always layer 0).</summary>
    public int WhitePixelLayer => _whitePixel.Layer;

    public FaceUvMapper MapFace(Texture? texture, ReadOnlySpan<Vector2> faceCoords)
    {
        if (TryComputeRegion(texture, faceCoords, out var region)
            && _regions.TryGetValue(region, out var entry))
            return new FaceUvMapper(entry.Origin, new VectorInt2(region.X, region.Y), Size, entry.Layer);
        return new FaceUvMapper(WhitePixelUv, _whitePixel.Layer);
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
    /// Atlas (layer, UV) for a sector-overlay sprite (slope arrow, slide, cross, ...).
    /// <paramref name="faceUv"/> is the per-vertex (0..1) coordinate inside
    /// the sprite. Returns the white-pixel (layer, UV) when the sprite is missing.
    /// </summary>
    public (int Layer, Vector2 Uv) GetSectorOverlayUv(SectorTexture st, Vector2 faceUv)
    {
        if (st == SectorTexture.None || !_sectorOverlays.TryGetValue(st, out var rect))
            return (_whitePixel.Layer, WhitePixelUv);
        return (rect.Layer, new Vector2(
            (rect.Origin.X + faceUv.X * rect.Size.X) / Size.X,
            (rect.Origin.Y + faceUv.Y * rect.Size.Y) / Size.Y));
    }

    /// <summary>
    /// Atlas (layer, UV) for the legacy "texture file missing" pattern, tiled
    /// across a face using its editor UV ({-1, 0, 1} sector-corner space).
    /// Mirrors the legacy Dx11RenderingDevice.TextureUnavailable path.
    /// Falls back to the white pixel if the pattern couldn't be packed.
    /// </summary>
    public (int Layer, Vector2 Uv) GetUnavailableUv(Vector2 editorUv)
        => MapPatternByEditorUv(_unavailablePattern, editorUv);

    /// <summary>
    /// Atlas (layer, UV) for the legacy "tex coords out of bounds" pattern,
    /// tiled across a face using its editor UV. Mirrors the legacy
    /// Dx11RenderingDevice.TextureCoordOutOfBounds path.
    /// </summary>
    public (int Layer, Vector2 Uv) GetOutOfBoundsUv(Vector2 editorUv)
        => MapPatternByEditorUv(_outOfBoundsPattern, editorUv);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private (int Layer, Vector2 Uv) MapPatternByEditorUv(
        (int Layer, VectorInt2 Origin, VectorInt2 Size) pattern, Vector2 editorUv)
    {
        if (pattern.Size.X == 0 || pattern.Size.Y == 0)
            return (_whitePixel.Layer, WhitePixelUv);
        // Legacy formula: Vector2.Abs(editorUv) * (imageSize - 1) + 0.5
        // -- maps the {-1, 0, 1} sector corner range to a pixel walk across
        // the pattern image, with the +0.5 nudging into the center of the
        // texel so bilinear filtering doesn't sample neighbouring atlas data.
        Vector2 abs = Vector2.Abs(editorUv);
        Vector2 pxInPattern = abs * new Vector2(pattern.Size.X - 1, pattern.Size.Y - 1)
                              + new Vector2(0.5f);
        return (pattern.Layer, new Vector2(
            (pattern.Origin.X + pxInPattern.X) / Size.X,
            (pattern.Origin.Y + pxInPattern.Y) / Size.Y));
    }

    /// <summary>
    /// Resolves the (layer + UV rectangle) of a TR1 / TR2 sprite frame inside
    /// the atlas. Returns false when the frame's texture wasn't packed --
    /// typically because the level isn't TR1 / TR2.
    /// </summary>
    public bool TryGetSpriteFrameUv(TombLib.Wad.WadSpriteSequence sequence, int frame,
                                    out Vector2 uvMin, out Vector2 uvMax, out int layer)
    {
        uvMin = uvMax = default;
        layer = 0;
        if (sequence == null || frame < 0 || frame >= sequence.Sprites.Count) return false;
        var tex = sequence.Sprites[frame].Texture;
        if (tex?.Image == null || tex.Image.Width <= 0 || tex.Image.Height <= 0) return false;

        var key = new TexRegion(tex, 0, 0, tex.Image.Width, tex.Image.Height);
        if (!_regions.TryGetValue(key, out var entry)) return false;

        float invW = 1f / Size.X, invH = 1f / Size.Y;
        uvMin = new Vector2(entry.Origin.X * invW, entry.Origin.Y * invH);
        uvMax = new Vector2((entry.Origin.X + tex.Image.Width) * invW,
                            (entry.Origin.Y + tex.Image.Height) * invH);
        layer = entry.Layer;
        return true;
    }

    /// <summary>
    /// Write each populated atlas layer to <paramref name="directory"/> as
    /// <c>LevelAtlas{N}.png</c>. Uses the CPU mirror retained from construction
    /// so no GPU readback is needed. Returns the first file path written
    /// (kept for backward-compatible callers that open one file in the shell).
    /// </summary>
    public string Dump(string directory)
    {
        Directory.CreateDirectory(directory);

        string firstPath = string.Empty;
        for (int layer = 0; layer < LayerCount; layer++)
        {
            var bytes = _layerBytes[layer];
            if (bytes == null) continue;
            var image  = ImageC.FromByteArray(bytes, Size.X, Size.Y);
            string path = Path.Combine(directory, $"LevelAtlas{layer}.png");
            image.SaveToFile(path);
            if (firstPath.Length == 0) firstPath = path;
        }
        return firstPath;
    }

    public void Dispose()
    {
        if (Sampler.IsValid) _device.Destroy(Sampler);
        if (Texture.IsValid) _device.Destroy(Texture);
    }

    /// <summary>
    /// Maps a single textured face's source texcoords to atlas UVs + the
    /// array layer the face's region lives in. Created by
    /// <see cref="TextureAtlas.MapFace(Texture, ReadOnlySpan{Vector2})"/> -- one
    /// per face -- then <see cref="Map"/> is called for each of the face's
    /// vertices. <see cref="Layer"/> is the same for every vertex of the face
    /// and goes into the per-vertex layer attribute.
    /// </summary>
    public readonly struct FaceUvMapper
    {
        private readonly Vector2 _atlasOrigin;
        private readonly Vector2 _sourceStart;
        private readonly Vector2 _invAtlasSize;
        private readonly Vector2 _fallbackUv;
        private readonly bool    _valid;
        public  readonly int     Layer;

        internal FaceUvMapper(VectorInt2 atlasOrigin, VectorInt2 sourceStart, VectorInt2 atlasSize, int layer)
        {
            _atlasOrigin  = new Vector2(atlasOrigin.X, atlasOrigin.Y);
            _sourceStart  = new Vector2(sourceStart.X, sourceStart.Y);
            _invAtlasSize = new Vector2(1f / atlasSize.X, 1f / atlasSize.Y);
            _fallbackUv   = default;
            _valid        = true;
            Layer         = layer;
        }

        internal FaceUvMapper(Vector2 fallbackUv, int layer)
        {
            _atlasOrigin  = default;
            _sourceStart  = default;
            _invAtlasSize = default;
            _fallbackUv   = fallbackUv;
            _valid        = false;
            Layer         = layer;
        }

        public Vector2 Map(Vector2 sourcePixelCoord)
            => _valid
               ? (_atlasOrigin + (sourcePixelCoord - _sourceStart)) * _invAtlasSize
               : _fallbackUv;
    }
}
