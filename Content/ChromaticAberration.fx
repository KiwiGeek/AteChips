// ChromaticAberration.fx
// Separates RGB channels slightly for an old-lens effect

sampler2D TextureSampler : register(s0);

float aberrationAmount = 0.003; // How far to shift RGB

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    // Shift each channel's UV slightly outward from center
    float2 offset = (uv - 0.5) * aberrationAmount;

    float r = tex2D(TextureSampler, uv + offset).r;
    float g = tex2D(TextureSampler, uv).g;
    float b = tex2D(TextureSampler, uv - offset).b;

    return float4(r, g, b, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}
