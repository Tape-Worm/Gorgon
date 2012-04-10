#define REJECT_ALPHA(alpha) if (alphaTestEnabled) clip((alpha <= alphaTestValueHi && alpha >= alphaTestValueLow) ? -1 : 1);
#define RANGE_BW(colorValue) (colorValue < oneBitRange.x || colorValue > oneBitRange.y) ? 0.0f : 1.0f;
#define MAX_KERNEL_SIZE 13

// Our default texture and sampler.
Texture2D _gorgonTexture : register(t0);
SamplerState _gorgonSampler : register(s0);

// Additional effect texture buffer.
Texture2D<float4> _gorgonEffectTexture : register(t1);
SamplerState _gorgonEffectSampler : register(s1);

// Our default sprite vertex.
struct GorgonSpriteVertex
{
   float4 position : SV_POSITION;
   float4 color : COLOR;
   float2 uv : TEXCOORD;
};

// The transformation matrices.
cbuffer GorgonViewProjection : register(b0)
{
	float4x4 ViewProjection;
}

// Alpha test value.
cbuffer GorgonAlphaTest : register(b1)
{
	bool alphaTestEnabled = false;
	float alphaTestValueLow = 0.0f;
	float alphaTestValueHi = 0.0f;
}

// Wave effect variables.
cbuffer GorgonWaveEffect
{
	float waveAmplitude = 0.0f;
	float waveLength = 0.0f;
	float wavePeriod = 0.0f;
	int waveType = 0;
}

// Gaussian blur variables.
cbuffer GorgonGaussBlurEffectStatic
{
	int blurRadius = 7;
	float _weights[MAX_KERNEL_SIZE];
}

cbuffer GorgonGaussEffectBlur
{
	float2 _offsets[MAX_KERNEL_SIZE];
}

// Sharpen/emboss effect variables.
cbuffer GorgonSharpenEmbossEffect
{
	float2 sharpEmbossTexelDistance = 0.0f;
	float sharpEmbossAmount = 10.0f;
	bool useEmboss = false;
}

// 1 bit color effect.
cbuffer Gorgon1BitEffect
{	
	float2 oneBitRange = float2(0.2f, 0.8f);
	bool oneBitUseAverage = false;
	bool oneBitInvert = false;
	bool oneBitUseAlpha = false;
}

// Invert effect variables.
cbuffer GorgonInvertEffect
{
	bool invertUseAlpha = false;
}

// Posterize effect variables.
cbuffer GorgonPosterizeEffect
{
	int posterizeBits = 8;
	float posterizeExponent = 1.5f;
	bool posterizeUseAlpha = false;
}

// Sobel edge detection effect variables.
cbuffer GorgonSobelEdgeDetectEffect
{
	float4 sobelLineColor = float4(0, 0, 0, 1);
	float sobelOffset = 0.0f;
	float sobelThreshold = 0.75f;
	bool sobelUseLineColor = true;
}

// Burn/dodge effect.
cbuffer GorgonBurnDodgeEffect
{
	bool burnDodgeUseDodge = false;
	bool burnDodgeLinear = false;
}

// Our default vertex shader.
GorgonSpriteVertex GorgonVertexShader(GorgonSpriteVertex vertex)
{
	GorgonSpriteVertex output = vertex;

	output.position = mul(ViewProjection, output.position);

	return output;
}

// Our default pixel shader with textures with alpha testing.
float4 GorgonPixelShaderTextured(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

	REJECT_ALPHA(color.a);
		
	return color;
}

// Our default pixel shader without textures with alpha testing.
float4 GorgonPixelShaderDiffuse(GorgonSpriteVertex vertex)  : SV_Target
{
   REJECT_ALPHA(vertex.color.a);

   return vertex.color;
}

// These functions are some basic effects for textured images.

// A pixel shader that converts to gray scale.
float4 GorgonPixelShaderGrayScale(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

	REJECT_ALPHA(color.a);

	float grayValue = color.r * 0.3f + color.g * 0.59f + color.b * 0.11f;

	return float4(grayValue, grayValue, grayValue, color.a);
}

// A vertical wave effect pixel shader.
float4 GorgonPixelShaderWaveEffect(GorgonSpriteVertex vertex) : SV_Target
{
	float2 uv = vertex.uv;
	float4 color;
	
	if (waveType == 0.0f)
		uv.x += sin((uv.y + wavePeriod) * waveLength) * waveAmplitude;

	if (waveType == 1.0f)
		uv.y += cos((uv.x + wavePeriod) * waveLength) * waveAmplitude;

	if (waveType == 2.0f)
	{
		uv.x += sin((uv.y + wavePeriod) * waveLength) * waveAmplitude;
		uv.y += cos((uv.x + wavePeriod) * waveLength) * waveAmplitude;
	}
			
	color = _gorgonTexture.Sample(_gorgonSampler, uv) * vertex.color;
	REJECT_ALPHA(color.a);
	return color;
}

// A pixel shader to invert the color on a texture.
float4 GorgonPixelShaderInvert(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

	if (invertUseAlpha)
		color = 1.0f - color;
	else
		color = float4(1.0f - color.rgb, color.a);
		
	REJECT_ALPHA(color.a);

	return color;
}

// A pixel shader to sharpen the color on a texture.
float4 GorgonPixelShaderSharpenEmboss(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
	float alpha = color.a;
	float2 texelPosition;
			
	REJECT_ALPHA(alpha);
	
	texelPosition = vertex.uv + sharpEmbossTexelDistance;
	color -= (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * sharpEmbossAmount);
	texelPosition = vertex.uv - sharpEmbossTexelDistance;
	color += (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * sharpEmbossAmount);

	if (useEmboss)
		color.rgb = (color.r + color.g + color.b) / 3.0f;

	color.a = alpha;
	return color;
}

// A pixel shader to show the texture data as 1 bit data.
float4 GorgonPixelShader1Bit(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = 0;
	
	if (oneBitUseAverage)
	{
		color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
		color.rgb = (color.r + color.g + color.b) / 3.0f;
	}
	else
		color = GorgonPixelShaderGrayScale(vertex);

	color.r = RANGE_BW(color.r);
	color.g = RANGE_BW(color.g);
	color.b = RANGE_BW(color.b);

	if (oneBitUseAlpha)
		color.a = RANGE_BW(color.a);

	if (oneBitInvert)
	{
		if (oneBitUseAlpha)
			color = 1.0f - color;
		else
			color.rgb = 1.0f - color.rgb;
	}

	REJECT_ALPHA(color.a);

	return color;
}

// Function to gather the a single pass of the separable gaussian blur.
float4 GorgonPixelShaderGaussBlur(GorgonSpriteVertex vertex) : SV_Target
{
	float4 sample = 0.0f;
	int kernelSize = (blurRadius * 2) + 1;

	[unroll]	
	for (int i = 0; i < kernelSize; i++)
		sample += _gorgonTexture.Sample(_gorgonSampler, vertex.uv + _offsets[i]) * vertex.color * _weights[i];

	return sample;
}

// Function to posterize texture data.
float4 GorgonPixelShaderPosterize(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

	if (!posterizeUseAlpha)	
		color.rgb = pow(floor((pow(color.rgb, posterizeExponent) * posterizeBits)) / posterizeBits, 1.0f / posterizeExponent);
	else
		color = pow(floor((pow(color, posterizeExponent) * posterizeBits)) / posterizeBits, 1.0f / posterizeExponent);

	REJECT_ALPHA(color.a);

	return color;
}

// Function to perform a sobel edge detection.
float4 GorgonPixelShaderSobelEdge(GorgonSpriteVertex vertex) : SV_Target
{
	float4 s00 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2(-sobelOffset, -sobelOffset));
	float4 s01 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( 0,   -sobelOffset));
	float4 s02 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( sobelOffset, -sobelOffset));

	float4 s10 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2(-sobelOffset,  0));
	float4 s12 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( sobelOffset,  0));

	float4 s20 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2(-sobelOffset,  sobelOffset));
	float4 s21 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( 0,    sobelOffset));
	float4 s22 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( sobelOffset,  sobelOffset));

	float4 sobelX = s00 + 2 * s10 + s20 - s02 - 2 * s12 - s22;
	float4 sobelY = s00 + 2 * s01 + s02 - s20 - 2 * s21 - s22;

	float4 edgeSqr = sobelX * sobelX + sobelY * sobelY;
	float4 color = 1 - dot(edgeSqr, 0.25f);

	if ((color.r < sobelThreshold) && (color.g < sobelThreshold) && (color.b < sobelThreshold))
	{
		color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

		if (sobelUseLineColor)
			color.rgb = sobelLineColor.rgb;		
	}
	else
		color.a = 0;

	REJECT_ALPHA(color.a);

	return color;
}

// Function to perform an image burn/dodge.
float4 GorgonPixelShaderBurnDodge(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
	
	if (!burnDodgeLinear)
	{
		if (burnDodgeUseDodge)
			color.rgb = color.rgb / (1.0f - color.rgb);
		else
			color.rgb = 1.0f - (1.0f - color.rgb) / color.rgb;
	}
	else
	{
		color.rgb *= 2;

		if (!burnDodgeUseDodge)
			color.rgb = color.rgb - 1;
	}

	REJECT_ALPHA(color.a);
	
	return color;
}

// Displacement effect variables.
cbuffer GorgonDisplacementEffect
{
	float4 displaceSizeAmount = 0;
}

// The displacement shader encoder.
float2 GorgonPixelShaderDisplacementEncoder(float2 uv)
{
	float4 offset = _gorgonEffectTexture.Sample(_gorgonEffectSampler, uv);

	float4 basisX = offset.x >= 0.5f ? float4(offset.x, 0.0f, 0.0f, 0) : float4(0.0f, 0.0f, -offset.x, 0.0f);
	float4 basisY = offset.y >= 0.5f ? float4(0.0f, offset.y, 0.0f, 0) : float4(0.0f, 0.0f, 0.0f, -offset.y);	
	float4 displacement = (basisX + basisY) * displaceSizeAmount.z;

	return (displacement.xy + displacement.zw) * displaceSizeAmount.xy;
}

// The displacement shader decoder.
float4 GorgonPixelShaderDisplacementDecoder(GorgonSpriteVertex vertex) : SV_Target
{	
	float2 offset = GorgonPixelShaderDisplacementEncoder(vertex.uv);		
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + offset.xy) * vertex.color;

	REJECT_ALPHA(color.a);

	return color;
}