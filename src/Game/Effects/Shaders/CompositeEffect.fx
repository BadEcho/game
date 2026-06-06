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
END_PARAMETERS

float4 CompositePixelShader(VSOutput input) : COLOR
{
    float4 color = sample2D(SpriteTexture, input.TexCoord) * input.Color;
    float4 light = sample2D(LightBuffer, input.TexCoord) * input.Color;

    light = saturate(light + AmbientLight);

    return color * light;
}

technique
{
    pass
    {
        PixelShader = compile PS_MODEL CompositePixelShader();        
    }
}