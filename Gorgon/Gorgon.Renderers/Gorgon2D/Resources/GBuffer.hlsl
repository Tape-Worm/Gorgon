#GorgonInclude "Gorgon2DShaders"

// Parameters for the effect.
cbuffer EffectParams : register(b1)
{
	// Flag to indicate which array indices are to be used for normal and specular maps.
    float4 _arrayIndices;
}

// Output data from our deferred vertex shader for lighting.
struct GorgonLitVertex
{
    float4 position : SV_POSITION;
	float4 worldPos : WORLDPOS;
    float4 color : COLOR;
    float4 uv : TEXCOORD;
    float3 tangent : TANGENT;
    float3 bitangent : BITANGENT;
};

// The output for the GBuffer targets.
struct GorgonGBufferOutput
{
	// The diffuse texel.
    float4 Diffuse : SV_Target0;
	// The normal map texel.
    float4 Normal : SV_Target1;
	// The specular map texel.
    float4 Specular : SV_Target2;
	// The position buffer.
	float4 Position : SV_Target3;
};

// Normal and specular map.
Texture2D _normalTexture : register(t1);
SamplerState _normalSampler : register(s1);
Texture2D _specularTexture : register(t2);
SamplerState _specularSampler : register(s2);
Texture2D _positionTexture : register(t3);
SamplerState _positionSampler : register(s3);

// Updated vertex shader that will perform tangent and bitangent transforms.
GorgonLitVertex GorgonVertexShaderGBuffer(GorgonSpriteVertex vertex)
{
    GorgonLitVertex output;
	
	output.worldPos = vertex.position;
	output.position = mul(ViewProjection, vertex.position);
	output.uv = vertex.uv;
	output.color = vertex.color;

	// We encode our rotation cosine and sine in our vertex data so that we don't need to perform the calculation more than needed.
	float3x3 rotation = float3x3(vertex.angle.x, -vertex.angle.y, 0, vertex.angle.y, vertex.angle.x, 0, 0, 0, 1);

	// Build up our tangents.  Without this, rotating a sprite would not look right when lit.
	output.tangent = normalize(mul(rotation, float3(1, 0, 0)));
	output.bitangent = normalize(cross(output.tangent, float3(0, 0, -1)));

	return output;
}

// Renders the normal maps to multiple render targets in the GBuffer.
GorgonGBufferOutput GorgonPixelShaderGBuffer(GorgonLitVertex vertex) : SV_Target
{	
	GorgonGBufferOutput result;

	float3 texCoords = float3(vertex.uv.xy / vertex.uv.w, vertex.uv.z);

	result.Position = float4(vertex.worldPos.rgb, 1.0f);
	result.Diffuse = _gorgonTexture.Sample(_gorgonSampler, texCoords) * vertex.color;
	
	REJECT_ALPHA(result.Diffuse.a);    	

    // Transform normals. If we rotate, the normals in the normal map need to be adjusted.
#ifdef USE_ARRAY
	float3 bump = normalize((_arrayIndices.x < 0 ? float3(0.5f, 0.5f, 1.0f) : _gorgonTexture.Sample(_gorgonSampler, float3(texCoords.xy, _arrayIndices.x)).xyz) * 2.0f - 1.0f);	
#else
    float3 bump = normalize((_normalTexture.Sample(_normalSampler, texCoords.xy).xyz * 2.0f) - 1.0f);
#endif
    result.Normal = float4((normalize(float3(0, 0, bump.z) + (bump.x * vertex.tangent + bump.y * vertex.bitangent)) + 1.0f) * 0.5f, 1.0f);

#ifdef USE_ARRAY
	result.Specular = _arrayIndices.y < 0 ? float4(0, 0, 0, 1) : _gorgonTexture.Sample(_gorgonSampler, float3(texCoords.xy, _arrayIndices.y));
#else
    result.Specular = _specularTexture.Sample(_specularSampler, texCoords.xy);
#endif

	return result;
}
