using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TombLib.Rendering.OpenGL
{
    // Per-frame uniform block (UBO at binding point 0). Layout matches the
    // Vulkan/DX11 ConstantBufferLayout bit-for-bit (std140 compatible).
    public sealed class OpenGLRenderingStateBuffer : RenderingStateBuffer
    {
        [StructLayout(LayoutKind.Explicit, Size = 128)]
        public struct ConstantBufferLayout
        {
            [FieldOffset(0)]   public Matrix4x4 TransformMatrix;
            [FieldOffset(64)]  public float RoomGridLineWidth;
            [FieldOffset(68)]  public int RoomGridForce;
            [FieldOffset(72)]  public int RoomDisableVertexColors;
            [FieldOffset(76)]  public int ShowExtraBlendingModes;
            [FieldOffset(80)]  public int ShowLightingWhiteTextureOnly;
            [FieldOffset(84)]  public int LightMode;
            [FieldOffset(88)]  public int BrushShape;
            [FieldOffset(92)]  public float BrushRotation;
            [FieldOffset(96)]  public Vector4 BrushCenter;
            [FieldOffset(112)] public Vector4 BrushColor;
        }
        public const uint Size = 128;
        // Binding point 0 for all shaders' FrameData uniform block.
        public const uint BindingPoint = 0;

        public readonly OpenGLRenderingDevice DeviceWrapper;
        public uint Buffer { get; private set; }

        private readonly GL _gl;

        public unsafe OpenGLRenderingStateBuffer(OpenGLRenderingDevice device)
        {
            DeviceWrapper = device;
            _gl = device.Gl;

            uint buf = 0;
            _gl.GenBuffers(1, &buf);
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, buf);
            _gl.BufferData(BufferTargetARB.UniformBuffer, Size, null, BufferUsageARB.DynamicDraw);
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
            Buffer = buf;
        }

        public override unsafe void Set(RenderingState State)
        {
            ConstantBufferLayout cb;
            cb.TransformMatrix = State.TransformMatrix;
            cb.RoomGridLineWidth = State.RoomGridLineWidth;
            cb.RoomGridForce = State.RoomGridForce ? 1 : 0;
            cb.RoomDisableVertexColors = State.RoomDisableVertexColors ? 1 : 0;
            cb.ShowExtraBlendingModes = State.ShowExtraBlendingModes ? 1 : 0;
            cb.ShowLightingWhiteTextureOnly = State.ShowLightingWhiteTextureOnly ? 1 : 0;
            cb.LightMode = State.LightMode;
            cb.BrushShape = State.BrushShape;
            cb.BrushRotation = State.BrushRotation;
            cb.BrushCenter = State.BrushCenter;
            cb.BrushColor = State.BrushColor;

            _gl.BindBuffer(BufferTargetARB.UniformBuffer, Buffer);
            _gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, Size, &cb);
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
        }

        // Binds this UBO to the FrameData binding point.
        public void Bind()
        {
            _gl.BindBufferBase(BufferTargetARB.UniformBuffer, BindingPoint, Buffer);
        }

        public override unsafe void Dispose()
        {
            if (Buffer != 0) { uint b = Buffer; _gl.DeleteBuffers(1, &b); Buffer = 0; }
        }
    }
}
