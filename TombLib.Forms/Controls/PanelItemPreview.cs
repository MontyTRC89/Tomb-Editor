using DarkUI.Controls;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Rendering;
using TombLib.Wad;

namespace TombLib.Controls
{
    public abstract class PanelItemPreview : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IWadObject CurrentObject
        {
            get { return _currentObject; }
            set
            {
                if (_currentObject == value)
                    return;
                _currentObject = value;
                if (value is WadFixedSoundInfo)
                {
                    _soundInfoEditor.SoundInfo = ((WadFixedSoundInfo)value).SoundInfo;
                    _soundInfoEditor.Visible = true;
                }
                else if (value is WadAdditionalSoundInfo)
                {
                    _soundInfoEditor.SoundInfo = ((WadAdditionalSoundInfo)value).SoundInfo;
                    _soundInfoEditor.Visible = true;
                }
                else
                    _soundInfoEditor.Visible = false;
                Invalidate();
            }
        }
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

        private GraphicsDevice _legacyDevice;
        private RasterizerState _rasterizerWireframe;
        private float _lastX;
        private float _lastY;
        private SpriteBatch _spriteBatch;
        private Texture2D _spriteTexture;
        private WadTexture _spriteTextureData;
        private WadRenderer _wadRenderer;
        private SoundInfoEditor _soundInfoEditor;
        private IWadObject _currentObject = null;

        public PanelItemPreview()
        {
            // Init fixed sound info editor
            _soundInfoEditor = new SoundInfoEditor();
            _soundInfoEditor.Name = "_soundInfoEditor";
            _soundInfoEditor.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            Rectangle clientRectangle = ClientRectangle;
            clientRectangle.Inflate(new Size(-5, -5));
            _soundInfoEditor.Bounds = clientRectangle;
            _soundInfoEditor.Visible = false;
            _soundInfoEditor.ReadOnly = ReadOnly;
            _soundInfoEditor.SoundInfoChanged += delegate
            {
                if (ReadOnly)
                    return;
                if (CurrentObject is WadFixedSoundInfo)
                {
                    WadFixedSoundInfo fixedSoundInfo = (WadFixedSoundInfo)CurrentObject;
                    fixedSoundInfo.SoundInfo = _soundInfoEditor.SoundInfo;
                }
                if (CurrentObject is WadAdditionalSoundInfo)
                {
                    WadAdditionalSoundInfo additionalSoundInfo = (WadAdditionalSoundInfo)CurrentObject;
                    additionalSoundInfo.SoundInfo = _soundInfoEditor.SoundInfo;
                }
                ObjectWasModified(this, EventArgs.Empty);
            };
            Controls.Add(_soundInfoEditor);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Rectangle clientRectangle = ClientRectangle;
            clientRectangle.Inflate(new Size(-5, -5));
            _soundInfoEditor.Bounds = clientRectangle;
        }

        public override void InitializeRendering(RenderingDevice device)
        {
            base.InitializeRendering(device);

            // Reset scrollbar
            _legacyDevice = DeviceManager.DefaultDeviceManager.___LegacyDevice;
            _wadRenderer = new WadRenderer(DeviceManager.DefaultDeviceManager.___LegacyDevice, true);

            ResetCamera();

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

            _rasterizerWireframe = RasterizerState.New(_legacyDevice, renderStateDesc);
            _spriteBatch = new SpriteBatch(_legacyDevice);
        }

        public void ResetCamera()
        {
            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 0, 1000000, FieldOfView * (float)(Math.PI / 180));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wadRenderer?.Dispose();
                _spriteBatch?.Dispose();
                _spriteTexture?.Dispose();
                _rasterizerWireframe?.Dispose();
                _soundInfoEditor?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_soundInfoEditor.Visible)
                using (var brush = new SolidBrush(_soundInfoEditor.BackColor))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
            else
                base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_soundInfoEditor.Visible || _legacyDevice == null)
                return;
            base.OnPaint(e);
        }

        protected override void OnDraw()
        {
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
            if (CurrentObject is WadMoveable)
            {
                AnimatedModel model = _wadRenderer.GetMoveable((WadMoveable)CurrentObject);
                // We don't need to rebuilt it everytime necessarily, but it's cheap to so and
                // simpler than trying to figure out when it may be necessary.
                model.UpdateAnimation(AnimationIndex, KeyFrameIndex);

                var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_legacyDevice.SamplerStates.Default);

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
                    var mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                    _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                    effect.Parameters["ModelViewProjection"].SetValue((matrices[i] * viewProjection).ToSharpDX());

                    effect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
                }
            }
            else if (CurrentObject is WadStatic)
            {
                StaticModel model = _wadRenderer.GetStatic((WadStatic)CurrentObject);

                var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["StaticModel"];

                effect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_legacyDevice.SamplerStates.Default);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var mesh = model.Meshes[i];

                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                    _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                    effect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                    effect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
                }
            }
            else if (CurrentObject is WadSpriteSequence)
            {
                WadSpriteSequence spriteSequence = (WadSpriteSequence)CurrentObject;
                int spriteIndex = Math.Min(spriteSequence.Sprites.Count - 1, KeyFrameIndex);
                if (spriteIndex < spriteSequence.Sprites.Count)
                {
                    WadSprite sprite = spriteSequence.Sprites[spriteIndex];

                    // Load texture
                    if (_spriteTextureData != sprite.Texture)
                    {
                        _spriteTexture?.Dispose();
                        _spriteTexture = TextureLoad.Load(_legacyDevice, sprite.Texture.Image);
                        _spriteTextureData = sprite.Texture;
                    }

                    // Draw
                    int x = (ClientSize.Width - _spriteTextureData.Image.Width) / 2;
                    int y = (ClientSize.Height - _spriteTextureData.Image.Height) / 2;
                    _spriteBatch.Begin(SpriteSortMode.Immediate, _legacyDevice.BlendStates.AlphaBlend);
                    _spriteBatch.Draw(_spriteTexture, new SharpDX.DrawingRectangle(x, y, _spriteTextureData.Image.Width, _spriteTextureData.Image.Height), SharpDX.Color.White);
                    _spriteBatch.End();
                }
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

            Camera.Zoom(-e.Delta * NavigationSpeedMouseWheelZoom);
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
                float deltaX = (e.X - _lastX) / Height;
                float deltaY = (e.Y - _lastY) / Height;

                _lastX = e.X;
                _lastY = e.Y;

                if ((ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Zoom(-deltaY * NavigationSpeedMouseZoom);
                else if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.MoveCameraPlane(new Vector3(-deltaX, -deltaY, 0) * NavigationSpeedMouseTranslate);
                else
                    Camera.Rotate(deltaX * NavigationSpeedMouseRotate,
                                  -deltaY * NavigationSpeedMouseRotate);
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
            if (CurrentObject is WadMoveable)
            {
                if (AnimationIndex < ((WadMoveable)CurrentObject).Animations.Count)
                    stateCount = ((WadMoveable)CurrentObject).Animations[AnimationIndex].KeyFrames.Count;
            }
            else if (CurrentObject is WadSpriteSequence)
            {
                stateCount = ((WadSpriteSequence)CurrentObject).Sprites.Count;
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

        public abstract float FieldOfView { get; }
        public abstract float NavigationSpeedMouseWheelZoom { get; }
        public abstract float NavigationSpeedMouseZoom { get; }
        public abstract float NavigationSpeedMouseTranslate { get; }
        public abstract float NavigationSpeedMouseRotate { get; }
        public abstract bool ReadOnly { get; }
        public event EventHandler ObjectWasModified;
    }
}