// Our default texture and sampler.
Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);

// The transformation matrices.
cbuffer WorldViewProjection : register(b0)
{
    float4x4 World;
	float4x4 WVP;
}

// The diffuse color.
cbuffer Material : register(b0)
{
	float4 diffuse;
    float4 emissive;
    float4 specular;
    float3 CameraPos;
}

// Our vertex.
struct ModelVertex
{
   float4 position : SV_POSITION;
   float3 normal : NORMAL;
   float4 color : COLOR0;
   float2 uv : TEXCOORD;
};

// The output vertex.
struct OutVertex
{
   float4 position : SV_POSITION;
   float4 worldPos : WORLDPOS;
   float3 normal : NORMAL;
   float4 color : COLOR0;
   float2 uv : TEXCOORD;
};

// Gets the specular value based on the specular map supplied.
float GetSpecularValue(float3 lightDir, float3 normal, float3 toEye)
{
    float3 halfWay = normalize(lightDir + toEye);
    float nDotH = saturate(dot(halfWay, normal));
    return pow(nDotH, specular.a);
}

// Simulates point lighting so our model can be seen.
float4 PointLight(float3 normal, float3 vertexPos)
{
    // Use a fixed position for the light.
    float3 lightRange = float3(-10, 10, -10) - vertexPos;
    float distance =  length(lightRange);

    float3 lightDirection = lightRange / distance;

    float NDotL = saturate(dot(normal, lightDirection));
    float3 lightAmount = float3(NDotL * float3(1, 1, 1));
    float specAmount = 0;

    if ((NDotL > 0) && (specular.a > 0))
    {
        specAmount = GetSpecularValue(lightDirection, normal, CameraPos);
    }
        
    return float4(lightAmount, specAmount);
}

// Our default vertex shader.
OutVertex ModelVS(ModelVertex vertex)
{
	OutVertex output;

	output.position = mul(vertex.position, WVP);

    // We calculate our lighting in world space, so we need to transform our vertex and normal 
    // into world space so that the lighting is correct when the model is transformed.
    output.worldPos = mul(vertex.position, World);
    output.normal = normalize(mul(World, vertex.normal));

    output.color = vertex.color;
    output.uv = vertex.uv;

	return output;
}

// Our pixel shader that will render objects with textures.
float4 ModelPS(OutVertex vertex) : SV_Target
{
    // Hard code an ambient lighting amount.
    float4 textureCol = _texture.Sample(_sampler, vertex.uv) * vertex.color * diffuse;
    float3 emissiveCol = textureCol.rgb * emissive.rgb;
    float4 litColor = PointLight(vertex.normal, vertex.worldPos);
    float3 specColor = litColor.a * specular.rgb;
    float3 finalColor = saturate(emissiveCol + (litColor.rgb * textureCol) + specColor + float3(0.125f, 0.125f, 0.125f));

	return float4(finalColor, textureCol.a);
}