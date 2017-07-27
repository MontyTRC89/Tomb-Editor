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
        public Viewport Viewport { get; set; }
        public EditorItemType ItemType { get; set; }

        private Editor _editor;
        private bool _drag;
        private float _lastX;
        private float LastY;
        private float _deltaX;
        private float _deltaY;
        private int _animation;
        private int _frame;
        private VertexInputLayout _layout;
        private int _selectedItem;
        
        public void InitializePanel()
        {
            _editor = Editor.Instance;
            SelectedItem = -1;
            _animation = -1;
            _frame = -1;

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

                Presenter = new SwapChainGraphicsPresenter(_editor.GraphicsDevice, pp);

                Viewport = new Viewport(0, 0, Width, Height, 10.0f, 100000.0f);

                // inizializzo la telecamera
                Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 1024.0f, 0, 1000000);
            }
        }

        public void Draw()
        {
            if (DesignMode)
                return;

            _editor.GraphicsDevice.Presenter = Presenter;
            _editor.GraphicsDevice.SetViewports(new ViewportF(0, 0, Width, Height));
            _editor.GraphicsDevice.SetRenderTargets(_editor.GraphicsDevice.Presenter.DepthStencilBuffer, _editor.GraphicsDevice.Presenter.BackBuffer);

            _editor.GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color4.White, 1.0f, 0);
            _editor.GraphicsDevice.SetDepthStencilState(_editor.GraphicsDevice.DepthStencilStates.Default);

            if (SelectedItem == -1)
            {
                _editor.GraphicsDevice.Present();
                return;
            }

            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
            if (ItemType == EditorItemType.Moveable)
            {
                SkinnedModel model;
                SkinnedModel skin;

                model = _editor.Level.Wad.Moveables[(uint)SelectedItem];
                skin = _editor.Level.Wad.Moveables[(uint)SelectedItem];

                model.BuildHierarchy();

                Effect mioEffect = _editor.Effects["Model"];
                mioEffect.Parameters["EditorTextureEnabled"].SetValue(false);
                mioEffect.Parameters["TextureEnabled"].SetValue(true);
                mioEffect.Parameters["SelectionEnabled"].SetValue(false);

                _editor.GraphicsDevice.SetVertexBuffer(0, model.VertexBuffer);
                _editor.GraphicsDevice.SetIndexBuffer(model.IndexBuffer, true);

                _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer<SkinnedVertex>(0, model.VertexBuffer));

                mioEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
                mioEffect.Parameters["TextureSampler"].SetResource(_editor.GraphicsDevice.SamplerStates.Default);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    SkinnedMesh mesh = skin.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    Matrix modelMatrix;
                    if (_animation > -1)
                        modelMatrix = model.AnimationTransforms[i];
                    else
                        modelMatrix = model.Bones[i].GlobalTransform;
                    mioEffect.Parameters["ModelViewProjection"].SetValue(modelMatrix * viewProjection);

                    mioEffect.Techniques[0].Passes[0].Apply();
                    _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);
                }
            }
            else
            {
                StaticModel model;

                model = _editor.Level.Wad.StaticMeshes[(uint)SelectedItem];

                Effect mioEffect = _editor.Effects["StaticModel"];
                mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection);
                mioEffect.Parameters["EditorTextureEnabled"].SetValue(false);
                mioEffect.Parameters["TextureEnabled"].SetValue(true);
                mioEffect.Parameters["SelectionEnabled"].SetValue(false);
                mioEffect.Parameters["LightEnabled"].SetValue(false);

                mioEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.Textures[0]);
                mioEffect.Parameters["TextureSampler"].SetResource(_editor.GraphicsDevice.SamplerStates.Default);

                _editor.GraphicsDevice.SetVertexInputLayout(_layout);

                _editor.GraphicsDevice.SetVertexBuffer(0, model.VertexBuffer);
                _editor.GraphicsDevice.SetIndexBuffer(model.IndexBuffer, true);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    StaticMesh mesh = model.Meshes[i];

                    if (_layout == null)
                    {
                        _layout = VertexInputLayout.FromBuffer<StaticVertex>(0, mesh.VertexBuffer);
                    }

                    mioEffect.Parameters["ModelViewProjection"].SetValue(Matrix.Identity);
                    mioEffect.Techniques[0].Passes[0].Apply();

                    _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);
                }
            }

            _editor.GraphicsDevice.Present();
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
                    if (model.Animations.Count == 0)
                    {
                        _animation = -1;
                        _frame = -1;
                    }
                    else
                    {
                        _animation = 0;
                        _frame = 0;
                        model.BuildAnimationPose(model.Animations[_animation].KeyFrames[_frame]);
                    }
                }
                else
                {
                    _animation = -1;
                    _frame = -1;

                    Camera = new ArcBallCamera(Vector3.Zero, 0, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 768.0f, 0, 1000000);
                }

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

            _drag = true;
            if (e.Button == MouseButtons.Right)
            {
                _lastX = e.X;
                LastY = e.Y;
            }

            Draw();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_drag && e.Button == MouseButtons.Right)
            {
                _deltaX = e.X - _lastX;
                _deltaY = e.Y - LastY;

                _lastX = e.X;
                LastY = e.Y;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    Camera.Move(-_deltaY / 1.0f);
                }
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    Camera.Translate(new Vector3(_deltaX, -_deltaY, 0));
                }
                else
                {
                    Camera.Rotate(_deltaX / 50, -_deltaY / 50);
                }
            }

            Draw();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_drag && e.Button == MouseButtons.Right)
            {
                _deltaX = e.X - _lastX;
                _deltaY = e.Y - LastY;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    Camera.Move(-_deltaY / 10.0f);
                }
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    Camera.Translate(new Vector3(_deltaX, -_deltaY, 0));
                }
                else
                {
                    Camera.Rotate(_deltaX / 500, -_deltaY / 500);
                }

                _drag = false;
            }

            Draw();
        }
    }
}
