#GorgonInclude "Gorgon2DShaders"

// The look up texture.
Texture1D _specLut : register(t1);
SamplerState _specLutSampler : register(s1);

// Chromatic Aberration code adapted from presentation by Mikkel Gjoel and code by Erik Faye Lund (@kusma).
// https://github.com/playdeadgames/publications/tree/master/INSIDE
// https://github.com/kusma/vlee/blob/master/data/postprocess.fx

// Settings to pass to the shader.
cbuffer ChromaticAberrationSettings : register(b1)
{
	// The intensity of the effect, plus the width/height of the output render target.
	float4 _settings;
};

// Shader to perform chromatic aberration on a render target view.
float4 ChromaticAberration(GorgonSpriteVertex vertex) : SV_Target
{
	float2 start = vertex.uv.xy;
	float2 centeredUv = 2.0f * start - 1.0f;	
	// Find the ending of our sample range and scale it by vingetteing, this will make the effect less intense as it goes to the center of the render target.
	float2 end = start - centeredUv * dot(centeredUv, centeredUv) * _settings.x; 
	float2 range = end - start;
	// Figure out how many samples we'll have depending on the size of the current render target.
	int sampleCount = clamp(int(length(_settings.zw * (range * 0.5f))), 3, 16);	

	// The offset for each sample iteration.
	float2 delta = range / sampleCount;

	float3 sum = 0;
	float3 filteredSum = 0;	

	[unroll(16)]
    for (int i = 0; i < sampleCount; ++i)
	{
		float t = (i + 0.5f) / sampleCount;
		// Instead of spitting out the r, g and b of our input texture, we'll use a look up texture to define which color channel
		// is split out, and then multiply that by our input texture color value. This allows us to use sampling to give us color 
		// shifts instead of harsh edges.
		float3 specLutTexel = _specLut.SampleLevel(_specLutSampler, t, 0).rgb;
		float3 chromTexel = _gorgonTexture.SampleLevel(_gorgonSampler, float3(start, 0), 0).rgb;

		sum += specLutTexel * chromTexel;
		filteredSum += specLutTexel;
		start += delta;
	}

	return float4(sum / filteredSum, 1.0f);
}

// A simplified chromatic aberration effect that is applied to the whole screen.
float4 ChromaticAberrationSimple(GorgonSpriteVertex vertex) : SV_Target
{
	float r = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy - _settings.xy, vertex.uv.z)).r;
	float g = _gorgonTexture.Sample(_gorgonSampler, vertex.uv, 0).g;
	float b = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy + _settings.xy, vertex.uv.z)).b;

	return float4(r, g, b, 1.0f);
}