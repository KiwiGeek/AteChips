// FxaaAntialias.fx
// Lightweight FXAA-inspired screen-space anti-aliasing pass

sampler2D TextureSampler : register(s0) = sampler_state
{
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};

float2 screenSize; // passed from C# (width, height)

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 invRes = 1.0 / screenSize;

    // Sample surrounding pixels
    float3 col = tex2D(TextureSampler, uv).rgb;
    float3 l = tex2D(TextureSampler, uv + float2(-invRes.x, 0)).rgb;
    float3 r = tex2D(TextureSampler, uv + float2(invRes.x, 0)).rgb;
    float3 u = tex2D(TextureSampler, uv + float2(0, -invRes.y)).rgb;
    float3 d = tex2D(TextureSampler, uv + float2(0, invRes.y)).rgb;

    float3 blur = (l + r + u + d + col) / 5.0;
    float3 diff = abs(col - blur);
    float edge = saturate(dot(diff, float3(0.333, 0.333, 0.333)) * 8.0);

    return float4(lerp(blur, col, 1.0 - edge), 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}
