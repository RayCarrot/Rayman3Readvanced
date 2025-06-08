#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

extern float4x4 WorldViewProj;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 TextureCoordinates : TEXCOORD0;
};
    
struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

PixelShaderInput MainVS(VertexShaderInput v)
{
    PixelShaderInput output;

    output.Position = mul(v.Position, WorldViewProj);
    output.Color = v.Color;
    output.TextureCoordinates = v.TextureCoordinates;
    return output;
}

technique SpriteBlending
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
    }
};