struct PixelInputType
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

float4 main(PixelInputType input) : SV_TARGET
{
    return input.Color;
}