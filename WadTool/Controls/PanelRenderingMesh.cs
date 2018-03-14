using SharpDX.Toolkit.Graphics;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace WadTool.Controls
{
    public class PanelRenderingMesh : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadMesh Mesh { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }

        private GraphicsDevice _device;
        private DeviceManager _deviceManager;
        private SwapChainGraphicsPresenter _presenter;
        private RasterizerState _rasterizerWireframe;
        private WadToolClass _tool;
        private VertexInputLayout _layout;
        private float _lastX;
        private float _lastY;
        private SpriteBatch _spriteBatch;

        public void InitializePanel(WadToolClass tool, DeviceManager deviceManager)
        {
            _tool = tool;
            _device = deviceManager.Device;
            _deviceManager = deviceManager;

            // Initialize the viewport, after the panel is added and sized on the form
            var pp = new PresentationParameters
            {
                BackBufferFormat = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                BackBufferWidth = ClientSize.Width,
                BackBufferHeight = ClientSize.Height,
                DepthStencilFormat = DepthFormat.Depth24Stencil8,
                DeviceWindowHandle = this,
                IsFullScreen = false,
                MultiSampleCount = MSAALevel.None,
                PresentationInterval = PresentInterval.Immediate,
                RenderTargetUsage = SharpDX.DXGI.Usage.RenderTargetOutput | SharpDX.DXGI.Usage.BackBuffer,
                Flags = SharpDX.DXGI.SwapChainFlags.None
            };

            _presenter = new SwapChainGraphicsPresenter(_device, pp);

            Camera = new ArcBallCamera(new Vector3(0.0f, 0.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 1024.0f, 0, 1000000, (float)Math.PI / 4.0f);

            // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
            new BasicEffect(_device);

            // Initialize the rasterizer state for wireframe drawing
            SharpDX.Direct3D11.RasterizerStateDescription renderStateDesc =
                new SharpDX.Direct3D11.RasterizerStateDescription
                {
                    CullMode = SharpDX.Direct3D11.CullMode.None,
                    DepthBias = 0,
                    DepthBiasClamp = 0,
                    FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
                    IsAntialiasedLineEnabled = true,
                    IsDepthClipEnabled = true,
                    IsFrontCounterClockwise = false,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0
                };

            _rasterizerWireframe = RasterizerState.New(_device, renderStateDesc);

            _spriteBatch = new SpriteBatch(_device);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_device == null || _presenter == null)
                e.Graphics.FillRectangle(Brushes.White, ClientRectangle);
            // Don't paint the background
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw();
        }

        public void Draw()
        {
            if (_device == null || _presenter == null)
                return;

            _device.Presenter = _presenter;
            _device.SetViewports(new SharpDX.ViewportF(0, 0, ClientSize.Width, ClientSize.Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer,
                _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, SharpDX.Color.CornflowerBlue, 1.0f, 0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            _device.SetBlendState(_device.BlendStates.Opaque);

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);

            Effect solidEffect = _deviceManager.Effects["Solid"];

            if (Mesh != null)
            {
                var mesh = _tool.DestinationWad.DirectXMeshes[Mesh];
                var effect = _deviceManager.Effects["StaticModel"];
                var world = Matrix4x4.Identity;

                effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["Texture"].SetResource(_tool.DestinationWad.DirectXTexture);
                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                _device.SetVertexBuffer(0, mesh.VertexBuffer);
                _device.SetIndexBuffer(mesh.IndexBuffer, true);
                _layout = VertexInputLayout.FromBuffer(0, mesh.VertexBuffer);
                _device.SetVertexInputLayout(_layout);

                effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                effect.Techniques[0].Passes[0].Apply();

                foreach (var submesh in mesh.Submeshes)
                    _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
            }

            _device.Present();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_presenter != null)
            {
                _presenter.Resize(ClientSize.Width, ClientSize.Height, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * _tool.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom);
            Invalidate();
        }

        private Ray GetRay(float x, float y)
        {
            return new SharpDX.ViewportF(0, 0, ClientSize.Width, ClientSize.Height).GetPickRay(new Vector2(x, y),
                Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Right)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / Height;
                float deltaY = (e.Y - _lastY) / Height;

                _lastX = e.X;
                _lastY = e.Y;

                if ((ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Zoom(-deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                else if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.MoveCameraPlane(new Vector3(-deltaX, -deltaY, 0) * _tool.Configuration.RenderingItem_NavigationSpeedMouseTranslate);
                else
                    Camera.Rotate(deltaX * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate,
                                  -deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }
    }
}
