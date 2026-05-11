// Vertex shader for RenderingDrawingMesh — moveables, statics, imported geometry,
// skybox. Matches the legacy Model.fx semantics:
//
//   - Optional skinning via 4 bone influences per vertex (BoneIndex / BoneWeight).
//   - Per-vertex Color used as static lighting modulator OR replaced with a flat tint.
//   - UVW input: UV in xy, atlas page index in z (sampled as Texture2DArray).
//
// cbuffer slot 0: shared editor state buffer (TransformMatrix = view-projection).
// cbuffer slot 1: per-batch data (World + Tint + Bones + flags).

// Mirrors Dx11RenderingStateBuffer.ConstantBufferLayout in C# (must match exactly).
// We only consume TransformMatrix in the VS — the trailing fields exist for PS use
// (brush overlay) and need to be declared so register slots line up.
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

#define MAX_BONES 32

cbuffer MeshData : register(b1)
{
    matrix World;
    float4 Tint;
    matrix Bones[MAX_BONES];
    int Skinned;          // 0/1
    int StaticLighting;   // 0/1: multiply Tint by per-vertex Color
    int ColoredVertices;  // 0/1: keep RGB, otherwise convert to luma (legacy behaviour)
    int _padding;
};

struct VertexInputType
{
    float3 Position : POSITION;
    float3 UVW      : TEXCOORD;
    float3 Normal   : NORMAL;
    float3 Color    : COLOR;
    float4 BoneIdx  : BLENDINDICES;
    float4 BoneW    : BLENDWEIGHTS;
};

struct PixelInputType
{
    float4 Position      : SV_POSITION;
    float3 UVW           : TEXCOORD;
    float4 Color         : COLOR;
    float3 WorldPosition : WORLDPOSITION;
};

PixelInputType main(VertexInputType input)
{
    PixelInputType output;

    float4 localPos = float4(input.Position, 1.0f);

    // Skinning: blend up to 4 bone influences. Mirrors Model.fx but with explicit
    // weight normalization to handle WAD data where weights don't always sum to 1.
    if (Skinned != 0)
    {
        float totalWeight = dot(input.BoneW, 1.0);
        int4 bi = int4(
            clamp((int)input.BoneIdx.x, 0, MAX_BONES - 1),
            clamp((int)input.BoneIdx.y, 0, MAX_BONES - 1),
            clamp((int)input.BoneIdx.z, 0, MAX_BONES - 1),
            clamp((int)input.BoneIdx.w, 0, MAX_BONES - 1));

        float4x4 blended = (float4x4)0;
        const float EPS = 1e-38;
        if (totalWeight < EPS)
        {
            blended = Bones[bi.x];
        }
        else
        {
            blended += Bones[bi.x] * (input.BoneW.x / totalWeight);
            blended += Bones[bi.y] * (input.BoneW.y / totalWeight);
            blended += Bones[bi.z] * (input.BoneW.z / totalWeight);
            blended += Bones[bi.w] * (input.BoneW.w / totalWeight);
            // Force the last column to identity to remove rounding artefacts
            blended[0].w = 0; blended[1].w = 0; blended[2].w = 0; blended[3].w = 1;
        }

        localPos = mul(blended, localPos);
    }

    float4 worldPos = mul(World, localPos);
    output.Position = mul(TransformMatrix, worldPos);
    output.UVW = input.UVW;
    output.WorldPosition = worldPos.xyz;

    // Per-vertex Color processing matches legacy Model.fx: optionally desaturate to
    // luma, then either modulate the Tint by color (StaticLighting) or use the Tint
    // directly. Vertex colors in WAD data are typically used as baked lighting.
    float3 vc = input.Color;
    if (ColoredVertices == 0)
    {
        float luma = vc.r * 0.2126f + vc.g * 0.7152f + vc.b * 0.0722f;
        vc = float3(luma, luma, luma);
    }
    if (StaticLighting != 0)
        output.Color = float4(Tint.rgb * vc, Tint.a);
    else
        output.Color = Tint;

    return output;
}
