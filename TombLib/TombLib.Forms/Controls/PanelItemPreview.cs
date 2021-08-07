using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
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
                if (value is WadSpriteSequence &&
                   ((WadSpriteSequence)value).Sprites.Count > 0)
                {
                    if (!_animTimer.Enabled) _animTimer.Enabled = true;
                    if (_currentObject != value) _currentFrame = 0;
                }
                else if(_animTimer.Enabled) 
                    _animTimer.Enabled = false;

                _currentObject = value;
                Invalidate();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AnimationIndex { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int KeyFrameIndex { get; set; }

        public bool DrawTransparency { get; set; } = false;

        // Preview animation state
        private Timer _animTimer = new Timer() { Interval = 333 };
        private int _currentFrame;
      
        // Interaction state
        private float _lastX;
        private float _lastY;
        private IWadObject _currentObject = null;

        // Rendering state
        private RenderingTextureAllocator _textureAllocator;

        // Legacy rendering state
        private GraphicsDevice _legacyDevice;
        private WadRenderer _wadRenderer;

        public PanelItemPreview()
        {
            _animTimer.Tick += _animTimer_Tick;
        }

        private void _animTimer_Tick(object sender, EventArgs e)
        {
            if (!(CurrentObject is WadSpriteSequence))
                return;

            if (_currentFrame < (CurrentObject as WadSpriteSequence).Sprites.Count - 1)
                _currentFrame++;
            else
                _currentFrame = 0;
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Rectangle clientRectangle = ClientRectangle;
            clientRectangle.Inflate(new Size(-5, -5));
        }

        public override void InitializeRendering(RenderingDevice device, bool antialias = false)
        {
            base.InitializeRendering(device, antialias);

            _textureAllocator = device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(1024, 1024, 1) });

            // Legacy rendering state
            {
                // Reset scrollbar
                _legacyDevice = DeviceManager.DefaultDeviceManager.___LegacyDevice;
                _wadRenderer = new WadRenderer(DeviceManager.DefaultDeviceManager.___LegacyDevice, true, true);

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
            }
        }

        public void ResetCamera()
        {
            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000, FieldOfView * (float)(Math.PI / 180));
        }

        public void GarbageCollect()
        {
            _wadRenderer?.GarbageCollect();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wadRenderer?.Dispose();
                _textureAllocator?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_legacyDevice == null)
                return;
            base.OnPaint(e);
        }

        protected override void OnDraw()
        {
            // To make sure things are in a defined state for legacy rendering...
            ((Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            if (DrawTransparency)
                _legacyDevice.SetBlendState(_legacyDevice.BlendStates.AlphaBlend);
            else
                _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
            if (CurrentObject is WadMoveable)
            {
                // HACK: new moveables have one bone with null mesh
                var moveable = (WadMoveable)CurrentObject;
                if (moveable.Meshes.Count == 0 || (moveable.Meshes.Count == 1 && moveable.Meshes[0] == null))
                    return;

                AnimatedModel model = _wadRenderer.GetMoveable((WadMoveable)CurrentObject);
                // We don't need to rebuilt it everytime necessarily, but it's cheap to so and
                // simpler than trying to figure out when it may be necessary.
                model.UpdateAnimation(AnimationIndex, KeyFrameIndex);

                var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

                effect.Parameters["AlphaTest"].SetValue(DrawTransparency);
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["StaticLighting"].SetValue(false);
                effect.Parameters["ColoredVertices"].SetValue(false);
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
                    _legacyDevice.SetVertexInputLayout(mesh.InputLayout);

                    effect.Parameters["ModelViewProjection"].SetValue((matrices[i] * viewProjection).ToSharpDX());

                    effect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                        _legacyDevice.Draw(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                }
            }
            else if (CurrentObject is WadStatic)
            {
                StaticModel model = _wadRenderer.GetStatic((WadStatic)CurrentObject);

                var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

                effect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                effect.Parameters["AlphaTest"].SetValue(DrawTransparency);
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["StaticLighting"].SetValue(false); 
                effect.Parameters["ColoredVertices"].SetValue(false);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_legacyDevice.SamplerStates.Default);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var mesh = model.Meshes[i];

                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                    _legacyDevice.SetVertexInputLayout(mesh.InputLayout);

                    effect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                    effect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                }
            }
            else if (CurrentObject is WadSpriteSequence)
            {
                var seq = (WadSpriteSequence)CurrentObject;
                if (seq.Sprites.Count <= _currentFrame)
                    return;

                WadSprite sprite = seq.Sprites[_currentFrame];
                float aspectRatioViewport = (float)ClientSize.Width / ClientSize.Height;
                float aspectRatioImage = (float)sprite.Texture.Image.Width / sprite.Texture.Image.Height;
                float aspectRatioAdjust = aspectRatioViewport / aspectRatioImage;
                Vector2 factor = Vector2.Min(new Vector2(1.0f / aspectRatioAdjust, aspectRatioAdjust), new Vector2(1.0f));

                SwapChain.RenderSprites(_textureAllocator, false, true, new List<Sprite>() { new Sprite
                {
                    Texture = sprite.Texture.Image,
                    PosStart = -0.9f * factor,
                    PosEnd = 0.9f * factor
                } });
            }
            else if (CurrentObject is ImportedGeometry)
            {
                var geo = (ImportedGeometry)CurrentObject;
                var model = geo.DirectXModel;

                var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["RoomGeometry"];

                effect.Parameters["UseVertexColors"].SetValue(true);
                effect.Parameters["AlphaTest"].SetValue(DrawTransparency);
                effect.Parameters["Color"].SetValue(Vector4.One);

                if (model != null && model.Meshes.Count > 0)
                    for (int i = 0; i < model.Meshes.Count; i++)
                    {
                        var mesh = model.Meshes[i];

                        _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                        _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                        _legacyDevice.SetVertexInputLayout(mesh.InputLayout);

                        effect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                        effect.Techniques[0].Passes[0].Apply();


                        foreach (var submesh in mesh.Submeshes)
                        {
                            var texture = submesh.Value.Material.Texture;
                            if (texture != null && texture is ImportedGeometryTexture)
                            {
                                effect.Parameters["TextureEnabled"].SetValue(true);
                                effect.Parameters["Texture"].SetResource(((ImportedGeometryTexture)texture).DirectXTexture);
                                effect.Parameters["ReciprocalTextureSize"].SetValue(new Vector2(1.0f / texture.Image.Width, 1.0f / texture.Image.Height));
                                effect.Parameters["TextureSampler"].SetResource(_legacyDevice.SamplerStates.AnisotropicWrap);
                            }
                            else
                                effect.Parameters["TextureEnabled"].SetValue(false);

                            effect.Techniques[0].Passes[0].Apply();
                            _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                        }
                    }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            // Make this control able to receive scroll and key board events...
            if (!Focused && Form.ActiveForm == FindForm())
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

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / Height;
                float deltaY = (e.Y - _lastY) / Height;

                _lastX = e.X;
                _lastY = e.Y;

                if (e.Button == MouseButtons.Right)
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Camera.Zoom(-deltaY * NavigationSpeedMouseZoom);
                    else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                        Camera.Rotate(deltaX * NavigationSpeedMouseRotate,
                                     -deltaY * NavigationSpeedMouseRotate);
                }
                if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
                     e.Button == MouseButtons.Middle)
                    Camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0) * NavigationSpeedMouseTranslate);

                Invalidate();
            }
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
        }

        public abstract float FieldOfView { get; }
        public abstract float NavigationSpeedMouseWheelZoom { get; }
        public abstract float NavigationSpeedMouseZoom { get; }
        public abstract float NavigationSpeedMouseTranslate { get; }
        public abstract float NavigationSpeedMouseRotate { get; }
        public abstract bool ReadOnly { get; }
    }
}