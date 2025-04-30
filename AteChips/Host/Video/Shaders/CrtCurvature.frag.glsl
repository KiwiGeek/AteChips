#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D Texture;

// Per-axis curvature (applies non-radial distortion)
uniform float CurvatureX;
uniform float CurvatureY;

// Per-axis warp (true barrel/pincushion: radial, but asymmetric)
uniform float WarpX;
uniform float WarpY;

void main()
{
    // Flip Y to match top-left origin
    vec2 flippedTexCoord = vec2(TexCoord.x, 1.0 - TexCoord.y);

    // Normalize coordinates to [-1, 1], centered at screen
    vec2 centered = flippedTexCoord * 2.0 - 1.0;

    // --- Apply directional warp (barrel/pincushion) ---
    float r2 = dot(centered, centered);
    vec2 warped;
    warped.x = centered.x * (1.0 + WarpX * r2);
    warped.y = centered.y * (1.0 + WarpY * r2);

    // --- Apply additional per-axis curvature (squash/stretch) ---
    warped.x = warped.x * (1.0 + CurvatureX * (centered.y * centered.y));
    warped.y = warped.y * (1.0 + CurvatureY * (centered.x * centered.x));

    // Convert back to [0, 1] texture coordinates
    vec2 finalUV = warped * 0.5 + 0.5;

    // Optional: Clamp or discard outside bounds
    if (finalUV.x < 0.0 || finalUV.x > 1.0 || finalUV.y < 0.0 || finalUV.y > 1.0)
        discard;

    FragColor = texture(Texture, finalUV);
}
