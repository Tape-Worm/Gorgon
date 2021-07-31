#GorgonInclude "Gorgon2DShaders"

// The light data.
struct Light
{
	// w = Light type.
    float4 Position;
    float4 Direction;
	// Attributes (X = Specular Power, Y = Intensity, Z = Attenuation (point light only), W = Specular on/off).
    float4 Attributes;
    float4 Color;
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
    float4 uv : TEXCOORD;
    float3 worldPos : WORLDPOS;
#ifdef TRANSFORM // Apply transforms to normals as needed.
    float3 tangent : TANGENT;
    float3 bitangent : BITANGENT;
#endif
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
	float4 specColor = _arrayIndices.y < 0 ? float4(0, 0, 0, 1) : _gorgonTexture.Sample(_specularSampler, float3(uv, _arrayIndices.y));
#endif

    float3 halfWay = normalize(toEye - lightDir);
    float nDotH = saturate(dot(normal, halfWay));
    return pow(nDotH, specularPower) * specColor;
}

// Retrieves a normal texel.
#ifdef TRANSFORM
float3 GetNormal(float2 uv, float3 tangent, float3 bitangent)
#else
float3 GetNormal(float2 uv)
#endif
{
    // TODO: We should probably be transforming the normal here instead of the gbuffer.
    //       Otherwise, rotation will be off when we aren't using a gbuffer.
#ifndef USE_ARRAY
    float4 normalTexel = _normalTexture.Sample(_normalSampler, uv);
#else
	float4 normalTexel = _arrayIndices.x < 0 ? float4(0.5f, 0.5f, 1.0f, 1.0f) : _gorgonTexture.Sample(_normalSampler, float3(uv, _arrayIndices.x));
#endif

    float3 normal = normalTexel.rgb * 2 - 1;
#ifdef TRANSFORM
    normal = normalize(float3(0, 0, normal.z) + (normal.x * tangent + normal.y * bitangent));
#endif

    return normal;
}

// Simulates a point light with attenuationuation falloff and optional specular highlighting.
float4 PointLight(GorgonSpriteLitVertex vertex, Light light)
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);

    REJECT_ALPHA(color.a);    

    float2 uv = vertex.uv.xy / vertex.uv.w;
        	
#ifdef TRANSFORM
    float3 normal = GetNormal(uv, vertex.tangent, vertex.bitangent);
#else
    float3 normal = GetNormal(uv);
#endif
	
    int specularEnabled = int(light.Attributes.w);
    float3 result;
    float3 lightRange = light.Position.xyz - vertex.worldPos;
    float3 lightDirection = normalize(lightRange);
    float distance = length(lightRange);

    float atten = clamp(abs(light.Attributes.z / distance), 0, 1);

    float diffuseAmount = saturate(dot(normal, lightDirection)) * atten * atten;
    result = float3(color.rgb * diffuseAmount * light.Color.rgb * light.Attributes.y);

    if (specularEnabled != 0)
    {        
        result += diffuseAmount * GetSpecularValue(uv, normalize(vertex.worldPos - light.Position.xyz), normalize(normal), normalize(vertex.worldPos - _cameraPos.xyz), light.Attributes.x).rgb;
    }

    return saturate(float4(result + (color.rgb * _ambientColor.rgb), color.a));
}

// Simulates a directional light (e.g. the sun) with optional specular highlighting.
float4 DirectionalLight(GorgonSpriteLitVertex vertex, Light light)
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);
	
    REJECT_ALPHA(color.a);

    float2 uv = vertex.uv.xy / vertex.uv.w;

#ifdef TRANSFORM
    float3 normal = GetNormal(uv, vertex.tangent, vertex.bitangent);
#else
    float3 normal = GetNormal(uv);
#endif
	
    int specularEnabled = int(light.Attributes.w);
    float3 result;
    float3 lightDir = normalize(light.Direction);
    float diffuseAmount;
	
    diffuseAmount = saturate(dot(normal, lightDir));

    result = float3(color.rgb * diffuseAmount * light.Color.rgb * light.Attributes.y);
	
    if (specularEnabled != 0)
    {        
		// Oddly enough, if we don't normalize Direction, our specular shows up correctly, and if we do normalize it, it gets weird at 0x0.
        result += diffuseAmount * GetSpecularValue(uv, -light.Direction, normalize(normal), normalize(vertex.worldPos - _cameraPos.xyz), light.Attributes.x).rgb;
    }
    
    return saturate(float4(result + (color.rgb * _ambientColor.rgb), color.a));
}

// Updated vertex shader that will capture the world position of the vertex prior to sending to the pixel shader.
GorgonSpriteLitVertex GorgonVertexLitShader(GorgonSpriteVertex vertex)
{
    GorgonSpriteLitVertex output;
	
    output.worldPos = vertex.position.xyz;
    output.position = mul(ViewProjection, vertex.position);
    output.uv = vertex.uv;
    output.color = vertex.color;

#ifdef TRANSFORM
	// We encode our rotation cosine and sine in our vertex data so that we don't need to perform the calculation more than needed.
	float3x3 rotation = float3x3(vertex.angle.x, -vertex.angle.y, 0, vertex.angle.y, vertex.angle.x, 0, 0, 0, 1);

	// Build up our tangents.  Without this, rotating a sprite would not look right when lit.
	output.tangent = normalize(mul(rotation, float3(1, 0, 0)));
	output.bitangent = normalize(cross(output.tangent, float3(0, 0, -1)));
#endif

    return output;
}

#ifdef MAX_LIGHTS
// Entry point for lighting shader.
float4 GorgonPixelShaderLighting(GorgonSpriteLitVertex vertex) : SV_Target
{
    float4 result = float4(0, 0, 0, 1);
    float4 color = float4(0, 0, 0, 0);

    for (int i = 0; i < MAX_LIGHTS; ++i)
    {
        Light light = _lights[i];
        int lightType = int(light.Position.w);

        switch (lightType)
        {			
            case 1:
                // Point lights.
                color = PointLight(vertex, light);
                result = float4(result.rgb + color.rgb, color.a);                
                break;
            case 2:
                // Directional lights.
				color = DirectionalLight(vertex, light);
                result = float4(result.rgb + color.rgb, color.a);
                break;			
        }
    }

    return saturate(result);
}
#endif
