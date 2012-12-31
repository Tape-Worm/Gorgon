// Our default texture and sampler.
Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);

// The transformation matrices.
cbuffer WorldViewProjection : register(b0)
{
	float4x4 WVP;
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

// Our default pixel shader.
float4 BoingerPS(BoingerVertex vertex) : SV_Target
{
	return _texture.Sample(_sampler, vertex.uv);
}

// Our default pixel shader.
float4 BoingerDiffusePS(BoingerVertex vertex) : SV_Target
{
	return float4(0, 0, 0, 0.5f);
}
