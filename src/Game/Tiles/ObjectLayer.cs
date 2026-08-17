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
/// Provides an area in a tile map filled with objects that mark out meaningful positions on the map.
/// </summary>
/// <remarks>
/// Object layers contribute nothing to what a tile map renders; they exist so that gameplay concerns anchored to a location
/// on the map, such as the transition points connecting one area to another, can be authored in the same editor that lays
/// out the map itself.
/// </remarks>
public sealed class ObjectLayer : Layer
{
    private readonly List<MapObject> _objects = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectLayer"/> class.
    /// </summary>
    /// <param name="name">The name of the layer.</param>
    /// <param name="objects">The objects appearing on the layer.</param>
    /// <param name="customProperties">The object layer's custom properties.</param>
    public ObjectLayer(string name, IEnumerable<MapObject> objects, CustomProperties customProperties)
        : base(name, customProperties)
    {
        Require.NotNull(objects, nameof(objects));

        _objects.AddRange(objects);
    }

    /// <summary>
    /// Gets the collection of objects appearing on this layer.
    /// </summary>
    public IReadOnlyCollection<MapObject> Objects
        => _objects;
}
