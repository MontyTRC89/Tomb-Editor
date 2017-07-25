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
};

float4x4 World;
float4x4 View;
float4x4 Projection;

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

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    
    // Calcolo la posizione finale
    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    
	// Passo le coordinate UV al pixel shader
	output.UV = input.UV;
	output.EditorUV = input.EditorUV;
	output.Color = input.Color;

    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	float4 pixel;

	if (Saved)
	{
		return float4(0, 1, 0, 1);
	}
	else
	{
		float2 uv;

		if (EditorTextureEnabled)
			uv = input.EditorUV;
		else
			uv = input.UV;

		if (TextureEnabled)
		{
			pixel = Texture.Sample(TextureSampler, uv);
			if (SelectionEnabled) pixel += float4(1.0f, -0.5f, -0.5f, 0.0f);
			if (LightingEnabled)
			{
				float4 col = float4(input.Color.x / 255.0f, input.Color.y / 255.0f, input.Color.z / 255.0f, 1.0f);

				pixel = clamp(pixel * col * 2.0f, 0, 1);
				//pixel.w = 1.0f;
			}
		}
		else
		{
			float3 distance = length(float4(CameraPosition, 1.0f) - input.Position);
			float LINE_SIZE = 0.025f * (1024000 - distance) / 1024000;

			pixel = float4(0.0f, 0.0f, 0.0f, 1.0f);

			if (input.EditorUV.x > LINE_SIZE && input.EditorUV.x < (1.0f - LINE_SIZE) &&
				input.EditorUV.y > LINE_SIZE && input.EditorUV.y < (1.0f - LINE_SIZE) && Shape == 0)
			{
				pixel = Color;
				if (SelectionEnabled) pixel = float4(0.988f, 0.0f, 0.0f, 1.0f);
			}

			if (input.EditorUV.x > LINE_SIZE && input.EditorUV.x < (1.0f - LINE_SIZE) &&
				input.EditorUV.y > LINE_SIZE && input.EditorUV.y < (1.0f - LINE_SIZE) &&
				(input.EditorUV.y < 1.0f - LINE_SIZE - input.EditorUV.x ||
					input.EditorUV.y > 1.0f + LINE_SIZE - input.EditorUV.x) && Shape == 1 && SplitMode == 1)
			{
				pixel = Color;
				if (SelectionEnabled) pixel = float4(0.988f, 0.0f, 0.0f, 1.0f);
			}

			if (input.EditorUV.x > LINE_SIZE && input.EditorUV.x < (1.0f - LINE_SIZE) &&
				input.EditorUV.y > LINE_SIZE && input.EditorUV.y < (1.0f - LINE_SIZE) &&
				(input.EditorUV.y < -LINE_SIZE + input.EditorUV.x ||
					input.EditorUV.y > +LINE_SIZE + input.EditorUV.x) && Shape == 1 && SplitMode == 0)
			{
				pixel = Color;
				if (SelectionEnabled) pixel = float4(0.988f, 0.0f, 0.0f, 1.0f);
			}
		}
	}

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