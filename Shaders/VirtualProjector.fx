uniform float4x4    WorldViewProj;
uniform float4x4    WorldProjectorViewProj;
uniform float4      Color = { 1, 1, 1, 1 };
uniform texture2D   Texture;
uniform sampler     TextureSampler = sampler_state 
{ 
    Texture = (Texture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

void VirtualProjectorVertexShader(inout float4 position : SV_Position, out float4 texCoord : TEXCOORD0)
{
    texCoord = mul(position, WorldProjectorViewProj);
    position = mul(position, WorldViewProj);
}

float4 VirtualProjectorPixelShader(in float4 texCoord : TEXCOORD0) : SV_Target0
{   
    texCoord.xyz /= texCoord.w;
   
    texCoord.x =  0.5 * texCoord.x + 0.5f;
    texCoord.y = -0.5 * texCoord.y + 0.5f;
    
    float4 texel = tex2D(TextureSampler, texCoord.xy);
    return texel.rgba * Color;
}

technique VirtualProjectorEffect
{
    pass
    {
        VertexShader = compile vs_2_0 VirtualProjectorVertexShader();
        PixelShader  = compile ps_2_0 VirtualProjectorPixelShader();
    }
}
