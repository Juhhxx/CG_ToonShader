#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;

uniform mat4        MatrixClip;
uniform mat4        MatrixWorld;

out vec3 fragPos;
out vec3 fragNormal;

void main()
{
    vec3 clipPos = (MatrixClip * vec4(position, 1.0)).xyz;
    vec3 clipNorm = (MatrixClip * vec4(normal, 0.0)).xyz;

    clipPos += normalize(clipNorm) * 0.2;

    gl_Position = vec4(clipPos, 1.0);
}
