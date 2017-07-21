// Texture and sampler for blitting a texture.
Texture2D _bltTexture : register(t0);
SamplerState _bltSampler : register(s0);

// Our default blitting vertex.
struct GorgonBltVertex
{
   float4 position : SV_POSITION;
   float2 uv : TEXCOORD;
   float4 color : COLOR;
};

// The transformation matrices (for vertex shader).
cbuffer GorgonBltWorldViewProjection : register(b0)
{
	float4x4 WorldViewProjection;
}

// Our vertex shader for blitting textures.
GorgonBltVertex GorgonBltVertexShader(GorgonBltVertex vertex)
{
	GorgonBltVertex output = vertex;

	output.position = mul(WorldViewProjection, output.position);

	return output;
}

// Our pixel shader for blitting textures.
float4 GorgonBltPixelShader(GorgonBltVertex vertex) : SV_Target
{
	return _bltTexture.Sample(_bltSampler, vertex.uv) * vertex.color;
}

#ifdef GAUSS_BLUR_EFFECT

// Gaussian blur effect starts here.

// Adjust the weighting and offsets so we can pack our float values in tightly (much less bandwidth when updating the CB).
#define MAX_WEIGHT_SIZE (((MAX_KERNEL_SIZE * 4) + 15) & (~15)) / 16
#define MAX_OFFSET_SIZE (((MAX_KERNEL_SIZE * 8) + 15) & (~15)) / 16

// Gaussian blur kernal (for the gaussian blur shader).
cbuffer GorgonGaussKernelData : register(b0)
{
	float4 _offsets[MAX_OFFSET_SIZE];
	float4 _weights[MAX_WEIGHT_SIZE];
	int _blurRadius;
}

// Gaussian blur pass data (for the gaussian blur shader).
cbuffer GorgonGaussPassSettings : register(b1)
{
	int _passIndex;
}

// Function to gather the a single pass of the separable gaussian blur with alpha preservation.
float4 GorgonPixelShaderGaussBlurNoAlpha(GorgonBltVertex vertex) : SV_Target
{
	float4 blurSample = 0.0f;
	int kernelSize = (_blurRadius * 2) + 1;
	float texelAlpha = _bltTexture.Sample(_bltSampler, vertex.uv).a;

	[unroll]
	for (int i = 0; i < kernelSize; i++)
	{
		int arrayIndex = i / 4;
		int componentIndex = i % 4;
		float2 offset = _passIndex == 0 ? float2(_offsets[arrayIndex][componentIndex], 0) : float2(0, _offsets[arrayIndex][componentIndex]);
		float4 texSample = _bltTexture.Sample(_bltSampler, vertex.uv + offset);

		blurSample.rgb += texSample.rgb * _weights[arrayIndex][componentIndex];		
	}

	blurSample.a = texelAlpha;

	return saturate(blurSample);
}

// Function to gather the a single pass of the separable gaussian blur.
float4 GorgonPixelShaderGaussBlur(GorgonBltVertex vertex) : SV_Target
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
		float4 texSample = _bltTexture.Sample(_bltSampler, vertex.uv + offset);

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
