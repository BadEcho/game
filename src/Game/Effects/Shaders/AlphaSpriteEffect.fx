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
    float4x4 MatrixTransform _vs(c0) _cb(c0);
    float Alpha _vs(c4) _cb(c4);
END_PARAMETERS

VSOutput AlphaSpriteVertexShader(VSInput input)
{
    VSOutput output;
    
    output.Position = mul(input.Position, MatrixTransform);
    output.Color = input.Color;
    output.Color.a *= Alpha;
    output.Color.rgb *= Alpha;    
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 AlphaSpritePixelShader(VSOutput input) : SV_Target0
{
    return sample2D(SpriteTexture, input.TexCoord) * input.Color;
}

technique SpriteBatch
{
    pass
    {
        VertexShader = compile VS_MODEL AlphaSpriteVertexShader();
        PixelShader = compile PS_MODEL AlphaSpritePixelShader();
    }
};