struct VertexInputType
{
    float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
	float4 PositionCopy : TEXCOORD0;
};

float4x4 World;
float4x4 View;
float4x4 Projection;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    
    // Calcolo la posizione finale
    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);

	output.PositionCopy = input.Position;

    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	float4 depth = float4(1.0f - (input.PositionCopy.z / input.PositionCopy.w), 0.0f, 0.0f, 1.0f);

	return depth;
}

technique10 Picking
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader(NULL);
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
} 