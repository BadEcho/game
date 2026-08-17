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

namespace BadEcho.Game.Tiles;

/// <summary>
/// Specifies the type of layer and the kind of content it contains.
/// </summary>
/// <remarks>
/// This is used solely for the purpose of expressing type identity during pipeline serialization, as only simple values may
/// be transmitted.
/// </remarks>
public enum LayerType
{
    /// <summary>
    /// A layer containing tile data.
    /// </summary>
    Tile,
    /// <summary>
    /// A layer containing image data.
    /// </summary>
    Image,
    /// <summary>
    /// A group of layers.
    /// </summary>
    Group,
    /// <summary>
    /// A layer containing map objects.
    /// </summary>
    /// <remarks>
    /// New members must always be appended to the end of this enumeration, as its values are serialized as integers into
    /// every built tile map.
    /// </remarks>
    Object
}
