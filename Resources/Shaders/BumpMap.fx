float4x4 _projectionMatrix;
float4x4 invWorldMatrix;

#define MAX_LIGHTS 4

float3 LightPosition[MAX_LIGHTS];
float4 LightColor[MAX_LIGHTS];// = {float4(1.0f, 1.0f, 1.0f, 1.0f), float4(1.0f, 1.0f, 1.0f, 1.0f), float4(1.0f, 1.0f, 1.0f, 1.0f), float4(1.0f, 1.0f, 1.0f, 1.0f)};
float4 LightAmbientColor[MAX_LIGHTS];// = {float4(0.0f, 0.0f, 0.0f, 1.0f), float4(0.0f, 0.0f, 0.0f, 1.0f), float4(0.0f, 0.0f, 0.0f, 1.0f), float4(0.0f, 0.0f, 0.0f, 1.0f)};
float4 LightSpecularColor[MAX_LIGHTS];// = {float4(1.0f, 1.0f, 1.0f, 1.0f), float4(1.0f, 1.0f, 1.0f, 1.0f), float4(1.0f, 1.0f, 1.0f, 1.0f), float4(1.0f, 1.0f, 1.0f, 1.0f)};
float LightIntensity[MAX_LIGHTS];// = {1.0f, 1.0f, 1.0f, 1.0f};
float LightSpecular[MAX_LIGHTS];// = {65536.0f, 65536.0f, 65536.0f, 65536.0f};
float4 GlobalAmbient = float4(0.25f, 0.25f, 0.25f, 1.0f);
bool LightEnabled[MAX_LIGHTS];
bool LightSpecularEnabled[MAX_LIGHTS];

float2 TextureSize;

texture ColorMap;
texture NormalMap;
texture SpecularMap;

sampler2D colorSampler = sampler_state 
{ 
	texture = <ColorMap>;
};

sampler2D normalSampler = sampler_state
{
	texture = <NormalMap>;
};

sampler2D specularSampler = sampler_state
{
	texture = <SpecularMap>;
};

struct VTX
{
	float4 position : POSITION;
	float4 diffuse : COLOR0;
	float2 uv : TEXCOORD0;
};

float4 psBump20(float2 coloruv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float4 texDiffuse = tex2D(colorSampler, coloruv);
	float3 normal = tex2D(normalSampler, coloruv) * 2.0f - 1.0f;	
	float3 pixelPosition = float3(TextureSize.x * coloruv.x, TextureSize.y * coloruv.y, 0);
	float4 result = float4(0, 0, 0 , 1.0f);
    	float3 lightDir;
	float lightAmount;
	
	normal = mul(normal, invWorldMatrix).xyz;
	
	for (int i = 0; i < MAX_LIGHTS; i++)
	{
		if (LightEnabled[i])
		{
			lightDir = LightPosition[i] - pixelPosition;
			lightAmount = max(dot(normal, normalize(lightDir)), 0) * LightIntensity[i];
			
			float4 diffuse = (texDiffuse * color * lightAmount * LightColor[i]);
			result += diffuse;
			result += (LightAmbientColor[i] * GlobalAmbient);		
		}
	}
	
	result.a = texDiffuse.a * color.a;		
	return result;
}

float4 psBump(float2 coloruv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float4 texDiffuse = tex2D(colorSampler, coloruv);
	float3 normal = tex2D(normalSampler, coloruv) * 2.0f - 1.0f;	
	float3 pixelPosition = float3(TextureSize.x * coloruv.x, TextureSize.y * coloruv.y, 0);
	float4 result = float4(0, 0, 0 , 1.0f);
    float3 lightDir;
	float lightAmount;
	float specularAmount;
	
	normal = mul(normal, invWorldMatrix).xyz;
	
	for (int i = 0; i < MAX_LIGHTS; i++)
	{
		if (LightEnabled[i])
		{
			lightDir = LightPosition[i] - pixelPosition;
			lightAmount = max(dot(normal, normalize(lightDir)), 0) * LightIntensity[i];
			specularAmount = pow(saturate(lightAmount), LightSpecular[i]);
			
			float4 diffuse = (texDiffuse * color * lightAmount * LightColor[i]);
			result += diffuse;
			if (LightSpecularEnabled[i])
				result += 2.0f * (tex2D(specularSampler, coloruv) * LightSpecularColor[i]) * (specularAmount * diffuse);
			result += (LightAmbientColor[i] * GlobalAmbient);		
		}
	}
	
	result.a = texDiffuse.a * color.a;		
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
		VertexShader = compile vs_3_0 vsBump();
		PixelShader = compile ps_3_0 psBump();
	}
}

technique Bump20
{
	pass P1	
	{
		VertexShader = compile vs_2_0 vsBump();
		PixelShader = compile ps_2_0 psBump20();
	}
}