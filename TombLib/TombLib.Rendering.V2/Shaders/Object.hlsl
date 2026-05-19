// Object pass: WadStatics, WadMoveables (default pose) and ImportedGeometry.
//
// Vertex layout (stride 16):
//   POSITION : float3   (model-local space)
//   COLOR    : R8G8B8A8 (vertex tint, .a unused)
//
// Bindings:
//   b0  ViewParams      : ViewProjection
//   b4  PushConstants   : ModelMatrix (4×4) + Tint (float4)
//
// Untextured for first iteration — texturing for objects (per-poly atlas
// UV from WadTexture / ImportedGeometryTexture references) comes next.

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer ViewParams : register(b0)
{
    float4x4 ViewProjection;
    float    GridLineWidth;
    float    GridEnabled;
    float    _pad1, _pad2;
};

// Push constants are emulated as a regular cbuffer at b4 on DX11/GL — the
// RHI Dx11CommandList.PushConstants writes to the dynamic CB bound at this
// slot. On Vulkan we'll switch to a native [[vk::push_constant]] struct in
// a follow-up.
VK_BINDING(4, 0)
cbuffer PushConstants : register(b4)
{
    float4x4 ModelMatrix;
    float4   Tint;
};

struct VsIn
{
    VK_LOCATION(0) float3 PositionMS : POSITION;
    VK_LOCATION(1) float4 Color      : COLOR;
};

struct VsOut
{
    float4 PositionCS : SV_Position;
    float4 Color      : COLOR;
};

VsOut vs_main(VsIn input)
{
    VsOut o;
    float4 worldPos  = mul(ModelMatrix, float4(input.PositionMS, 1.0));
    o.PositionCS = mul(ViewProjection, worldPos);
    o.Color      = input.Color * Tint;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    return input.Color;
}
