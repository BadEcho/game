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

using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides an effect that emits light from a point against a normal map.
/// </summary>
public sealed class PointLightEffect : OrthographicEffect
{
    private EffectParameter _lightBrightnessParam;
    private EffectParameter _lightSharpnessParam;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PointLightEffect"/> class.
    /// </summary>
    /// <param name="device">The graphics device used for rendering.</param>
    public PointLightEffect(GraphicsDevice device) 
        : base(device, Shaders.PointLightEffect)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointLightEffect"/> class.
    /// </summary>
    /// <param name="cloneSource">The <see cref="PointLightEffect"/> instance to clone.</param>
    private PointLightEffect(PointLightEffect cloneSource) 
        : base(cloneSource)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Gets or sets the brightness of the light; has no bearing on the effective distance of the light.
    /// </summary>
    public float LightBrightness
    {
        get => _lightBrightnessParam.GetValueSingle();
        set => _lightBrightnessParam.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the sharpness of the light's falloff.
    /// </summary>
    public float LightSharpness
    {
        get => _lightSharpnessParam.GetValueSingle();
        set => _lightSharpnessParam.SetValue(value);
    }

    /// <inheritdoc/>
    public override Effect Clone()
        => new PointLightEffect(this);

    [MemberNotNull(nameof(_lightBrightnessParam),
                   nameof(_lightSharpnessParam))]
    private void CacheEffectParameters()
    {
        _lightBrightnessParam = Parameters[nameof(LightBrightness)];
        _lightSharpnessParam = Parameters[nameof(LightSharpness)];
    }
}
