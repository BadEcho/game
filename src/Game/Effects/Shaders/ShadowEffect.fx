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
    float4x4 MatrixTransform;
    float2 LightPosition;
END_PARAMETERS

float4 ShadowPS(VSOutput input) : SV_Target
{   
    float alpha = sample2D(SpriteTexture, input.TexCoord).a;

    // If the pixel is fully transparent, then there is nothing here to block the light and produce a shadow, so we clip.
    clip(alpha == 0.0f ? -1 : 1);

    // This shader is meant to be drawn into a stencil buffer. 
    // The color is irrelevant, all that matters in regards to the stencil test is whether the RGBA is a non-zero value or not.
    return float4(0, 0, 0, alpha);    
}

VSOutput ShadowVS(VSInput input)
{
    VSOutput output;
    
    // The x-coordinate for the shadow origin is encoded in the R and G channels of the input color; the y-coordinate is in the B and A channels.
    float2 shadowOrigin;
    shadowOrigin.x = round(input.Color.r * 255.0) * 256.0 + round(input.Color.g * 255.0);
    shadowOrigin.y = round(input.Color.b * 255.0) * 256.0 + round(input.Color.a * 255.0);

    // Find the vector pointing from our light to our current vertex.
    float2 lightVector = input.Position.xy - LightPosition;

    // Ensure the light vector is not a zero vector (which would happen if the vertex is exactly on the light) in order to prevent a divide-by-zero error with normalize().
    if (any(lightVector))
    {   // This is the unit vector that will point in the direction the shadow will be cast.
        float2 castDir = normalize(lightVector);

        // Get coordinates in world space relative to the sprite's center. This is needed in order to pivot the shadow around the desired center.
        float2 localOffset = input.Position.xy - shadowOrigin;

        // We use the shadow cast direction and the following unit vector to define a rotated coordinate frame aligned to the light.        
        // The shadow cast direction rotated 90 degrees counter-clockwise. This basically gives us the direction of the shadow's width.
        float2 perpendicularAxis = float2(-castDir.y, castDir.x);

        // Rotate the sprite around the desired center so both its local y-axis aligns with the shadow cast direction and its local x-axis aligns with the direction shadow's width.
        input.Position.xy = shadowOrigin + perpendicularAxis * localOffset.x - castDir * localOffset.y;
    }

    output.Position = mul(input.Position, MatrixTransform);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    
    return output;
}

technique SpriteBatch
{
    pass
    {
        VertexShader = compile VS_MODEL ShadowVS();
        PixelShader = compile PS_MODEL ShadowPS();
    }
};