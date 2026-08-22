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

namespace BadEcho.Game.Tiles;

/// <summary>
/// Provides a shape occupying a position on a tile map, giving meaning to that position without contributing anything to
/// what is rendered.
/// </summary>
public sealed class MapObject : Extensible
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapObject"/> class occupying a single coordinate.
    /// </summary>
    /// <param name="id">The identifier for the map object.</param>
    /// <param name="name">The name of the map object.</param>
    /// <param name="type">The name of the class the map object belongs to.</param>
    /// <param name="location">The coordinates the map object occupies.</param>
    /// <param name="customProperties">The map object's custom properties.</param>
    public MapObject(int id, string name, string type, PointF location, CustomProperties customProperties)
        : base(customProperties)
    {
        Id = id;
        Name = name;
        Type = type;
        Location = location;
        Bounds = RectangleF.Empty;
        IsPoint = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapObject"/> class occupying a rectangular region.
    /// </summary>
    /// <param name="id">The identifier for the map object.</param>
    /// <param name="name">The name of the map object.</param>
    /// <param name="type">The name of the class the map object belongs to.</param>
    /// <param name="bounds">The region the map object occupies.</param>
    /// <param name="customProperties">The map object's custom properties.</param>
    public MapObject(int id, string name, string type, RectangleF bounds, CustomProperties customProperties)
        : base(customProperties)
    {
        Id = id;
        Name = name;
        Type = type;
        Bounds = bounds;
        Location = bounds.Location;
    }

    /// <summary>
    /// Gets the identifier for this map object, unique within its tile map.
    /// </summary>
    public int Id
    { get; }

    /// <summary>
    /// Gets the name of this map object.
    /// </summary>
    public string Name
    { get; }

    /// <summary>
    /// Gets the name of the class this map object belongs to, which gives it its meaning.
    /// </summary>
    /// <remarks>Well-known class names are found in <see cref="KnownObjectTypes"/>.</remarks>
    public string Type
    { get; }

    /// <summary>
    /// Gets a value indicating if this map object occupies a single coordinate as opposed to a rectangular region.
    /// </summary>
    public bool IsPoint
    { get; }

    /// <summary>
    /// Gets the coordinates this map object occupies, which is the upper-left corner of <see cref="Bounds"/> if this object
    /// occupies a region instead.
    /// </summary>
    /// <remarks>
    /// These location coordinates will already account for the offset of the object layer this object belongs to, allowing consumers
    /// of the map object to be able to use said object without needing to be aware of the layer's offset.
    /// </remarks>
    public PointF Location
    { get; }

    /// <summary>
    /// Gets the region this map object occupies, which is <see cref="RectangleF.Empty"/> if this object occupies a single
    /// coordinate instead.
    /// </summary>
    public RectangleF Bounds
    { get; }

    /// <summary>
    /// Gets the clockwise rotation of this map object, measured in degrees.
    /// </summary>
    /// <remarks>
    /// Both <see cref="Location"/> and <see cref="Bounds"/> are axis-aligned, so a consumer that honors
    /// rotation has to apply it itself.
    /// </remarks>
    public float Rotation
    { get; init; }
}
