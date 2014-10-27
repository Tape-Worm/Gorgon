// Shaders for image viewing.
#GorgonInclude "Gorgon2DShaders"

// Our default texture and sampler.
Texture1D _gorgonTexture1D : register(t0);
Texture1DArray _gorgonTexture1DArray : register(t0);
Texture2DArray _gorgonTexture2DArray : register(t0);
Texture3D _gorgonTexture3D : register(t0);
TextureCube _gorgonTextureCube : register(t0);
#ifdef SM41
TextureCubeArray _gorgonTextureCubeArray : register(t0);
#endif

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

// Pixel shader to view a 1D texture array.
float4 Gorgon1DTextureArrayView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture1DArray.Sample(_gorgonSampler, float2(vertex.uv.x, 0));

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

// Pixel shader to view a 2D texture array.
float4 Gorgon2DTextureArrayView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture2DArray.Sample(_gorgonSampler, float3(vertex.uv, 0));

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 2D texture cube.
float4 Gorgon2DTextureCubeView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTextureCube.Sample(_gorgonSampler, float3(vertex.uv, 0));

	REJECT_ALPHA(color.a);
		
	return color;
}

#ifdef SM41
// Pixel shader to view a 2D texture cube array.
float4 Gorgon2DTextureCubeArrayView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTextureCubeArray.Sample(_gorgonSampler, float4(vertex.uv, 0, 0));

	REJECT_ALPHA(color.a);
		
	return color;
}
#endif

// Pixel shader to view a 3D texture.
float4 Gorgon3DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float3 coords = float3(vertex.uv.x, vertex.uv.y, depthSlice);
	float4 color = _gorgonTexture3D.Sample(_gorgonSampler, coords);

	REJECT_ALPHA(color.a);
		
	return color;
}