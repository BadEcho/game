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

using BadEcho.Game.UI;
using BadEcho.Game.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Scenes;

/// <summary>
/// Provides a game scene for hosting core gameplay.
/// </summary>
public abstract class GameplayScene : GameScene
{
    private readonly Brush _pauseOverlay = new(Color.Black);

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameplayScene"/> class.
    /// </summary>
    /// <param name="game">The game this scene is for.</param>
    protected GameplayScene(Microsoft.Xna.Framework.Game game)
        : base(game)
    { }

    /// <summary>
    /// Gets a value indicating if gameplay is paused.
    /// </summary>
    public bool IsPaused
    { get; protected set; }

    /// <summary>
    /// Gets or sets the transparency of the overlay that appears when the game is paused.
    /// </summary>
    public float PauseOverlayAlpha
    { get; set; } = 0.5f;

    /// <inheritdoc/>
    protected override bool AlwaysDisplay 
        => true;

    /// <inheritdoc/>
    protected override void UpdateCore(GameUpdateTime time, bool isActive)
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
            _pauseOverlay.Color = Color.Black * PauseOverlayAlpha;
            _pauseOverlay.Draw(spriteBatch, spriteBatch.GraphicsDevice.Viewport.Bounds);
            spriteBatch.End();
        }
    }

    /// <summary>
    /// Executes custom gameplay-specific update logic.
    /// </summary>
    /// <param name="time">The game timing configuration and scene for this update.</param>
    protected abstract void UpdateGameplay(GameUpdateTime time);

    /// <summary>
    /// Executes the custom rendering logic required to draw the gameplay to the screen.
    /// </summary>
    /// <param name="spriteBatch">A sprite batch for drawing the scene.</param>
    protected abstract void DrawGameplay(SpriteBatch spriteBatch);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _pauseOverlay.Dispose();

            _disposed = true;
        }

        base.Dispose(disposing);
    }
}
