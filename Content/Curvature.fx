// Curvature.fx
// God-tier CRT curvature: barrel distortion, edge fade, RGB warp, and corner flicker (all user-tweakable)

sampler2D TextureSampler : register(s0);

float2 resolution;
float curvatureAmount = 0.25; // Adjustable strength
float time; // Required for corner flicker
float edgeFadeStrength = 1.5; // Higher = stronger vignette
float flickerStrength = 0.02; // How much brightness flickers
float flickerSpeed = 60.0; // Flicker frequency
float warpAmount = 0.002; // RGB separation offset

float edgeFade(float2 uv)
{
    float2 offset = uv - 0.5;
    float dist = length(offset);
    return saturate(1.0 - pow(dist * edgeFadeStrength, 3.0));
}

float cornerFlicker(float2 uv)
{
    float2 delta = uv - 0.5;
    float flicker = sin(time * flickerSpeed + dot(delta, delta) * 400.0);
    return 1.0 + flickerStrength * flicker;
}

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 coord = uv * 2.0 - 1.0; // -1 to 1 space

    // Barrel distortion
    float2 offset = coord;
    offset.x *= 1.0 + (coord.y * coord.y) * curvatureAmount;
    offset.y *= 1.0 + (coord.x * coord.x) * curvatureAmount;
    coord = offset;

    uv = (coord * 0.5) + 0.5; // Back to 0–1 space

    // Clamp to screen bounds
    if (uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0)
        return float4(0, 0, 0, 1);

    // RGB warp
    float2 warpOffset = normalize(uv - 0.5) * warpAmount;
    float r = tex2D(TextureSampler, uv - warpOffset).r;
    float g = tex2D(TextureSampler, uv).g;
    float b = tex2D(TextureSampler, uv + warpOffset).b;
    float4 color = float4(r, g, b, 1.0);

    // Distance-based fade
    float vignette = edgeFade(uv);
    color.rgb *= vignette;

    // Corner flicker
    color.rgb *= cornerFlicker(uv);

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}
