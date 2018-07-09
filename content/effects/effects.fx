float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightDirection;
float4 xTerrainColour;
float4 xContourColour;
float4 xSpotHeightColour;

texture xTerrain;
sampler TerrainSampler = sampler_state { texture = <xTerrain>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };

texture xDebugTexture;
sampler DebugTextureSampler = sampler_state { texture = <xDebugTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

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
	float2 TexCoords : TEXCOORD0;
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
	output.TexCoords = input.TexCoords;

	return output;
}

PixelToFrame TerrainPS(VertexToPixel input)
{
	PixelToFrame output;

	float4 colour = tex2D(TerrainSampler, input.TexCoords);

	float3 normal = normalize(input.Normal);

	float lightingFactor = saturate(dot(normal, -xLightDirection));

	output.Colour = colour * (lightingFactor + 0.2);

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

//------- Technique: TerrainTexture --------

VertexToPixel TerrainTextureVS(VertexShaderInput input)
{
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	VertexToPixel output;

	output.Position = mul(input.Position, preWorldViewProjection);
	output.Normal.xyz = mul(input.Position, xWorld).xyz;
	output.TexCoords = input.TexCoords;

	return output;
}

PixelToFrame TerrainTexturePS(VertexToPixel input)
{
	PixelToFrame output;

	float height = (input.Normal.z - 0.0) / 90.0;

	output.Colour = xTerrainColour * height;

	return output;
}

technique TerrainTexture
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 TerrainTextureVS();
		PixelShader = compile ps_4_0 TerrainTexturePS();
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

//------- Technique: Debug --------

VertexToPixel DebugVS(float4 inPos : SV_POSITION, float2 inTexCoords : TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;

	Output.Position = inPos;
	Output.TexCoords = inTexCoords;

	return Output;
}

PixelToFrame DebugPS(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;

	Output.Colour = tex2D(DebugTextureSampler, PSIn.TexCoords);

	return Output;
}

technique Debug
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 DebugVS();
		PixelShader = compile ps_4_0 DebugPS();
	}
}