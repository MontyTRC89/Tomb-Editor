// Room geometry pass: world-space position + per-vertex tint, atlas UV
// and a separate "grid UV" (the raw VertexEditorUVs from RoomGeometry,
// values in {-1, 0, 1} at the four sector-face corners). The pixel shader
// uses fwidth on the grid UV to draw thin dividers at integer-axis
// crossings, matching the legacy outline behaviour.
//
// Bindings:
//   b0  ViewParams : float4x4 ViewProjection
//   t0  Atlas      : Texture2D (BGRA8 atlas: white pixel + arrows + level tex)
//   s0  AtlasSamp  : SamplerState
//
// Vertex layout (stride 40):
//   POSITION  : float3   (world space)
//   COLOR     : float3   (RGB tint, 0..1)
//   TEXCOORD0 : float2   (atlas UV, 0..1 normalized)
//   TEXCOORD1 : float2   (grid UV, raw VertexEditorUVs)

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer ViewParams : register(b0)
{
    float4x4 ViewProjection;
};

VK_BINDING(1, 0) Texture2D    Atlas     : register(t0);
VK_BINDING(2, 0) SamplerState AtlasSamp : register(s0);

struct VsIn
{
    VK_LOCATION(0) float3 PositionWS : POSITION;
    VK_LOCATION(1) float3 Color      : COLOR;
    VK_LOCATION(2) float2 Uv         : TEXCOORD0;
    VK_LOCATION(3) float2 GridUv     : TEXCOORD1;
};

struct VsOut
{
    float4 PositionCS : SV_Position;
    float3 Color      : COLOR;
    float2 Uv         : TEXCOORD0;
    float2 GridUv     : TEXCOORD1;
};

VsOut vs_main(VsIn input)
{
    VsOut o;
    o.PositionCS = mul(ViewProjection, float4(input.PositionWS, 1.0));
    o.Color      = input.Color;
    o.Uv         = input.Uv;
    o.GridUv     = input.GridUv;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 sampled = Atlas.Sample(AtlasSamp, input.Uv);

    // Sector outline: grid UV is in {-1, 0, 1} at corners, continuous
    // across the face. abs() collapses it to [0, 1]; the distance to the
    // nearest integer crossing (0 or 1) scaled by the screen-space
    // derivative gives a constant-width pixel line regardless of distance.
    float2 absUv  = abs(input.GridUv);
    float2 dUv    = max(fwidth(absUv), 0.00001);
    float2 dist   = min(absUv, 1.0 - absUv);
    float2 lineF  = saturate(dist / dUv - 0.5);
    float  edge   = min(lineF.x, lineF.y);     // 0 right on a grid line, 1 in the middle of a sector
    float  darken = lerp(0.45, 1.0, edge);     // 55% darker on the line

    float3 rgb = sampled.rgb * input.Color * darken;
    return float4(rgb, sampled.a);
}
