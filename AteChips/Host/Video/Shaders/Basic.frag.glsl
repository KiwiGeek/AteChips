#version 330 core

out vec4 FragColor;

in vec2 vTexCoord;

uniform sampler2D uTexture;
uniform vec3 u_PhosphorColor;

void main()
{
    float luminance = texture(uTexture, vTexCoord).r; // fetch RED only

    vec3 color = luminance * u_PhosphorColor; // spread luminance across phosphor RGB
    FragColor = vec4(color, 1.0);
}