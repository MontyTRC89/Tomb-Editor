cbuffer WorldData
{
    matrix TransformMatrix;
    float RoomGridLineWidth;
	bool RoomGridForce;
	bool RoomDisableVertexColors;
};
struct VertexInputType
{
    float4 Position : POSITION;
    float4 Color : COLOR;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

PixelInputType main(VertexInputType input)
{
    PixelInputType output;

    input.Position.w = 1.0f;
    output.Position = mul(TransformMatrix, input.Position);
    output.Color = input.Color;

    return output;
}

