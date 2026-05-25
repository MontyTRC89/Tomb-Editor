// Gizmo overlay pass — flat colour, no texture. Vertex positions are
// already in WORLD space (the CPU bakes Position + rotation + scale into the
// vertex stream every frame — gizmo geometry is tiny, ~few hundred verts).

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer ViewParams : register(b0)
{
    float4x4 ViewProjection;
    float4   _pad0;
    float4   _pad1;
    float4   _pad2;
    float4   _pad3;
};

struct VsIn
{
    VK_LOCATION(0) float3 PositionWS : POSITION;
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
    o.PositionCS = mul(ViewProjection, float4(input.PositionWS, 1.0));
    o.Color      = input.Color;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    return input.Color;
}
