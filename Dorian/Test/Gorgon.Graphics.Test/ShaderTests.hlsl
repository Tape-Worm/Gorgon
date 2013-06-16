Texture2D _texture : register(t0);
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
	
	output.pos = float4(input.pos.xyz, 1);
	output.col = input.col;
	output.uv = input.uv;
	
	return output;
}

[maxvertexcount(3)]
void TestGS(triangle PS_IN input[3], inout TriangleStream<PS_IN> triangles)
{
	PS_IN output;

	for (uint i = 0; i < 3; i++)
	{
		output.pos = input[i].pos;
		output.col = float4(1, 1, 1, 1);
		output.uv = input[i].uv;
		triangles.Append(output);
	}

	triangles.RestartStrip();
}

float4 TestPS(PS_IN input) : SV_Target
{
	return input.col;
}
