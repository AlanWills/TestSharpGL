#version 330

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec3 colour;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

out vec3 fragColour;
out vec3 fragPos; 
out vec3 fragNormal;

void main()
{
    gl_Position = projection * view * model * vec4(position, 1.0f);
	fragColour = colour;
	fragPos = vec3(model * vec4(position, 1.0f));
	fragNormal = mat3(transpose(inverse(model))) * normal;
}