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

using BadEcho.Game.Scenes;
using BadEcho.Game.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Tests;

/// <summary>
/// Provides a gameplay scene that exposes the area transition machinery so that it can be driven a step at a time.
/// </summary>
internal sealed class TestGameplayScene : GameplayScene
{
    private readonly List<Area> _loadedAreas = [];

    public TestGameplayScene(Microsoft.Xna.Framework.Game game, params Area[] areas)
        : base(game)
        => _loadedAreas.AddRange(areas);

    public IEntity? Activator
    { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if a begun transition is left for the test to complete itself.
    /// </summary>
    public bool DefersTransitions
    { get; set; }

    public int TransitionedCount
    { get; private set; }

    public Area? PreviousArea
    { get; private set; }

    public Area? NewArea
    { get; private set; }

    public TransitionPoint? DestinationPoint
    { get; private set; }

    public TransitionPoint? ObservedActiveTransitionPoint
    { get; private set; }

    /// <summary>
    /// Advances this scene by a single update, exactly as its scene manager would.
    /// </summary>
    public void Tick()
        => Update(new GameUpdateTime(Game, new GameTime()), true);

    public void Complete(TransitionPoint transitionPoint, Area loadedArea)
        => CompleteAreaTransition(transitionPoint, loadedArea);

    public void MakeCurrent(Area area)
        => CurrentArea = area;

    protected override IEntity? TransitionActivator
        => Activator;

    protected override IEnumerable<Area> LoadAreas()
        => _loadedAreas;

    protected override void OnAreaTransitioning(TransitionPoint transitionPoint, Area newArea)
    {
        ObservedActiveTransitionPoint = transitionPoint;

        if (DefersTransitions)
            return;

        base.OnAreaTransitioning(transitionPoint, newArea);
    }

    protected override void OnAreaTransitioned(Area? previousArea, Area newArea, TransitionPoint? destinationPoint)
    {
        TransitionedCount++;
        PreviousArea = previousArea;
        NewArea = newArea;
        DestinationPoint = destinationPoint;
    }

    protected override void DrawGameplay(SpriteBatch spriteBatch)
    { }
}
