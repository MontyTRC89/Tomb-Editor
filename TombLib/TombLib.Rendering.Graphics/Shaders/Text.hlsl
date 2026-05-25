// Screen-space text + overlay pass. The CPU emits quads already positioned in
// pixel coordinates (top-left origin); the vertex shader converts them to clip
// space using the viewport size handed in via the constant buffer.
//
// Glyph coverage is stored in the atlas ALPHA channel (RGB is left white), so
// glyph quads and solid background boxes share one path: a background box just
// samples the atlas' fully-opaque "solid" texel and carries a dark, half-
// transparent vertex colour.

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer TextParams : register(b0)
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
    VK_LOCATION(2) float4 Color      : COLOR;
};

struct VsOut
{
    float4 PositionCS : SV_Position;
    float2 Uv         : TEXCOORD0;
    float4 Color      : COLOR;
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
    o.Color      = input.Color;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float coverage = Atlas.Sample(AtlasSamp, input.Uv).a;
    return float4(input.Color.rgb, input.Color.a * coverage);
}
