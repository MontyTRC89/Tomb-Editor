using Vortice.Direct3D11;
using System.Numerics;
using System.Runtime.InteropServices;

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
            [FieldOffset(76)]
            public int ShowExtraBlendingModes;
            [FieldOffset(80)]
            public int ShowLightingWhiteTextureOnly;
            [FieldOffset(84)]
            public int LightMode;
            [FieldOffset(88)]
            public int BrushShape; // 0=none, 1=circle, 2=square
            [FieldOffset(92)]
            public float BrushRotation; // Degrees, for rotation indicator line
            [FieldOffset(96)]
            public Vector4 BrushCenter; // xyz = world center, w = radius
            [FieldOffset(112)]
            public Vector4 BrushColor;
        };
        public static readonly int Size = ((Marshal.SizeOf(typeof(ConstantBufferLayout)) + 15) / 16) * 16;

        public readonly ID3D11DeviceContext Context;
        public readonly ID3D11Buffer ConstantBuffer;

        public Dx11RenderingStateBuffer(Dx11RenderingDevice device)
        {
            Context = device.Context;
            ConstantBuffer = device.Device.CreateBuffer(new BufferDescription((uint)Size, BindFlags.ConstantBuffer, ResourceUsage.Default));
        }

        public override void Dispose()
        {
            ConstantBuffer.Dispose();
        }

        public override void Set(RenderingState State)
        {
            ConstantBufferLayout buffer;
            buffer.TransformMatrix = State.TransformMatrix;
            buffer.RoomGridLineWidth = State.RoomGridLineWidth;
            buffer.RoomGridForce = State.RoomGridForce ? 1 : 0;
            buffer.RoomDisableVertexColors = State.RoomDisableVertexColors ? 1 : 0;
            buffer.ShowExtraBlendingModes = State.ShowExtraBlendingModes ? 1 : 0;
            buffer.ShowLightingWhiteTextureOnly = State.ShowLightingWhiteTextureOnly ? 1 : 0;
            buffer.LightMode = State.LightMode;
            buffer.BrushShape = State.BrushShape;
            buffer.BrushRotation = State.BrushRotation;
            buffer.BrushCenter = State.BrushCenter;
            buffer.BrushColor = State.BrushColor;
            Context.UpdateSubresource(buffer, ConstantBuffer);
        }
    }
}
