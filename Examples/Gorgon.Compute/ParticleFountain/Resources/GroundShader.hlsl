#GorgonInclude "ParticleShaderCommon"

// Common render data.
struct RenderData
{
    float4x4 wvp;
};

// Data required to render the ground.
struct GroundRenderData
{    
    float2 targetSize;
    int vertexBufferHandle;
    int groundSamplerHandle;
    int reflectionSamplerHandle;
    int textureHandle;
    int reflectionTextureHandle;
};

ConstantBuffer<RenderData> _renderData : register(b0);
ConstantBuffer<GroundRenderData> _groundRenderData : register(b1);

// Our default vertex shader.
OutVertex VertexShader(uint id : SV_VertexID)
{
    StructuredBuffer<Vertex> vertexBuffer = ResourceDescriptorHeap[_groundRenderData.vertexBufferHandle];
    Vertex vertex = vertexBuffer[id];
    OutVertex output;

    output.position = mul(vertex.position, _renderData.wvp);
    output.color = vertex.diffuse;
    output.uv = vertex.uv;

    return output;
}

// Our pixel shader that will render objects with textures.
float4 PixelShader(OutVertex vertex) : SV_Target
{
    float2 screenPos = vertex.position.xy / _groundRenderData.targetSize;

    SamplerState textureSampler = SamplerDescriptorHeap[_groundRenderData.groundSamplerHandle];
    SamplerState reflectionSampler = SamplerDescriptorHeap[_groundRenderData.reflectionSamplerHandle];
    Texture2D texture = ResourceDescriptorHeap[_groundRenderData.textureHandle];
    Texture2D reflectTexture = ResourceDescriptorHeap[_groundRenderData.reflectionTextureHandle];

    return lerp(texture.Sample(textureSampler, vertex.uv) * vertex.color, reflectTexture.Sample(reflectionSampler, screenPos), 0.1f);
}