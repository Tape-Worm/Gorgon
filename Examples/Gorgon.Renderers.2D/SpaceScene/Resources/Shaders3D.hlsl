#define MAX_LIGHTS 8

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
	float4x4 ViewProjection;	
}

// The world matrix.
cbuffer WorldMatrix : register(b1)
{
	float4x4 World;
}

// The material for our object.
cbuffer Material : register(b2)
{
	float4 matAlbedo;
	float2 uvOffset;
	float specularPower;
}

// Camera data.
cbuffer Camera : register(b0)
{
	float3 CameraPosition;
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
		float  Intensity;
	} lights[MAX_LIGHTS];
}

// Our vertex.
struct PrimVertex
{
	float4 position : SV_POSITION;
	float3 normal: NORMAL;
	float2 uv : TEXCOORD;
	float4 tangent: TANGENT;
};

// The vertex for our pixel shader.
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
	float4 textureColor = _texture.Sample(_sampler, vertex.uv) * matAlbedo;	
	float3 output = float3(0, 0, 0);

	normal = normalize(normal);
	
	for (int i = 0; i < MAX_LIGHTS; ++i)
	{
		LightData light = lights[i];

		if (light.Attenuation <= 0.0f)
		{
			continue;
		}

		float3 lightRange = light.LightPosition - vertex.worldPos;
		float3 lightDirection = normalize(lightRange);
		float distance = length(lightRange);		
				
		float attenuation = clamp(light.Attenuation / distance, 0, 1);
		float diffuse = saturate(dot(normal, lightDirection)) * attenuation * attenuation;

		output += float3(textureColor.rgb * light.LightColor.rgb * diffuse * light.Intensity);
		
		if ((specularPower > 0) && (light.SpecularPower > 0.0f))
		{
			float3 h = normalize(-normalize(vertex.worldPos - light.LightPosition) + normalize(vertex.worldPos - CameraPosition));
			float specLighting = pow(saturate(dot(normal, h)), light.SpecularPower);
			output = output + (diffuse * (light.SpecularColor * specLighting * spec.r * specularPower));
		}
	}

	return float4(output, textureColor.a);
}

// Lit pixel shader with no bump mapping
float4 PrimPSNoBump(VertexOut vertex) : SV_Target
{	
	float4 textureColor = _texture.Sample(_sampler, vertex.uv);		
	float4 result = blinn(vertex, vertex.normal, float3(1, 0, 0));

	return result * matAlbedo + (textureColor * 0.25f);
}

// Our bump mapped pixel shader that will render the bump mapped texture.
float4 PrimPSBump(VertexOut vertex) : SV_Target
{
	float3 bumpAmount = _normalTexture.Sample(_normalSampler, vertex.uv).xyz;
	bumpAmount.g = 1.0f - bumpAmount.g;
	bumpAmount = bumpAmount * 2.0f - 1.0f;
	float3 normal = normalize(vertex.normal + (bumpAmount.x * vertex.tangent + bumpAmount.y * vertex.bitangent));
	float3 spec = 0;

	if (specularPower > 0)
	{
		spec = _specTexture.Sample(_specSampler, vertex.uv).xyz;
	}

	return blinn(vertex, normal, spec);
}