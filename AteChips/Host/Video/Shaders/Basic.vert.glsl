#version 330 core

// Vertex attributes from the VBO
layout(location = 0) in vec2 aPosition;   // screen-space position
layout(location = 1) in vec2 aTexCoord;   // texture coordinate

// Output to fragment shader
out vec2 vTexCoord;

void main()
{
	// Output position to clip space (no transformation needed for full-screen quad)
	gl_Position = vec4(aPosition, 0.0, 1.0);

	// Pass texture coordinate to the fragment shader
	vTexCoord = aTexCoord;
}