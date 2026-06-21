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

// Common definitions for targeting various platforms.
#ifndef DEFINES
#define DEFINES

#if OPENGL
	#define _vs(r)  : register(vs, r)
	#define _ps(r)  : register(ps, r)
	#define _cb(r)
	#define SV_POSITION POSITION
	#define VS_MODEL vs_3_0	
	#define PS_MODEL ps_3_0
	
	#define BEGIN_PARAMETERS
	#define END_PARAMETERS

	#define sample2D(texture, texCoord) tex2D(texture##Sampler, texCoord)
	#define declare_texture(name, index) \
		sampler2D name : register(s##index);
#elif VULKAN
	#define _vs(r)
	#define _ps(r)
	#define _cb(r)
	#define VS_MODEL vs_6_0
	#define PS_MODEL ps_6_0

	#define BEGIN_PARAMETERS    cbuffer Parameters : register(b0) {
	#define END_PARAMETERS      };

	#define sample2D(texture, texCoord) texture.Sample(texture##Sampler, texCoord)
	#define sampler2D SamplerState
	#define declare_texture(name, index) \
		Texture2D<float4> name : register(t##index);
#else
	#define _vs(r)
	#define _ps(r)
	#define _cb(r)
	#define VS_MODEL vs_4_0_level_9_3
	#define PS_MODEL ps_4_0_level_9_3
	
	#define BEGIN_PARAMETERS    cbuffer Parameters : register(b0) {
	#define END_PARAMETERS      };

	#define sample2D(texture, texCoord) texture.Sample(texture##Sampler, texCoord)
	#define sampler2D SamplerState
	#define declare_texture(name, index) \
		Texture2D<float4> name : register(t##index); 	
#endif

struct VSInput 
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_Position;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

#endif