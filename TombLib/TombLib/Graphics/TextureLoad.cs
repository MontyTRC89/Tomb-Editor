using NLog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.IO;
using TombLib.Utils;
using D3D11Usage = Silk.NET.Direct3D11.Usage;

namespace TombLib.Graphics
{
    // Raw D3D11 texture loading using Silk.NET bindings.
    //
    // Caller responsibilities:
    //   - Provide a device pointer as nint (ID3D11Device*).
    //   - Dispose the returned LoadedTexture when done. Both the Texture2D and
    //     the ShaderResourceView are released.
    //
    // Because this type lives in TombLib (not TombLib.Rendering), the
    // LoadedTexture record uses nint handles rather than Silk.NET pointer types
    // so that non-unsafe consumers (ImportedGeometryTexture) can store them.
    public static unsafe class TextureLoad
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Holds an ID3D11Texture2D* and an ID3D11ShaderResourceView* as nint handles.
        /// Disposing releases both COM objects.
        /// </summary>
        public readonly record struct LoadedTexture(nint Texture, nint View) : IDisposable
        {
            public void Dispose()
            {
                if (View != 0)
                    ((ID3D11ShaderResourceView*)View)->Release();
                if (Texture != 0)
                    ((ID3D11Texture2D*)Texture)->Release();
            }
        }

        public static LoadedTexture Load(nint deviceHandle, ImageC image, D3D11Usage usage = D3D11Usage.Immutable)
        {
            if (deviceHandle == 0)
                return default;

            var device = (ID3D11Device*)deviceHandle;

            try
            {
                LoadedTexture result = default;
                image.GetIntPtr((IntPtr data) =>
                {
                    var description = new Texture2DDesc
                    {
                        ArraySize = 1,
                        BindFlags = (uint)BindFlag.ShaderResource,
                        CPUAccessFlags = 0,
                        Format = Format.FormatB8G8R8A8Unorm,
                        Height = (uint)image.Height,
                        MipLevels = 1,
                        MiscFlags = 0,
                        SampleDesc = new SampleDesc(1, 0),
                        Usage = usage,
                        Width = (uint)image.Width,
                    };
                    var subresource = new SubresourceData
                    {
                        PSysMem = (void*)data,
                        SysMemPitch = (uint)(image.Width * ImageC.PixelSize),
                        SysMemSlicePitch = 0,
                    };
                    ID3D11Texture2D* tex;
                    SilkMarshal.ThrowHResult(device->CreateTexture2D(&description, &subresource, &tex));
                    ID3D11ShaderResourceView* srv;
                    SilkMarshal.ThrowHResult(device->CreateShaderResourceView((ID3D11Resource*)tex, null, &srv));
                    result = new LoadedTexture((nint)tex, (nint)srv);
                });
                return result;
            }
            catch
            {
                // Fallback to a plain red texture so the editor can keep rendering.
                if (image != ImageC.Red)
                    return Load(deviceHandle, ImageC.Red, usage);
                return default;
            }
        }

        public static LoadedTexture Load(nint device, Stream stream)
            => Load(device, ImageC.FromStream(stream));

        public static LoadedTexture Load(nint device, string path)
            => Load(device, ImageC.FromFile(path));

        // Updates a sub-region of an existing texture array slice. `position.X/Y` is the
        // pixel offset within the slice; `position.Z` selects the slice (texture array).
        // The texture must be Usage.Default (or Dynamic) — Immutable cannot be updated.
        public static void Update(nint contextHandle, nint textureHandle, ImageC image, VectorInt3 position)
        {
            if (image.Width == 0 || image.Height == 0)
                return;

            var context = (ID3D11DeviceContext*)contextHandle;
            var texture = (ID3D11Texture2D*)textureHandle;

            image.GetIntPtr((IntPtr data) =>
            {
                var box = new Box
                {
                    Left = (uint)position.X,
                    Right = (uint)(position.X + image.Width),
                    Top = (uint)position.Y,
                    Bottom = (uint)(position.Y + image.Height),
                    Front = 0,
                    Back = 1,
                };
                uint subresourceIndex = (uint)position.Z; // mip 0 of slice `position.Z`
                context->UpdateSubresource(
                    (ID3D11Resource*)texture,
                    subresourceIndex,
                    &box,
                    (void*)data,
                    (uint)(image.Width * ImageC.PixelSize),
                    0);
            });
        }
    }
}
