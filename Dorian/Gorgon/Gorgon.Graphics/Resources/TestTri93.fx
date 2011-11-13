 matrix _world;

 struct VS_IN
 {
 	float4 pos : POSITION;
 	float4 col : COLOR;
 };
 
 struct PS_IN
 {
 	float4 pos : SV_POSITION;
 	float4 col : COLOR;
 };
 
 PS_IN VS( VS_IN input )
 {
 	PS_IN output = (PS_IN)0;
 	
 	output.pos = mul(input.pos, _world);
 	output.col = input.col;
 	
 	return output;
 }
 
 float4 PS( PS_IN input ) : SV_Target
 {
 	return input.col;
 }
 
 technique10 Render
 {
 	pass P0
 	{
 		SetGeometryShader( 0 );
 		SetVertexShader( CompileShader( vs_4_0_level_9_3, VS() ) );
 		SetPixelShader( CompileShader( ps_4_0_level_9_3, PS() ) );
 	}
 }