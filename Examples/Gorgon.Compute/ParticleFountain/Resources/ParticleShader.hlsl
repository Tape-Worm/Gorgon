#GorgonInclude "ParticleShaderCommon"

struct ParticleRenderData
{
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
    int particleBufferHandle;
    int samplerHandle;
    int textureHandle;
};

ConstantBuffer<ParticleRenderData> _particleRenderData : register(b0);

static const float2 _corners[6] = 
{
    float2(-1, 1),
    float2(1, 1),
    float2(-1, -1),

    float2(-1, -1),
    float2(1, 1),
    float2(1, -1),
};

// Converts the particle speed (squared) into a step count.
int GetStepCount(float speed)
{
    if (speed < 1)
    {
        return 2;
    }
    else if (speed < 4)
    {
        return 3;
    }
    else if (speed < 9)
    {
        return 4;
    }
    else if (speed < 12.25f)
    {
        return 5;
    }
    else if (speed < 16)
    {
        return 6;
    }
    else if (speed < 20.25)
    {
        return 7;
    }

    return 8;
}

// Our default vertex shader.
OutVertex VertexShader(uint id : SV_VertexID)
{
    StructuredBuffer<ParticleData> particleBuffer = ResourceDescriptorHeap[_particleRenderData.particleBufferHandle];
    OutVertex output;

    int index = id / 48;
    int step = (id / 6) % 8;
    int corner = id % 6;

    ParticleData particle = particleBuffer[index];

    float speed = dot(particle.currentVelocity, particle.currentVelocity);
    int stepCount = GetStepCount(speed);

    if ((step >= stepCount) || (particle.options.z < 1.0f))
    {
        output.position = float4(0, 0, 0, 0);
        output.color = float4(0, 0, 0, 0);
        output.uv = float2(0, 0);

        return output;
    }

    float4 trailPosition = float4(particle.currentPosition.xyz + particle.currentVelocity.xyz * (-0.04f / stepCount) * step, 1);
    float4 transformedPosition = mul(trailPosition, _particleRenderData.viewMatrix);
    
    output.position = mul(float4(transformedPosition.xy + _corners[corner] * 0.03f, transformedPosition.z, 1.0f), _particleRenderData.projectionMatrix);
    output.uv = _corners[corner] * float2(0.5f, -0.5f) + 0.5f;    
    output.color = particle.currentDiffuse;

    return output;
}

// The vertex shader for drawing particle lights.
OutVertex LightVertexShader(uint id : SV_VertexID)
{
    StructuredBuffer<ParticleData> particleBuffer = ResourceDescriptorHeap[_particleRenderData.particleBufferHandle];
    OutVertex output;

    int index = id / 6;
    int corner = id % 6;

    ParticleData particle = particleBuffer[index];

    float speed = dot(particle.currentVelocity, particle.currentVelocity);
    int stepCount = GetStepCount(speed);

    if ((particle.currentPosition.y >= 1.0f) || (particle.options.z < 1.0f))
    {
        output.position = float4(0, 0, 0, 0);
        output.color = float4(0, 0, 0, 0);
        output.uv = float2(0, 0);

        return output;
    }

    float4 trailPosition = float4(particle.currentPosition.x, 0, particle.currentPosition.z, 1.0f);
    trailPosition.xz += _corners[corner] * (saturate(particle.currentPosition.y)  * 0.25f + 0.03f);
    float4 transformedPosition = mul(trailPosition, _particleRenderData.viewMatrix);    
    
    output.position = mul(transformedPosition, _particleRenderData.projectionMatrix);
    output.uv = _corners[corner] * float2(0.5f, -0.5f) + 0.5f;    
    output.color = particle.currentDiffuse * ((1.0f - saturate(particle.currentPosition.y)) * 0.5f);

    return output;
}

// Our pixel shader that will render objects with textures.
float4 PixelShader(OutVertex vertex) : SV_Target
{
    SamplerState textureSampler = SamplerDescriptorHeap[_particleRenderData.samplerHandle];
    Texture2D texture = ResourceDescriptorHeap[_particleRenderData.textureHandle];

    return vertex.color * texture.Sample(textureSampler, vertex.uv);
}
