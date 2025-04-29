
// Shaders/Scanlines.frag
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
    vec4 color = texture(Texture, uv);

    // Better scanline pattern
    float scanline = sin(uv.y * Resolution.y * 3.14159) * 0.5 + 0.5;
    scanline = pow(scanline, Sharpness * 10.0);
    scanline = clamp(scanline, 0.3, 1.0);
    
    // Calculate "frame number" (assuming ~60 FPS)
    float frame = floor(Time * 60.0);
    
    // Flicker: randomize slightly per frame
    float flicker = fract(sin(frame * 12.9898) * 43758.5453);
    flicker = (flicker * 2.0 - 1.0) * FlickerStrength;
    
    // Bleed
    vec4 bleedUp   = texture(Texture, uv + vec2(0.0,  1.0 / Resolution.y));
    vec4 bleedDown = texture(Texture, uv - vec2(0.0,  1.0 / Resolution.y));
    vec4 bleed     = (bleedUp + bleedDown) * (BleedAmount * 0.5);
    
    // Mask
    float mask = sin(uv.x * Resolution.x * 0.3) * sin(uv.y * Resolution.y * 0.8);
    mask = pow(abs(mask), SlotSharpness * 5.0);
    
    // Correct modulation
    float modulation = 1.0 - (1.0 - scanline) * Intensity;
    modulation += flicker + (mask * MaskStrength);
    modulation = clamp(modulation, 0.0, 1.5);
    
    FragColor = (color + bleed) * modulation;
}
