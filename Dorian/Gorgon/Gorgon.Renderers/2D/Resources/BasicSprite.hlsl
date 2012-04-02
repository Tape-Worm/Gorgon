#define REJECT_ALPHA(alpha) if (alphaTestEnabled) clip((alpha <= alphaTestValueHi && alpha >= alphaTestValueLow) ? -1 : 1);
#define RANGE_BW(colorValue) (colorValue < oneBitRange.x || colorValue > oneBitRange.y) ? 0.0f : 1.0f;

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
	float waveLength = 20.0f;
	float wavePeriod = 0.0f;
}

// Sharpen/emboss effect variables.
cbuffer GorgonSharpenEmbossEffect
{
	float sharpEmbossAmount = 10.0f;
	float2 sharpEmbossTexelDistance = 0.0f;
}

// 1 bit color effect.
cbuffer Gorgon1BitEffect
{	
	float2 oneBitRange = float2(0.2f, 0.8f);
	bool oneBitUseAverage = false;
}

// Quick blur effect.
cbuffer GorgonQuickBlurEffect
{
	float2 quickBlurTexelDistance = 0.0f;
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
float4 GorgonPixelShaderVWaveEffect(GorgonSpriteVertex vertex) : SV_Target
{
	float2 uv = vertex.uv;
	float4 color;
	
	uv.x += sin((uv.y + wavePeriod) * waveLength) * waveAmplitude;
		
	color = _gorgonTexture.Sample(_gorgonSampler, uv) * vertex.color;
	REJECT_ALPHA(color.a);
	return color;
}

// A horizontal wave effect pixel shader.
float4 GorgonPixelShaderHWaveEffect(GorgonSpriteVertex vertex) : SV_Target
{
	float2 uv = vertex.uv;
	float4 color;
	
	uv.y += cos((uv.x + wavePeriod) * waveLength) * waveAmplitude;
		
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
float4 GorgonPixelShaderSharpen(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
	float alpha = color.a * vertex.color.a;
	float2 texelPosition;
			
	REJECT_ALPHA(alpha);
	
	texelPosition = vertex.uv + sharpEmbossTexelDistance;
	color -= (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * sharpEmbossAmount);
	texelPosition = vertex.uv - sharpEmbossTexelDistance;
	color += (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * sharpEmbossAmount);
	color.a = alpha;
	return alpha;
}

// A pixel shader to sharpen the color on a texture.
float4 GorgonPixelShaderEmboss(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = 0.5f;
	float alpha = _gorgonTexture.Sample(_gorgonSampler, vertex.uv).a * vertex.color.a;
	float2 texelPosition;
			
	REJECT_ALPHA(alpha);
	
	texelPosition = vertex.uv + sharpEmbossTexelDistance;
	color -= (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * sharpEmbossAmount);
	texelPosition = vertex.uv - sharpEmbossTexelDistance;
	color += (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * sharpEmbossAmount);

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

	return color;
}

// A pixel shader to perform a quick blur on an image.
float4 GorgonPixelShaderQuickBlur(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv);
	float2 texelDistance = 0.0f;
	float alpha = color.a * vertex.color.a;
			
	REJECT_ALPHA(alpha);
		
	texelDistance = vertex.uv - quickBlurTexelDistance;
	color += _gorgonTexture.Sample(_gorgonSampler, texelDistance);
	texelDistance = float2(vertex.uv.x - quickBlurTexelDistance.x, vertex.uv.y);
	color += _gorgonTexture.Sample(_gorgonSampler, texelDistance);
	texelDistance = float2(vertex.uv.x - quickBlurTexelDistance.x, vertex.uv.y - quickBlurTexelDistance.y);
	color += _gorgonTexture.Sample(_gorgonSampler, texelDistance);

	texelDistance = float2(vertex.uv.x, vertex.uv.y - quickBlurTexelDistance.y);
	color += _gorgonTexture.Sample(_gorgonSampler, texelDistance);
	texelDistance = float2(vertex.uv.x + quickBlurTexelDistance.x, vertex.uv.y - quickBlurTexelDistance.y);
	color += _gorgonTexture.Sample(_gorgonSampler, texelDistance);

	texelDistance = vertex.uv + quickBlurTexelDistance;
	color += _gorgonTexture.Sample(_gorgonSampler, texelDistance);
	texelDistance = float2(vertex.uv.x, vertex.uv.y + quickBlurTexelDistance.y);
	color += _gorgonTexture.Sample(_gorgonSampler, texelDistance);
	texelDistance = float2(vertex.uv.x + quickBlurTexelDistance.x, vertex.uv.y);
	color += _gorgonTexture.Sample(_gorgonSampler, texelDistance);

	color = (color * vertex.color) / 9.0f;

	return float4(color.rgb, alpha);
}