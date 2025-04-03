// CrtScanlines.fx
// Pixel-accurate scanlines using screen-space position (VPOS) and resolution-aware bleed
// Now with softened sampling for anti-aliasing and curvature friendliness

sampler2D TextureSampler : register(s0) = sampler_state
{
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};

float time;
float scanlineIntensity = 0.4;
float scanlineSharpness = 3.0;
float bleedAmount = 0.2;
float flickerStrength = 0.05;
float maskStrength = 0.1;
float slotSharpness = 6.0;
float screenWidth = 1920.0; // passed from C# at runtime

float3 ApplyRGBMask(float2 uv, float3 color)
{
    float slot = sin(uv.y * slotSharpness * 3.14159);
    float3 mask = float3(1.0, 1.0, 1.0);

    if (fmod(floor(uv.x * 3.0), 3.0) == 0.0)
        mask = float3(1.0, 0.7, 0.7);
    else if (fmod(floor(uv.x * 3.0), 3.0) == 1.0)
        mask = float3(0.7, 1.0, 0.7);
    else
        mask = float3(0.7, 0.7, 1.0);

    return lerp(color, color * mask, maskStrength * (slot * 0.5 + 0.5));
}

float4 main(float2 uv : TEXCOORD0, float2 inputPos : VPOS) : COLOR0
{
    // === Anti-aliased sampling ===
    float2 offset = float2(1.0 / screenWidth, 0);
    float4 color = tex2D(TextureSampler, uv) * 0.5;
    color += tex2D(TextureSampler, uv + offset * 0.5) * 0.25;
    color += tex2D(TextureSampler, uv - offset * 0.5) * 0.25;

    // === Animated flicker ===
    float flicker = 1.0 + sin(time * 120.0 + uv.y * 240.0) * flickerStrength;
    color.rgb *= flicker;

    // === Scanline modulation using actual screen pixel row ===
    float scan = sin(inputPos.y * 3.14159 * scanlineSharpness);
    float scanlineMod = lerp(1.0, scanlineIntensity, (scan + 1.0) * 0.5);
    color.rgb *= scanlineMod;

    // === Horizontal bleed (now resolution-aware using screenWidth) ===
    float2 pixelSize = float2(1.0 / screenWidth, 0.0);
    float3 bleed = 0;
    bleed += tex2D(TextureSampler, uv - pixelSize).rgb * 0.25;
    bleed += tex2D(TextureSampler, uv).rgb * 0.5;
    bleed += tex2D(TextureSampler, uv + pixelSize).rgb * 0.25;
    color.rgb = lerp(color.rgb, bleed, bleedAmount);

    // === RGB Mask ===
    color.rgb = ApplyRGBMask(uv, color.rgb);

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}