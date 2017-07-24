struct PixelInputType
{
    float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
};

float4 PS(PixelInputType input) : SV_TARGET
{
	return float4(1.0f, 1.0f, 1.0f, 1.0f);
}

technique10 ClearScreen
{
    pass P0
    {
        SetVertexShader(NULL);
        SetGeometryShader(NULL);
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
} 