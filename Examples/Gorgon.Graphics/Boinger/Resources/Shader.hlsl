// The render data.
struct RenderData
{
    float4x4 wvp;
    int vertexBufferHandle;
};

struct BoingerVertex
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD;
};

// The data structure holding our material.
struct BoingerMaterial
{
    float4 diffuse;
    int textureHandle;
    int samplerHandle;
};

ConstantBuffer<RenderData> _renderData : register(b0);
ConstantBuffer<BoingerMaterial> _material : register(b1);

// Our default vertex shader.
BoingerVertex BoingerVS(uint vertexID : SV_VertexID)
{	
    StructuredBuffer<BoingerVertex> buffer = ResourceDescriptorHeap[_renderData.vertexBufferHandle];
    
    BoingerVertex vertex = buffer[vertexID];
    
    BoingerVertex psOut;
    
    psOut.position = mul(vertex.position, _renderData.wvp);
    psOut.uv = vertex.uv;

    return psOut;
}

// Our pixel shader that will render objects with textures.
float4 BoingerPS(BoingerVertex vertex) : SV_Target
{
    SamplerState textureSampler = SamplerDescriptorHeap[_material.samplerHandle];
    Texture2D texture = ResourceDescriptorHeap[_material.textureHandle];
    
    return texture.Sample(textureSampler, vertex.uv) * _material.diffuse;
}