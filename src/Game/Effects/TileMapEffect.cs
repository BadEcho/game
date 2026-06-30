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
    private EffectParameter _worldViewProjectionParam;
   
    private Viewport _lastViewport;
    private bool _matrixDirty;

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

    /// <summary>
    /// Gets or sets the world view projection matrix used by the effect.
    /// </summary>
    private Matrix WorldViewProjection
    { get; set; }

    /// <summary>
    /// Gets or sets the world matrix.
    /// </summary>
    public Matrix World
    {
        get;
        set
        {
            _matrixDirty = field != value;
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the view matrix.
    /// </summary>
    public Matrix View
    {
        get;
        set
        {
            _matrixDirty = field != value;
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the projection matrix.
    /// </summary>
    public Matrix Projection
    {
        get;
        set
        {
            _matrixDirty = field != value;
            field = value;
        }
    }

    /// <inheritdoc/>
    public override Effect Clone()
        => new TileMapEffect(this);

    /// <inheritdoc/>
    protected override void OnApply()
    {
        base.OnApply();

        Viewport viewport = GraphicsDevice.Viewport;

        bool parametersChanged =
            viewport.Width != _lastViewport.Width || viewport.Height != _lastViewport.Height || _matrixDirty;

        if (parametersChanged)
        {
            WorldViewProjection = World * View * Projection;

            _lastViewport = viewport;
            _matrixDirty = false;
        }

        _worldViewProjectionParam.SetValue(WorldViewProjection);
    }

    [MemberNotNull(nameof(_textureParam), nameof(_worldViewProjectionParam))]
    private void CacheEffectParameters()
    {
        _textureParam = Parameters[nameof(Texture)];
        _worldViewProjectionParam = Parameters[nameof(WorldViewProjection)];
    }
}
