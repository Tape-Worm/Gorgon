Texture2D _gorgonTexture : register(t0);
SamplerState _gorgonSampler : register(s0);
 
struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 uv : TEXTURECOORD;
};
 
struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 uv : TEXTURECOORD;
};
 
PS_IN VS( VS_IN input )
{
	return input;
}

float4 PS( PS_IN input ) : SV_Target
{		
	return _gorgonTexture.Sample(_gorgonSampler, input.uv) * input.col;
}
