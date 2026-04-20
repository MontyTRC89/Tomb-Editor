using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TombLib;
using TombLib.Rendering;
using TombLib.Rendering.DirectX11;

namespace WadTool.Controls
{
    /// <summary>
    /// A WPF rendering surface that uses Dx11OffscreenSwapChain to render 3D content
    /// and displays the result via WriteableBitmap.
    /// </summary>
    public class WpfRenderingPanel : Border
    {
        private readonly Image _image;
        private WriteableBitmap _writeableBitmap;
        private Dx11OffscreenSwapChain _swapChain;
        private Dx11RenderingDevice _device;
        private bool _initialized;

        public event EventHandler Draw;

        public RenderingSwapChain SwapChain => _swapChain;
        public RenderingDevice Device => _device;

        public bool AllowRendering { get; set; } = true;

        public WpfRenderingPanel()
        {
            _image = new Image
            {
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            Child = _image;
            Background = Brushes.Transparent;
            ClipToBounds = true;

            SizeChanged += OnSizeChanged;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        public void InitializeRendering(RenderingDevice device)
        {
            _device = (Dx11RenderingDevice)device;

            int width = Math.Max(1, (int)ActualWidth);
            int height = Math.Max(1, (int)ActualHeight);

            _swapChain = new Dx11OffscreenSwapChain(_device, new VectorInt2(width, height));
            _writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            _image.Source = _writeableBitmap;
            _initialized = true;
        }

        /// <summary>
        /// Requests a re-render of the 3D content.
        /// </summary>
        public void Render()
        {
            if (!_initialized || !AllowRendering || !IsVisible)
                return;

            if (_swapChain.Size.X <= 0 || _swapChain.Size.Y <= 0)
                return;

            try
            {
                _swapChain.Clear(ClearColor);
                OnDraw();
                PresentToWriteableBitmap();
            }
            catch (Exception)
            {
                // Silently handle render errors to avoid crashes during resize.
            }
        }

        protected virtual void OnDraw()
        {
            Draw?.Invoke(this, EventArgs.Empty);
        }

        protected virtual Vector4 ClearColor => new Vector4(0.392f, 0.584f, 0.929f, 1.0f);

        private void PresentToWriteableBitmap()
        {
            if (_writeableBitmap == null || _swapChain == null)
                return;

            _writeableBitmap.Lock();
            try
            {
                _swapChain.ReadPixelsDirect(
                    _writeableBitmap.BackBuffer,
                    _writeableBitmap.BackBufferStride);

                _writeableBitmap.AddDirtyRect(
                    new Int32Rect(0, 0, _writeableBitmap.PixelWidth, _writeableBitmap.PixelHeight));
            }
            finally
            {
                _writeableBitmap.Unlock();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_initialized)
                return;

            int width = Math.Max(1, (int)e.NewSize.Width);
            int height = Math.Max(1, (int)e.NewSize.Height);

            _swapChain.Resize(new VectorInt2(width, height));
            _writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            _image.Source = _writeableBitmap;

            Render();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && _initialized)
                Render();
        }

        public void DisposeRendering()
        {
            _swapChain?.Dispose();
            _swapChain = null;
            _initialized = false;
        }
    }
}
