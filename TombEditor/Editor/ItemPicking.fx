struct VertexInputType
{
    float4 position : POSITION0;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
};

struct PixelOutput
{
	float4 Picking1 : SV_TARGET0;
	float4 Picking2 : SV_TARGET1;
};

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 Color;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    
    // Calcolo la posizione finale
    output.position = mul(input.position, World);
    output.position = mul(output.position, View);
    output.position = mul(output.position, Projection);
    
    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
PixelOutput PS(PixelInputType input)
{
	float4 color = Color;

	PixelOutput output = (PixelOutput)0;

	output.Picking1 = float4(color.x, color.y, color.z, 1.0f);
	output.Picking2 = float4(color.w, 0.0f, 0.0f, 1.0f);

	return output;
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