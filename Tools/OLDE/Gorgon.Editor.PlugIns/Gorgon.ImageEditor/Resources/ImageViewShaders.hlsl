// Shaders for image viewing.
#GorgonInclude "Gorgon2DShaders"

// Our default texture and sampler.
Texture1D _gorgonTexture1D : register(t0);
Texture1DArray _gorgonTexture1DArray : register(t0);
Texture2DArray _gorgonTexture2DArray : register(t0);
Texture3D _gorgonTexture3D : register(t0);
TextureCube _gorgonTextureCube : register(t0);

// The texture parameters
cbuffer GorgonTextureParams : register(b1)
{
	// Depth slice to view.
	float depthSlice;
	// Index of the array to view.
	float arrayIndex;
	// Mip level to view.
	float mipLevel;
};

// Pixel shader to view a 1D texture.
float4 Gorgon1DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture1D.SampleLevel(_gorgonSampler, vertex.uv.x, mipLevel);

	color.a = color.a * vertex.color.a;

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 1D texture array.
float4 Gorgon1DTextureArrayView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture1DArray.SampleLevel(_gorgonSampler, float2(vertex.uv.x, arrayIndex), mipLevel);

	color.a = color.a * vertex.color.a;

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 2D texture.
float4 Gorgon2DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture.SampleLevel(_gorgonSampler, vertex.uv, mipLevel);

	color.a = color.a * vertex.color.a;

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 2D texture array.
float4 Gorgon2DTextureArrayView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture2DArray.SampleLevel(_gorgonSampler, float3(vertex.uv, arrayIndex), mipLevel);

	color.a = color.a * vertex.color.a;

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 3D texture.
float4 Gorgon3DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float3 coords = float3(vertex.uv.x, vertex.uv.y, depthSlice);
	float4 color = _gorgonTexture3D.SampleLevel(_gorgonSampler, coords, mipLevel);

	color.a = color.a * vertex.color.a;

	REJECT_ALPHA(color.a);
		
	return color;
}