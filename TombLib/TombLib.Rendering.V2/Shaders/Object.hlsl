// Instanced object pass. Row-vector convention (legacy compatible).
//
// Slot 0 (per-vertex, stride 20):
//   POSITION  : float3       (model-local space)
//   COLOR     : R8G8B8A8     (vertex tint)
//   TEXCOORD0 : R16G16_UNorm (atlas UV)
//
// Slot 1 (per-instance, stride 80):
//   TEXCOORD1..4 : float4 each (rows of the model matrix, in .NET row-major order)
//   TEXCOORD5    : float4       (RGBA tint)
//
// Matrices are declared row_major so the CPU's System.Numerics.Matrix4x4
// rows map 1:1 to HLSL's logical rows. The math chain is then the same as
// the legacy Model.fx:   clipPos = v × ModelMatrix × ViewProjection
// expressed in HLSL as   mul(mul(v, ModelMatrix), ViewProjection).

#pragma pack_matrix(row_major)

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

struct VsIn
{
    VK_LOCATION(0) float3 PositionMS  : POSITION;
    VK_LOCATION(1) float4 Color       : COLOR;
    VK_LOCATION(2) float2 Uv          : TEXCOORD0;
    VK_LOCATION(3) float4 InstMat0    : TEXCOORD1;
    VK_LOCATION(4) float4 InstMat1    : TEXCOORD2;
    VK_LOCATION(5) float4 InstMat2    : TEXCOORD3;
    VK_LOCATION(6) float4 InstMat3    : TEXCOORD4;
    VK_LOCATION(7) float4 InstTint    : TEXCOORD5;
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
    float4x4 model = float4x4(input.InstMat0, input.InstMat1, input.InstMat2, input.InstMat3);
    float4 v = float4(input.PositionMS, 1.0);
    float4 world = mul(v,    model);
    o.PositionCS = mul(world, ViewProjection);
    o.Color      = input.Color * input.InstTint;
    o.Uv         = input.Uv;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 sampled = Atlas.Sample(AtlasSamp, input.Uv);
    return float4(sampled.rgb * input.Color.rgb, sampled.a * input.Color.a);
}
