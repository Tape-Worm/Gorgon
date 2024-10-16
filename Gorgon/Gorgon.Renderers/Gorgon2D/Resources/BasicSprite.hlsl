#define REJECT_ALPHA(alpha) if (alphaTestEnabled) clip((alpha <= alphaTestValueHi && alpha >= alphaTestValueLow) ? -1 : 1);
#define RANGE_BW(colorValue) (colorValue < oneBitRange.x || colorValue > oneBitRange.y) ? 0.0f : 1.0f;

// Our default texture and sampler.
Texture2DArray _gorgonTexture : register(t0);
SamplerState _gorgonSampler : register(s0);

// Additional effect texture buffer.
Texture2D _gorgonEffectTexture : register(t1);

// Our default sprite vertex.
struct GorgonSpriteVertex
{
   float4 position : SV_POSITION;
   float4 color : COLOR;
   float4 uv : TEXCOORD;
   float2 angle : ANGLE;
};

// The output for the polygon sprite from the vertex to the pixel shader.
struct GorgonPolySpriteVertex
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float4 textureTransform : TEXTRANSFORM;
    float3 uv : TEXCOORD;
    float2 angle : ANGLE;
};

// The transformation matrices (for vertex shader).
cbuffer GorgonViewProjection : register(b0)
{
    float4x4 ViewProjection;
}

// The world matrix, and other information for polygon sprites.
cbuffer GorgonPolyData : register(b1)
{
    float4x4 World = float4x4(1, 0, 0, 0, 
                              0, 1, 0, 0, 
                              0, 0, 1, 0, 
                              0, 0, 0, 1);
    float4 PolyColor;
    float4 PolyTextureTransform;
    float4 PolyMiscData;
    float PolyTextureArrayIndex;
}

// Alpha test value (for pixel shader).
cbuffer GorgonAlphaTest : register(b0)
{
    bool alphaTestEnabled = false;
    float alphaTestValueLow = 0.0f;
    float alphaTestValueHi = 0.0f;
}

// Timing values to pass to the shaders.
cbuffer GorgonTimingValues : register(b12)
{
    // The number of seconds between frames.
    float FrameDelta;
    // The number of seconds since the application was started.
    float SecondsSinceStart;
    // The number of frames per second.
    float Fps;
    // The number of frames since the application was started.
    uint FrameCount;
}

// Miscellaneous values to pass to the shaders.
cbuffer GorgonMiscValues : register(b13)
{
    // The current position of the currently active viewport.
    float2 ViewportOffset;
    // The current size (width, height) of the currently active viewport.
    float2 ViewportSize;
    // The depth near/far values for the currently active viewport.
    float DepthNear;
    float DepthFar;
    // The width and height of the current render target.
    float TargetWidth;
    float TargetHeight;	
    // The width and height of the current texture, in slot 0.
    float2 Texture0Size;
}

// Converts an sRGB color value to linear.
float3 SRgbToLinear(float3 c)
{
    half3 linearRGBLo = c / 12.92;
    half3 linearRGBHi = pow((c + 0.055) / 1.055, half3(2.4, 2.4, 2.4));
    half3 linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
}

// Converts a linear color value to sRGB.
float3 LinearToSRgb(float3 c)
{
    half3 sRGBLo = c * 12.92;
    half3 sRGBHi = (pow(c, half3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
    half3 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    return sRGB;
}

// Function to sample the main texture.
float4 SampleMainTexture(float4 uv, float4 vertexColor)
{
    return _gorgonTexture.Sample(_gorgonSampler, float3(uv.xy / uv.w, uv.z)) * vertexColor;
}

// Function to perform a simple 4-tap box sample.
float4 BoxSample4Tap(Texture2DArray t, SamplerState s, float2 texelSize, float2 uv, float spread)
{
    float4 delta = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0) * spread;

    float4 output;
    output =  t.Sample(s, float3(uv + delta.xy, 0));    
    output += t.Sample(s, float3(uv + delta.zy, 0));
    output += t.Sample(s, float3(uv + delta.xw, 0));
    output += t.Sample(s, float3(uv + delta.zw, 0));

    return output / 4.0f;
}

// Function to perform a 9-tap tent sample.
float4 TentSample9Tap(Texture2DArray t, SamplerState s, float2 texelSize, float2 uv, float spread)
{
	float4 delta = texelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * spread;

    float4 output;
    output =  t.Sample(s, float3(uv - delta.xy, 0));
    output += t.Sample(s, float3(uv - delta.wy, 0)) * 2.0;
    output += t.Sample(s, float3(uv - delta.zy, 0));

    output += t.Sample(s, float3(uv + delta.zw, 0)) * 2.0;
    output += t.Sample(s, float3(uv, 0)) * 4.0;
    output += t.Sample(s, float3(uv + delta.xw, 0)) * 2.0;

    output += t.Sample(s, float3(uv + delta.zy, 0));
    output += t.Sample(s, float3(uv + delta.wy, 0)) * 2.0;
    output += t.Sample(s, float3(uv + delta.xy, 0));

    return output / 16.0f;
}

// Function to perform a 13-tap tent sample.
float4 TentSample13Tap(Texture2DArray t, SamplerState s, float2 texelSize, float2 uv)
{
	float4 A = t.Sample(s, float3(uv + texelSize * float2(-1.0, -1.0), 0));
    float4 B = t.Sample(s, float3(uv + texelSize * float2( 0.0, -1.0), 0));
    float4 C = t.Sample(s, float3(uv + texelSize * float2( 1.0, -1.0), 0));
    float4 D = t.Sample(s, float3(uv + texelSize * float2(-0.5, -0.5), 0));
    float4 E = t.Sample(s, float3(uv + texelSize * float2( 0.5, -0.5), 0));
    float4 F = t.Sample(s, float3(uv + texelSize * float2(-1.0,  0.0), 0));
    float4 G = t.Sample(s, float3(uv, 0));
    float4 H = t.Sample(s, float3(uv + texelSize * float2( 1.0,  0.0), 0));
    float4 I = t.Sample(s, float3(uv + texelSize * float2(-0.5,  0.5), 0));
    float4 J = t.Sample(s, float3(uv + texelSize * float2( 0.5,  0.5), 0));
    float4 K = t.Sample(s, float3(uv + texelSize * float2(-1.0,  1.0), 0));
    float4 L = t.Sample(s, float3(uv + texelSize * float2( 0.0,  1.0), 0));
    float4 M = t.Sample(s, float3(uv + texelSize * float2( 1.0,  1.0), 0));

    float2 scale = float2(0.125, 0.03125);

	// Average the output so we minimize the "firefly" effect (although in the right context, it looks pretty cool).
    float4 output = (D + E + I + J) * scale.x;
    output += (A + B + G + F) * scale.y;
    output += (B + C + H + G) * scale.y;
    output += (F + G + L + K) * scale.y;
    output += (G + H + M + L) * scale.y;

    return output;
}

// Creates a 4x4 matrix from the 4, 4 component floating point values (columns).
float4x4 CreateFrom4x4FromFloat4(float4 c0, float4 c1, float4 c2, float4 c3)
{
    return float4x4(c0.x, c1.x, c2.x, c3.x,
                    c0.y, c1.y, c2.y, c3.y,
                    c0.z, c1.z, c2.z, c3.z,
                    c0.w, c1.w, c2.w, c3.w);
}

// Our default vertex shader.
GorgonSpriteVertex GorgonVertexShader(GorgonSpriteVertex vertex)
{
    GorgonSpriteVertex output = vertex;

    output.position = mul(ViewProjection, output.position);

    return output;
}

// Our default vertex shader for polygon sprites.
GorgonPolySpriteVertex GorgonVertexShaderPoly(GorgonSpriteVertex vertex)
{
    GorgonPolySpriteVertex output;
    
    float4x4 final = mul(ViewProjection, World);
    output.position = mul(final, vertex.position);
    output.color = PolyColor * vertex.color;
    output.uv = float3(PolyMiscData.x == 1.0f ? 1.0f - vertex.uv.x : vertex.uv.x, PolyMiscData.y == 1.0f ? 1.0f - vertex.uv.y : vertex.uv.y, PolyTextureArrayIndex);
    output.textureTransform = PolyTextureTransform;
    output.angle = float2(PolyMiscData.z, PolyMiscData.w);

    return output;
}

// Our default pixel shader with textures with alpha testing.
float4 GorgonPixelShaderTextured(GorgonSpriteVertex vertex) : SV_Target
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);

    REJECT_ALPHA(color.a);
        
    return color;
}

// Our default pixel shader for poly sprites with textures with alpha testing.
float4 GorgonPixelShaderPoly(GorgonPolySpriteVertex vertex) : SV_Target
{
    float2 texCoords = (vertex.uv.xy * vertex.textureTransform.zw) + vertex.textureTransform.xy;
    float4 color = _gorgonTexture.Sample(_gorgonSampler, float3(texCoords, vertex.uv.z)) * vertex.color;

    REJECT_ALPHA(color.a);
        
    return color;
}

#ifdef INVERSE_EFFECT
// Invert effect variables.
cbuffer GorgonInvertEffect : register(b1)
{
    bool invertUseAlpha = false;
}

// A pixel shader to invert the color on a texture.
float4 GorgonPixelShaderInvert(GorgonSpriteVertex vertex) : SV_Target
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);

    if (invertUseAlpha)
    {
        color = 1.0f - color;
    }
    else
    {
        color = float4(1.0f - color.rgb, color.a);
    }
        
    REJECT_ALPHA(color.a);

    return color;
}
#endif

#ifdef BURN_DODGE_EFFECT
// Burn/dodge effect.
cbuffer GorgonBurnDodgeEffect : register(b1)
{
    bool burnDodgeUseDodge;
}

// Function to perform a linear image burn/dodge.
float4 GorgonPixelShaderLinearBurnDodge(GorgonSpriteVertex vertex) : SV_Target
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);

    REJECT_ALPHA(color.a);
    
    color.rgb = color.rgb * 2.0f;

    if (!burnDodgeUseDodge)
    {
        color.rgb = color.rgb - 1.0f;
    }

    return color;
}

// Function to perform an image burn/dodge.
float4 GorgonPixelShaderBurnDodge(GorgonSpriteVertex vertex) : SV_Target
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);

    REJECT_ALPHA(color.a);

    if (burnDodgeUseDodge)
    {		
        float3 invColor = float3(color.r < 1.0f ? 1.0f - color.r : 1.0f, 
                                color.g < 1.0f ? 1.0f - color.g : 1.0f, 
                                color.b < 1.0f ? 1.0f - color.b : 1.0f);

        color.r = min(color.r / invColor.r, 1.0f);
        color.g = min(color.g / invColor.g, 1.0f);
        color.b = min(color.b / invColor.b, 1.0f);		
    }
    else
    {

        color.r = color.r == 0 ? 0 : max((1.0f - ((1.0f - color.r) / color.r)), 0); 
        color.g = color.g == 0 ? 0 : max((1.0f - ((1.0f - color.g) / color.g)), 0); 
        color.b = color.b == 0 ? 0 : max((1.0f - ((1.0f - color.b) / color.b)), 0); 
    }

    return saturate(color);
}
#endif

#ifdef GRAYSCALE_EFFECT
// A pixel shader that converts to gray scale.
float4 GorgonPixelShaderGrayScale(GorgonSpriteVertex vertex) : SV_Target
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);

    REJECT_ALPHA(color.a);

    float grayValue = color.r * 0.3f + color.g * 0.59f + color.b * 0.11f;

    return float4(grayValue, grayValue, grayValue, color.a);
}
#endif

#ifdef ONEBIT_EFFECT
// 1 bit color effect.
cbuffer Gorgon1BitEffect : register(b1)
{	
    bool oneBitUseAverage;
    bool oneBitInvert;
    bool oneBitUseAlpha;
    float2 oneBitRange;
}

// A pixel shader to show the texture data as 1 bit data.
float4 GorgonPixelShader1Bit(GorgonSpriteVertex vertex) : SV_Target
{
    float4 color = 0;

    if (oneBitUseAverage)
    {
        color = SampleMainTexture(vertex.uv, vertex.color);
        color.rgb = (color.r + color.g + color.b) / 3.0f;
    }
    else
    {
        color = GorgonPixelShaderGrayScale(vertex);
    }

    color.r = RANGE_BW(color.r);
    color.g = RANGE_BW(color.g);
    color.b = RANGE_BW(color.b);

    if (oneBitUseAlpha)
    {
        color.a = RANGE_BW(color.a);
    }

    if (oneBitInvert)
    {
        if (oneBitUseAlpha)
            color = 1.0f - color;
        else
            color.rgb = 1.0f - color.rgb;
    }

    REJECT_ALPHA(color.a);

    return color;
}
#endif

#ifdef WAVE_EFFECT
// Wave effect variables.
cbuffer GorgonWaveEffect : register(b1)
{
    float waveAmplitude;
    float waveLength;
    float wavePeriod;
    float waveLengthScale;
    int waveType;
}

// A vertical wave effect pixel shader.
float4 GorgonPixelShaderWaveEffect(GorgonSpriteVertex vertex) : SV_Target
{
    float2 uv = vertex.uv;
    float4 color;
    float length = abs(1.0f - (waveLength / waveLengthScale)) * waveLength;
    float amp = waveAmplitude / 360.0f;
        
    if ((waveType == 0) || (waveType == 2))
    {
        uv.x += sin((uv.y + wavePeriod) * length) * amp;
    }

    if ((waveType == 1) || (waveType == 2))
    {
        uv.y += cos((uv.x + wavePeriod) * length) * amp;
    }
            
    color = SampleMainTexture(float4(uv, vertex.uv.z, vertex.uv.w), vertex.color);	
    REJECT_ALPHA(color.a);
    return color;
}
#endif

#ifdef DISPLACEMENT_EFFECT
// Displacement effect variables.
cbuffer GorgonDisplacementEffect : register(b1)
{
    // Settings for the displacement effect.
    // X = texel size of horizontal pixel position.
    // Y = texel size of vertical pixel position.
    // Z = Displacement strength.
    // W = 0 - Chromatic aberration off, 1 - Chromatic aberration on.
    float4 displacementSettings;
    // The scale to apply to the separation of the color channels when chromatic aberration is on.
    // If chromatic aberration is off, then this will do nothing.
    float2 chromAbScale;
}

// The displacement shader encoder.
float2 GorgonPixelShaderDisplacementEncoder(float4 displaceValue)
{
    REJECT_ALPHA(displaceValue.a);

    float4 basisX = displaceValue.x >= 0.5f ? float4(displaceValue.x, 0.0f, 0.0f, 0) : float4(0.0f, 0.0f, -displaceValue.x, 0.0f);
    float4 basisY = displaceValue.y >= 0.5f ? float4(0.0f, displaceValue.y, 0.0f, 0) : float4(0.0f, 0.0f, 0.0f, -displaceValue.y);	
    float4 displacement = (basisX + basisY) * displacementSettings.z;

    return ((displacement.xy + displacement.zw) * displacementSettings.xy) * displaceValue.a;
}

// The displacement shader decoder.
float4 GorgonPixelShaderDisplacementDecoder(GorgonSpriteVertex vertex) : SV_Target
{	
    // The texel used to displace the underlying texels.
    float4 displaceTexel = SampleMainTexture(vertex.uv, vertex.color);
    // Project our UV coordinates into the coordinate space of the background image.
    float2 backUV = vertex.position.xy * displacementSettings.xy;
    // Calculate the offset.
    float2 offset = backUV + GorgonPixelShaderDisplacementEncoder(displaceTexel);
    
    // Get the background color to offset.
    float4 color = _gorgonEffectTexture.Sample(_gorgonSampler, offset);
    
    REJECT_ALPHA(color.a);

    float r = color.r;
    float b = color.b;
        
    if (displacementSettings.w != 0)
    {
        float2 adjustment = float2(displacementSettings.x * displacementSettings.z * chromAbScale.x, 
                                   displacementSettings.y * displacementSettings.z * chromAbScale.y);
        
        r = _gorgonEffectTexture.Sample(_gorgonSampler, float2(offset.x - adjustment.x, offset.y + adjustment.y)).r;
        b = _gorgonEffectTexture.Sample(_gorgonSampler, float2(offset.x + adjustment.x, offset.y - adjustment.y)).b;
    }

    return float4(r, color.g, b, color.a);
}
#endif

#ifdef GAUSS_BLUR_EFFECT
// Gaussian blur effect starts here.

// Adjust the weighting and offsets so we can pack our float values in tightly (much less bandwidth when updating the CB).
#define MAX_WEIGHT_SIZE (((MAX_KERNEL_SIZE * 4) + 15) & (~15)) / 16
#define MAX_OFFSET_SIZE (((MAX_KERNEL_SIZE * 8) + 15) & (~15)) / 16

// Gaussian blur kernal (for the gaussian blur shader).
cbuffer GorgonGaussKernelData : register(b1)
{
    float4 _offsets[MAX_OFFSET_SIZE];
    float4 _weights[MAX_WEIGHT_SIZE];
    float _blurRadius;
}

// Gaussian blur pass data (for the gaussian blur shader).
cbuffer GorgonGaussPassSettings : register(b2)
{
    int _passIndex;
}

// Function to gather a single pass of the separable gaussian blur with alpha preservation.
float4 GorgonPixelShaderGaussBlurNoAlpha(GorgonSpriteVertex vertex) : SV_Target
{
    float4 blurSample = 0.0f;
    int kernelSize = (_blurRadius * 2) + 1;

    [unroll]
    for (int i = 0; i < kernelSize; i++)
    {
        int arrayIndex = i / 4;
        int componentIndex = i % 4;
        float2 offset = _passIndex == 0 ? float2(_offsets[arrayIndex][componentIndex], 0) : float2(0, _offsets[arrayIndex][componentIndex]);
        float4 texSample = SampleMainTexture(float4(vertex.uv.xy + offset, vertex.uv.z, vertex.uv.w), float4(1, 1, 1, 1));

        blurSample.rgb += texSample.rgb * _weights[arrayIndex][componentIndex];	
        blurSample.a = texSample.a;
    }

    return saturate(blurSample);
}

// Function to gather a single pass of the separable gaussian blur.
float4 GorgonPixelShaderGaussBlur(GorgonSpriteVertex vertex) : SV_Target
{
    float4 blurSample = 0.0f;
    int kernelSize = (_blurRadius * 2) + 1;
    float totalWeight = 0.0f;

    [unroll]
    for (int i = 0; i < kernelSize; i++)
    {
        int arrayIndex = i / 4;
        int componentIndex = i % 4;
        float2 offset = _passIndex == 0 ? float2(_offsets[arrayIndex][componentIndex], 0) : float2(0, _offsets[arrayIndex][componentIndex]);
        float4 texSample = SampleMainTexture(float4(vertex.uv.xy + offset, vertex.uv.z, vertex.uv.w), float4(1, 1, 1, 1));

        // Skip blurring if there's no alpha.  If there's no alpha, then there's nothing to contribute to the blur.
        // We can't use clip() here because it causes smearing when animating the image.		
        if (texSample.a == 0)
        {			
            continue;
        }

        float newAlpha = _weights[arrayIndex][componentIndex] * texSample.a;

        blurSample.rgb += texSample.rgb * newAlpha;
        blurSample.a += newAlpha;

        totalWeight += newAlpha;
    }

    // If there's no weighting, then do nothing with this texel.
    return totalWeight != 0 ? saturate(float4(blurSample.rgb / totalWeight, blurSample.a)) : float4(blurSample.rgb, 0);
}
#endif

#ifdef SHARPEN_EMBOSS_EFFECT
// Sharpen/emboss effect variables.
cbuffer GorgonSharpenEmbossEffect : register(b1)
{
    float2 sharpEmbossTexelDistance;
    float sharpEmbossAmount;
}


// A pixel shader to sharpen the color on a texture.
float4 GorgonPixelShaderSharpen(GorgonSpriteVertex vertex) : SV_Target
{
    float2 offset[8] =
    {
        float2(-sharpEmbossTexelDistance.x, -sharpEmbossTexelDistance.y),
        float2(0.0, -sharpEmbossTexelDistance.y),
        float2(-sharpEmbossTexelDistance.x, 0),
        float2(-sharpEmbossTexelDistance.x, 0.0),
        float2(sharpEmbossTexelDistance.x, 0.0),
        float2(sharpEmbossTexelDistance.x, sharpEmbossTexelDistance.y),
        float2(-sharpEmbossTexelDistance.x, sharpEmbossTexelDistance.y),
        float2(sharpEmbossTexelDistance.x, -sharpEmbossTexelDistance.y)
    };

    const float convolution = -0.5f;
    float mainConvolution = 1.0f + ((-convolution * 8.0f) * sharpEmbossAmount);

    float4 c = _gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy / vertex.uv.w), vertex.uv.z)) * vertex.color;
    float4 c0 = (_gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy + offset[0]) / vertex.uv.w, vertex.uv.z)) * vertex.color) * convolution;
    float4 c1 = (_gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy + offset[1]) / vertex.uv.w, vertex.uv.z)) * vertex.color) * convolution;
    float4 c2 = (_gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy + offset[2]) / vertex.uv.w, vertex.uv.z)) * vertex.color) * convolution;
    float4 c3 = (_gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy + offset[3]) / vertex.uv.w, vertex.uv.z)) * vertex.color) * convolution;
    float4 c4 = (_gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy + offset[4]) / vertex.uv.w, vertex.uv.z)) * vertex.color) * convolution;
    float4 c5 = (_gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy + offset[5]) / vertex.uv.w, vertex.uv.z)) * vertex.color) * convolution;
    float4 c6 = (_gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy + offset[6]) / vertex.uv.w, vertex.uv.z)) * vertex.color) * convolution;
    float4 c7 = (_gorgonTexture.Sample(_gorgonSampler, float3((vertex.uv.xy + offset[7]) / vertex.uv.w, vertex.uv.z)) * vertex.color) * convolution;
    float4 c8 = c * mainConvolution;

    return saturate(((c0 + c1 + c2 + c3 + c4 + c5 + c6 + c7) * sharpEmbossAmount) + c8);
}

// A pixel shader to sharpen the color on a texture.
float4 GorgonPixelShaderEmboss(GorgonSpriteVertex vertex) : SV_Target
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);
    float alpha = color.a;
    float amount = 3.5f * sharpEmbossAmount;
    float3 texelPosition;
            
    REJECT_ALPHA(alpha);
    
    texelPosition = float3((vertex.uv.xy + sharpEmbossTexelDistance) / vertex.uv.w, vertex.uv.z);
    color -= (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * amount);
    texelPosition = float3((vertex.uv.xy - sharpEmbossTexelDistance) / vertex.uv.w, vertex.uv.z);
    color += (_gorgonTexture.Sample(_gorgonSampler, texelPosition) * amount);

    color.a = alpha;
    color.rgb = (color.r + color.g + color.b) / 3.0f;
    
    return color;
}
#endif

#ifdef POSTERIZE_EFFECT
// Posterize effect variables.
cbuffer GorgonPosterizeEffect : register(b1)
{
    bool posterizeUseAlpha;
    float posterizeGamma;
    int posterizeColors;
}

// Function to posterize texture data.
float4 GorgonPixelShaderPosterize(GorgonSpriteVertex vertex) : SV_Target
{
    float4 color = SampleMainTexture(vertex.uv, vertex.color);
    float4 posterColor = color;	
    
    REJECT_ALPHA(color.a);

    posterColor = pow(posterColor, posterizeGamma);
    
    posterColor *= (posterizeColors - 1);
    posterColor = floor(posterColor);
    posterColor /= (posterizeColors - 1);
    
    posterColor = pow(posterColor, 1.0f / posterizeGamma);
    
    if (!posterizeUseAlpha)
    {
        posterColor.a = color.a;
    }
    else
    {
        REJECT_ALPHA(posterColor.a);
    }
    
    return posterColor;
}
#endif

#ifdef SOBEL_EDGE_EFFECT
// Sobel edge detection effect variables.
cbuffer GorgonSobelEdgeDetectEffect : register(b1)
{
    float2 sobelOffset = float2(0, 0);
    float sobelThreshold = 0.75f;
    float4 sobelLineColor = float4(0, 0, 0, 1);
}

// Function to perform a sobel edge detection.
float4 GorgonPixelShaderSobelEdge(GorgonSpriteVertex vertex) : SV_Target
{
    float2 uv = vertex.uv.xy / vertex.uv.w;
    float4 s00 = _gorgonTexture.Sample(_gorgonSampler, float3(uv + -sobelOffset, vertex.uv.z));
    float4 s01 = _gorgonTexture.Sample(_gorgonSampler, float3(uv + float2( 0,   -sobelOffset.y), vertex.uv.z));
    float4 s02 = _gorgonTexture.Sample(_gorgonSampler, float3(uv + float2( sobelOffset.x, -sobelOffset.y), vertex.uv.z));

    float4 s10 = _gorgonTexture.Sample(_gorgonSampler, float3(uv + float2(-sobelOffset.x,  0), vertex.uv.z));
    float4 s12 = _gorgonTexture.Sample(_gorgonSampler, float3(uv + float2( sobelOffset.x,  0), vertex.uv.z));

    float4 s20 = _gorgonTexture.Sample(_gorgonSampler, float3(uv + float2(-sobelOffset.x,  sobelOffset.y), vertex.uv.z));
    float4 s21 = _gorgonTexture.Sample(_gorgonSampler, float3(uv + float2( 0,    sobelOffset.y), vertex.uv.z));
    float4 s22 = _gorgonTexture.Sample(_gorgonSampler, float3(uv + sobelOffset, vertex.uv.z));

    float4 sobelX = s00 + 2 * s10 + s20 - s02 - 2 * s12 - s22;
    float4 sobelY = s00 + 2 * s01 + s02 - s20 - 2 * s21 - s22;

    float4 edgeSqr = sobelX * sobelX + sobelY * sobelY;
    float4 color = (1 - float4(edgeSqr.r <= sobelThreshold,
                            edgeSqr.g <= sobelThreshold,
                            edgeSqr.b <= sobelThreshold,
                            0));

    if ((color.r > 0) || (color.g > 0) || (color.b > 0))
    {
        color = sobelLineColor;
    }
    else
    {
        color = 0;
    }
    
    REJECT_ALPHA(color.a);

    return color;
}
#endif

#ifdef SILHOUETTE_EFFECT
// Function to draw the sprite as a colored silhouette.
float4 GorgonPixelShaderSilhouettePixelShader(GorgonSpriteVertex vertex) : SV_Target
{
    float3 color = vertex.color.rgb;
    float alpha = SampleMainTexture(vertex.uv, float4(0, 0, 0, 1)).a * vertex.color.a;
        
    REJECT_ALPHA(alpha);
    
    return float4(color, alpha);
}
#endif