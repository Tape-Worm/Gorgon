Texture2D<float4> _inputTexture : register(t0);
RWTexture2D<uint> _outputTexture : register(u0);

// Parameters to control the filter.
cbuffer SobelParams : register(b0)
{
	float Thickness;
	float Threshold;
}

// Function to gray scale a pixel.
float Luminance(float3 sourceRgb)
{
	return ((sourceRgb.r + sourceRgb.g + sourceRgb.b) / 3.0f);//dot(sourceRgb, float3(0.2125f, 0.7154f, 0.0721f));
}

// This method packs a 4-component floating point value into a single unsigned integer for our output view.
uint Pack(float4 inputValue)
{
	// We need to pack the value into an integer value for our output.
	return (uint)(inputValue.a * 255) << 24 | (uint)(inputValue.b * 255) << 16 | (uint)(inputValue.g * 255) << 8 | (uint)(inputValue.r * 255);
}

[numthreads(32, 32, 1)]
void SobelCS(uint3 threadID : SV_DispatchThreadID)
{
	float4 inputValue = _inputTexture[threadID.xy];

	// This is based on the optimized Sobel algorithm found at http://homepages.inf.ed.ac.uk/rbf/HIPR2/sobel.htm
	float value1 = Luminance(_inputTexture[threadID.xy + float2(-Thickness, -Thickness)].rgb);
	float value2 = Luminance(_inputTexture[threadID.xy + float2(0, -Thickness)].rgb);
	float value3 = Luminance(_inputTexture[threadID.xy + float2(Thickness, -Thickness)].rgb);
	float value4 = Luminance(_inputTexture[threadID.xy + float2(-Thickness, 0)].rgb);
	float value5 = Luminance(inputValue.rgb);
	float value6 = Luminance(_inputTexture[threadID.xy + float2(Thickness, 0)].rgb);
	float value7 = Luminance(_inputTexture[threadID.xy + float2(-Thickness, Thickness)].rgb);
	float value8 = Luminance(_inputTexture[threadID.xy + float2(0, -Thickness)].rgb);
	float value9 = Luminance(_inputTexture[threadID.xy + float2(Thickness, Thickness)].rgb);

	float sobelX = mad(2, value2, value1 + value3) - mad(2, value8, value7 + value9);
	float sobelY = mad(2, value6, value3 + value9) - mad(2, value4, value1 + value7);
	
	float edgeSqr = (sobelX * sobelX + sobelY * sobelY);
	float result = 1.0 - (edgeSqr > Threshold * Threshold);
		
	// We need to pack the value into an integer value for our output.
	_outputTexture[threadID.xy] = Pack(float4(float3(result, result, result) * inputValue.rgb, result < 0.001f ? 1 : inputValue.a));
}