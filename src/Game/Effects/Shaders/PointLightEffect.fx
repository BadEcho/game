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

declare_texture(NormalBuffer, 0);
sampler2D NormalBufferSampler : register(s0) = sampler_state
{
	Texture = <NormalBuffer>;
};

BEGIN_PARAMETERS
    float LightBrightness _ps(c0) _cb(c0);
    float LightSharpness _ps(c1) _cb(c1);	
	float4x4 MatrixTransform _vs(c2) _cb(c2);
END_PARAMETERS

struct PointLightVSOutput
{
	float4 Position : SV_POSITION;
    float4 Color: COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 ScreenData : TEXCOORD1;
};

PointLightVSOutput PointLightVS(VSInput input)
{
    PointLightVSOutput output;

    output.Position = mul(input.Position, MatrixTransform);
    output.Color = input.Color;        
    output.TexCoord = input.TexCoord;

    /*
    The values in the normal buffer are relative to the entire screen. To find the normal value
    for pixels being shaded in the pixel shader, we'll need to be able to figure out where the
    light pixel is on screen.

    Pixel shaders cannot read from position semantics (i.e., the Position member in this shader's output struct),
    so we stuff the screen data into an extra TEXCOORD field in the vertex shader.
    */
    output.ScreenData.xy = output.Position.xy;
    
    // Store w as the z-coordinate so we can use it to perform a perspective divide inside the pixel shader (required since 
    // it's being received in a POSITION semantic).
    output.ScreenData.z = output.Position.w;

    return output;
}

float4 PointLightPS(PointLightVSOutput input) : COLOR
{
    float distance = length(input.TexCoord - 0.5);
    float range = 5;

    float falloff = saturate(0.5 - distance) * (LightBrightness * range + 1);
    falloff = pow(abs(falloff), LightSharpness * range + 1);

    float4 color = input.Color;
    color.a *= falloff;

    // Perspective divide.
    input.ScreenData /= input.ScreenData.z;

    // Take our clip-space coordinates and convert them to uv coordinate space for the screen.
    float2 screenCoords = .5 * (input.ScreenData.xy + 1);
    screenCoords.y = 1 - screenCoords.y;

    //float shadow = sample2D(ShadowBuffer,screenCoords).r;

    //color.a *= shadow;

    float4 normal = sample2D(NormalBuffer,screenCoords);    
    
    // If the normal is transparent, then no normal values were mapped to the position on the screen this pixel occupies,
    // so we will simply return the light as is.
    if (normal.a == 0)
        return color;

    normal.y = 1 - normal.y;

    // Convert from (0,1) to (-1,1). This is our normal vector.
    float3 normalDir = (normal.xyz-.5)*2;

    // Get the direction the light is travelling at the current pixel.
    float3 lightDir = float3(normalize(.5 - input.TexCoord), 1);

    // Calculate the degree to which the normal and light vectors are pointing in the same direction.
    float lightAmount = (dot(normalDir, lightDir));

    color.a *= lightAmount;// * shadow;
    //color.a = falloff;

    return color;
}

technique
{
    pass
    {
        PixelShader = compile PS_MODEL PointLightPS();
        VertexShader = compile VS_MODEL PointLightVS();
    }
}