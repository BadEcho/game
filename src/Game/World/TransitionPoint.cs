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

using BadEcho.Game.Properties;

namespace BadEcho.Game.World;

/// <summary>
/// Provides a point or region in an area that, when entered, triggers a transition to another area.
/// </summary>
/// <remarks>
/// <para>
/// A transition point takes one of two forms, chosen by the constructor used. A point form, entered when the entering entity's
/// bounds contain that coordinate, and a region form, entered when the entering entity's bounds intersect it.
/// </para>
/// <para>
/// The geometry and target of a transition point are immutable; only <see cref="IsEnabled"/> may be changed after
/// construction, (which covers cases such as a portal suddenly appearing or a pathway becoming inaccessible due to a cave-in, etc.).
/// </para>
/// </remarks>
public sealed class TransitionPoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransitionPoint"/> class occupying a single coordinate.
    /// </summary>
    /// <param name="targetAreaName">The name of the area entering this point transitions to.</param>
    /// <param name="location">The coordinates this point occupies.</param>
    public TransitionPoint(string targetAreaName, PointF location)
    {
        ValidateTargetAreaName(targetAreaName);

        TargetAreaName = targetAreaName;
        Location = location;
        Bounds = RectangleF.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransitionPoint"/> class occupying a rectangular region.
    /// </summary>
    /// <param name="targetAreaName">The name of the area entering this point transitions to.</param>
    /// <param name="bounds">The region this point occupies.</param>
    public TransitionPoint(string targetAreaName, RectangleF bounds)
    {
        ValidateTargetAreaName(targetAreaName);

        TargetAreaName = targetAreaName;
        Bounds = bounds;
        Location = bounds.Location;
        IsRegion = true;
    }

    /// <summary>
    /// Gets the name identifying this transition point.
    /// </summary>
    /// <remarks>
    /// Names are optional and are not validated for uniqueness; a lookup by name yields the first match found. Map-authored
    /// points source their name from the name of the object they were loaded from.
    /// </remarks>
    public string Name
    { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the area entering this transition point transitions to.
    /// </summary>
    public string TargetAreaName
    { get; }

    /// <summary>
    /// Gets the name of the transition point in the target area that the transitioning entity should spawn at.
    /// </summary>
    /// <remarks>
    /// This wires two transition points together across two different areas, the usual arrangement being a pair of doorways
    /// naming each other. Because it references a point belonging to another area, and areas are built independently of each
    /// other, it cannot be validated until the transition actually occurs. An empty value means no destination is wired, and
    /// placement of the transitioning entity is left entirely to the consumer.
    /// </remarks>
    public string TargetPointName
    { get; init; } = string.Empty;

    /// <summary>
    /// Gets the region this transition point occupies, which is <see cref="RectangleF.Empty"/> if this point occupies a
    /// single coordinate instead.
    /// </summary>
    public RectangleF Bounds
    { get; }

    /// <summary>
    /// Gets the coordinates this transition point occupies, which is the upper-left corner of <see cref="Bounds"/> if this
    /// point occupies a region instead.
    /// </summary>
    public PointF Location
    { get; }

    /// <summary>
    /// Gets a value indicating if this transition point occupies a rectangular region as opposed to a single coordinate.
    /// </summary>
    public bool IsRegion
    { get; }

    /// <summary>
    /// Gets the position an entity spawning at this transition point should be placed at.
    /// </summary>
    public PointF SpawnPosition
        => IsRegion ? Bounds.Center : Location;

    /// <summary>
    /// Gets or sets a value indicating if this transition point currently triggers transitions.
    /// </summary>
    public bool IsEnabled
    { get; set; } = true;

    /// <summary>
    /// Determines if an entity occupying the specified bounds has entered this transition point.
    /// </summary>
    /// <param name="entityBounds">The spatial bounds of the entity to check.</param>
    /// <returns>True if an entity occupying <c>entityBounds</c> has entered this transition point; otherwise, false.</returns>
    public bool IsEntered(IShape entityBounds)
    {
        Require.NotNull(entityBounds, nameof(entityBounds));

        if (!IsEnabled)
            return false;

        IShape bounds = Bounds;

        return IsRegion ? bounds.Intersects(entityBounds) : entityBounds.Contains(Location);
    }

    private static void ValidateTargetAreaName(string targetAreaName)
    {
        if (string.IsNullOrEmpty(targetAreaName))
            throw new ArgumentException(Strings.TransitionPointNoTargetAreaName, nameof(targetAreaName));
    }
}
