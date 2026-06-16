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

// Bayer 4x4 values normalized
static const float bayer4x4[16] = {
    0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
   12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
    3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
   15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
};

BEGIN_PARAMETERS
    float4x4 MatrixTransform _vs(c0) _cb(c0);
    float2 ScreenSize _vs(c4) _ps(c4) _cb(c4);
    float2 LightPosition _vs(c5) _cb(c5);
    float ShadowFadeStart _ps(c6) _cb(c6);
    float ShadowFadeEnd _ps(c7) _cb(c7);
END_PARAMETERS

float2 UnpackVector2FromColor_SNorm(float4 color)  
{  
    // Convert [0,1] to byte range [0,255]  
    float4 bytes = color * 255.0;  
  
    // Reconstruct 16-bit unsigned ints (x and y)  
    float xInt = bytes.r * 256.0 + bytes.g;  
    float yInt = bytes.b * 256.0 + bytes.a;  
  
    // Convert from unsigned to signed short range [-32768, 32767]  
    if (xInt >= 32768.0) xInt -= 65536.0;  
    if (yInt >= 32768.0) yInt -= 65536.0;  
  
    // Convert from signed 16-bit to float in [-1, 1]  
    float x = xInt / 32767.0;  
    float y = yInt / 32767.0;  
  
    return float2(x, y);  
}

VSOutput ShadowHullVS(VSInput input)   
{     
    VSInput modified = input;  
    float distance = ScreenSize.x + ScreenSize.y;  
    float2 pos = input.Position.xy;  
      
    float2 P = pos - (.5 * input.TexCoord) / ScreenSize;  
    float2 A = P;  
      
    float2 aToB = UnpackVector2FromColor_SNorm(input.Color) * ScreenSize;  
    float2 B = A + aToB;  

    float2 direction = normalize(aToB);
    A -= direction;
    B += direction;

    float2 normal = float2(-direction.y, direction.x);  
    float alignment = dot(normal, (LightPosition - A));  
    if (alignment < 0){  
        modified.Color.a = -1;  
    }
      
    float2 lightRayA = normalize(A - LightPosition);  
    float2 a = A + distance * lightRayA;  
    float2 lightRayB = normalize(B - LightPosition);  
    float2 b = B + distance * lightRayB;      
      
    int id = input.TexCoord.x + input.TexCoord.y * 2;  
    if (id == 0) {        // S --> A  
       pos = A;  
    } else if (id == 1) { // D --> a  
       pos = a;  
    } else if (id == 3) { // F --> b  
       pos = b;  
    } else if (id == 2) { // G --> B  
       pos = B;  
    }  
      
    modified.Position.xy = pos;  
    VSOutput output; 
    
    output.Position = mul(modified.Position, MatrixTransform);
    output.Color = modified.Color;
    output.TexCoord = modified.TexCoord;

    return output;  
}

float4 ShadowHullPS(VSOutput input) : COLOR
{
    int2 pixel = int2(input.TexCoord * ScreenSize);
    int idx = (pixel.x % 4) + (pixel.y % 4) * 4;
    float ditherValue = bayer4x4[idx];

    // produce the fade-out gradient
    float maxDistance = ScreenSize.x + ScreenSize.y;
    float endDistance = ShadowFadeEnd;
    float startDistance = ShadowFadeStart;
    float fade = saturate((input.TexCoord.x - endDistance) / (startDistance - endDistance));

    if (ditherValue > fade) {
        clip(-1);
    }



    clip(input.Color.a);
    return float4(0,0,0,1);
}

technique
{
    pass P0
    {
        PixelShader = compile PS_MODEL ShadowHullPS();
        VertexShader = compile VS_MODEL ShadowHullVS();
    }
}