// Shaders for image viewing.
#GorgonInclude "Gorgon2DShaders"

// Our default texture and sampler.
Texture1D _gorgonTexture1D : register(t0);
Texture3D _gorgonTexture3D : register(t0);

// The depth slice to view.
cbuffer GorgonDepthSlice : register(b1)
{
	float depthSlice;
};

// Pixel shader to view a 1D texture.
float4 Gorgon1DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture1D.Sample(_gorgonSampler, vertex.uv.x);

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 2D texture.
float4 Gorgon2DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.Sample(_gorgonSampler, vertex.uv);

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 3D texture.
float4 Gorgon3DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float3 coords = float3(vertex.uv.x, vertex.uv.y, depthSlice);
	float4 color = _gorgonTexture3D.Sample(_gorgonSampler, coords);

	REJECT_ALPHA(color.a);
		
	return color;
}