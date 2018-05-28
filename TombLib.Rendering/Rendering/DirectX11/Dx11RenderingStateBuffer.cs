using System;
using System.Numerics;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingStateBuffer : RenderingStateBuffer
    {
        // Microsoft reference for "Packing Rules for Constant Variables":
        // https://msdn.microsoft.com/en-us/library/windows/desktop/bb509632(v=vs.85).aspx
        [StructLayout(LayoutKind.Explicit)]
        public struct ConstantBufferLayout
        {
            [FieldOffset(0)]
            public Matrix4x4 TransformMatrix;
            [FieldOffset(64)]
            public float RoomGridLineWidth;
            [FieldOffset(68)]
            public int RoomGridForce;
            [FieldOffset(72)]
            public int RoomDisableVertexColors;
        };
        public static readonly int Size = ((Marshal.SizeOf(typeof(ConstantBufferLayout)) + 15) / 16) * 16;

        public readonly DeviceContext Context;
        public readonly Buffer ConstantBuffer;

        public Dx11RenderingStateBuffer(Dx11RenderingDevice device)
        {
            Context = device.Context;
            ConstantBuffer = new Buffer(device.Device, Size, ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public override void Dispose()
        {
            ConstantBuffer.Dispose();
        }

        public override void Set(RenderingState State)
        {
            ConstantBufferLayout Buffer;
            Buffer.TransformMatrix = State.TransformMatrix;
            Buffer.RoomGridLineWidth = State.RoomGridLineWidth;
            Buffer.RoomGridForce = State.RoomGridForce ? 1 : 0;
            Buffer.RoomDisableVertexColors = State.RoomDisableVertexColors ? 1 : 0;
            Context.UpdateSubresource(ref Buffer, ConstantBuffer);
        }
    }
}
