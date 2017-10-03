using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingStaticMeshEditor : Panel
    {
        public WadStatic StaticMesh { get; set; }
        public ArcBallCamera Camera { get; set; }

        private GraphicsDevice _device;
        private SwapChainGraphicsPresenter _presenter;
        private RasterizerState _rasterizerWireframe;
        private WadToolClass _tool;
        private VertexInputLayout _layout;
        private float _lastX;
        private float _lastY;
        private SpriteBatch _spriteBatch;
        private GizmoStaticMeshEditor _gizmo;
        private GeometricPrimitive _plane;

        public Vector3 StaticPosition { get; set; } = Vector3.Zero;
        public Vector3 StaticRotation { get; set; } = Vector3.Zero;
        public float StaticScale { get; set; } = 1.0f;

        public void InitializePanel(GraphicsDevice device)
        {
            _device = device;

            // Initialize the viewport, after the panel is added and sized on the form
            var pp = new PresentationParameters
            {
                BackBufferFormat = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                BackBufferWidth = Width,
                BackBufferHeight = Height,
                DepthStencilFormat = DepthFormat.Depth24Stencil8,
                DeviceWindowHandle = this,
                IsFullScreen = false,
                MultiSampleCount = MSAALevel.None,
                PresentationInterval = PresentInterval.Immediate,
                RenderTargetUsage = SharpDX.DXGI.Usage.RenderTargetOutput | SharpDX.DXGI.Usage.BackBuffer,
                Flags = SharpDX.DXGI.SwapChainFlags.None
            };

            _presenter = new SwapChainGraphicsPresenter(_device, pp);

            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 2048.0f, 0, 1000000, (float)Math.PI / 4.0f);

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

            _tool = WadToolClass.Instance;
            _spriteBatch = new SpriteBatch(_tool.Device);
            _gizmo = new GizmoStaticMeshEditor(_tool.Device, _tool.Effects["Solid"], this);
            _plane = GeometricPrimitive.GridPlane.New(_tool.Device, 8, 4);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_device == null || _presenter == null)
                e.Graphics.FillRectangle(Brushes.White, new System.Drawing.Rectangle(0, 0, Width, Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_device == null || _presenter == null)
                e.Graphics.FillRectangle(Brushes.White, new System.Drawing.Rectangle(0, 0, Width, Height));
            else
                Draw();
        }

        public void Draw()
        {
            if (_device == null || _presenter == null) return;

            _device.Presenter = _presenter;
            _device.SetViewports(new ViewportF(0, 0, Width, Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer,
                _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, SharpDX.Color.CornflowerBlue, 1.0f, 0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            _device.SetBlendState(_device.BlendStates.Opaque);

            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);

            if (StaticMesh != null)
            {
                var model = _tool.DestinationWad.DirectXStatics[StaticMesh.ObjectID];

                Effect mioEffect = _tool.Effects["StaticModel"];

                var world = Matrix.Scaling(StaticScale) *
                            Matrix.RotationYawPitchRoll(StaticRotation.Y, StaticRotation.X, StaticRotation.Z) *
                            Matrix.Translation(StaticPosition);

                mioEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);

                mioEffect.Parameters["Color"].SetValue(Vector4.One);

                mioEffect.Parameters["Texture"].SetResource(_tool.DestinationWad.DirectXTexture);
                mioEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                _device.SetVertexBuffer(0, model.VertexBuffer);
                _device.SetIndexBuffer(model.IndexBuffer, true);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    StaticMesh mesh = model.Meshes[i];

                    _layout = VertexInputLayout.FromBuffer<StaticVertex>(0, mesh.VertexBuffer);
                    _device.SetVertexInputLayout(_layout);
                    
                    mioEffect.Parameters["ModelViewProjection"].SetValue(world * viewProjection);
                    mioEffect.Techniques[0].Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);
                }
            }

            // Draw the grid
            _device.SetVertexBuffer(0, _plane.VertexBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<SolidVertex>(0, _plane.VertexBuffer));
            _device.SetIndexBuffer(_plane.IndexBuffer, true);

            Effect effect = _tool.Effects["Solid"];
            effect.Parameters["ModelViewProjection"].SetValue(viewProjection);
            effect.Parameters["Color"].SetValue(Vector4.One);
            effect.Techniques[0].Passes[0].Apply();
            
            _device.Draw(PrimitiveType.LineList, _plane.VertexBuffer.ElementCount);

            // Draw the gizmo
            _gizmo.Draw(viewProjection);

            // Draw debug strings
            _spriteBatch.Begin(SpriteSortMode.Immediate, _device.BlendStates.AlphaBlend);

            _spriteBatch.DrawString(_tool.Font,
                                    "Position: [" + StaticPosition.X + ", " + StaticPosition.Y + ", " + StaticPosition.Z + "]",
                                    new Vector2(0, 0),
                                    SharpDX.Color.White);
            _spriteBatch.DrawString(_tool.Font,
                                    "Rotation: [" + MathUtil.RadiansToDegrees(StaticRotation.X) + ", " +
                                                    MathUtil.RadiansToDegrees(StaticRotation.Y) + ", " +
                                                    MathUtil.RadiansToDegrees(StaticRotation.Z) + "]",
                                    new Vector2(0, 18),
                                    SharpDX.Color.White);
            _spriteBatch.DrawString(_tool.Font,
                                    "Scale: " + StaticScale,
                                    new Vector2(0, 36),
                                    SharpDX.Color.White);

            _spriteBatch.End();

            _device.Present();
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

            Camera.Zoom(-e.Delta * 46000 /*_.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom*/);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Ray ray = Ray.GetPickRay((int)Math.Round((double)e.X), (int)Math.Round((double)e.Y),
                                     new ViewportF(0, 0, Width, Height), Camera.GetViewProjectionMatrix(Width, Height));

            if (e.Button == MouseButtons.Left)
            {
                var result = _gizmo.DoPicking(ray);
                if (result == null) return;
                _gizmo.SetGizmoAxis(result.Axis);
                return;
            }

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
            {
                _gizmo.MouseMoved(Camera.GetViewProjectionMatrix(Width, Height), (int)e.X, (int)e.Y);
            }

            if (e.Button == MouseButtons.Right)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / (float)Height;
                float deltaY = (e.Y - _lastY) / (float)Height;

                _lastX = e.X;
                _lastY = e.Y;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Zoom(-deltaY * 46000f /*_editor.Configuration.RenderingItem_NavigationSpeedMouseZoom*/);
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.MoveCameraPlane(new Vector3(-deltaX, -deltaY, 0) * 22000f /* _editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate*/);
                else
                    Camera.Rotate(deltaX * 2.2f /*_editor.Configuration.RenderingItem_NavigationSpeedMouseRotate*/,
                                  -deltaY * 2.2f /*_editor.Configuration.RenderingItem_NavigationSpeedMouseRotate*/);
                Invalidate();
            }
        }
    }
}
