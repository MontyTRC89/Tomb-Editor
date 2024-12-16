﻿struct VertexInputType
{
    float3 Position : POSITION0;
    float3 UVW : TEXCOORD0;
    float3 Normal : NORMAL0;
    float3 Color : COLOR0;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float3 UVW : TEXCOORD0;
    float4 Color : COLOR;
};

float4x4 ModelViewProjection;
float4 Color;
bool AlphaTest;
bool StaticLighting;
bool ColoredVertices;

Texture2DArray Texture;
sampler TextureSampler;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    output.Position = mul(float4(input.Position, 1.0f), ModelViewProjection);
    output.UVW = input.UVW;
	output.Color = float4(input.Color, 1.0f);
	
	if (!ColoredVertices) 
	{
		float luma = (output.Color.x * 0.2126f) + (output.Color.y * 0.7152f) + (output.Color.z * 0.0722f);
		output.Color = float4(luma, luma, luma, output.Color.w);
	}
	
	if (StaticLighting)
		output.Color = float4(Color.xyz * output.Color.xyz, 1.0f);
	else
		output.Color = Color;
		
    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
    float4 pixel = Texture.Sample(TextureSampler, input.UVW);
	
    float3 colorAdd = max(input.Color.xyz - 1.0f, 0.0f) * 0.37f;
    float3 colorMul = min(input.Color.xyz, 1.0f);
    pixel.xyz = pixel.xyz * colorMul + colorAdd;
    pixel.w *= input.Color.w;

	if (AlphaTest == true && pixel.w <= 0.01f)
		discard;

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