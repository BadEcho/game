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

using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Scenes;

/// <summary>
/// Provides a scene that runs an operation on a background thread.
/// </summary>
public class WorkerScene : GameScene
{
    private Action? _action;
    private bool _isRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerScene"/> class.
    /// </summary>
    /// <param name="game">The game this scene is for.</param>
    protected WorkerScene(Microsoft.Xna.Framework.Game game)
        : base(game)
    { }

    /// <summary>
    /// Executes the action in the background once this scene has transitioned onto the screen.
    /// </summary>
    /// <param name="action">The action to execute in the background.</param>
    public void Execute(Action action)
    {
        _isRunning = false;
        _action = action;
    }

    /// <inheritdoc/>
    protected override void UpdateCore(GameUpdateTime time, bool isActive)
    {
        if (TransitionStatus != TransitionStatus.Entered || _action == null || _isRunning)
            return;

        _isRunning = true;

        Task.Run(_action).ContinueWith(UnloadSelf,
                                       CancellationToken.None,
                                       TaskContinuationOptions.None,
                                       TaskScheduler.Current);
    }

    /// <inheritdoc/>
    protected override void DrawCore(SpriteBatch spriteBatch)
    { }

    private void UnloadSelf(Task task)
    {
        _action = null;
        _isRunning = false;
        Manager?.RemoveScene(this);
    }
}
