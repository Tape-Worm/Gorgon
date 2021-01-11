// Texture and sampler for blitting a texture.
Texture2D _bltTexture : register(t0);
SamplerState _bltSampler : register(s0);

// Our default blitting vertex.
struct GorgonBltVertex
{
   float4 position : SV_POSITION;
   float4 color : COLOR;
   float2 uv : TEXCOORD;
};

// A vertex for full screen triangle blit.
struct GorgonFullScreenVertex
{
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD;
};

// The transformation matrices (for vertex shader).
cbuffer GorgonBltWorldViewProjection : register(b0)
{
	float4x4 WorldViewProjection;
}

// Our vertex shader for blitting textures.
GorgonBltVertex GorgonBltVertexShader(GorgonBltVertex vertex)
{
	GorgonBltVertex output = vertex;

	output.position = mul(WorldViewProjection, output.position);

	return output;
}

// Our pixel shader for blitting textures.
float4 GorgonBltPixelShader(GorgonBltVertex vertex) : SV_Target
{
	float4 result = _bltTexture.Sample(_bltSampler, vertex.uv) * vertex.color;	

	clip(result.a <= 0 ? -1 : 1);

	return result;
}

// The vertex shader for full screen triangle blitting.
GorgonFullScreenVertex GorgonFullScreenVertexShader(uint vertexID : SV_VertexID)
{
    GorgonFullScreenVertex result;

    result.uv = float2((vertexID << 1) & 2, vertexID & 2);
    result.position = float4(result.uv * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f), 0.0f, 1.0f);

    return result;
}

// The pixel shader for full screen triangle blitting.
float4 GorgonFullScreenPixelShader(GorgonFullScreenVertex vertex) : SV_Target
{
	float4 result = _bltTexture.Sample(_bltSampler, vertex.uv);

	clip(result.a <= 0 ? -1 : 1);

	return result;
}