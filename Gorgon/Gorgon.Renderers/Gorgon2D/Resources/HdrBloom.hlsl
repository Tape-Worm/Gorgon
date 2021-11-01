#GorgonInclude "Gorgon2DShaders"

/*
* The shaders used for our GorgonBloomEffect.
*
* This effect is based on the work of Jorge Jimenez from his SIGGRAPH 2014 presentation on advances in realtime rendering.
* The website for this presentation is located here: http://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare
* 
* The PowerPoint presentation is here (it's a big'un): http://www.iryoku.com/downloads/Next-Generation-Post-Processing-in-Call-of-Duty-Advanced-Warfare-v18.pptx
*/

// Our default texture and sampler.
Texture2DArray _blurTexture : register(t1);
SamplerState _blurSampler : register(s1);

Texture2DArray _dirtTexture : register(t2);
SamplerState _dirtSampler : register(s2);

// Parameters for our HDR shaders.
cbuffer HdrParams : register(b1)
{
	// Settings for the bright pass and down sampling.
	float4 _brightPassSettings;
	// Color for the bloom.
	float4 _bloomColor;
	// The blur amount (veil spread), bloom intensity and dirt intensity.
	float4 _blurAmountIntensity;
	// Transformation values for the dirt texture.
	float4 _dirtTransform;
}

// Parameters for texture settings.
cbuffer TextureSettings : register(b2)
{
	// The size of an individual texel.
	float2 _texelSize;
}

// Function to retrieve the texel color based on the threshold value and filtering curve applied.
float4 FilterColor(float4 color, float threshold, float3 curve)
{
    float brightness = max(color.r, max(color.g, color.b));
    float curveScale = clamp(brightness - curve.x, 0.0, curve.y);	
    curveScale = curve.z * curveScale * curveScale;

    color *= max(curveScale, brightness - threshold) / max(brightness, 1e-4f);

    return color;
}

// Function to upsample the texture data.
float4 UpSample(Texture2DArray t, SamplerState s, float2 uv)
{
#ifdef LOW_QUALITY_BLOOM
	return BoxSample4Tap(t, s, _texelSize, uv, _blurAmountIntensity.x * 0.5f);
#else
	return TentSample9Tap(t, s, _texelSize, uv, _blurAmountIntensity.x * 0.5f);
#endif
}


// Function to down sample the texture data.
float4 DownSample(Texture2DArray t, SamplerState s, float2 uv)
{
#ifdef LOW_QUALITY_BLOOM
	return BoxSample4Tap(t, s, _texelSize, uv, 1.0f);
#else
	return TentSample13Tap(t, s, _texelSize, uv);
#endif
}

static float _clamp = pow(65472.0f, 2.2f);

// Function to extract the bright texels from an image based on a threshold value.
float4 MaskPixels(float4 hdrTexel) 
{
	// TODO: Maybe apply clamping value to hdr texel.	
	hdrTexel = min(_clamp, hdrTexel);
	hdrTexel = FilterColor(hdrTexel, _brightPassSettings.x, _brightPassSettings.yzw);
	return hdrTexel;
}

/** Actual shaders. **/

// The shader used to upsample an image.
float4 UpSampleBlur(GorgonSpriteVertex vertex) : SV_Target
{
	float4 blurTexel = _blurTexture.Sample(_blurSampler, vertex.uv.xyz);	
	return UpSample(_gorgonTexture, _gorgonSampler, vertex.uv.xy) + blurTexel;
}

// The shader used to downsample an image.
float4 DownSampleBlur(GorgonSpriteVertex vertex) : SV_Target
{
	return DownSample(_gorgonTexture, _gorgonSampler, vertex.uv.xy);
}

// The shader used to filter the bright areas of the image and down sample the source texture data.
float4 FilterBrightPass(GorgonSpriteVertex vertex) : SV_Target
{
	float4 texel = DownSample(_gorgonTexture, _gorgonSampler, vertex.uv.xy);
	return MaskPixels(texel);
}

// The shader used to build up the final pass and composite our scene.
float4 FinalPass(GorgonSpriteVertex vertex) : SV_Target
{
	float3 hdrTexel = SRgbToLinear(_gorgonTexture.Sample(_gorgonSampler, vertex.uv.xyz).rgb);
	float3 blurTexel = UpSample(_blurTexture, _blurSampler, vertex.uv.xy);
	float4 dirtTexel = _dirtTexture.Sample(_dirtSampler, float3(vertex.uv.xy * _dirtTransform.zw + _dirtTransform.xy, 0));
	
	dirtTexel *= _blurAmountIntensity.z;	
	blurTexel *= _blurAmountIntensity.y;	
	hdrTexel += (blurTexel * float4(_bloomColor.rgb, 1)) + (blurTexel * dirtTexel);	
	return float4(LinearToSRgb(hdrTexel), 1.0f);	
}