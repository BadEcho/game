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
    float AmbientLight;
    float2 ScreenSize;
    float BoxBlurStride;
END_PARAMETERS

float4 Blur(float2 texCoord)
{   // A simple box blur.
    float4 color = float4(0, 0, 0, 0);

    float2 pixelSize = 1 / ScreenSize;
    int kernelSize = 1;
    float stride = BoxBlurStride * 30; // allow the stride to range up a size of 30
    
    // A 3x3 box blur, we'll set the current pixel's color to the  average value of the neighboring pixels.
    for (int x = -kernelSize; x <= kernelSize; x++)
    {
        for (int y = -kernelSize; y <= kernelSize; y++)
        {
            float2 offset = float2(x, y) * pixelSize * stride;
            color += sample2D(LightBuffer, texCoord + offset);
        }
    }

    int totalSamples = (kernelSize * 2 + 1) * (kernelSize * 2 + 1); // ...or 9 :)

    color /= totalSamples;
    color.a = 1;

    return color;
}

float4 CompositePS(VSOutput input) : SV_Target
{
    float4 color = sample2D(SpriteTexture, input.TexCoord) * input.Color;
    float4 light = Blur(input.TexCoord) * input.Color;

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