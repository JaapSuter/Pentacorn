uniform float4x4    WorldViewProj;
uniform float4      Color;
uniform texture2D   Texture;
uniform sampler     TextureSampler = sampler_state { Texture = (Texture); };

void RgbaTextureVertexShader(inout float4 position : SV_Position, inout float2 texCoord : TEXCOORD0, inout float4 color : COLOR0)
{
    position = mul(position, WorldViewProj);
}

float4 RgbaTexturePixelShader(in float2 texCoord : TEXCOORD0, in float4 color : COLOR0) : SV_Target0
{   
    float4 texel = tex2D(TextureSampler, texCoord);    
    return texel.rgba * color.rgba * Color;
}

technique RgbaTextureEffect
{
    pass
    {
        VertexShader = compile vs_2_0 RgbaTextureVertexShader();
        PixelShader  = compile ps_2_0 RgbaTexturePixelShader();
    }
}
