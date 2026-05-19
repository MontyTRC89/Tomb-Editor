using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib;
using TombLib.LevelData;
using TombLib.RenderingV2.Rhi;
using TombLib.Utils;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Single-page texture atlas for the level. Built once per level: every
/// unique <see cref="Texture"/> referenced by any room's geometry is packed
/// into one large BGRA8 texture using a shelf packer. Triangles whose
/// texture overflows the page or is invisible/null fall back to a reserved
/// 1×1 white pixel so the shader can always sample.
///
/// <para>This is intentionally minimal — multi-page atlases, garbage
/// collection on texture changes, and bindless arrays come later as the V2
/// pipeline matures.</para>
/// </summary>
public sealed class TextureAtlas : IDisposable
{
    public TextureHandle Texture  { get; }
    public SamplerHandle Sampler  { get; }
    public VectorInt2    Size     { get; }

    private readonly IRhiDevice _device;
    private readonly Dictionary<Texture, VectorInt2> _origins = new();
    private readonly VectorInt2 _whitePixel;

    public TextureAtlas(IRhiDevice device, Level level, int atlasSize = 4096)
    {
        _device = device;
        Size = new VectorInt2(atlasSize, atlasSize);

        var atlasBytes = new byte[atlasSize * atlasSize * 4];
        var packer = new RectPackerSimpleStack(Size);

        // Reserve a 1×1 fully-white pixel for untextured / out-of-bounds
        // triangles, so the pixel shader can always do tex × color.
        var wp = packer.TryAdd(new VectorInt2(1, 1))
                 ?? throw new InvalidOperationException("Atlas too small for the reserved white pixel.");
        _whitePixel = wp;
        int wpIdx = (wp.Y * atlasSize + wp.X) * 4;
        atlasBytes[wpIdx + 0] = 255; // B
        atlasBytes[wpIdx + 1] = 255; // G
        atlasBytes[wpIdx + 2] = 255; // R
        atlasBytes[wpIdx + 3] = 255; // A

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
            if (pos == null) continue; // overflow → silently drop, GetUv falls back to white
            _origins[tex] = pos.Value;

            // Blit pixel rows.
            var src = tex.Image.ToByteArray();
            int srcRowBytes = size.X * 4;
            for (int y = 0; y < size.Y; y++)
            {
                int srcRow = y * srcRowBytes;
                int dstRow = ((pos.Value.Y + y) * atlasSize + pos.Value.X) * 4;
                Buffer.BlockCopy(src, srcRow, atlasBytes, dstRow, srcRowBytes);
            }
        }

        Texture = device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, atlasSize, atlasSize,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            debugName: "LevelAtlas"),
            atlasBytes);

        Sampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Linear, AddressMode.Wrap, maxAnisotropy: 4));
    }

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
        return new Vector2(
            (_whitePixel.X + 0.5f) / Size.X,
            (_whitePixel.Y + 0.5f) / Size.Y);
    }

    public void Dispose()
    {
        if (Sampler.IsValid) _device.Destroy(Sampler);
        if (Texture.IsValid) _device.Destroy(Texture);
    }
}
