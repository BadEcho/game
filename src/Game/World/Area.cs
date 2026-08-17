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

using BadEcho.Extensions;
using BadEcho.Game.Effects;
using BadEcho.Game.Lighting;
using BadEcho.Game.Properties;
using BadEcho.Game.Tiles;
using BadEcho.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.World;

/// <summary>
/// Provides a self-contained, playable region of the game world, composed of a tile map, a population of
/// sprite actors, and light sources, all wired to a dedicated collision engine.
/// </summary>
public class Area
{
    private readonly List<Sprite> _actors = [];
    private readonly List<Sprite> _actorsWithNormals = [];
    private readonly List<Sprite> _actorsWithShadows = [];
    private readonly List<ILight> _lights = [];
    private readonly List<TransitionPoint> _transitionPoints = [];
    private readonly CollisionEngine _collisionEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="Area"/> class.
    /// </summary>
    /// <param name="device">The graphics device used for rendering.</param>
    /// <param name="tileMap">The tile map defining the layout of this area.</param>
    public Area(GraphicsDevice device, TileMap tileMap)
    {
        Require.NotNull(device, nameof(device));
        Require.NotNull(tileMap, nameof(tileMap));
        
        TileMap = tileMap;

        var bounds = new RectangleF(-Size, Size * 2);

        _collisionEngine = new CollisionEngine(bounds);

        foreach (Collider tileCollider in TileMap.ToCollidableMap())
        {
            _collisionEngine.Register(tileCollider);
        }

        FoldTransitionPoints();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Area"/> class.
    /// </summary>
    /// <param name="source">The <see cref="Area"/> instance to source common properties from.</param>
    protected Area(Area source)
    {
        Require.NotNull(source, nameof(source));
        
        TileMap = source.TileMap;
        _collisionEngine = source._collisionEngine;
        
        _actors.AddRange(source.Actors);
        _actorsWithNormals.AddRange(source._actorsWithNormals);
        _actorsWithShadows.AddRange(source._actorsWithShadows);
        _lights.AddRange(source.Lights);
        _transitionPoints.AddRange(source.TransitionPoints);

        AmbientLight = source.AmbientLight;
        Name = source.Name;
    }

    /// <summary>
    /// Gets the tile map defining the layout of this area.
    /// </summary>
    public TileMap TileMap 
    { get; }

    /// <summary>
    /// Gets or sets the human-readable name for the area.
    /// </summary>
    public string Name
    { get; set; } = string.Empty;

    /// <summary>
    /// Gets the size of this area, measured in pixels.
    /// </summary>
    public SizeF Size
        => new(TileMap.Size.Width * TileMap.TileSize.Width,
               TileMap.Size.Height * TileMap.TileSize.Height);

    /// <summary>
    /// Gets or sets the amount of ambient light applied to this area when its composite image is drawn.
    /// </summary>
    public float AmbientLight
    { get; set; } = 1.0f;

    /// <summary>
    /// Gets the collection of sprite actors populating this area.
    /// </summary>
    public IReadOnlyCollection<Sprite> Actors
        => _actors;

    /// <summary>
    /// Gets the collection of light sources illuminating this area.
    /// </summary>
    public IReadOnlyCollection<ILight> Lights
        => _lights;

    /// <summary>
    /// Gets the collection of transition points that, when entered, trigger a transition out of this area.
    /// </summary>
    /// <remarks>
    /// Transition points authored on this area's tile map are folded into this collection automatically; further points may
    /// be added and removed at runtime. Because a point's geometry is permanent, a map-authored doorway is normally taken out
    /// of service by clearing its <see cref="TransitionPoint.IsEnabled"/> property rather than by removing it.
    /// </remarks>
    public IReadOnlyCollection<TransitionPoint> TransitionPoints
        => _transitionPoints;

    /// <summary>
    /// Adds a sprite actor to this area.
    /// </summary>
    /// <param name="actor">The sprite actor to add to this area.</param>
    /// <param name="isShadowless">Value indicating if the actor does not cast a shadow.</param>
    public void AddActor(Sprite actor, bool isShadowless)
    {
        Require.NotNull(actor, nameof(actor));

        if (_actors.Contains(actor))
            return;

        _actors.Add(actor);

        if (actor.NormalMap != null)
            _actorsWithNormals.Add(actor);

        if (!isShadowless)
            _actorsWithShadows.Add(actor);

        _collisionEngine.Register(actor.Collider);
    }

    /// <summary>
    /// Removes a sprite actor from this area.
    /// </summary>
    /// <param name="actor">The sprite actor to remove from this area.</param>
    public void RemoveActor(Sprite actor)
    {
        Require.NotNull(actor, nameof(actor));

        if (!_actors.Remove(actor))
            return;

        _actorsWithNormals.Remove(actor);
        _actorsWithShadows.Remove(actor);

        _collisionEngine.Unregister(actor.Collider);
    }

    /// <summary>
    /// Adds a light source to this area.
    /// </summary>
    /// <param name="light">The light source to add to this area.</param>
    public void AddLight(ILight light)
    {
        Require.NotNull(light, nameof(light));

        _lights.Add(light);
    }

    /// <summary>
    /// Adds a transition point to this area.
    /// </summary>
    /// <param name="transitionPoint">The transition point to add to this area.</param>
    public void AddTransitionPoint(TransitionPoint transitionPoint)
    {
        Require.NotNull(transitionPoint, nameof(transitionPoint));

        _transitionPoints.Add(transitionPoint);
    }

    /// <summary>
    /// Removes a transition point from this area.
    /// </summary>
    /// <param name="transitionPoint">The transition point to remove from this area.</param>
    public void RemoveTransitionPoint(TransitionPoint transitionPoint)
    {
        Require.NotNull(transitionPoint, nameof(transitionPoint));

        _transitionPoints.Remove(transitionPoint);
    }

    /// <summary>
    /// Finds the transition point in this area entered by an entity occupying the specified bounds.
    /// </summary>
    /// <param name="entityBounds">The spatial bounds of the entity to check.</param>
    /// <returns>
    /// The first <see cref="TransitionPoint"/> in this area entered by an entity occupying <c>entityBounds</c>, or null if
    /// no such transition point exists.
    /// </returns>
    /// <remarks>Disabled transition points are never considered entered, and so are inherently skipped.</remarks>
    public TransitionPoint? FindEnteredTransitionPoint(IShape entityBounds)
    {
        Require.NotNull(entityBounds, nameof(entityBounds));

        return _transitionPoints.FirstOrDefault(t => t.IsEntered(entityBounds));
    }

    /// <summary>
    /// Advances the state of this area's tile map and actors by one tick, then processes collisions.
    /// </summary>
    /// <param name="time">The game timing configuration and state for this update.</param>
    public void Update(GameUpdateTime time)
    {
        Require.NotNull(time, nameof(time));

        TileMap.Update(time);

        foreach (Sprite actor in _actors)
        {
            actor.Update(time);
        }

        _collisionEngine.Update();
    }

    /// <summary>
    /// Draws this area to the screen using the provided deferred renderer and camera view.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to use to draw the area.</param>
    /// <param name="renderer">The deferred renderer orchestrating the drawing phases.</param>
    /// <param name="renderStates">Device render states to use when drawing.</param>
    public void Draw(SpriteBatch spriteBatch, DeferredRenderer renderer, RenderStates renderStates)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));
        Require.NotNull(renderer, nameof(renderer));
        Require.NotNull(renderStates, nameof(renderStates));
        
        // Color + normal phase: tile map and actors.
        StandardEffect effect = renderer.StartColorPhase(renderStates);

        TileMap.Draw(renderStates.MatrixTransform);

        spriteBatch.Begin(renderStates, effect);

        foreach (Sprite actor in _actors)
        {
            actor.Draw(spriteBatch);
        }

        spriteBatch.End();

        StandardEffect normalEffect = renderer.StartNormalPhase(renderStates);
        spriteBatch.Begin(renderStates, normalEffect);

        foreach (Sprite actor in _actorsWithNormals)
        {
            actor.DrawNormals(spriteBatch);
        }

        spriteBatch.End();

        // Light + shadow phase.
        renderer.DrawLights(spriteBatch, renderStates, _lights, _actorsWithShadows);

        // Composite to screen.
        renderer.Finish();
        renderer.DrawComposite(spriteBatch, AmbientLight);
    }

    private static TransitionPoint? CreateTransitionPoint(MapObject mapObject)
    {
        CustomProperties properties = mapObject.CustomProperties;

        if (!properties.Strings.TryGetValue(KnownProperties.TargetAreaName, out string? targetAreaName)
            || string.IsNullOrEmpty(targetAreaName))
        {   // The tile map pipeline rejects transition point objects lacking a target area, however handcrafted content, or
            // content built before that validation existed, can still reach us. We warn and move on instead of throwing.
            Logger.Warning(
                Strings.TransitionObjectNoTargetAreaName.InvariantFormat(mapObject.Name, KnownProperties.TargetAreaName));

            return null;
        }

        if (!properties.Strings.TryGetValue(KnownProperties.TargetPointName, out string? targetPointName))
            targetPointName = string.Empty;

        if (!properties.Booleans.TryGetValue(KnownProperties.Enabled, out bool isEnabled))
            isEnabled = true;

        TransitionPoint transitionPoint
            = mapObject.IsPoint
                ? new TransitionPoint(targetAreaName, mapObject.Location)
                  {
                      Name = mapObject.Name, TargetPointName = targetPointName
                  }
                : new TransitionPoint(targetAreaName, mapObject.Bounds)
                  {
                      Name = mapObject.Name, TargetPointName = targetPointName
                  };

        transitionPoint.IsEnabled = isEnabled;

        return transitionPoint;
    }

    /// <summary>
    /// Folds the transition points authored on this area's tile map into this area's collection of transition points.
    /// </summary>
    /// <remarks>
    /// This happens here, at runtime, rather than during the processing of the area's own asset, because the area processor
    /// holds nothing more than an external reference to the tile map, and reparsing the map from there would violate the
    /// layering between the two assets. Folding from the primary constructor means both content-loaded areas and ones built
    /// in code get their map's transition points; the copy constructor copies the folded collection instead, so no area is
    /// ever folded twice.
    /// </remarks>
    private void FoldTransitionPoints()
    {
        IEnumerable<MapObject> mapObjects
            = TileMap.Layers.OfType<ObjectLayer>().SelectMany(l => l.Objects);

        foreach (MapObject mapObject in mapObjects)
        {
            if (!mapObject.Type.Equals(KnownObjectTypes.TransitionPoint, StringComparison.OrdinalIgnoreCase))
                continue;

            TransitionPoint? transitionPoint = CreateTransitionPoint(mapObject);

            if (transitionPoint != null)
                _transitionPoints.Add(transitionPoint);
        }
    }
}
