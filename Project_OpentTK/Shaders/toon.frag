#version 330 core

uniform vec4    MaterialColor = vec4(1,1,0,1);
uniform vec2    MaterialSpecular = vec2(0,1);
uniform vec4    MaterialColorEmissive = vec4(0,0,0,1);
uniform vec4    EnvColor;
uniform vec3    ViewPos;

uniform sampler2D   TextureBaseColor;
uniform sampler2D   TextureNormalMap;

uniform bool        HasTextureBaseColor;
uniform bool        HasTextureNormalMap;

const int MAX_LIGHTS = 8;
struct Light
{
    int     type;
    vec3    position;
    vec3    direction;
    vec4    color;
    float   intensity;
    vec2    spot;
    float   range;
};
uniform int     LightCount;
uniform Light   Lights[MAX_LIGHTS];

float saturate(float v)
{
    return clamp(v, 0, 1);
}

float ComputeAttenuation(Light light, vec3 worldPos)
{
    float d = length(worldPos - light.position) / light.range;

    return saturate(saturate(5 * (1 - d)) / (1 + 25 * d * d));
}

const int ToonColorLevels = 4;
const int ToonColorLevelsSpec = 2;

void AddToonEffect(inout float value, int toonFactor)
{
    // Restrain the given value into the number of factors given by toonFactor
    value = floor(value * toonFactor) / toonFactor;
}

vec3 ComputeDirectional(Light light, vec3 worldPos, vec3 worldNormal, vec4 materialColor)
{
    float d = clamp(-dot(worldNormal, light.direction), 0, 1);
    AddToonEffect( d, ToonColorLevels);

    vec3  v = normalize(ViewPos - worldPos);
    vec3  h =  normalize(v - light.direction);
    float spec = pow(max(dot(h, worldNormal), 0), MaterialSpecular.y);
    AddToonEffect( spec, ToonColorLevelsSpec);
    float s = MaterialSpecular.x * spec;

    return clamp(d * materialColor.xyz + s, 0, 1) * light.color.rgb * light.intensity;
}

vec3 ComputePoint(Light light, vec3 worldPos, vec3 worldNormal, vec4 materialColor)
{
    vec3  lightDir = normalize(worldPos - light.position);
    float d = -dot(worldNormal, lightDir);
    AddToonEffect( d, ToonColorLevels);
    
    vec3  v = normalize(ViewPos - worldPos);
    vec3  h =  normalize(v - lightDir);
    float spec = pow(max(dot(h, worldNormal), 0), MaterialSpecular.y);
    AddToonEffect( spec, ToonColorLevelsSpec);
    float s = MaterialSpecular.x * spec;

    return clamp(d * materialColor.xyz + s, 0, 1) * light.color.rgb * light.intensity * ComputeAttenuation(light, worldPos);
}

vec3 ComputeSpot(Light light, vec3 worldPos, vec3 worldNormal, vec4 materialColor)
{
    vec3  lightDir = normalize(worldPos - light.position);
    float d = clamp(-dot(worldNormal, lightDir), 0, 1);

    float spot = (acos(dot(lightDir, light.direction)) - light.spot.x) / (light.spot.y - light.spot.x);
    d = d * mix(1, 0, clamp(spot, 0, 1));
    AddToonEffect( d, ToonColorLevels);

    vec3  v = normalize(ViewPos - worldPos);
    vec3  h =  normalize(v - lightDir);
    float spec = pow(max(dot(h, worldNormal), 0), MaterialSpecular.y);
    AddToonEffect( spec, ToonColorLevelsSpec);
    float s = MaterialSpecular.x * spec;
    
    return clamp(d * materialColor.xyz + s, 0, 1) * light.color.rgb * light.intensity * ComputeAttenuation(light, worldPos);
}

vec3 ComputeLight(Light light, vec3 worldPos, vec3 worldNormal, vec4 materialColor)
{
    if (light.type == 0)
    {
        return ComputeDirectional(light, worldPos, worldNormal, materialColor);
    }
    else if (light.type == 1)
    {
        return ComputePoint(light, worldPos, worldNormal, materialColor);
    }
    else if (light.type == 2)
    {
        return ComputeSpot(light, worldPos, worldNormal, materialColor);
    }
}

in vec3 fragPos;
in vec3 fragNormal;
in vec4 fragTangent;
in vec2 fragUV;

out vec4 OutputColor;

void main()
{
    vec3 worldPos = fragPos.xyz;
    vec3 worldNormal;
    if (HasTextureNormalMap)
    {
        vec3 n = normalize(fragNormal);
        vec3 t = normalize(fragTangent.xyz);

        // Create tangent space
        vec3 binormal = cross(n, t) * fragTangent.w;
        mat3 TBN = mat3(t, binormal, n);
        vec3 normalMap = texture(TextureNormalMap, fragUV).xyz * 2 - 1;

        worldNormal = TBN * normalMap;
    }
    else
    {
        worldNormal = normalize(fragNormal.xyz);
    }

    // Compute material color
    vec4 matColor = MaterialColor;
    if (HasTextureBaseColor) matColor *= texture(TextureBaseColor, fragUV);

    // Ambient component - get data from 4th mipmap of the cubemap (effective hardware blur)
    vec3 envLighting = EnvColor.xyz * MaterialColor.xyz;
    // Emissive component
    vec3 emissiveLighting = MaterialColorEmissive.rgb;

    // Direct light
    vec3 directLight = vec3(0,0,0);
    for (int i = 0; i < LightCount; i++)
    {
        directLight += ComputeLight(Lights[i], worldPos, worldNormal, matColor);
    }    

    // Add all lighting components
    OutputColor = vec4(envLighting + emissiveLighting + directLight, 1);
}
