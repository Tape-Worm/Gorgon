// Per frame data.
struct RenderData
{
	float4x4 viewProjection;	
	float4 cameraPosition;
	int samplerHandle;	
	int lightConstantBufferHandle;
};

// Data pertaining to the object being rendered.
struct ObjectData
{
	float4x4 world;
	float2 uvOffset;
	float matSpecularPower;
	int vertexBufferHandle;
	int textureHandle;
	int normalHandle;
	int specularHandle;
};

// Lighting information.
struct Light
{
	float4 position;
	float4 attenuation;
	float4 attributes;
	float4 color;
};

struct LightData
{
	Light lights[8];
};

ConstantBuffer<RenderData> _renderData : register(b0);
ConstantBuffer<ObjectData> _objectData : register(b1);

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
VertexOut PrimVS(uint vertexIndex : SV_VertexID)
{
	StructuredBuffer<PrimVertex> vertexBuffer = ResourceDescriptorHeap[_objectData.vertexBufferHandle];
	PrimVertex vertex = vertexBuffer[vertexIndex];
	VertexOut output;

	float4 worldPos = mul(_objectData.world, vertex.position);
	output.worldPos = worldPos.xyz;
	output.position = mul(_renderData.viewProjection, worldPos);	
	
	output.normal = normalize(mul((float3x3)_objectData.world, vertex.normal));
	
	output.uv = vertex.uv;

	output.tangent = normalize(mul((float3x3)_objectData.world, vertex.tangent.xyz));
	output.bitangent = normalize(cross(output.tangent, output.normal) * vertex.tangent.w);
	
	return output;
}

// Function to perform blinn/phong lighting.
float4 blinn(VertexOut vertex, float3 normal, float3 spec)
{
	SamplerState textureSampler = SamplerDescriptorHeap[_renderData.samplerHandle];
	ConstantBuffer<LightData> lightBuffer = ResourceDescriptorHeap[_renderData.lightConstantBufferHandle];
	Texture2D texture = ResourceDescriptorHeap[_objectData.textureHandle];
	
	float4 textureColor = texture.Sample(textureSampler, vertex.uv);
	float3 output = float3(0, 0, 0);

	clip(textureColor.a < 0.02f ? -1 : 1);

	for (int i = 0; i < 8; ++i)
	{
		Light light = lightBuffer.lights[i];

		float specularPower = light.attributes.x;
		float lightMaxRange = light.attenuation.w;

		if ((light.attenuation.x <= 0.0f)
			&& (light.attenuation.y <= 0.0f)
			&& (light.attenuation.z <= 0.0f))
		{			
			output += float3(0.392157f * 0.025f, 0.584314f * 0.025f, 0.929412f * 0.025f);
			continue;
		}

		float3 lightPos = float3(light.position.x, light.position.y, -light.position.z);
		float3 lightRange = lightPos - vertex.worldPos;
		float distance = length(lightRange);

		// If the distance between the light and the vertex is larger than the range, then we won't be lighting today.
		if (distance >= lightMaxRange)
		{
			output += float3(0.392157f * 0.025f, 0.584314f * 0.025f, 0.929412f * 0.025f);
			continue;
		}

		float3 lightDirection = lightRange / distance; 
		float atten = 1.0f / (light.attenuation.x + (distance * light.attenuation.y) + (distance * distance * light.attenuation.z));
		float ndotl = saturate(dot(lightDirection, normal));

		float3 result = textureColor.rgb * ndotl * light.color.rgb; 
				
		if ((_objectData.matSpecularPower > 0) && (specularPower >= 1.0f))
		{
			// Using Blinn half angle modification for performance over correctness
			float3 h = normalize(normalize(_renderData.cameraPosition.xyz - vertex.worldPos) + lightRange);
			float specLighting = pow(saturate(dot(h, normal)), specularPower);
			result += textureColor.rgb * (specLighting * 0.5 * spec.r * _objectData.matSpecularPower);
		}		

		result *= atten;
		output += saturate(result);
	}	
	
	return float4(saturate(float3(0.392157f * 0.025f, 0.584314f * 0.025f, 0.929412f * 0.025f) + output), textureColor.a);
}

// Our bump mapped pixel shader that will render the bump mapped texture.
float4 PrimPSWaterBump(VertexOut vertex) : SV_Target
{
	SamplerState sampler = SamplerDescriptorHeap[_renderData.samplerHandle];
	Texture2D normalTexture = ResourceDescriptorHeap[_objectData.normalHandle];	
	float2 uvOffset = _objectData.uvOffset;

	float2 scaledUv = vertex.uv * 2.0f;
	float3 bumpAmount = 1.0f * (normalTexture.Sample(sampler, scaledUv + uvOffset).xyz * 2.0f - 1.0f);
	bumpAmount += saturate(0.5f * (normalTexture.Sample(sampler, scaledUv + uvOffset * 2.0f).xyz * 2.0f - 1.0f));
	bumpAmount += saturate(0.25f * (normalTexture.Sample(sampler, scaledUv + uvOffset * 4.0f).xyz * 2.0f - 1.0f));
	float3 normal = normalize(vertex.normal + (bumpAmount.x * vertex.tangent + bumpAmount.y * vertex.bitangent));
		
	float3 spec = 0; 
	
	if (_objectData.matSpecularPower > 0)
	{
		Texture2D specTexture = ResourceDescriptorHeap[_objectData.specularHandle];
		spec = specTexture.Sample(sampler, scaledUv + uvOffset).xyz;
		spec += 2.0f * specTexture.Sample(sampler, scaledUv + uvOffset * 0.5f).xyz;
		spec += 4.0 * specTexture.Sample(sampler, scaledUv + uvOffset * 0.25f).xyz;
	}

	return blinn(vertex, normal, spec);
}

// Our bump mapped pixel shader that will render the bump mapped texture.
float4 PrimPSBump(VertexOut vertex) : SV_Target
{
	SamplerState sampler = SamplerDescriptorHeap[_renderData.samplerHandle];
	Texture2D normalTexture = ResourceDescriptorHeap[_objectData.normalHandle];	

	float3 bumpAmount = 1.0f * (normalTexture.Sample(sampler, vertex.uv).xyz * 2.0f - 1.0f);
	float3 normal = normalize(vertex.normal + (bumpAmount.x * vertex.tangent + bumpAmount.y * vertex.bitangent));
	float3 spec = 0;

	if (_objectData.matSpecularPower > 0)
	{
		Texture2D specTexture = ResourceDescriptorHeap[_objectData.specularHandle];
		spec = specTexture.Sample(sampler, vertex.uv).xyz;
	}

	return blinn(vertex, normal, spec);
}

// Our pixel shader that will render objects with textures.
float4 PrimPS(VertexOut vertex) : SV_Target
{
	return blinn(vertex, vertex.normal, float3(1, 0, 0));
}

NormalVertex NormalVS(uint vertexIndex : SV_VertexID)
{
	StructuredBuffer<NormalVertex> vertexBuffer = ResourceDescriptorHeap[_objectData.vertexBufferHandle];
	NormalVertex vertex = vertexBuffer[vertexIndex];
	NormalVertex output;

	float4 worldPos = mul(_objectData.world, vertex.pos);
	output.pos = mul(_renderData.viewProjection, worldPos);

	return output;
}

float4 NormalPS(NormalVertex vertex) : SV_Target
{
	return float4(1, 0, 0, 1);
}
