using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombLib.Graphics;

namespace TombEditor.Controls
{
    class PanelRenderingItem : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SwapChainGraphicsPresenter Presenter { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }

        private Editor _editor;
        private VertexInputLayout _layout;
        private DeviceManager _deviceManager;
        private GraphicsDevice _device;
        private int _lastX;
        private int _lastY;

        public PanelRenderingItem()
        {
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ChosenItemChangedEvent)
            {
                Editor.ChosenItemChangedEvent e = (Editor.ChosenItemChangedEvent)obj;
                if ((e.Current != null) && (_editor?.Level?.Wad != null))
                {
                    Camera = new ArcBallCamera(Vector3.Zero, 0, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 768.0f, 0, 1000000);
                }
                Invalidate();
            }
        }

        public void InitializePanel(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
            _device = deviceManager.Device;

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
                
                Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 1024.0f, 0, 1000000);
            }
        }

        // Do NOT call this method to redraw the scene!
        // Call Invalidate() instead to schedule a redraw in the message loop.
        private void Draw()
        {
            if (DesignMode)
                return;

            _device.Presenter = Presenter;
            _device.SetViewports(new ViewportF(0, 0, Width, Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer, _device.Presenter.BackBuffer);

            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color4.White, 1.0f, 0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            
            if ((_editor.ChosenItem == null) || (_editor?.Level?.Wad == null))
            {
                _device.Present();
                return;
            }
            ItemType chosenItem = _editor.ChosenItem.Value;

            Matrix viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
            if (chosenItem.IsStatic)
            {
                StaticModel model = _editor.Level.Wad.DirectXStatics[chosenItem.Id];

                Effect mioEffect = _deviceManager.Effects["StaticModel"];
                mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection);
                mioEffect.Parameters["EditorTextureEnabled"].SetValue(false);
                mioEffect.Parameters["TextureEnabled"].SetValue(true);
                mioEffect.Parameters["SelectionEnabled"].SetValue(false);
                mioEffect.Parameters["LightEnabled"].SetValue(false);

                mioEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
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
            else
            {
                SkinnedModel model = _editor.Level.Wad.DirectXMoveables[chosenItem.Id];
                SkinnedModel skin = model;
                
                Effect mioEffect = _deviceManager.Effects["Model"];
                mioEffect.Parameters["EditorTextureEnabled"].SetValue(false);
                mioEffect.Parameters["TextureEnabled"].SetValue(true);
                mioEffect.Parameters["SelectionEnabled"].SetValue(false);

                _device.SetVertexBuffer(0, model.VertexBuffer);
                _device.SetIndexBuffer(model.IndexBuffer, true);

                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<SkinnedVertex>(0, model.VertexBuffer));

                mioEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
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

            _device.Present();
        }
        
        protected override void OnPaintBackground(PaintEventArgs e)
        { }

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
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / (float)Height;
                float deltaY = (e.Y - _lastY) / (float)Height;

                _lastX = e.X;
                _lastY = e.Y;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Zoom(-deltaY * _editor.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.MoveCameraPlane(new Vector3(-deltaX, -deltaY, 0) * _editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate);
                else
                    Camera.Rotate(deltaX * _editor.Configuration.RenderingItem_NavigationSpeedMouseRotate, -deltaY * _editor.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                Invalidate();
            }
        }
    }
}
