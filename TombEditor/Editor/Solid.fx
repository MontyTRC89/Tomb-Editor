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
	float4 Color : COLOR0;
};

float4x4 ModelViewProjection;

float4 Color;
float3 CameraPosition;

bool SelectionEnabled;

PixelInputType VS(VertexInputType input)
{
	PixelInputType output;
	output.Position = mul(input.Position, ModelViewProjection);
	output.Color = input.Color;
	return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	float4 pixel;
	
	if (SelectionEnabled)
		pixel = float4(0.988f, 0.0f, 0.0f, 1.0f);
	else
		pixel = Color;

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