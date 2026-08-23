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

using Xunit;

namespace BadEcho.Game.Tests;

public class DeferredWorkerTests
{
    private static readonly TimeSpan _Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public void Constructor_NullAction_ThrowsException()
        => Assert.Throws<ArgumentNullException>(() => new DeferredWorker(null!));

    [Fact]
    public void Update_NotStarted_ReturnsFalse()
    {
        var worker = new DeferredWorker(() => { });

        Assert.False(worker.Update());
        Assert.False(worker.IsStarted);
        Assert.False(worker.IsFinished);
    }

    [Fact]
    public void Start_Twice_ThrowsException()
    {
        var worker = new DeferredWorker(() => { });

        worker.Start();

        Assert.True(worker.IsStarted);
        Assert.Throws<InvalidOperationException>(worker.Start);
    }

    [Fact]
    public void Update_StartedAction_ExecutesAction()
    {
        using var actionRan = new ManualResetEventSlim();

        var worker = new DeferredWorker(actionRan.Set);

        worker.Start();

        Assert.True(actionRan.Wait(_Timeout));
        Assert.True(SpinUntilFinished(worker));
        Assert.True(worker.IsFinished);
    }

    [Fact]
    public void Update_ActionCompleted_RaisesFinishedOnce()
    {
        using var actionRan = new ManualResetEventSlim();

        var worker = new DeferredWorker(actionRan.Set);
        int finishedCount = 0;

        worker.Finished += (_, _) => finishedCount++;
        worker.Start();

        Assert.True(actionRan.Wait(_Timeout));

        // The event belongs to the polling of the task, not to the task itself; nothing is raised until we ask for it.
        Assert.Equal(0, finishedCount);
        Assert.True(SpinUntilFinished(worker));
        Assert.Equal(1, finishedCount);

        // Polling an already-observed worker is idempotent.
        Assert.True(worker.Update());
        Assert.True(worker.Update());
        Assert.Equal(1, finishedCount);
    }

    [Fact]
    public void Update_FaultedAction_RethrowsException()
    {
        var worker = new DeferredWorker(() => throw new InvalidOperationException("Faulted."));

        worker.Start();

        InvalidOperationException exception
            = Assert.Throws<InvalidOperationException>(() => SpinUntilFinished(worker));

        Assert.Equal("Faulted.", exception.Message);
    }

    [Fact]
    public void Update_FaultedAction_FinishesWithoutRethrowingAgain()
    {
        var worker = new DeferredWorker(() => throw new InvalidOperationException("Faulted."));
        int finishedCount = 0;

        worker.Finished += (_, _) => finishedCount++;
        worker.Start();

        Assert.Throws<InvalidOperationException>(() => SpinUntilFinished(worker));

        // The fault belongs to the poll that observed it; the worker is finished either way, so later polls are quiet.
        Assert.True(worker.IsFinished);
        Assert.True(worker.Update());
        Assert.True(worker.Update());
        Assert.Equal(0, finishedCount);
    }

    private static bool SpinUntilFinished(DeferredWorker worker)
        => SpinWait.SpinUntil(worker.Update, _Timeout);
}
