// Vertex shader for RenderingDrawingLines — solid colored geometry (lines or
// triangles) with per-batch World transform and Tint multiplier.
//
// cbuffer slot 0 aliases the editor-wide RenderingStateBuffer. We only reference
// TransformMatrix; the trailing fields are present in memory but unused here.
//
// cbuffer slot 1 is owned by the Dx11RenderingDrawingLines instance and uploaded
// once per Render() call. Layout MUST match Dx11RenderingDrawingLines.LineDataLayout.
cbuffer FrameData : register(b0)
{
    matrix TransformMatrix;
};

cbuffer LineData : register(b1)
{
    matrix World;
    float4 Tint;
};

struct VertexInputType
{
    float3 Position : POSITION;
    float4 Color : COLOR;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

PixelInputType main(VertexInputType input)
{
    PixelInputType output;
    float4 worldPos = mul(World, float4(input.Position, 1.0f));
    output.Position = mul(TransformMatrix, worldPos);
    output.Color = input.Color * Tint;
    return output;
}
