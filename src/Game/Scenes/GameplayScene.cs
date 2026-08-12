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
public class GameplayScene : GameScene
{
    private readonly WorkerScene _loadingScene;
    private readonly DeferredRenderer _renderer;

    private Area? _loadedArea;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameplayScene"/> class.
    /// </summary>
    /// <param name="game">The game this scene is for.</param>
    /// <param name="loadingScene">The loading <see cref="WorkerScene"/> to use when loading assets and areas.</param>
    public GameplayScene(Microsoft.Xna.Framework.Game game, WorkerScene loadingScene)
        : base(game)
    {
        _loadingScene = loadingScene;
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
    [MemberNotNullWhen(true, nameof(_loadedArea))]
    public bool IsAreaLoaded
        => _loadedArea != null;

    /// <summary>
    /// Gets or sets the transparency of the overlay that appears when the game is paused.
    /// </summary>
    public float PauseOverlayAlpha
    { get; set; } = 0.5f;

    /// <inheritdoc/>
    protected override bool AlwaysDisplay 
        => true;

    /// <summary>
    /// Loads the named area into the scene.
    /// </summary>
    /// <param name="areaName">The name of the area to load.</param>
    public void LoadArea(string areaName)
    {
        _loadingScene.Execute(() => _loadedArea = Content.Load<Area>(areaName));
    }

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

    /// <summary>
    /// Executes custom gameplay-specific update logic.
    /// </summary>
    /// <param name="time">The game timing configuration and scene for this update.</param>
    protected virtual void UpdateGameplay(GameUpdateTime time)
    {
        if (!IsAreaLoaded)
            return;

        _loadedArea.Update(time);
    }

    /// <summary>
    /// Executes the custom rendering logic required to draw the gameplay to the screen.
    /// </summary>
    /// <param name="spriteBatch">A sprite batch for drawing the scene.</param>
    protected virtual void DrawGameplay(SpriteBatch spriteBatch)
    {
        if (!IsAreaLoaded)
            return;

        _loadedArea.Draw(spriteBatch, _renderer, RenderStates);
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
