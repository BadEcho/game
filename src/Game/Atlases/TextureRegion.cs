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

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BadEcho.Game.Atlases;

/// <summary>
/// Provides an image-bounding region of a texture atlas, able to be drawn independently from other images
/// found in the atlas.
/// </summary>
public class TextureRegion : IVisualRegion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureRegion"/> class.
    /// </summary>
    /// <param name="atlasTexture">The texture of the atlas that contains this region.</param>
    /// <param name="sourceArea">
    /// The bounding rectangle of the region in <paramref name="atlasTexture"/> that will be rendered when drawing this.
    /// </param>
    public TextureRegion(Texture2D atlasTexture, Rectangle sourceArea)
    {
        Require.NotNull(atlasTexture, nameof(atlasTexture));

        AtlasTexture = atlasTexture;
        SourceArea = sourceArea;
    }

    /// <summary>
    /// Gets the texture of the atlas that contains this region.
    /// </summary>
    public Texture2D AtlasTexture
    { get; }

    /// <summary>
    /// Gets the bounding rectangle of the region in <see cref="AtlasTexture"/> that will be rendered when drawing this.
    /// </summary>
    public Rectangle SourceArea
    { get; }

    /// <inheritdoc />
    public Size Size
        => SourceArea.Size;

    /// <inheritdoc/>
    public virtual void Draw(ConfiguredSpriteBatch spriteBatch, Rectangle targetArea)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));

        spriteBatch.Draw(AtlasTexture, targetArea, SourceArea, Color.White);
    }
}
