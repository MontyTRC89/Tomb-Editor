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

        // 1) White pixel. Pad it like the other entries so the surrounding
        // padded pixels stay white as well.
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

        // 2) Sector overlay sprites (arrows, crosses, slide directions, ...).
        LoadSectorOverlays(packer, atlasBytes, atlasSize);

        // 3) Level textures. Include EVERY loaded LevelTexture, not only the
        // ones already applied to a face — otherwise applying a freshly
        // loaded texture has no effect until the next full atlas rebuild
        // (it would fall back to the white pixel during meshing).
        var unique = new HashSet<Texture>();
        if (level.Settings?.Textures != null)
        {
            foreach (var tex in level.Settings.Textures)
            {
                if (tex == null || tex.IsUnavailable || tex.Image == null) continue;
                if (tex.Image.Width <= 0 || tex.Image.Height <= 0) continue;
                unique.Add(tex);
            }
        }
        // Also include any face-referenced texture that isn't in the level
        // texture list (imported geometry / WAD-embedded etc.).
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
            if (!PackAndBlit(packer, atlasBytes, atlasSize, tex.Image, out var innerOrigin))
                continue;
            _origins[tex] = innerOrigin;
        }

        // mipLevels = 0 → auto-generate a full mip chain. Without it,
        // distant textures alias hard (no detail averaging) and anisotropic
        // filtering can't do its job.
        Texture = device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, atlasSize, atlasSize,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            mipLevels: 0,
                            debugName: "LevelAtlas"),
            atlasBytes);

        // Anisotropic 4x matches the legacy SamplerDefault. Combined with the
        // mip chain above, this is the legacy "high quality" look.
        Sampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Anisotropic, AddressMode.Wrap, maxAnisotropy: 4));
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

            if (!PackAndBlit(packer, atlasBytes, atlasSize, img, out var innerOrigin)) continue;
            _sectorOverlays[st] = (innerOrigin, new VectorInt2(img.Width, img.Height));
        }
    }

    // Pack <texture image> with a 1-pixel padding gutter (edge-replicated)
    // around it and blit into the atlas. The padding stops bilinear /
    // anisotropic / mipmap sampling from bleeding pixels from neighbouring
    // packed textures — exactly what Dx11RenderingTextureAllocator does in
    // the legacy renderer.
    //
    // <paramref name="innerOrigin"/> is the top-left of the texture proper
    // (one pixel inside the packed rectangle).
    private bool PackAndBlit(
        RectPackerSimpleStack packer, byte[] atlasBytes, int atlasSize,
        ImageC image, out VectorInt2 innerOrigin)
    {
        innerOrigin = default;
        var inner = new VectorInt2(image.Width, image.Height);
        var padded = new VectorInt2(inner.X + 2, inner.Y + 2);
        var pos = packer.TryAdd(padded);
        if (pos == null) return false;

        var origin = new VectorInt2(pos.Value.X + 1, pos.Value.Y + 1);
        innerOrigin = origin;

        var src = image.ToByteArray();
        int rowBytes = inner.X * 4;
        for (int y = 0; y < inner.Y; y++)
        {
            int srcRow = y * rowBytes;
            int dstRow = ((origin.Y + y) * atlasSize + origin.X) * 4;
            Buffer.BlockCopy(src, srcRow, atlasBytes, dstRow, rowBytes);
        }

        // 1-pixel replicated border around the texture.
        for (int x = 0; x < inner.X; x++)
        {
            CopyPixel(atlasBytes, atlasSize, origin.X + x, origin.Y - 1,
                                              origin.X + x, origin.Y);            // top
            CopyPixel(atlasBytes, atlasSize, origin.X + x, origin.Y + inner.Y,
                                              origin.X + x, origin.Y + inner.Y - 1); // bottom
        }
        for (int y = 0; y < inner.Y; y++)
        {
            CopyPixel(atlasBytes, atlasSize, origin.X - 1,         origin.Y + y,
                                              origin.X,             origin.Y + y); // left
            CopyPixel(atlasBytes, atlasSize, origin.X + inner.X,   origin.Y + y,
                                              origin.X + inner.X - 1, origin.Y + y); // right
        }
        // Corners.
        CopyPixel(atlasBytes, atlasSize, origin.X - 1,        origin.Y - 1,
                                          origin.X,            origin.Y);
        CopyPixel(atlasBytes, atlasSize, origin.X + inner.X,  origin.Y - 1,
                                          origin.X + inner.X - 1, origin.Y);
        CopyPixel(atlasBytes, atlasSize, origin.X - 1,        origin.Y + inner.Y,
                                          origin.X,            origin.Y + inner.Y - 1);
        CopyPixel(atlasBytes, atlasSize, origin.X + inner.X,  origin.Y + inner.Y,
                                          origin.X + inner.X - 1, origin.Y + inner.Y - 1);
        return true;
    }

    private static void CopyPixel(byte[] atlas, int atlasSize, int dstX, int dstY, int srcX, int srcY)
    {
        int s = (srcY * atlasSize + srcX) * 4;
        int d = (dstY * atlasSize + dstX) * 4;
        atlas[d + 0] = atlas[s + 0];
        atlas[d + 1] = atlas[s + 1];
        atlas[d + 2] = atlas[s + 2];
        atlas[d + 3] = atlas[s + 3];
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
