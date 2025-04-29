#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D InputTexture;
uniform vec2 Resolution;

void main()
{
    vec2 uv = vec2(TexCoord.x, 1.0 - TexCoord.y); // FLIP VERTICALLY
    vec2 texel = 1.0 / Resolution;
    vec3 result = vec3(0.0);

    for (int x = -1; x <= 1; ++x)
    for (int y = -1; y <= 1; ++y)
    {
        vec2 offset = vec2(float(x), float(y)) * texel;
        result += texture(InputTexture, uv + offset).rgb;
    }

    result /= 9.0;
    FragColor = vec4(result, 1.0);
}
