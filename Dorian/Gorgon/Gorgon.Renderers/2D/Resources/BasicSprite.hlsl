#define REJECT_ALPHA(alpha) if (alphaTestEnabled) clip((alpha <= alphaTestValueHi && alpha >= alphaTestValueLow) ? -1 : 1);
#define RANGE_BW(colorValue) (colorValue < oneBitRange.x || colorValue > oneBitRange.y) ? 0.0f : 1.0f;
#define MAX_KERNEL_SIZE 13

// Our default texture and sampler.
Texture2D _gorgonTexture : register(t0);
SamplerState _gorgonSampler : register(s0);

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

cbuffer GorgonGaussBlurStatic
{
	int blurRadius = 7;
	float _weights[MAX_KERNEL_SIZE];
}

cbuffer GorgonGaussBlur
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
		
	REJECT_ALPHA(color.a);

	return float4(1.0f - color.rgb, color.a);
}

// A pixel shader to sharpen the color on a texture.
float4 GorgonPixelShaderSharpenEmboss(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
	float alpha = color.a * vertex.color.a;
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
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
			
	REJECT_ALPHA(color.a);
		
	color.rgb = (oneBitUseAverage ? ((color.r + color.g + color.b) / 3.0f) : GorgonPixelShaderGrayScale(vertex).rgb);

	color.r = RANGE_BW(color.r);
	color.g = RANGE_BW(color.g);
	color.b = RANGE_BW(color.b);

	if (oneBitInvert)
		color.rgb = 1.0f - color;

	return color;
}

// Function to gather the a single pass of the separable gaussian blur.
float4 GorgonGaussBlur(GorgonSpriteVertex vertex) : SV_Target
{
	float4 sample = 0.0f;
	int kernelSize = (blurRadius * 2) + 1;

	[unroll]	
	for (int i = 0; i < kernelSize; i++)
		sample += _gorgonTexture.Sample(_gorgonSampler, vertex.uv + _offsets[i]) * vertex.color * _weights[i];

	return sample;
}