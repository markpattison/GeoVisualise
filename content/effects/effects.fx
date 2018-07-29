float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightDirection;
float4 xTerrainColour;
float4 xContourColour;
float4 xSpotHeightColour;

texture xDebugTexture;
sampler DebugTextureSampler = sampler_state { texture = <xDebugTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

struct VertexShaderInput
{
    float4 Position  : SV_POSITION;
	float3 Normal    : NORMAL;
};

struct VertexToPixel
{
	float4 Position  : SV_POSITION;
	float3 Normal    : NORMAL;
	float  Height    : TEXCOORD0;
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
	output.Height = mul(input.Position, xWorld).z / 90.0;

	return output;
}

PixelToFrame TerrainPS(VertexToPixel input)
{
	PixelToFrame output;

	float3 normal = normalize(input.Normal);

	float lightingFactor = saturate(dot(normal, -xLightDirection));

	output.Colour = xTerrainColour * input.Height;

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

//------- Technique: Contours --------

VertexToPixel ContourVS(float4 Position : SV_POSITION)
{
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	VertexToPixel output;

	output.Position = mul(Position, preWorldViewProjection);

	return output;
}

PixelToFrame ContourPS(VertexToPixel input)
{
	PixelToFrame output;

	output.Colour = xContourColour;

	return output;
}

technique Contour
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ContourVS();
		PixelShader = compile ps_4_0 ContourPS();
	}
}
