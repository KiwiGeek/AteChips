#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D OriginalTexture;
uniform sampler2D BloomTexture;
uniform float BloomIntensity;

void main()
{
    vec2 uv = vec2(TexCoord.x, 1.0 - TexCoord.y); // FLIP VERTICALLY
    vec3 original = texture(OriginalTexture, uv).rgb;
    vec3 bloom = texture(BloomTexture, uv).rgb;

    vec3 result = original + bloom * BloomIntensity;
    FragColor = vec4(result, 1.0);
}
