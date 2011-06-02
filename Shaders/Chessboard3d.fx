uniform float4x4 WorldViewProj;
uniform float4 WhiteColor = { 1, 1, 1, 1 };
uniform float4 BlackColor = { 0, 0, 0, 1 };
uniform float2 Origin = {0, 0};
uniform float2 Size = { 1.0, 0.1 };
uniform float2 Dim = { 3, 5 };

void Chessboard3dVertexShader(inout float4 position : SV_Position,
                              out float4 posGrid : TEXCOORD0)
{
    posGrid = position;
    position = mul(position, WorldViewProj);    
}

float4 Chessboard3dPixelShader(in float4 posGrid : TEXCOORD0) : SV_Target0
{
    float2 size = { Size.x, -Size.y };
    float2 orig = Origin - size;
    float2 dim = Dim + float2(1, 1);
    float2 xy = (posGrid.xy - orig) / size;

    float2 bl = { 0, 0 };
    float2 tr = dim;

    if (any(xy < bl)) return WhiteColor;            
    if (any(xy > tr)) return WhiteColor;
    
    int2 eo = floor(xy) % 2;

    if (eo.x != eo.y)
        return WhiteColor;
    
    return BlackColor;
}

technique Chessboard3dEffect
{
    pass
    {
        VertexShader = compile vs_2_0 Chessboard3dVertexShader();
        PixelShader  = compile ps_2_0 Chessboard3dPixelShader();
    }
}
