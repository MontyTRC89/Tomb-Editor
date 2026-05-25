#nullable enable
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering.Graphics.Backends.Dx11;
using TombLib.Rendering.Graphics.Rhi;
using TombLib.Rendering.Graphics.Text;
using TombLib.Wad;
using TextRenderer = TombLib.Rendering.Graphics.Text.TextRenderer;

namespace TombLib.Rendering.Graphics.Preview;

/// <summary>
/// Single shared device used by every preview panel and the offscreen
/// thumbnail capturer. One D3D11 context per process keeps GPU resource use
/// down and lets the mesh / atlas cache in <see cref="WadObjectPreviewRenderer"/>
/// be shared across the item browser, thumbnail batch render, and any future
/// preview surface.
/// </summary>
public static class PreviewDevice
{
    private static IRhiDevice? _device;
    private static bool _ownsDevice;
    private static WadObjectPreviewRenderer? _renderer;
    private static GizmoRenderer? _gizmo;
    private static TextRenderer? _text;
    private static string? _backendPreference;
    private static readonly object _lock = new();

    /// <summary>
    /// Set the RHI backend preference ("Default" / "Vulkan" / "OpenGL" /
    /// "DX11") before the first lazy device creation. The environment
    /// variable <c>TOMBEDITOR_RHI</c> still overrides this. Must be called
    /// before any preview panel paints -- once the device is up, the
    /// preference is fixed.
    /// </summary>
    public static void SetBackendPreference(string? preference)
    {
        lock (_lock)
        {
            if (_device == null) _backendPreference = preference;
        }
    }

    public static IRhiDevice Device
    {
        get { EnsureCreated(); return _device!; }
    }

    public static WadObjectPreviewRenderer Renderer
    {
        get { EnsureCreated(); return _renderer!; }
    }

    /// <summary>Shared V2 gizmo renderer for editor preview overlays.</summary>
    public static GizmoRenderer Gizmo
    {
        get { EnsureCreated(); return _gizmo!; }
    }

    /// <summary>Shared V2 text renderer for preview-panel HUD/debug overlays.</summary>
    public static TextRenderer Text
    {
        get { EnsureCreated(); return _text!; }
    }

    /// <summary>
    /// Human-readable name of the active RHI backend ("Vulkan" / "OpenGL" /
    /// "DirectX 11"), or <c>null</c> if the shared device hasn't been created
    /// yet. Mirrors <c>LevelRenderer.BackendName</c> in TombEditor.
    /// </summary>
    public static string? BackendName
    {
        get
        {
            if (_device == null)
                return null;
            return _device.Capabilities.Backend switch
            {
                RhiBackendKind.Vulkan    => "Vulkan",
                RhiBackendKind.OpenGL    => "OpenGL",
                RhiBackendKind.DirectX11 => "DirectX 11",
                _                        => _device.Capabilities.Backend.ToString(),
            };
        }
    }

    /// <summary>
    /// Adopt an existing <see cref="IRhiDevice"/> instead of creating a fresh
    /// one on first use. Called by <see cref="LevelRenderer"/> at construction
    /// so the preview panel + thumbnail renderer share the same device as
    /// the main 3D viewport -- one device, one atlas cache, no extra ~100 ms
    /// device-creation hit on the first thumbnail batch.
    /// </summary>
    public static void RegisterSharedDevice(IRhiDevice device)
    {
        lock (_lock)
        {
            if (_renderer != null)
                return;
            _device     = device;
            _ownsDevice = false;
            _renderer   = new WadObjectPreviewRenderer(_device);
            _gizmo      = new GizmoRenderer(_device);
            _text       = new TextRenderer(_device);
        }
    }

    private static void EnsureCreated()
    {
        if (_renderer != null)
            return;
        lock (_lock)
        {
            if (_renderer != null)
                return;
            _device     = TombLib.Rendering.Graphics.Rhi.RhiBackend.Create(_backendPreference);
            _ownsDevice = true;
            _renderer   = new WadObjectPreviewRenderer(_device);
            _gizmo      = new GizmoRenderer(_device);
            _text       = new TextRenderer(_device);
        }
    }

    /// <summary>Drop every cached per-object mesh / atlas -- call on WAD reload.</summary>
    public static void InvalidateAll() => _renderer?.InvalidateAll();

    /// <summary>
    /// Dump every live preview atlas as a PNG into <paramref name="directory"/>.
    /// Wired to the WadTool debug menu so developers can inspect packing /
    /// gutter behaviour without attaching a graphics debugger.
    /// </summary>
    public static System.Collections.Generic.IReadOnlyList<string> DumpAtlases(string directory)
        => _renderer?.DumpAtlases(directory) ?? Array.Empty<string>();
}

/// <summary>
/// WinForms control that renders a single <see cref="IWadObject"/> using the
/// device. Replaces the legacy <c>PanelItemPreview</c> for the item
/// browser preview. Camera + mouse navigation kept compatible so the rest of
/// the editor needs no changes.
/// </summary>
public abstract class ItemPreviewPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IWadObject? CurrentObject
    {
        get => _currentObject;
        set
        {
            if (ReferenceEquals(_currentObject, value))
                return;
            _currentObject = value;
            // Match the legacy panel: do NOT call ResetCamera here. Callers
            // that need a fresh frame call ResetCamera() explicitly (so that
            // a code path reaching this setter before _editor / Configuration
            // are fully initialised doesn't blow up via the abstract
            // FieldOfView accessor).
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ArcBallCamera Camera { get; set; } = MakeDefaultCamera(45.0f);

    public bool AnimatePreview
    {
        get => _animatePreview;
        set
        {
            if (_animatePreview == value)
                return;
            _animatePreview = value;
            _animTimer.Enabled = value;
            _rotationFactor = 0.0f;
        }
    }
    private bool _animatePreview = true;

    /// <summary>
    /// Kept for designer compatibility with the old <c>PanelItemPreview</c>
    /// (transparency was a per-control switch in the legacy renderer). The
    /// preview pipeline does not yet implement alpha-blended draws -- the flag
    /// is accepted but has no effect.
    /// </summary>
    public bool DrawTransparency { get; set; } = false;

    private IWadObject? _currentObject;
    private SwapchainHandle _swap;
    private int _width, _height;
    private bool _swapchainReady;

    /// <summary>Render-target width inside the current OnPaint pass.</summary>
    protected int ViewportWidth  => _width;
    /// <summary>Render-target height inside the current OnPaint pass.</summary>
    protected int ViewportHeight => _height;

    private readonly Timer _animTimer = new() { Interval = 15 };
    private float _rotationFactor;
    private const float _rotationSpeed = 0.005f;
    private const float _rotationStep  = 0.000125f;

    private float _lastX, _lastY;

    // Per-frame scratch for text overlays. Cleared at the start of each paint
    // and refilled by CollectText overrides.
    private readonly System.Collections.Generic.List<TextLabel> _textLabels = new();

    protected ItemPreviewPanel()
    {
        BorderStyle = BorderStyle.None;
        // Don't enable OptimizedDoubleBuffer / UserPaint / AllPaintingInWmPaint
        // here -- those make WinForms allocate a GDI back buffer and BitBlt it
        // over the window each WM_PAINT, which wipes the DXGI swapchain's
        // presented frame to black between draws. The legacy RenderingPanel
        // also leaves these alone for the same reason.
        _animTimer.Tick += OnAnimTick;
    }

    public void ResetCamera()
    {
        if (_currentObject == null || !IsValid(_currentObject))
        {
            Camera = MakeDefaultCamera(FieldOfView);
            return;
        }
        var (center, radius) = WadObjectPreviewHelper.ComputeBoundingSphere(_currentObject);
        radius = Math.Max(radius * 1.15f, 50.0f);
        Camera = new ArcBallCamera(center, MathC.DegToRad(35), MathC.DegToRad(35),
            -(float)Math.PI / 2, (float)Math.PI / 2,
            radius * 3, 50, 1_000_000, FieldOfView * (float)(Math.PI / 180));
    }

    private static ArcBallCamera MakeDefaultCamera(float fovDegrees) =>
        new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f),
            0, 0, -(float)Math.PI / 2, (float)Math.PI / 2,
            2048.0f, 100, 1_000_000, fovDegrees * (float)(Math.PI / 180));

    private static bool IsValid(IWadObject obj)
    {
        return obj switch
        {
            WadMoveable mv  => mv.Meshes.Any(m => m != null && m.VertexPositions.Count > 0),
            WadStatic   st  => st.Mesh != null && st.Mesh.VertexPositions.Count > 0,
            ImportedGeometry => true,
            _ => false,
        };
    }

    public void GarbageCollect()
    {
        // Per-object cache is shared and dropped centrally; keep this method
        // for API compatibility with the old legacy panel.
    }

    private void EnsureSwapchain()
    {
        if (_swapchainReady)
            return;
        if (!IsHandleCreated || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;

        _width  = ClientSize.Width;
        _height = ClientSize.Height;
        _swap = PreviewDevice.Device.CreateSwapchain(new SwapchainDesc(
            hwnd:    Handle,
            width:   _width,
            height:  _height,
            color:   Format.R8G8B8A8_UNorm,
            depth:   Format.D24_UNorm_S8_UInt,
            samples: 4,
            vsync:   true));
        _swapchainReady = true;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (!_swapchainReady)
            return;
        int w = Math.Max(1, ClientSize.Width);
        int h = Math.Max(1, ClientSize.Height);
        if (w == _width && h == _height)
            return;
        PreviewDevice.Device.WaitIdle();
        PreviewDevice.Device.ResizeSwapchain(_swap, w, h);
        _width  = w;
        _height = h;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Suppress GDI background paint -- the swapchain owns the surface.
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        EnsureSwapchain();
        if (!_swapchainReady)
        {
            e.Graphics.Clear(Parent?.BackColor ?? Color.Black);
            return;
        }

        var device = PreviewDevice.Device;

        var viewProjection = Camera.GetViewProjectionMatrix(_width, _height);

        // Collect any text overlays. Text atlas updates MUST run before
        // BeginCommandList -- TextRenderer.Prepare does its own one-shot
        // submit, which would deadlock the frame's command buffer.
        _textLabels.Clear();
        CollectText(_textLabels, viewProjection);
        if (_textLabels.Count > 0)
            PreviewDevice.Text.Prepare(_textLabels);

        var cl = device.BeginCommandList();

        var clear = ClearColor;
        var clearColors = new[] { clear };

        cl.BeginPass(new PassDesc
        {
            UseSwapchain   = true,
            Swapchain      = _swap,
            ColorLoadOps   = new[] { LoadOp.Clear },
            ColorStoreOps  = new[] { StoreOp.Store },
            DepthLoadOp    = LoadOp.Clear,
            DepthStoreOp   = StoreOp.Store,
            ClearColors    = clearColors,
            ClearDepth     = 1.0f,
            ViewportWidth  = _width,
            ViewportHeight = _height,
        });

        RenderContents(cl, viewProjection);

        if (_textLabels.Count > 0)
            PreviewDevice.Text.Render(cl, _textLabels, viewProjection, _width, _height);

        cl.EndPass();
        device.Submit(cl);
        device.Present(_swap);
    }

    /// <summary>
    /// Override hook for subclasses that need to draw screen / world text
    /// labels over the rendered scene. Default implementation emits nothing.
    /// </summary>
    protected virtual void CollectText(System.Collections.Generic.List<TextLabel> labels, Matrix4x4 viewProjection)
    {
    }

    /// <summary>
    /// Override hook for subclasses that need to draw additional geometry
    /// (skeleton bones, gizmo overlay, wireframe grid, ...) inside the same
    /// swapchain pass. The default implementation draws <see cref="CurrentObject"/>
    /// -- matching the simple item-browser behaviour.
    /// </summary>
    protected virtual void RenderContents(ICommandList cl, Matrix4x4 viewProjection)
    {
        if (_currentObject != null && IsValid(_currentObject))
            PreviewDevice.Renderer.Render(cl, _currentObject, viewProjection);
    }

    private void OnAnimTick(object? sender, EventArgs e)
    {
        if (!AnimatePreview || _currentObject == null)
            return;
        if (Form.ActiveForm != FindForm())
            return;
        if (_rotationFactor < _rotationSpeed) _rotationFactor += _rotationStep;
        Camera.Rotate(_rotationFactor, 0.0f);
        Invalidate();
    }

    // ------------------------------------------------- Mouse / wheel handling

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (!Focused && Form.ActiveForm == FindForm()) Focus();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Camera.Zoom(-e.Delta * NavigationSpeedMouseWheelZoom);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _lastX = e.X; _lastY = e.Y;
        if (e.Button != MouseButtons.Left)
        {
            _animTimer.Stop();
            _rotationFactor = 0.0f;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _animTimer.Start();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // Editor panels (skeleton / static / animation / mesh) don't set
        // CurrentObject -- they own their own scene state. Camera nav must
        // run regardless of whether CurrentObject is populated; otherwise
        // right/middle drag is a no-op for every editor.
        if (e.Button != MouseButtons.Right && e.Button != MouseButtons.Middle)
            return;

        float dx = (e.X - _lastX) / Height;
        float dy = (e.Y - _lastY) / Height;
        _lastX = e.X; _lastY = e.Y;

        if (e.Button == MouseButtons.Right)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
                Camera.Zoom(-dy * NavigationSpeedMouseZoom);
            else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                Camera.Rotate(dx * NavigationSpeedMouseRotate, -dy * NavigationSpeedMouseRotate);
        }

        if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
             e.Button == MouseButtons.Middle)
            Camera.MoveCameraPlane(new Vector3(dx, dy, 0) * NavigationSpeedMouseTranslate);

        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animTimer.Dispose();
            if (_swapchainReady && _swap.IsValid)
            {
                PreviewDevice.Device.WaitIdle();
                PreviewDevice.Device.Destroy(_swap);
                _swapchainReady = false;
            }
        }
        base.Dispose(disposing);
    }

    protected virtual Vector4 ClearColor { get; } = new Vector4(0.392f, 0.584f, 0.929f, 1.0f);

    public abstract float FieldOfView { get; }
    public abstract float NavigationSpeedMouseWheelZoom { get; }
    public abstract float NavigationSpeedMouseZoom { get; }
    public abstract float NavigationSpeedMouseTranslate { get; }
    public abstract float NavigationSpeedMouseRotate { get; }

    /// <summary>When true, the panel suppresses drag-source behaviour -- used
    /// by the imported-geometry browser and the WAD preview window.</summary>
    public virtual bool ReadOnly => false;
}
