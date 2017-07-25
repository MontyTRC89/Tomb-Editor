struct VertexInputType
{
    float4 position : SV_POSITION;
	float4 Color : COLOR0;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
	float4 Color : COLOR0;
};

float4x4 ModelViewProjection;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    
    // Calcolo la posizione finale
    output.position = mul(input.position, ModelViewProjection);
    
	// Passo il colore al pixel shader
	output.Color = input.Color;

    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET
{
	return input.Color;
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