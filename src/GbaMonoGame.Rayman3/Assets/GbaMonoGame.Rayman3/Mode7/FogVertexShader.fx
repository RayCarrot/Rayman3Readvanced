#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Matrix for transforming vertex positions
float4x4 WorldViewProj;
float FarPlane;
float FadeDistance;
Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

// Vertex input
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

// Vertex output
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float4 DepthInfo : TEXCOORD1;
};

// Vertex shader
VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProj);
    output.TextureCoordinates = input.TextureCoordinates;
    output.DepthInfo = float4(output.Position.w, 0, 0, 0);
    return output;
}

// Pixel shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float distance = FarPlane - input.DepthInfo.x;
    
    float alpha;
    if (distance < FadeDistance)
        alpha = distance / FadeDistance;
    else
        alpha = 1;
    
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    color.a = alpha;
    return color;
}

// Technique
technique SpriteDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
