using System;
using System.Numerics;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.RenderingV2.Rhi;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Renders a single <see cref="IWadObject"/> off-screen and returns the
/// captured frame as a BGRA <see cref="ImageC"/>. Replaces the legacy
/// <c>OffscreenItemRenderer</c> for the content browser thumbnail batch.
///
/// <para>The render target is allocated as B8G8R8A8 so the readback bytes
/// can be handed straight to <see cref="ImageC.FromByteArray"/> with no
/// channel swizzle.</para>
/// </summary>
public sealed class OffscreenObjectRendererV2 : IDisposable
{
    private readonly IRhiDevice _device;
    private readonly WadObjectPreviewRenderer _renderer;

    private TextureHandle _color;
    private TextureHandle _depth;
    private int _size;

    private static readonly Vector4 _defaultClear = new(0.392f, 0.584f, 0.929f, 1f);

    public OffscreenObjectRendererV2()
    {
        _device   = V2PreviewDevice.Device;
        _renderer = V2PreviewDevice.Renderer;
    }

    public ImageC RenderThumbnail(IWadObject? obj, TRVersion.Game gameVersion, Vector4 backColor, int size = 128)
    {
        if (obj == null || size <= 0) return ImageC.CreateNew(Math.Max(1, size), Math.Max(1, size));

        EnsureTargets(size);

        // Camera framed on the object.
        var (center, radius) = WadObjectPreviewHelper.ComputeBoundingSphere(obj);
        radius = Math.Max(radius * 1.15f, 50f);
        const float Fov = 50f;
        var camera = new ArcBallCamera(center,
            MathC.DegToRad(35), MathC.DegToRad(35),
            -(float)Math.PI / 2, (float)Math.PI / 2,
            radius * 3, 50, 1_000_000, Fov * (float)(Math.PI / 180));
        var viewProjection = camera.GetViewProjectionMatrix(size, size);

        // Render pass into the offscreen color / depth pair.
        var cl = _device.BeginCommandList();
        cl.BeginPass(new PassDesc
        {
            UseSwapchain    = false,
            ColorAttachments = new[] { _color },
            DepthAttachment  = _depth,
            ColorLoadOps     = new[] { LoadOp.Clear },
            ColorStoreOps    = new[] { StoreOp.Store },
            DepthLoadOp      = LoadOp.Clear,
            DepthStoreOp     = StoreOp.Store,
            ClearColors      = new[] { backColor },
            ClearDepth       = 1f,
            ViewportWidth    = size,
            ViewportHeight   = size,
        });

        _renderer.Render(cl, obj, viewProjection);

        cl.EndPass();
        _device.Submit(cl);

        // Sync readback — small (128×128×4 = 64 KB typical) so the stall is
        // brief. The Map call is mandatory because WPF needs a frozen
        // BitmapSource on the UI thread.
        byte[] bgraBytes = _device.ReadTexture(_color);
        return ImageC.FromByteArray(bgraBytes, size, size);
    }

    private void EnsureTargets(int size)
    {
        if (_size == size && _color.IsValid && _depth.IsValid) return;

        if (_color.IsValid) _device.Destroy(_color);
        if (_depth.IsValid) _device.Destroy(_depth);

        _size = size;
        _color = _device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, size, size,
                            Format.B8G8R8A8_UNorm,
                            TextureBindFlags.RenderTarget | TextureBindFlags.ShaderResource,
                            mipLevels: 1,
                            debugName: "ThumbnailColor"),
            ReadOnlySpan<byte>.Empty);
        _depth = _device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, size, size,
                            Format.D24_UNorm_S8_UInt,
                            TextureBindFlags.DepthStencil,
                            mipLevels: 1,
                            debugName: "ThumbnailDepth"),
            ReadOnlySpan<byte>.Empty);
    }

    public void GarbageCollect()
    {
        // Cached object meshes / atlases are owned by the shared
        // V2PreviewDevice.Renderer; let the caller (ContentBrowser) decide
        // when to invalidate via V2PreviewDevice.InvalidateAll().
    }

    public void Dispose()
    {
        if (_color.IsValid) _device.Destroy(_color);
        if (_depth.IsValid) _device.Destroy(_depth);
        _color = default;
        _depth = default;
    }
}
