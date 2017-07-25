/*struct VertexInputType
{
    float4 position : POSITION0;
    float2 tex : TEXCOORD0;
	float3 Normal : NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
	float4 FaceColor : COLOR0;
	float4 VertexColor : COLOR1;
};

struct PixelInputType
{
float4 position : SV_POSITION;
float3 ViewDirection : TEXCOORD0;
float3 Normal : NORMAL;
};

*/

struct VertexInputType
{
	float3 position : POSITION0;
	float3 Normal : NORMAL0;
	float2 tex : TEXCOORD0;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
};

float4x4 ModelViewProjection;

float4 Color;
bool UseVertexColor;
float3 CameraPosition;

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;
    
    // Calcolo la posizione finale
	output.position = float4(input.position, 1); // +float4(input.Normal * 2, 0);
    output.position = mul(output.position, ModelViewProjection);

	//output.ViewDirection = normalize(CameraPosition - mul(input.position, World));
	//output.Normal = normalize(mul(input.Normal, (float3x3) World));

    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PS(PixelInputType input) : SV_TARGET0
{
	//float4 color = float4(0.0f, 0.0f, 0.0f, 1.0f);
	//if (dot(input.Normal, input.ViewDirection) < 0) color = float4(0, 0, 0, 0);

	return float4(1,1,1,1);
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