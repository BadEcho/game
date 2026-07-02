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
    float4x4 WorldViewProjection;
END_PARAMETERS

struct TileMapPSOutput
{
    float4 Color: SV_Target0;
    float4 Ignore: SV_Target1;
};

struct TileMapVSInput
{
    float4 Position: POSITION;    
    float2 TexCoord: TEXCOORD0;
};

struct TileMapVSOutput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

TileMapPSOutput TileMapPS(TileMapVSOutput input)
{
    TileMapPSOutput output;

    output.Color = sample2D(Texture, input.TexCoord);
    /*
    We need to use a pixel shader with two color outputs and with the second output zeroed out, otherwise 
    (either due to a bug in MonoGame or a quirk of OpenGL) the single color output will not only get assigned 
    to the first render target, but any additional render targets as well when using OpenGL. 
    The second render target is typically used for effects such as normal mapping: something we don't use with
    our tile maps (we just want to render the tiles, nothing more).
    */
    output.Ignore = float4(0,0,0,0);

    return output;
}

TileMapVSOutput TileMapVS(TileMapVSInput input)
{
    TileMapVSOutput output;

    output.Position = mul(input.Position, WorldViewProjection);
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