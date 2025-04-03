// Bloom.fx
// Multi-directional blur bloom effect for glowing pixels

sampler2D TextureSampler : register(s0);

float2 resolution;
float bloomIntensity = 1.2;
float bloomSpread = 1.0 / 256.0;

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, uv) * 0.5; // base

    float4 bloom = float4(0, 0, 0, 0);
    float2 offset = float2(bloomSpread, bloomSpread);

    // Sample in 8 directions around the pixel
    bloom += tex2D(TextureSampler, uv + float2(offset.x, 0));
    bloom += tex2D(TextureSampler, uv + float2(-offset.x, 0));
    bloom += tex2D(TextureSampler, uv + float2(0, offset.y));
    bloom += tex2D(TextureSampler, uv + float2(0, -offset.y));
    bloom += tex2D(TextureSampler, uv + float2(offset.x, offset.y));
    bloom += tex2D(TextureSampler, uv + float2(-offset.x, -offset.y));
    bloom += tex2D(TextureSampler, uv + float2(offset.x, -offset.y));
    bloom += tex2D(TextureSampler, uv + float2(-offset.x, offset.y));

    bloom /= 8.0;
    bloom *= bloomIntensity;

    return saturate(color + bloom);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}