using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Rendering
{
    // Dynamic solid-color geometry batch — supports both LineList (gizmi, bounding
    // boxes, sector-split highlights, flyby paths) and TriangleList (ghost block bodies,
    // volume fills, anything that was a hand-built SolidVertex buffer in the legacy
    // path). The class is named "Lines" for historical reasons but is genuinely a
    // generic solid-color batch — the topology is selected at Render() time.
    //
    // Per-batch state options (BlendMode + DepthMode) cover every combination the
    // legacy callers needed: opaque depth-tested geometry, premultiplied-alpha
    // overlays, depth-read translucent fills (ghost block bodies), additive glow.
    public abstract class RenderingDrawingLines : IDisposable
    {
        public enum Topology
        {
            LineList,
            TriangleList,
        }

        public class Description
        {
            // Hint to the implementation: if the batch is rebuilt frame-by-frame, a
            // dynamic ring buffer is used. If it's mostly stable, the implementation
            // may upload to an immutable buffer the first time and skip re-uploads.
            public bool Dynamic = true;
        }

        public class RenderArgs
        {
            public RenderingSwapChain RenderTarget;
            public RenderingStateBuffer StateBuffer;
            public Matrix4x4 World = Matrix4x4.Identity;

            // Multiplied with each vertex's Color in the shader. Lets the caller reuse
            // a single uploaded VB and tint it differently per draw — analog of the
            // legacy `Color` parameter on Solid.fx. Default is white (no tint).
            public Vector4 Tint = Vector4.One;

            public Topology Topology = Topology.LineList;

            // Wireframe disables back-face culling AND switches the rasterizer to
            // wireframe fill — equivalent to the legacy `_rasterizerWireframe` state.
            public bool Wireframe;

            // Default = PremultipliedAlpha (the device-wide default state). Set to
            // NonPremultipliedAlpha for translucent overlays whose vertex colors are
            // straight (alpha not pre-multiplied into RGB). Additive for glow effects.
            public BlendMode Blend = BlendMode.PremultipliedAlpha;

            // Default = Default (depth test + write). Set to DepthRead for translucent
            // overlays that should be occluded but should NOT write to depth (e.g.
            // ghost block bodies). NoZ disables depth entirely.
            public DepthMode Depth = DepthMode.Default;
        }

        public abstract void Dispose();

        // Replaces the batch's current vertex set. Cheap when Description.Dynamic=true.
        public abstract void SetVertices(ReadOnlySpan<SolidLineVertex> vertices);

        public abstract void Render(RenderArgs arg);
    }

    public enum BlendMode
    {
        Opaque,                 // SrcRGB,             ignored,           opaque output
        PremultipliedAlpha,     // SrcRGB + Dest*(1-SrcA),                premultiplied input
        NonPremultipliedAlpha,  // SrcRGB*SrcA + Dest*(1-SrcA),           straight alpha input
        Additive,               // SrcRGB + Dest,                          glow / overlay
    }

    public enum DepthMode
    {
        Default,    // depth test + write enabled
        DepthRead,  // depth test enabled, write disabled (translucent passes)
        NoZ,        // depth test + write disabled (always draws on top)
    }

    // Vertex layout used by the lines/triangles solid shader. Matches Solid.fx for
    // continuity, so the legacy SolidVertex buffers can be ported without recomputing
    // anything.
    public struct SolidLineVertex
    {
        public Vector3 Position;
        public Vector4 Color;
    }
}
