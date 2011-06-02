uniform float4x4 WorldViewProj;
uniform float4 WhiteColor = { 1, 1, 1, 1 };
uniform float4 BlackColor = { 0, 0, 0, 1 };
uniform float2 Offset = { 0, 0 };
uniform int2 Step = { 0, 0 };

void GrayCode2dVertexShader(inout float4 position : SV_Position, out float2 screenPos : TEXCOORD0)
{
    screenPos = position.xy;
    position = mul(position, WorldViewProj);
}

float4 GrayCode2dPixelShader(in float2 screenPos : TEXCOORD0) : SV_Target0
{    
    return BlackColor;
}

technique Chessboard2dEffect
{
    pass
    {
        VertexShader = compile vs_2_0 GrayCode2dVertexShader();
        PixelShader  = compile ps_2_0 GrayCode2dPixelShader();
    }
}
