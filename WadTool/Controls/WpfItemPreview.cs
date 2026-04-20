using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Rendering.DirectX11;
using TombLib.Wad;

namespace WadTool.Controls
{
    /// <summary>
    /// WPF equivalent of PanelItemPreview. Renders a wad object with arc-ball camera controls.
    /// </summary>
    public class WpfItemPreview : WpfRenderingPanel
    {
        private IWadObject _currentObject;
        private ArcBallCamera _camera;

        // Animation state.
        private readonly DispatcherTimer _animTimer;
        private int _currentFrame;
        private int _frameTimeout;
        private float _rotationFactor;
        private bool _animatePreview = true;

        // Mouse interaction state.
        private double _lastX;
        private double _lastY;
        private bool _isDragging;

        // Rendering state.
        private RenderingTextureAllocator _textureAllocator;
        private GraphicsDevice _legacyDevice;
        private WadRenderer _wadRenderer;

        private const float RotationSpeed = 0.005f;
        private const float RotationStep = 0.000125f;

        public IWadObject CurrentObject
        {
            get => _currentObject;
            set
            {
                if (value is WadSpriteSequence seq && seq.Sprites.Count > 0)
                {
                    if (!_animTimer.IsEnabled)
                        _animTimer.IsEnabled = true;
                    if (_currentObject != value)
                        _currentFrame = 0;
                }
                else
                {
                    _animTimer.IsEnabled = ValidObject(value) && AnimatePreview;
                }

                _currentObject = value;
                Render();
            }
        }

        public ArcBallCamera Camera
        {
            get => _camera;
            set => _camera = value;
        }

        public bool DrawTransparency { get; set; }

        public bool AnimatePreview
        {
            get => _animatePreview;
            set
            {
                if (_animatePreview == value)
                    return;

                _animatePreview = value;
                _animTimer.IsEnabled = value;
                _rotationFactor = 0.0f;
            }
        }

        // Configuration-driven properties. Set these from the host.
        public float FieldOfView { get; set; } = 50.0f;
        public float NavigationSpeedMouseRotate { get; set; } = 3.0f;
        public float NavigationSpeedMouseTranslate { get; set; } = 1024.0f;
        public float NavigationSpeedMouseWheelZoom { get; set; } = 1.0f;
        public float NavigationSpeedMouseZoom { get; set; } = 5000.0f;
        public Vector4 BackgroundColor { get; set; } = new Vector4(0.392f, 0.584f, 0.929f, 1.0f);

        protected override Vector4 ClearColor => BackgroundColor;

        public WpfItemPreview()
        {
            _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(15) };
            _animTimer.Tick += AnimTimer_Tick;

            Focusable = true;
            MouseEnter += OnPanelMouseEnter;
            MouseWheel += OnPanelMouseWheel;
            MouseDown += OnPanelMouseDown;
            MouseUp += OnPanelMouseUp;
            MouseMove += OnPanelMouseMove;
        }

        public void InitializeItemPreview(RenderingDevice device)
        {
            InitializeRendering(device);

            _textureAllocator = device.CreateTextureAllocator(
                new RenderingTextureAllocator.Description { Size = new VectorInt3(1024, 1024, 1) });

            _legacyDevice = DeviceManager.DefaultDeviceManager.___LegacyDevice;
            _wadRenderer = new WadRenderer(_legacyDevice, true, true, 1024, 512, false);

            ResetCamera();
        }

        public void ResetCamera()
        {
            Func<ArcBallCamera> defaultCamera = () =>
                new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0,
                    -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000,
                    FieldOfView * (float)(Math.PI / 180));

            _camera = WadObjectRenderHelper.CreateCameraForObject(_currentObject, _wadRenderer, FieldOfView)
                ?? defaultCamera();
        }

        public void GarbageCollect()
        {
            _wadRenderer?.GarbageCollect();
        }

        protected override void OnDraw()
        {
            if (!ValidObject(_currentObject))
                return;

            var swapChain = (Dx11OffscreenSwapChain)SwapChain;
            var device = (Dx11RenderingDevice)Device;

            // Ensure consistent state for legacy rendering.
            swapChain.BindForce();
            device.ResetState();

            int width = swapChain.Size.X;
            int height = swapChain.Size.Y;
            var viewProjection = _camera.GetViewProjectionMatrix(width, height);

            if (_currentObject is WadSpriteSequence seq)
            {
                if (seq.Sprites.Count <= _currentFrame)
                    return;

                var sprite = seq.Sprites[_currentFrame];
                float aspectRatioViewport = (float)width / height;
                float aspectRatioImage = (float)sprite.Texture.Image.Width / sprite.Texture.Image.Height;
                float aspectRatioAdjust = aspectRatioViewport / aspectRatioImage;
                var factor = Vector2.Min(new Vector2(1.0f / aspectRatioAdjust, aspectRatioAdjust), new Vector2(1.0f));

                SwapChain.RenderSprites(_textureAllocator, false, true, new List<Sprite>
                {
                    new Sprite
                    {
                        Texture = sprite.Texture.Image,
                        PosStart = -0.9f * factor,
                        PosEnd = 0.9f * factor
                    }
                });
            }
            else
            {
                WadObjectRenderHelper.RenderObject(_currentObject, _wadRenderer, _legacyDevice,
                    viewProjection, _camera.GetPosition(), DrawTransparency);
            }
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            if (!AnimatePreview && !(_currentObject is WadSpriteSequence))
                return;

            if (!IsVisible || !IsLoaded)
                return;

            if (_currentObject is WadSpriteSequence seq)
            {
                _frameTimeout++;
                if (_frameTimeout >= 20)
                {
                    _frameTimeout = 0;
                    if (_currentFrame < seq.Sprites.Count - 1)
                        _currentFrame++;
                    else
                        _currentFrame = 0;
                    Render();
                }
            }
            else if (AnimatePreview && _currentObject != null)
            {
                if (_rotationFactor < RotationSpeed)
                    _rotationFactor += RotationStep;
                else if (_rotationFactor > RotationSpeed)
                    _rotationFactor = RotationSpeed;

                _camera.Rotate(_rotationFactor, 0.0f);
                Render();
            }
        }

        private void OnPanelMouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsFocused && IsVisible)
                Focus();
        }

        private void OnPanelMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_camera == null)
                return;

            _camera.Zoom(-e.Delta * NavigationSpeedMouseWheelZoom);
            Render();
        }

        private void OnPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            _lastX = pos.X;
            _lastY = pos.Y;
            _isDragging = true;

            if (!(_currentObject is WadSpriteSequence) && e.ChangedButton != MouseButton.Left)
            {
                _animTimer.Stop();
                _rotationFactor = 0;
            }

            CaptureMouse();
        }

        private void OnPanelMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();

            if (!(_currentObject is WadSpriteSequence))
                _animTimer.Start();
        }

        private void OnPanelMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _currentObject == null || _camera == null)
                return;

            var pos = e.GetPosition(this);
            float deltaX = (float)(pos.X - _lastX) / (float)ActualHeight;
            float deltaY = (float)(pos.Y - _lastY) / (float)ActualHeight;

            _lastX = pos.X;
            _lastY = pos.Y;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    _camera.Zoom(-deltaY * NavigationSpeedMouseZoom);
                else if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    _camera.Rotate(deltaX * NavigationSpeedMouseRotate,
                                  -deltaY * NavigationSpeedMouseRotate);
            }

            if ((e.RightButton == MouseButtonState.Pressed && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) ||
                 e.MiddleButton == MouseButtonState.Pressed)
                _camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0) * NavigationSpeedMouseTranslate);

            Render();
        }

        private bool ValidObject(IWadObject obj)
        {
            if (obj == null)
                return false;

            if (obj is WadMoveable moveable)
                return moveable.Meshes.Any(m => m.VertexPositions.Count > 0);
            else if (obj is WadStatic wadStatic)
                return wadStatic.Mesh.VertexPositions.Count > 0;
            else if (obj is WadSpriteSequence spriteSeq)
                return spriteSeq.Sprites.Count > 0;
            else if (obj is ImportedGeometry)
                return true;
            else
                return false;
        }

        public void DisposeItemPreview()
        {
            _animTimer.Stop();
            _wadRenderer?.Dispose();
            _textureAllocator?.Dispose();
            DisposeRendering();
        }
    }
}
