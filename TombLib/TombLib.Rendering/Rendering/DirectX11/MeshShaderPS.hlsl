// Pixel shader for RenderingDrawingMesh — samples the bound atlas Texture2DArray
// using the interpolated UVW (W = atlas page), modulates by per-vertex color, then
// applies optional alpha test (matches legacy Model.fx).

// Mirrors Dx11RenderingStateBuffer.ConstantBufferLayout. The brush fields are read
// by the included BrushOverlay.hlsli helper.
cbuffer FrameData : register(b0)
{
    matrix TransformMatrix;
    float RoomGridLineWidth;
    int RoomGridForce;
    int RoomDisableVertexColors;
    int ShowExtraBlendingModes;
    int ShowLightingWhiteTextureOnly;
    int LightMode;
    int BrushShape;
    float BrushRotation;
    float4 BrushCenter;
    float4 BrushColor;
};

cbuffer MeshData : register(b1)
{
    matrix World;
    float4 Tint;
    matrix Bones[32];
    int Skinned;
    int StaticLighting;
    int ColoredVertices;
    int AlphaTest;
};

Texture2DArray Atlas : register(t0);
SamplerState AtlasSampler : register(s0);

struct PixelInputType
{
    float4 Position      : SV_POSITION;
    float3 UVW           : TEXCOORD;
    float4 Color         : COLOR;
    float3 WorldPosition : WORLDPOSITION;
};

#include "../Legacy/BrushOverlay.hlsli"

float4 main(PixelInputType input) : SV_TARGET
{
    float4 texel = Atlas.Sample(AtlasSampler, input.UVW);

    // Same color combination as Model.fx: per-vertex Color may be > 1.0 (over-bright
    // baked lighting). Clamp the multiplier to 1.0 and add the overbright as an
    // additive offset scaled by 0.37 (matches legacy Model.fx and RoomShaderPS).
    float3 colorAdd = max(input.Color.rgb - 1.0f, 0.0f) * 0.37f;
    float3 colorMul = min(input.Color.rgb, 1.0f);
    texel.rgb = texel.rgb * colorMul + colorAdd;
    texel.a *= input.Color.a;

    if (AlphaTest != 0 && texel.a <= 0.01f)
        discard;

    // Object-placement brush overlay (active in EditorMode.ObjectPlacement). When
    // BrushShape == 0 the helper early-returns so non-brush modes pay nothing.
    ApplyBrushOverlay(texel.rgb, texel.a, false, input.Position, input.WorldPosition, RoomGridLineWidth);

    return texel;
}
