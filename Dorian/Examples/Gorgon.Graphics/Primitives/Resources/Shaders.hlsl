// Our default texture and sampler.
Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);

// The transformation matrices.
cbuffer WorldViewProjection : register(b0)
{
	float4x4 World;
	float4x4 View;
	float4x4 Projection;
}

// The light used for lighting calculations.
cbuffer Light : register(b0)
{
	float4 LightColor;
	float3 LightPosition;
}

// Our vertex.
struct PrimVertex
{
	float4 position : SV_POSITION;
	float3 normal: NORMAL;
	float2 uv : TEXCOORD;
};

struct VertexOut
{
	float4 position: SV_POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
	float3 worldPos : TEXCOORD1;
};


// Our default vertex shader.
VertexOut PrimVS(PrimVertex vertex)
{
	VertexOut output;

	float4x4 WVP = mul(View, Projection);
	float4 worldPos = mul(World, vertex.position);
	output.worldPos = worldPos.xyz;
	output.position = mul(WVP, worldPos);
	
	output.normal = mul(vertex.normal, (float3x3)World);
	output.uv = vertex.uv;
	

	return output;
}

// Our pixel shader that will render objects with textures.
float4 PrimPS(VertexOut vertex) : SV_Target
{
	float3 lightDirection = normalize(vertex.worldPos - LightPosition);
	float diffuse = saturate(dot(vertex.normal, -lightDirection)) * (100.0f / dot(LightPosition - vertex.worldPos, LightPosition - vertex.worldPos));

	// Using Blinn half angle modification for perofrmance over correctness
	float3 h = normalize(normalize(-vertex.worldPos) - lightDirection);

	float specLighting = pow(saturate(dot(h, vertex.normal)), 128.0f);

	return float4(saturate(
		(float3(1, 1, 1) * LightColor.rgb * diffuse * 0.6) + // Use light diffuse vector as intensity multiplier
		(float3(1, 1, 1) * specLighting * 0.5) // Use light specular vector as intensity multiplier
		), 1);
}