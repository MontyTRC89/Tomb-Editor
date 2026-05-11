using NLog;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.IO;
using TombLib.Utils;

namespace TombLib.Graphics
{
    // Raw D3D11 texture loading. Migrated from SharpDX.Toolkit Texture2D to
    // SharpDX.Direct3D11.Texture2D + ShaderResourceView so the unified rendering path
    // can bind textures directly without the toolkit wrapper.
    //
    // Caller responsibilities:
    //   - Provide a SharpDX.Direct3D11.Device.
    //   - Dispose both the Texture2D AND the ShaderResourceView when done. The pair
    //     is returned via the LoadedTexture record below.
    public static class TextureLoad
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public readonly record struct LoadedTexture(Texture2D Texture, ShaderResourceView View) : IDisposable
        {
            public void Dispose()
            {
                View?.Dispose();
                Texture?.Dispose();
            }
        }

        public static LoadedTexture Load(Device device, ImageC image, ResourceUsage usage = ResourceUsage.Immutable)
        {
            if (device == null)
                return default;

            try
            {
                LoadedTexture result = default;
                image.GetIntPtr((IntPtr data) =>
                {
                    var description = new Texture2DDescription
                    {
                        ArraySize = 1,
                        BindFlags = BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        Height = image.Height,
                        MipLevels = 1,
                        OptionFlags = ResourceOptionFlags.None,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        Usage = usage,
                        Width = image.Width,
                    };
                    var dataBox = new DataBox(data, image.Width * ImageC.PixelSize, 0);
                    var tex = new Texture2D(device, description, new[] { dataBox });
                    var srv = new ShaderResourceView(device, tex);
                    result = new LoadedTexture(tex, srv);
                });
                return result;
            }
            catch
            {
                // Fallback to a plain red texture so the editor can keep rendering.
                if (image != ImageC.Red)
                    return Load(device, ImageC.Red, usage);
                return default;
            }
        }

        public static LoadedTexture Load(Device device, Stream stream)
            => Load(device, ImageC.FromStream(stream));

        public static LoadedTexture Load(Device device, string path)
            => Load(device, ImageC.FromFile(path));

        // Updates a sub-region of an existing texture array slice. `position.X/Y` is the
        // pixel offset within the slice; `position.Z` selects the slice (texture array).
        // The texture must be Usage.Default (or Dynamic) — Immutable cannot be updated.
        public static void Update(DeviceContext context, Texture2D texture, ImageC image, VectorInt3 position)
        {
            if (image.Width == 0 || image.Height == 0)
                return;

            image.GetIntPtr((IntPtr data) =>
            {
                var region = new ResourceRegion
                {
                    Left = position.X,
                    Right = position.X + image.Width,
                    Top = position.Y,
                    Bottom = position.Y + image.Height,
                    Front = 0,
                    Back = 1,
                };
                var box = new DataBox(data, image.Width * ImageC.PixelSize, 0);
                int subresourceIndex = position.Z; // mip 0 of slice `position.Z`
                context.UpdateSubresource(box, texture, subresourceIndex, region);
            });
        }
    }
}
