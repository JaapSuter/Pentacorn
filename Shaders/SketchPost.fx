uniform float4x4    WorldViewProj;
uniform float4      Color;
uniform texture2D   Texture;
uniform sampler     TextureSampler = sampler_state { Texture = (Texture); };

const float threshold = 2.9;
const float2 c[9] = {
        float2(-0.0078125, 0.0078125), 
        float2( 0.00 ,     0.0078125),
        float2( 0.0078125, 0.0078125),
        float2(-0.0078125, 0.00 ),
        float2( 0.0,       0.0),
        float2( 0.0078125, 0.007 ),
        float2(-0.0078125,-0.0078125),
        float2( 0.00 ,    -0.0078125),
        float2( 0.0078125,-0.0078125),
};


void SketchPostVertexShader(inout float4 position : SV_Position, inout float2 texCoord : TEXCOORD0)
{
    position = mul(position, WorldViewProj);
}

float OutlinesFunction3x3(float2 texCoord)
{
  float4 lum = float4(0.30, 0.59, 0.11, 1);

  float s11 = dot(tex2D(TextureSampler, texCoord + float2(-1.0f / 1024.0f, -1.0f / 768.0f)), lum);	
  float s12 = dot(tex2D(TextureSampler, texCoord + float2(0, -1.0f / 768.0f)), lum);				    
  float s13 = dot(tex2D(TextureSampler, texCoord + float2(1.0f / 1024.0f, -1.0f / 768.0f)), lum);	
  float s21 = dot(tex2D(TextureSampler, texCoord + float2(-1.0f / 1024.0f, 0)), lum);				
  float s23 = dot(tex2D(TextureSampler, texCoord + float2(-1.0f / 1024.0f, 0)), lum); 				  
  float s31 = dot(tex2D(TextureSampler, texCoord + float2(-1.0f / 1024.0f, 1.0f / 768.0f)), lum);	
  float s32 = dot(tex2D(TextureSampler, texCoord + float2(0, 1.0f / 768.0f)), lum);
  float s33 = dot(tex2D(TextureSampler, texCoord + float2(1.0f / 1024.0f, 1.0f / 768.0f)), lum);
  
  float t1 = s13 + s33 + (2 * s23) - s11 - (2 * s21) - s31;
  float t2 = s31 + (2 * s32) + s33 - s11 - (2 * s12) - s13;

  float4 col;

  return (t1 * t1) + (t2 * t2);
}

float4 SketchPostPixelShader(in float2 texCoord : TEXCOORD0, in float4 color : COLOR0) : SV_Target0
{   
    float ol = OutlinesFunction3x3(texCoord);

    if (ol > 0.03)
        return float4(0,0,0,1);

    float4 texel = tex2D(TextureSampler, texCoord);    
    return texel.rgba * color.rgba * Color;
}

technique SketchPostEffect
{
    pass
    {
        VertexShader = compile vs_2_0 SketchPostVertexShader();
        PixelShader  = compile ps_2_0 SketchPostPixelShader();
    }
}
