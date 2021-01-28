// Our default texture and sampler.
Texture2D _texture : register(t0);
Texture2D _normalTexture : register(t1); 
Texture2D _specTexture : register(t2); 
SamplerState _sampler : register(s0);
SamplerState _normalSampler : register(s1);
SamplerState _specSampler : register(s2);

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

cbuffer Material : register(b2)
{
	float4 albedo;
	float2 uvOffset;
	float matSpecularPower;
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
		float4 LightPosition;
		float4 LightDirection;
		float4 LightAttributes;
		float4 LightColor;
	} lights[8];
}

// Our vertex.
struct PrimVertex
{
	float4 position : SV_POSITION;
	float3 normal: NORMAL;
	float2 uv : TEXCOORD;
	float4 tangent: TANGENT;
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
	float3 worldPos : WORLDPOS;
	float3 tangent : TANGENT;
	float3 bitangent : BITANGENT;
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

	output.tangent = normalize(mul((float3x3)World, vertex.tangent.xyz));
	output.bitangent = normalize(cross(output.tangent, output.normal) * vertex.tangent.w);
	
	return output;
}

// Function to perform blinn/phong lighting.
float4 blinn(VertexOut vertex, float3 normal, float3 spec)
{
	float4 textureColor = _texture.Sample(_sampler, vertex.uv);
	float3 output = float3(0, 0, 0);

	clip(textureColor.a < 0.02f ? -1 : 1);

	for (int i = 0; i < 8; ++i)
	{
		LightData light = lights[i];

		float specularPower = light.LightAttributes.x;
		float attenuation = light.LightAttributes.z;

		if (attenuation <= 0.0f)
		{			
			continue;
		}

		float3 lightDirection = normalize(vertex.worldPos - light.LightPosition);
		float diffuse = saturate(dot(normal, -lightDirection)) * clamp(attenuation / length(light.LightPosition - vertex.worldPos), 0, 1);

		output += float3(textureColor.rgb * float3(1, 1, 1) * light.LightColor.rgb * diffuse * 0.6); // Use light diffuse vector as intensity multiplier
				
		if ((matSpecularPower > 0) && (specularPower >= 1.0f))
		{
			// Using Blinn half angle modification for performance over correctness
			float3 h = normalize(normalize(CameraPosition - vertex.worldPos) - lightDirection);
			float specLighting = pow(saturate(dot(normal, h)), specularPower);
			output = output + (specLighting * 0.5 * spec.r * matSpecularPower);
		}
	}	

	return float4(saturate(float3(0.392157f * 0.025f, 0.584314f * 0.025f, 0.929412f * 0.025f) + output), textureColor.a);	
}

// Our bump mapped pixel shader that will render the bump mapped texture.
float4 PrimPSWaterBump(VertexOut vertex) : SV_Target
{
	float2 scaledUv = vertex.uv * 2.0f;
	float3 bumpAmount = 1.0f * (_normalTexture.Sample(_normalSampler, scaledUv + uvOffset).xyz * 2.0f - 1.0f);
	bumpAmount += saturate(0.5f * (_normalTexture.Sample(_normalSampler, scaledUv + uvOffset * 2.0f).xyz * 2.0f - 1.0f));
	bumpAmount += saturate(0.25f * (_normalTexture.Sample(_normalSampler, scaledUv + uvOffset * 4.0f).xyz * 2.0f - 1.0f));
	float3 normal = normalize(vertex.normal + (bumpAmount.x * vertex.tangent + bumpAmount.y * vertex.bitangent));
		
	float3 spec = 0; 
	
	if (matSpecularPower > 0)
	{
		spec = _specTexture.Sample(_specSampler, scaledUv + uvOffset).xyz;
		spec += 2.0f * _specTexture.Sample(_specSampler, scaledUv + uvOffset * 0.5f).xyz;
		spec += 4.0 * _specTexture.Sample(_specSampler, scaledUv + uvOffset * 0.25f).xyz;
	}

	return blinn(vertex, normal, spec);
}

// Our bump mapped pixel shader that will render the bump mapped texture.
float4 PrimPSBump(VertexOut vertex) : SV_Target
{
	float3 bumpAmount = 1.0f * (_normalTexture.Sample(_normalSampler, vertex.uv).xyz * 2.0f - 1.0f);
	float3 normal = normalize(vertex.normal + (bumpAmount.x * vertex.tangent + bumpAmount.y * vertex.bitangent));
	float3 spec = 0;

	if (matSpecularPower > 0)
	{
		spec = _specTexture.Sample(_specSampler, vertex.uv).xyz;
	}

	return blinn(vertex, normal, spec);
}

// Our pixel shader that will render objects with textures.
float4 PrimPS(VertexOut vertex) : SV_Target
{
	return blinn(vertex, vertex.normal, float3(1, 0, 0));
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
