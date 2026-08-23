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

using Microsoft.Xna.Framework;

namespace BadEcho.Game.Pipeline.Areas;

/// <summary>
/// Provides configuration data for an actor in an area.
/// </summary>
public sealed class AreaActorAsset
{
    /// <summary>
    /// Gets or sets the path to the actor's sprite sheet asset.
    /// </summary>
    public string SpriteSheetPath
    { get; set; } = string.Empty;

    /// <summary>
    /// Gets the initial drawing location of the actor.
    /// </summary>
    public Vector2 Position
    { get; init; }

    /// <summary>
    /// Gets a value indicating if the actor does not cast a shadow.
    /// </summary>
    public bool IsShadowless
    { get;init; }
}
