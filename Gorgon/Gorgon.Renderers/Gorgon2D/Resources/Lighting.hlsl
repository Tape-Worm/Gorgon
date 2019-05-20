#GorgonInclude "Gorgon2DShaders"

// Information about the light being rendered.
cbuffer LightData : register(b1)
{
	float4 _lightPosition;
	float4 _lightColor;		
	float4 _dirLightDirection;
	// Attributes (x = specular power, y = attenuation, z = intenssity, w = specular enabled flag).
	float4 _attribs;
	// The type of light being rendered.
	int _lightType;
};

// Global lighting information.
cbuffer GlobalData : register(b2)
{
	// The position of the current camera. Used for specular highlight calculations.
	float3 _cameraPos;
	// Flag to flip the normal map green channel.
	int _flipYNormal;
}

// Output data from our deferred vertex shader for lighting.
struct GorgonLitVertex
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float3 uv : TEXCOORD;
	float3 tangent : TANGENT;
	float3 bitangent : BITANGENT;
	float3 worldPos : WORLDPOS;
};

// Output data from our lighting vertex shader.
struct GorgonSpriteLitVertex
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float3 uv : TEXCOORD;
	float3 worldPos : WORLDPOS;
};

Texture2D _normalTexture : register(t1);
SamplerState _normalSampler : register(s1);

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

// Updated vertex shader that will perform tangent and bitangent transforms.
GorgonLitVertex GorgonVertexLightingShader(GorgonSpriteVertex vertex)
{
	GorgonLitVertex output;
	
	output.worldPos = vertex.position.xyz;
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

// This will render our scene data into 3 targets simultaneously, and perform the needed transformation on 
// the normal mapping output.
DeferredRenderOutput GorgonPixelShaderDeferred(GorgonLitVertex vertex)
{
	DeferredRenderOutput result;
	float4 diffuse = _gorgonTexture.Sample(_gorgonSampler, vertex.uv);

	REJECT_ALPHA(diffuse.a);

	float4 specular = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy, vertex.uv.z + 1));
	float4 normal = _gorgonTexture.Sample(_gorgonSampler, float3(vertex.uv.xy, vertex.uv.z + 2));
		
	result.Diffuse = diffuse;
	result.Specular = specular;
	result.Normal = TransformNormal(normal, vertex.tangent, vertex.bitangent);

	return result;
}
#endif

#ifdef LIGHTS
// Gets a normal map texel and modifies it for use.
float3 GetNormalData(float2 uv)
{
	float3 result = _normalTexture.Sample(_normalSampler, uv).xyz;
	
	if (_flipYNormal != 0)
	{
		result.g = 1.0f - result.g;
	}

	return (result * 2) - 1;
}

// Gets the specular value based on the specular map supplied.
float4 GetSpecularValue(float3 lightDir, float3 normal, float3 toEye, float3 uv, float specularPower)
{
	float4 specTexCol = _gorgonTexture.Sample(_gorgonSampler, uv);
	float3 halfWay = normalize(-lightDir + toEye);
	float specIntensity = saturate(dot(normal, halfWay));
	return pow(specIntensity, specularPower) * specTexCol;
}

// Simulates a point light with attenuationuation falloff and optional specular highlighting.
float4 PointLight(float3 worldPos, float2 uv)
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, float3(uv, 0));	
	
	int specularEnabled = int(_attribs.w);
	float3 normal = GetNormalData(uv);
	float4 result;	
	float3 lightRange = _lightPosition - worldPos;
	float3 lightDirection = normalize(lightRange);	
	float distance = length(lightRange);

	float atten = clamp(_attribs.y / distance, 0, 1);

	float diffuseAmount = saturate(dot(normal, lightDirection)) * atten * atten;
	result = float4(color.rgb * diffuseAmount * _lightColor.rgb * _attribs.z, 0);

	if (specularEnabled != 0)
	{
		result += diffuseAmount * GetSpecularValue(normalize(worldPos - _lightPosition), normal, normalize(worldPos - _cameraPos), float3(uv.xy, 1), _attribs.x);
	}
	result.a = 1.0f;

	return saturate(result);
}

// Simulates a directional light (e.g. the sun) with optional specular highlighting.
float4 DirectionalLight(float3 worldPos, float2 uv)
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, float3(uv, 0));

	REJECT_ALPHA(color.a);

	int specularEnabled = int(_attribs.w);
	float3 normal = GetNormalData(uv);
	float4 result = float4(0, 0, 0, 1);
	float3 lightDir = normalize(_dirLightDirection);
	float diffuseAmount;
	
	diffuseAmount = saturate(dot(normal, -lightDir));	

	result = color * diffuseAmount * _lightColor * _attribs.z;
	
	if (specularEnabled != 0)
	{
		result += diffuseAmount * GetSpecularValue(normalize(_dirLightDirection - worldPos), normal, normalize(worldPos - _cameraPos), float3(uv.xy, 1), _attribs.x);
	}
	result.a = color.a;

	return saturate(result);
}

// Updated vertex shader that will capture the world position of the vertex prior to sending to the pixel shader.
GorgonSpriteLitVertex GorgonVertexLitShader(GorgonSpriteVertex vertex)
{
	GorgonSpriteLitVertex output;
	
	output.worldPos = vertex.position.xyz;
	output.position = mul(ViewProjection, vertex.position);
	output.uv = vertex.uv;
	output.color = vertex.color;

	return output;
}

// Entry point for lighting shader.
float4 GorgonPixelShaderLighting(GorgonSpriteLitVertex vertex) : SV_Target
{
	switch(_lightType)
	{
		// Directional lights.
		case 1:
			return DirectionalLight(vertex.worldPos, vertex.uv.xy);
		// Point lights.
		default:
			return PointLight(vertex.worldPos, vertex.uv.xy);
	}
}
#endif
