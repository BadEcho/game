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

using BadEcho.Game.Properties;

namespace BadEcho.Game;

/// <summary>
/// Provides an executor of an action on a background thread whose completion is observed by polling from the game
/// thread, allowing for continuations to be run on the game thread.
/// </summary>
/// <remarks>
/// MonoGame's game loop never establishes a <see cref="SynchronizationContext"/>, which means a continuation attached
/// to a background task will run on the thread pool, not the game thread, regardless of where it was attached. Rather
/// than attempting to marshal work back to a context that does not exist, the direction is inverted: the game thread,
/// which is already running an update every frame, polls the task via <see cref="Update"/>.
/// </remarks>
public sealed class DeferredWorker
{
    private readonly Action _action;

    private Task? _execution;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredWorker"/> class.
    /// </summary>
    /// <param name="action">The action to execute on a background thread.</param>
    public DeferredWorker(Action action)
    {
        Require.NotNull(action, nameof(action));

        _action = action;
    }

    /// <summary>
    /// Occurs when this worker's action has completed, raised from <see cref="Update"/> on the game thread.
    /// </summary>
    public event EventHandler? Finished;

    /// <summary>
    /// Gets a value indicating if this worker's action has been dispatched to a background thread.
    /// </summary>
    public bool IsStarted
        => _execution != null;

    /// <summary>
    /// Gets a value indicating if this worker's action has completed and had its completion observed by the game thread.
    /// </summary>
    /// <remarks>
    /// An action that faulted counts as completed; its fault is rethrown by the <see cref="Update"/> that observes it,
    /// and only by that one.
    /// </remarks>
    public bool IsFinished
    { get; private set; }

    /// <summary>
    /// Dispatches this worker's action to a background thread.
    /// </summary>
    /// <exception cref="InvalidOperationException">This worker has already been started.</exception>
    public void Start()
    {
        if (_execution != null)
            throw new InvalidOperationException(Strings.WorkerAlreadyStarted);

        _execution = Task.Run(_action);
    }

    /// <summary>
    /// Polls this worker's action for completion, raising <see cref="Finished"/> on the calling thread if it has just
    /// completed.
    /// </summary>
    /// <returns>True if this worker's action has completed; otherwise, false.</returns>
    /// <remarks>
    /// This is meant to be called from the game thread during each update, and will never block. Any fault experienced
    /// by the action is rethrown from here, ensuring it surfaces on the game thread instead of being swallowed. A faulted
    /// action is finished all the same, so its fault is rethrown once and <see cref="Finished"/> is never raised for it.
    /// </remarks>
    public bool Update()
    {
        if (_execution == null || IsFinished)
            return IsFinished;

        // MonoGame does not establish a SynchronizationContext, so we poll for task completion, allowing for the "continuation" to be run on the game thread.
        if (!_execution.IsCompleted)
            return false;

        // Marked as finished ahead of the rethrow below, so that a faulted action is observed exactly once rather than
        // faulting every later poll and never raising anything.
        IsFinished = true;

        // This will force any faults to be rethrown here on the game thread, so they don't end up swallowed and gone forever!
        _execution.GetAwaiter().GetResult();

        Finished?.Invoke(this, EventArgs.Empty);

        return true;
    }
}
