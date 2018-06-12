struct PixelInputType
{
    float4 Position : SV_POSITION;
    float3 Uvw : UVW;
};

SamplerState DefaultSampler : register(s0);
Texture2DArray FontTexture : register(t0);

float4 main(PixelInputType input) : SV_TARGET
{
    float4 result = FontTexture.Sample(DefaultSampler, input.Uvw);
	result.rgb *= result.a;
	return result;
}