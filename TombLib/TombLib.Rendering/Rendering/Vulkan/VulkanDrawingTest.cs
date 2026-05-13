using System;

namespace TombLib.Rendering.Vulkan
{
    // Smoke-test drawing class. The Dx11 counterpart's Render is also a no-op
    // (commented out) — the type exists purely so the factory method
    // CreateDrawingTest doesn't throw and any caller wiring is preserved.
    // If full diagnostics are required, replace this with a 3-vertex triangle
    // pipeline mirroring VulkanDrawingLines.
    public sealed class VulkanDrawingTest : RenderingDrawingTest
    {
        public readonly VulkanRenderingDevice DeviceWrapper;

        public VulkanDrawingTest(VulkanRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
        }

        public override void Dispose() { }

        public override void Render(RenderArgs arg) { }
    }
}
