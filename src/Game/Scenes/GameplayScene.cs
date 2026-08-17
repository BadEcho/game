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

using System.Diagnostics.CodeAnalysis;
using BadEcho.Extensions;
using BadEcho.Game.Effects;
using BadEcho.Game.Properties;
using BadEcho.Game.World;
using BadEcho.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Scenes;

/// <summary>
/// Provides a game scene for hosting core gameplay.
/// </summary>
public abstract class GameplayScene : GameScene
{
    private readonly List<Area> _areas = [];
    private readonly DeferredRenderer _renderer;

    private bool _transitionArmed;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameplayScene"/> class.
    /// </summary>
    /// <param name="game">The game this scene is for.</param>
    protected GameplayScene(Microsoft.Xna.Framework.Game game)
        : base(game)
    {
        _renderer = new DeferredRenderer(game.GraphicsDevice);
    }
    
    /// <summary>
    /// Gets a value indicating if gameplay is paused.
    /// </summary>
    public bool IsPaused
    { get; protected set; }

    /// <summary>
    /// Gets a value indicating if an area has been loaded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(CurrentArea))]
    public bool IsAreaLoaded
        => CurrentArea != null;

    /// <summary>
    /// Gets the currently loaded area.
    /// </summary>
    public Area? CurrentArea
    { get; protected set; }

    /// <summary>
    /// Gets a value indicating if a transition from one area to another is in progress.
    /// </summary>
    public bool IsTransitioningAreas
    { get; private set; }

    /// <inheritdoc/>
    protected override bool AlwaysDisplay
        => true;

    /// <summary>
    /// Gets the collection of loaded areas.
    /// </summary>
    protected IReadOnlyCollection<Area> Areas
        => _areas;

    /// <summary>
    /// Gets the transition point that initiated the transition currently in progress, if there is one.
    /// </summary>
    /// <remarks>
    /// This is set when a transition begins and cleared once <see cref="OnAreaTransitioned"/> has returned, making it readable
    /// throughout the transition. It is null for a transition completed without having been begun, such as a scripted switch
    /// from one area to another.
    /// </remarks>
    protected TransitionPoint? ActiveTransitionPoint
    { get; private set; }

    /// <summary>
    /// Gets the entity whose movement into a transition point triggers a transition to another area.
    /// </summary>
    /// <remarks>
    /// A scene desiring area transitions should override this to return the entity acting on the player's behalf.
    /// Returning null will disable the monitoring of transition points entirely.
    /// </remarks>
    protected abstract ISpatial? TransitionActivator
    { get; }

    /// <summary>
    /// Loads all areas associated with this scene.
    /// </summary>
    /// <returns>The <see cref="Area"/> instances associated with this scene.</returns>
    /// <remarks>
    /// The names of the returned areas must be unique, without regard to case, as an area is looked up by name when a
    /// transition to it completes.
    /// </remarks>
    protected abstract IEnumerable<Area> LoadAreas();

    /// <inheritdoc/>
    protected sealed override void UpdateCore(GameUpdateTime time, bool isActive)
    {
        IsPaused = !isActive;

        if (IsPaused)
            return;

        UpdateGameplay(time);
        CheckAreaTransitions();
    }

    /// <inheritdoc/>
    protected sealed override void DrawCore(SpriteBatch spriteBatch)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));        

        DrawGameplay(spriteBatch);
    }

    /// <inheritdoc/>
    protected override void OnLoad(SceneManager manager)
    {
        foreach (Area area in LoadAreas())
        {
            AddArea(area);
        }

        base.OnLoad(manager);
    }

    /// <summary>
    /// Executes custom gameplay-specific update logic.
    /// </summary>
    /// <param name="time">The game timing configuration and scene for this update.</param>
    protected virtual void UpdateGameplay(GameUpdateTime time)
    {
        if (!IsAreaLoaded)
            return;

        CurrentArea.Update(time);
    }

    /// <summary>
    /// Begins a transition to the area targeted by the specified transition point.
    /// </summary>
    /// <param name="transitionPoint">The transition point initiating the transition.</param>
    /// <remarks>
    /// This does nothing if a transition is already in progress. Whether the transition completes immediately or is deferred
    /// is up to <see cref="OnAreaTransitioning"/>.
    /// </remarks>
    protected void BeginAreaTransition(TransitionPoint transitionPoint)
    {
        Require.NotNull(transitionPoint, nameof(transitionPoint));

        if (IsTransitioningAreas)
            return;

        IsTransitioningAreas = true;
        ActiveTransitionPoint = transitionPoint;

        OnAreaTransitioning(transitionPoint);
    }

    /// <summary>
    /// Called when a transition to another area has begun.
    /// </summary>
    /// <param name="transitionPoint">The transition point that initiated the transition.</param>
    /// <remarks>
    /// The default implementation switches to the target area immediately, which is all a game with no loading concerns
    /// requires. An override wishing to defer the switch must not call the base implementation; instead, it arranges for the
    /// new area to become available and then calls <see cref="CompleteAreaTransition(Area)"/> or
    /// <see cref="CompleteAreaTransition(string)"/> once it is.
    /// </remarks>
    protected virtual void OnAreaTransitioning(TransitionPoint transitionPoint)
    {
        Require.NotNull(transitionPoint, nameof(transitionPoint));

        CompleteAreaTransition(transitionPoint.TargetAreaName);
    }

    /// <summary>
    /// Completes a transition by making the loaded area with the specified name the current area.
    /// </summary>
    /// <param name="areaName">The name of the area to transition to.</param>
    /// <exception cref="ArgumentException">
    /// No loaded area is named <c>areaName</c>, and <see cref="LoadDeferredArea"/> provided none.
    /// </exception>
    protected void CompleteAreaTransition(string areaName)
    {
        Require.NotNull(areaName, nameof(areaName));

        Area? nextArea = FindArea(areaName);

        if (nextArea == null)
        {
            nextArea = LoadDeferredArea(areaName)
                ?? throw new ArgumentException(Strings.AreaNotFound.InvariantFormat(areaName), nameof(areaName));

            AddArea(nextArea);
        }

        CompleteAreaTransitionCore(nextArea);
    }

    /// <summary>
    /// Completes a transition by making the specified, already-constructed area the current area.
    /// </summary>
    /// <param name="loadedArea">The area to transition to.</param>
    /// <exception cref="InvalidOperationException">
    /// A different area sharing <c>loadedArea</c>'s name has already been loaded.
    /// </exception>
    /// <remarks>
    /// This is the hand-off for a deferred transition: a <see cref="DeferredWorker"/> builds the area on a background thread
    /// and the code observing its completion passes the result here, sparing it from having to stash the area somewhere for
    /// a <see cref="LoadDeferredArea"/> override to retrieve. An area already loaded under this exact instance is not added
    /// a second time.
    /// </remarks>
    protected void CompleteAreaTransition(Area loadedArea)
    {
        Require.NotNull(loadedArea, nameof(loadedArea));

        Area? existingArea = FindArea(loadedArea.Name);

        if (existingArea == null)
            _areas.Add(loadedArea);
        else if (!ReferenceEquals(existingArea, loadedArea))
            throw new InvalidOperationException(Strings.AreaNameDuplicate.InvariantFormat(loadedArea.Name));

        CompleteAreaTransitionCore(loadedArea);
    }

    /// <summary>
    /// Called to load an area absent from the collection of loaded areas.
    /// </summary>
    /// <param name="areaName">The name of the area to load.</param>
    /// <returns>The <see cref="Area"/> named <c>areaName</c>, or null if no such area can be provided.</returns>
    /// <remarks>
    /// The default implementation returns null, appropriate for a scene whose areas all come from <see cref="LoadAreas"/>.
    /// An override supporting on-demand loading is always invoked on the game thread.
    /// </remarks>
    protected virtual Area? LoadDeferredArea(string areaName)
        => null;

    /// <summary>
    /// Called after a transition has completed and the new area has become the current area.
    /// </summary>
    /// <param name="previousArea">The area transitioned away from, or null if there was none.</param>
    /// <param name="newArea">The area transitioned to, which is now the current area.</param>
    /// <param name="destinationPoint">
    /// The transition point in <c>newArea</c> named by the initiating point's
    /// <see cref="TransitionPoint.TargetPointName"/>, or null if there was none to resolve.
    /// </param>
    /// <remarks>
    /// <para>
    /// This is where the entity acting as the transition activator gets moved into the new area. The framework has no concept
    /// of a player, so it supplies the destination and leaves the move to the game.
    /// </para>
    /// <para>
    /// A null <c>destinationPoint</c> means the initiating point wired no destination, the wired destination was not found in
    /// the new area, or the transition had no initiating point at all. The initiating point itself remains readable through
    /// <see cref="ActiveTransitionPoint"/> for the duration of this call.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override void OnAreaTransitioned(Area? previousArea, Area newArea, TransitionPoint? destinationPoint)
    /// {
    ///     if (destinationPoint != null)
    ///         _player.Position = destinationPoint.SpawnPosition;
    /// }
    /// </code>
    /// </example>
    protected virtual void OnAreaTransitioned(Area? previousArea, Area newArea, TransitionPoint? destinationPoint)
    { }

    /// <summary>
    /// Executes the custom rendering logic required to draw the gameplay to the screen.
    /// </summary>
    /// <param name="spriteBatch">A sprite batch for drawing the scene.</param>
    protected virtual void DrawGameplay(SpriteBatch spriteBatch)
    {
        if (!IsAreaLoaded)
            return;

        CurrentArea.Draw(spriteBatch, _renderer, RenderStates);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _renderer.Dispose();

            _disposed = true;
        }

        base.Dispose(disposing);
    }

    private Area? FindArea(string areaName)
        => _areas.FirstOrDefault(a => a.Name.Equals(areaName, StringComparison.OrdinalIgnoreCase));

    private void AddArea(Area area)
    {
        // Areas are looked up by name without regard to case, which makes two areas sharing a name a silent mis-transition
        // waiting to happen. We fail at the moment the second one shows up instead.
        if (FindArea(area.Name) != null)
            throw new InvalidOperationException(Strings.AreaNameDuplicate.InvariantFormat(area.Name));

        _areas.Add(area);
    }

    private void CompleteAreaTransitionCore(Area newArea)
    {
        TransitionPoint? destinationPoint = null;
        string targetPointName = ActiveTransitionPoint?.TargetPointName ?? string.Empty;

        if (!string.IsNullOrEmpty(targetPointName))
        {   // Whether the destination is currently able to trigger a transition of its own has no bearing on its suitability
            // as a place to spawn, so the point's enabled state is deliberately ignored here.
            destinationPoint
                = newArea.TransitionPoints
                         .FirstOrDefault(t => t.Name.Equals(targetPointName, StringComparison.OrdinalIgnoreCase));

            if (destinationPoint == null)
            {
                Logger.Warning(
                    Strings.TransitionPointDestinationNotFound.InvariantFormat(targetPointName, newArea.Name));
            }
        }

        Area? previousArea = CurrentArea;

        CurrentArea = newArea;
        IsTransitioningAreas = false;
        _transitionArmed = false;

        OnAreaTransitioned(previousArea, newArea, destinationPoint);

        ActiveTransitionPoint = null;
    }

    private void CheckAreaTransitions()
    {
        if (!IsAreaLoaded || IsTransitioningAreas)
            return;

        ISpatial? activator = TransitionActivator;

        if (activator == null)
            return;

        TransitionPoint? enteredPoint = CurrentArea.FindEnteredTransitionPoint(activator.Bounds);

        if (!_transitionArmed)
        {   // Detection arms only once the activator has been observed standing outside every enabled transition point. That
            // covers the activator being spawned on top of one, whether by the transition that just completed, by the game
            // starting, or by a save being restored; in none of those cases should a transition fire straight back.
            if (enteredPoint == null)
                _transitionArmed = true;

            return;
        }

        if (enteredPoint != null)
            BeginAreaTransition(enteredPoint);
    }
}
