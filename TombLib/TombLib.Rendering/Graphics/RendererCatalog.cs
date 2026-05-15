using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TombLib.Graphics
{
    // Single source of truth for which renderer backends are available on the
    // current platform, what their canonical IDs are (as stored in the
    // configuration / TOMBEDITOR_GRAPHIC_API env var), and how to display
    // them in the UI.
    //
    // Windows: DirectX 11, Vulkan, OpenGL 4.1.
    // Linux / macOS: Vulkan, OpenGL 4.1 (no DX11).
    public static class RendererCatalog
    {
        public readonly struct Entry
        {
            public readonly string Id;          // canonical id stored in config and env var
            public readonly string DisplayName; // human-readable name for the UI
            public Entry(string id, string displayName) { Id = id; DisplayName = displayName; }
            public override string ToString() => DisplayName;
        }

        public const string DefaultId = "vulkan";

        public static IReadOnlyList<Entry> Available { get; } = BuildList();

        private static IReadOnlyList<Entry> BuildList()
        {
            var list = new List<Entry>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                list.Add(new Entry("dx11", "DirectX 11"));
            list.Add(new Entry("vulkan", "Vulkan"));
            list.Add(new Entry("opengl", "OpenGL 4.1"));
            return list;
        }

        // Resolves an id (case-insensitive, falls back to DefaultId if unknown
        // or not available on this platform) to its catalog Entry.
        public static Entry Resolve(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                string norm = id.ToLowerInvariant();
                if (norm == "directx11") norm = "dx11";
                foreach (var e in Available)
                    if (e.Id == norm)
                        return e;
            }
            foreach (var e in Available)
                if (e.Id == DefaultId)
                    return e;
            return Available[0];
        }
    }
}
