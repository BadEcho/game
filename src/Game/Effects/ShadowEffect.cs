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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides an effect that will cast a shadow based on the position and direction of a light source.
/// </summary>
public sealed class ShadowEffect : OrthographicEffect
{
    private EffectParameter _lightPositionParam;
    private EffectParameter _shadowOriginParam;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowEffect"/> class.
    /// </summary>
    /// <param name="device">The graphics device used for rendering.</param>
    public ShadowEffect(GraphicsDevice device)
        : base(device, Shaders.ShadowEffect)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowEffect"/> class.
    /// </summary>
    /// <param name="cloneSource">The <see cref="ShadowEffect"/> instance to clone.</param>
    private ShadowEffect(ShadowEffect cloneSource)
        : base(cloneSource)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Gets or sets the position of a light source.
    /// </summary>
    public Vector2 LightPosition
    {
        get => _lightPositionParam.GetValueVector2();
        set => _lightPositionParam.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the center of the shadow's rotation.
    /// </summary>
    public Vector2 ShadowOrigin
    {
        get => _shadowOriginParam.GetValueVector2();
        set => _shadowOriginParam.SetValue(value);
    }

    /// <inheritdoc/>
    public override Effect Clone()
        => new ShadowEffect(this);

    [MemberNotNull(nameof(_lightPositionParam), nameof(_shadowOriginParam))]
    private void CacheEffectParameters()
    {
        _lightPositionParam = Parameters[nameof(LightPosition)];
        _shadowOriginParam = Parameters[nameof(ShadowOrigin)];
    }
}
