struct VertexInputType
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR1;
};

struct PixelInputType
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR1;
};

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 Color;

bool TextureEnabled;
bool SelectionEnabled;
bool EditorTextureEnabled;

int Shape;
int SplitMode;

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
	output.Color = input.Color;

	return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	float4 pixel;

	float2 uv;

	uv = input.UV;

	if (TextureEnabled)
	{
		pixel = mul(Texture.Sample(TextureSampler, uv), input.Color);
	}
	else
	{
		pixel = float(0, 0, 0, 0);
	}

	return pixel;
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