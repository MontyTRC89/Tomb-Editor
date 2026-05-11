// Pixel shader for imported geometry. Samples the per-submesh texture (Texture2D,
// not array) when TextureEnabled, modulates by per-vertex Color, applies optional
// alpha test, and overlays the object-placement brush.

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

cbuffer ImportedGeometryData : register(b1)
{
    matrix World;
    float4 Tint;
    float2 ReciprocalTextureSize;
    int TextureEnabled;
    int UseVertexColors;
    int AlphaTest;
    int _pad0; int _pad1; int _pad2;
};

Texture2D PerSubmeshTexture : register(t0);
SamplerState TextureSampler : register(s0);

struct PixelInputType
{
    float4 Position      : SV_POSITION;
    float2 UV            : TEXCOORD;
    float4 Color         : COLOR;
    float3 WorldPosition : WORLDPOSITION;
};

#include "../Legacy/BrushOverlay.hlsli"

float4 main(PixelInputType input) : SV_TARGET
{
    // When no texture is bound, render flat per-vertex color (the legacy fallback
    // returned (UV.x, UV.y, UV.x, 1) which is debug; we keep flat color which is
    // what the user expects from imported meshes lacking a material texture).
    float4 pixel = TextureEnabled != 0
        ? PerSubmeshTexture.Sample(TextureSampler, input.UV)
        : float4(1, 1, 1, 1);

    float3 colorAdd = max(input.Color.rgb - 1.0f, 0.0f) * 0.37f;
    float3 colorMul = min(input.Color.rgb, 1.0f);
    pixel.rgb = pixel.rgb * colorMul + colorAdd;
    pixel.a *= input.Color.a;

    if (AlphaTest != 0 && pixel.a <= 0.05f)
        discard;

    ApplyBrushOverlay(pixel.rgb, pixel.a, false, input.Position, input.WorldPosition, RoomGridLineWidth);

    return pixel;
}
