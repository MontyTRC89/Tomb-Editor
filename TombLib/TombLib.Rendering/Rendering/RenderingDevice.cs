using System;

namespace TombLib.Rendering
{
    // Abstract factory for every GPU resource the editor needs.
    //
    // The renderer is split in two layers:
    //   1) This abstract layer (RenderingDevice + RenderingSwapChain + RenderingDrawing*
    //      + RenderingStateBuffer + RenderingTextureAllocator + RenderingFont) — pure
    //      backend-agnostic contracts.
    //   2) DirectX11/* — the only concrete backend today, built on SharpDX 2.4.
    //
    // The split was added to leave room for a future backend swap (Vulkan via Silk.NET,
    // or D3D12 via Vortice). For that to actually work, every renderer-side caller must
    // go through this abstraction — see the migration TODOs in DeviceManager.cs.
    public abstract class RenderingDevice : IDisposable
    {
        public abstract void Dispose();

        // The caller owns the lifetime of every object created through these factories.
        // SwapChain is bound to a Win32 HWND; one per RenderingPanel.
        public abstract RenderingSwapChain CreateSwapChain(RenderingSwapChain.Description description);

        // Texture allocator = packed atlas of GPU textures with garbage collection.
        // Multiple allocators are typically created (one for room textures, one per font).
        public abstract RenderingTextureAllocator CreateTextureAllocator(RenderingTextureAllocator.Description description);

        // Per-frame uniform/constant block uploaded once and bound to every draw.
        public abstract RenderingStateBuffer CreateStateBuffer();

        // Smoke test draw (single triangle). Currently unused — kept as a sanity hook
        // for backend bring-up.
        public abstract RenderingDrawingTest CreateDrawingTest(RenderingDrawingTest.Description description);

        // Per-room baked geometry batch. Created lazily per Room and cached
        // (Panel3D._renderingCachedRooms); invalidated on geometry/lighting/texture edits.
        public abstract RenderingDrawingRoom CreateDrawingRoom(RenderingDrawingRoom.Description description);

        public abstract RenderingFont CreateFont(RenderingFont.Description description);

        // ===== Tappa 1 scaffold — see DeviceManager.cs for the migration plan =====
        // These factories are virtual (not abstract) so a backend can be ported
        // incrementally: the abstract layer ships the contract first, callers migrate
        // off the legacy path when their target shader becomes available.

        public virtual RenderingDrawingMesh CreateDrawingMesh(RenderingDrawingMesh.Description description)
            => throw new NotSupportedException("Backend " + GetType().Name + " has not implemented CreateDrawingMesh yet — see DeviceManager.cs migration plan.");

        public virtual RenderingDrawingLines CreateDrawingLines(RenderingDrawingLines.Description description)
            => throw new NotSupportedException("Backend " + GetType().Name + " has not implemented CreateDrawingLines yet — see DeviceManager.cs migration plan.");

        public virtual RenderingDrawingImportedGeometry CreateDrawingImportedGeometry(RenderingDrawingImportedGeometry.Description description)
            => throw new NotSupportedException("Backend " + GetType().Name + " has not implemented CreateDrawingImportedGeometry yet — see DeviceManager.cs migration plan.");
    }
}
