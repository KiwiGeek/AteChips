// PhosphorTint.fx
// Always-on shader to apply user-selectable phosphor tint color

sampler2D TextureSampler : register(s0);

float3 phosphorColor = float3(0.1f, 1.0f, 0.1f); // Default: white

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, uv);
    color.rgb *= phosphorColor;
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}
