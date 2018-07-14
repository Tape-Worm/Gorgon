#define REJECT_ALPHA(alpha) if (alphaTestEnabled) clip((alpha <= alphaTestValueHi && alpha >= alphaTestValueLow) ? -1 : 1);
#define RANGE_BW(colorValue) (colorValue < oneBitRange.x || colorValue > oneBitRange.y) ? 0.0f : 1.0f;

// Our default texture and sampler.
Texture2DArray _gorgonTexture : register(t0);
SamplerState _gorgonSampler : register(s0);

// Additional effect texture buffer.
Texture2D _gorgonEffectTexture : register(t1);

// Our default sprite vertex.
struct GorgonSpriteVertex
{
   float4 position : SV_POSITION;
   float4 color : COLOR;
   float3 uv : TEXCOORD;
   float angle : ANGLE;
};

// The transformation matrices (for vertex shader).
cbuffer GorgonViewProjection : register(b0)
{
	float4x4 WorldViewProjection;
}

// Alpha test value (for pixel shader).
cbuffer GorgonAlphaTest : register(b0)
{
	bool alphaTestEnabled = false;
	float alphaTestValueLow = 0.0f;
	float alphaTestValueHi = 0.0f;
}

// Material.
cbuffer GorgonMaterial : register(b1)
{
	float4 matDiffuse;
	float4 matTextureTransform;
}

// Our default vertex shader.
GorgonSpriteVertex GorgonVertexShader(GorgonSpriteVertex vertex)
{
	GorgonSpriteVertex output = vertex;

	output.position = mul(WorldViewProjection, output.position);

	return output;
}

// Our default pixel shader with textures with alpha testing.
float4 GorgonPixelShaderTextured(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

	REJECT_ALPHA(color.a);
		
	return color;
}

#ifdef INVERSE_EFFECT
// Invert effect variables.
cbuffer GorgonInvertEffect : register(b1)
{
	bool invertUseAlpha = false;
}

// A pixel shader to invert the color on a texture.
float4 GorgonPixelShaderInvert(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

	if (invertUseAlpha)
	{
		color = 1.0f - color;
	}
	else
	{
		color = float4(1.0f - color.rgb, color.a);
	}
		
	REJECT_ALPHA(color.a);

	return color;
}
#endif

#ifdef BURN_DODGE_EFFECT
// Burn/dodge effect.
cbuffer GorgonBurnDodgeEffect : register(b1)
{
	bool burnDodgeUseDodge;
}

// Function to perform a linear image burn/dodge.
float4 GorgonPixelShaderLinearBurnDodge(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
	
	color.rgb = color.rgb * 2.0f;

	if (!burnDodgeUseDodge)
	{
		color.rgb = color.rgb - 1.0f;
	}

	REJECT_ALPHA(color.a);
	
	return saturate(color);
}

// Function to perform an image burn/dodge.
float4 GorgonPixelShaderBurnDodge(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
	
	if (burnDodgeUseDodge)
	{
		float3 invColor = float3(color.r < 1.0f ? 1.0f - color.r : 1.0f, 
								color.g < 1.0f ? 1.0f - color.g : 1.0f, 
								color.b < 1.0f ? 1.0f - color.b : 1.0f);

		color.r = min(color.r / invColor.r, 1.0f);
		color.g = min(color.g / invColor.g, 1.0f);
		color.b = min(color.b / invColor.b, 1.0f);		
	}
	else
	{
		color.r = color.r == 0 ? 0 : max((1.0f - ((1.0f - color.r) / color.r)), 0); 
		color.g = color.g == 0 ? 0 : max((1.0f - ((1.0f - color.g) / color.g)), 0); 
		color.b = color.b == 0 ? 0 : max((1.0f - ((1.0f - color.b) / color.b)), 0); 
	}

	REJECT_ALPHA(color.a);
	
	return saturate(color);
}
#endif

#ifdef GRAYSCALE_EFFECT
// A pixel shader that converts to gray scale.
float4 GorgonPixelShaderGrayScale(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

	REJECT_ALPHA(color.a);

	float grayValue = color.r * 0.3f + color.g * 0.59f + color.b * 0.11f;

	return float4(grayValue, grayValue, grayValue, color.a);
}
#endif

#ifdef ONEBIT_EFFECT
// 1 bit color effect.
cbuffer Gorgon1BitEffect : register(b1)
{	
	bool oneBitUseAverage;
	bool oneBitInvert;
	bool oneBitUseAlpha;
	float2 oneBitRange;
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
	{
		color = GorgonPixelShaderGrayScale(vertex);
	}

	color.r = RANGE_BW(color.r);
	color.g = RANGE_BW(color.g);
	color.b = RANGE_BW(color.b);

	if (oneBitUseAlpha)
	{
		color.a = RANGE_BW(color.a);
	}

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
#endif

#ifdef WAVE_EFFECT
// Wave effect variables.
cbuffer GorgonWaveEffect : register(b1)
{
	float waveAmplitude;
	float waveLength;
	float wavePeriod;
	float waveLengthScale;
	int waveType;
}

// A vertical wave effect pixel shader.
float4 GorgonPixelShaderWaveEffect(GorgonSpriteVertex vertex) : SV_Target
{
	float2 uv = vertex.uv;
	float4 color;
	float length = abs(1.0f - (waveLength / waveLengthScale)) * waveLength;
	float amp = waveAmplitude / 360.0f;
		
	if ((waveType == 0) || (waveType == 2))
	{
		uv.x += sin((uv.y + wavePeriod) * length) * amp;
	}

	if ((waveType == 1) || (waveType == 2))
	{
		uv.y += cos((uv.x + wavePeriod) * length) * amp;
	}
			
	color = _gorgonTexture.Sample(_gorgonSampler, float3(uv, vertex.uv.z)) * vertex.color;
	REJECT_ALPHA(color.a);
	return color;
}
#endif

#ifdef DISPLACEMENT_EFFECT
// Displacement effect variables.
cbuffer GorgonDisplacementEffect : register(b1)
{
	float4 displaceSizeAmount;
}

// The displacement shader encoder.
float2 GorgonPixelShaderDisplacementEncoder(float3 uv)
{
	float4 offset = _gorgonEffectTexture.Sample(_gorgonSampler, uv);

	float4 basisX = offset.x >= 0.5f ? float4(offset.x, 0.0f, 0.0f, 0) : float4(0.0f, 0.0f, -offset.x, 0.0f);
	float4 basisY = offset.y >= 0.5f ? float4(0.0f, offset.y, 0.0f, 0) : float4(0.0f, 0.0f, 0.0f, -offset.y);	
	float4 displacement = (basisX + basisY) * displaceSizeAmount.z;

	return (displacement.xy + displacement.zw) * displaceSizeAmount.xy;
}

// The displacement shader decoder.
float4 GorgonPixelShaderDisplacementDecoder(GorgonSpriteVertex vertex) : SV_Target
{	
	float2 offset = GorgonPixelShaderDisplacementEncoder(vertex.uv);		
	float4 color = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy + offset.xy, vertex.uv.z)) * vertex.color;

	REJECT_ALPHA(color.a);

	return color;
}
#endif

#ifdef GAUSS_BLUR_EFFECT
// Gaussian blur effect starts here.

// Adjust the weighting and offsets so we can pack our float values in tightly (much less bandwidth when updating the CB).
#define MAX_WEIGHT_SIZE (((MAX_KERNEL_SIZE * 4) + 15) & (~15)) / 16
#define MAX_OFFSET_SIZE (((MAX_KERNEL_SIZE * 8) + 15) & (~15)) / 16

// Gaussian blur kernal (for the gaussian blur shader).
cbuffer GorgonGaussKernelData : register(b1)
{
	float4 _offsets[MAX_OFFSET_SIZE];
	float4 _weights[MAX_WEIGHT_SIZE];
	float _blurRadius;
}

// Gaussian blur pass data (for the gaussian blur shader).
cbuffer GorgonGaussPassSettings : register(b2)
{
	int _passIndex;
}

// Function to gather a single pass of the separable gaussian blur with alpha preservation.
float4 GorgonPixelShaderGaussBlurNoAlpha(GorgonSpriteVertex vertex) : SV_Target
{
	float4 blurSample = 0.0f;
	int kernelSize = (_blurRadius * 2) + 1;

	[unroll]
	for (int i = 0; i < kernelSize; i++)
	{
		int arrayIndex = i / 4;
		int componentIndex = i % 4;
		float2 offset = _passIndex == 0 ? float2(_offsets[arrayIndex][componentIndex], 0) : float2(0, _offsets[arrayIndex][componentIndex]);
		float4 texSample = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy + offset, vertex.uv.z));

		blurSample.rgb += texSample.rgb * _weights[arrayIndex][componentIndex];	
		blurSample.a = texSample.a;
	}

	return saturate(blurSample);
}

// Function to gather a single pass of the separable gaussian blur.
float4 GorgonPixelShaderGaussBlur(GorgonSpriteVertex vertex) : SV_Target
{
	float4 blurSample = 0.0f;
	int kernelSize = (_blurRadius * 2) + 1;
	float totalWeight = 0.0f;

	[unroll]
	for (int i = 0; i < kernelSize; i++)
	{
		int arrayIndex = i / 4;
		int componentIndex = i % 4;
		float2 offset = _passIndex == 0 ? float2(_offsets[arrayIndex][componentIndex], 0) : float2(0, _offsets[arrayIndex][componentIndex]);
		float4 texSample = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy + offset, vertex.uv.z));

		// Skip blurring if there's no alpha.  If there's no alpha, then there's nothing to contribute to the blur.
		// We can't use clip() here because it causes smearing when animating the image.		
		if (texSample.a == 0)
		{			
			continue;
		}

		float newAlpha = _weights[arrayIndex][componentIndex] * texSample.a;

		blurSample.rgb += texSample.rgb * newAlpha;
		blurSample.a += newAlpha;

		totalWeight += newAlpha;
	}

	// If there's no weighting, then do nothing with this texel.
	return totalWeight != 0 ? saturate(float4(blurSample.rgb / totalWeight, blurSample.a)) : float4(blurSample.rgb, 0);
}
#endif

#ifdef SHARPEN_EMBOSS_EFFECT
// Sharpen/emboss effect variables.
cbuffer GorgonSharpenEmbossEffect : register(b1)
{
	float2 sharpEmbossTexelDistance;
	float sharpEmbossAmount;
}


// A pixel shader to sharpen the color on a texture.
float4 GorgonPixelShaderSharpen(GorgonSpriteVertex vertex) : SV_Target
{
	static float kernel[9] =
	{
		0.0f, -1.0f, 0.0f, 
		-1.0f, 5.0f, -1.0f, 
		0.0f, -1.0f, 0.0f
	};

	float2 offset[9] =
	{
		float2(-sharpEmbossTexelDistance.x, -sharpEmbossTexelDistance.y),
		float2(0.0, -sharpEmbossTexelDistance.y),
		float2(sharpEmbossTexelDistance.x, -sharpEmbossTexelDistance.y),
		float2(-sharpEmbossTexelDistance.x, 0.0),
		float2(0.0, 0.0),
		float2(sharpEmbossTexelDistance.x, 0.0),
		float2(-sharpEmbossTexelDistance.x, sharpEmbossTexelDistance.y),
		float2(0.0, sharpEmbossTexelDistance.y),
		float2(sharpEmbossTexelDistance.x, sharpEmbossTexelDistance.y)
	};

	float4 result = 0;
	
	for (int i = 0; i < 9; i++) 
	{
		float4 color = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy + offset[i], vertex.uv.z));
		result += float4((color.rgb * sharpEmbossAmount) * kernel[i], color.a);
	}

	return result;
}

// A pixel shader to sharpen the color on a texture.
float4 GorgonPixelShaderEmboss(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
	float alpha = color.a;
	float amount = 3.5f * sharpEmbossAmount;
	float3 texelPosition;
			
	REJECT_ALPHA(alpha);
	
	texelPosition = float3(vertex.uv.xy + sharpEmbossTexelDistance, vertex.uv.z);
	color -= (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * amount);
	texelPosition = float3(vertex.uv.xy - sharpEmbossTexelDistance, vertex.uv.z);
	color += (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * amount);

	color.a = alpha;
	color.rgb = (color.r + color.g + color.b) / 3.0f;
	
	return color;
}
#endif

#ifdef POSTERIZE_EFFECT
// Posterize effect variables.
cbuffer GorgonPosterizeEffect : register(b1)
{
	bool posterizeUseAlpha;
	float posterizeExponent;
	int posterizeBits;
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
#endif

#ifdef SOBEL_EDGE_EFFECT
// Sobel edge detection effect variables.
cbuffer GorgonSobelEdgeDetectEffect : register(b1)
{
	float2 sobelOffset = float2(0, 0);
	float sobelThreshold = 0.75f;
	float4 sobelLineColor = float4(0, 0, 0, 1);
}

// Function to perform a sobel edge detection.
float4 GorgonPixelShaderSobelEdge(GorgonSpriteVertex vertex) : SV_Target
{
	float4 s00 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + -sobelOffset);
	float4 s01 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( 0,   -sobelOffset.y));
	float4 s02 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( sobelOffset.x, -sobelOffset.y));

	float4 s10 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2(-sobelOffset.x,  0));
	float4 s12 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( sobelOffset.x,  0));

	float4 s20 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2(-sobelOffset.x,  sobelOffset.y));
	float4 s21 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + float2( 0,    sobelOffset.y));
	float4 s22 = _gorgonTexture.Sample(_gorgonSampler, vertex.uv + sobelOffset);

	float4 sobelX = s00 + 2 * s10 + s20 - s02 - 2 * s12 - s22;
	float4 sobelY = s00 + 2 * s01 + s02 - s20 - 2 * s21 - s22;

	float4 edgeSqr = sobelX * sobelX + sobelY * sobelY;
	float4 color = (1 - float4(edgeSqr.r <= sobelThreshold,
							edgeSqr.g <= sobelThreshold,
							edgeSqr.b <= sobelThreshold,
							0));

	if ((color.r > 0) || (color.g > 0) || (color.b > 0))
		color = sobelLineColor;
	else
		color = 0;
	
	REJECT_ALPHA(color.a);

	return color;
}
#endif

// Our default pixel shader with textures, alpha testing and materials.
/*float4 GorgonPixelShaderTexturedMaterial(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, (vertex.uv * matTextureTransform.zw) + matTextureTransform.xy) * vertex.color * matDiffuse;

	REJECT_ALPHA(color.a);
		
	return color;
}

// Our default pixel shader with diffuse, alpha testing and materials.
float4 GorgonPixelShaderDiffuseMaterial(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = vertex.color * matDiffuse;

	REJECT_ALPHA(color.a);
		
	return color;
}

// Our default pixel shader without textures with alpha testing.
float4 GorgonPixelShaderDiffuse(GorgonSpriteVertex vertex)  : SV_Target
{
   REJECT_ALPHA(vertex.color.a);

   return vertex.color;
}

*/