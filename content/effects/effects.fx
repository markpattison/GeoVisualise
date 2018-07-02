float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightDirection;

struct VertexShaderInput
{
    float4 Position  : SV_POSITION;
	float3 Normal    : NORMAL;
	float2 TexCoords : TEXCOORD0;
};

struct VertexToPixel
{
	float4 Position  : SV_POSITION;
	float3 Normal    : NORMAL;
	float  Height    : COLOR0;
};

struct PixelToFrame
{
	float4 Colour    : COLOR0;
};

VertexToPixel ColouredVS(VertexShaderInput input)
{
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	VertexToPixel output;

	float3 normal = normalize(mul(float4(normalize(input.Normal), 0.0), xWorld)).xyz;

	output.Position = mul(input.Position, preWorldViewProjection);
    output.Normal = normal;
	output.Height = saturate((input.Position.z - 50.0) / 150.0);

	return output;
}

PixelToFrame ColouredPS(VertexToPixel input)
{
	PixelToFrame output;

	float lightingFactor = saturate(dot(input.Normal, -xLightDirection));

	output.Colour = float4(input.Height, 1.0, input.Height, 1.0) * (lightingFactor + 0.1);

	return output;
}

technique Coloured
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ColouredVS();
		PixelShader = compile ps_4_0 ColouredPS();
	}
}
