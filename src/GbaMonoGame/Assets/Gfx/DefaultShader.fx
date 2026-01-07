#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// The original sprite texture (the first Texture2D always gets set to this)
extern Texture2D SpriteTexture;

sampler2D SpriteTextureSampler : register(s0) = sampler_state
{
    Texture = <SpriteTexture>;

    // Do this to prevent interpolated values
    AddressU = clamp;
    AddressV = clamp;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Get the alpha value from the sprite texture. This is our palette index, in a range from 0-1.
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);

    // If the alpha is 0 then we discard the pixel. This is mainly because when rendering sprites
    // to the render targets, for layer-specific alpha blending, we have a custom blend state which
    // overwrites pixels instead of blending them, including fully transparent ones, which we don't
    // want. So this solves that by discarding fully transparent pixels.
    if (color.a <= 0)
        discard;
    
    // Return and multiply by the input color.
    return color * input.Color;
}

technique SpriteBlending
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};