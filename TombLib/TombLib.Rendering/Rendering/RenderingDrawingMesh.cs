using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Rendering
{
    // Pre-baked GPU geometry for one moveable / static / imported model.
    //
    // STATUS: Tappa 1 scaffold — the abstraction is defined and a D3D11 implementation
    // exists, but no caller is wired up yet. Today moveables and statics are still
    // drawn through the legacy SharpDX.Toolkit path (DeviceManager.___LegacyEffects
    // ["Model"]). The plan is to migrate Panel3DDraw.DrawMoveables / DrawStatics /
    // DrawImportedGeometry and every WadTool PanelRendering* one at a time onto this
    // abstraction. See DeviceManager.cs for the migration roadmap.
    //
    // Compared to RenderingDrawingRoom this layer is intentionally simpler:
    //   - No per-triangle face metadata (sectors are room-only).
    //   - Per-mesh world transform passed at Render() time (rooms bake worldPos in).
    //   - Optional skinning matrices in the StateBuffer (cbuffer slot 1 reserved).
    public abstract class RenderingDrawingMesh : IDisposable
    {
        // Per-submesh material attributes. Index range into the mesh's index buffer
        // identifies which triangles of the mesh this submesh owns.
        public class Submesh
        {
            public int IndexStart;
            public int IndexCount;
            public bool DoubleSided;
            public bool AdditiveBlending;  // Additive (One/One) blending instead of opaque
        }

        public class Description
        {
            // Source vertex/index data. The implementation copies into IMMUTABLE GPU
            // buffers; the source lists can be released after CreateDrawingMesh returns.
            public IList<MeshVertex> Vertices;
            public IList<int> Indices;
            public IList<Submesh> Submeshes;

            // Hint for one-shot meshes (thumbnails, previews) — the implementation may
            // skip optimizations whose setup cost outweighs the win for a single frame.
            public bool HintOneTime;
        }

        public class RenderArgs
        {
            public RenderingSwapChain RenderTarget;
            public RenderingStateBuffer StateBuffer;

            // Atlas the model's textures live in. The renderer binds it to slot t0 as
            // a Texture2DArray (the W coordinate of MeshVertex.UVW selects the slice).
            // Type expected by the D3D11 backend: a SharpDX.Direct3D11.ShaderResourceView
            // (caller can extract one from a SharpDX.Toolkit Texture2D via .GetShaderResourceView).
            // Typed as `object` because the abstraction layer doesn't depend on SharpDX —
            // a future backend would interpret this differently. Caller is responsible
            // for keeping the atlas alive for the duration of the Render() call.
            public object Atlas;

            // Per-instance world transform applied before the view-projection.
            public Matrix4x4 World = Matrix4x4.Identity;

            // Skinning matrices (model-space). Up to 32 bones. Set to null AND leave
            // Skinned=false for static meshes.
            public Matrix4x4[] BoneMatrices;

            // True to enable the GPU-side bone blend path. When false, BoneMatrices is
            // ignored and vertex positions go straight through.
            public bool Skinned;

            // Tint applied to (or replacing) per-vertex Color. Used by selection
            // highlighting. Default white = no change.
            public Vector4 Tint = Vector4.One;

            // Mirrors the legacy Model.fx flags:
            //   StaticLighting  : multiply Tint by per-vertex Color (baked lighting).
            //   ColoredVertices : keep RGB; otherwise convert to luma (legacy WAD style).
            public bool StaticLighting = true;
            public bool ColoredVertices = true;

            // Discard alpha < 0.01 fragments. Required for cutout textures.
            public bool AlphaTest;

            // Anisotropic filtering vs. point-sampling. False reproduces the editor's
            // "pixelated" preview mode.
            public bool BilinearFilter = true;

            // Disables both depth test and depth write for this draw. Set to
            // true for the skybox (so it doesn't occlude subsequent geometry
            // even if ClearDepth misbehaves) and for "always-on-top" overlays.
            public bool NoDepth;
        }

        public abstract void Dispose();
        public abstract void Render(RenderArgs arg);
    }


    // Vertex format expected by the mesh shader. Layout MUST match the IL declared
    // in Dx11RenderingDevice.MeshShader (POSITION/TEXCOORD/NORMAL/COLOR/BLENDINDICES/
    // BLENDWEIGHTS, all at offsets 0/12/24/36/48/64 = 80 bytes total) AND mirror the
    // legacy ObjectVertex so existing CPU-side mesh data can be uploaded without
    // conversion.
    //
    // UVW.xy = atlas UV (0..1), UVW.z = atlas page index as float (cast to int in
    // shader and used as the W coordinate of the Texture2DArray sample).
    //
    // Skinning fields use float4s (one float per influence) for simplicity. Set both
    // BoneIndex and BoneWeight to (0,0,0,0) for static meshes — the shader skips the
    // skinning math when Skinned=0 in MeshData regardless.
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct MeshVertex
    {
        public Vector3 Position;
        public Vector3 UVW;
        public Vector3 Normal;
        public Vector3 Color;
        public Vector4 BoneIndex;
        public Vector4 BoneWeight;
    }
}
