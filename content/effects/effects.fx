float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightDirection;

struct VertexShaderInput
{
    float4 Position	: SV_POSITION;
};

struct VertexToPixel
{
	float4 Position : SV_POSITION;
	float Height : COLOR0;
};

struct PixelToFrame
{
	float4 Colour   : COLOR0;
};

VertexToPixel ColouredVS(VertexShaderInput input)
{
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	VertexToPixel output;

	output.Position = mul(input.Position, preWorldViewProjection);
	output.Height = clamp((input.Position.y - 50.0) / 150.0, 0.0, 1.0);

	return output;
}

PixelToFrame ColouredPS(VertexToPixel input)
{
	PixelToFrame output;

	output.Colour = float4(input.Height, 1.0, input.Height, 1.0);

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
