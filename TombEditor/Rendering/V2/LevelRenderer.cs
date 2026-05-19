using System;
using System.Numerics;
using TombLib.RenderingV2.Backends.Dx11;
using TombLib.RenderingV2.Rhi;

namespace TombEditor.Rendering.V2;

/// <summary>
/// V2 renderer for the editor's main 3D viewport (Panel3D). Owns a single
/// <see cref="Dx11Device"/> + swapchain bound to the host control's HWND.
///
/// <para>Current state: scaffolding only. Each frame clears the swapchain to
/// a distinctive cyan so we can visually verify when the V2 path is active.
/// Room geometry, objects, gizmo, sprites etc. will be added pass-by-pass.</para>
///
/// <para>The renderer is owned by Panel3D and follows its lifetime: created
/// in InitializeRendering when <c>Configuration.Rendering3D_UseV2Renderer</c>
/// is set, resized on the panel's <see cref="System.Windows.Forms.Control.Resize"/>,
/// disposed when Panel3D is disposed.</para>
/// </summary>
public sealed class LevelRenderer : IDisposable
{
    private readonly Dx11Device     _device;
    private SwapchainHandle         _swap;
    private int                     _width;
    private int                     _height;

    /// <summary>The underlying RHI device, exposed for pass implementations.</summary>
    public IRhiDevice Device => _device;

    /// <summary>The swapchain bound to the host HWND.</summary>
    public SwapchainHandle Swapchain => _swap;

    public LevelRenderer(IntPtr hwnd, int width, int height)
    {
        _width  = Math.Max(1, width);
        _height = Math.Max(1, height);
        _device = new Dx11Device();

        _swap = _device.CreateSwapchain(new SwapchainDesc(
            hwnd:    hwnd,
            width:   _width,
            height:  _height,
            color:   Format.R8G8B8A8_UNorm,
            depth:   Format.D24_UNorm_S8_UInt,
            samples: 1,
            vsync:   true));
    }

    public void Resize(int width, int height)
    {
        width  = Math.Max(1, width);
        height = Math.Max(1, height);
        if (width == _width && height == _height) return;

        _device.WaitIdle();
        _device.ResizeSwapchain(_swap, width, height);
        _width  = width;
        _height = height;
    }

    /// <summary>
    /// Render one frame and present.
    /// Stub: clears to a distinct cyan so the V2 path is visually
    /// recognisable while we wire up passes.
    /// </summary>
    public void RenderFrame()
    {
        var cl = _device.BeginCommandList();
        cl.BeginPass(new PassDesc
        {
            UseSwapchain   = true,
            Swapchain      = _swap,
            ColorLoadOps   = new[] { LoadOp.Clear },
            ColorStoreOps  = new[] { StoreOp.Store },
            DepthLoadOp    = LoadOp.Clear,
            DepthStoreOp   = StoreOp.Store,
            ClearColors    = new[] { new Vector4(0.0f, 0.35f, 0.45f, 1.0f) }, // distinct cyan
            ClearDepth     = 1.0f,
            ViewportWidth  = _width,
            ViewportHeight = _height,
        });
        cl.EndPass();
        _device.Submit(cl);
        _device.Present(_swap);
    }

    public void Dispose()
    {
        _device.WaitIdle();
        if (_swap.IsValid)
        {
            _device.Destroy(_swap);
            _swap = default;
        }
        _device.Dispose();
    }
}
