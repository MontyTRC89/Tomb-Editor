// Instanced object pass.
//
// Slot 0 (per-vertex, stride 20):
//   POSITION  : float3       (model-local space)
//   COLOR     : R8G8B8A8     (vertex tint)
//   TEXCOORD0 : R16G16_UNorm (atlas UV)
//
// Slot 1 (per-instance, stride 80):
//   TEXCOORD1..4 : float4 each (rows of the transposed model matrix)
//   TEXCOORD5    : float4       (RGBA tint)
//
// We upload the transposed model matrix from the CPU so that constructing
// it as `float4x4(row0, row1, row2, row3)` in HLSL and using mul(M, v)
// applies the same row-vector convention as the legacy renderer (and
// matches the cbuffer-uploaded ViewProjection above).

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
    float4 worldPos = mul(model, float4(input.PositionMS, 1.0));
    o.PositionCS = mul(ViewProjection, worldPos);

    // InstTint.a is a 0..1 *replace* factor — same trick the legacy Model.fx
    // uses, where selecting a mesh sets output.Color = Color (full replace
    // of the per-vertex contribution). With a=0 we keep the vertex-colour ×
    // tint multiply (lighting / per-instance tint), with a=1 we throw the
    // vertex colour away so the selection red shows through even on dark
    // meshes that would otherwise multiply to near-black.
    float3 mul3   = input.Color.rgb * input.InstTint.rgb;
    o.Color.rgb   = lerp(mul3, input.InstTint.rgb, input.InstTint.a);
    o.Color.a     = 1.0;
    o.Uv          = input.Uv;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 sampled = Atlas.Sample(AtlasSamp, input.Uv);
    return float4(sampled.rgb * input.Color.rgb, sampled.a);
}
