using NLog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Rendering.DirectX11;
using TombLib.Utils;
using TombLib.Wad;
using D3D11Usage = Silk.NET.Direct3D11.Usage;

namespace TombLib.Controls
{
    public unsafe class OffscreenItemRenderer : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dx11RenderingDevice _device;
        private readonly WadRenderer _wadRenderer;
        private readonly RenderingStateBuffer _stateBuffer;

        private ID3D11Texture2D* _renderTarget;
        private ID3D11RenderTargetView* _renderTargetView;
        private ID3D11Texture2D* _depthBuffer;
        private ID3D11DepthStencilView* _depthBufferView;
        private ID3D11Texture2D* _stagingTexture;
        private int _currentSize;

        public OffscreenItemRenderer()
        {
            // Thumbnail rendering is DX11-only — the path uses Silk.NET-typed offscreen
            // render targets that haven't been ported to Vulkan. Under Vulkan we keep
            // _device null and RenderThumbnail returns a blank ImageC.
            _device = DeviceManager.DefaultDeviceManager.Device as Dx11RenderingDevice;
            _wadRenderer = DeviceManager.DefaultDeviceManager.CreateWadRenderer(true, true, 1024, 512, false);
            _stateBuffer = _device?.CreateStateBuffer();
        }

        public ImageC RenderThumbnail(IWadObject wadObject, TRVersion.Game version, Vector4 backColor, int size = 128)
        {
            const int FieldOfView = 50;

            if (wadObject == null || _device == null)
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
                float* clearColor = stackalloc float[4];
                clearColor[0] = backColor.X;
                clearColor[1] = backColor.Y;
                clearColor[2] = backColor.Z;
                clearColor[3] = backColor.W;
                _device.Context->ClearRenderTargetView(_renderTargetView, clearColor);
                _device.Context->ClearDepthStencilView(_depthBufferView, (uint)ClearFlag.Depth, 1.0f, 0);

                // Reset device state.
                _device.ResetState();

                // Get view-projection matrix.
                var viewProjection = camera.GetViewProjectionMatrix(size, size);

                // Render the object using shared helper. RenderTarget=null tells the
                // unified path to NOT bind a SwapChain — we already bound our offscreen
                // RTV via BindRenderTarget().
                _stateBuffer.Set(new TombLib.Rendering.RenderingState { TransformMatrix = viewProjection });
                WadObjectRenderHelper.RenderObject(wadObject, _wadRenderer, _device, /*swapChain*/ null, _stateBuffer, camera.GetPosition(), false);

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
            var rtDesc = new Texture2DDesc
            {
                Format = Format.FormatB8G8R8A8Unorm,
                Width = (uint)size,
                Height = (uint)size,
                ArraySize = 1,
                MipLevels = 1,
                SampleDesc = new SampleDesc(1, 0),
                Usage = D3D11Usage.Default,
                BindFlags = (uint)BindFlag.RenderTarget,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };
            ID3D11Texture2D* rt;
            SilkMarshal.ThrowHResult(_device.Device->CreateTexture2D(&rtDesc, null, &rt));
            _renderTarget = rt;

            ID3D11RenderTargetView* rtv;
            SilkMarshal.ThrowHResult(_device.Device->CreateRenderTargetView((ID3D11Resource*)rt, null, &rtv));
            _renderTargetView = rtv;

            // Create depth buffer.
            var dsDesc = new Texture2DDesc
            {
                Format = Format.FormatD32Float,
                Width = (uint)size,
                Height = (uint)size,
                ArraySize = 1,
                MipLevels = 1,
                SampleDesc = new SampleDesc(1, 0),
                Usage = D3D11Usage.Default,
                BindFlags = (uint)BindFlag.DepthStencil,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };
            ID3D11Texture2D* ds;
            SilkMarshal.ThrowHResult(_device.Device->CreateTexture2D(&dsDesc, null, &ds));
            _depthBuffer = ds;

            ID3D11DepthStencilView* dsv;
            SilkMarshal.ThrowHResult(_device.Device->CreateDepthStencilView((ID3D11Resource*)ds, null, &dsv));
            _depthBufferView = dsv;

            // Create staging texture for CPU readback.
            var stagingDesc = new Texture2DDesc
            {
                Format = Format.FormatB8G8R8A8Unorm,
                Width = (uint)size,
                Height = (uint)size,
                ArraySize = 1,
                MipLevels = 1,
                SampleDesc = new SampleDesc(1, 0),
                Usage = D3D11Usage.Staging,
                BindFlags = 0,
                CPUAccessFlags = (uint)CpuAccessFlag.Read,
                MiscFlags = 0,
            };
            ID3D11Texture2D* staging;
            SilkMarshal.ThrowHResult(_device.Device->CreateTexture2D(&stagingDesc, null, &staging));
            _stagingTexture = staging;
        }

        private void BindRenderTarget(int size)
        {
            var viewport = new Viewport
            {
                TopLeftX = 0,
                TopLeftY = 0,
                Width = size,
                Height = size,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };
            _device.Context->RSSetViewports(1, &viewport);

            var rtv = _renderTargetView;
            _device.Context->OMSetRenderTargets(1, &rtv, _depthBufferView);
            _device.CurrentRenderTarget = null;
        }

        private ImageC ReadPixels(int size)
        {
            // Copy render target to staging texture.
            _device.Context->CopyResource((ID3D11Resource*)_stagingTexture, (ID3D11Resource*)_renderTarget);

            // Map and read pixels.
            MappedSubresource mapped;
            SilkMarshal.ThrowHResult(
                _device.Context->Map((ID3D11Resource*)_stagingTexture, 0, Map.Read, 0, &mapped));
            try
            {
                int bytesPerPixel = 4;
                int rowPitch = (int)mapped.RowPitch;
                byte[] pixels = new byte[size * size * bytesPerPixel];

                // Copy row by row (rowPitch may differ from size * bytesPerPixel due to alignment).
                for (int y = 0; y < size; y++)
                    Marshal.Copy((IntPtr)((byte*)mapped.PData + y * rowPitch), pixels, y * size * bytesPerPixel, size * bytesPerPixel);

                return ImageC.FromByteArray(pixels, size, size);
            }
            catch
            {
                logger.Error("Error while reading pixels from offscreen render target.");
                return Dx11RenderingDevice.TextureUnavailable;
            }
            finally
            {
                _device.Context->Unmap((ID3D11Resource*)_stagingTexture, 0);
            }
        }

        private void DisposeRenderTargets()
        {
            if (_stagingTexture != null) { _stagingTexture->Release(); _stagingTexture = null; }
            if (_depthBufferView != null) { _depthBufferView->Release(); _depthBufferView = null; }
            if (_depthBuffer != null) { _depthBuffer->Release(); _depthBuffer = null; }
            if (_renderTargetView != null) { _renderTargetView->Release(); _renderTargetView = null; }
            if (_renderTarget != null) { _renderTarget->Release(); _renderTarget = null; }
        }

        public void GarbageCollect()
        {
            _wadRenderer?.GarbageCollect();
        }

        public void Dispose()
        {
            DisposeRenderTargets();
            _stateBuffer?.Dispose();
            _wadRenderer?.Dispose();
        }
    }
}
