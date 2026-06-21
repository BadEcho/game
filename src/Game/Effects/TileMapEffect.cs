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

public sealed class TileMapEffect : Effect, ITextureEffect
{
    private EffectParameter _textureParam;
    private EffectParameter _worldViewProjectionParam;
    private EffectParameter _alphaParam;

    public TileMapEffect(GraphicsDevice device)
        : base(device, Shaders.TileMapEffect)
    {
        CacheEffectParameters();
    }

    public Texture2D Texture
    {
        get => _textureParam.GetValueTexture2D();
        set => _textureParam.SetValue(value);
    }

    public Matrix WorldViewProjection
    {
        get => _worldViewProjectionParam.GetValueMatrix();
        set => _worldViewProjectionParam.SetValue(value);
    }

    public Matrix? MatrixTransform { get; set; }

    public float Alpha
    {
        get => _alphaParam.GetValueSingle();
        set => _alphaParam.SetValue(value);
    }

    [MemberNotNull(nameof(_textureParam), nameof(_worldViewProjectionParam), nameof(_alphaParam))]
    private void CacheEffectParameters()
    {
        _textureParam = Parameters[nameof(Texture)];
        _worldViewProjectionParam = Parameters[nameof(WorldViewProjection)];
        _alphaParam = Parameters[nameof(Alpha)];
    }
}
