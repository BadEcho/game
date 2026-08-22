// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2025 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

namespace BadEcho.Game.Pipeline.Tiles;

/// <summary>
/// Provides a set of constants related to the TMX map XML file format.
/// </summary>
internal static class XmlConstants
{
    /// <summary>
    /// The name for attributes defining the name of a TMX-related entity.
    /// </summary>
    internal static string NameAttribute
        => "name";

    /// <summary>
    /// The name for attributes defining the path to external files containing further configuration data.
    /// </summary>
    internal static string SourceAttribute
        => "source";

    /// <summary>
    /// The name for attributes defining the width of a TMX-related entity.
    /// </summary>
    internal static string WidthAttribute
        => "width";

    /// <summary>
    /// The name for attributes defining the height of a TMX-related entity.
    /// </summary>
    internal static string HeightAttribute
        => "height";

    /// <summary>
    /// The name for attributes defining the width of tiles.
    /// </summary>
    internal static string TileWidthAttribute
        => "tilewidth";

    /// <summary>
    /// The name for attributes defining the height of tiles.
    /// </summary>
    internal static string TileHeightAttribute
        => "tileheight";

    /// <summary>
    /// The name for attributes defining the horizontal offset of a TMX-related entity from the tile map's origin.
    /// </summary>
    internal static string OffsetXAttribute
        => "offsetx";

    /// <summary>
    /// The name for attributes defining the vertical offset of a TMX-related entity from the tile map's origin.
    /// </summary>
    internal static string OffsetYAttribute
        => "offsety";

    /// <summary>
    /// The name for attributes defining the visibility of a TMX-related entity.
    /// </summary>
    internal static string VisibleAttribute
        => "visible";

    /// <summary>
    /// The name for attributes defining the opacity of a TMX-related entity.
    /// </summary>
    internal static string OpacityAttribute
        => "opacity";

    /// <summary>
    /// The name for elements defining image data.
    /// </summary>
    internal static string ImageElement
        => "image";

    /// <summary>
    /// The name for attributes defining the identifier of a TMX-related entity.
    /// </summary>
    internal static string IdAttribute
        => "id";

    /// <summary>
    /// The name for attributes defining the class of a map object, as written by Tiled 1.9 and later.
    /// </summary>
    internal static string ClassAttribute
        => "class";

    /// <summary>
    /// The name for attributes defining the class of a map object, as written by Tiled versions prior to 1.9.
    /// </summary>
    internal static string TypeAttribute
        => "type";

    /// <summary>
    /// The name for attributes defining the global tile identifier stamped by a map object.
    /// </summary>
    internal static string GidAttribute
        => "gid";

    /// <summary>
    /// The name for attributes defining the horizontal coordinate of a TMX-related entity.
    /// </summary>
    internal static string XAttribute
        => "x";

    /// <summary>
    /// The name for attributes defining the vertical coordinate of a TMX-related entity.
    /// </summary>
    internal static string YAttribute
        => "y";

    /// <summary>
    /// The name for attributes defining the clockwise rotation of a TMX-related entity, in degrees.
    /// </summary>
    internal static string RotationAttribute
        => "rotation";

    /// <summary>
    /// The name for elements marking a map object as occupying a single coordinate.
    /// </summary>
    internal static string PointElement
        => "point";

    /// <summary>
    /// The name for elements marking a map object as occupying an elliptical region.
    /// </summary>
    internal static string EllipseElement
        => "ellipse";

    /// <summary>
    /// The name for elements marking a map object as occupying a closed polygonal region.
    /// </summary>
    internal static string PolygonElement
        => "polygon";

    /// <summary>
    /// The name for elements marking a map object as tracing an open sequence of connected lines.
    /// </summary>
    internal static string PolylineElement
        => "polyline";

    /// <summary>
    /// The name for elements marking a map object as displaying text.
    /// </summary>
    internal static string TextElement
        => "text";
}
