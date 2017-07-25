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
bool TextureEnabled;
bool SelectionEnabled;
bool EditorTextureEnabled;
bool LightEnabled;

int Shape;
int SplitMode;

Texture2D Texture;
sampler TextureSampler;

Texture2D HelperTexture;
sampler HelperTextureSampler;

float4x4 BoneTransforms[32];

PixelInputType VS(VertexInputType input)
{
	PixelInputType output;
	float4 worldPos;

	// Calcolo la posizione finale
	output.Position = mul(input.Position, ModelViewProjection);

	// Passo le coordinate UV al pixel shader
	output.UV = input.UV;

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
		if (SelectionEnabled)
		{
			pixel = Texture.Sample(TextureSampler, uv);
			if (SelectionEnabled) pixel += float4(1.0f, -0.5f, -0.5f, 0.0f);
		}
		else
		{
			pixel = Texture.Sample(TextureSampler, uv);
			if (LightEnabled) pixel = pixel * Color * 2.0f;
			if (pixel.w > 1.0f) pixel.w = 1.0f;
		}		
	}
	else
	{

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