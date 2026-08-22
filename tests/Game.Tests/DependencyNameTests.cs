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

using BadEcho.Game.Pipeline;
using Xunit;

namespace BadEcho.Game.Tests;

public class DependencyNameTests
{
    [Theory]
    // The expressions the pipeline's own call sites pass.
    [InlineData("input.Asset.TileMapPath", "tileMapPath")]
    [InlineData("actor.SpriteSheetPath", "spriteSheetPath")]
    [InlineData("asset.TexturePath", "texturePath")]
    [InlineData("asset.NormalMapPath", "normalMapPath")]
    [InlineData("input.Asset.FontPath", "fontPath")]
    // Tiled's attributes are already lowercase, and its nesting is discarded along with everything else.
    [InlineData("tileSet.Source", "source")]
    [InlineData("asset.Image.Source", "source")]
    [InlineData("imageLayer.Image.Source", "source")]
    // Shapes the call sites do not currently use, but which must not produce nonsense.
    [InlineData("TexturePath", "texturePath")]
    [InlineData("asset.TexturePath!", "texturePath")]
    [InlineData("asset.Actors[0].SpriteSheetPath", "spriteSheetPath")]
    public void FromExpression_ReturnsAuthoredName(string expression, string expected)
        => Assert.Equal(expected, DependencyName.FromExpression(expression));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromExpression_NothingToName_ReturnsInput(string? expression)
        => Assert.Equal(expression, DependencyName.FromExpression(expression));
}
