#GorgonInclude "Gorgon2DShaders"

// A texture array to allow access to the diffuse, normal map, and specular map.
Texture2DArray _gorgonDiffSpecNormTexture : register(t0);

// Texture used for normal mapping.
Texture2D _gorgonNormalTexture : register(t1);

// Texture used for specular mapping.
Texture2D _gorgonSpecTexture : register(t2);

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
	float specEnable;
	float specPower;
	float atten;
	float4 dirLightDirection;
};

// Output data from our vertex shader for lighting.
struct GorgonSpriteLightData
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float3 uv : TEXCOORD;
	float3 tangent : TANGENT;
	float3 bitangent : BITANGENT;
};

// Updated vertex shader that will perform tangent and bitangent transforms.
GorgonSpriteLightData GorgonVertexLightingShader(GorgonSpriteVertex vertex)
{
	GorgonSpriteLightData output;
	
	output.position = mul(WorldViewProjection, vertex.position);
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

// First, we draw our normal mapped sprites with this shader to the normal map layer.
float4 GorgonPixelShaderNormalSprite(GorgonSpriteLightData vertex) : SV_Target
{
	float4 worldNormal = _gorgonTexture.Sample(_gorgonSampler, vertex.uv);

	REJECT_ALPHA(worldNormal.a);

	return TransformNormal(worldNormal, vertex.tangent, vertex.bitangent);
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
DeferredRenderOutput GorgonPixelShaderDeferred(GorgonSpriteLightData vertex)
{
	DeferredRenderOutput result;
	float4 diffuse = _gorgonDiffSpecNormTexture.Sample(_gorgonSampler, float3(vertex.uv.xy, 0)) * vertex.color;

	REJECT_ALPHA(diffuse.a);

	float4 specular = _gorgonDiffSpecNormTexture.Sample(_gorgonSampler, float3(vertex.uv.xy, 1));
	float4 normal = _gorgonDiffSpecNormTexture.Sample(_gorgonSampler, float3(vertex.uv.xy, 2));
		
	result.Diffuse = diffuse;
	result.Specular = specular;
	result.Normal = TransformNormal(normal, vertex.tangent, vertex.bitangent);

	return result;
}
#endif

#ifdef DIRECTIONAL_LIGHT
// Simulates a directional light (e.g. the sun) with optional specular highlighting.
float4 GorgonPixelShaderDirLight(GorgonSpriteLightData vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv);

	REJECT_ALPHA(color.a);

	float3 normal = ((_gorgonNormalTexture.Sample(_gorgonNormalSampler, vertex.uv).xyz) * 2.0f) - 1.0f;
	float4 result = float4(0, 0, 0, 1);
	float4 diffuse;
	float3 lightDir = dirLightDirection;
	float lightAmount;
	float specularAmount;	

	lightAmount = saturate(dot(normal, lightDir)) * atten;	

	diffuse = (color * lightAmount * lightColor);
	result += diffuse;

	if (specEnable != 0)
	{
		float4 specTexCol = _gorgonSpecTexture.Sample(_gorgonSpecSampler, vertex.uv);
		specularAmount = pow(lightAmount, specPower);		
		result += 2.0f * (specTexCol * specularAmount * diffuse);
	}

	result.a = color.a;

	return saturate(result);
}
#endif

#ifdef POINT_LIGHT
// Simulates a point light with attenuation falloff and optional specular highlighting.
float4 GorgonPixelShaderPointLight(GorgonSpriteLightData vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv);

	REJECT_ALPHA(color.a);

	float3 normal = ((_gorgonNormalTexture.Sample(_gorgonSampler, vertex.uv).xyz) * 2.0f) - 1.0f;
	float3 pixelPosition = float3(screenSize.x * vertex.uv.x, screenSize.y * vertex.uv.y, 0);
	float4 result;
	float3 lightDir;
	float lightAmount;
	float specularAmount;
	float3 distance = pixelPosition - lightPosition;
	
	lightDir = normalize(lightPosition.xyz - pixelPosition);

	lightAmount = saturate(dot(normal, lightDir) * (((lightPosition.z * lightPosition.z) * atten) / dot(distance, distance)));
	result = (color * lightAmount * lightColor);

	if (specEnable != 0)
	{
		float4 specTexCol = _gorgonSpecTexture.Sample(_gorgonSampler, vertex.uv);
		specularAmount = pow(lightAmount, specPower);		
		result += 2.0f * (specTexCol * specularAmount * result);
	}

	result.a = color.a;

	return saturate(result);
}
#endif
