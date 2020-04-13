// This is adopted from the nvidia film grain shader.
//
// To learn more about shading, shaders, and to bounce ideas off other shader
//    authors and users, visit the NVIDIA Shader Library Forums at:
//
//    http://developer.nvidia.com/forums/

#GorgonInclude "Gorgon2DShaders"

// Additional effect texture buffer.
Texture2D _filmEffectTexture : register(t1);
SamplerState _gorgonFilmGrainSampler : register(s1);		// Sampler used for film grain random texture.

cbuffer FilmGrainTiming : register(b1)
{
	float filmGrainTime = 0.0f;								// Delta time - Used to animate the effect.
};

cbuffer FilmGrainScratch : register(b2)
{
	float filmGrainSpeed = 0.003f;							// Speed (larger values means longer scratches)	
	float filmGrainScrollSpeed = 0.01f;						// Side scrolling speed.
	float filmGrainIntensity = 0.49f;						// Scratch intensity
	float filmGrainScratchWidth = 0.01f;					// Scratch width.
}

cbuffer FilmGrainSepia : register(b3)
{
	float4 filmGrainLight = float4(1, 0.9f, 0.65f, 1);		// Light coloring.
	float4 filmGrainDark = float4(0.2f, 0.102f, 0, 1);		// Dark coloring.
	float filmGrainDesaturation = 0.0f;						// Desaturation.
	float filmGrainTone = 0.5f;								// Tone.	
}

// Function to make a sepia tone out of a specific color.
float4 makeSepia(float4 color)
{
	float3 newColor = filmGrainLight.rgb * color.rgb;
	float3 grayTransfer = float3(0.3f, 0.59f, 0.11f);
	float gray = dot(grayTransfer, newColor);
	float3 mute = lerp(newColor, gray.xxx, filmGrainDesaturation);
	float3 sepia = lerp(filmGrainDark.rgb, filmGrainLight.rgb, gray);

	return float4(lerp(mute, sepia, filmGrainTone), color.a);
}

// Our default pixel shader to apply a film grain effect.
float4 GorgonPixelShaderFilmGrain(GorgonSpriteVertex vertex) : SV_Target
{
	float scanLine = filmGrainTime * filmGrainSpeed;
	float side = filmGrainTime * filmGrainScrollSpeed;

    float4 texel = _gorgonTexture.Sample(_gorgonSampler, vertex.uv.xyz) * vertex.color;
	
	float2 randomValue = float2(vertex.uv.x + side, scanLine);
	float scratch = _filmEffectTexture.Sample(_gorgonFilmGrainSampler, randomValue).x;
    
    scratch = 2.0f * (scratch - filmGrainIntensity) / filmGrainScratchWidth;
    scratch = 1.0f - abs(1.0f - scratch);
    scratch = max(0,scratch);
    
    return makeSepia(texel + float4(scratch.xxx,0));
}