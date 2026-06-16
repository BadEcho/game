// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2026 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

#include "Defines.fxh"

declare_texture(SpriteTexture, 0);
sampler2D SpriteTextureSampler : register(s0) = sampler_state
{
    Texture = <SpriteTexture>;
};

declare_texture(LightBuffer, 1);
sampler2D LightBufferSampler : register(s1) = sampler_state
{
    Texture = <LightBuffer>;
};

BEGIN_PARAMETERS
    float AmbientLight _ps(c0) _cb(c0);
    float2 ScreenSize _ps(c1) _cb(c1);
    float BoxBlurStride _ps(c2) _cb(c2);
END_PARAMETERS


float4 Blur(float2 texCoord)
{
    float4 color = float4(0, 0, 0, 0);

    float2 texelSize = 1 / ScreenSize;
    int kernalSize = 1;
    float stride = BoxBlurStride * 30; // allow the stride to range up a size of 30
    for (int x = -kernalSize; x <= kernalSize; x++)
    {
        for (int y = -kernalSize; y <= kernalSize; y++)
        {
            float2 offset = float2(x, y) * texelSize * stride;
            color += sample2D(LightBuffer, texCoord + offset);
        }
    }

    int totalSamples = pow(kernalSize*2+1, 2);
    color /= totalSamples;
    color.a = 1;
    return color;
}

float4 CompositePS(VSOutput input) : COLOR
{
    float4 color = sample2D(SpriteTexture, input.TexCoord) * input.Color;
    float4 light = 
    //sample2D(LightBuffer, input.TexCoord) * input.Color;
     Blur( input.TexCoord) * input.Color;

    float3 toneMapped = light.xyz / (.5 + dot(light.xyz, float3(0.299, 0.587, 0.114)));
    light.xyz = toneMapped;
    
    light = saturate(light + AmbientLight);

    return color * light;
}

technique
{
    pass
    {
        PixelShader = compile PS_MODEL CompositePS();        
    }
}