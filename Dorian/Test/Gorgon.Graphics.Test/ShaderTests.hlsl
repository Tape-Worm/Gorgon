Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);
RWTexture2D<float4> _csTexture : register(t0);

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

[numthreads(8, 8, 16)]
void TestCS(uint3 threads : SV_DispatchThreadID)
{
	float r = (threads.x << threads.z) / 255.0f;
	float g = (threads.y << threads.z) / 255.0f;
	float b = ((threads.z * 1) / 255.0f);

	_csTexture[threads.xy] = float4(r, g, b, 1);
}

[maxvertexcount(12)]
void TestGS(triangle PS_IN input[3], inout TriangleStream<PS_IN> triangles, uint primID : SV_PrimitiveID)
{
	PS_IN output;

	for (uint i = 0; i < 3; i++)
	{
		if ((i != 0) && (primID == 1))
		{
			continue;
		}

		float3 center = input[i].pos;

		float4 corner1 = float4(center.x - ((i + 1) * 0.083333f), center.y - ((i + 1) * 0.083333f), 0, 1.0f);
		float4 corner2 = float4(center.x - ((i + 1) * 0.083333f), center.y + ((i + 1) * 0.083333f), 0, 1.0f);
		float4 corner3 = float4(center.x + ((i + 1) * 0.083333f), center.y - ((i + 1) * 0.083333f), 0, 1.0f);
		float4 corner4 = float4(center.x + ((i + 1) * 0.083333f), center.y + ((i + 1) * 0.083333f), 0, 1.0f);

		output.pos = corner1;
		output.uv = float2(0, 1);
		output.col = input[i].col;
		triangles.Append(output);

		output.pos = corner2;
		output.uv = float2(0, 0);
		triangles.Append(output);

		output.pos = corner3;
		output.uv = float2(1.0f, 1.0f);
		triangles.Append(output);

		output.pos = corner4;
		output.uv = float2(1.0f, 0);
		triangles.Append(output);

		triangles.RestartStrip();
	}	
}

float4 TestPS(PS_IN input) : SV_Target
{
	return _texture.Sample(_sampler, input.uv) * input.col;
}
