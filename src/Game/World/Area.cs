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

using BadEcho.Game.Effects;
using BadEcho.Game.Lighting;
using BadEcho.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.World;

/// <summary>
/// Provides a self-contained, playable region of the game world, composed of a tile map, a population of
/// sprite actors, and light sources, all wired to a dedicated collision engine.
/// </summary>
public sealed class Area : IDisposable
{
    private readonly List<Sprite> _actors = [];
    private readonly List<ILight> _lights = [];
    private readonly CollisionEngine _collisionEngine;
    private readonly TileMap _tileMap;

    private bool _collidersRegistered;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Area"/> class.
    /// </summary>
    /// <param name="tileMap">The tile map defining the layout of this area.</param>
    public Area(TileMap tileMap)
    {
        Require.NotNull(tileMap, nameof(tileMap));

        _tileMap = tileMap;

        var bounds = new RectangleF(PointF.Empty,
                                    new SizeF(tileMap.Size.Width * tileMap.TileSize.Width,
                                              tileMap.Size.Height * tileMap.TileSize.Height));

        _collisionEngine = new CollisionEngine(bounds);
    }

    /// <summary>
    /// Gets the tile map defining the layout of this area.
    /// </summary>
    public TileMap TileMap
        => _tileMap;

    /// <summary>
    /// Gets the size of this area, measured in pixels.
    /// </summary>
    public SizeF Size
        => new(_tileMap.Size.Width * _tileMap.TileSize.Width,
               _tileMap.Size.Height * _tileMap.TileSize.Height);

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
    /// Adds a sprite actor to this area, registering its collider with the area's collision engine.
    /// </summary>
    /// <param name="actor">The sprite actor to add to this area.</param>
    public void AddActor(Sprite actor)
    {
        Require.NotNull(actor, nameof(actor));

        _actors.Add(actor);

        if (_collidersRegistered)
            _collisionEngine.Register(actor.Collider);
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
    /// Wires all of this area's colliders, including those of its tile map and actors, to its collision engine.
    /// </summary>
    public void Load()
    {
        if (_collidersRegistered)
            return;

        foreach (Collider tileCollider in _tileMap.ToCollidableMap())
        {
            _collisionEngine.Register(tileCollider);
        }

        foreach (Sprite actor in _actors)
        {
            _collisionEngine.Register(actor.Collider);
        }

        _collidersRegistered = true;
    }

    /// <summary>
    /// Advances the state of this area's tile map and actors by one tick, then processes collisions.
    /// </summary>
    /// <param name="time">The game timing configuration and state for this update.</param>
    public void Update(GameUpdateTime time)
    {
        Require.NotNull(time, nameof(time));

        _tileMap.Update(time);

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
    /// <param name="view">The camera view matrix mapping world-space to view-space.</param>
    public void Draw(SpriteBatch spriteBatch, DeferredRenderer renderer, RenderStates renderStates, Matrix view)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));
        Require.NotNull(renderer, nameof(renderer));
        Require.NotNull(renderStates, nameof(renderStates));

        RenderStates worldStates = renderStates with { MatrixTransform = view };

        // Color + normal phase: tile map and actors.
        renderer.StartColorPhase(worldStates);

        _tileMap.Draw(view);

        spriteBatch.Begin(worldStates);

        foreach (Sprite actor in _actors)
        {
            actor.Draw(spriteBatch);
        }

        spriteBatch.End();

        // Light + shadow phase.
        renderer.DrawLights(spriteBatch, worldStates, _lights, _actors);

        // Composite to screen.
        renderer.Finish();
        renderer.DrawComposite(spriteBatch, AmbientLight);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _collisionEngine.UnregisterAll();
        _tileMap.Dispose();

        _disposed = true;
    }
}
