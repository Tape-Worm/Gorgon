// Per frame rendering data for debug visualization.
struct GorgonDebugRenderData
{
    float4x4 worldViewProjection;
    int vertexBufferHandle;
};

// A vertex for drawing debug lines.
struct GorgonDebugLineVertex
{
    float4 position : SV_Position;
};

ConstantBuffer<GorgonDebugRenderData> _debugRenderData : register(b15);

// The vertex shader for lines used in drawing debug data.
GorgonDebugLineVertex GorgonDebugVertexShader(uint vertexID : SV_VertexID)
{
    StructuredBuffer<GorgonDebugLineVertex> vertexBuffer = ResourceDescriptorHeap[_debugRenderData.vertexBufferHandle];
    GorgonDebugLineVertex vertex = vertexBuffer[vertexID];
    GorgonDebugLineVertex result;

    result.position = mul(float4(vertex.position.xyz, 1.0f), _debugRenderData.worldViewProjection);

    return result;
}

// The pixel shader for lines used in drawing debug data.
float4 GorgonDebugPixelShader(GorgonDebugLineVertex vertex) : SV_Target
{
    return float4(1, 0, 0, 1);
}
