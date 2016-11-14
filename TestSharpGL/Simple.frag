#version 330

in vec3 fragColour;
out vec4 outputColor;

void main()
{
	outputColor = vec4(fragColour, 1.0f);
}