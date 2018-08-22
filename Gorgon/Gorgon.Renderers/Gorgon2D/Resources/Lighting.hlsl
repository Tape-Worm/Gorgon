#GorgonInclude "Gorgon2DShaders"

// Data for the final output.
cbuffer FinalOutputData : register(b1)
{
	float2 screenSize;
};

// Information about the light being rendered.
cbuffer LightData : register(b2)
{
	float4 lightPosition;
	float4 lightColor;		
	float4 dirLightDirection;
	float specularPower;
	float attenuation;	
	int enableSpecular;
	int lightType;
};

// Output data from our vertex shader for lighting.
struct GorgonLitVertex
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float3 uv : TEXCOORD;
	float3 tangent : TANGENT;
	float3 bitangent : BITANGENT;
};

// Updated vertex shader that will perform tangent and bitangent transforms.
GorgonLitVertex GorgonVertexLightingShader(GorgonSpriteVertex vertex)
{
	GorgonLitVertex output;
	
	output.position = mul(ViewProjection, vertex.position);
	output.uv = vertex.uv;
	output.color = vertex.color;

	// We encode our rotation cosine and sine in our vertex data so that we don't need to perform the calculation more than needed.
	float3x3 rotation = float3x3(vertex.angle.x, -vertex.angle.y, 0, vertex.angle.y, vertex.angle.x, 0, 0, 0, 1);

	// Build up our tangents.  Without this, rotating a sprite would not look right when lit.
	output.tangent = normalize(mul(rotation, float3(1, 0, 0)));
	output.bitangent = normalize(float3(output.tangent.y, output.tangent.x, 0) * -1.0f);

	return output;
}

// Function to transform a normal from the normal map using our tangent and bitangent vectors.
float4 TransformNormal(float4 texel, float3 tangent, float3 biTangent)
{
	float3 bump = (texel.xyz * 2.0f) - 1.0f;
	float3 normal = (normalize(float3(0, 0, 1) + (bump.x * tangent + bump.y * biTangent)) + 1.0f) / 2.0f;

	return float4(normal, texel.a);
}

#ifdef DEFERRED_LIGHTING
// The 3 render targets we use for our g-buffer.
struct DeferredRenderOutput
{
	float4 Diffuse : SV_Target0;
	float4 Specular : SV_Target1;
	float4 Normal : SV_Target2;
};

// This will render our scene data into 3 targets simultaneously, and perform the needed transformation on 
// the normal mapping output.
DeferredRenderOutput GorgonPixelShaderDeferred(GorgonLitVertex vertex)
{
	DeferredRenderOutput result;
	float4 diffuse = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy, 0)) * vertex.color;

	REJECT_ALPHA(diffuse.a);

	float4 specular = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy, 1));
	float4 normal = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy, 2));
		
	result.Diffuse = diffuse;
	result.Specular = specular;
	result.Normal = TransformNormal(normal, vertex.tangent, vertex.bitangent);

	return result;
}
#endif

#ifdef LIGHTS
// Gets the specular value based on the specular map supplied.
float4 GetSpecularValue(float3 uv, float4 diffuseValue, float lightAmount, float specularPower)
{
	float4 specTexCol = _gorgonTexture.Sample(_gorgonSampler, uv);
	float specularAmount = pow(lightAmount, specularPower);		
	return 2.0f * (specTexCol * specularAmount * diffuseValue);
}

// Simulates a point light with attenuationuation falloff and optional specular highlighting.
float4 PointLight(float2 uv)
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, float3(uv, 0));

	REJECT_ALPHA(color.a);
		
	float3 normal = ((_gorgonTexture.Sample(_gorgonSampler, float3(uv, 2)).xyz) * 2.0f) - 1.0f;	
	float3 pixelPosition = float3(screenSize.x * uv.x, screenSize.y * uv.y, 0);
	float4 result;
	float3 lightDir;
	float lightAmount;
	float specularAmount;
	float3 distance = pixelPosition - lightPosition;
	
	lightDir = normalize(lightPosition.xyz - pixelPosition);
	// Reverse Y direction, otherwise the light will be flipped vertically.
	lightDir.y = -lightDir.y;

	lightAmount = saturate(dot(normal, lightDir) * (((lightPosition.z * lightPosition.z) * attenuation) / dot(distance, distance)));
	result = (color * lightAmount * lightColor);
	if (enableSpecular != 0)
	{
		result += GetSpecularValue(float3(uv, 1), result, lightAmount, specularPower);
	}
	result.a = color.a;

	return saturate(result);
}

// Simulates a directional light (e.g. the sun) with optional specular highlighting.
float4 DirectionalLight(float2 uv)
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, float3(uv, 0));

	REJECT_ALPHA(color.a);

	float3 normal = ((_gorgonTexture.Sample(_gorgonSampler, float3(uv, 2)).xyz) * 2.0f) - 1.0f;
	float4 result = float4(0, 0, 0, 1);
	float4 diffuse;
	float3 lightDir = dirLightDirection;
	float lightAmount;
	float specularAmount;	

	lightAmount = saturate(dot(normal, lightDir)) * attenuation;	

	diffuse = (color * lightAmount * lightColor);
	result += diffuse;
	if (enableSpecular != 0)
	{
		result += GetSpecularValue(float3(uv, 1), diffuse, lightAmount, specularPower);
	}
	result.a = color.a;

	return saturate(result);
}

// Entry point for lighting shader.
float4 GorgonPixelShaderLighting(GorgonSpriteVertex vertex) : SV_Target
{
	switch(lightType)
	{
		// Directional lights.
		case 1:
			return DirectionalLight(vertex.uv.xy);
		// Point lights.
		default:
			return PointLight(vertex.uv.xy);
	}
}
#endif
