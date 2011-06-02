uniform float4x4 WorldViewProj;
uniform float4 FillColor = { 1, 1, 1, 1 };
uniform float4 LineColor = { 0, 0, 0, 1 };
uniform float LineEvery = 1.0;
uniform float LineWidth = 0.1;

void GridVertexShader(inout float4 position : SV_Position,
                        out float4 posGrid : TEXCOORD0)
{
    posGrid = position;
    position = mul(position, WorldViewProj);    
}

void GridPixelShader(in float4 posGrid : TEXCOORD0, out float4 pixel : SV_Target0)
{
    float2 offset = { 100, 100 }; // Todo, Jaap Suter, November 2010, fix ugly fmod negative hack
    
    posGrid.z = sqrt(posGrid.x * posGrid.x + posGrid.y * posGrid.y);
    posGrid.xy += offset;
    posGrid /= LineEvery;
    
    float3 dst = abs(posGrid - round(posGrid));
    dst /= (LineWidth / 2);
    float3 scale = dst;
    
    pixel = lerp(LineColor, FillColor, saturate(min(min(scale.x, scale.y), scale.z)));
}

technique Grid
{
    pass
    {
        VertexShader = compile vs_2_0 GridVertexShader();
        PixelShader  = compile ps_2_0 GridPixelShader();
    }
}
