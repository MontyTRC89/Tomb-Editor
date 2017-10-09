﻿using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
            // Update field of view
            if (obj is Editor.ConfigurationChangedEvent)
            {
                Camera.FieldOfView = ((Editor.ConfigurationChangedEvent)obj).Current.RenderingItem_FieldOfView * (float)(Math.PI / 180);
                Invalidate();
            }

            // Update currently viewed item
            if (obj is Editor.ChosenItemChangedEvent)
            {
                Editor.ChosenItemChangedEvent e = (Editor.ChosenItemChangedEvent)obj;
                if ((e.Current != null) && (_editor?.Level?.Wad != null))
                    ResetCamera();
                Invalidate();
            }

            if (obj is Editor.LoadedWadsChangedEvent)
                Invalidate();
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
                pp.BackBufferWidth = ClientSize.Width;
                pp.BackBufferHeight = ClientSize.Height;
                pp.DepthStencilFormat = DepthFormat.Depth24Stencil8;
                pp.DeviceWindowHandle = this;
                pp.IsFullScreen = false;
                pp.MultiSampleCount = MSAALevel.None;
                pp.PresentationInterval = PresentInterval.Immediate;
                pp.RenderTargetUsage = SharpDX.DXGI.Usage.RenderTargetOutput | SharpDX.DXGI.Usage.BackBuffer;
                pp.Flags = SharpDX.DXGI.SwapChainFlags.None;
                Presenter = new SwapChainGraphicsPresenter(_deviceManager.Device, pp);

                ResetCamera();
            }
        }

        public void ResetCamera()
        {
            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -MathUtil.PiOverTwo, MathUtil.PiOverTwo, 2048.0f, 0, 1000000, _editor.Configuration.RenderingItem_FieldOfView * (float)(Math.PI / 180));
        }
        
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * _editor.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom);
            Invalidate();
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

                mioEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
                mioEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                mioEffect.Parameters["Color"].SetValue(Vector4.One);

                _device.SetVertexBuffer(0, model.VertexBuffer);
                _device.SetIndexBuffer(model.IndexBuffer, true);

                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<StaticVertex>(0, model.VertexBuffer));

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    StaticMesh mesh = model.Meshes[i];

                    if (_layout == null)
                    {
                        _layout = VertexInputLayout.FromBuffer<StaticVertex>(0, mesh.VertexBuffer);
                        _device.SetVertexInputLayout(_layout);
                    }

                    mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection);
                    mioEffect.Techniques[0].Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);
                }
            }
            else
            {
                SkinnedModel model = _editor.Level.Wad.DirectXMoveables[chosenItem.Id];
                SkinnedModel skin = model;

                Effect mioEffect = _deviceManager.Effects["Model"];

                _device.SetVertexBuffer(0, model.VertexBuffer);
                _device.SetIndexBuffer(model.IndexBuffer, true);

                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<SkinnedVertex>(0, model.VertexBuffer));

                mioEffect.Parameters["Texture"].SetResource(_editor.Level.Wad.DirectXTexture);
                mioEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                mioEffect.Parameters["Color"].SetValue(Vector4.One);

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
        {
            // Don't paint the background
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_editor?.Level?.Settings?.WadFilePath == null)
            {
                e.Graphics.Clear(Parent.BackColor);
                e.Graphics.DrawString("Click here to load WAD.",
                    Font, System.Drawing.Brushes.DarkGray,
                    ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            else
                Draw();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Presenter != null)
            {
                Presenter.Resize(ClientSize.Width, ClientSize.Height, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_editor?.Level?.Settings?.WadFilePath == null)
            {
                EditorActions.LoadWad(Parent);
                return;
            }

            _lastX = e.X;
            _lastY = e.Y;

            //https://stackoverflow.com/questions/14191219/receive-mouse-move-even-cursor-is-outside-control
            Capture = true; // Capture mouse for zoom and panning
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            switch (e.Button)
            {
                case MouseButtons.Right:
                case MouseButtons.Middle:
                    // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                    float deltaX = (e.X - _lastX) / (float)Height;
                    float deltaY = (e.Y - _lastY) / (float)Height;

                    _lastX = e.X;
                    _lastY = e.Y;

                    if (((ModifierKeys & Keys.Shift) == Keys.Shift) || (e.Button == MouseButtons.Middle))
                        Camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0) * _editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate);
                    else if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Camera.Zoom(-deltaY * _editor.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                    else
                        Camera.Rotate(deltaX * _editor.Configuration.RenderingItem_NavigationSpeedMouseRotate, -deltaY * _editor.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                    Invalidate();
                    break;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Capture = false;
        }
    }
}
