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
using BadEcho.Game.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.UI;

/// <summary>
/// Provides a self-contained user interface game scene that loads and deploys packaged controls onto a provided
/// <see cref="Screen"/> instance.
/// </summary>
public abstract class ScreenScene : GameScene
{
    private readonly Screen _screen;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenScene"/> class.
    /// </summary>
    /// <param name="game">The game this scene is for.</param>
    protected ScreenScene(Microsoft.Xna.Framework.Game game)
        : base(game)
    {
        _screen = new Screen(game.GraphicsDevice);

        RenderStates = RenderStates with { SortMode = SpriteSortMode.Immediate };
    }

    /// <inheritdoc/>
    protected override bool ClipDuringTransitions
        => false;

    /// <inheritdoc/>
    protected override void UpdateCore(GameUpdateTime time, bool isActive)
    {
        _screen.Update();

        ContentOrigin = _screen.Content.LayoutBounds.Location;
    }

    /// <inheritdoc/>
    protected override void DrawCore(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(RenderStates);

        _screen.Draw(spriteBatch);

        spriteBatch.End();

        _screen.DrawPrimitives(RenderStates.Effect);
    }

    /// <inheritdoc/>
    protected override void OnLoad(SceneManager manager)
    {
        _screen.Content = LoadControls(manager);

        base.OnLoad(manager);
    }

    /// <summary>
    /// Initializes and returns a layout panel containing this user interface's controls.
    /// </summary>
    /// <param name="manager">The scene manager this scene is being loaded into.</param>
    /// <returns>An <see cref="IPanel"/> instance containing this user interface's controls.</returns>
    protected abstract IPanel LoadControls(SceneManager manager);
}