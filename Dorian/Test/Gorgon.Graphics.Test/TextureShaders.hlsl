Texture2D<int> _texture : register(t0);
Texture2D _textureView2 : register(t1);
Texture2D _dynTexture : register(t2);
SamplerState _sampler : register(s0);

struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 uv: TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 uv: TEXCOORD;
};

PS_IN TestVS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = input.pos;
	output.col = input.col;
	output.uv = input.uv;
	
	return output;
}


float4 TestPS( PS_IN input ) : SV_Target
{
	float4 pixel = _texture.Load(int3((int)(input.uv.x * 256), (int)(input.uv.y * 256), 0));

	pixel += 1.0f;
	pixel *= 0.5f;	
	pixel.w = 1.0f;

	return (_textureView2.Sample(_sampler, input.uv) + pixel) * input.col;		
}

float4 TestPSUpdateSub(PS_IN input) : SV_Target
{
	float4 dynTexel = _dynTexture.Sample(_sampler, input.uv);
	float4 texel = _textureView2.Sample(_sampler, input.uv);

	if (input.uv.x <= 0.5f)
	{
		return dynTexel;
	}

	return texel;
}

float4 TestPSGeneric(PS_IN input) : SV_Target
{
	return _textureView2.Sample(_sampler, input.uv);
}