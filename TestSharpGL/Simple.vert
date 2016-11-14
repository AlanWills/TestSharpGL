#version 330

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec3 colour;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

out vec3 fragColour;

void main()
{
    gl_Position = model * vec4(position, 1.0f);
	fragColour = colour;
}