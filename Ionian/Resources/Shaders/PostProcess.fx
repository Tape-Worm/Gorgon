Texture sourceImage;

// Amount to blur.
float blurAmount : UNITSSCALE
<	
   	string units = "inches";
    string UIWidget = "slider";
    float uimin = 0.00001;
    float uimax = 0.11125;
    float uistep = 0.00001;
    string UIName = "Blur";
> = 0.00025;

// Amount to sharpen.
float sharpenAmount : UNITSSCALE
<
	string units = "inches";
	string UIWidget = "slider";
	float uimin = 0.0f;
	float uimax = 50.0f;
	float uistep = 0.01f;
	string UIName = "Sharpen Amount";
> = 10.0f;

// Sharpening offset.
float sharpenOffset : UNITSSCALE
<
	string units = "inches";
	string UIWidget = "slider";
	float uimin = 0.0f;
	float uimax = 10.0f;
	float uistep = 0.01f;
	string UIName = "Sharpen Offset";
> = 0.35f;

// Position of the wave.
float wavePosition : UNITSSCALE
<
	string units = "inches";
	string UIWidget = "slider";
	float uimin = 0.0f;
	float uimax = 256.0f;
	float uistep = 0.01f;
	string UIName = "Wave Position";
> = 0.0f;

// Number of waves.
float waveCount : UNITSSCALE
<
	string units = "inches";
	string UIWidget = "slider";
	float uimin = 0.0f;
	float uimax = 25;
	float uistep = 0.05;
	string UIName = "Wave Count";
> = 4.0f;

// Amplitude of the waves.
float waveAmplitude : UNITSSCALE
<
	string units = "inches";
	string UIWidget = "slider";
	float uimin = 0.1f;
	float uimax = 256.0f;
	float uistep = 0.1;
	string UIName = "Wave Amplitude";
> = 64.0f;

// Flag to switch the waves to vertical (TRUE), or horizontal (FALSE).
bool waveVertical
<
	string UIName = "Vertical wave";
> = true;

// Flag to only apply the wave to one axis at a time.
bool waveRestrictOppositeDirection
<
	string UIName = "Restrict wave in the opposite direction";
> = true;

// Threshold for 2 converting two bit color.
float twoBitThreshold : UNITSSCALE
<
	string units = "inches";
	string UIWidget = "slider";
	float uimin = 0.1f;
	float uimax = 0.95f;
	float uistep = 0.05f;
	string UIName = "2-Bit color threshold";
> = 0.4f;

// Flag to invert the two bit color.
bool twoBitInvert
<
	string UIName = "Inverse 2-bit color.";
> = false;


// Our texture sampler.
sampler2D sourceSampler = sampler_state 
{ 
	texture = <sourceImage>;
};

// Our processed vertex.
struct VTX_OUTPUT
{
	float4 position : POSITION;
	float4 diffuse : COLOR0;
	float2 texCoords : TEXCOORD0;
};

// Function to convert a pixel to its grayscale equivalent.
float4 psConvertToGray(VTX_OUTPUT vtx) : COLOR0
{
    float4 Color;		// Output color.

    Color = tex2D( sourceSampler, vtx.texCoords.xy) * vtx.diffuse;
    // Average.  This is a cheap way to do grayscale, and is not entirely accurate.
    Color.rgb = (Color.r + Color.g + Color.b) / 3.0f;
    return Color;
}

// Function to convert a pixel to a two bit color.
float4 psConvertToTwoBit(VTX_OUTPUT vtx) : COLOR0
{
	float4 Color;
	
	// Get the average color
	Color = tex2D( sourceSampler, vtx.texCoords.xy);
	Color.rgb *= vtx.diffuse;
	Color.rgb = (Color.r + Color.g + Color.b) / 3.0f;	

	// If it falls within the range, convert it to black, or the color specified by the vertex diffuse.
	if (!twoBitInvert)
	{
		if (Color.r<=twoBitThreshold) Color.r = 0.0f; else Color.r = vtx.diffuse.r;
		if (Color.g<=twoBitThreshold) Color.g = 0.0f; else Color.g = vtx.diffuse.g;
		if (Color.b<=twoBitThreshold) Color.b = 0.0f; else Color.b = vtx.diffuse.b;
	} else
	{
		if (Color.r>=twoBitThreshold) Color.r = 0.0f; else Color.r = vtx.diffuse.r;
		if (Color.g>=twoBitThreshold) Color.g = 0.0f; else Color.g = vtx.diffuse.g;
		if (Color.b>=twoBitThreshold) Color.b = 0.0f; else Color.b = vtx.diffuse.b;
	}
	
	Color.a *= vtx.diffuse.a;
	return Color;
}

// Function to sharpen a pixel.
float4 psSharpen(VTX_OUTPUT vtx) : COLOR0
{
	float4 Color;			// Output.
	
	Color = tex2D( sourceSampler, vtx.texCoords.xy);
	Color -= tex2D( sourceSampler, vtx.texCoords.xy+(sharpenOffset/1000))*sharpenAmount;
	Color += tex2D( sourceSampler, vtx.texCoords.xy-(sharpenOffset/1000))*sharpenAmount;
	Color *= vtx.diffuse;	
		
	return saturate(Color);
}

// Function to perform the sampling for the blur.
float4 psBlurSample(float2 Tex : TEXCOORD0, float4 baseColor, float offX, float offY) : COLOR0
{
	float4 Color;				// Output.
  	float scaler = 0;			// Scale of the sample.
  	  
  	// Calculate sample.
  	scaler = (1 + (offY * offX));
	Tex.x = Tex.x + offX;
	Tex.y = Tex.y + offY;
	
   	Color = baseColor + tex2D(sourceSampler, Tex / scaler);   	
   	return Color;
}

// Function to blur an image.
float4 psBlur(VTX_OUTPUT vtx) : COLOR0
{
  	float4 Color = 0;		// Output.
  	float Alpha = 0;		// Alpha component.

	Color = tex2D(sourceSampler, vtx.texCoords);	
	// Store the alpha for later, we don't want to blur that.
	Alpha = Color.a;
	
	// Sample eight directions + the center.
  	Color = psBlurSample(vtx.texCoords, Color, -blurAmount, -blurAmount);
  	Color = psBlurSample(vtx.texCoords, Color, 0, -blurAmount);
  	Color = psBlurSample(vtx.texCoords, Color, blurAmount, -blurAmount);  	
  	Color = psBlurSample(vtx.texCoords, Color, -blurAmount, blurAmount);
  	Color = psBlurSample(vtx.texCoords, Color, 0, blurAmount);
  	Color = psBlurSample(vtx.texCoords, Color, blurAmount, blurAmount);  	
  	Color = psBlurSample(vtx.texCoords, Color, -blurAmount, 0);
  	Color = psBlurSample(vtx.texCoords, Color, blurAmount, 0);
  	
  	// Calculate final color.
   	Color.rgb = saturate((Color.rgb / 9) * vtx.diffuse.rgb);
   	// Restore and combine the alpha.
   	Color.a = Alpha * vtx.diffuse.a;
   	
    return Color;
}

// Function to make an image appear 'wavey'.
float4 psWave(VTX_OUTPUT vtx) : COLOR0
{
    float4 Color;		// Output.
    
    // Update wave positioning.
    if (waveVertical)
    {   	
   		vtx.texCoords.x = vtx.texCoords.x + (cos(vtx.texCoords.y * waveCount)) / waveAmplitude;
    	if (!waveRestrictOppositeDirection)
    		vtx.texCoords.y = vtx.texCoords.y + (sin(vtx.texCoords.x * waveCount)) / waveAmplitude;
    	else
    		vtx.texCoords.y = vtx.texCoords.y + (sin(vtx.texCoords.y * waveCount)) / waveAmplitude;
    	vtx.texCoords.y += wavePosition;
    } else    
    {            
    	if (!waveRestrictOppositeDirection)
    		vtx.texCoords.x = vtx.texCoords.x + (cos(vtx.texCoords.y * waveCount)) / waveAmplitude;
    	else
    		vtx.texCoords.x = vtx.texCoords.x + (cos(vtx.texCoords.x * waveCount)) / waveAmplitude;
    	vtx.texCoords.y = vtx.texCoords.y + (sin(vtx.texCoords.x * waveCount)) / waveAmplitude;
    	vtx.texCoords.x += wavePosition;
    }
    // Adjust image.
   	Color = tex2D( sourceSampler, vtx.texCoords.xy) * vtx.diffuse;   	
    return Color;
}
 
// Technique to make an image appear wavey.
technique Wave
{
    pass p1
    {
    	VertexShader = null;
        PixelShader = compile ps_2_0 psWave();
    }
}

// Techinique to sharpen an image.
technique Sharpen
{
    pass p1
    {
        VertexShader = null;
        PixelShader = compile ps_2_0 psSharpen();
    }

}

// Technique to convert an image to grayscale.
technique GrayScale
{
	pass p1
	{
		VertexShader = null;
		PixelShader = compile ps_2_0 psConvertToGray();
	}
}

// Technique to convert an image into a two bit color image.
technique TwoBitColor
{
	pass p1
	{
		VertexShader = null;
		PixelShader = compile ps_2_0 psConvertToTwoBit();
	}
}

// Technique to blur an image.
technique Blur
{	
	pass p1
	{
		VertexShader = null;
		PixelShader = compile ps_2_0 psBlur();
	}
}