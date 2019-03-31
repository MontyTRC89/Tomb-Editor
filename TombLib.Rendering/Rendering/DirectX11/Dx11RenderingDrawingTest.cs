using SharpDX;
using SharpDX.Direct3D11;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingDrawingTest : RenderingDrawingTest
    {
        public readonly Dx11RenderingDevice Device;
        public readonly Buffer VertexBuffer;
        public readonly VertexBufferBinding[] VertexBufferBindings;

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
                VertexBuffer = new Buffer(device.Device, new IntPtr(data),
                    new BufferDescription(size, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                    CpuAccessFlags.None, ResourceOptionFlags.None, 0));
                VertexBufferBindings = new VertexBufferBinding[] {
                    new VertexBufferBinding(VertexBuffer, sizeof(Vector3), (int)((byte*)positions - data)),
                    new VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)colors - data))
                };
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
            context.InputAssembler.SetVertexBuffers(0, VertexBufferBindings);

            // Render
            context.Draw(3, 0);*/
        }
    }
}
