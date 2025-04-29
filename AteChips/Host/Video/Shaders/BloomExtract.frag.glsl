#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D InputTexture;
uniform float Threshold;

void main()
{
    vec2 uv = vec2(TexCoord.x, 1.0 - TexCoord.y);
    vec3 color = texture(InputTexture, uv).rgb;
    float brightness = dot(color, vec3(0.2126, 0.7152, 0.0722)); // luminance

    // Only keep pixels above the brightness threshold
    vec3 result = brightness > Threshold ? color : vec3(0.0);

    FragColor = vec4(result, 1.0);
}
