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

// The transformation matrix.
cbuffer GorgonTransformMatrix
{
	float4x4 worldViewProjection;
}

// TODO:  Add constant buffers for transformation and other sprite attributes.

// Our default vertex shader.
GorgonSpriteVertex SpriteVertexShader(GorgonSpriteVertex vertex)
{
	GorgonSpriteVertex output = vertex;

	output.position = mul(worldViewProjection, vertex.position);

	return output;
}

// Our default pixel shader with textures.
float4 SpritePixelShaderTexture(GorgonSpriteVertex vertex) : SV_Target
{
   // TODO: Create function(s) for alpha testing.
   return _gorgonTexture.Sample(_gorgonSampler, vertex.uv) * vertex.color;
}

// Our default pixel shader without textures.
float4 SpritePixelShaderNoTexture(GorgonSpriteVertex vertex)  : SV_Target
{
   // TODO: Create function(s) for alpha testing.
   return vertex.color;
}
