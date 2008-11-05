float4x4 _projectionMatrix;
float4x4 worldMatrix;

float3 Position;
float4 Color = float4(1.0f, 1.0f, 1.0f, 1.0f);
float Intensity = 1.0f;
float4 Ambient = float4(0.25f, 0.25f, 0.25f, 1.0f);

texture ColorMap;
texture NormalMap;

sampler2D colorSampler = sampler_state 
{ 
	texture = <ColorMap>;
};

sampler2D normalSampler = sampler_state
{
	texture = <NormalMap>;
};

struct IN_VTX
{
	float4 position : POSITION;
	float4 diffuse : COLOR0;
	float2 uv : TEXCOORD0;
};

struct OUT_VTX
{
	float4 position : POSITION;
	float4 diffuse : COLOR0;
	float2 coloruv : TEXCOORD0;
	float3 normaluv : TEXCOORD1;
};

float4 psBump(float2 coloruv : TEXCOORD0, float3 normaluv : TEXCOORD1, float4 color : COLOR0) : COLOR0
{
	float3 light = (normaluv - 0.5f) * 2.0f;		
	float3 normalTexel = tex2D(normalSampler, coloruv).xyz;
	float4 imageTexel = tex2D(colorSampler, coloruv);
	float3 normal = (normalTexel - 0.5f) * 2.0f;
	
	return float4(((dot(-normal, light) * Intensity * color + Ambient) * imageTexel).rgb, imageTexel.a);
}

OUT_VTX vsBump(IN_VTX inVtx)
{
	OUT_VTX outVtx;
	
	outVtx.position = float4(mul(inVtx.position, _projectionMatrix).xyz, 1.0f);
	outVtx.coloruv = inVtx.uv;
	outVtx.diffuse = inVtx.diffuse * Color;
	// If we want to inherit the transforms of the sprite, we need to send in a world matrix
	// and multiply by that.  It's not giving 100% perfect per-pixel lighting, but it'll do.
	outVtx.normaluv = normalize(mul(inVtx.position.xyz - Position, worldMatrix).xyz);
	
	return outVtx;
}

technique Bump
{
	pass P1
	{	
		MagFilter[0] = Point;
		MinFilter[0] = Point;
		MagFilter[1] = Point;
		MinFilter[1] = Point;
		VertexShader = compile vs_2_0 vsBump();
		PixelShader = compile ps_2_0 psBump();
	}
}

technique BumpSmooth
{
	pass P1
	{	
		MagFilter[0] = Linear;
		MinFilter[0] = Linear;
		MagFilter[1] = Linear;
		MinFilter[1] = Linear;
		VertexShader = compile vs_2_0 vsBump();
		PixelShader = compile ps_2_0 psBump();
	}
}

