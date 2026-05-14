using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using TombLib.Utils;
using D3D11Usage = Silk.NET.Direct3D11.Usage;

namespace TombLib.Graphics
{
    // Silk.NET/D3D11 implementation of WadRenderer's atlas. Holds a
    // Texture2DArray + ShaderResourceView as raw COM pointers. The legacy
    // WadRenderer body now lives in the base class; only the atlas-storage
    // bits are here.
    public unsafe class Dx11WadRenderer : WadRenderer
    {
        // Raw COM pointers. Stored as nint at the field level so the class
        // can live in TombLib (which avoids forcing every consumer into an
        // unsafe context). Internal methods cast to typed pointers.
        private nint _deviceHandle;
        private nint _contextHandle;
        private nint _atlasTextureHandle;
        private nint _textureViewHandle;

        public override object Texture => _textureViewHandle;

        public Dx11WadRenderer(nint deviceHandle, bool compactTexture, bool correctTexture, int atlasSize, int maxAllocationSize, bool loadAnimations)
            : base(compactTexture, correctTexture, atlasSize, maxAllocationSize, loadAnimations)
        {
            _deviceHandle = deviceHandle;

            // Cache the immediate context pointer so we don't query it every frame.
            var device = (ID3D11Device*)deviceHandle;
            ID3D11DeviceContext* ctx;
            device->GetImmediateContext(&ctx);
            _contextHandle = (nint)ctx;
        }

        protected override void OnInitializeTexture()
        {
            if (_atlasTextureHandle != 0) return;

            var device = (ID3D11Device*)_deviceHandle;

            var desc = new Texture2DDesc
            {
                Width = (uint)TextureAtlasSize,
                Height = (uint)TextureAtlasSize,
                MipLevels = 1,
                ArraySize = (uint)CurrentPageCount,
                Format = Format.FormatB8G8R8A8Unorm,
                SampleDesc = new SampleDesc(1, 0),
                Usage = D3D11Usage.Default,
                BindFlags = (uint)BindFlag.ShaderResource,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };
            ID3D11Texture2D* tex;
            SilkMarshal.ThrowHResult(device->CreateTexture2D(&desc, null, &tex));
            _atlasTextureHandle = (nint)tex;

            ID3D11ShaderResourceView* srv;
            SilkMarshal.ThrowHResult(device->CreateShaderResourceView((ID3D11Resource*)tex, null, &srv));
            _textureViewHandle = (nint)srv;
        }

        protected override void OnEnsureCapacity(int pages)
        {
            var oldTex = (ID3D11Texture2D*)_atlasTextureHandle;

            Texture2DDesc oldDesc;
            oldTex->GetDesc(&oldDesc);
            int arraySize = (int)oldDesc.ArraySize;
            if (pages <= arraySize) return;

            var device = (ID3D11Device*)_deviceHandle;
            var context = (ID3D11DeviceContext*)_contextHandle;

            var newDesc = oldDesc;
            newDesc.ArraySize = (uint)pages;
            ID3D11Texture2D* newTex;
            SilkMarshal.ThrowHResult(device->CreateTexture2D(&newDesc, null, &newTex));

            // Copy existing slices. With MipLevels=1, subresource index == array slice index.
            for (uint i = 0; i < (uint)arraySize; i++)
            {
                context->CopySubresourceRegion(
                    (ID3D11Resource*)newTex, i, 0, 0, 0,
                    (ID3D11Resource*)oldTex, i, null);
            }

            // Release old resources.
            if (_textureViewHandle != 0)
                ((ID3D11ShaderResourceView*)_textureViewHandle)->Release();
            oldTex->Release();

            _atlasTextureHandle = (nint)newTex;

            ID3D11ShaderResourceView* srv;
            SilkMarshal.ThrowHResult(device->CreateShaderResourceView((ID3D11Resource*)newTex, null, &srv));
            _textureViewHandle = (nint)srv;
        }

        protected override void OnUploadSubregion(ImageC image, VectorInt3 position)
        {
            TextureLoad.Update(_contextHandle, _atlasTextureHandle, image, position);
        }

        protected override void OnDisposeTexture()
        {
            if (_textureViewHandle != 0)
            {
                ((ID3D11ShaderResourceView*)_textureViewHandle)->Release();
                _textureViewHandle = 0;
            }
            if (_atlasTextureHandle != 0)
            {
                ((ID3D11Texture2D*)_atlasTextureHandle)->Release();
                _atlasTextureHandle = 0;
            }
        }
    }
}
