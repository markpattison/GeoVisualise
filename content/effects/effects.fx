float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightDirection;
float4 xTerrainColour;
float4 xSpotHeightColour;

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
};

struct PixelToFrame
{
	float4 Colour    : COLOR0;
};

VertexToPixel TerrainVS(VertexShaderInput input)
{
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	VertexToPixel output;

	float3 normal = normalize(mul(float4(normalize(input.Normal), 0.0), xWorld)).xyz;

	output.Position = mul(input.Position, preWorldViewProjection);
    output.Normal = normal;

	return output;
}

PixelToFrame TerrainPS(VertexToPixel input)
{
	PixelToFrame output;

	float lightingFactor = saturate(dot(input.Normal, -xLightDirection));

	output.Colour = xTerrainColour * (lightingFactor + 0.2);

	return output;
}

PixelToFrame SpotHeightPS(VertexToPixel input)
{
	PixelToFrame output;

	float lightingFactor = saturate(dot(input.Normal, -xLightDirection));

	output.Colour = xSpotHeightColour * (lightingFactor + 0.2);

	return output;
}

technique Terrain
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 TerrainVS();
		PixelShader = compile ps_4_0 TerrainPS();
	}
}

technique SpotHeight
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 TerrainVS();
		PixelShader = compile ps_4_0 SpotHeightPS();
	}
}
