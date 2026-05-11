using SharpDX.Direct3D11;
using System.Numerics;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TombLib.Rendering.DirectX11
{
    // Wraps a single constant buffer (cbuffer slot 0) used by every "new path" shader
    // (RoomShader, SpriteShader, TextShader). The struct layout MUST match the cbuffer
    // declared in RoomShaderPS.hlsl bit-for-bit — that's why each field has an explicit
    // FieldOffset and not just sequential ordering.
    //
    // HLSL constant buffer packing rules (link below) require that no scalar straddles
    // a 16-byte boundary; vectors of size 4 must start on a 16-byte boundary; smaller
    // vectors may pack into the remaining slots. Bools become 4-byte ints in the cbuffer
    // (which is why we marshal them as `int` here, not C# bool/byte).
    //
    // The buffer is created with ResourceUsage.Default + UpdateSubresource — DEFAULT
    // beats DYNAMIC for cbuffers updated once per frame because UpdateSubresource on
    // a small constant buffer goes through a fast path on every modern driver.
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
            Buffer.ShowExtraBlendingModes = State.ShowExtraBlendingModes ? 1 : 0;
            Buffer.ShowLightingWhiteTextureOnly = State.ShowLightingWhiteTextureOnly ? 1 : 0;
            Buffer.LightMode = State.LightMode;
            Buffer.BrushShape = State.BrushShape;
            Buffer.BrushRotation = State.BrushRotation;
            Buffer.BrushCenter = State.BrushCenter;
            Buffer.BrushColor = State.BrushColor;
            Context.UpdateSubresource(ref Buffer, ConstantBuffer);
        }
    }
}
