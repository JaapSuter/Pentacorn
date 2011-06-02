uniform float4x4 WorldViewProj;
uniform float4 Color = { 1, 1, 1, 1 };
uniform float2 Position = { 100, 100 };
uniform float2 InnerAndOuterRadius = 100;


void Circle2dVertexShader(inout float4 position : SV_Position, out float2 screenPos : TEXCOORD0)
{
    screenPos = position.xy;
    position = mul(position, WorldViewProj);    
}

float4 Circle2dPixelShader(in float2 screenPos : TEXCOORD0) : SV_Target0
{    
    screenPos -= Position;
    float dist = length(screenPos);
    if (dist > (InnerAndOuterRadius.y + 1))
        clip(-1);
    if (dist < (InnerAndOuterRadius.x - 1))
        clip(-1);
    
    if (dist <= InnerAndOuterRadius.y)
    {
        if (dist >= InnerAndOuterRadius.x)
            return Color;
        else 
            return (dist - InnerAndOuterRadius.x) * Color.rgba;
    }
    else
        return (InnerAndOuterRadius.y + 1 - dist) * Color.rgba;
    
    return float4(0, 0, 0, 1);
}

technique Circle2dEffect
{
    pass
    {
        VertexShader = compile vs_2_0 Circle2dVertexShader();
        PixelShader  = compile ps_2_0 Circle2dPixelShader();
    }
}
