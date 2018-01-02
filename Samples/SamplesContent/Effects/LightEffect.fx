#include "Macros.fxh"

matrix WorldViewProjection;
float2 LightPosition;
float  LightRadius;
float  ShadowMapTexelSize;

Texture2D Texture;
sampler2D TextureSampler = sampler_state { Texture = <Texture>; };

Texture2D ShadowMapU;
Texture2D ShadowMapR;
Texture2D ShadowMapD;
Texture2D ShadowMapL;
sampler2D ShadowMapUSampler = sampler_state
{ 
    Texture = <ShadowMapU>;
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler2D ShadowMapRSampler = sampler_state 
{
    Texture = <ShadowMapR>;
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler2D ShadowMapDSampler = sampler_state 
{ 
    Texture = <ShadowMapD>;
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler2D ShadowMapLSampler = sampler_state 
{ 
    Texture = <ShadowMapL>;
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};


struct LightVertexShaderInput
{
	float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct LightVertexShaderOutput
{
	float4 Position : POSITION0;
    float2 PositionWS : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float4 Color : COLOR0;
};

LightVertexShaderOutput LightVS(in LightVertexShaderInput input)
{
    LightVertexShaderOutput output = (LightVertexShaderOutput) 0;

	output.Position = mul(input.Position, WorldViewProjection);
    output.PositionWS = input.Position.xy;
	output.Color = input.Color;
    output.TexCoord = input.TexCoord;

	return output;
}

float4 LightPS(LightVertexShaderOutput input) : COLOR
{
    float4 texColor = tex2D(TextureSampler, input.TexCoord) * input.Color;
    
    float2 lightDirection = input.PositionWS - LightPosition;
    float2 lightDirectionAbs = abs(lightDirection);
    float2 mapCoord = lightDirection.xy / lightDirection.yx;
    mapCoord = (mapCoord * 0.5) + 0.5;
     
    float normalizedShadowDistance = 0;
    float normalizedLightDistance = 1;

	float v = 0.0;

    if (lightDirection.y >= lightDirectionAbs.x) //up
    {
        normalizedShadowDistance = tex2D(ShadowMapUSampler, float2(mapCoord.x, v)).r;
        normalizedLightDistance = lightDirectionAbs.y / LightRadius;
    }
    else if (lightDirection.x >= lightDirectionAbs.y) //right
    {
        normalizedShadowDistance = tex2D(ShadowMapRSampler, float2(1 - mapCoord.y, v)).r;
        normalizedLightDistance = lightDirectionAbs.x / LightRadius;
    }
    else if (lightDirection.y <= -lightDirectionAbs.x) //down
    {
        normalizedShadowDistance = tex2D(ShadowMapDSampler, float2(mapCoord.x, v)).r;
        normalizedLightDistance = lightDirectionAbs.y / LightRadius;
    }
    else // if (lightDirection.x <= -lightDirectionAbs.y) //left
    {
        normalizedShadowDistance = tex2D(ShadowMapLSampler, float2(1 - mapCoord.y, v)).r;
        normalizedLightDistance = lightDirectionAbs.x / LightRadius;
    }
        
    float lightIndensity = lightIndensity = clamp(1 - (normalizedLightDistance), v, 1);
    if ((normalizedShadowDistance - lightIndensity) > 0)
    {
        lightIndensity = 0;
    }
    else
    {
        float normalizedLightDistance2 = length(lightDirection) / LightRadius;
        lightIndensity = clamp(1 - (normalizedLightDistance2), 0, 1);
    }
    
    return texColor * lightIndensity;
}

TECHNIQUE(LightDrawing, LightVS, LightPS);
