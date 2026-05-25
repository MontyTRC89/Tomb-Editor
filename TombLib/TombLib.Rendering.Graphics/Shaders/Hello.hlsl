// Minimal end-to-end test shader for the HLSL -> DXBC / SPIR-V / GLSL pipeline.
// Renders a textured triangle with a per-frame MVP matrix and a tint color.
//
// Binding map (kept identical across all backends):
//   b0  ViewParams  : float4x4 Mvp; float4 Tint;
//   t0  ColorTex    : Texture2D
//   s0  ColorSamp   : SamplerState

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer ViewParams : register(b0)
{
    float4x4 Mvp;
    float4   Tint;
};

VK_BINDING(1, 0) Texture2D    ColorTex  : register(t0);
VK_BINDING(2, 0) SamplerState ColorSamp : register(s0);

struct VsIn
{
    VK_LOCATION(0) float3 Position : POSITION;
    VK_LOCATION(1) float2 Uv       : TEXCOORD0;
};

struct VsOut
{
    float4 PositionCS : SV_Position;
    float2 Uv         : TEXCOORD0;
};

VsOut vs_main(VsIn input)
{
    VsOut o;
    o.PositionCS = mul(Mvp, float4(input.Position, 1.0));
    o.Uv         = input.Uv;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 sampled = ColorTex.Sample(ColorSamp, input.Uv);
    return sampled * Tint;
}
