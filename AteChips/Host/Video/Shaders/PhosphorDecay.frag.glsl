#version 330 core

in      vec2        TexCoord;
out     vec4        FragColor;
uniform sampler2D   History;
uniform sampler2D   NewFrame;
uniform float       DecayRate;

void main()
{
    vec2 tc = vec2(TexCoord.x, 1.0 - TexCoord.y);

    float old = texture(History, tc).r;
    float cur = texture(NewFrame, tc).r;

    float result = max(cur, old * DecayRate);
    FragColor    = vec4(result);
}