using System;
using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace TombLib.Rendering.Vulkan
{
    // Small helper to convert a non-Success VkResult into a thrown exception with
    // call-site info. Every Vulkan call routes through this so we never miss a
    // failure silently (Silk.NET returns the VkResult code as the function value,
    // it does not throw on its own).
    internal static class VkCheck
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ok(Result result, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            if (result != Result.Success)
                throw new InvalidOperationException($"Vulkan call failed: {result} at {member}:{line}");
        }
    }
}
