// Shaders for image viewing.
#GorgonInclude "Gorgon2DShaders"

// Our default texture and sampler.
Texture2DArray _gorgonTexture2DArray : register(t0);
TextureCube _gorgonTextureVolume : register(t0);
Texture3D _gorgonTexture3D : register(t1);			// Store this in slot 1 so that we can render a slice using our 2D renderer (which only accepts 2D views)..
SamplerState _gorgon3DSampler : register(s1);

// The texture parameters
cbuffer GorgonTextureParams : register(b1)
{
	// Depth slice to view.
	float depthSlice;
	// Mip level to view.
	float mipLevel;
};

// Pixel shader to view a 2D texture array.
float4 Gorgon2DTextureArrayView(GorgonSpriteVertex vertex) : SV_Target
{
	float4 color = _gorgonTexture2DArray.SampleLevel(_gorgonSampler, vertex.uv, mipLevel);

	color.a = color.a * vertex.color.a;

	REJECT_ALPHA(color.a);
		
	return color;
}

// Pixel shader to view a 3D texture.
float4 Gorgon3DTextureView(GorgonSpriteVertex vertex) : SV_Target
{
	float3 coords = float3(vertex.uv.xy, depthSlice);
	float4 color = _gorgonTexture3D.SampleLevel(_gorgon3DSampler, coords, mipLevel);

	color.a = color.a * vertex.color.a;

	REJECT_ALPHA(color.a);
		
	return color;
}
