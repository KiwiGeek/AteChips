// Vignette.fx
// Soft dark corners for retro CRT display feel

sampler2D TextureSampler : register(s0);

float vignetteStrength = 1.5; // Higher = stronger corners

float vignetteMask(float2 uv)
{
    float2 pos = uv - 0.5;
    float dist = dot(pos, pos);
    return saturate(1.0 - dist * vignetteStrength);
}

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, uv);
    color.rgb *= vignetteMask(uv);
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}
