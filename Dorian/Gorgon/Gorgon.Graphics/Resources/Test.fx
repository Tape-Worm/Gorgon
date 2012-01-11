 struct TEMPGUY
 {
	float value1;
	float4x4 _view;
	float tempArray[3];
 };

 cbuffer _transformOnce : register(b0)
 {
	float4x4 _proj;
	float4x4 value2;
	float _array[3];
	TEMPGUY _valueType;
	Texture2D _myTex;
	SamplerState _sampler;
 }

 cbuffer _transformPerFrame : register(b1)
 {
	matrix _world;
	float4 _alpha = float4(1.0f, 1.0f, 1.0f, 1.0f);
 }

 Texture2D theTexture : register(t0);
 SamplerState sample : register(s0);
  
 struct VS_IN
 {
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 uv : TEXTURECOORD;
 };
 
 struct PS_IN
 {
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 uv : TEXTURECOORD;
 };
 
 PS_IN VS( VS_IN input )
 {
	PS_IN output = (PS_IN)0;

	input.pos.w = 1.0f;	
	//output.pos = input.pos;
	output.pos = mul(input.pos, _world);
	output.pos = mul(output.pos, _valueType._view);
	output.pos = mul(output.pos, _proj);
	//output.col = input.col;
	output.col = float4(_array[0], _array[1], _array[2], 1.0f);
	output.uv = input.uv;
	
	return output;
 }
 
 float4 PS( PS_IN input ) : SV_Target
 {	
	float4 color = theTexture.Sample(sample, input.uv) * input.col;
	float4 color2 = _myTex.Sample(_sampler, input.uv);

	if (color.b > 1.00f)
		color.a *= _alpha.a;
	else
		color *= _alpha;

	if ((color.r > 0.75f) && (color.g > 0.75f) && (color.b > 0.75f) && ((input.uv.x < 0.5f) || (input.uv.y < 0.5f)))
		return color;
	else
		return float4(color.rgb * color2.rgb, color.a);
 }