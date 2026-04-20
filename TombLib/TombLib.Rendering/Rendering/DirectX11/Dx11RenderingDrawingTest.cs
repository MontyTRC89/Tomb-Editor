using Vortice.Direct3D11;
using System;
using System.Numerics;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingDrawingTest : RenderingDrawingTest
    {
        public readonly Dx11RenderingDevice Device;
        public readonly ID3D11Buffer VertexBuffer;
        public readonly ID3D11Buffer[] VertexBuffers;
        public readonly int[] VertexStrides;
        public readonly int[] VertexOffsets;

        public unsafe Dx11RenderingDrawingTest(Dx11RenderingDevice device, Description description)
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
                int posOffset = (int)((byte*)positions - data);
                int colOffset = (int)((byte*)colors - data);

                VertexBuffer = device.Device.CreateBuffer(
                    new BufferDescription((uint)size, BindFlags.VertexBuffer, ResourceUsage.Immutable),
                    new SubresourceData(new IntPtr(data)));
                VertexBuffers = new ID3D11Buffer[] { VertexBuffer, VertexBuffer };
                VertexStrides = new int[] { sizeof(Vector3), sizeof(uint) };
                VertexOffsets = new int[] { posOffset, colOffset };
            }
        }

        public override void Dispose()
        {
            VertexBuffer.Dispose();
        }

        public override void Render(RenderArgs arg)
        {
            /*var context = Device.Context;

            // Setup state
            ((Dx11RenderingSwapChain)arg.RenderTarget).Bind();
            Device.TestShader.Apply(context, arg.StateBuffer);
            context.IASetVertexBuffers(0, VertexBuffers, VertexStrides, VertexOffsets);

            // Render
            context.Draw(3, 0);*/
        }
    }
}
