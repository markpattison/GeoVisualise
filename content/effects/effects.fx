float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightDirection;
float4 xContourColour;
float4 xSpotHeightColour;

texture xDebugTexture;
sampler DebugTextureSampler = sampler_state { texture = <xDebugTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

texture xTerrainHeightTexture;
sampler TerrainHeightTextureSampler = sampler_state { texture = <xTerrainHeightTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };

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
	output.Height = (mul(input.Position, xWorld).z - 70.) / 90.0;

	return output;
}

PixelToFrame TerrainPS(VertexToPixel input)
{
	PixelToFrame output;

	float3 normal = normalize(input.Normal);

	float lightingFactor = saturate(dot(normal, -xLightDirection));
	float2 texCoords = float2(input.Height, 0.0);
	output.Colour = tex2D(TerrainHeightTextureSampler, texCoords);

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

	VertexToPixel output = (VertexToPixel)0;

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

//------- Technique: TerrainHeight --------

struct TerrainHeightVertexShaderInput
{
    float4 Position  : SV_POSITION;
	float4 Colour    : COLOR;
	float2 TexCoords : TEXCOORD0;
};

struct TerrainHeightVertexToPixel
{
	float4 Position  : SV_POSITION;
	float4 Colour    : COLOR;
};

TerrainHeightVertexToPixel TerrainHeightVS(TerrainHeightVertexShaderInput input)
{
	TerrainHeightVertexToPixel output;

	output.Position = input.Position;
	output.Colour = input.Colour;

	return output;
}

PixelToFrame TerrainHeightPS(TerrainHeightVertexToPixel input)
{
	PixelToFrame output;

	output.Colour = input.Colour;

	return output;
}

technique TerrainHeight
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 TerrainHeightVS();
		PixelShader = compile ps_4_0 TerrainHeightPS();
	}
}

//------- Technique: Debug --------

struct DebugVertexShaderInput
{
    float4 Position  : SV_POSITION;
	float2 TexCoords : TEXCOORD0;
};

struct DebugVertexToPixel
{
	float4 Position  : SV_POSITION;
	float2 TexCoords : TEXCOORD0;
};

DebugVertexToPixel DebugVS(DebugVertexShaderInput input)
{
	DebugVertexToPixel output;

	output.Position = input.Position;
	output.TexCoords = input.TexCoords;

	return output;
}

PixelToFrame DebugPS(DebugVertexToPixel PSIn)
{
	PixelToFrame output;

	output.Colour = tex2D(DebugTextureSampler, PSIn.TexCoords);

	return output;
}

technique Debug
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 DebugVS();
		PixelShader = compile ps_4_0 DebugPS();
	}
}