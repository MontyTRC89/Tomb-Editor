using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TombLib.Rendering.Graphics.Backends.Dx11;
using TombLib.Rendering.Graphics.Rhi;

namespace TombLib.Rendering.Graphics.Sample;

// Smoke test for the V2 DX11 backend. Opens a window, creates a swapchain,
// uploads a textured triangle and renders it on every Application.Idle tick.
// If you see a rotating triangle with a red/black checker pattern modulated
// by a slowly oscillating tint, the whole RHI is live end-to-end:
//   swapchain present, pipeline, vertex buffer, push constants emulation,
//   sampler + texture binding, draw call.

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            using var view = new RenderForm();
            Application.Run(view);
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "DX11 sample failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }
}

internal sealed unsafe class RenderForm : Form
{
    private readonly Dx11Device     _dev;
    private SwapchainHandle         _swap;
    private PipelineHandle          _pipe;
    private BufferHandle            _vb;
    private BufferHandle            _viewCb;
    private TextureHandle           _tex;
    private SamplerHandle           _samp;
    private readonly Stopwatch      _clock = Stopwatch.StartNew();
    private int                     _frameCount;
    private double                  _lastFpsTime;

    // 3 vertices, 20 bytes each: float3 position + float2 uv.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Vertex
    {
        public float Px, Py, Pz;
        public float U, V;
        public Vertex(float px, float py, float pz, float u, float v)
        { Px = px; Py = py; Pz = pz; U = u; V = v; }
    }

    // Layout has to match the b0 ViewParams cbuffer in Hello.hlsl.
    // float4x4 Mvp (64B) + float4 Tint (16B) = 80B; padded to 256 for the
    // dynamic CB minimum alignment.
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct ViewParams
    {
        public Matrix4x4 Mvp;
        public Vector4   Tint;
    }

    public RenderForm()
    {
        Text          = "TombLib.Rendering.Graphics — DX11 Sample";
        ClientSize    = new System.Drawing.Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        DoubleBuffered = false;

        _dev = new Dx11Device();

        // Swapchain bound to this Form's HWND.
        _swap = _dev.CreateSwapchain(new SwapchainDesc(
            hwnd:    Handle,
            width:   ClientSize.Width,
            height:  ClientSize.Height,
            color:   Format.R8G8B8A8_UNorm,
            depth:   Format.D24_UNorm_S8_UInt,
            samples: 1,
            vsync:   true));

        // Triangle in clip space.
        var verts = new[]
        {
            new Vertex( 0.0f,  0.6f, 0.0f, 0.5f, 0.0f),
            new Vertex( 0.6f, -0.6f, 0.0f, 1.0f, 1.0f),
            new Vertex(-0.6f, -0.6f, 0.0f, 0.0f, 1.0f),
        };
        var vbBytes = new byte[verts.Length * sizeof(Vertex)];
        fixed (Vertex* p = verts)
        fixed (byte*   d = vbBytes)
            Buffer.MemoryCopy(p, d, vbBytes.Length, vbBytes.Length);
        _vb = _dev.CreateBuffer(
            new BufferDesc(vbBytes.Length, BufferUsage.Immutable, BufferBindFlags.Vertex, debugName: "TriangleVB"),
            vbBytes);

        // View / tint cbuffer, dynamic.
        _viewCb = _dev.CreateBuffer(
            new BufferDesc(sizeof(ViewParams), BufferUsage.DynamicUniform, BufferBindFlags.Constant, debugName: "ViewParams"),
            ReadOnlySpan<byte>.Empty);

        // 4x4 RGBA checkerboard texture, red / black.
        var tex = new byte[4 * 4 * 4];
        for (int y = 0; y < 4; y++)
        for (int x = 0; x < 4; x++)
        {
            int i = (y * 4 + x) * 4;
            bool dark = ((x + y) & 1) == 0;
            tex[i + 0] = (byte)(dark ? 25  : 230);
            tex[i + 1] = 25;
            tex[i + 2] = 25;
            tex[i + 3] = 255;
        }
        _tex = _dev.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, 4, 4, Format.R8G8B8A8_UNorm,
                            TextureBindFlags.ShaderResource, debugName: "Checker4x4"),
            tex);

        _samp = _dev.CreateSampler(new SamplerDesc(FilterMode.Nearest, AddressMode.Clamp));

        // Pipeline.
        var (vs, ps) = ShaderLibrary.Load("Hello");
        var pipeDesc = new PipelineDesc
        {
            VertexShader        = vs,
            FragmentShader      = ps,
            VertexAttributes    = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float, bufferSlot: 0, offset: 0),
                new VertexAttribute("TEXCOORD", 0, Format.R32G32_Float,    bufferSlot: 0, offset: 12),
            },
            VertexBufferLayouts = new[] { new VertexBufferLayout(sizeof(Vertex)) },
            Topology            = PrimitiveTopology.TriangleList,
            Rasterizer          = new RasterizerState(CullMode.None),  // no backface culling, simpler
            DepthStencil        = DepthStencilState.DepthReadOnly,
            BlendStates         = new[] { BlendState.Opaque },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "HelloPipeline",
        };
        _pipe = _dev.CreatePipeline(pipeDesc);

        // Drive a continuous render loop on idle.
        Application.Idle += OnIdle;
        FormClosed       += (_, _) => Application.Idle -= OnIdle;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (_swap.IsValid && ClientSize.Width > 0 && ClientSize.Height > 0)
        {
            _dev.WaitIdle();
            _dev.ResizeSwapchain(_swap, ClientSize.Width, ClientSize.Height);
        }
    }

    private void OnIdle(object? sender, EventArgs e)
    {
        while (IsAppIdle()) Frame();
    }

    private void Frame()
    {
        float t = (float)_clock.Elapsed.TotalSeconds;

        // Animate the MVP and tint to prove the loop is alive.
        var mvp  = Matrix4x4.CreateRotationZ(t * 1.2f);
        var tint = new Vector4(1.0f, 0.7f + 0.3f * MathF.Sin(t * 2f), 1.0f, 1.0f);
        var vp   = new ViewParams { Mvp = Matrix4x4.Transpose(mvp), Tint = tint };

        // Update the dynamic view CB.
        var cl = _dev.BeginCommandList();
        var vpBytes = new ReadOnlySpan<byte>(&vp, sizeof(ViewParams));
        cl.UpdateBuffer(_viewCb, 0, vpBytes);

        // Pass.
        var pass = new PassDesc
        {
            UseSwapchain    = true,
            Swapchain       = _swap,
            ColorLoadOps    = new[] { LoadOp.Clear },
            ColorStoreOps   = new[] { StoreOp.Store },
            DepthLoadOp     = LoadOp.Clear,
            DepthStoreOp    = StoreOp.Store,
            ClearColors     = new[] { new Vector4(0.08f, 0.08f, 0.12f, 1f) },
            ClearDepth      = 1f,
            ClearStencil    = 0,
            ViewportWidth   = ClientSize.Width,
            ViewportHeight  = ClientSize.Height,
        };
        cl.BeginPass(in pass);
        cl.SetPipeline(_pipe);

        // Bindings: ViewParams at b0, checker tex at t0, sampler at s0.
        var cbufs = new[] { _viewCb };
        var texes = new[] { _tex };
        var samps = new[] { _samp };
        cl.SetBindings(new Bindings
        {
            ConstantBuffers = cbufs,
            Textures        = texes,
            Samplers        = samps,
        });

        var vbs = new[] { new VertexBufferBinding(_vb, 0) };
        cl.SetVertexBuffers(vbs);
        cl.Draw(vertexCount: 3);

        cl.EndPass();
        _dev.Submit(cl);
        _dev.Present(_swap);

        // FPS counter every second.
        _frameCount++;
        double now = _clock.Elapsed.TotalSeconds;
        if (now - _lastFpsTime >= 1.0)
        {
            double fps = _frameCount / (now - _lastFpsTime);
            Text = $"TombLib.Rendering.Graphics — DX11 Sample — {fps:F0} fps";
            _frameCount  = 0;
            _lastFpsTime = now;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dev.WaitIdle();
            _dev.Destroy(_pipe);
            _dev.Destroy(_samp);
            _dev.Destroy(_tex);
            _dev.Destroy(_viewCb);
            _dev.Destroy(_vb);
            _dev.Destroy(_swap);
            _dev.Dispose();
        }
        base.Dispose(disposing);
    }

    // --- WinForms idle-loop helpers ---------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMessage
    {
        public IntPtr  hWnd;
        public uint    Msg;
        public IntPtr  WParam;
        public IntPtr  LParam;
        public uint    Time;
        public int     X, Y;
    }

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out NativeMessage msg, IntPtr hWnd, uint min, uint max, uint flag);

    private static bool IsAppIdle() => !PeekMessage(out _, IntPtr.Zero, 0, 0, 0);
}
