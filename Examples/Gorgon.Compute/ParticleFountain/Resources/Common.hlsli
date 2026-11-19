// Our vertex.
struct Vertex
{
   float4 position : SV_POSITION;
   float4 diffuse : COLOR0;
   float2 uv : TEXCOORD;
};

// The output vertex.
struct OutVertex
{
   float4 position : SV_POSITION;
   float4 color : COLOR0;
   float2 uv : TEXCOORD;
};

// Particle data.
struct ParticleData
{
    // Current particle position, initial particle velocity.
    float4 currentPosition;
    float4 initialPosition;

    // Current particle velocity, initial particle velocity.
    float4 currentVelocity;
    float4 initialVelocity;

    // Initial diffuse colour, current diffuse colour.
    float4 currentDiffuse;
    float4 initialDiffuse;

    // X = time of creation, Y = fade progression, Z = alive (1)/dead (0), W = normal (0)/spark(1).
    float4 options;
};
