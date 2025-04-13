#version 330 core

// Input texture coordinate from vertex shader
in vec2 vTexCoord;

// Output color of the pixel
out vec4 FragColor;

// The texture containing the emulator's framebuffer (red-channel only)
uniform sampler2D uTexture;

void main()
{
    // Sample the full RGBA color from the texture
    FragColor = texture(uTexture, vTexCoord);
}