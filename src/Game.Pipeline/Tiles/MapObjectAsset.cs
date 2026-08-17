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

namespace BadEcho.Game.Pipeline.Tiles;

/// <summary>
/// Provides configuration data for an object appearing on a tile map's object layer.
/// </summary>
/// <remarks>
/// Only rectangular and point-shaped objects are supported. Rectangles are anchored at their upper-left corner, and points
/// carry no size. Objects that stamp a tile (identified by a <c>gid</c> attribute) are notable for anchoring their vertical
/// coordinate at their bottom edge instead; because they are unsupported, and dropped during processing, that discrepancy
/// never reaches the runtime.
/// </remarks>
public sealed class MapObjectAsset : ExtensibleAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapObjectAsset"/> class.
    /// </summary>
    /// <param name="root">XML element for the map object's configuration.</param>
    public MapObjectAsset(XElement root)
        : base(root)
    {
        Require.NotNull(root, nameof(root));

        Id = (int?) root.Attribute(XmlConstants.IdAttribute) ?? default;
        Name = (string?) root.Attribute(XmlConstants.NameAttribute) ?? string.Empty;
        // Tiled 1.9 renamed the object 'type' attribute to 'class'; maps saved by either era of the editor are accepted.
        Type = (string?) root.Attribute(XmlConstants.ClassAttribute)
            ?? (string?) root.Attribute(XmlConstants.TypeAttribute)
            ?? string.Empty;
        X = (float?) root.Attribute(XmlConstants.XAttribute) ?? default;
        Y = (float?) root.Attribute(XmlConstants.YAttribute) ?? default;
        Width = (float?) root.Attribute(XmlConstants.WidthAttribute) ?? default;
        Height = (float?) root.Attribute(XmlConstants.HeightAttribute) ?? default;
        IsPoint = root.Element(XmlConstants.PointElement) != null;

        IsSupportedShape
            = root.Attribute(XmlConstants.GidAttribute) == null
                && root.Element(XmlConstants.EllipseElement) == null
                && root.Element(XmlConstants.PolygonElement) == null
                && root.Element(XmlConstants.PolylineElement) == null
                && root.Element(XmlConstants.TextElement) == null;
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
    public string Type
    { get; }

    /// <summary>
    /// Gets the horizontal coordinate of this map object, measured in pixels.
    /// </summary>
    public float X
    { get; }

    /// <summary>
    /// Gets the vertical coordinate of this map object, measured in pixels.
    /// </summary>
    public float Y
    { get; }

    /// <summary>
    /// Gets the width of this map object, measured in pixels.
    /// </summary>
    public float Width
    { get; }

    /// <summary>
    /// Gets the height of this map object, measured in pixels.
    /// </summary>
    public float Height
    { get; }

    /// <summary>
    /// Gets a value indicating if this map object occupies a single coordinate as opposed to a rectangular region.
    /// </summary>
    public bool IsPoint
    { get; }

    /// <summary>
    /// Gets a value indicating if this map object's shape is one supported by the Bad Echo Game Framework.
    /// </summary>
    /// <remarks>
    /// Elliptical, polygonal, polyline, text, and tile-stamping objects are all unsupported, and are dropped during
    /// processing.
    /// </remarks>
    public bool IsSupportedShape
    { get; }
}
