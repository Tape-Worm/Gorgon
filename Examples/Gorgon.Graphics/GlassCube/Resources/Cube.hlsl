// The data used to render the cube.
// Includes the transformation matrices combined together. This is a combination of a World transform (the position, scale and 
// rotation of a vertex mesh), View transform (the camera position, scale, and rotation), and the Projection transform 
// (transforms 3D space into 2D space).
//
// Also contains, the handle for the vertex buffer that contains the cube geometry, the handle for the texture sampler to use 
// and the handle for the texture to use for our cube.
struct RenderData
{
    float4x4 wvp;
    int vertexBufferHandle;
    int samplerHandle;
    int textureHandle;
};

ConstantBuffer<RenderData> _renderData : register(b0);

// The shader replication of our GlassCube vertex.
// Note that the semantics at the end of each element matches that of our definition in C#.
struct GlassCubeVertex
{
   float4 position : SV_POSITION;
   float2 uv : TEXCOORD;
};

// Our vertex shader.
// This transforms a vertex from object space into world space by multiplying it by the World/View/Projection matrix passed to the shader.
GlassCubeVertex GlassCubeVS(uint vertexID : SV_VertexID)
{
    StructuredBuffer<GlassCubeVertex> vertexBuffer = ResourceDescriptorHeap[_renderData.vertexBufferHandle];

    GlassCubeVertex output = vertexBuffer[vertexID];

    // Transform from object space into world space by multiplying the vertex position by the world/view/projection matrix.
    output.position = mul(output.position, _renderData.wvp);

    return output;
}

// Our pixel shader.
// This applies coloring (shading) to the pixel that is currently rendering on the GPU.
// For this example, our output is very simple.  We just return the color of the vertex.  In more complex examples we could sample from 
// a texture, performing per-pixel lighting, etc...
float4 GlassCubePS(GlassCubeVertex vertex) : SV_Target
{
    SamplerState textureSampler = SamplerDescriptorHeap[_renderData.samplerHandle];
    Texture2D texture = ResourceDescriptorHeap[_renderData.textureHandle];
    
    return texture.Sample(textureSampler, vertex.uv);
}