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