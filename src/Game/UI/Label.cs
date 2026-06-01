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

using BadEcho.Game.Fonts;
using BadEcho.Game.Properties;
using BadEcho.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.UI;

/// <summary>
/// Provides a text label user interface element.
/// </summary>
public sealed class Label : Control<Label>
{
    private IModelRenderer? _textRenderer;

    /// <summary>
    /// Gets or sets the font used for this label's text.
    /// </summary>
    public DistanceFieldFont? Font
    {
        get;
        set => RemeasureIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets or sets the color of the font used for this label's text.
    /// </summary>
    public Color FontColor
    { get; set; }

    /// <summary>
    /// Gets or sets the size of the font in points used for this label's text.
    /// </summary>
    public float FontSize
    { get; set; }

    /// <summary>
    /// Gets or sets the text contents of this label.
    /// </summary>
    public string Text
    {
        get;
        set => RemeasureIfChanged(ref field, value);
    } = string.Empty;

    /// <inheritdoc />
    protected override Size MeasureCore(Size availableSize)
    {
        if (Font == null)
            return Size.Empty;
        
        _textRenderer = Font.AddModel(Text, Vector2.Zero, FontColor, FontSize / 0.767f);

        return (Size) _textRenderer.Size;
    }

    /// <inheritdoc />
    protected override void DrawCore(SpriteBatch spriteBatch)
    { }

    /// <inheritdoc/>
    protected override void DrawPrimitivesCore(IStandardEffect? effect)
    {
        if (_textRenderer == null)
        {
            Logger.Debug(Strings.LabelNoFont);
            return;
        }

        Matrix textTranslation = Matrix.CreateTranslation(ContentBounds.X, ContentBounds.Y, 0);
        float alpha = 1.0f;

        if (effect != null)
        {
            textTranslation *= effect.MatrixTransform ?? Matrix.Identity;
            alpha = effect.Alpha;
        }

        _textRenderer.Draw(textTranslation, alpha);
    }
}