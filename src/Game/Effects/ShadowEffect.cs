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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Effects;

public sealed class ShadowEffect : OrthographicEffect
{
    private EffectParameter _lightPositionParam;
    private EffectParameter _screenSizeParam;
    private EffectParameter _spriteSizeParam;
    private EffectParameter _spriteCenterParam;
    public ShadowEffect(GraphicsDevice device)
        : base(device, Shaders.ShadowEffect)
    {
        CacheEffectParameters();
    }

    public ShadowEffect(ShadowEffect cloneSource)
        : base(cloneSource)
    {
        CacheEffectParameters();
    }

    public Vector2 LightPosition
    {
        get => _lightPositionParam.GetValueVector2();
        set => _lightPositionParam.SetValue(value);
    }

    public Vector2 ScreenSize
    {
        get => _screenSizeParam.GetValueVector2();
        set => _screenSizeParam.SetValue(value);
    }

    public Vector2 SpriteSize
    {
        get => _spriteSizeParam.GetValueVector2();
        set => _spriteSizeParam.SetValue(value);
    }

    public Vector2 SpriteCenter
    {
        get => _spriteCenterParam.GetValueVector2();
        set => _spriteCenterParam.SetValue(value);
    }

    public override Effect Clone()
        => new ShadowEffect(this);

    [MemberNotNull(nameof(_lightPositionParam), nameof(_screenSizeParam), nameof(_spriteSizeParam), nameof(_spriteCenterParam))]
    private void CacheEffectParameters()
    {
        _lightPositionParam = Parameters[nameof(LightPosition)];
        _screenSizeParam = Parameters[nameof(ScreenSize)];
        _spriteSizeParam = Parameters[nameof(SpriteSize)];
        _spriteCenterParam = Parameters[nameof(SpriteCenter)];
    }
}
