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
/// <para>
/// MonoGame's game loop never establishes a <see cref="SynchronizationContext"/>, which means a continuation attached
/// to a background task will run on the thread pool, not the game thread, regardless of where it was attached. Rather
/// than attempting to marshal work back to a context that does not exist, the direction is inverted: the game thread,
/// which is already running an update every frame, polls the task via <see cref="Update"/>.
/// </para>
/// <para>
/// The typical consumer of this type is a loading screen. A game derives a scene from <c>ScreenScene</c>, constructs a
/// worker around the action that loads the content, starts it when the scene loads, and then calls <see cref="Update"/>
/// during each update of the scene. Once <see cref="Update"/> returns true, the load is complete and the scene may close
/// itself and hand the loaded content over to whatever awaits it.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// protected override void UpdateCore(GameUpdateTime time, bool isActive)
/// {
///     if (TransitionStatus != TransitionStatus.Entered)
///         return;
///
///     if (!_worker.IsStarted)
///         _worker.Start();
///     else if (_worker.Update())
///         Close();
/// }
/// </code>
/// </example>
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
    /// by the action is rethrown from here, ensuring it surfaces on the game thread instead of being swallowed.
    /// </remarks>
    public bool Update()
    {
        if (_execution == null || IsFinished)
            return IsFinished;

        // MonoGame does not establish a SynchronizationContext, so we poll for task completion, allowing for the "continuation" to be run on the game thread.
        if (!_execution.IsCompleted)
            return false;

        // This will force any faults to be rethrown here on the game thread, so they don't end up swallowed and gone forever!
        _execution.GetAwaiter().GetResult();

        IsFinished = true;

        Finished?.Invoke(this, EventArgs.Empty);

        return true;
    }
}
