// PhosphorDecay.fx
// Decay blending with saturation-safe output (no tint)

sampler2D CurrentFrame : register(s0);
sampler2D PreviousFrame : register(s1);

float decayFactor = 0.97; // Controls trail persistence

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float3 curr = tex2D(CurrentFrame, uv).rgb;
    float3 prev = tex2D(PreviousFrame, uv).rgb;

    float3 result = lerp(prev, curr, 1.0 - decayFactor);
    return float4(saturate(result), 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}