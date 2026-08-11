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

namespace BadEcho.Game.Pipeline.Areas;

/// <summary>
/// Provides configuration data for an area.
/// </summary>
public sealed class AreaAsset
{
    /// <summary>
    /// Gets the human-readable description of the area.
    /// </summary>
    public string Name
    { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the area's tile map asset.
    /// </summary>
    public string TileMapPath
    { get; set; } = string.Empty;

    /// <summary>
    /// Gets the collection of actors the area is initially populated with.
    /// </summary>
    public IReadOnlyCollection<AreaActorAsset> Actors
    { get; init; } = new List<AreaActorAsset>();
}

