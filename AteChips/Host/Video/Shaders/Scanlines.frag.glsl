#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D Texture;
uniform float Intensity;
uniform float Sharpness;
uniform float BleedAmount;
uniform float FlickerStrength;
uniform float MaskStrength;
uniform float SlotSharpness;
uniform float Time;
uniform vec2 Resolution;

void main()
{
    vec2 uv = vec2(TexCoord.x, 1.0 - TexCoord.y);

    // Texture sampling
    vec4 color = texture(Texture, uv);

    // --- Scanlines ---
    float pixelY = Resolution.y * uv.y; // pixel-space Y
    float pixelsPerScanline = 2.0; //  How many physical pixels between dark/light cycles (can expose to ImGui later)
    float phase = 3.14159 * pixelY / pixelsPerScanline;
    float scan = sin(phase);
    float normalizedScan = (scan + 1.0) * 0.5;
    float modulation = mix(1.0, normalizedScan, Intensity);

    // --- Flicker ---
    float frame = floor(Time * 60.0); // ~60fps assumption
    float flickerNoise = fract(sin(frame * 12.9898) * 43758.5453); // randomish per frame
    float flicker = (flickerNoise * 2.0 - 1.0) * FlickerStrength;
    modulation += flicker;

    // --- Bleed (vertical) ---
    vec4 bleedUp   = texture(Texture, uv + vec2(0.0, 1.0 / Resolution.y));
    vec4 bleedDown = texture(Texture, uv - vec2(0.0, 1.0 / Resolution.y));
    vec4 bleed = (bleedUp + bleedDown) * (BleedAmount * 0.5);

    // --- RGB Mask ---
    float pixelX = Resolution.x * uv.x;
    float mask = sin(pixelX * 3.14159 / 1.5) * sin(pixelY * 3.14159 / 1.5); 
    mask = pow(abs(mask), SlotSharpness * 5.0);
    modulation += mask * MaskStrength;

    // --- Final output ---
    modulation = clamp(modulation, 0.0, 1.5);
    FragColor = (color + bleed) * modulation;
}
