Texture2D _gorgonTexture : register(t0);
Texture2D _backTexture : register(t1);
SamplerState _gorgonSampler : register(s0);
SamplerState _backSampler : register(s1);
 
struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 uv : TEXTURECOORD0;
};
 
struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 uv : TEXTURECOORD0;
};
 
PS_IN VS( VS_IN input )
{
	return input;
}

float4 PS( PS_IN input ) : SV_Target
{
	return _gorgonTexture.Sample(_gorgonSampler, input.uv) * input.col;
//	return float4((_gorgonTexture.Sample(_gorgonSampler, input.uv).rgb / 2.0f) * _backTexture.Sample(_backSampler, input.uv).rgb, _gorgonTexture.Sample(_gorgonSampler, input.uv).a) * input.col;
/*	float4 topTexel = _gorgonTexture.Sample(_gorgonSampler, input.uv);
	float4 bottomTexel = _backTexture.Sample(_backSampler, input.uv);

	if (input.uv.x >= 0.5f)
	{
		float2 newUV = float2(input.uv.x, input.uv.y + 0.5f);
		float4 mask = _gorgonTexture.Sample(_gorgonSampler, newUV);

		if ((mask.r != 0) || (mask.g != 0) || (mask.b != 0))
			return float4(topTexel.rgb * bottomTexel.rgb, topTexel.a * input.col.a);
	}

	return topTexel * input.col;*/
}
