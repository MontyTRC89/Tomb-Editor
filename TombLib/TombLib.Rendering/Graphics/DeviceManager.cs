using System;
using NLog;
using TombLib.Rendering;

namespace TombLib.Graphics
{
    // Process-wide singleton owning the rendering device.
    //
    // Backend selection:
    //   - default: Dx11RenderingDevice (SharpDX raw D3D11).
    //   - env TOMBEDITOR_RENDERER=vulkan-direct: VulkanRenderingDevice (Silk.NET.Vulkan
    //     direct backend). Foundation only at the moment — most Drawing*/Atlas
    //     subsystems will throw NotSupportedException until they are ported.
    //
    // The Vulkan backend lives on the develop_vulkan_direct branch and is being
    // built up subsystem by subsystem. The legacy Dx11 path stays the production
    // default until the Vulkan path is feature-complete.
    public class DeviceManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static DeviceManager DefaultDeviceManager = new DeviceManager();

        public RenderingDevice Device;

        // The raw ID3D11Device — non-null only when the Dx11 backend is active.
        // Components that need direct D3D11 (WadRenderer, ImportedGeometryTexture)
        // must null-check and skip / throw under the Vulkan backend.
        public SharpDX.Direct3D11.Device D3D11Device { get; }

        public DeviceManager()
        {
            string requested = Environment.GetEnvironmentVariable("TOMBEDITOR_RENDERER");
            if (string.Equals(requested, "vulkan-direct", StringComparison.OrdinalIgnoreCase))
            {
                logger.Info("Backend: VulkanRenderingDevice (direct Silk.NET.Vulkan) — forced via TOMBEDITOR_RENDERER.");
                Device = new Rendering.Vulkan.VulkanRenderingDevice();
                return;
            }

            Device = new Rendering.DirectX11.Dx11RenderingDevice();
            D3D11Device = ((Rendering.DirectX11.Dx11RenderingDevice)Device).Device;
            LevelData.ImportedGeometry.Device = D3D11Device;
        }
    }
}
