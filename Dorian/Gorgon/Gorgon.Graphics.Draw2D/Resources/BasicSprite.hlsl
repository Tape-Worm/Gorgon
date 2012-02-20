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
cbuffer GorgonViewProjection
{
	float4x4 ViewProjection;
}

// Alpha test value.
cbuffer GorgonRenderable
{
	bool alphaTestEnabled;
	float alphaTestValueLow;
	float alphaTestValueHi;
}

// Our default vertex shader.
GorgonSpriteVertex GorgonVertexShader(GorgonSpriteVertex vertex)
{
	GorgonSpriteVertex output = vertex;

	output.position = mul(ViewProjection, output.position);

	return output;
}

// Our default pixel shader with textures with alpha testing.
float4 GorgonPixelShaderTextureAlphaTest(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;

	if (alphaTestEnabled)
		clip((color.a <= alphaTestValueHi && color.a >= alphaTestValueLow) ? -1 : 1);		
		
	return color;
}

// Our default pixel shader without textures with alpha testing.
float4 GorgonPixelShaderNoTextureAlphaTest(GorgonSpriteVertex vertex)  : SV_Target
{
	if (alphaTestEnabled)
		clip((vertex.color.a <= alphaTestValueHi && vertex.color.a >= alphaTestValueLow) ? -1 : 1);		

   return vertex.color;
}

// Our default pixel shader with textures.
float4 GorgonPixelShaderTexture(GorgonSpriteVertex vertex) : SV_Target
{
	return _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
}

// Our default pixel shader without textures.
float4 GorgonPixelShaderNoTexture(GorgonSpriteVertex vertex)  : SV_Target
{
   return vertex.color;
}
