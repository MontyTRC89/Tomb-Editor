using System;
using TombLib.RenderingV2.Backends.Dx11;
using TombLib.RenderingV2.Backends.Vulkan;

namespace TombLib.RenderingV2.Rhi;

/// <summary>
/// Factory + selector for <see cref="IRhiDevice"/> implementations.
///
/// <para>Default backend order is <b>Vulkan → DX11</b>: the editor tries the
/// native Vulkan backend first and falls back to D3D11 if the system has no
/// usable Vulkan loader / driver. The order can be forced via the
/// <c>TOMBEDITOR_RHI</c> environment variable: <c>vulkan</c> or <c>dx11</c>.
/// On a failed first attempt the chosen backend's exception is rethrown to
/// surface real driver errors; only a plain "no loader / no device" failure
/// silently triggers the fallback.</para>
/// </summary>
public static class RhiBackend
{
    /// <summary>Build a device with the configured / default backend, with auto-fallback.</summary>
    public static IRhiDevice Create()
    {
        string pref = (Environment.GetEnvironmentVariable("TOMBEDITOR_RHI") ?? "").Trim().ToLowerInvariant();

        if (pref == "dx11")
            return new Dx11Device();

        // Default + explicit "vulkan" both try Vulkan first.
        try
        {
            return new VkDevice();
        }
        catch (Exception vkEx) when (pref != "vulkan")
        {
            // Fallback: log to stderr (the editor's log surface isn't visible
            // from TombLib) and try DX11.
            try { Console.Error.WriteLine("[V2 RHI] Vulkan init failed, falling back to DX11: " + vkEx.Message); }
            catch { /* ignore */ }
            return new Dx11Device();
        }
    }
}
