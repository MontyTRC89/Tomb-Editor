// TR1 / TR2 sprite-instance pass — screen-space textured quads. The CPU
// projects each SpriteInstance's world position to viewport pixels, offsets
// by the WadSprite's pixel alignment to form a quad, and the shader samples
// the level atlas at the precomputed UVs.
//
// Sprite texels are stored in standard RGBA; unlike the text pass we sample
// all four channels so the actual sprite colours come through.

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer SpriteParams : register(b0)
{
    float4 InvViewport;  // xy = 2 / viewport size (px), zw = padding
    float4 _pad0;
    float4 _pad1;
    float4 _pad2;
};

VK_BINDING(1, 0) Texture2D    Atlas     : register(t0);
VK_BINDING(2, 0) SamplerState AtlasSamp : register(s0);

struct VsIn
{
    VK_LOCATION(0) float2 PositionPx : POSITION;
    VK_LOCATION(1) float2 Uv         : TEXCOORD0;
    VK_LOCATION(2) float4 Tint       : COLOR;
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
    // Pixel -> normalized device coords. Y is flipped here so pixel (0,0) is
    // the top-left corner; the Vulkan backend additionally negates Y in the
    // VS (build flag -fvk-invert-y), exactly like every other V2 shader.
    float2 ndc = float2(input.PositionPx.x * InvViewport.x - 1.0,
                        1.0 - input.PositionPx.y * InvViewport.y);
    o.PositionCS = float4(ndc, 0.0, 1.0);
    o.Uv         = input.Uv;
    o.Tint       = input.Tint;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 tex = Atlas.Sample(AtlasSamp, input.Uv);
    return tex * input.Tint;
}
