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
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace BadEcho.Game.Tests;

public class AreaTests : IClassFixture<ContentManagerFixture>
{
    private readonly ContentManager _content;
    private readonly GraphicsDevice _device;

    public AreaTests(ContentManagerFixture contentFixture)
    {
        _content = contentFixture.Content;
        _device = contentFixture.Device;
    }

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

    [Fact]
    public void Load_NoActors_ReturnsNoTransitionPoints()
    {
        Area area = _content.Load<Area>("Areas\\NoActors");

        Assert.Empty(area.TransitionPoints);
    }

    [Fact]
    public void Copy_Transitions_SharesTransitionPoints()
    {
        Area area = _content.Load<Area>("Areas\\Transitions");
        var copy = new CopiedArea(area);

        Assert.Equal(area.TransitionPoints.Count, copy.TransitionPoints.Count);
        // The copy is shallow, exactly as it is for actors and lights, so the two areas see each other's toggles.
        Assert.Same(area.TransitionPoints.First(), copy.TransitionPoints.First());
    }

    [Fact]
    public void Load_Transitions_FoldsClassedObjectsOnly()
    {
        Area area = _content.Load<Area>("Areas\\Transitions");

        // Five objects survive the content build; the one that isn't classed as a transition point is not folded.
        Assert.Equal(5, area.TileMap.Layers.OfType<ObjectLayer>().Single().Objects.Count);
        Assert.Equal(4, area.TransitionPoints.Count);
        Assert.DoesNotContain(area.TransitionPoints, t => "Signpost".Equals(t.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Load_Transitions_FoldsRegionWithWiredDestination()
    {
        TransitionPoint northDoor = FindTransitionPoint("NorthDoor");

        Assert.True(northDoor.IsRegion);
        Assert.True(northDoor.IsEnabled);
        Assert.Equal("Cave", northDoor.TargetAreaName);
        Assert.Equal("SouthDoor", northDoor.TargetPointName);
        // The object layer's (8, 4) offset is reflected in the folded point's coordinates.
        Assert.Equal(new RectangleF(24, 4, 16, 8), northDoor.Bounds);
    }

    [Fact]
    public void Load_Transitions_FoldsCoordinateWithoutWiredDestination()
    {
        TransitionPoint shrinePortal = FindTransitionPoint("ShrinePortal");

        Assert.False(shrinePortal.IsRegion);
        Assert.Equal("Shrine", shrinePortal.TargetAreaName);
        Assert.Equal(string.Empty, shrinePortal.TargetPointName);
        Assert.Equal(new PointF(16, 28), shrinePortal.SpawnPosition);
    }

    [Fact]
    public void Load_Transitions_FoldsDisabledPoint()
    {
        TransitionPoint sealedDoor = FindTransitionPoint("SealedDoor");

        Assert.False(sealedDoor.IsEnabled);
        Assert.Equal("Crypt", sealedDoor.TargetAreaName);
    }

    [Fact]
    public void FindEnteredTransitionPoint_SkipsDisabledPoint()
    {
        Area area = CreateEmptyArea();
        var sealedDoor = new TransitionPoint("Crypt", new RectangleF(0, 0, 16, 16)) { IsEnabled = false };

        area.AddTransitionPoint(sealedDoor);

        Assert.Null(area.FindEnteredTransitionPoint(new RectangleF(4, 4, 8, 8)));

        sealedDoor.IsEnabled = true;

        Assert.Same(sealedDoor, area.FindEnteredTransitionPoint(new RectangleF(4, 4, 8, 8)));
    }

    [Fact]
    public void FindEnteredTransitionPoint_OverlappingPoints_ReturnsFirstMatch()
    {
        Area area = CreateEmptyArea();
        var first = new TransitionPoint("Cave", new RectangleF(0, 0, 16, 16));
        var second = new TransitionPoint("Crypt", new RectangleF(0, 0, 16, 16));

        area.AddTransitionPoint(first);
        area.AddTransitionPoint(second);

        Assert.Same(first, area.FindEnteredTransitionPoint(new RectangleF(4, 4, 8, 8)));
    }

    [Fact]
    public void AddTransitionPoint_Null_ThrowsException()
        => Assert.Throws<ArgumentNullException>(() => CreateEmptyArea().AddTransitionPoint(null!));

    [Fact]
    public void RemoveTransitionPoint_Null_ThrowsException()
        => Assert.Throws<ArgumentNullException>(() => CreateEmptyArea().RemoveTransitionPoint(null!));

    [Fact]
    public void FindEnteredTransitionPoint_Null_ThrowsException()
        => Assert.Throws<ArgumentNullException>(() => CreateEmptyArea().FindEnteredTransitionPoint(null!));

    [Fact]
    public void AddRemoveTransitionPoint_AddedPoint_IsDetectedThenGone()
    {
        Area area = CreateEmptyArea();
        var portal = new TransitionPoint("Cave", new RectangleF(0, 0, 16, 16)) { Name = "Portal" };

        area.AddTransitionPoint(portal);

        Assert.Same(portal, area.FindEnteredTransitionPoint(new RectangleF(4, 4, 8, 8)));

        area.RemoveTransitionPoint(portal);

        Assert.Empty(area.TransitionPoints);
        Assert.Null(area.FindEnteredTransitionPoint(new RectangleF(4, 4, 8, 8)));
    }

    private Area CreateEmptyArea()
        => new(_device, new TileMap(_device, "Empty", new Size(2, 2), new Size(16, 16), new CustomProperties()));

    private TransitionPoint FindTransitionPoint(string name)
    {
        Area area = _content.Load<Area>("Areas\\Transitions");

        return area.TransitionPoints.Single(t => name.Equals(t.Name, StringComparison.Ordinal));
    }

    private sealed class CopiedArea : Area
    {
        public CopiedArea(Area source)
            : base(source)
        { }
    }
}
