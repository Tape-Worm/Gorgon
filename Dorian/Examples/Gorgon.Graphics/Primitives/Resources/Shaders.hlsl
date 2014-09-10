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
	struct LightData
	{
		float3 LightColor;
		float3 SpecularColor;
		float3 LightPosition;
		float  SpecularPower;
		float  Attenuation;
	} lights[8];
}

// Our vertex.
struct PrimVertex
{
	float4 position : SV_POSITION;
	float3 normal: NORMAL;
	float2 uv : TEXCOORD;
};

struct NormalVertex
{
	float4 pos : SV_POSITION;
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
	float3 output = float3(0, 0, 0);

	for (int i = 0; i < 8; ++i)
	{
		LightData light = lights[i];

		if (light.Attenuation <= 0.0f)
		{
			continue;
		}

		float3 lightDirection = normalize(vertex.worldPos - light.LightPosition);
		float diffuse = saturate(dot(vertex.normal, -lightDirection)) * (light.Attenuation / dot(light.LightPosition - vertex.worldPos, light.LightPosition - vertex.worldPos));

		// Using Blinn half angle modification for perofrmance over correctness
		float3 h = normalize(normalize(CameraPosition - vertex.worldPos) - lightDirection);
			
		output += float3(textureColor.rgb * float3(1, 1, 1) * light.LightColor.rgb * diffuse * 0.6); // Use light diffuse vector as intensity multiplier

		if (light.SpecularPower >= 1.0f)
		{
			float specLighting = pow(saturate(dot(h, vertex.normal)), light.SpecularPower);
			output = output + (light.SpecularColor * specLighting * 0.5);
		}
	}

	return float4(saturate(float3(0.392157f * 0.25f, 0.584314f * 0.25f, 0.929412f * 0.25f) + output), textureColor.a);
}

NormalVertex NormalVS(NormalVertex vertex)
{
	NormalVertex output;

	float4 worldPos = mul(World, vertex.pos);
	output.pos = mul(ViewProjection, worldPos);

	return output;
}

float4 NormalPS(NormalVertex vertex) : SV_Target
{
	return float4(1, 0, 0, 1);
}
