// Our default texture and sampler.
Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);

// The transformation matrices.
cbuffer ViewProjectionData : register(b0)
{
	float4x4 View;
	float4x4 Projection;
	float4x4 ViewProjection;	
}

// The world matrix.
cbuffer WorldMatrix : register(b1)
{
	float4x4 World;
}

// Camera data.
cbuffer Camera : register(b0)
{
	float3 CameraPosition;
	float3 CameraLookAt;
	float3 CameraUp;
}

// The light used for lighting calculations.
cbuffer Light : register(b1)
{
	float3 LightColor;
	float3 SpecularColor;
	float3 LightPosition;
	float  SpecularPower;
	float  Attenuation;
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

	float4 worldPos = mul(World, vertex.position);
	output.worldPos = worldPos.xyz;
	output.position = mul(ViewProjection, worldPos);
	
	output.normal = normalize(mul((float3x3)World, vertex.normal));
	
	output.uv = vertex.uv;

	return output;
}

// Our pixel shader that will render objects with textures.
float4 PrimPS(VertexOut vertex) : SV_Target
{
	float4 textureColor = _texture.Sample(_sampler, vertex.uv);
	float3 lightDirection = normalize(vertex.worldPos - LightPosition);
	float diffuse = saturate(dot(vertex.normal, -lightDirection)) * (Attenuation / dot(LightPosition - vertex.worldPos, LightPosition - vertex.worldPos));

	// Using Blinn half angle modification for perofrmance over correctness
	float3 h = normalize(normalize(CameraPosition - vertex.worldPos) - lightDirection);

	float specLighting = pow(saturate(dot(h, vertex.normal)), SpecularPower);

	return float4(saturate(
		(textureColor.rgb * float3(1, 1, 1) * LightColor.rgb * diffuse * 0.6) + // Use light diffuse vector as intensity multiplier
	(SpecularColor * specLighting * 0.5) // Use light specular vector as intensity multiplier
	), textureColor.a);

	//return float4(saturate(textureColor.rgb * dot(vertex.normal, -lightDirection)), textureColor.a);
}