texture sourceImage;
texture blur1;
texture blur2;

// Amount to blur.
float blurAmount = 1.0f / 512.0f;

// Our texture samplers
sampler2D sourceSampler
{
	Texture=<sourceImage>;
};

sampler2D hBlurSampler
{
	Texture=<blur1>;
};

sampler2D vBlurSampler
{
	Texture=<blur2>;
};


// Kernel.
float kernel[13] = 
{
	-6,
	-5,
	-4,
	-3,
	-2,
	-1,
	0,
	1,
	2,
	3,
	4,
	5,
	6
};

float weights[13] =
{
    0.002216,
    0.008764,
    0.026995,
    0.064759,
    0.120985,
    0.176033,
    0.199471,
    0.176033,
    0.120985,
    0.064759,
    0.026995,
    0.008764,
    0.002216,
};

// Our processed vertex.
struct VTX_OUTPUT
{
	float4 diffuse : COLOR0;
	float2 texCoords : TEXCOORD0;
};

// Function to draw the 'shadow' of the image.
float4 psDrawShadow(VTX_OUTPUT vtx) : COLOR0
{
	float4 sample = tex2D(sourceSampler, vtx.texCoords);
	
	if (sample.a == 0)
	   return sample;
	else
	   return float4(1.0, 1.0, 1.0, sample.a);
}

// Function to blur an image horizontally.
float4 psHBlur(VTX_OUTPUT vtx) : COLOR0
{
	float4 sample = float4(0, 0, 0, 0);
	
	for (int i = 0; i < 13; i++)
	{
	   sample += tex2D(vBlurSampler, float2(vtx.texCoords.x + kernel[i] / blurAmount, vtx.texCoords.y)) * weights[i];
	}
	
//	sample /= 13;	
	
	return sample;
}

// Function to blur an image vertically.
float4 psVBlur(VTX_OUTPUT vtx) : COLOR0
{
	float4 sample = float4(0, 0, 0, 0);
	
	for (int i = 0; i < 13; i++)
	{
	   sample += tex2D(hBlurSampler, float2(vtx.texCoords.x, vtx.texCoords.y + kernel[i] / blurAmount)) * weights[i];
	}
	
//	sample /= 13;
	
	return sample;
}

// Technique to blur an image.
technique Blur
{
	pass DrawShadow	
	{
		VertexShader = null;
		PixelShader = compile ps_2_0 psDrawShadow();
	}
	
	pass hBlur
	{
		VertexShader = null;
		PixelShader = compile ps_2_0 psHBlur();
	}

	pass vBlur
	{
		VertexShader = null;
		PixelShader = compile ps_2_0 psVBlur();
	}
}