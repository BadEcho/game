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
/// Provides a set of known class names used to give meaning to the objects appearing on a tile map's object layers.
/// </summary>
/// <remarks>
/// A map object's class is its <c>class</c> attribute in the TMX map format, named <c>type</c> prior to Tiled 1.9. Objects
/// are matched against these names without regard to case.
/// </remarks>
public static class KnownObjectTypes
{
    /// <summary>
    /// Gets the class name for map objects that are folded into an area's collection of transition points.
    /// </summary>
    public static string TransitionPoint
        => nameof(TransitionPoint);
}
