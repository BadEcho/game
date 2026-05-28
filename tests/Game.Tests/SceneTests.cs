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

using System.Diagnostics;
using BadEcho.Game.Scenes;
using Microsoft.Xna.Framework;
using Xunit;

namespace BadEcho.Game.Tests;

/// <suppressions>
/// ReSharper disable AccessToDisposedClosure
/// </suppressions>
public class SceneTests
{
    private const double DRIFT_TOLERANCE = 125;

    [Fact]
    public void AddScene_Default_Entered()
    {
        var game = new TestGame();
        var sceneManager = new SceneManager(game);

        TestGameScene? gameScene = null;

        game.Components.Add(sceneManager);
        game.Initialized += (_, _) =>
        {
            gameScene = new TestGameScene(game);
            sceneManager.AddScene(gameScene);
        };

        game.ExitCondition = () => gameScene?.TransitionStatus == TransitionStatus.Entered;
        game.Run();
        game.Dispose();

        Assert.Contains(gameScene, sceneManager.Scenes);
        Assert.True(gameScene?.TransitionStatus == TransitionStatus.Entered);
    }

    [Theory]
    [InlineData(2.0)]
    [InlineData(1.5)]
    [InlineData(0.5)]
    [InlineData(4)]
    public void AddScene_ConfiguredTransitionTime_TransitionsInTime(double transitionTimeSeconds)
    {
        TimeSpan transitionTime = TimeSpan.FromSeconds(transitionTimeSeconds);
        var stopwatch = new Stopwatch();
        var game = new TestGame();
        var sceneManager = new SceneManager(game);

        TestGameScene? gameScene = null;

        game.Components.Add(sceneManager);
        game.Initialized += (_, _) =>
        {
            gameScene = new TestGameScene(game)
                        {
                            TransitionTime = transitionTime,
                            SceneTransitions = Transitions.Fade | Transitions.MoveLeft | Transitions.Rotate | Transitions.Zoom,
                            PowerCurve = 4
                            
                        };
            stopwatch.Start();
            sceneManager.AddScene(gameScene);
        };

        game.Exiting += (_, _) => stopwatch.Stop();

        game.ExitCondition = () => gameScene?.TransitionStatus == TransitionStatus.Entered;
        game.Run();
        game.Dispose();

        double drift = Math.Abs(stopwatch.Elapsed.Subtract(transitionTime).TotalMilliseconds);

        Assert.True(drift < DRIFT_TOLERANCE, $"Expected Transition Time: {transitionTime} Actual Transition Time: {stopwatch.Elapsed}");
    }

    [Fact]
    public void AddScene_Background_StaysEntered()
    {
        var game = new TestGame();
        var sceneManager = new SceneManager(game);

        TestGameScene? gameScene = null;
        BackgroundScene? backgroundScene = null;

        game.Components.Add(sceneManager);
        game.Initialized += (_, _) =>
        {
            backgroundScene = new BackgroundScene(game, "Images\\Circle")
                              {
                                  SceneTransitions = Transitions.MoveDown | Transitions.Zoom
                              };
            sceneManager.AddScene(backgroundScene);
            gameScene = new TestGameScene(game) {TransitionTime = TimeSpan.FromSeconds(2), SceneTransitions = Transitions.MoveUp};
            sceneManager.AddScene(gameScene);
        };

        game.ExitCondition = () => gameScene?.TransitionStatus == TransitionStatus.Entered;
        game.Run();
        game.Dispose();

        Assert.True(backgroundScene?.TransitionStatus == TransitionStatus.Entered);
    }

    [Fact]
    public void AddScene_TwoDirections_NoMovement()
    {
        var game = new TestGame();
        var sceneManager = new SceneManager(game);

        BackgroundScene? gameScene = null;

        game.Components.Add(sceneManager);
        game.Initialized += (_, _) =>
        {
            gameScene = new BackgroundScene(game, "Images\\Circle")
                              {
                                  SceneTransitions = Transitions.MoveDown | Transitions.MoveRight
                              };
            sceneManager.AddScene(gameScene);
        };

        game.ExitCondition = () => gameScene?.TransitionStatus == TransitionStatus.Entered;
        game.Run();
        game.Dispose();

        Assert.True(gameScene?.TransitionStatus == TransitionStatus.Entered);
    }

    [Fact]
    public void AddScene_TwoScenes_FirstExited()
    {
        var game = new TestGame();
        var sceneManager = new SceneManager(game);

        TestGameScene? firstScene = null;
        TestGameScene? secondScene = null;

        game.Components.Add(sceneManager);
        game.Initialized += (_, _) =>
        {
            firstScene = new TestGameScene(game);
            firstScene.Entered += (_, _) =>
            {
                secondScene = new TestGameScene(game);
                sceneManager.AddScene(secondScene);
            };
            sceneManager.AddScene(firstScene);
        };

        game.ExitCondition = () =>
            secondScene?.TransitionStatus == TransitionStatus.Entered 
            && firstScene?.TransitionStatus == TransitionStatus.Exited;

        game.Run();
        game.Dispose();

        Assert.True(firstScene?.TransitionStatus == TransitionStatus.Exited);
        Assert.True(secondScene?.TransitionStatus == TransitionStatus.Entered);
    }

    [Fact]
    public void AddScene_Screen_Entered()
    {
        var game = new TestGame();
        var sceneManager = new SceneManager(game);

        TestScreenScene? gameScene = null;

        game.Components.Add(sceneManager);
        game.Initialized += (_, _) =>
        {
            gameScene = new TestScreenScene(game)
                        {
                            TransitionTime = TimeSpan.FromSeconds(2), SceneTransitions = Transitions.MoveRight
                        };
            
            sceneManager.AddScene(gameScene);
        };

        game.ExitCondition = () => gameScene?.TransitionStatus == TransitionStatus.Entered;
        game.Run();
        game.Dispose();

        Assert.True(gameScene?.TransitionStatus == TransitionStatus.Entered);
    }

    [Fact]
    public void Close_Default_Exited()
    {
        var game = new TestGame();
        var sceneManager = new SceneManager(game);

        TestGameScene? gameScene = null;
        bool entered = false;

        game.Components.Add(sceneManager);
        game.Initialized += (_, _) =>
        {
            gameScene = new TestGameScene(game);

            gameScene.Entered += (_, _) =>
            {
                entered = true;
                gameScene.Close();
            };

            sceneManager.AddScene(gameScene);
        };

        game.ExitCondition = () => entered && gameScene?.TransitionStatus == TransitionStatus.Exited;
        game.Run();
        game.Dispose();

        Assert.True(gameScene?.TransitionStatus == TransitionStatus.Exited);
    }

    [Fact]
    public void Draw_Uninitialized_ThrowsException()
    {
        var game = new TestGame();
        var sceneManager = new SceneManager(game);

        Assert.Throws<InvalidOperationException>(() => sceneManager.Draw(new GameTime()));
    }

    [Fact]
    public void Dispose_Twice_NoException()
    {
        using var game = new TestGame();
        var gameScene = new TestGameScene(game);

        gameScene.Dispose();
        gameScene.Dispose();
    }
}
