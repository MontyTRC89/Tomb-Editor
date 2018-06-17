struct VertexInputType
{
    float2 Position : POSITION;
	// Bit 0 - 24; U coordinate fixed point 0 - 1
	// Bit 24 - 48; V coordinate fixed point 0 - 1
	// Bit 48 - 60; W coordinate integer
	// Bit 61 - 64; Blend mode
    uint2 Uvw : UVW;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float3 Uvw : UVW;
};

PixelInputType main(VertexInputType input)
{
    PixelInputType output;

    output.Position = float4(input.Position, 1.0f, 1.0f);
    output.Uvw = float3( // Decompress UV coordinates
        (float)(input.Uvw.x & 0xffffff) / 16777216.0f,
        (float)((input.Uvw.x >> 24) | ((input.Uvw.y & 0xffff) << 8)) / 16777216.0f,
        (float)((input.Uvw.y >> 16) & 0xfff));

    return output;
}

