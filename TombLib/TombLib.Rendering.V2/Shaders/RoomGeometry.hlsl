// Minimal room geometry pass: world-space vertex position + per-vertex
// color, transformed by a per-frame view-projection matrix.
//
// Bindings:
//   b0  ViewParams : float4x4 ViewProjection
//
// Vertex layout (stride 24):
//   POSITION : float3   (world space, room offset baked at upload)
//   COLOR    : float3   (RGB, 0..1 floats)
//
// Texturing, sector overlays, highlights, blend modes and other features
// from the legacy RoomShader will be added in follow-up passes.

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer ViewParams : register(b0)
{
    float4x4 ViewProjection;
};

struct VsIn
{
    VK_LOCATION(0) float3 PositionWS : POSITION;
    VK_LOCATION(1) float3 Color      : COLOR;
};

struct VsOut
{
    float4 PositionCS : SV_Position;
    float3 Color      : COLOR;
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
    return float4(input.Color, 1.0);
}
