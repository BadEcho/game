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

declare_texture(NormalBuffer, 1);
sampler2D NormalBufferSampler : register(s1) = sampler_state
{
	Texture = <NormalBuffer>;
};

BEGIN_PARAMETERS
    float4x4 MatrixTransform;
    float Alpha;
END_PARAMETERS

// Fully transparent pixels are clipped so they aren't factored into any active stencil buffers.
#define SetColorPSOutput \
    color = sample2D(SpriteTexture, input.TexCoord) * input.Color; \
	clip(color.a - .01);

struct ColorNormalStandardPSOutput
{
	float4 Color: SV_Target0;
    float4 Normal: SV_Target1;
};

ColorNormalStandardPSOutput ColorNormalStandardPS(VSOutput input)
{
    float4 color;
	ColorNormalStandardPSOutput output;

    SetColorPSOutput;
    
    output.Color = color;
    output.Normal = sample2D(NormalBuffer, input.TexCoord);
    
    return output;
}

float4 ColorStandardPS(VSOutput input) : SV_Target
{
    float4 color;
    
    SetColorPSOutput;

    return color;
}

VSOutput StandardVS(VSInput input)
{
    VSOutput output;
    
    output.Position = mul(input.Position, MatrixTransform);
    output.Color = input.Color;
    // We don't want existing alpha data overwritten, so we just multiply the current value by the parameter.
    output.Color.a *= Alpha;
    output.Color.rgb *= Alpha;    
    output.TexCoord = input.TexCoord;
    
    return output;
}

technique ColorStandard
{
    pass
    {
        VertexShader = compile VS_MODEL StandardVS();
        PixelShader = compile PS_MODEL ColorStandardPS();
    }
};

technique ColorNormalStandard
{
    pass
    {
        VertexShader = compile VS_MODEL StandardVS();
        PixelShader = compile PS_MODEL ColorNormalStandardPS();
    }
};