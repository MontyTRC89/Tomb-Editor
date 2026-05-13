using System;
using NLog;
using TombLib.Rendering;

namespace TombLib.Graphics
{
    // Process-wide singleton owning the rendering device.
    //
    // Backend selection:
    //   - default: VulkanRenderingDevice (Silk.NET.Vulkan direct backend).
    //   - env TOMBEDITOR_RENDERER=dx11: Dx11RenderingDevice (legacy SharpDX D3D11),
    //     useful as a fallback while the Vulkan path is being built up
    //     subsystem by subsystem.
    //
    // KNOWN STATE (develop_vulkan_direct): the Vulkan backend implements the
    // foundation only — instance/device/swapchain + Clear/Present cycle. Every
    // CreateXxx factory below `CreateSwapChain` throws NotSupportedException;
    // the editor will crash on first subsystem init under Vulkan until those
    // are ported. Use TOMBEDITOR_RENDERER=dx11 in the meantime.
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
            string requested = Environment.GetEnvironmentVariable("TOMBEDITOR_RENDERER")?.ToLowerInvariant();
            if (requested == "dx11" || requested == "directx11")
            {
                logger.Info("Backend: Dx11RenderingDevice (forced via TOMBEDITOR_RENDERER).");
                Device = new Rendering.DirectX11.Dx11RenderingDevice();
                D3D11Device = ((Rendering.DirectX11.Dx11RenderingDevice)Device).Device;
                LevelData.ImportedGeometry.Device = D3D11Device;
                return;
            }

            logger.Info("Backend: VulkanRenderingDevice (default, Silk.NET.Vulkan direct).");
            Device = new Rendering.Vulkan.VulkanRenderingDevice();
        }
    }
}
