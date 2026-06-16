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

public sealed class ShadowHullEffect : OrthographicEffect
{
    private EffectParameter _lightPositionParam;
    private EffectParameter _screenSizeParam;
    private EffectParameter _shadowFadeStartParam;
    private EffectParameter _shadowFadeEndParam;

    public ShadowHullEffect(GraphicsDevice device)
        : base(device, Shaders.ShadowHullEffect)
    {
        CacheEffectParameters();
    }

    public ShadowHullEffect(ShadowHullEffect cloneSource)
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

    public float ShadowFadeStart
    {
        get => _shadowFadeStartParam.GetValueSingle();
        set => _shadowFadeStartParam.SetValue(value);
    }

    public float ShadowFadeEnd
    {
        get => _shadowFadeStartParam.GetValueSingle();
        set => _shadowFadeEndParam.SetValue(value);
    }

    public override Effect Clone()
        => new ShadowHullEffect(this);

    [MemberNotNull(nameof(_lightPositionParam), nameof(_screenSizeParam), nameof(_shadowFadeEndParam), nameof(_shadowFadeStartParam))]
    private void CacheEffectParameters()
    {
        _lightPositionParam = Parameters[nameof(LightPosition)];
        _screenSizeParam = Parameters[nameof(ScreenSize)];
        _shadowFadeEndParam = Parameters[nameof(ShadowFadeEnd)];
        _shadowFadeStartParam = Parameters[nameof(ShadowFadeStart)];
    }
}
