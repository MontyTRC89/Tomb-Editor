// Vertex shader for RenderingDrawingImportedGeometry — mirrors the legacy
// RoomGeometry.fx semantics (used for non-WAD imported 3D models).
//
// Distinguishing features vs. MeshShader:
//   - Per-submesh texture (NOT atlas) — each Submesh.Texture is bound to t0.
//   - UV in pixel coordinates (multiplied by 1/textureSize on GPU to normalize).
//   - No skinning, no atlas page index.
//   - Optional UseVertexColors flag selects between per-vertex tint and a flat color.

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

struct VertexInputType
{
    float3 Position : POSITION;
    float2 UV       : TEXCOORD;
    float3 Color    : COLOR;
    float3 Normal   : NORMAL;
};

struct PixelInputType
{
    float4 Position      : SV_POSITION;
    float2 UV            : TEXCOORD;
    float4 Color         : COLOR;
    float3 WorldPosition : WORLDPOSITION;
};

PixelInputType main(VertexInputType input)
{
    PixelInputType output;

    float4 worldPos = mul(World, float4(input.Position, 1.0f));
    output.Position = mul(TransformMatrix, worldPos);
    output.UV = input.UV * ReciprocalTextureSize;
    output.WorldPosition = worldPos.xyz;

    if (UseVertexColors != 0)
        output.Color = float4(input.Color * Tint.rgb, Tint.a);
    else
        output.Color = float4(Tint.rgb, Tint.a);

    return output;
}
