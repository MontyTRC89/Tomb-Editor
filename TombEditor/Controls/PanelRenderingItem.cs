using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombLib.Graphics;

namespace TombEditor.Controls
{
    class PanelRenderingItem : Panel
    {
        public SwapChainGraphicsPresenter Presenter { get; set; }
        public ArcBallCamera Camera { get; set; }
        public bool SelectedItemIsStatic { get; set; }

        private Editor _editor;
        private VertexInputLayout _layout;
        private DeviceManager _deviceManager;
        private GraphicsDevice _device;
        private int _lastX;
        private int _lastY;
        private int _selectedItem;
        
        public void InitializePanel(DeviceManager deviceManager)
        {
            _editor = Editor.Instance;
            _deviceManager = deviceManager;
            _device = deviceManager.Device;
            SelectedItem = -1;

            // inizializzo il Presenter se necessario
            if (Presenter == null)
            {
                PresentationParameters pp = new PresentationParameters();
                pp.BackBufferFormat = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                pp.BackBufferWidth = Width;
                pp.BackBufferHeight = Height;
                pp.DepthStencilFormat = DepthFormat.Depth24Stencil8;
                pp.DeviceWindowHandle = this;
                pp.IsFullScreen = false;
                pp.MultiSampleCount = MSAALevel.None;
                pp.PresentationInterval = PresentInterval.Immediate;
                pp.RenderTargetUsage = SharpDX.DXGI.Usage.RenderTargetOutput | SharpDX.DXGI.Usage.BackBuffer;
                pp.Flags = SharpDX.DXGI.SwapChainFlags.None;

                Presenter = new SwapChainGraphicsPresenter(_deviceManager.Device, pp);
                
                // inizializzo la telecamera
                Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 1024.0f, 0, 1000000);
            }
        }

        public void Draw()
        {
            if (DesignMode)
                return;

            _device.Presenter = Presenter;
            _device.SetViewports(new ViewportF(0, 0, Width, Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer, _device.Presenter.BackBuffer);

            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color4.White, 1.0f, 0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);

            if (SelectedItem == -1)
            {
                _device.Present();
                return;
            }

            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
            if (SelectedItemIsStatic)
            {
                SkinnedModel model;
                SkinnedModel skin;

                model = _editor.Level.Wad.Moveables[(uint)SelectedItem];
                skin = _editor.Level.Wad.Moveables[(uint)SelectedItem];

                model.BuildHierarchy();

                Effect mioEffect = _deviceManager.Effects["Model"];
                mioEffect.Parameters["EditorTextureEnabled"].SetValue(false);
                mioEffect.Parameters["TextureEnabled"].SetValue(true);
                mioEffect.Parameters["SelectionEnabled"].SetValue(false);

                _device.SetVertexBuffer(0, model.VertexBuffer);
                _device.SetIndexBuffer(model.IndexBuffer, true);

                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<SkinnedVertex>(0, model.VertexBuffer));

                mioEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
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
            else
            {
                StaticModel model;

                model = _editor.Level.Wad.StaticMeshes[(uint)SelectedItem];

                Effect mioEffect = _deviceManager.Effects["StaticModel"];
                mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection);
                mioEffect.Parameters["EditorTextureEnabled"].SetValue(false);
                mioEffect.Parameters["TextureEnabled"].SetValue(true);
                mioEffect.Parameters["SelectionEnabled"].SetValue(false);
                mioEffect.Parameters["LightEnabled"].SetValue(false);

                mioEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
                mioEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                _device.SetVertexInputLayout(_layout);

                _device.SetVertexBuffer(0, model.VertexBuffer);
                _device.SetIndexBuffer(model.IndexBuffer, true);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    StaticMesh mesh = model.Meshes[i];

                    if (_layout == null)
                    {
                        _layout = VertexInputLayout.FromBuffer<StaticVertex>(0, mesh.VertexBuffer);
                    }

                    mioEffect.Parameters["ModelViewProjection"].SetValue(Matrix.Identity);
                    mioEffect.Techniques[0].Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);
                }
            }

            _device.Present();
        }

        public int SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                if (_selectedItem == -1 || _editor == null || _editor.Level == null || _editor.Level.Wad == null)
                    return;

                if (_editor.Level.Wad.Moveables.Count > _selectedItem)
                {
                    SkinnedModel model = _editor.Level.Wad.Moveables.ElementAt(_selectedItem).Value;
                    if (model.Animations.Count != 0)
                        model.BuildAnimationPose(model.Animations[0].KeyFrames[0]);
                }
                else
                    Camera = new ArcBallCamera(Vector3.Zero, 0, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 768.0f, 0, 1000000);

                Draw();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Presenter != null)
                Presenter.Resize(Width, Height, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
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
                float deltaX = (e.X - _lastX) / Width;
                float deltaY = (e.Y - _lastY) / Height;

                _lastX = e.X;
                _lastY = e.Y;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Move(deltaY * -200);
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.Translate(new Vector3(deltaX * 200, deltaY * -200, 0));
                else
                    Camera.Rotate(deltaX *4, -deltaY * 4);
                Draw();
            }
        }
    }
}
