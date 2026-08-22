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
using Xunit;

namespace BadEcho.Game.Tests;

public class TransitionPointTests
{
    [Fact]
    public void Constructor_EmptyTargetAreaName_ThrowsException()
        => Assert.Throws<ArgumentException>(() => new TransitionPoint(string.Empty, new PointF(8, 8)));

    [Fact]
    public void Constructor_Coordinate_ReturnsDefaults()
    {
        var transitionPoint = new TransitionPoint("Cave", new PointF(8, 8));

        Assert.Equal(string.Empty, transitionPoint.Name);
        Assert.Equal(string.Empty, transitionPoint.TargetPointName);
        Assert.True(transitionPoint.IsEnabled);
        Assert.False(transitionPoint.IsRegion);
        Assert.Equal(RectangleF.Empty, transitionPoint.Bounds);
    }

    [Fact]
    public void Constructor_Region_ReturnsBoundsLocation()
    {
        var transitionPoint = new TransitionPoint("Cave", new RectangleF(16, 32, 8, 8));

        Assert.True(transitionPoint.IsRegion);
        Assert.Equal(new PointF(16, 32), transitionPoint.Location);
    }

    [Fact]
    public void SpawnPosition_Coordinate_ReturnsLocation()
    {
        var transitionPoint = new TransitionPoint("Cave", new PointF(8, 12));

        Assert.Equal(new PointF(8, 12), transitionPoint.SpawnPosition);
    }

    [Fact]
    public void SpawnPosition_Region_ReturnsCenter()
    {
        var transitionPoint = new TransitionPoint("Cave", new RectangleF(16, 32, 8, 16));

        Assert.Equal(new PointF(20, 40), transitionPoint.SpawnPosition);
    }

    [Fact]
    public void IsEntered_CoordinateInsideEntity_ReturnsTrue()
    {
        var transitionPoint = new TransitionPoint("Cave", new PointF(10, 10));

        Assert.True(transitionPoint.IsEntered(new RectangleF(0, 0, 20, 20)));
    }

    [Fact]
    public void IsEntered_CoordinateOutsideEntity_ReturnsFalse()
    {
        var transitionPoint = new TransitionPoint("Cave", new PointF(30, 30));

        Assert.False(transitionPoint.IsEntered(new RectangleF(0, 0, 20, 20)));
    }

    [Fact]
    public void IsEntered_CoordinateOnEntityEdge_ReturnsFalse()
    {   // Rectangles are endpoint-exclusive, so an entity's right and bottom edges lie outside of it.
        var transitionPoint = new TransitionPoint("Cave", new PointF(10, 10));

        Assert.False(transitionPoint.IsEntered(new RectangleF(0, 0, 10, 10)));
    }

    [Fact]
    public void IsEntered_CoordinateWithZeroSizedEntity_ReturnsFalse()
    {   // An entity occupying no space contains nothing at all, its own coordinates included.
        var transitionPoint = new TransitionPoint("Cave", new PointF(10, 10));

        Assert.False(transitionPoint.IsEntered(new RectangleF(10, 10, 0, 0)));
    }

    [Fact]
    public void IsEntered_RegionOverlappingEntity_ReturnsTrue()
    {
        var transitionPoint = new TransitionPoint("Cave", new RectangleF(0, 0, 10, 10));

        Assert.True(transitionPoint.IsEntered(new RectangleF(5, 5, 10, 10)));
    }

    [Fact]
    public void IsEntered_RegionSeparateFromEntity_ReturnsFalse()
    {
        var transitionPoint = new TransitionPoint("Cave", new RectangleF(0, 0, 10, 10));

        Assert.False(transitionPoint.IsEntered(new RectangleF(20, 20, 5, 5)));
    }

    [Fact]
    public void IsEntered_RegionTouchingEntity_ReturnsFalse()
    {
        var transitionPoint = new TransitionPoint("Cave", new RectangleF(0, 0, 10, 10));

        Assert.False(transitionPoint.IsEntered(new RectangleF(10, 0, 10, 10)));
    }

    [Fact]
    public void IsEntered_Disabled_ReturnsFalse()
    {
        var transitionPoint = new TransitionPoint("Cave", new RectangleF(0, 0, 10, 10))
                              {
                                  IsEnabled = false
                              };

        Assert.False(transitionPoint.IsEntered(new RectangleF(5, 5, 10, 10)));
    }

    [Fact]
    public void IsEntered_DisabledThenEnabled_ReturnsTrue()
    {
        var transitionPoint = new TransitionPoint("Cave", new RectangleF(0, 0, 10, 10))
                              {
                                  IsEnabled = false
                              };

        Assert.False(transitionPoint.IsEntered(new RectangleF(5, 5, 10, 10)));

        transitionPoint.IsEnabled = true;

        Assert.True(transitionPoint.IsEntered(new RectangleF(5, 5, 10, 10)));
    }

    [Fact]
    public void IsEntered_Null_ThrowsException()
    {
        var transitionPoint = new TransitionPoint("Cave", new PointF(10, 10));

        Assert.Throws<ArgumentNullException>(() => transitionPoint.IsEntered(null!));
    }
}
