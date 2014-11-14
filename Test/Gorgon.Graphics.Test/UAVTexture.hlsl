Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);

RWTexture2D<uint> _uavText : register(u1);

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

void TestUAV(PS_IN input)
{
	float width;
	float height;
	
	_uavText.GetDimensions(width, height);
	
	uint value = _uavText[uint2((uint)(input.uv.x * width), (uint)(input.uv.y * height))];
	float4 srcColor = float4((value & 0xFF)  / 255.0f, ((value >> 8) & 0xFF)  / 255.0f, ((value >> 16) & 0xFF)  / 255.0f, ((value >> 24) & 0xFF) / 255.0f);
	value = _uavText[uint2((uint)(input.uv.x * width), (uint)(input.uv.y * height))];
	float4 destColor = float4((value & 0xFF)  / 255.0f, ((value >> 8) & 0xFF)  / 255.0f, ((value >> 16) & 0xFF)  / 255.0f, ((value >> 24) & 0xFF) / 255.0f);;
	
	destColor.b = destColor.g = destColor.r = (destColor.r + destColor.g + destColor.b) / 3.0f;

	destColor.rgb = saturate(((srcColor.a * 0.25f) * srcColor.rgb) + ((1.0f - (srcColor.a * 0.25f)) * destColor.rgb));

	value = ((uint)(destColor.a * 255.0f) << 24) | ((uint)(destColor.b * 255.0f) << 16) | ((uint)(destColor.g * 255.0f) << 8) | (uint)(destColor.r * 255.0f);
	
	_uavText[uint2((uint)(input.uv.x * width), (uint)(input.uv.y * height))] = value;
}

float4 TestPS(PS_IN input) : SV_Target
{
	return _texture.Sample(_sampler, input.uv);
}
