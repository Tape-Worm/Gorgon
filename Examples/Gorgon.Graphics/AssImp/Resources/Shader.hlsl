// The transformation matrices.
struct WorldViewProjection
{
    float4x4 world;
    float4x4 wvp;
    float3 cameraPos;
};

// Data required to render our model.
struct RenderData
{    
    float4 diffuse;
    float4 emissive;
    float4 specular;
    int vertexBufferHandle;
    int textureHandle;
    int samplerHandle;
};

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

ConstantBuffer<WorldViewProjection> _wvp : register(b0);
ConstantBuffer<RenderData> _renderData : register(b1);

// Gets the specular value based on the specular map supplied.
float GetSpecularValue(float3 lightDir, float3 normal, float3 toEye, float specular)
{
    float3 halfWay = normalize(lightDir + toEye);
    float nDotH = saturate(dot(halfWay, normal));
    return pow(nDotH, specular);
}

// Simulates point lighting so our model can be seen.
float4 PointLight(float3 normal, float3 vertexPos, float specular, float3 cameraPosition)
{
    // Use a fixed position for the light.
    float3 lightRange = float3(-10, 10, -10) - vertexPos;
    float distance =  length(lightRange);

    float3 lightDirection = lightRange / distance;

    float NDotL = saturate(dot(normal, lightDirection));
    float3 lightAmount = float3(NDotL * float3(1, 1, 1));
    float specAmount = 0;

    if ((NDotL > 0) && (specular > 0))
    {
        specAmount = GetSpecularValue(lightDirection, normal, cameraPosition, specular);
    }
        
    return float4(lightAmount, specAmount);
}

// Our default vertex shader.
OutVertex ModelVS(uint id : SV_VertexID
#if (__SHADER_TARGET_MAJOR > 6) || ((__SHADER_TARGET_MAJOR == 6) && (__SHADER_TARGET_MINOR >= 8))
    , int base : SV_StartVertexLocation
#endif
)
{
    StructuredBuffer<ModelVertex> vertexBuffer = ResourceDescriptorHeap[_renderData.vertexBufferHandle];
    #if (__SHADER_TARGET_MAJOR > 6) || ((__SHADER_TARGET_MAJOR == 6) && (__SHADER_TARGET_MINOR >= 8))
    ModelVertex vertex = vertexBuffer[id + base];
    #else
    ModelVertex vertex = vertexBuffer[id];
    #endif
    OutVertex output;

    output.position = mul(vertex.position, _wvp.wvp);

    // We calculate our lighting in world space, so we need to transform our vertex and normal 
    // into world space so that the lighting is correct when the model is transformed.
    output.worldPos = mul(vertex.position, _wvp.world);
    output.normal = normalize(mul(_wvp.world, float4(vertex.normal, 0))).xyz;

    output.color = vertex.color;
    output.uv = vertex.uv;

    return output;
}

// Our pixel shader that will render objects with textures.
float4 ModelPS(OutVertex vertex) : SV_Target
{
    SamplerState textureSampler = SamplerDescriptorHeap[_renderData.samplerHandle];
    Texture2D texture = ResourceDescriptorHeap[_renderData.textureHandle];

    // Hard code an ambient lighting amount.
    float4 textureColor = texture.Sample(textureSampler, vertex.uv) * vertex.color * _renderData.diffuse;
    float3 emissiveColor = textureColor.rgb * _renderData.emissive.rgb;
    float4 litColor = PointLight(vertex.normal, vertex.worldPos.xyz, _renderData.specular.a, _wvp.cameraPos);
    float3 specColor = litColor.a * _renderData.specular.rgb;
    float3 finalColor = saturate(emissiveColor + (litColor.rgb * textureColor.rgb) + specColor + float3(0.105f, 0.105f, 0.105f));

    return float4(finalColor, textureColor.a);
}