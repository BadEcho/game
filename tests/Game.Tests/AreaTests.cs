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

using BadEcho.Game.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Xunit;

namespace BadEcho.Game.Tests;

public class AreaTests : IClassFixture<ContentManagerFixture>
{
    private readonly ContentManager _content;

    public AreaTests(ContentManagerFixture contentFixture)
        => _content = contentFixture.Content;

    [Fact]
    public void Load_Simple_ReturnsValid()
    {
        Area area = _content.Load<Area>("Areas\\Simple");

        Assert.NotNull(area);
        Assert.Equal("Simple Area", area.Name);
        Assert.NotNull(area.TileMap);
        Assert.NotEmpty(area.Actors);
        Assert.Collection(area.Actors,
                          a1 =>
                          {
                              Assert.Equal("Images\\StickMan_2", a1.Texture.Name);
                              Assert.Equal(new Vector2(16, 16), a1.Position);
                          });

    }
}
