using System;
using NLog;
using TombLib.Rendering;

namespace TombLib.Graphics
{
    // Process-wide singleton owning the rendering device.
    //
    // Backend selection:
    //   - default: Dx11RenderingDevice (Silk.NET D3D11).
    //   - env TOMBEDITOR_RENDERER=vulkan: VulkanRenderingDevice (Silk.NET.Vulkan).
    //   - env TOMBEDITOR_RENDERER=opengl: OpenGLRenderingDevice (Silk.NET.OpenGL 4.1).
    public class DeviceManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static DeviceManager DefaultDeviceManager = new DeviceManager();

        public RenderingDevice Device;

        // Human-readable name of the active backend (e.g. "DirectX 11",
        // "Vulkan", "OpenGL 4.1"). Set during DeviceManager construction;
        // consumed by FormMain title bars in TombEditor / WadTool.
        public string BackendName { get; private set; } = "Unknown";

        // The raw ID3D11Device* as nint — non-zero only when the Dx11 backend
        // is active. Components that need direct D3D11 (WadRenderer,
        // ImportedGeometryTexture) must check for zero and skip / throw under
        // the Vulkan backend. Consumers cast to ID3D11Device* in unsafe code.
        public nint D3D11Device { get; }

        public unsafe DeviceManager()
        {
            string requested = Environment.GetEnvironmentVariable("TOMBEDITOR_RENDERER")?.ToLowerInvariant();
            
			if (requested == "opengl")
            {
                logger.Info("Backend: OpenGLRenderingDevice (forced via TOMBEDITOR_RENDERER).");
                var glDevice = new Rendering.OpenGL.OpenGLRenderingDevice();
                Device = glDevice;
                BackendName = "OpenGL 4.1";

                LevelData.ImportedGeometryTexture.GpuTextureFactory = img =>
                    new Rendering.OpenGL.OpenGLTexture2D(glDevice, img).Texture;
                return;
            }
            if (requested == "vulkan")
            {
                logger.Info("Backend: VulkanRenderingDevice (forced via TOMBEDITOR_RENDERER).");
                var vulkanDevice = new Rendering.Vulkan.VulkanRenderingDevice();
                Device = vulkanDevice;
                BackendName = "Vulkan";

                // ImportedGeometryTexture lazily uploads its ImageC into a per-texture
                // VulkanTexture2D on first GpuTexture access. The factory below is
                // the only TombLib → TombLib.Rendering bridge that side of the
                // dependency graph; without it, imported-geometry textures would
                // never reach the GPU under Vulkan.
                LevelData.ImportedGeometryTexture.GpuTextureFactory = img =>
                    new Rendering.Vulkan.VulkanTexture2D(vulkanDevice, img).View;
                return;
            }

            logger.Info("Backend: Dx11RenderingDevice (default, Silk.NET D3D11).");
            var dx11Dev = new Rendering.DirectX11.Dx11RenderingDevice();
            Device = dx11Dev;
            D3D11Device = (nint)dx11Dev.Device;
            LevelData.ImportedGeometry.Device = D3D11Device;
            BackendName = "DirectX 11";
        }

        // WadRenderer factory. Returns the backend-appropriate concrete:
        // Dx11WadRenderer (Silk.NET Texture2DArray) under DX11, VulkanWadRenderer
        // (VkImage Texture2DArray) under Vulkan. Callers use this so they don't
        // have to switch on the active backend themselves.
        public WadRenderer CreateWadRenderer(bool compactTexture, bool correctTexture, int atlasSize, int maxAllocationSize, bool loadAnimations)
        {
            if (Device is Rendering.OpenGL.OpenGLRenderingDevice gl)
                return new Rendering.OpenGL.OpenGLWadRenderer(gl, compactTexture, correctTexture, atlasSize, maxAllocationSize, loadAnimations);
            if (Device is Rendering.Vulkan.VulkanRenderingDevice vk)
                return new Rendering.Vulkan.VulkanWadRenderer(vk, compactTexture, correctTexture, atlasSize, maxAllocationSize, loadAnimations);
            if (D3D11Device != 0)
                return new Dx11WadRenderer(D3D11Device, compactTexture, correctTexture, atlasSize, maxAllocationSize, loadAnimations);
            throw new System.NotSupportedException("DeviceManager.CreateWadRenderer: no supported backend.");
        }
    }
}
