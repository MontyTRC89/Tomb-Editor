struct PixelInputType
{
    float4 Position : SV_POSITION;
	float4 Color : COLOR;
    float3 Uvw : UVW;
};

SamplerState DefaultSampler : register(s0);
Texture2DArray SpriteTexture : register(t0);

float4 main(PixelInputType input) : SV_TARGET
{
    float4 result = SpriteTexture.Sample(DefaultSampler, input.Uvw);
	result.rgb *= result.a;
	result *= input.Color;
	
	if (result.a <= 0.05f)
		discard;

	return result;
}