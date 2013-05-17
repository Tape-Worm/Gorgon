Texture2D _texture : register(t0);
Texture2D _textureView2 : register(t1);
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
	return (_textureView2.Sample(_sampler, input.uv)) * input.col;		
}