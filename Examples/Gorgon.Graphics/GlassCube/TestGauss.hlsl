//#define MAX_KERNEL_SIZE 26
// Adjust the weighting and offsets so we can pack our float values in tightly (much less bandwidth when updating the CB).
#define MAX_WEIGHT_SIZE (((MAX_KERNEL_SIZE * 4) + 15) & (~15)) / 16
#define MAX_OFFSET_SIZE (((MAX_KERNEL_SIZE * 8) + 15) & (~15)) / 16

// Pull in the blitter shader functions.
#GorgonInclude "__Gorgon_TextureBlitter_Shader__"

// Gaussian blur kernal.
cbuffer GorgonGaussKernelData : register(b0)
{
	float4 _offsets[MAX_OFFSET_SIZE];
	float4 _weights[MAX_WEIGHT_SIZE];
	int _blurRadius;
}

// Gaussian blur data.
cbuffer GorgonGaussPassSettings : register(b1)
{
	int _passIndex;
	int _preserveAlpha;
}

// Our pixel shader for blitting textures.
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

		// Kill this pixel if we don't have any alpha (nothing would be visible anyway).
		if (texSample.a == 0)
		{
			continue;
		}

		float newAlpha = _weights[arrayIndex][componentIndex] * texSample.a;

		//blurSample += texSample * _weights[arrayIndex][componentIndex];
		blurSample.rgb += texSample.rgb * newAlpha;
		blurSample.a += newAlpha;

		totalWeight += newAlpha;
	}

	blurSample.rgb /= totalWeight;

	return saturate(blurSample);
}
