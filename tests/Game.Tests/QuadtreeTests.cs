// -----------------------------------------------------------------------
// <copyright>
//		Created by Matt Weber <matt@badecho.com>
//		Copyright @ 2024 Bad Echo LLC. All rights reserved.
//
//		Bad Echo Technologies are licensed under a
//		GNU Affero General Public License v3.0.
//
//		See accompanying file LICENSE.md or a copy at:
//		https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------
using Xunit;

namespace BadEcho.Game.Tests;

public class QuadtreeTests
{
    private const int LARGE_BUCKET_CAPACITY = 32;
    private const int LARGE_MAX_DEPTH = 5;

    private readonly Quadtree<SpatialStub> _quadtree;
    private readonly SpatialStub _upperLeft;
    private readonly SpatialStub _upperRight;
    private readonly SpatialStub _bottomRight;

    private readonly Quadtree<SpatialStub> _largeQuadtree;
    private readonly List<SpatialStub> _largeQuadtreeItems;

    public QuadtreeTests()
    {
        _quadtree = new Quadtree<SpatialStub>(new RectangleF(0, 0, 100, 100), 3, 3);
        _upperLeft = new SpatialStub(20, 20, 10, 10);
        _upperRight = new SpatialStub(80, 20, 10, 10);
        _bottomRight = new SpatialStub(80, 80, 10, 10);

        _quadtree.Insert(_upperLeft);
        _quadtree.Insert(_upperRight);
        _quadtree.Insert(_bottomRight);

        _largeQuadtree 
            = new Quadtree<SpatialStub>(new RectangleF(0, 0, 10000, 10000), LARGE_BUCKET_CAPACITY, LARGE_MAX_DEPTH);

        _largeQuadtreeItems = [];
        
        for (int i = 0; i < 400; i++)
        {
            var x = i * 24;
            var y = i * 24;

            var stub = new SpatialStub(x, y, 7, 7);

            _largeQuadtreeItems.Add(stub);
            _largeQuadtree.Insert(stub);
        }
    }

    [Fact]
    public void GetChildren_Small_ReturnsValid()
    {
        var children = _quadtree.GetElements();

        Assert.Collection(children,
                          t => Assert.Equal(_upperLeft, t),
                          t => Assert.Equal(_upperRight, t),
                          t => Assert.Equal(_bottomRight, t));
    }

    [Fact]
    public void GetChildren_Large_ReturnsValidCount()
    {
        var children = _largeQuadtree.GetElements();

        Assert.Equal(400, children.Count());
    }

    [Fact]
    public void FindCollisions_TopLeftColliderSmall_ReturnsUpperLeft()
    {
        var collider = new SpatialStub(10, 10, 20, 20);

        var collisions = _quadtree.FindCollisions(collider);

        Assert.Collection(collisions, t => Assert.Equal(_upperLeft, t));
    }

    [Fact]
    public void FindCollisions_TopRightColliderSmall_ReturnsUpperRight()
    {
        var collider = new SpatialStub(70, 10, 20, 20);

        var collisions = _quadtree.FindCollisions(collider);

        Assert.Collection(collisions, t => Assert.Equal(_upperRight, t));
    }

    [Fact]
    public void FindCollisions_BottomRightColliderSmall_ReturnsBottomRight()
    {
        var collider = new SpatialStub(70, 70, 20, 20);

        var collisions = _quadtree.FindCollisions(collider);

        Assert.Collection(collisions, t => Assert.Equal(_bottomRight, t));
    }

    [Fact]
    public void Insert_SingleIntoSmall_CausesSplit()
    {
        Assert.True(_quadtree.IsLeaf);

        var item = new SpatialStub(10, 10, 20, 20);

        _quadtree.Insert(item);

        Assert.False(_quadtree.IsLeaf);
    }
    
    [Fact]
    public void Remove_Large_Merges()
    {
        Assert.False(_largeQuadtree.IsLeaf);

        foreach (var stub in _largeQuadtreeItems.Take(_largeQuadtreeItems.Count - LARGE_BUCKET_CAPACITY))
        {
            _largeQuadtree.Remove(stub);
        }

        Assert.True(_largeQuadtree.IsLeaf);
    }

    [Fact]
    public void Remove_ElementMovedAcrossQuadrants_RemovesElement()
    {
        var mover = new SpatialStub(20, 20, 10, 10);
        _quadtree.Insert(mover);

        Assert.False(_quadtree.IsLeaf);

        mover.Bounds = new RectangleF(60, 60, 10, 10);

        Assert.True(_quadtree.Remove(mover));
        Assert.DoesNotContain(mover, _quadtree.GetElements());
        Assert.Equal(3, _quadtree.CountElements());
    }

    [Fact]
    public void Insert_ElementAlreadyPresent_RelocatesWithoutDuplicating()
    {
        var mover = new SpatialStub(20, 20, 10, 10);
        _quadtree.Insert(mover);

        mover.Bounds = new RectangleF(60, 60, 10, 10);
        _quadtree.Insert(mover);

        Assert.Single(_quadtree.GetElements(), e => ReferenceEquals(e, mover));
        Assert.Equal(4, _quadtree.CountElements());
    }

    [Fact]
    public void Remove_ElementsMovedInLargeTree_RemovesAllAndMerges()
    {
        Assert.False(_largeQuadtree.IsLeaf);

        foreach (var stub in _largeQuadtreeItems)
        {   // Shift each element by roughly half the tree so it no longer sits where it was inserted.
            stub.Bounds = new RectangleF((stub.Bounds.Center.X + 5000) % 9990, stub.Bounds.Center.Y, 7, 7);

            Assert.True(_largeQuadtree.Remove(stub));
        }

        Assert.True(_largeQuadtree.IsLeaf);
        Assert.Equal(0, _largeQuadtree.CountElements());
    }

    private sealed class SpatialStub : ISpatial
    {
        public SpatialStub(float x, float y, float width, float height)
        {
            Bounds = new RectangleF(x, y, width, height);
        }

        public IShape Bounds { get; set; }
    }
}
