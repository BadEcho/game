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
using Microsoft.Xna.Framework;
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
    /// <param name="context">Contextual information for the game.</param>
    protected GameplayScene(GameContext context)
        : base(context)
    {
        Require.NotNull(context, nameof(context));

        _renderer = new DeferredRenderer(context.GraphicsDevice);
        Camera = new Camera(context.ViewportConnector);
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
    {
        get;
        protected set
        {
            field = value;
            IsTransitioningAreas = false;
            _transitionArmed = false;
        }
    }

    /// <summary>
    /// Gets a value indicating if a transition from one area to another is in progress.
    /// </summary>
    public bool IsTransitioningAreas
    { get; private set; }

    /// <inheritdoc/>
    protected override bool AlwaysDisplay
        => true;

    /// <inheritdoc/>
    protected override Matrix MatrixTransform
        => Camera.GetViewMatrix();

    /// <summary>
    /// Gets the collection of loaded areas.
    /// </summary>
    protected IReadOnlyCollection<Area> Areas
        => _areas;

    /// <summary>
    /// Gets the camera for viewing the scene.
    /// </summary>
    protected Camera Camera
    { get; }

    /// <summary>
    /// Gets the entity whose movement into a transition point triggers a transition to another area.
    /// </summary>
    /// <remarks>
    /// A scene desiring area transitions should override this to return the entity acting on the player's behalf.
    /// Returning null will disable the monitoring of transition points entirely.
    /// </remarks>
    protected abstract IEntity? TransitionActivator
    { get; }

    /// <summary>
    /// Loads all areas associated with this scene.
    /// </summary>
    /// <returns>The <see cref="Area"/> instances associated with this scene.</returns>
    /// <remarks>
    /// The names of the returned areas must be unique, without regard to case, as an area will be looked up by its name when
    /// being transitioned to.
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
        {   // Ensure no other area with same name has already been added.
            if (FindArea(area.Name) != null)
                throw new InvalidOperationException(Strings.AreaNameDuplicate.InvariantFormat(area.Name));

            _areas.Add(area);
        }

        CurrentArea = _areas.FirstOrDefault();

        if (CurrentArea != null)
            OnAreaLoaded(CurrentArea, CurrentArea.DefaultSpawnPosition);

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

        Camera.Update();
        CurrentArea.Update(time);
    }

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

    /// <summary>
    /// Begins a transition to the area targeted by the specified transition point.
    /// </summary>
    /// <param name="transitionPoint">The transition point initiating the transition.</param>
    /// <remarks>
    /// <para>
    /// This will be called during the <see cref="UpdateCore"/> tick if the transition activator has entered a transition point.
    /// It can also be manually called by the scene in order to force an area transition.
    /// </para>
    /// <para>This does nothing if a transition is already in progress.</para>
    /// </remarks>
    protected void BeginAreaTransition(TransitionPoint transitionPoint)
    {
        Require.NotNull(transitionPoint, nameof(transitionPoint));

        if (IsTransitioningAreas)
            return;

        Area nextArea =
            FindArea(transitionPoint.TargetAreaName)
            ?? throw new InvalidOperationException(Strings.AreaNotFound.InvariantFormat(transitionPoint.TargetAreaName));

        IsTransitioningAreas = true;

        OnAreaTransitioning(transitionPoint, nextArea);
    }

    /// <summary>
    /// Completes a transition by making the specified, already-constructed area the current area.
    /// </summary>
    /// <param name="transitionPoint">The transition point that initiated the transition.</param>
    /// <param name="newArea">The area being transitioned to.</param>
    protected void CompleteAreaTransition(TransitionPoint transitionPoint, Area newArea)
    {
        Require.NotNull(transitionPoint, nameof(transitionPoint));
        Require.NotNull(newArea, nameof(newArea));

        string targetPointName = transitionPoint.TargetPointName;

        // Every transition point names a destination, but the areas naming each other are built independently, so the name
        // is only ever resolved here and a miss leaves placement of the transitioning entity to the consumer.
        TransitionPoint? destinationPoint
            = newArea.TransitionPoints
                     .FirstOrDefault(t => t.Name.Equals(targetPointName, StringComparison.OrdinalIgnoreCase));

        if (destinationPoint == null)
        {
            Logger.Warning(
                Strings.TransitionPointDestinationNotFound.InvariantFormat(targetPointName, newArea.Name));
        }

        CurrentArea = newArea;
        OnAreaLoaded(newArea, destinationPoint?.SpawnPosition ?? newArea.DefaultSpawnPosition);
    }

    /// <summary>
    /// Called when a transition to another area has begun.
    /// </summary>
    /// <param name="transitionPoint">The transition point that initiated the transition.</param>
    /// <param name="newArea">The area being transitioned to.</param>
    /// <remarks>
    /// This default implementation will switch to the target area immediately. Scenes wishing to perform other actions first,
    /// such as some sort of transition effect on the screen, should override this, avoid calling the base implementation of this method,
    /// and then call <see cref="CompleteAreaTransition"/> when wanting to finalize the transition.
    /// </remarks>
    protected virtual void OnAreaTransitioning(TransitionPoint transitionPoint, Area newArea)
    {
        Require.NotNull(transitionPoint, nameof(transitionPoint));

        CompleteAreaTransition(transitionPoint, newArea);
    }

    /// <summary>
    /// Called after a new area has become the current area.
    /// </summary>
    /// <param name="newArea">The now current area.</param> 
    /// <param name="spawnPoint">The position to place the activator entity.</param>
    protected virtual void OnAreaLoaded(Area newArea, Vector2 spawnPoint)
    {
        Require.NotNull(newArea, nameof(newArea));
        
        TransitionActivator?.Position = spawnPoint;

        SizeF mapSize = newArea.TileMap.Size;
        SizeF tileSize = newArea.TileMap.TileSize;

        Camera.LockToContent(new SizeF(mapSize.Width * tileSize.Width, mapSize.Height * tileSize.Height));
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
