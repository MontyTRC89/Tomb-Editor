cbuffer WorldData
{
	matrix TransformMatrix;
	float RoomGridLineWidth;
	bool RoomGridForce;
	bool RoomDisableVertexColors;
	bool ShowExtraBlendingModes;
	bool ShowLightingWhiteTextureOnly;
	bool Show15BitLighting;
};

struct VertexInputType
{
    float4 Position : POSITION;
    float4 Color : COLOR;
	// Overlay for geometry mode symbols, e.g. arrows
	float4 Overlay : OVERLAY;
	// Bit 0 - 24; U coordinate fixed point 0 - 1
	// Bit 24 - 48; V coordinate fixed point 0 - 1
	// Bit 48 - 60; W coordinate integer
	// Bit 61 - 64; Blend mode
    uint2 UvwAndBlendMode : UVWANDBLENDMODE;
	// Bit 0 - 1: X coordinate
	// Bit 2 - 3: Y coordinate
	// Bit 4: Highlighted
	// Bit 5: Dimmed
	// Bit 6: Is textured?
	// Bit 7: Selection
	// Bit 8 - 31: either coloring or texture index
    uint EditorUv : EDITORUVANDSECTORTEXTURE;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
	float4 Overlay : OVERLAY;
    float3 Uvw : UVW;
	int BlendMode : BLENDMODE;
    float2 EditorUv : EDITORUV;
	int EditorSectorTexture : EDITORSECTORTEXTURE;
};

PixelInputType main(VertexInputType input)
{
    PixelInputType output;

    input.Position.w = 1.0f;
    output.Position = mul(TransformMatrix, input.Position);
    output.Color = RoomDisableVertexColors ? float4(1.0f, 1.0f, 1.0f, 1.0f) : (input.Color * float4(2.0f, 2.0f, 2.0f, 1.0f));
	if (Show15BitLighting) 
	{
		int r = output.Color.r * 32.0f;
		int g = output.Color.g * 32.0f;
		int b = output.Color.b * 32.0f;
		r = floor(r);
		g = floor(g);
		b = floor(b);
		output.Color = float4(r / 32.0f, g / 32.0f, b / 32.0f, output.Color.a);
	}

    output.Uvw = float3( // Decompress UV coordinates
        (float)(input.UvwAndBlendMode.x & 0xffffff) / 16777216.0f,
        (float)((input.UvwAndBlendMode.x >> 24) | ((input.UvwAndBlendMode.y & 0xffff) << 8)) / 16777216.0f,
        (float)((input.UvwAndBlendMode.y >> 16) & 0xfff));
	output.BlendMode = input.UvwAndBlendMode.y >> 28;
	output.EditorUv = float2(
		(int)(input.EditorUv << 30) >> 30, // Sign extend
		(int)((input.EditorUv >> 2) << 30) >> 30); // Sign extend;
	output.EditorSectorTexture = input.EditorUv;
	output.Overlay = input.Overlay;
    return output;
}

