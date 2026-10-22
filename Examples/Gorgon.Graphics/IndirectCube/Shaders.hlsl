// Data used to render the scene per frame.
struct RenderData
{
    // The world matrix for transforming vertices.
    float4x4 world;

    // The projection matrix for projecting to the display.
    float4x4 projection;

    // The time, in seconds, used to animate the wobble.
    float time;

    // The strength of the wobble. 0 means the cube is at rest.
    float amplitude;

    // The number of slices in the stack.
    uint sliceCount;

    // The handle for the texture view on the texture descriptor heap.
    int textureHandle;

    // The handle for the sampler on the sampler descriptor heap.
    int samplerHandle;
};

// The vertex sent from the geometry shader to the pixel shader.
struct VertexOutput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD;
    float shade : SHADE;
};

// The constant buffer holding the per frame render data.
ConstantBuffer<RenderData> _renderData : register(b0);

// The execute constants for the command being drawn. Each command in the execute call writes its slice index into the first value.
cbuffer ExecuteConstants : register(b0, space1)
{
    uint4 _executeConstants;
};

static const float PI = 3.14159265f;

// How fast the cube wobbles, in radians per second.
static const float WobbleSpeed = 9.0f;

// The direction the light shines from.
static const float3 LightDirection = normalize(float3(-0.4f, 0.6f, -0.7f));

// Our vertex shader. The commands draw a single point each, so there's nothing to read here. We just pass the ID along to the geometry shader.
uint VsMain(uint id : SV_VertexID) : VERTEXID
{
    return id;
}

// Function to return the position of a corner on one of the levels between the slices.
// Level 0 is the bottom of the cube, and level sliceCount is the top. Moving whole levels (instead of whole slices) keeps the sides of neighbouring slices joined.
float3 GetCorner(float x, float z, uint level)
{
    float height = level / (float)_renderData.sliceCount;
    float phase = (WobbleSpeed * _renderData.time) - (height * 1.2f);

    // The bottom of the cube stays planted, and the sway grows towards the top.
    float2 sway = _renderData.amplitude * height * height * float2(sin(phase), 0.6f * sin((phase * 1.3f) + 0.7f));

    // Squash and stretch the cube. When it gets taller, the middle gets thinner, and when it gets shorter, the middle bulges.
    float squash = _renderData.amplitude * 0.2f * sin(WobbleSpeed * 2.0f * _renderData.time);
    float width = 1.0f - (squash * 0.5f * sin(PI * height));

    return float3((x * width) + sway.x, -0.5f + (height * (1.0f + squash)), (z * width) + sway.y);
}

// Function to write a face of a slice to the output stream.
// The corners are given as they would be seen from outside of the cube: top left, top right, bottom left, then bottom right.
void EmitFace(float3 topLeft, float3 topRight, float3 bottomLeft, float3 bottomRight, float vTop, float vBottom, inout TriangleStream<VertexOutput> output)
{
    float4 corners[4] =
    {
        mul(_renderData.world, float4(topLeft, 1.0f)),
        mul(_renderData.world, float4(topRight, 1.0f)),
        mul(_renderData.world, float4(bottomLeft, 1.0f)),
        mul(_renderData.world, float4(bottomRight, 1.0f))
    };

    float2 uvs[4] =
    {
        float2(0, vTop),
        float2(1, vTop),
        float2(0, vBottom),
        float2(1, vBottom)
    };

    // The slices are sheared while they wobble, so we build the normal from the corners instead of using a fixed one.
    float3 normal = normalize(cross(corners[1].xyz - corners[0].xyz, corners[2].xyz - corners[0].xyz));
    float shade = 0.35f + (0.65f * saturate(dot(normal, LightDirection)));

    VertexOutput vertex;

    for (int i = 0; i < 4; ++i)
    {
        vertex.position = mul(_renderData.projection, corners[i]);
        vertex.uv = uvs[i];
        vertex.shade = shade;
        output.Append(vertex);
    }

    output.RestartStrip();
}

// Our geometry shader.
// This builds a single slice of the cube from the point sent by the command. The slice to build comes from the execute constants for the command.
[maxvertexcount(24)]
void GsMain(point uint input[1] : VERTEXID, inout TriangleStream<VertexOutput> output)
{
    uint slice = _executeConstants.x;
    uint bottom = slice;
    uint top = slice + 1;

    // Each slice covers its own part of the texture on the sides, so the texture covers the whole side of the cube.
    float vTop = 1.0f - (top / (float)_renderData.sliceCount);
    float vBottom = 1.0f - (bottom / (float)_renderData.sliceCount);

    // Front.
    EmitFace(GetCorner(-0.5f, -0.5f, top), GetCorner(0.5f, -0.5f, top), GetCorner(-0.5f, -0.5f, bottom), GetCorner(0.5f, -0.5f, bottom), vTop, vBottom, output);
    // Back.
    EmitFace(GetCorner(0.5f, 0.5f, top), GetCorner(-0.5f, 0.5f, top), GetCorner(0.5f, 0.5f, bottom), GetCorner(-0.5f, 0.5f, bottom), vTop, vBottom, output);
    // Right.
    EmitFace(GetCorner(0.5f, -0.5f, top), GetCorner(0.5f, 0.5f, top), GetCorner(0.5f, -0.5f, bottom), GetCorner(0.5f, 0.5f, bottom), vTop, vBottom, output);
    // Left.
    EmitFace(GetCorner(-0.5f, 0.5f, top), GetCorner(-0.5f, -0.5f, top), GetCorner(-0.5f, 0.5f, bottom), GetCorner(-0.5f, -0.5f, bottom), vTop, vBottom, output);

    // The slices are stacked, so only the top slice needs a lid, and only the bottom slice needs a floor.
    if (top == _renderData.sliceCount)
    {
        EmitFace(GetCorner(-0.5f, 0.5f, top), GetCorner(0.5f, 0.5f, top), GetCorner(-0.5f, -0.5f, top), GetCorner(0.5f, -0.5f, top), 0, 1, output);
    }

    if (bottom == 0)
    {
        EmitFace(GetCorner(-0.5f, -0.5f, bottom), GetCorner(0.5f, -0.5f, bottom), GetCorner(-0.5f, 0.5f, bottom), GetCorner(0.5f, 0.5f, bottom), 0, 1, output);
    }
}

// Our pixel shader for drawing to our render target.
float4 PsMain(VertexOutput vertex) : SV_Target
{
    SamplerState sampler = SamplerDescriptorHeap[_renderData.samplerHandle];
    Texture2D texture = ResourceDescriptorHeap[_renderData.textureHandle];

    // We don't want blending for this example, so the alpha from the texture is ignored.
    return float4(texture.Sample(sampler, vertex.uv).rgb * vertex.shade, 1.0f);
}
