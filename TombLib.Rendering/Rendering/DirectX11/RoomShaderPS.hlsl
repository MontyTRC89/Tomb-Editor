cbuffer WorldData
{
    matrix TransformMatrix;
    float RoomGridLineWidth;
	bool RoomGridForce;
	bool RoomDisableVertexColors;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
    float3 Uvw : UVW;
	int BlendMode : BLENDMODE;
    float2 EditorUv : EDITORUV;
	int EditorSectorTexture : EDITORSECTORTEXTURE;
};

Texture2DArray RoomTexture : register(t0);
Texture2DArray SectorTexture : register(t1);
SamplerState DefaultSampler : register(s0);



float ddAny(float value)
{
    return length(float2(ddx(value), ddy(value)));
}

float4 main(PixelInputType input) : SV_TARGET
{
	int drawOutline = 0;
    float4 result;
    if (input.Uvw.x != 0 && !RoomGridForce)
    { // Textured view
        result = RoomTexture.Sample(DefaultSampler, input.Uvw);

		float3 colorAdd = max(input.Color.xyz - 1.0f, 0.0f) * 0.37f;
		float3 colorMul = min(input.Color.xyz, 1.0f);
		result.xyz = result.xyz * colorMul + colorAdd;
        result.w *= input.Color.w;

		result.xyz *= input.Color.w; // Turn into premultiplied alpha

		if (input.BlendMode == 2)
			result.w = 0.0f; // Additive blending

		if (input.EditorSectorTexture & 0x40) // Selected?
		{
			result.x += 0.05f;
			drawOutline = 2;
		}
    }
    else
    { // Geometric view
		drawOutline = 1;

        if (input.Uvw.y == 0 && !RoomGridForce)
            result = float4(0.0f, 0.0f, 0.0f, 0.0f);
        else
		{
			if (input.EditorSectorTexture & 0x20) // Textured or colored?
				result = SectorTexture.Sample(DefaultSampler, float3(input.EditorUv, (float)(input.EditorSectorTexture >> 7)));
			else
				result = float4(
					((input.EditorSectorTexture >> 7) & 0xff) * (1.0f / 255.0f),
					((input.EditorSectorTexture >> 15) & 0xff) * (1.0f / 255.0f),
					((input.EditorSectorTexture >> 23) & 0xff) * (1.0f / 255.0f),
					1.0f);
			if (input.EditorSectorTexture & 0x10) // Highlight?
				result.xyz += 0.2f;
		}
	}

	if (drawOutline > 0)
	{	// Draw outline
		float2 absUV = abs(input.EditorUv);

		float lineWidth = (RoomGridLineWidth * 1024) / input.Position.w - 0.5f;
		float resolutionX = ddAny(input.EditorUv.x);
		float resolutionY = ddAny(input.EditorUv.y);
		float resolutionDiagonal = ddAny(input.EditorUv.x + input.EditorUv.y);

		float distanceX = min(absUV.x, 1.0f - absUV.x);
		float distanceY = min(absUV.y, 1.0f - absUV.y);
		float distanceDiagonal = min(
			abs(input.EditorUv.x + input.EditorUv.y + 1.0f),
			abs(input.EditorUv.x + input.EditorUv.y));

		float lineX = distanceX / resolutionX - lineWidth;
		float lineY = distanceY / resolutionY - lineWidth;
		float lineDiagonal = distanceDiagonal / resolutionDiagonal - lineWidth;

		float sectorAreaStrength = clamp(min(min(lineX, lineY), lineDiagonal), 0.0F, 1.0f);

		result.xyz *= sectorAreaStrength;
		if (drawOutline == 2)
			result.x -= sectorAreaStrength - 1.0f;

		result.w = 1.0f - (1.0f - result.w) * sectorAreaStrength;
	}

    if ((result.x + result.y + result.z + result.w) < 0.02f)
        discard;
    return result;
}