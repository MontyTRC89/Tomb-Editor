struct VertexInputType
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
};

float4x4 ModelViewProjection;

float4 Color;
bool SelectionEnabled;

Texture2D Texture;
sampler TextureSampler;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    output.Position = mul(input.Position, ModelViewProjection);
	output.UV = input.UV;
    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	float4 pixel = Texture.Sample(TextureSampler, input.UV);
		
	float3 colorAdd = clamp(Color.xyz * 2.0f - 1.0f, 0.0f, 1.0f) * (1.0f / 3.0f);
	float3 colorMul = max(Color.xyz * 2.0f, 1.0f);
	pixel.xyz = pixel.xyz * colorMul + colorAdd;
	
	if (SelectionEnabled) 
		pixel += float4(1.0f, -0.5f, -0.5f, 0.0f);

	return pixel;
}

technique10 Textured
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader(NULL);
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
} 