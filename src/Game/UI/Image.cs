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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.UI;

/// <summary>
/// Provides a control that displays an image.
/// </summary>
public sealed class Image : Control<Image>
{
    /// <summary>
    /// Gets or sets the visual component data composing the image.
    /// </summary>
    public IVisualRegion? Visual
    {
        get;
        set => RemeasureIfChanged(ref field, value);
    }

    /// <inheritdoc />
    protected override Size MeasureCore(Size availableSize)
    {
        if (Visual == null)
            return Size.Empty;

        int width = Visual.Size.Width;
        int height = Visual.Size.Height;

        if (width == 0 || height == 0)
            return new Size(width, height);

        if (Width != null && Height == null)
            height = (int)MathF.Round((float)Width.Value * height / width);

        else if (Height != null && Width == null)
            width = (int)MathF.Round((float)Height.Value * width / height);

        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override void DrawCore(SpriteBatch spriteBatch) 
        => Visual?.Draw(spriteBatch, ContentBounds);

    /// <inheritdoc/>
    protected override void DrawPrimitivesCore(Matrix? transform, float alpha)
    { }
}
