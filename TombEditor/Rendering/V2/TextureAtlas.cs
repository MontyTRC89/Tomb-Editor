using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.RenderingV2.Rhi;
using TombLib.Utils;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Single-page atlas owned by the V2 renderer. Packs three classes of texel
/// data into one BGRA8 page:
///   1. A reserved 1×1 white pixel (untextured / fallback).
///   2. All <see cref="SectorTexture"/> overlay sprites loaded from
///      TombLib.Rendering's embedded resources (slope arrows, slide
///      directions, illegal-slope markers, etc.).
///   3. Every unique level <see cref="Texture"/> referenced by any room.
///
/// <para>Triangles whose texture overflows the page or is invisible/null
/// fall back to the white pixel so the shader can always sample.</para>
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

    public TextureAtlas(IRhiDevice device, Level level, int atlasSize = 4096)
    {
        _device = device;
        Size = new VectorInt2(atlasSize, atlasSize);

        var atlasBytes = new byte[atlasSize * atlasSize * 4];
        var packer = new RectPackerSimpleStack(Size);

        // 1) White pixel.
        var wp = packer.TryAdd(new VectorInt2(1, 1))
                 ?? throw new InvalidOperationException("Atlas too small for the reserved white pixel.");
        _whitePixel = wp;
        int wpIdx = (wp.Y * atlasSize + wp.X) * 4;
        atlasBytes[wpIdx + 0] = 255;
        atlasBytes[wpIdx + 1] = 255;
        atlasBytes[wpIdx + 2] = 255;
        atlasBytes[wpIdx + 3] = 255;

        // 2) Sector overlay sprites (arrows, crosses, slide directions, ...).
        LoadSectorOverlays(packer, atlasBytes, atlasSize);

        // 3) Level textures.
        var unique = new HashSet<Texture>();
        foreach (var room in level.Rooms)
        {
            if (room?.RoomGeometry == null) continue;
            foreach (var ta in room.RoomGeometry.TriangleTextureAreas)
            {
                var tex = ta.Texture;
                if (tex == null || tex is TextureInvisible) continue;
                if (tex.IsUnavailable || tex.Image == null) continue;
                if (tex.Image.Width <= 0 || tex.Image.Height <= 0) continue;
                unique.Add(tex);
            }
        }

        foreach (var tex in unique)
        {
            var size = new VectorInt2(tex.Image.Width, tex.Image.Height);
            var pos = packer.TryAdd(size);
            if (pos == null) continue;
            _origins[tex] = pos.Value;
            BlitInto(atlasBytes, atlasSize, pos.Value, size, tex.Image.ToByteArray());
        }

        Texture = device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, atlasSize, atlasSize,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            debugName: "LevelAtlas"),
            atlasBytes);

        Sampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Linear, AddressMode.Wrap, maxAnisotropy: 4));
    }

    private static readonly Assembly RenderingAssembly = typeof(SectorTexture).Assembly;

    private void LoadSectorOverlays(RectPackerSimpleStack packer, byte[] atlasBytes, int atlasSize)
    {
        // SectorTexture.None is not a real sprite; every other enum value
        // maps to a PNG embedded in TombLib.Rendering.
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

            var size = new VectorInt2(img.Width, img.Height);
            var pos = packer.TryAdd(size);
            if (pos == null) continue;

            _sectorOverlays[st] = (pos.Value, size);
            BlitInto(atlasBytes, atlasSize, pos.Value, size, img.ToByteArray());
        }
    }

    private static void BlitInto(byte[] dst, int atlasSize, VectorInt2 origin, VectorInt2 size, byte[] src)
    {
        int rowBytes = size.X * 4;
        for (int y = 0; y < size.Y; y++)
        {
            int srcRow = y * rowBytes;
            int dstRow = ((origin.Y + y) * atlasSize + origin.X) * 4;
            Buffer.BlockCopy(src, srcRow, dst, dstRow, rowBytes);
        }
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
        // VertexEditorUVs are 0..1 across the face; map onto the sprite rect.
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
