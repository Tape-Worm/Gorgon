// The data used to render the triangle.
// Includes the transformation matrices combined together. This is a combination of a World transform (the position, scale and 
// rotation of a vertex mesh), View transform (the camera position, scale, and rotation), and the Projection transform 
// (transforms 3D space into 2D space).
//
// Also contains, the handle for the vertex buffer that contains the triangle geometry, the handle for the texture sampler to use 
// and the handle for the texture to use for our triangle.
struct RenderData
{
    float4x4 wvp;
    int vertexBufferHandle;
};

ConstantBuffer<RenderData> _renderData : register(b0);

// The shader replication of our MiniTri vertex.
// Note that the semantics at the end of each element matches that of our definition in C#.
struct MiniTriVertex
{
   float4 position : SV_POSITION;
   float4 color : COLOR;
};

// Our vertex shader.
// This transforms a vertex from object space into world space by multiplying it by the World/View/Projection matrix passed to the shader.
MiniTriVertex MiniTriVS(uint vertexID : SV_VertexID)
{
	StructuredBuffer<MiniTriVertex> vertexBuffer = ResourceDescriptorHeap[_renderData.vertexBufferHandle];
	MiniTriVertex output = vertexBuffer[vertexID];

	// Transform from object space into world space by multiplying the vertex position by the world/view/projection matrix.
	output.position = mul(_renderData.wvp, output.position);

	return output;
}

// Our pixel shader.
// This applies coloring (shading) to the pixel that is currently rendering on the GPU.
// For this example, our output is very simple.  We just return the color of the vertex.  In more complex examples we could sample from 
// a texture, performing per-pixel lighting, etc...
float4 MiniTriPS(MiniTriVertex vertex) : SV_Target
{
	return vertex.color;
}