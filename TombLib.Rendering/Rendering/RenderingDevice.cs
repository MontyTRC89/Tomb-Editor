using System;

namespace TombLib.Rendering
{
    /// <summary>
    /// Represents the graphical device for rendering.
    /// At the moment we only support rendering using DirectX11
    /// But later we may want to add other rendering implementations.
    /// </summary>
    public abstract class RenderingDevice : IDisposable
    {
        public abstract void Dispose();

        public abstract RenderingSwapChain CreateSwapChain(RenderingSwapChain.Description description);
        public abstract RenderingTextureAllocator CreateTextureAllocator(RenderingTextureAllocator.Description description);
        public abstract RenderingStateBuffer CreateStateBuffer();
        public abstract RenderingDrawingTest CreateDrawingTest(RenderingDrawingTest.Description description);
        public abstract RenderingDrawingRoom CreateDrawingRoom(RenderingDrawingRoom.Description description);
        public abstract RenderingFont CreateFont(RenderingFont.Description description);

        public RenderingTextureAllocator CreateTextureAllocator() => CreateTextureAllocator(new RenderingTextureAllocator.Description());
    }
}
