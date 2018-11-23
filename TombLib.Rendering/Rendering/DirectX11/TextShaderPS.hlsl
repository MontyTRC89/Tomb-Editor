struct PixelInputType
{
    float4 Position : SV_POSITION;
    float3 Uvw : UVW;
	int BlendMode : BLENDMODE;
};

SamplerState DefaultSampler : register(s0);
Texture2DArray FontTexture : register(t0);

float4 main(PixelInputType input) : SV_TARGET
{
    float4 result;
    if(input.BlendMode == 0)
    {
        result = FontTexture.Sample(DefaultSampler, input.Uvw);
        //result.a = 0.0f;
        result.a = (result.r + result.g + result.b) / 3.0f;
        result.xyz *= result.a;
    }
    else
        result = float4(0.0f, 0.0f, 0.0f, 0.7f);    // overlay
        
	return result;
}