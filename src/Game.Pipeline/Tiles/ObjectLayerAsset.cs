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

using System.Xml.Linq;
using BadEcho.Game.Tiles;

namespace BadEcho.Game.Pipeline.Tiles;

/// <summary>
/// Provides configuration data for a tile map object layer asset.
/// </summary>
public sealed class ObjectLayerAsset : LayerAsset
{
    private const string OBJECT_ELEMENT = "object";

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectLayerAsset"/> class.
    /// </summary>
    /// <param name="root">XML element for the object layer's configuration.</param>
    public ObjectLayerAsset(XElement root)
        : base(root, LayerType.Object)
    {
        Require.NotNull(root, nameof(root));

        foreach (XElement mapObject in root.Elements(OBJECT_ELEMENT))
        {
            Objects.Add(new MapObjectAsset(mapObject));
        }
    }

    /// <summary>
    /// Gets the collection of objects appearing on this object layer.
    /// </summary>
    public ICollection<MapObjectAsset> Objects
    { get; } = new List<MapObjectAsset>();
}
