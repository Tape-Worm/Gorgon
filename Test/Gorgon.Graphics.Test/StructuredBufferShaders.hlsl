struct MyCoord
{
	float x;
	float y;
	float z;
};

StructuredBuffer<MyCoord> _bufferView;

struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 uv: TEXCOORD;
	uint vtxID : SV_VertexID;
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
	MyCoord buffer = _bufferView[0];
	
	output.pos = float4(input.pos.x + buffer.x, input.pos.y + buffer.y, input.pos.z + buffer.z, 1.0f);
	output.col = input.col;
	output.uv = input.uv;
	
	return output;
}

float4 TestPS( PS_IN input ) : SV_Target
{
	return input.col;
}