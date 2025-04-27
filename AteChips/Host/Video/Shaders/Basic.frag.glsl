#version 330 core

out vec4 FragColor;

in vec2 vTexCoord;

uniform sampler2D uTexture;
uniform vec3 u_PhosphorColor;

void main()
{
    vec4 texColor = texture(uTexture, vTexCoord);
    texColor.rgb *= u_PhosphorColor; // <- tint it!
    FragColor = texColor;
}