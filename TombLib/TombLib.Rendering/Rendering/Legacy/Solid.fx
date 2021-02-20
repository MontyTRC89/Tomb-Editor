struct VertexInputType
{
    float3 Position : POSITION0;
    float4 Color : COLOR0;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

float4x4 ModelViewProjection;

float4 Color;
float3 CameraPosition;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    output.Position = mul(float4(input.Position, 1.0f), ModelViewProjection);
    output.Color = input.Color * Color;
    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
    return input.Color;
}

technique10 Textured
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_4_0, VS()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_4_0, PS()));
    }
}