// Service-object billboard pass — textured camera-facing quads for lights,
// cameras, sinks, sound sources, etc. The CPU emits 4 vertices per icon, all
// sharing the world-space object centre; per-vertex Corner.xy carries the
// world-space offset along the camera right / up basis (so the quad faces
// the camera) and Uv samples the icon out of the icon atlas.

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer ViewParams : register(b0)
{
    float4x4 ViewProjection;
    float4   CamRightWS;   // xyz used, w padding
    float4   CamUpWS;
    float4   _pad;
};

VK_BINDING(1, 0) Texture2D    Atlas     : register(t0);
VK_BINDING(2, 0) SamplerState AtlasSamp : register(s0);

struct VsIn
{
    VK_LOCATION(0) float3 CenterWS : POSITION;
    VK_LOCATION(1) float2 Corner   : TEXCOORD0; // world-space offset coefficients along right/up
    VK_LOCATION(2) float2 Uv       : TEXCOORD1;
    VK_LOCATION(3) float4 Tint     : COLOR;
};

struct VsOut
{
    float4 PositionCS : SV_Position;
    float2 Uv         : TEXCOORD0;
    float4 Tint       : COLOR;
};

VsOut vs_main(VsIn input)
{
    VsOut o;
    float3 worldPos = input.CenterWS
                    + CamRightWS.xyz * input.Corner.x
                    + CamUpWS.xyz    * input.Corner.y;
    o.PositionCS = mul(ViewProjection, float4(worldPos, 1.0));
    o.Uv         = input.Uv;
    o.Tint       = input.Tint;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 t = Atlas.Sample(AtlasSamp, input.Uv);
    return float4(t.rgb * input.Tint.rgb, t.a * input.Tint.a);
}
