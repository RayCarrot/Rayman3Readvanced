#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define CONTRAST 25

#define TEX_WIDTH 256.0
#define WRAP float2(2, 1)

#define SCROLL_Y1 32.0 / TEX_WIDTH
#define SCROLL_Y2 120.0 / TEX_WIDTH

// Original values are 2, 4 and 8
#define SCROLL_SPEED_Y0 3.0
#define SCROLL_SPEED_Y1 4.0
#define SCROLL_SPEED_Y2 8.0

// The original sprite texture (the first Texture2D always gets set to this)
extern Texture2D PrimaryTexture;

// The parameters
extern Texture2D SecondaryTexture;
const float Time;

sampler2D PrimaryTextureSampler = sampler_state
{
    Texture = <PrimaryTexture>;

    // Do this to prevent interpolated values
    AddressU = wrap;
    AddressV = wrap;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
};

sampler2D SecondaryTextureSampler = sampler_state
{
    Texture = <SecondaryTexture>;

    // Do this to prevent interpolated values
    AddressU = wrap;
    AddressV = wrap;
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

float4 mix(float4 a, float4 b, float factor)
{
    return a + (b - a) * factor;
}

float Sigmoid(float x)
{
    return 1.0 / (1.0 + (exp(-(x - 0.5) * CONTRAST)));
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Get the UV
    float2 uv = input.TextureCoordinates;
    
    // Determine the scroll speed based on the Y coordinate
    float scrollSpeed;
    if (uv.y > SCROLL_Y2)
        scrollSpeed = SCROLL_SPEED_Y2;
    else if (uv.y > SCROLL_Y1)
        scrollSpeed = SCROLL_SPEED_Y1;
    else
        scrollSpeed = SCROLL_SPEED_Y0;
    
    // Get the colors from the two textures
    float4 c1 = tex2D(PrimaryTextureSampler, float2((Time / scrollSpeed / TEX_WIDTH), 0.0) + uv * WRAP);
    float4 c2 = tex2D(SecondaryTextureSampler, float2((Time / scrollSpeed / TEX_WIDTH), 0.0) + uv * WRAP);
    
    // Calculate the factor
    float factor = Sigmoid(uv.x);
    
    // Blend and return
    return mix(c1, c2, factor);
}

technique SpriteBlending
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};