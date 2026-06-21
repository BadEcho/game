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

declare_texture(Texture, 0);
sampler2D TextureSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
};

BEGIN_PARAMETERS
    float4x4 WorldViewProjection _vs(c0) _cb(c0);
    float Alpha _vs(c4) _cb(c4);
END_PARAMETERS

struct TileMapPSOutput
{
    float4 Color: SV_Target0;
    float4 Ignore: SV_Target1;
};

TileMapPSOutput TileMapPS(VSOutput input)
{
    TileMapPSOutput output;

    output.Color = sample2D(Texture, input.TexCoord);// * input.Color;
    output.Ignore = float4(0,0,0,0);

    return output;
}

VSOutput TileMapVS(VSInput input)
{
    VSOutput output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.Color.a *= Alpha;
    output.Color.rgb *= Alpha;
    output.TexCoord = input.TexCoord;

    return output;
}

technique
{
    pass
    {
        VertexShader = compile VS_MODEL TileMapVS();
        PixelShader = compile PS_MODEL TileMapPS();
    }
}