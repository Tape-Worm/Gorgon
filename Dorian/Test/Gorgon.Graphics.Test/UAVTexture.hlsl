Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);

RWTexture2D<uint> _uavText : register(u1);

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 uv: TEXCOORD;
};

float4 TestUAV(PS_IN input) : SV_Target
{
	float width;
	float height;
	
	_uavText.GetDimensions(width, height);
	
	uint value = _uavText[uint2((uint)(input.uv.x * width) -16, (uint)(input.uv.y * height)-16)];
	float4 srcColor = float4((value & 0xFF)  / 255.0f, ((value >> 8) & 0xFF)  / 255.0f, ((value >> 16) & 0xFF)  / 255.0f, ((value >> 24) & 0xFF) / 255.0f);
	float4 destColor = srcColor;
	
	destColor.b = destColor.g = destColor.r = (destColor.r + destColor.g + destColor.b) / 3.0f;

	destColor.rgb *= (1.0f - 0.125f);
	destColor.rgb += srcColor.rgb * 0.125f;

	value = ((uint)(destColor.a * 255.0f) << 24) | ((uint)(destColor.b * 255.0f) << 16) | ((uint)(destColor.g * 255.0f) << 8) | (uint)(destColor.r * 255.0f);
	

	_uavText[uint2((uint)(input.uv.x * width)-16, (uint)(input.uv.y * height)-16)] = value;

	return float4(1, 1, 1, 1);//srcColor;
}

float4 TestPS(PS_IN input) : SV_Target
{
	return _texture.Sample(_sampler, input.uv);
}
