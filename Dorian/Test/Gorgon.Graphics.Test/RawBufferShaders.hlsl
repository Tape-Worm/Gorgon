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
	uint x = (uint)(input.uv.x * 255);
	uint y = (uint)(input.uv.y * 255);
	uint pos = y * 1024 + x * 4;
	uint value = _bufferView.Load(pos);
	float4 color = float4(((value >> 24) & 0xff) / 255.0f, ((value >> 16) & 0xff) / 255.0f, ((value >> 8) & 0xff) / 255.0f, (value & 0xff) / 255.0f);	

	return color + input.col;
}