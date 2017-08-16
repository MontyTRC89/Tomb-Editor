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
	float2 UV : TEXCOORD0;
	float2 EditorUV : TEXCOORD1;
	float4 Color : COLOR0;
	float4 WorldPosition : TEXCOORD2;
};

float4x4 ModelViewProjection;
float4x4 Model;

float4 Color;
float3 CameraPosition;

bool TextureEnabled;
bool SelectionEnabled;
bool EditorTextureEnabled;
bool InvisibleFaceEnabled;
bool LightingEnabled;

int Shape;
int SplitMode;

bool Saved;

Texture2D Texture;
sampler TextureSampler;

bool FogBulbEnabled;
float4 FogBulbPosition;
float FogBulbRadius;
float FogBulbIntensity;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    output.Position = mul(input.Position, ModelViewProjection);
	output.WorldPosition = mul(input.Position, Model);
	output.UV = input.UV;
	output.EditorUV = input.EditorUV;
	output.Color = input.Color * (1.0f / 255.0f);
    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	if (Saved)
	{
		return float4(0, 1, 0, 1);
	}
	else
	{
		float2 uv = EditorTextureEnabled ? input.EditorUV : input.UV;
		
		float4 pixel;
		if (TextureEnabled)
		{
			pixel = Texture.Sample(TextureSampler, uv);
			if (SelectionEnabled) pixel += float4(1.0f, -0.5f, -0.5f, 0.0f);
			if (LightingEnabled)
			{
				float3 colorAdd = clamp(input.Color.xyz * 2.0f - 1.0f, 0.0f, 1.0f) * (1.0f / 3.0f);
				float3 colorMul = min(input.Color.xyz * 2.0f, 1.0f);
				pixel.xyz = pixel.xyz * colorMul + colorAdd;

				if (FogBulbEnabled)
				{
					float distance = length(FogBulbPosition - input.WorldPosition);

					if (distance < FogBulbRadius)
					{
						pixel = lerp(pixel, float4(float3(1.0f, 1.0f, 1.0f) * FogBulbIntensity, 1.0f), pow(1.0f - distance / FogBulbRadius, 2));
					}
				}   
			}
		}
		else
		{
			float distance = length(float4(CameraPosition, 1.0f) - input.Position);
			float LINE_SIZE = 0.025f * (1024000 - distance) / 1024000;

			pixel = float4(0.0f, 0.0f, 0.0f, 1.0f);

			if (input.EditorUV.x > LINE_SIZE && input.EditorUV.x < (1.0f - LINE_SIZE) &&
				input.EditorUV.y > LINE_SIZE && input.EditorUV.y < (1.0f - LINE_SIZE) && Shape == 0)
			{
				pixel = Color;
				if (SelectionEnabled) 
					pixel = float4(0.988f, 0.0f, 0.0f, 1.0f);
			}

			if (input.EditorUV.x > LINE_SIZE && input.EditorUV.x < (1.0f - LINE_SIZE) &&
				input.EditorUV.y > LINE_SIZE && input.EditorUV.y < (1.0f - LINE_SIZE) &&
				(input.EditorUV.y < 1.0f - LINE_SIZE - input.EditorUV.x ||
					input.EditorUV.y > 1.0f + LINE_SIZE - input.EditorUV.x) && Shape == 1 && SplitMode == 1)
			{
				pixel = Color;
				if (SelectionEnabled)
					pixel = float4(0.988f, 0.0f, 0.0f, 1.0f);
			}

			if (input.EditorUV.x > LINE_SIZE && input.EditorUV.x < (1.0f - LINE_SIZE) &&
				input.EditorUV.y > LINE_SIZE && input.EditorUV.y < (1.0f - LINE_SIZE) &&
				(input.EditorUV.y < -LINE_SIZE + input.EditorUV.x ||
					input.EditorUV.y > +LINE_SIZE + input.EditorUV.x) && Shape == 1 && SplitMode == 0)
			{
				pixel = Color;
				if (SelectionEnabled) 
					pixel = float4(0.988f, 0.0f, 0.0f, 1.0f);
			}
		}
		return pixel;
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