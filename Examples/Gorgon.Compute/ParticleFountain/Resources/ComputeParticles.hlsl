#GorgonInclude "ParticleShaderCommon"

static const float PI = 3.14159265f;

// Data needed to perform the particle computation.
struct ComputeData
{
    // The view handle for the particle buffer.
    int particleBufferHandle;
    // The current number of particles that can be rendered.
    int maxParticles;
    // A seed value to help with randomness.
    uint randomSeed;
    // The current running time.
    float runningTime;
    // The initial diffuse colour of all particles.
    float4 initialDiffuse;
};

ConstantBuffer<ComputeData> _computeData : register(b0);

// A hashing function for randomness.
// Jarzynski and Olano, "Hash Functions for GPU Rendering", JCGT 2020
uint PcgHash(uint input)
{
    uint state = input * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

// Converts a hash value into a normalized floating point value.
float HashToNormalFloat(uint hash)
{
    return (hash >> 8) / 16777216.0f;
}

// Function to calculate random velocities.
float4 RandomVelocity(uint hash, float3 scale)
{
    uint hash0 = PcgHash(hash ^ PcgHash(_computeData.randomSeed));
    uint hash1 = PcgHash(hash0);
    uint hash2 = PcgHash(hash1);
    float x = HashToNormalFloat(hash0) * (2 * PI);
    float y = HashToNormalFloat(hash1) * (PI / 4.0f);
    float z = HashToNormalFloat(hash2);    

    return float4(cos(x) * sin(y), cos(y) * z, sin(x) * sin(y), 0.0f) * float4(scale, 0);
}

// Function to initialize a new particle.
ParticleData InitParticle(uint threadID, float runningTime, float4 initialDiffuse)
{
    ParticleData particle;

    particle.options.z = 1.0f;
    particle.options.y = 1.0f;
    particle.options.x = runningTime;
    particle.options.w = 0;   
    particle.initialPosition = particle.currentPosition = float4(0, 0.03f, 0, 1);
    particle.initialVelocity = particle.currentVelocity = RandomVelocity(threadID, float3(2.5f, 8.0f, 2.5f));
    particle.initialDiffuse = particle.currentDiffuse = initialDiffuse; //float4(0.59f, 0.52f, 0.20f, 1.0f);

    return particle;
}

// Function to initialize a new spark.
ParticleData InitSpark(uint threadID, ParticleData parent, float runningTime)
{
    ParticleData particle;

    particle.options.z = 1.0f;
    particle.options.y = 1.0f;
    particle.options.x = runningTime;
    particle.options.w = 1.0f;   
    particle.initialPosition = particle.currentPosition = float4(parent.currentPosition.x, 0.03f, parent.currentPosition.z, 1);
    particle.initialVelocity = particle.currentVelocity = RandomVelocity(threadID, float3(1.0f, 1.5f, 1.0f)) + float4(parent.currentVelocity.x * 0.25f, 0, parent.currentVelocity.z * 0.25f, 0);
    particle.initialDiffuse = particle.currentDiffuse = parent.currentDiffuse;

    return particle;
}

// Function to update the particle.
ParticleData UpdateParticle(int index, ParticleData input, float runningTime)
{
    bool isParticle = input.options.w == 0;
    float gravity = isParticle ? -9.8f : -5.0f;
    ParticleData output = input;

    float age = runningTime - input.options.x;
    output.currentPosition = float4(input.initialPosition.xyz + input.initialVelocity.xyz * age, 1.0f);
    output.currentPosition.y += 0.5f * gravity * age * age;
    output.options.y = saturate(1 - age * (isParticle ? 0.25f : 2.25f));

    if ((output.currentPosition.y < 0.03f) || ((!isParticle) && (output.options.y <= 0)))
    {
        output.options.z = 0.0f;
        return output;
    }
    
    output.currentVelocity.y = input.initialVelocity.y + gravity * age;
    if (isParticle)
    {
        output.currentDiffuse = lerp(float4(1.0f, 0.10f, 0.05f, 1.0f), input.initialDiffuse, output.options.y);
    }
    else
    {
        output.currentDiffuse = lerp(float4(0, 0, 0, 1.0f), input.initialDiffuse, output.options.y);
    }

    return output;
}

// The shader used to update the particles.
[numthreads(64, 1, 1)]
void ComputeParticles(uint3 threadID : SV_DispatchThreadID)
{
    RWStructuredBuffer<ParticleData> particleBuffer = ResourceDescriptorHeap[_computeData.particleBufferHandle];

    // * 5 because we need room for any sparks that might pop up.
    if (threadID.x >= _computeData.maxParticles * 5)
    {
        return;
    }

    ParticleData particle = particleBuffer[threadID.x];

    if (particle.options.z == 0)
    {
        // Only dead particles (items between 0 and the max particle count - 1) are revived here.
        if (threadID.x < _computeData.maxParticles)
        {
            particleBuffer[threadID.x] = InitParticle(threadID.x, _computeData.runningTime, _computeData.initialDiffuse);
        }
    }
    else
    {
        particle = particleBuffer[threadID.x] = UpdateParticle(threadID.x, particle, _computeData.runningTime);

        // If we're a normal particle about to die, then create some sparks.
        if ((particle.options.z < 1.0f) && (particle.options.w < 1.0f))
        {
            int index = _computeData.maxParticles + threadID.x * 4;

            for (int i = index; i < index + 4; ++i)
            {
                particleBuffer[i] = InitSpark(i, particle, _computeData.runningTime);
            }
        }
    }
}