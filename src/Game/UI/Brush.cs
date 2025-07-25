﻿// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2025 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.UI;

/// <summary>
/// Provides a brush that will paint a solid color on the screen when drawn.
/// </summary>
public sealed class Brush : IVisual, IDisposable
{
    private Texture2D? _palette;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Brush"/> class.
    /// </summary>
    /// <param name="color">The color of the brush.</param>
    public Brush(Color color)
    {
        Color = color;
    }

    /// <inheritdoc />
    public void Draw(ConfiguredSpriteBatch spriteBatch, Rectangle targetArea)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));

        if (_palette == null)
        {
            _palette = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);

            _palette.SetData([Color.White]);
        }
        
        spriteBatch.Draw(_palette, targetArea, Color);
    }

    /// <summary>
    /// Gets or sets the color of this brush.
    /// </summary>
    public Color Color
    { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _palette?.Dispose();

        _disposed = true;
    }
}
