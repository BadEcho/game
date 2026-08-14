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

using BadEcho.Game.Tiles;
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
                              Assert.Equal("Images\\StickMan_0", a1.Texture.Name);
                              Assert.Equal(new Vector2(16, 16), a1.Position);
                          });

    }

    [Fact]
    public void Load_Simple_ReturnsValidTileMap()
    {
        Area area = _content.Load<Area>("Areas\\Simple");

        Assert.Equal(new SizeF(2, 2), area.TileMap.Size);
        Assert.Equal(new Size(16, 16), area.TileMap.TileSize);
        Assert.Single(area.TileMap.TileSets);
        Assert.Single(area.TileMap.Layers.OfType<TileLayer>());
    }

    [Fact]
    public void Load_Simple_ReturnsSizeInPixels()
    {
        Area area = _content.Load<Area>("Areas\\Simple");

        // A two-by-two map of sixteen pixel tiles.
        Assert.Equal(new SizeF(32, 32), area.Size);
    }

    [Fact]
    public void Load_Simple_ActorIsAnimatedSprite()
    {
        Area area = _content.Load<Area>("Areas\\Simple");

        Sprite actor = Assert.Single(area.Actors);

        Assert.IsType<AnimatedSprite>(actor);
        // The sprite sheet is four frames across a 64x32 texture, giving the actor a 16x32 frame.
        Assert.Equal(new SizeF(16, 32), actor.Bounds.Size);
    }

    [Fact]
    public void Load_Simple_ActorColliderTracksActor()
    {
        Area area = _content.Load<Area>("Areas\\Simple");

        Sprite actor = Assert.Single(area.Actors);

        // Adding an actor to an area registers its collider, which sources its bounds from the actor itself.
        Assert.Same(actor, actor.Collider.Entity);
        Assert.Equal(new PointF(24, 32), actor.Collider.Bounds.Center);
    }

    [Fact]
    public void Load_Simple_ReturnsUnlitAreaWithFullAmbience()
    {
        Area area = _content.Load<Area>("Areas\\Simple");

        Assert.Empty(area.Lights);
        Assert.Equal(1.0f, area.AmbientLight);
    }

    [Fact]
    public void Load_NoActors_ReturnsNoActors()
    {
        Area area = _content.Load<Area>("Areas\\NoActors");

        Assert.Equal("No Actors Area", area.Name);
        Assert.NotNull(area.TileMap);
        Assert.Empty(area.Actors);
    }

    [Fact]
    public void Load_MultipleActors_ReturnsAllActors()
    {
        Area area = _content.Load<Area>("Areas\\MultipleActors");

        Assert.Equal("Multiple Actors Area", area.Name);
        Assert.Collection(area.Actors,
                          a1 => Assert.Equal(new Vector2(16, 32), a1.Position),
                          a2 => Assert.Equal(new Vector2(64, 128), a2.Position));
    }

    [Fact]
    public void Load_SharedSpriteSheet_ReturnsBothActors()
    {
        Area area = _content.Load<Area>("Areas\\SharedSpriteSheet");

        Assert.Equal("Shared Sprite Sheet Area", area.Name);
        Assert.Collection(area.Actors,
                          a1 => Assert.Equal(new Vector2(16, 16), a1.Position),
                          a2 => Assert.Equal(new Vector2(32, 48), a2.Position));

        Assert.Same(area.Actors.First().Texture, area.Actors.Last().Texture);
    }
}
