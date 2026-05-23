using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Rendering;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        // The base RenderingPanel signature still expects a legacy
        // RenderingDevice / antialias / quality triple, but the V2 renderer
        // owns its own swapchain (Silk.NET) and ignores every parameter.
        public override void InitializeRendering(RenderingDevice device, bool antialias, ObjectRenderingQuality objectQuality)
        {
            _v2Renderer = new TombEditor.Rendering.V2.LevelRenderer(
                Handle, ClientSize.Width, ClientSize.Height,
                _editor.Configuration.Rendering3D_Backend);

            // The gizmo math (picking + drag) lives in BaseGizmo. The headless
            // ctor skips every legacy GraphicsDevice / Effect allocation —
            // crucial on Vulkan, where Intel UHD's WSI refuses to coexist
            // with DXGI in the same process.
            _gizmo = new Gizmo();

            ResetCamera(true);
        }
    }
}
