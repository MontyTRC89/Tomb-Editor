using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Rendering
{
    // Drawing batch for non-WAD imported 3D models (FBX, OBJ, COLLADA assets dropped
    // into the level via the Imported Geometry feature).
    //
    // Different from RenderingDrawingMesh in two key ways:
    //   1. Per-submesh texture: each Submesh carries its own Texture object instead
    //      of sampling from a shared atlas. The vertex layout has a 2D UV (no atlas
    //      page index in the W coordinate).
    //   2. UVs are in PIXEL space (not normalized 0..1). The vertex shader divides
    //      by the per-submesh ReciprocalTextureSize. This matches how the importer
    //      stores UVs from the source files.
    //
    // Used by Panel3D.DrawImportedGeometry. Preserved as a separate type so the
    // simpler RenderingDrawingMesh path stays focused on atlas-based rendering.
    public abstract class RenderingDrawingImportedGeometry : IDisposable
    {
        public class Submesh
        {
            public int IndexStart;
            public int IndexCount;
            public bool DoubleSided;
            public bool AdditiveBlending;

            // Per-submesh Texture2D (NOT Texture2DArray). Type expected by the D3D11
            // backend: a SharpDX.Direct3D11.ShaderResourceView, or a
            // SharpDX.Toolkit.Graphics.Texture2D (which implicitly converts to one).
            // Pass null for an untextured submesh; the shader falls back to the
            // per-vertex Color.
            public object Texture;
            public Vector2 TextureSize; // pixels — used to normalize UVs in the VS
        }

        public class Description
        {
            public IList<Vertex> Vertices;
            public IList<int> Indices;
            public IList<Submesh> Submeshes;
            public bool HintOneTime;
        }

        // Vertex format expected by ImportedGeometryShader. MUST match the IL declared
        // in Dx11RenderingDevice.ImportedGeometryShader and the legacy
        // TombLib.LevelData.ImportedGeometryVertex layout (Position@0, UV@12, Color@20,
        // Normal@32 = 44B). Nested inside the class to avoid a name collision with
        // TombLib.LevelData.ImportedGeometryVertex.
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
        public struct Vertex
        {
            public Vector3 Position;
            public Vector2 UV;
            public Vector3 Color;
            public Vector3 Normal;
        }

        public class RenderArgs
        {
            public RenderingSwapChain RenderTarget;
            public RenderingStateBuffer StateBuffer;
            public Matrix4x4 World = Matrix4x4.Identity;
            public Vector4 Tint = Vector4.One;
            public bool UseVertexColors;
            public bool AlphaTest;
            public bool BilinearFilter = true;
            // When true, the renderer overrides the per-submesh blend with Additive —
            // matches the legacy behaviour of "DisablePickingForImportedGeometry"
            // which forces additive translucency on every submesh.
            public bool ForceAdditive;
        }

        public abstract void Dispose();
        public abstract void Render(RenderArgs arg);
    }

}
