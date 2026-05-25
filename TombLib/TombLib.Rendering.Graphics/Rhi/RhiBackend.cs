using System;
using TombLib.Rendering.Graphics.Backends.Dx11;
using TombLib.Rendering.Graphics.Backends.OpenGL;
using TombLib.Rendering.Graphics.Backends.Vulkan;

namespace TombLib.Rendering.Graphics.Rhi;

/// <summary>
/// Factory + selector for <see cref="IRhiDevice"/> implementations.
///
/// <para>Default backend order is <b>Vulkan → OpenGL → DX11</b>: the editor
/// tries native Vulkan first, falls back to OpenGL 4.3 if Vulkan init fails
/// (no loader / no compatible driver), and finally to D3D11 if OpenGL also
/// fails. The choice can be forced via the <c>TOMBEDITOR_RHI</c> environment
/// variable: <c>vulkan</c> (or <c>vk</c>), <c>opengl</c> (or <c>gl</c>),
/// <c>dx11</c>. A forced choice disables the fallback chain so real driver
/// errors surface instead of being silently swallowed.</para>
/// </summary>
public static class RhiBackend
{
    /// <summary>Build a device with the configured / default backend.</summary>
    /// <param name="preference">Optional override from the editor's Configuration
    /// (e.g. "Vulkan", "OpenGL", "DX11", or "Default" / empty for the cascade).
    /// The TOMBEDITOR_RHI environment variable always wins over this when set.</param>
    public static IRhiDevice Create(string? preference = null)
    {
        string pref = (Environment.GetEnvironmentVariable("TOMBEDITOR_RHI") ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(pref) && !string.IsNullOrEmpty(preference))
            pref = preference.Trim().ToLowerInvariant();
        if (pref == "default") pref = ""; // explicit "Default" = auto cascade

        if (pref == "dx11")
            return new Dx11Device();
        if (pref == "vulkan" || pref == "vk")
            return new VkDevice();
        if (pref == "opengl" || pref == "gl")
            return new GLDevice();

        // Default cascade: Vulkan → OpenGL → DX11.
        try
        {
            return new VkDevice();
        }
        catch (Exception vkEx)
        {
            LogFallback("Vulkan", vkEx);
            try
            {
                return new GLDevice();
            }
            catch (Exception glEx)
            {
                LogFallback("OpenGL", glEx);
                return new Dx11Device();
            }
        }
    }

    private static void LogFallback(string backendName, Exception ex)
    {
        try { Console.Error.WriteLine($"[V2 RHI] {backendName} init failed, falling back: {ex.Message}"); }
        catch { /* ignore — logging should never crash startup */ }
    }
}
