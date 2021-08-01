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
	float4 textureColor = _texture.Sample(_sampler, vertex.uv) * float4(0.2, 0.2, 0.2, 1.0);
	float3 output = float3(0, 0, 0);

	clip(textureColor.a < 0.02f ? -1 : 1);

	for (int i = 0; i < MAX_LIGHTS; ++i)
	{
		LightData light = lights[i];

		float lightSpecularPower = light.LightAttributes.x;
		float intensity = light.LightAttributes.y;
		float attenuation = light.LightAttributes.z;
		bool specEnabled = bool(light.LightAttributes.w);

		if (attenuation <= 0.0f)
		{			
			continue;
		}

		float3 lightDirection = normalize(vertex.worldPos - light.LightPosition);
		float diffuse = saturate(dot(normal, -lightDirection)) * clamp(attenuation / length(light.LightPosition - vertex.worldPos), 0, 1);

		output += float3(textureColor.rgb * light.LightColor.rgb * diffuse * intensity); // Use light diffuse vector as intensity multiplier
				
		if (specEnabled)
		{
			// Using Blinn half angle modification for performance over correctness
			float3 h = normalize(normalize(CameraPosition - vertex.worldPos) - lightDirection);
			float specLighting = pow(saturate(dot(normal, h)), lightSpecularPower);
			output = output + (specLighting * spec.r * specularPower);
		}
	}	

	return float4(output, textureColor.a);	
}

// Lit pixel shader with no bump mapping
float4 PrimPSNoBump(VertexOut vertex) : SV_Target
{	
	float4 textureColor = _texture.Sample(_sampler, vertex.uv);		
	float4 result = blinn(vertex, vertex.normal, float3(1, 0, 0));

	return result * float4(0.2, 0.2, 0.2, 1.0) + (textureColor * 0.25f);
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