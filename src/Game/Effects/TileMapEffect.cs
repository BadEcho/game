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
/// Provides an effect that renders a tile map.
/// </summary>
public sealed class TileMapEffect : Effect, ITextureEffect
{
    private EffectParameter _textureParam;
    private EffectParameter _matrixTransformParam;

    /// <summary>
    /// Initializes a new instance of the <see cref="TileMapEffect"/> class.
    /// </summary>
    /// <param name="device">The graphics device used for sprite rendering.</param>
    public TileMapEffect(GraphicsDevice device)
        : base(device, Shaders.TileMapEffect)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TileMapEffect"/> class.
    /// </summary>
    /// <param name="cloneSource">The <see cref="TileMapEffect"/> instance to clone.</param>
    private TileMapEffect(TileMapEffect cloneSource)
        : base (cloneSource)
    {
        CacheEffectParameters();
    }

    /// <inheritdoc/>
    public Texture2D Texture
    {
        get => _textureParam.GetValueTexture2D();
        set => _textureParam.SetValue(value);
    }

    /// <inheritdoc/>
    public Matrix? MatrixTransform
    {
        get => _matrixTransformParam.GetValueMatrix();
        set => _matrixTransformParam.SetValue(value ?? Matrix.Identity);
    }

    /// <inheritdoc/>
    public override Effect Clone()
        => new TileMapEffect(this);

    [MemberNotNull(nameof(_textureParam), nameof(_matrixTransformParam))]
    private void CacheEffectParameters()
    {
        _textureParam = Parameters[nameof(Texture)];
        _matrixTransformParam = Parameters[nameof(MatrixTransform)];
    }
}
