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

cbuffer wvp : register(c0)
{
	float4x4 world;
	float4x4 projection;
	float4x4 view;
};


PS_IN TestVS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(float4(input.pos.xyz, 1.0f), world);
	output.pos = mul(output.pos, view);
	output.pos = mul(output.pos, projection);

	output.col = input.col;
	output.uv = input.uv;
	
	return output;
}

float4 TestPS( PS_IN input ) : SV_Target
{
	return input.col;
}