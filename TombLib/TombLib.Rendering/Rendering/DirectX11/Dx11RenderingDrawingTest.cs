using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using D3D11Usage = Silk.NET.Direct3D11.Usage;
using Vector3 = System.Numerics.Vector3;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11RenderingDrawingTest : RenderingDrawingTest
    {
        public readonly Dx11RenderingDevice Device;
        public readonly ID3D11Buffer* VertexBuffer;
        public readonly Dx11VertexBufferBinding[] VertexBufferBindings;

        public Dx11RenderingDrawingTest(Dx11RenderingDevice device, Description description)
        {
            Device = device;

            // Create buffer
            const int vertexCount = 3;
            int size = vertexCount * (sizeof(Vector3) + sizeof(uint));
            fixed (byte* data = new byte[size])
            {
                Vector3* positions = (Vector3*)(data);
                uint* colors = (uint*)(data + vertexCount * sizeof(Vector3));

                // Setup vertices
                positions[0] = new Vector3(0.0f, 0.0f, 0.0f);
                colors[0] = 0xff000080;
                positions[1] = new Vector3(0.0f, 1.0f, 0.0f);
                colors[1] = 0xff008000;
                positions[2] = new Vector3(1.0f, 0.0f, 0.0f);
                colors[2] = 0xff800000;

                // Create GPU resources
                var desc = new BufferDesc
                {
                    ByteWidth = (uint)size,
                    Usage = D3D11Usage.Immutable,
                    BindFlags = (uint)BindFlag.VertexBuffer,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    StructureByteStride = 0,
                };
                var subresData = new SubresourceData
                {
                    PSysMem = data,
                };
                ID3D11Buffer* buf;
                SilkMarshal.ThrowHResult(device.Device->CreateBuffer(&desc, &subresData, &buf));
                VertexBuffer = buf;

                VertexBufferBindings = new Dx11VertexBufferBinding[] {
                    new Dx11VertexBufferBinding(VertexBuffer, sizeof(Vector3), (int)((byte*)positions - data)),
                    new Dx11VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)colors - data))
                };
            }
        }

        public override void Dispose()
        {
            VertexBuffer->Release();
        }

        public override void Render(RenderArgs arg)
        {
            /*var context = Device.Context;

            // Setup state
            ((Dx11RenderingSwapChain)arg.RenderTarget).Bind();
            Device.TestShader.Apply(context, arg.StateBuffer);
            Dx11RenderingDevice.SetVertexBuffers(context, 0, VertexBufferBindings);

            // Render
            context->Draw(3, 0);*/
        }
    }
}
