uniform float4x4 WorldViewProj;
uniform float4 WhiteColor = { 1, 1, 1, 1 };
uniform float4 BlackColor = { 0, 0, 0, 1 };
uniform float4 OtherColor = { 1, 0, 0, 1 };
uniform int4 Board;
uniform int2 Square;

void Chessboard2dVertexShader(inout float4 position : SV_Position, out float2 screenPos : TEXCOORD0)
{
    screenPos = position.xy;
    position = mul(position, WorldViewProj);    
}

float4 Chessboard2dPixelShader(in float2 screenPos : TEXCOORD0) : SV_Target0
{    
    int2 xy = floor(screenPos.xy);
    
    xy -= Board.xy;

    if (xy.x < 0) return OtherColor;
    if (xy.y < 0) return OtherColor;
    if (xy.x >= Board.z) return OtherColor;
    if (xy.y >= Board.w) return OtherColor;

    xy /= Square;
    
    int2 eo = xy % 2;
    if (eo.x != eo.y)
        return WhiteColor;
    
    return BlackColor;
}

technique Chessboard2dEffect
{
    pass
    {
        VertexShader = compile vs_2_0 Chessboard2dVertexShader();
        PixelShader  = compile ps_2_0 Chessboard2dPixelShader();
    }
}
