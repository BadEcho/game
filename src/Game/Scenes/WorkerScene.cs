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
/// Provides a scene that runs an operation on a background thread and then exits.
/// </summary>
public class WorkerScene : GameScene
{
    private readonly Action _action;
    private Task? _execution;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerScene"/> class.
    /// </summary>
    /// <param name="game">The game this scene is for.</param>
    /// <param name="action">The action to execute once this scene loads.</param>
    public WorkerScene(Microsoft.Xna.Framework.Game game, Action action)
        : base(game)
    {
        _action = action;
    }

    /// <inheritdoc/>
    protected override void UpdateCore(GameUpdateTime time, bool isActive)
    {
        if (TransitionStatus != TransitionStatus.Entered)
            return;

        if (_execution == null)
        {
            _execution = Task.Run(_action);
            return;
        }

        // MonoGame does not establish a SynchronizationContext, so we poll for task completion, allowing for the "continuation" to be run on the game thread.
        if (!_execution.IsCompleted)
            return;

        // This will force any faults to be rethrown here on the game thread, so they don't end up swallowed and gone forever!
        _execution.GetAwaiter().GetResult();

        Close();
    }

    /// <inheritdoc/>
    protected override void DrawCore(SpriteBatch spriteBatch)
    { }
}
