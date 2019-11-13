#GorgonInclude "Gorgon2DShaders"

// The light data.
struct Light
{
	// w = Light type.
    float4 lightPosition;
    float4 lightColor;
    float4 dirLightDirection;
	// Attributes (x = specular power, y = attenuation, z = intensity, w = specular enabled flag).
    float4 attribs;
};

#ifdef MAX_LIGHTS
// Information about the light being rendered.
cbuffer LightData : register(b1)
{
	// A list of list to render.
    Light _lights[MAX_LIGHTS];
};
#endif

// Global lighting information.
cbuffer GlobalData : register(b2)
{
	// The global ambient color.
    float4 _ambientColor;
	// The position of the current camera. Used for specular highlight calculations.
    float4 _cameraPos;
	// The texture array indices to use.
    float4 _arrayIndices;
}

// Output data from our lighting vertex shader.
struct GorgonSpriteLitVertex
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float3 uv : TEXCOORD;
    float3 worldPos : WORLDPOS;
};

// Normal and specular map.
Texture2D _normalTexture : register(t1);
SamplerState _normalSampler : register(s1);
Texture2D _specularTexture : register(t2);
SamplerState _specularSampler : register(s2);

// Gets the specular value based on the specular map supplied.
float4 GetSpecularValue(float2 uv, float3 lightDir, float3 normal, float3 toEye, float specularPower)
{
#ifndef USE_ARRAY
    float4 specColor = _specularTexture.Sample(_specularSampler, uv);
#else
	float4 specColor = _gorgonTexture.Sample(_specularSampler, float3(uv, _arrayIndices.y));
#endif

    float3 halfWay = normalize(toEye - lightDir);
    float nDotH = saturate(dot(normal, halfWay));
    return pow(nDotH, specularPower) * specColor;
}

// Retrieves a normal texel.
float3 GetNormal(float2 uv)
{
#ifndef USE_ARRAY
    float4 normalTexel = _normalTexture.Sample(_normalSampler, uv);
#else
	float4 normalTexel = _gorgonTexture.Sample(_normalSampler, float3(uv, _arrayIndices.x));
#endif

    return normalTexel.rgb * 2 - 1;
}

// Simulates a point light with attenuationuation falloff and optional specular highlighting.
float4 PointLight(float3 worldPos, float3 uv, Light light)
{
    float4 color = _gorgonTexture.Sample(_gorgonSampler, uv);

    REJECT_ALPHA(color.a);
		
    float3 normal = GetNormal(uv.xy);
	
    int specularEnabled = int(light.attribs.w);
    float4 result;
    float3 lightRange = light.lightPosition.xyz - worldPos;
    float3 lightDirection = normalize(lightRange);
    float distance = length(lightRange);

    float atten = clamp(light.attribs.y / distance, 0, 1);

    float diffuseAmount = saturate(dot(normal, lightDirection)) * atten * atten;
    result = float4(color.rgb * diffuseAmount * light.lightColor.rgb * light.attribs.z, color.a * diffuseAmount * light.attribs.z);

    if (specularEnabled != 0)
    {
        result += diffuseAmount * GetSpecularValue(uv.xy, normalize(worldPos - light.lightPosition.xyz), normalize(normal), normalize(worldPos - _cameraPos.xyz), light.attribs.x);
    }

    return saturate(float4(result.rgb + (color.rgb * _ambientColor.rgb), color.a));
}

// Simulates a directional light (e.g. the sun) with optional specular highlighting.
float4 DirectionalLight(float3 worldPos, float3 uv, Light light)
{
    float4 color = _gorgonTexture.Sample(_gorgonSampler, uv);
	
    REJECT_ALPHA(color.a);

    float3 normal = GetNormal(uv.xy);
	
    int specularEnabled = int(light.attribs.w);
    float4 result = float4(0, 0, 0, 1);
    float3 lightRange = -light.dirLightDirection;
    float3 lightDir = normalize(lightRange);
    float diffuseAmount;
	
    diffuseAmount = saturate(dot(normal, lightDir));

    result = float4(color.rgb * diffuseAmount * light.lightColor.rgb * light.attribs.z, color.a * diffuseAmount * light.attribs.z);
	
    if (specularEnabled != 0)
    {
		// Oddly enough, if we don't normalize dirLightDirection, our specular shows up correctly, and if we do normalize it, it gets weird at 0x0.
        result += diffuseAmount * GetSpecularValue(uv.xy, light.dirLightDirection.xyz, normalize(normal), normalize(worldPos - _cameraPos.xyz), light.attribs.x);
    }
    
    return saturate(float4(result.rgb + (color.rgb * _ambientColor.rgb), color.a));
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

#ifdef MAX_LIGHTS
// Entry point for lighting shader.
float4 GorgonPixelShaderLighting(GorgonSpriteLitVertex vertex) : SV_Target
{
    float4 result = float4(0, 0, 0, 1);

    for (int i = 0; i < MAX_LIGHTS; ++i)
    {
        Light light = _lights[i];
        int lightType = int(light.lightPosition.w);

        switch (lightType)
        {
			// Directional lights.
            case 1:
                result += DirectionalLight(vertex.worldPos, vertex.uv, light);
                break;
			// Point lights.
            default:
                result += PointLight(vertex.worldPos, vertex.uv, light);
                break;
        }
    }

    return saturate(result);
}
#endif
