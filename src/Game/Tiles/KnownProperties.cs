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
/// Provides a set of known custom property names used by tile-related content.
/// </summary>
public static class KnownProperties
{
    /// <summary>
    /// Gets the name for the boolean custom property on tile layers that indicates whether its tiles will cause
    /// collisions for entities attempting to cross over them. 
    /// </summary>
    public static string Collidable
        => nameof(Collidable);

    /// <summary>
    /// Gets the name for the string custom property on transition point map objects that identifies the area entering the
    /// point transitions to.
    /// </summary>
    public static string TargetAreaName
        => nameof(TargetAreaName);

    /// <summary>
    /// Gets the name for the optional string custom property on transition point map objects that identifies the transition
    /// point in the target area that the transitioning entity should spawn at.
    /// </summary>
    /// <remarks>
    /// This references a transition point belonging to a different map, and maps are built independently of each other, so
    /// no build-time validation of the name is possible; it is resolved at the time the transition occurs.
    /// </remarks>
    public static string TargetPointName
        => nameof(TargetPointName);

    /// <summary>
    /// Gets the name for the optional boolean custom property on transition point map objects that indicates whether the
    /// point starts out able to trigger transitions.
    /// </summary>
    /// <remarks>An absent value means the transition point is enabled.</remarks>
    public static string Enabled
        => nameof(Enabled);
}
