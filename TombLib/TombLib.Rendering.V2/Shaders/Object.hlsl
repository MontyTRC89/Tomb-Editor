// Object pass: WadStatics, WadMoveables (default pose) and ImportedGeometry.
//
// Vertex layout (stride 20):
//   POSITION : float3           (model-local space)
//   COLOR    : R8G8B8A8         (vertex tint, .a unused)
//   TEXCOORD : R16G16_UNorm     (atlas UV)
//
// Bindings:
//   b0  ViewParams      : ViewProjection + grid settings (unused here)
//   t0  Atlas           : Texture2D (shared with the room pass)
//   s0  AtlasSamp       : SamplerState (anisotropic 4x + mips)
//   b4  PushConstants   : ModelMatrix (4×4) + Tint (float4)

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer ViewParams : register(b0)
{
    float4x4 ViewProjection;
    float    GridLineWidth;
    float    GridEnabled;
    float    _pad1, _pad2;
};

VK_BINDING(1, 0) Texture2D    Atlas     : register(t0);
VK_BINDING(2, 0) SamplerState AtlasSamp : register(s0);

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
    VK_LOCATION(2) float2 Uv         : TEXCOORD0;
};

struct VsOut
{
    float4 PositionCS : SV_Position;
    float4 Color      : COLOR;
    float2 Uv         : TEXCOORD0;
};

VsOut vs_main(VsIn input)
{
    VsOut o;
    float4 worldPos = mul(ModelMatrix, float4(input.PositionMS, 1.0));
    o.PositionCS = mul(ViewProjection, worldPos);
    o.Color      = input.Color * Tint;
    o.Uv         = input.Uv;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 sampled = Atlas.Sample(AtlasSamp, input.Uv);
    return float4(sampled.rgb * input.Color.rgb, sampled.a * input.Color.a);
}
