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
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.World;

/// <summary>
/// Provides a self-contained, playable region of the game world, composed of a tile map, a population of
/// sprite actors, and light sources, all wired to a dedicated collision engine.
/// </summary>
public sealed class Area : IDisposable
{
    private readonly List<Sprite> _actors = [];
    private readonly List<Sprite> _actorsWithNormals = [];
    private readonly List<Sprite> _actorsWithShadows = [];
    private readonly List<ILight> _lights = [];
    private readonly CollisionEngine _collisionEngine;

    private bool _disposed;

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

        PresentationParameters parameters = device.PresentationParameters;

        var bounds = new RectangleF(new PointF(-parameters.BackBufferWidth, -parameters.BackBufferHeight),
                                    new SizeF(parameters.BackBufferWidth * 2,
                                              parameters.BackBufferHeight * 2));

        _collisionEngine = new CollisionEngine(bounds);

        foreach (Collider tileCollider in TileMap.ToCollidableMap())
        {
            _collisionEngine.Register(tileCollider);
        }
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
    /// Adds a sprite actor to this area.
    /// </summary>
    /// <param name="actor">The sprite actor to add to this area.</param>
    /// <param name="isShadowless">Value indicating if the actor does not cast a shadow.</param>
    public void AddActor(Sprite actor, bool isShadowless)
    {
        Require.NotNull(actor, nameof(actor));

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

        _actors.Remove(actor);
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

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _collisionEngine.UnregisterAll();
        TileMap.Dispose();

        _disposed = true;
    }
}
