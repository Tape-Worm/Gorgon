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

struct HS_CON
{
	float edges[4] : SV_TessFactor;
	float inner[2] : SV_InsideTessFactor;
};

struct HS_POS
{
	float3 position : POSITION;
	float4 color : COLOR;
	float2 uv : TEXCOORD;
};

cbuffer info : register(c0)
{
	float2 size;
};

cbuffer anim : register(c1)
{
	float3 animValue = float3(1, 1, 1);
};

PS_IN TestVS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = float4(input.pos.xyz, 1);
	output.col = input.col;
	output.uv = input.uv;
	
	return output;
}

VS_IN TestVSTess( VS_IN input )
{
	VS_IN output = (VS_IN)0;
	
	output.pos = float4(input.pos.xyz, 1);
	output.col = input.col;
	output.uv = input.uv;
	
	return output;
}

HS_CON GetConstant(InputPatch<VS_IN, 4> patch, uint patchID : SV_PrimitiveID)
{
	HS_CON result;

	result.edges[0] = size.y;
	result.edges[1] = size.y;
	result.edges[2] = size.y;
	result.edges[3] = size.y;

	result.inner[0] = size.y;
	result.inner[1] = size.y;

	return result;
}

[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(4)]
[patchconstantfunc("GetConstant")]
HS_POS TestHS(InputPatch<VS_IN, 4> patch, uint controlID : SV_OutputControlPointID, uint patchID : SV_PrimitiveID)
{
	HS_POS result;

    result.position = patch[controlID].pos.xyz;
	result.color = patch[controlID].col;
	result.uv = patch[controlID].uv;

    return result;
}

[domain("quad")]
PS_IN TestDS(HS_CON hsResult, float2 uv : SV_DomainLocation, const OutputPatch<HS_POS, 4> patch)
{
	PS_IN result;

	float3 upperMid = lerp(patch[0].position, patch[3].position, (1.0f - uv.x * size.x));
	float3 lowerMid = lerp(patch[2].position, patch[1].position, uv.x * (size.y / 64.0f));

	float2 uvUpperMid = lerp(patch[0].uv, patch[3].uv, uv.x);
	float2 uvLowerMid = lerp(patch[2].uv, patch[1].uv, uv.x);

	result.pos = float4(lerp(upperMid, lowerMid, uv.y), 1);
	result.col = float4(uv.yx, 1-uv.x, 1);
	result.uv = float2(lerp(uvUpperMid, uvLowerMid, uv.y));

	return result;
}

[numthreads(10, 10, 1)]
void TestCS(uint3 threads : SV_DispatchThreadID)
{
	float r = animValue.x - (threads.x / size.x);
	float g = animValue.y - (threads.y / size.y);
	float b = animValue.z - ((r * 2.0f) * (g * 2.0f));

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

		float3 center = input[i].pos.xyz;

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
