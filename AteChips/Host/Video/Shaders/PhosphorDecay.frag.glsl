#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D u_History;
uniform sampler2D u_NewFrame;
uniform float     u_DecayRate;

void main()
{
    vec2 tc = vec2(vTexCoord.x, 1.0 - vTexCoord.y);

    float old = texture(u_History, tc).r;
    float cur = texture(u_NewFrame, tc).r;

    float result = max(cur, old * u_DecayRate);
    FragColor    = vec4(result);
}