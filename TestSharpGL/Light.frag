#version 330

in vec3 fragColour;
in vec3 fragPos;
in vec3 fragNormal;
out vec4 outputColor;

uniform vec3 lightPos;

void main()
{
	vec3 lightColour = vec3(1.0f, 1.0f, 1.0f);

	float ambientStrength = 0.5f;
    vec3 ambient = ambientStrength * lightColour;

	vec3 norm = normalize(fragNormal);
	vec3 lightDir = normalize(lightPos - fragPos);

	float diff = max(dot(norm, lightDir), 0.0);
		
	vec3 diffuse = diff * lightColour;

	vec3 result = (ambient + diffuse) * fragColour;
	outputColor = vec4(result, 1.0f);
}