using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public int Animation { get; set; }
        public int KeyFrame { get; set; }

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

            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 0, 1000000, (float)Math.PI / 4.0f);

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
            if (_device == null || _presenter == null)
                return;

            _device.Presenter = _presenter;
            _device.SetViewports(new SharpDX.ViewportF(0, 0, Width, Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer, _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target,
                         _tool.Configuration.Rendering3D_BackgroundColor.ToSharpDX(),
                         1.0f,
                         0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetBlendState(_device.BlendStates.Opaque);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);

            if (CurrentWad != null && CurrentObject != null)
            {
                Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
                if (CurrentObject.GetType() == typeof(StaticModel))
                {
                    var model = (StaticModel)CurrentObject;

                    Effect mioEffect = _tool.Effects["StaticModel"];
                    mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());

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

                        mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
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

                    // Build animation transforms
                    var matrices = new List<Matrix4x4>();
                    if (model.Animations.Count != 0)
                    {
                        var animation = model.Animations[Animation];
                        /*if (KeyFrame % animation.Framerate == 0)
                        {*/
                        model.BuildAnimationPose(model.Animations[Animation].KeyFrames[KeyFrame/* / animation.Framerate*/]);
                        for (var b = 0; b < model.Meshes.Count; b++)
                            matrices.Add(model.AnimationTransforms[b]);
                        /*}
                        else
                        {

                            var transforms1 = new List<Matrix4x4>();
                            var transforms2 = new List<Matrix4x4>();
                            var frame1 = (int)Math.Floor((double)KeyFrame / animation.Framerate);
                            var frame2 = frame1 + 1;

                            // Build transforms for current keyframe
                            model.BuildAnimationPose(model.Animations[Animation].KeyFrames[frame1]);
                            for (var b = 0; b < model.Meshes.Count; b++)
                                transforms1.Add(model.AnimationTransforms[b]);

                            var amount = (float)(KeyFrame - frame1) / animation.Framerate;

                            if (frame2 >= animation.KeyFrames.Count)
                            {
                                // Impossible case (in theory...)
                                for (var b = 0; b < model.Meshes.Count; b++)
                                    matrices.Add(transforms1[b]);
                            }
                            else
                            {
                                // Build transforms for current keyframe + 1
                                model.BuildAnimationPose(model.Animations[Animation].KeyFrames[frame2]);
                                for (var b = 0; b < model.Meshes.Count; b++)
                                    transforms2.Add(model.AnimationTransforms[b]);

                                // Interpolate
                                for (var b = 0; b < model.Meshes.Count; b++)
                                    matrices.Add(Matrix4x4.Lerp(transforms1[b], transforms2[2], amount));
                            }
                        }    */
                    }
                    else
                    {
                        foreach (var bone in model.Bones)
                            matrices.Add(bone.GlobalTransform);
                    }

                    for (int i = 0; i < model.Meshes.Count; i++)
                    {
                        SkinnedMesh mesh = skin.Meshes[i];
                        if (mesh.Vertices.Count == 0)
                            continue;

                        mioEffect.Parameters["ModelViewProjection"].SetValue((matrices[i] * viewProjection).ToSharpDX());

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
                    _spriteBatch.Draw(sprite.DirectXTexture, new SharpDX.DrawingRectangle(x, y, sprite.Width, sprite.Height), SharpDX.Color.White);
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
