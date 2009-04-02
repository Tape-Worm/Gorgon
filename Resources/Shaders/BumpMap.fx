float4x4 _projectionMatrix;

float3 Position;
float4 Color = float4(1.0f, 1.0f, 1.0f, 1.0f);
float Intensity = 1.0f;
float SpecularIntensity = 64.0f;
float4 Specular = float4(0.0f, 0.0f, 0.0f, 1.0f);
float4 Ambient = float4(0.25f, 0.25f, 0.25f, 1.0f);
float4 GlobalAmbient = float4(0.25f, 0.25f, 0.25f, 1.0f);
bool UseAdditiveLighting = false;

float2 TextureSize;

texture ColorMap;
texture NormalMap;
texture SpecMap;

sampler2D colorSampler = sampler_state 
{ 
	texture = <ColorMap>;
};

sampler2D normalSampler = sampler_state
{
	texture = <NormalMap>;
};

sampler2D specSampler = sampler_state
{
	texture = <SpecMap>;
};

struct VTX
{
	float4 position : POSITION;
	float4 diffuse : COLOR0;
	float2 uv : TEXCOORD0;
};

float4 psBump(float2 coloruv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float4 tex = tex2D(colorSampler, coloruv);
	float3 normal = tex2D(normalSampler, coloruv) * 2.0f - 1.0f;	
	float3 pixelPosition = float3(TextureSize.x * coloruv.x, TextureSize.y * coloruv.y, 0);                                  
    float3 lightDir = (Position - pixelPosition);    
	float lightAmount = max(dot(normal, normalize(lightDir)), 0) * Intensity;
	float spec = pow(saturate(lightAmount), SpecularIntensity);
	
	float4 result;
	if (!UseAdditiveLighting)
		result = tex * color * lightAmount * Color;
	else
		result = tex + (color * lightAmount * Color);
		
	result += 2 * (tex2D(specSampler, coloruv) * Specular) * (spec * result);
	result += (Ambient * GlobalAmbient);
	result.a = tex.a;		
	return result;
}

VTX vsBump(VTX inVtx)
{
	VTX outVtx;
	
	outVtx.position = float4(mul(inVtx.position, _projectionMatrix).xyz, 1.0f);
	outVtx.uv = inVtx.uv;
	outVtx.diffuse = inVtx.diffuse;
	
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

