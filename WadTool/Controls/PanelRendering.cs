using DarkUI.Controls;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRendering : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Wad2 CurrentWad { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IWadObjectId CurrentObjectId { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AnimationIndex { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int KeyFrameIndex { get; set; }

        private DarkScrollBar _animationScrollBar;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkScrollBar AnimationScrollBar
        {
            get { return _animationScrollBar; }
            set
            {
                if (_animationScrollBar != null)
                    _animationScrollBar.ValueChanged -= AnimationScrollBar_Value_ValueChanged;
                _animationScrollBar = value;
                value.ValueChanged += AnimationScrollBar_Value_ValueChanged;
            }
        }

        private GraphicsDevice _device;
        private DeviceManager _deviceManager;
        private SwapChainGraphicsPresenter _presenter;
        private RasterizerState _rasterizerWireframe;
        private WadToolClass _tool;
        private VertexInputLayout _layout;
        private float _lastX;
        private float _lastY;
        private SpriteBatch _spriteBatch;
        private Texture2D _spriteTexture;
        private WadTexture _spriteTextureData = null;

        public void InitializePanel(WadToolClass tool, DeviceManager deviceManager)
        {
            // Reset scrollbar
            _tool = tool;
            _device = deviceManager.Device;
            _deviceManager = deviceManager;

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
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer, _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target,
                         _tool.Configuration.Rendering3D_BackgroundColor.ToSharpDX(),
                         1.0f,
                         0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetBlendState(_device.BlendStates.Opaque);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);

            if (CurrentWad != null && CurrentObjectId != null)
            {
                Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
                if (CurrentObjectId is WadMoveableId)
                {
                    SkinnedModel model;
                    if (CurrentWad.DirectXMoveables.TryGetValue((WadMoveableId)CurrentObjectId, out model))
                    {
                        var skin = model;

                        Effect mioEffect = _deviceManager.Effects["Model"];

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
                            for (var b = 0; b < model.Meshes.Count; b++)
                                matrices.Add(model.AnimationTransforms[b]);
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

                            foreach (var submesh in mesh.Submeshes)
                                _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                        }
                    }
                }
                else if (CurrentObjectId is WadStaticId)
                {
                    StaticModel model;
                    if (CurrentWad.DirectXStatics.TryGetValue((WadStaticId)CurrentObjectId, out model))
                    {

                        Effect mioEffect = _deviceManager.Effects["StaticModel"];
                        mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());

                        mioEffect.Parameters["Color"].SetValue(Vector4.One);

                        mioEffect.Parameters["Texture"].SetResource(CurrentWad.DirectXTexture);
                        mioEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                        _device.SetVertexBuffer(0, model.VertexBuffer);
                        _device.SetIndexBuffer(model.IndexBuffer, true);

                        for (int i = 0; i < model.Meshes.Count; i++)
                        {
                            StaticMesh mesh = model.Meshes[i];
                            _layout = VertexInputLayout.FromBuffer<StaticVertex>(0, model.VertexBuffer);
                            _device.SetVertexInputLayout(_layout);

                            mioEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                            mioEffect.Techniques[0].Passes[0].Apply();

                            foreach (var submesh in mesh.Submeshes)
                                _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                        }
                    }
                }
                else if (CurrentObjectId is WadSpriteSequenceId)
                {
                    WadSpriteSequence spriteSequence;
                    if (CurrentWad.SpriteSequences.TryGetValue((WadSpriteSequenceId)CurrentObjectId, out spriteSequence))
                    {
                        int spriteIndex = Math.Min(spriteSequence.Sprites.Count - 1, KeyFrameIndex);
                        if (spriteIndex < spriteSequence.Sprites.Count)
                        {
                            WadSprite sprite = spriteSequence.Sprites[spriteIndex];

                            // Load texture
                            if (_spriteTextureData != sprite.Texture)
                            {
                                _spriteTexture?.Dispose();
                                _spriteTexture = TextureLoad.Load(_device, sprite.Texture.Image);
                                _spriteTextureData = sprite.Texture;
                            }

                            // Draw
                            int x = (ClientSize.Width - _spriteTextureData.Image.Width) / 2;
                            int y = (ClientSize.Height - _spriteTextureData.Image.Height) / 2;
                            _spriteBatch.Begin(SpriteSortMode.Immediate, _device.BlendStates.AlphaBlend);
                            _spriteBatch.Draw(_spriteTexture, new SharpDX.DrawingRectangle(x, y, _spriteTextureData.Image.Width, _spriteTextureData.Image.Height), SharpDX.Color.White);
                            _spriteBatch.End();
                        }
                    }
                }
                else if (CurrentObjectId is WadFixedSoundInfoId)
                {
                    int IMPLEMENT_SHOW_FIXED_SOUND_INFOl;
                }

                _device.Present();
            }
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

        private void AnimationScrollBar_Value_ValueChanged(object sender, ScrollValueEventArgs e)
        {
            KeyFrameIndex = AnimationScrollBar.Value;
            Invalidate();
        }

        public void UpdateAnimationScrollbar()
        {
            // Figure out scroll bar maximum
            int stateCount = -1;
            if (CurrentObjectId is WadMoveableId)
            {
                WadMoveable wadObject;
                if (CurrentWad.Moveables.TryGetValue((WadMoveableId)CurrentObjectId, out wadObject))
                    if (AnimationIndex < wadObject.Animations.Count)
                        stateCount = wadObject.Animations[AnimationIndex].KeyFrames.Count;
            }
            else if (CurrentObjectId is WadSpriteSequenceId)
            {
                WadSpriteSequence wadObject;
                if (CurrentWad.SpriteSequences.TryGetValue((WadSpriteSequenceId)CurrentObjectId, out wadObject))
                    stateCount = wadObject.Sprites.Count;
            }

            // Setup scroll bar
            KeyFrameIndex = Math.Max(KeyFrameIndex, stateCount - 1);
            AnimationScrollBar.ViewSize = 1;
            AnimationScrollBar.Enabled = stateCount > 1;
            AnimationScrollBar.Minimum = 0;
            AnimationScrollBar.Maximum = Math.Max(1, stateCount);
            AnimationScrollBar.Value = KeyFrameIndex;
            AnimationScrollBar.Invalidate();
        }
    }
}
