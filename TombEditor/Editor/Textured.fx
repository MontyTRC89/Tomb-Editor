struct VertexInputType
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR0;
	float2 EditorUV : TEXCOORD1;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
	float2 UV : COLOR0;
};

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 Color;
bool TextureEnabled;
bool SelectionEnabled;

Texture2D Texture;
sampler TextureSampler;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    
    // Calcolo la posizione finale
    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    
	// Passo le coordinate UV al pixel shader
	output.UV = input.UV;

    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	if (TextureEnabled)
	{
		float4 pixel = Texture.Sample(TextureSampler, input.UV);
		if (SelectionEnabled) pixel += float4(1.0f, -0.5f, -0.5f, 0.0f);
		return pixel;
	}
	else
	{
		return Color;
	}
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