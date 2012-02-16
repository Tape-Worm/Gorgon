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

// TODO:  Add constant buffers for transformation and other sprite attributes.

// Our default vertex shader.
GorgonSpriteVertex GorgonVertexShader(GorgonSpriteVertex vertex)
{
	GorgonSpriteVertex output = vertex;

	output.position = mul(ViewProjection, output.position);

	return output;
}

// Our default pixel shader with textures.
float4 GorgonPixelShaderTexture(GorgonSpriteVertex vertex) : SV_Target
{
   // TODO: Create function(s) for alpha testing.
   return _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
}

// Our default pixel shader without textures.
float4 GorgonPixelShaderNoTexture(GorgonSpriteVertex vertex)  : SV_Target
{
   // TODO: Create function(s) for alpha testing.
   return vertex.color;
}
