uniform float4x4 WorldViewProj;
uniform float4 WhiteColor = { 1, 1, 1, 1 };
uniform float4 BlackColor = { 0, 0, 0, 1 };
uniform float4 OtherColor = { 1, 0, 0, 1 };
uniform float2 V0 = { 100, 100 };
uniform float2 V1 = { 100, 100 };
uniform float2 V2 = { 100, 100 };
uniform float2 V3 = { 100, 100 };
uniform int M = 100;
uniform int N = 100;


void Circle2dVertexShader(inout float4 position : SV_Position, out float2 screenPos : TEXCOORD0)
{
    screenPos = position.xy;
    position = mul(position, WorldViewProj);    
}

float4 Circle2dPixelShader(in float2 screenPos : TEXCOORD0) : SV_Target0
{    
    return float4(1, 0, 1, 1);
}

technique Circle2dEffect
{
    pass
    {
        VertexShader = compile vs_2_0 Circle2dVertexShader();
        PixelShader  = compile ps_2_0 Circle2dPixelShader();
    }
}
