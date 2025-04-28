#version 330 core

in      vec2        TexCoord;
out     vec4        FragColor;
uniform sampler2D   Texture;
uniform vec3        PhosphorColor;

void main()
{
    float luminance = texture(Texture, TexCoord).r;
    vec3 color = luminance * PhosphorColor;
    FragColor = vec4(color, 1.0);
}