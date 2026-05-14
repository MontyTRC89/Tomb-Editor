using System;

namespace TombLib.Rendering.OpenGL
{
    // Smoke-test drawing class. Same no-op as the Vulkan/DX11 counterparts.
    public sealed class OpenGLRenderingDrawingTest : RenderingDrawingTest
    {
        public readonly OpenGLRenderingDevice DeviceWrapper;

        public OpenGLRenderingDrawingTest(OpenGLRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
        }

        public override void Dispose() { }

        public override void Render(RenderArgs arg) { }
    }
}
