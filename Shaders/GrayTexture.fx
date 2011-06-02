uniform float4x4    WorldViewProj;
uniform float4      Color;
uniform texture2D   Texture;
uniform sampler     TextureSampler = sampler_state { Texture = (Texture); };

void GrayTextureVertexShader(inout float4 position : SV_Position, inout float4 texCoord : TEXCOORD0, inout float4 color : COLOR0)
{
    position = mul(position, WorldViewProj);
    color = color;
}

float4 GrayTexturePixelShader(in float4 texCoord : TEXCOORD0, in float4 color : COLOR0) : SV_Target0
{   
    float4 texel = tex2D(TextureSampler, texCoord);
    return float4(texel.aaa, 1) * color.rgba * Color;
}

technique GrayTextureEffect
{
    pass
    {
        VertexShader = compile vs_2_0 GrayTextureVertexShader();
        PixelShader  = compile ps_2_0 GrayTexturePixelShader();
    }
}
