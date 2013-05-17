ByteAddressBuffer _bufferView;

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

PS_IN TestVS(VS_IN input )
{
	PS_IN output = (PS_IN)0;	
	
	output.pos = input.pos;
	output.col = input.col;
	output.uv = input.uv;
	
	return output;
}

float4 TestPS( PS_IN input ) : SV_Target
{
	uint value = _bufferView.Load4((uint)(input.pos.x * 255.0f));
	float4 color = float4(((value << 24) & 0xff) / 255.0f, ((value << 16) & 0xff) / 255.0f, ((value << 8) & 0xff), (value & 0xff) / 255.0f);

	return input.col * color;
}