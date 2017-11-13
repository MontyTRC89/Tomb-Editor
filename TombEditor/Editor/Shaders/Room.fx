struct VertexInputType
{
    float3 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR0;
	float2 EditorUV : TEXCOORD1;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;
	float4 WorldPosition : TEXCOORD2;
};

float4x4 ModelViewProjection;
float4x4 Model;
float LineWidth;

float4 Color;
bool Dim;
bool Highlight;

bool DrawSectorOutlinesAndUseEditorUV;
bool TextureEnabled;
bool UseVertexColors;

Texture2D Texture;
sampler TextureSampler;
float2 TextureCoordinateFactor;

bool FogBulbEnabled;
float4 FogBulbPosition;
float FogBulbRadius;
float FogBulbIntensity;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    output.Position = mul(float4(input.Position, 1.0f), ModelViewProjection);
	output.WorldPosition = mul(float4(input.Position, 1.0f), Model);
	output.UV = DrawSectorOutlinesAndUseEditorUV ? input.EditorUV : input.UV;
	if (UseVertexColors)
		output.Color = input.Color;
	else
		output.Color = Color;
    return output;
}




// Hack to enable textures with a height greater than 8096
float2 TextureAtlasRemappingSize;
[maxvertexcount(3)]
void GS(triangle PixelInputType input[3], inout TriangleStream<PixelInputType> SpriteStream)
{
	if (!DrawSectorOutlinesAndUseEditorUV)
	{
		float minY = min(input[0].UV.y, min(input[1].UV.y, input[2].UV.y));
		float pageIndex = floor((minY - 0.5f) / TextureAtlasRemappingSize.y);
		float2 uvMovementVector = TextureAtlasRemappingSize * float2(pageIndex, -pageIndex);

		input[0].UV += uvMovementVector;
		input[1].UV += uvMovementVector;
		input[2].UV += uvMovementVector;

		input[0].UV *= TextureCoordinateFactor;
		input[1].UV *= TextureCoordinateFactor;
		input[2].UV *= TextureCoordinateFactor;
	}

	SpriteStream.Append(input[0]);
	SpriteStream.Append(input[1]);
	SpriteStream.Append(input[2]);
}


float ddAny(float value)
{
	return length(float2(ddx(value), ddy(value)));
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	// Gather texture data
	float4 result;
	if (TextureEnabled)
		result = Texture.Sample(TextureSampler, abs(input.UV));
	else
		result = float4(1.0f, 1.0f, 1.0f, 1.0f);

	// Apply color
	float3 colorAdd = max(input.Color.xyz - 1.0f, 0.0f) * 0.37f;
	float3 colorMul = min(input.Color.xyz, 1.0f);
	result.xyz = result.xyz * colorMul + colorAdd;
	result.w *= input.Color.w;
    
    // Highlight & Dim (selection & diagonal steps)
    if(Highlight)
        result.xyz += 0.2f;
    if(Dim && !(result.x > 0.85f && result.y > 0.85f && result.z > 0.85f))
        result *= 0.70f;

	// Draw outline
	if (DrawSectorOutlinesAndUseEditorUV)
	{
		float2 absUV = abs(input.UV);

		float lineWidth = (LineWidth * 1024) / input.Position.w - 0.5f;
		float resolutionX = ddAny(input.UV.x);
		float resolutionY = ddAny(input.UV.y);
		float resolutionDiagonal = ddAny(input.UV.x + input.UV.y);

		float distanceX = min(absUV.x, 1.0f - absUV.x);
		float distanceY = min(absUV.y, 1.0f - absUV.y);
		float distanceDiagonal = min(abs(input.UV.x + input.UV.y + 1.0f), abs(input.UV.x + input.UV.y));

		float lineX = distanceX / resolutionX - lineWidth;
		float lineY = distanceY / resolutionY - lineWidth;
		float lineDiagonal = distanceDiagonal / resolutionDiagonal - lineWidth;

		float sectorAreaStrength = clamp(min(min(lineX, lineY), lineDiagonal), 0.0F, 1.0f);

		result.xyz *= sectorAreaStrength;
		result.w = 1.0f - (sectorAreaStrength - result.w * sectorAreaStrength);
	}

	// Render Fog Bulb
	if (FogBulbEnabled)
	{
		float distance = length(FogBulbPosition - input.WorldPosition);

		if (distance < FogBulbRadius)
		{
			result = lerp(result, float4(float3(1.0f, 1.0f, 1.0f) * FogBulbIntensity, 1.0f), 1.0f - distance / FogBulbRadius);
		}
	}
	return result;
}

technique10 Textured
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS() ) );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}