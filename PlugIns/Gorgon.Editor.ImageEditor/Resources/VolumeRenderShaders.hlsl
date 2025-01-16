// Our default texture and sampler.
Texture3D _volumeTexture : register(t0);
SamplerState _volumeSampler : register(s0);
Texture2D _cubeFront : register(t1);
SamplerState _cubeFrontSampler : register(s1);
Texture2D _cubeBack : register(t2);
SamplerState _cubeBackSampler : register(s2);

// A vertex for the volume cube.
struct VolumeVertex
{	
	float4 position : SV_POSITION;
	float3 uvw : TEXCOORD;
};

// A vertex for a cube.
struct VolumeVertexOut
{	
	// The position of the vertex.
	float4 position : SV_POSITION;
	// The texture coordinate for the vertex.
	float3 uvw : TEXCOORD0;
	// The unprocessed world position (no viewport applied) after transform.
	float4 worldPosition : TEXCOORD1;
};

// The transformation matrices combined together.
cbuffer WorldViewProjection : register(b0)
{
	float4x4 WVP;
};

// The parameters for volume rendering.
cbuffer VolumeRayParams : register(b0)
{
	// The number steps between slices.
	float3 stepSize;	
	// The number of iterations to build up the slice view.
	int iterations = 1;	
};

// The scaling factor for the vertex shader.
cbuffer VolumeScaleParams : register(b1)
{
	float4 scaleFactor;
};

// Vertex shader for the 3D texture volume.
VolumeVertexOut VolumeVS(VolumeVertex vertex)
{
	VolumeVertexOut output;

	// Transform from object space into world space by multiplying the vertex position by the world/view/projection matrix.	
	output.position = mul(vertex.position * scaleFactor, WVP);	
	output.worldPosition = output.position;
	output.uvw = float3((vertex.position.x  + 0.5f) * scaleFactor.x, (-vertex.position.y + 0.5f) * scaleFactor.y, (-vertex.position.z + 0.5f) * scaleFactor.z);

	return output;
}

// Pixel shader used to render the positions within the cube volume.
float4 VolumePositionPS(VolumeVertexOut vertex) : SV_Target
{
	return float4(vertex.uvw, 1.0f);
}

// Pixel shader used to the volume using raycasting to build up the slices and the intersection of the view and volume area.
float4 VolumeRayCastPS(VolumeVertexOut vertex) : SV_Target
{
	float2 textureCoord = (vertex.worldPosition.xy / vertex.worldPosition.w);
	textureCoord.x = 0.5f * textureCoord.x + 0.5f;
	textureCoord.y = -0.5f * textureCoord.y + 0.5f;

	float3 front = _cubeFront.Sample(_cubeFrontSampler, textureCoord).rgb;
	float3 back = _cubeBack.Sample(_cubeBackSampler, textureCoord).rgb;
	float3 direction = normalize(back - front);
	// Start from the front and layer.
	float4 position = float4(front, 0);
	float4 dest = 0;
	float4 src= 0;
	float3 step = direction * stepSize;

	[loop]
	for (int i = 0; i < iterations; ++i)
	{
		position.w = 0;
		src = _volumeTexture.Sample(_volumeSampler, position.xyz);

		// Blend front to back.
		src.rgb *= src.a;
		dest = (1.0f - dest.a) * src + dest;		
		
		// If the destination is nearly opaque, then we can consider this pixel done.
		if (dest.a >= 0.95f)
		{
			break;	
		}
		
		// Move on to the next volume position.
		position.xyz += step;

		if((position.x > 1.0f) 
			|| (position.y > 1.0f) 
			|| (position.z > 1.0f))
		{
			break;
		}			
	}

	return dest;
}