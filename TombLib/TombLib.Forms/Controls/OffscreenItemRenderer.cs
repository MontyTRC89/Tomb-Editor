using NLog;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using SharpDX.Toolkit.Graphics;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering.DirectX11;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Controls
{
    public class OffscreenItemRenderer : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dx11RenderingDevice _device;
        private readonly GraphicsDevice _legacyDevice;
        private readonly WadRenderer _wadRenderer;

        private ID3D11Texture2D _renderTarget;
        private ID3D11RenderTargetView _renderTargetView;
        private ID3D11Texture2D _depthBuffer;
        private ID3D11DepthStencilView _depthBufferView;
        private ID3D11Texture2D _stagingTexture;
        private int _currentSize;

        public OffscreenItemRenderer()
        {
            _device = (Dx11RenderingDevice)DeviceManager.DefaultDeviceManager.Device;
            _legacyDevice = DeviceManager.DefaultDeviceManager.___LegacyDevice;
            _wadRenderer = new WadRenderer(_legacyDevice, true, true, 1024, 512, false);
        }

        public ImageC RenderThumbnail(IWadObject wadObject, TRVersion.Game version, Vector4 backColor, int size = 128)
        {
            const int FieldOfView = 50;

            if (wadObject == null)
                return ImageC.CreateNew(size, size);

            try
            {
                EnsureRenderTarget(size);

                // Set up camera using shared helper.
                var camera = WadObjectRenderHelper.CreateCameraForObject(wadObject, _wadRenderer, FieldOfView);
                if (camera == null)
                    return ImageC.CreateNew(size, size);

                // Bind our offscreen render target.
                BindRenderTarget(size);

                // Clear
                _device.Context.ClearRenderTargetView(_renderTargetView, new Color4(backColor.X, backColor.Y, backColor.Z, backColor.W));
                _device.Context.ClearDepthStencilView(_depthBufferView, DepthStencilClearFlags.Depth, 1.0f, 0);

                // Reset device state.
                _device.ResetState();

                // Get view-projection matrix.
                var viewProjection = camera.GetViewProjectionMatrix(size, size);

                // Render the object using shared helper.
                WadObjectRenderHelper.RenderObject(wadObject, _wadRenderer, _legacyDevice, viewProjection, camera.GetPosition(), false);

                // Read back pixels.
                return ReadPixels(size);
            }
            catch
            {
                logger.Error("Error while rendering thumbnail for object " + wadObject.ToString(version));
                return Dx11RenderingDevice.TextureUnavailable;
            }
        }

        private void EnsureRenderTarget(int size)
        {
            if (_renderTarget != null && _currentSize == size)
                return;

            DisposeRenderTargets();
            _currentSize = size;

            // Create color render target.
            _renderTarget = _device.Device.CreateTexture2D(new Texture2DDescription
            {
                Format = Format.B8G8R8A8_UNorm,
                Width = (uint)size,
                Height = (uint)size,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            });
            _renderTargetView = _device.Device.CreateRenderTargetView(_renderTarget);

            // Create depth buffer.
            _depthBuffer = _device.Device.CreateTexture2D(new Texture2DDescription
            {
                Format = Format.D32_Float,
                Width = (uint)size,
                Height = (uint)size,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            });
            _depthBufferView = _device.Device.CreateDepthStencilView(_depthBuffer);

            // Create staging texture for CPU readback.
            _stagingTexture = _device.Device.CreateTexture2D(new Texture2DDescription
            {
                Format = Format.B8G8R8A8_UNorm,
                Width = (uint)size,
                Height = (uint)size,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CPUAccessFlags = CpuAccessFlags.Read,
                MiscFlags = ResourceOptionFlags.None
            });
        }

        private void BindRenderTarget(int size)
        {
            _device.Context.RSSetViewport(0, 0, size, size, 0.0f, 1.0f);
            _device.Context.OMSetRenderTargets(_renderTargetView, _depthBufferView);
            _device.CurrentRenderTarget = null;
        }

        private ImageC ReadPixels(int size)
        {
            // Copy render target to staging texture.
            _device.Context.CopyResource(_stagingTexture, _renderTarget);

            // Map and read pixels.
            var mapped = _device.Context.Map(_stagingTexture, 0, MapMode.Read);
            try
            {
                int bytesPerPixel = 4;
                int rowPitch = (int)mapped.RowPitch;
                byte[] pixels = new byte[size * size * bytesPerPixel];

                // Copy row by row (rowPitch may differ from size * bytesPerPixel due to alignment).
                for (int y = 0; y < size; y++)
                    Marshal.Copy(mapped.DataPointer + y * rowPitch, pixels, y * size * bytesPerPixel, size * bytesPerPixel);

                return ImageC.FromByteArray(pixels, size, size);
            }
            catch
            {
                logger.Error("Error while reading pixels from offscreen render target.");
                return Dx11RenderingDevice.TextureUnavailable;
            }
            finally
            {
                _device.Context.Unmap(_stagingTexture, 0);
            }
        }

        private void DisposeRenderTargets()
        {
            _stagingTexture?.Dispose();
            _stagingTexture = null;
            _depthBufferView?.Dispose();
            _depthBufferView = null;
            _depthBuffer?.Dispose();
            _depthBuffer = null;
            _renderTargetView?.Dispose();
            _renderTargetView = null;
            _renderTarget?.Dispose();
            _renderTarget = null;
        }

        public void GarbageCollect()
        {
            _wadRenderer?.GarbageCollect();
        }

        public void Dispose()
        {
            DisposeRenderTargets();
            _wadRenderer?.Dispose();
        }
    }
}
