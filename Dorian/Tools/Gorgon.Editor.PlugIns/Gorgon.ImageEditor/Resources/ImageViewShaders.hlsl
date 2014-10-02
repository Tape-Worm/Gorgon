// Shaders for image viewing.
#GorgonInclude "Gorgon2DShaders"

// Our default texture and sampler.
Texture1D _gorgonTexture1D : register(t0);
Texture3D _gorgonTexture3D : register(t0);

// The depth/array index to view.
cbuffer GorgonDepthArrayIndex {
	float depthArrayIndex : register(b1)
}

// Pixel shader to view a 1D texture.
float4 Gorgon1DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float2 coords = float2(vertex.u, depthArrayIndex);
	float4 color = _gorgonTexture1D.Sample(_gorgonSampler, coords);

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 1D texture.
float4 Gorgon2DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float3 coords = float3(vertex.u, vertex.v, depthArrayIndex);
	float4 color = _gorgonTexture1D.Sample(_gorgonSampler, coords);

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 3D texture.
float4 Gorgon3DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float3 coords = float3(vertex.u, vertex.v, depthArrayIndex);
	float4 color = _gorgonTexture3D.Sample(_gorgonSampler, coords);

	REJECT_ALPHA(color.a);
		
	return color;
}