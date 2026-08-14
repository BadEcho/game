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
using BadEcho.Game.Effects;
using BadEcho.Game.World;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Scenes;

/// <summary>
/// Provides a game scene for hosting core gameplay.
/// </summary>
public abstract class GameplayScene : GameScene
{
    private readonly List<Area> _areas = [];
    private readonly DeferredRenderer _renderer;

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

    /// <inheritdoc/>
    protected override bool AlwaysDisplay 
        => true;

    /// <summary>
    /// Gets the collection of loaded areas.
    /// </summary>
    protected IReadOnlyCollection<Area> Areas
        => _areas;

    /// <summary>
    /// Loads all areas associated with this scene.
    /// </summary>
    /// <returns>The <see cref="Area"/> instances associated with this scene.</returns>
    protected abstract IEnumerable<Area> LoadAreas();

    /// <inheritdoc/>
    protected sealed override void UpdateCore(GameUpdateTime time, bool isActive)
    {
        IsPaused = !isActive;

        if (!IsPaused)
            UpdateGameplay(time);
    }

    /// <inheritdoc/>
    protected sealed override void DrawCore(SpriteBatch spriteBatch)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));        

        DrawGameplay(spriteBatch);

        if (IsPaused)
        {
            spriteBatch.Begin(RenderStates);
            spriteBatch.End();
        }
    }

    /// <inheritdoc/>
    protected override void OnLoad(SceneManager manager)
    {
        _areas.AddRange(LoadAreas());

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
}
