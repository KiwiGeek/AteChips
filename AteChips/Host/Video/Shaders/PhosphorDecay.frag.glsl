#version 330 core

out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D PrevFrame;
uniform sampler2D NewFrame;
uniform vec2 Resolution;
uniform float DecayRate;

void main()
{
    vec2 uv = gl_FragCoord.xy / Resolution;

    vec4 prevColor = texture(PrevFrame, uv);
    vec4 newColor = texture(NewFrame, uv);

    // Blend new color into old color based on decay rate
    vec4 blended = mix(newColor, prevColor, DecayRate);

    FragColor = blended;
}
