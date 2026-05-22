// Room geometry pass: world-space position + per-vertex tint, atlas UV
// and a separate "grid UV" (the raw VertexEditorUVs from RoomGeometry,
// values in {-1, 0, 1} at the four sector-face corners).
//
// Grid line algorithm: ported verbatim from the legacy RoomShaderPS.hlsl.
// Uses screen-space derivatives (length of (ddx, ddy), not fwidth) to
// keep a constant pixel width regardless of distance, and adds a diagonal
// line component that catches the sector-face split. Lines render BLACK
// (multiplicative darkening) — same look as the original.
//
// Bindings:
//   b0  ViewParams : float4x4 ViewProjection; float GridLineWidth; ...
//   t0  Atlas      : Texture2D (BGRA8 atlas: white pixel + arrows + level tex)
//   s0  AtlasSamp  : SamplerState
//
// Vertex layout (stride 40):
//   POSITION  : float3   (world space)
//   COLOR     : float3   (RGB tint, 0..1)
//   TEXCOORD0 : float2   (atlas UV)
//   TEXCOORD1 : float2   (grid UV = raw VertexEditorUVs)

#include "Bindings.hlsli"

VK_BINDING(0, 0)
cbuffer ViewParams : register(b0)
{
    float4x4 ViewProjection;
    float    GridLineWidth;     // legacy default 10.0
    float    GridEnabled;       // 1.0 = draw sector outlines, 0.0 = skip
    float    RoomAlpha;         // 1.0 normally; < 1.0 fades a hidden room
    float    _pad2;
    // Flyby depth-of-field overlay — DofColorStrength.w packs the mode
    // (0 = inactive). Mirrors the legacy IndicationOverlay.hlsli.
    float4   DofCenterRange;        // xyz = camera origin, w = focus range
    float4   DofDirectionDistance;  // xyz = view direction, w = focus distance
    float4   DofColorStrength;      // xyz = darkening colour, w = mode
};

VK_BINDING(1, 0) Texture2D    Atlas     : register(t0);
VK_BINDING(2, 0) SamplerState AtlasSamp : register(s0);

struct VsIn
{
    VK_LOCATION(0) float3 PositionWS : POSITION;
    // Color.rgb = vertex tint (sector classification colour, or lighting in
    // texturing mode). Color.a is a sector-overlay flag (1.0 = sprite is a
    // SectorTexture arrow/icon and should be additively/subtractively
    // composited like the legacy shader, 0.0 = plain multiply).
    VK_LOCATION(1) float4 Color      : COLOR;
    VK_LOCATION(2) float2 Uv         : TEXCOORD0;
    VK_LOCATION(3) float2 GridUv     : TEXCOORD1;
};

struct VsOut
{
    float4 PositionCS    : SV_Position;
    float4 Color         : COLOR;
    float2 Uv            : TEXCOORD0;
    float2 GridUv        : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;   // for the depth-of-field overlay
};

// Per-axis derivative length, used as the "resolution" of the grid UV in
// the legacy shader. Equivalent to length(ddx(v), ddy(v)).
float ddAny(float value)
{
    return length(float2(ddx(value), ddy(value)));
}

// Flyby depth-of-field overlay — multiplicative darkening that fades in with
// distance from the focus plane. Ported from the legacy ApplyDofOverlay
// (IndicationOverlay.hlsli).
float3 ApplyDof(float3 rgb, float3 worldPos)
{
    int dofMode = (int)round(DofColorStrength.w);
    if (dofMode == 0)
        return rgb;

    float3 dir    = DofDirectionDistance.xyz;
    float  dirLen = length(dir);
    if (dirLen <= 0.0001)
        return rgb;
    dir /= dirLen;

    float3 focusPoint  = DofCenterRange.xyz + dir * DofDirectionDistance.w;
    float  signedDepth = dot(worldPos - focusPoint, dir);

    if (dofMode == 2)            // Front: darken only behind the focus plane
    {
        if (signedDepth >= 0.0) return rgb;
        signedDepth = -signedDepth;
    }
    else if (dofMode == 3)       // Back: darken only in front of the focus plane
    {
        if (signedDepth <= 0.0) return rgb;
    }
    else                         // Full: darken both sides
    {
        signedDepth = abs(signedDepth);
    }

    float gradient = saturate(signedDepth / max(DofCenterRange.w, 1.0));
    if (gradient <= 0.0)
        return rgb;
    return rgb * lerp(float3(1.0, 1.0, 1.0), DofColorStrength.xyz, gradient);
}

VsOut vs_main(VsIn input)
{
    VsOut o;
    o.PositionCS    = mul(ViewProjection, float4(input.PositionWS, 1.0));
    o.Color         = input.Color;
    o.Uv            = input.Uv;
    o.GridUv        = input.GridUv;
    o.WorldPosition = input.PositionWS;
    return o;
}

float4 ps_main(VsOut input) : SV_Target
{
    float4 sampled = Atlas.Sample(AtlasSamp, input.Uv);
    float4 outColor;

    // GridEnabled is a uniform (cbuffer value), so this split is NOT a
    // divergent branch — every fragment of the draw takes the same side.
    if (GridEnabled < 0.5)
    {
        // ---- Texturing / Lighting mode: real textures + per-face blend modes.
        // Color.a carries the face BlendMode (0-15), packed by
        // LevelRenderer.PackColor. Color.rgb is the tint (full-bright in
        // texturing mode, per-vertex lighting in lighting mode).
        uint   blendMode = (uint)(input.Color.a * 255.0 + 0.5);
        float3 rgb       = sampled.rgb * input.Color.rgb;
        float  texA      = sampled.a;

        // AlphaTest (1): hard cutout — discard see-through texels.
        if (blendMode == 1u)
            clip(texA - 0.5);

        // Premultiplied-alpha output for the transparent room pass, which
        // blends with (One, OneMinusSrcAlpha): out-alpha 0 => additive,
        // texA => alpha blend, 1 => opaque.
        if (blendMode == 2u)               // Additive
            outColor = float4(rgb * texA, 0.0);
        else if (blendMode >= 3u)          // AlphaBlend + exotic modes
            outColor = float4(rgb * texA, texA);
        else                               // Normal / AlphaTest (opaque)
            outColor = float4(rgb, 1.0);
    }
    else
    {
        // ---- Geometry mode: sector classification colour + overlay sprites + grid.
        // Branchless overlay/regular composite — an if/else here becomes a
        // divergent SPIR-V branch that breaks the grid pass' derivatives.
        float  bright     = dot(input.Color.rgb, float3(0.299, 0.587, 0.114));
        float3 addRgb     = saturate(input.Color.rgb + sampled.rgb);
        float3 subRgb     = saturate(input.Color.rgb - sampled.rgb);
        float3 overlayRgb = lerp(addRgb, subRgb, step(0.8, bright));
        float3 regularRgb = sampled.rgb * input.Color.rgb;

        float  ovMask   = step(0.5, input.Color.a);
        float3 finalRgb = lerp(regularRgb, overlayRgb, ovMask);
        float  finalA   = lerp(sampled.a,  1.0,        ovMask);
        float4 result   = float4(finalRgb, finalA);

        // Sector outline — direct port of legacy RoomShaderPS.hlsl.
        float2 absUV = abs(input.GridUv);

        // Perspective-corrected width: 10 (default) * 1024 / clip-w - 0.5.
        float lineWidth = (GridLineWidth * 1024.0) / input.PositionCS.w - 0.5;

        float resolutionX        = ddAny(input.GridUv.x);
        float resolutionY        = ddAny(input.GridUv.y);
        float resolutionDiagonal = ddAny(input.GridUv.x + input.GridUv.y);

        float distanceX = min(absUV.x, 1.0 - absUV.x);
        float distanceY = min(absUV.y, 1.0 - absUV.y);
        float distanceDiagonal = min(
            abs(input.GridUv.x + input.GridUv.y + 1.0),
            abs(input.GridUv.x + input.GridUv.y));

        float lineX        = distanceX        / resolutionX        - lineWidth;
        float lineY        = distanceY        / resolutionY        - lineWidth;
        float lineDiagonal = distanceDiagonal / resolutionDiagonal - lineWidth;

        float sectorAreaStrength = clamp(min(min(lineX, lineY), lineDiagonal), 0.0, 1.0);

        result.xyz *= sectorAreaStrength;
        result.w    = 1.0 - (1.0 - result.w) * sectorAreaStrength;
        outColor = result;
    }

    // Flyby depth-of-field overlay (no-op when DofColorStrength.w == 0).
    outColor.rgb = ApplyDof(outColor.rgb, input.WorldPosition);

    // RoomAlpha < 1 fades the whole room (hidden-room rendering). Scaling a
    // premultiplied-alpha colour by a scalar keeps it premultiplied.
    return outColor * RoomAlpha;
}
