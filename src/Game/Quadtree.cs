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

using System.Diagnostics.CodeAnalysis;
using BadEcho.Game.Properties;

namespace BadEcho.Game;

/// <summary>
/// Provides a tree data structure in which each internal (non-leaf) node has exactly four children, useful
/// for 2D spatial information queries.
/// </summary>
/// <typeparam name="T">The type of <see cref="ISpatial"/> data in the quadtree.</typeparam>
public sealed class Quadtree<T>
    where T : ISpatial
{
    private readonly List<T> _elements = [];
    private readonly Dictionary<T, Quadtree<T>> _locations;
    private readonly Quadtree<T>? _parent;

    private readonly int _bucketCapacity;
    private readonly int _maxDepth;

    private Quadtree<T>? _topLeft, _topRight, _bottomLeft, _bottomRight;

    /// <summary>
    /// Initializes a new instance of the <see cref="Quadtree{T}"/> class.
    /// </summary>
    /// <param name="bounds">The bounding rectangle of the region that this quadtree's contents occupy.</param>
    /// <remarks>
    /// This initializes the <see cref="Quadtree{T}"/> instance with a default bucket capacity of <c>32</c> and a default maximum
    /// node depth of <c>5</c>, which are reasonable general-purpose values for these settings.
    /// </remarks>
    public Quadtree(RectangleF bounds)
        : this(bounds, 32, 5)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Quadtree{T}"/> class.
    /// </summary>
    /// <param name="bounds">The bounding rectangle of the region that this quadtree's contents occupy.</param>
    /// <param name="bucketCapacity">The maximum number of items allowed in this node before it gets quartered.</param>
    /// <param name="maxDepth">The maximum number of levels of child nodes allowed across the entire quad tree.</param>
    public Quadtree(RectangleF bounds, int bucketCapacity, int maxDepth)
        : this(bounds, bucketCapacity, maxDepth, null)
    { }

    private Quadtree(RectangleF bounds, int bucketCapacity, int maxDepth, Quadtree<T>? parent)
    {
        _bucketCapacity = bucketCapacity;
        _maxDepth = maxDepth;
        _parent = parent;
        // All nodes in a tree share the root's record of which node each element is stored in.
        _locations = parent?._locations ?? new Dictionary<T, Quadtree<T>>();

        Bounds = bounds;
    }

    /// <summary>
    /// Gets the bounding rectangle of the region that this quadtree's contents occupy.
    /// </summary>
    public RectangleF Bounds
    { get; }

    /// <summary>
    /// Gets the level of this node within the overall quadtree; that is, the number of edges on the path from this node to the
    /// root node.
    /// </summary>
    public int Level
    { get; init; }

    /// <summary>
    /// Gets a value indicating if this node is a terminal node for the tree (i.e., it has no children).
    /// </summary>
    /// <remarks>
    /// The majority of spatial elements reside in leaf nodes, the only exceptions being elements whose bounds overlap multiple
    /// split node boundaries.
    /// </remarks>
    [MemberNotNullWhen(false, nameof(_topLeft), nameof(_topRight), nameof(_bottomLeft), nameof(_bottomRight))]
    public bool IsLeaf
        => _topLeft == null || _topRight == null || _bottomLeft == null || _bottomRight == null;

    /// <summary>
    /// Gets or sets a value if elements not fully contained with this quadtree's bounds are allowed.
    /// </summary>
    public bool AllowOutOfBounds
    { get; set; }

    /// <summary>
    /// Inserts the specified spatial element into the quadtree at the appropriate node, removing it from elsewhere in the tree
    /// if already present.
    /// </summary>
    /// <param name="element">The spatial element to insert into the quadtree.</param>
    public void Insert(T element)
    {
        Require.NotNull(element, nameof(element));

        if (!(Bounds.Contains(element.Bounds) || AllowOutOfBounds))
            throw new ArgumentException(Strings.ElementOutsideQuadtreeBounds, nameof(element));

        if (_locations.ContainsKey(element))
            Remove(element);

        Add(element);
    }

    /// <summary>
    /// Removes the specified spatial element from the quadtree and whichever node it's been assigned to.
    /// </summary>
    /// <param name="element">The spatial element to remove from the quadtree.</param>
    /// <returns>True if <c>element</c> is successfully removed; otherwise, false.</returns>
    public bool Remove(T element)
    {
        Require.NotNull(element, nameof(element));

        if (!_locations.Remove(element, out Quadtree<T>? node))
            return false;

        node._elements.Remove(element);

        // Any node on the path back to the root whose total descendant element count no longer exceeds the bucket capacity
        // is returned to a non-split state.
        for (Quadtree<T>? current = node; current != null; current = current._parent)
        {
            if (!current.IsLeaf && current.CountElements() <= _bucketCapacity)
                current.Merge();
        }

        return true;
    }

    /// <summary>
    /// Looks for and returns all spatial elements that exist within this node and its children whose bounds collide with
    /// the specified spatial element.
    /// </summary>
    /// <param name="element">The spatial element to find collisions for.</param>
    /// <returns>All spatial elements that collide with <c>element</c>.</returns>
    public IEnumerable<T> FindCollisions(T element)
    {
        Require.NotNull(element, nameof(element));

        var nodes = new Queue<Quadtree<T>>();
        var collisions = new List<T>();

        nodes.Enqueue(this);

        while (nodes.Count > 0)
        {
            var node = nodes.Dequeue();

            if (!element.Bounds.Intersects(node.Bounds))
                continue;

            collisions.AddRange(node._elements.Where(e => !ReferenceEquals(e, element) && e.Bounds.Intersects(element.Bounds)));

            if (!node.IsLeaf)
            {
                if (element.Bounds.Intersects(node._topLeft.Bounds))
                    nodes.Enqueue(node._topLeft);

                if (element.Bounds.Intersects(node._topRight.Bounds))
                    nodes.Enqueue(node._topRight);

                if (element.Bounds.Intersects(node._bottomLeft.Bounds))
                    nodes.Enqueue(node._bottomLeft);

                if (element.Bounds.Intersects(node._bottomRight.Bounds))
                    nodes.Enqueue(node._bottomRight);
            }
        }

        return collisions;
    }

    /// <summary>
    /// Gets the total number of elements belonging to this and all descending nodes.
    /// </summary>
    /// <returns>The total number of elements belong to this and all descending nodes.</returns>
    public int CountElements()
    {
        int count = _elements.Count;

        if (!IsLeaf)
        {
            count += _topLeft.CountElements();
            count += _topRight.CountElements();
            count += _bottomLeft.CountElements();
            count += _bottomRight.CountElements();
        }

        return count;
    }

    /// <summary>
    /// Retrieves the elements belonging to this and all descendant nodes.
    /// </summary>
    /// <returns>A sequence of the elements belonging to this and all descendant nodes.</returns>
    public IEnumerable<T> GetElements()
    {
        var children = new List<T>();
        var nodes = new Queue<Quadtree<T>>();

        nodes.Enqueue(this);

        while (nodes.Count > 0)
        {
            var node = nodes.Dequeue();

            if (!node.IsLeaf)
            {
                nodes.Enqueue(node._topLeft);
                nodes.Enqueue(node._topRight);
                nodes.Enqueue(node._bottomLeft);
                nodes.Enqueue(node._bottomRight);
            }

            children.AddRange(node._elements);
        }

        return children;
    }

    private void Add(T element)
    {   // A node exceeding its allotted number of items will get split (if it hasn't been already) into four equal quadrants.
        if (_elements.Count >= _bucketCapacity)
            Split();

        Quadtree<T>? containingChild = GetContainingChild(element.Bounds);

        if (containingChild != null)
        {
            containingChild.Add(element);
        }
        else
        {   // If no child was returned, then this is either a leaf node, or the element's boundaries overlap multiple children.
            // Either way, the element gets assigned to this node.
            _elements.Add(element);
            _locations[element] = this;
        }
    }

    private void Split()
    {   // If we're not a leaf node, then we're already split.
        if (!IsLeaf)
            return;

        // Splitting is only allowed if it doesn't cause us to exceed our maximum depth.
        if (Level + 1 > _maxDepth)
            return;

        _topLeft = CreateChild(Bounds.Location);
        _topRight = CreateChild(new PointF(Bounds.Center.X, Bounds.Location.Y));
        _bottomLeft = CreateChild(new PointF(Bounds.Location.X, Bounds.Center.Y));
        _bottomRight = CreateChild(new PointF(Bounds.Center.X, Bounds.Center.Y));

        var elements = _elements.ToList();

        foreach (var element in elements)
        {
            Quadtree<T>? containingChild = GetContainingChild(element.Bounds);
            // An element is only moved if it completely fits into a child quadrant.
            if (containingChild != null)
            {
                _elements.Remove(element);

                containingChild.Add(element);
            }
        }
    }

    private Quadtree<T> CreateChild(PointF location)
        => new(new RectangleF(location, Bounds.Size / 2), _bucketCapacity, _maxDepth, this)
        {
            Level = Level + 1
        };

    private void Merge()
    {   // If we're a leaf node, then there is nothing to merge.
        if (IsLeaf)
            return;

        // Children are merged first so that elements from every descendant level are reclaimed.
        _topLeft.Merge();
        _topRight.Merge();
        _bottomLeft.Merge();
        _bottomRight.Merge();

        TakeElements(_topLeft);
        TakeElements(_topRight);
        TakeElements(_bottomLeft);
        TakeElements(_bottomRight);

        _topLeft = _topRight = _bottomLeft = _bottomRight = null;
    }

    private void TakeElements(Quadtree<T> child)
    {
        foreach (T element in child._elements)
        {
            _elements.Add(element);
            _locations[element] = this;
        }

        child._elements.Clear();
    }

    private Quadtree<T>? GetContainingChild(IShape bounds)
    {
        if (IsLeaf)
            return null;

        if (_topLeft.Bounds.Contains(bounds))
            return _topLeft;

        if (_topRight.Bounds.Contains(bounds))
            return _topRight;

        if (_bottomLeft.Bounds.Contains(bounds))
            return _bottomLeft;

        return _bottomRight.Bounds.Contains(bounds) ? _bottomRight : null;
    }
}
