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

BEGIN_PARAMETERS
    float LightBrightness _ps(c0) _cb(c0);
    float LightSharpness _ps(c1) _cb(c1);
END_PARAMETERS

float4 PointLightPixelShader(VSOutput input) : COLOR
{
    float distance = length(input.TexCoord - 0.5);
    float range = 5;

    float falloff = saturate(0.5 - distance) * (LightBrightness * range + 1);
    falloff = pow(abs(falloff), LightSharpness * range + 1);

    float4 color = input.Color;
    color.a = falloff;

    return color;
}

technique
{
    pass
    {
        PixelShader = compile PS_MODEL PointLightPixelShader();
    }
}