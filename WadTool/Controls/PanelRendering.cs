using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRendering : Panel
    {
        public Wad2 CurrentWad { get; set; }
        public IRenderableObject CurrentObject { get; set; }
        public ArcBallCamera Camera { get; set; }

        private GraphicsDevice _device;
        private SwapChainGraphicsPresenter _presenter;
        private RasterizerState _rasterizerWireframe;
        private WadToolClass _tool;
        private VertexInputLayout _layout;
        private float _lastX;
        private float _lastY;
        private SpriteBatch _spriteBatch;
        
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
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        { }

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw();
        }

        public void Draw()
        {
            if (_device == null || _presenter == null) return;

            _device.Presenter = _presenter;
            _device.SetViewports(new ViewportF(0, 0, Width, Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer, _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, 
                          _tool.Configuration.Rendering3D_BackgroundColor, 
                          1.0f, 
                          0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetBlendState(_device.BlendStates.Opaque);

            if (CurrentWad != null && CurrentObject != null)
            {
                Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
                if (CurrentObject.GetType() == typeof(StaticModel))
                {
                    var model = (StaticModel)CurrentObject;

                    Effect mioEffect = _tool.Effects["StaticModel"];
                    mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection);

                    mioEffect.Parameters["Color"].SetValue(Vector4.One);

                    mioEffect.Parameters["Texture"].SetResource(CurrentWad.DirectXTexture);
                    mioEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                    _device.SetVertexBuffer(0, model.VertexBuffer);
                    _device.SetIndexBuffer(model.IndexBuffer, true);

                    for (int i = 0; i < model.Meshes.Count; i++)
                    {
                        StaticMesh mesh = model.Meshes[i];
                        _layout = VertexInputLayout.FromBuffer<StaticVertex>(0, mesh.VertexBuffer);
                        _device.SetVertexInputLayout(_layout);
                        
                        mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection);
                        mioEffect.Techniques[0].Passes[0].Apply();

                        _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);
                    }
                }
                else if (CurrentObject.GetType() == typeof(SkinnedModel))
                {
                    var model = (SkinnedModel)CurrentObject;
                    var skin = model;

                    Effect mioEffect = _tool.Effects["Model"];

                    _device.SetVertexBuffer(0, model.VertexBuffer);
                    _device.SetIndexBuffer(model.IndexBuffer, true);

                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<SkinnedVertex>(0, model.VertexBuffer));

                    mioEffect.Parameters["Color"].SetValue(Vector4.One);

                    mioEffect.Parameters["Texture"].SetResource(CurrentWad.DirectXTexture);
                    mioEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                    for (int i = 0; i < model.Meshes.Count; i++)
                    {
                        SkinnedMesh mesh = skin.Meshes[i];
                        if (mesh.Vertices.Count == 0)
                            continue;

                        Matrix modelMatrix;
                        if (model.AnimationTransforms != null)
                            modelMatrix = model.AnimationTransforms[i];
                        else
                            modelMatrix = model.Bones[i].GlobalTransform;
                        mioEffect.Parameters["ModelViewProjection"].SetValue(modelMatrix * viewProjection);

                        mioEffect.Techniques[0].Passes[0].Apply();
                        _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);
                    }
                }
                else if (CurrentObject.GetType() == typeof(WadSprite))
                {
                    var sprite = (WadSprite)CurrentObject;

                    int x = (Width - sprite.Width) / 2;
                    int y = (Height - sprite.Height) / 2;

                    _spriteBatch.Begin(SpriteSortMode.Immediate, _device.BlendStates.AlphaBlend);
                    _spriteBatch.Draw(sprite.DirectXTexture, new DrawingRectangle(x, y, sprite.Width, sprite.Height), Color.White);
                    _spriteBatch.End();
                }
                else
                {

                }
            }

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

            Camera.Zoom(-e.Delta * _tool.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom);
            Invalidate();
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
                float deltaX = (e.X - _lastX) / (float)Height;
                float deltaY = (e.Y - _lastY) / (float)Height;

                _lastX = e.X;
                _lastY = e.Y;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Zoom(-deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.MoveCameraPlane(new Vector3(-deltaX, -deltaY, 0) * _tool.Configuration.RenderingItem_NavigationSpeedMouseTranslate);
                else
                    Camera.Rotate(deltaX * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate, 
                                  -deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                Invalidate();
            }
        }
    }
}
