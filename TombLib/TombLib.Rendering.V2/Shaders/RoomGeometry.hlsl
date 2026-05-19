// Room geometry pass: world-space position + per-vertex tint (lighting) +
// atlas UV. Sampled atlas color is modulated by the vertex tint, so
// untextured triangles fall back to the lit vertex color (the atlas
// returns a reserved white pixel for unknown / null textures).
//
// Bindings:
//   b0  ViewParams : float4x4 ViewProjection
//   t0  Atlas      : Texture2D (BGRA8 single-page atlas built by TextureAtlas)
//   s0  AtlasSamp  : SamplerState
//
// Vertex layout (stride 32):
//   POSITION : float3   (world space)
//   COLOR    : float3   (RGB tint, 0..1)
//   TEXCOORD : float2   (atlas UV, 0..1 normalized)

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
};

struct VsOut
{
    float4 PositionCS : SV_Position;
    float3 Color      : COLOR;
    float2 Uv         : TEXCOORD0;
};

VsOut vs_main(VsIn input)
{
    VsOut o;
    o.PositionCS = mul(ViewProjection, float4(input.PositionWS, 1.0));
    o.Color      = input.Color;
    o.Uv         = input.Uv;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 sampled = Atlas.Sample(AtlasSamp, input.Uv);
    return float4(sampled.rgb * input.Color, sampled.a);
}
