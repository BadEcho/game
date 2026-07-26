using System;
using System.Collections.Generic;
using System.Text;

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

