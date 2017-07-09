// Our default texture and sampler.
Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);

// The transformation matrices.
cbuffer WorldViewProjection : register(b0)
{
	float4x4 WVP;
}

// The diffuse color.
cbuffer Material : register(b0)
{
	float4 Diffuse;
}

// Our vertex.
struct BoingerVertex
{
   float4 position : SV_POSITION;
   float2 uv : TEXCOORD;
};


// Our default vertex shader.
BoingerVertex BoingerVS(BoingerVertex vertex)
{
	BoingerVertex output = vertex;

	output.position = mul(output.position, WVP);

	return output;
}

// Our pixel shader that will render objects with textures.
float4 BoingerPS(BoingerVertex vertex) : SV_Target
{
	return _texture.Sample(_sampler, vertex.uv) * Diffuse;
}