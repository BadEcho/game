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

using System.Runtime.ExceptionServices;
using BadEcho.Game.Scenes;
using BadEcho.Game.Tiles;
using BadEcho.Game.World;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace BadEcho.Game.Tests;

/// <suppressions>
/// ReSharper disable AccessToDisposedClosure
/// </suppressions>
public class GameplaySceneTests
{
    private static readonly RectangleF _Doorway = new(0, 0, 16, 16);
    private static readonly RectangleF _InsideDoorway = new(4, 4, 8, 8);
    private static readonly RectangleF _AwayFromDoorway = new(100, 100, 8, 8);

    [Fact]
    public void OnLoad_DuplicateAreaNames_ThrowsException()
        => RunTest((game, device) =>
        {
            using var scene = new TestGameplayScene(game, CreateArea(device, "Field"), CreateArea(device, "FIELD"));

            Assert.Throws<InvalidOperationException>(() => scene.Load(new SceneManager(game)));
        });

    [Fact]
    public void UpdateGameplay_ActivatorEntersPoint_SwitchesArea()
        => RunTest((game, device) =>
        {
            Area field = CreateArea(device, "Field");
            Area cave = CreateArea(device, "Cave");

            field.AddTransitionPoint(new TransitionPoint("Cave", _Doorway) { Name = "NorthDoor" });

            var activator = new SpatialStub(_AwayFromDoorway);
            using TestGameplayScene scene = LoadScene(game, activator, field, cave);

            scene.Tick();

            activator.Bounds = _InsideDoorway;
            scene.Tick();

            Assert.Same(cave, scene.CurrentArea);
            Assert.Same(field, scene.PreviousArea);
            Assert.Equal(1, scene.TransitionedCount);
            Assert.False(scene.IsTransitioningAreas);
        });

    [Fact]
    public void UpdateGameplay_NoActivator_NoTransition()
        => RunTest((game, device) =>
        {
            Area field = CreateArea(device, "Field");
            Area cave = CreateArea(device, "Cave");

            field.AddTransitionPoint(new TransitionPoint("Cave", _Doorway));

            using TestGameplayScene scene = LoadScene(game, null, field, cave);

            scene.Tick();
            scene.Tick();

            Assert.Same(field, scene.CurrentArea);
            Assert.Equal(0, scene.TransitionedCount);
        });

    [Fact]
    public void UpdateGameplay_DisabledPoint_NoTransition()
        => RunTest((game, device) =>
        {
            Area field = CreateArea(device, "Field");
            Area cave = CreateArea(device, "Cave");

            field.AddTransitionPoint(new TransitionPoint("Cave", _Doorway) { IsEnabled = false });

            var activator = new SpatialStub(_InsideDoorway);
            using TestGameplayScene scene = LoadScene(game, activator, field, cave);

            // A disabled point is invisible to detection, so the guard arms even though the activator stands on the doorway.
            scene.Tick();
            scene.Tick();

            Assert.Same(field, scene.CurrentArea);
            Assert.Equal(0, scene.TransitionedCount);
        });

    [Fact]
    public void UpdateGameplay_ActivatorStartsOnPoint_NoTransitionUntilItLeaves()
        => RunTest((game, device) =>
        {
            Area field = CreateArea(device, "Field");
            Area cave = CreateArea(device, "Cave");

            field.AddTransitionPoint(new TransitionPoint("Cave", _Doorway));

            var activator = new SpatialStub(_InsideDoorway);
            using TestGameplayScene scene = LoadScene(game, activator, field, cave);

            // Detection starts disarmed, so a game start or restored save placing the activator on a doorway is safe.
            scene.Tick();
            scene.Tick();

            Assert.Same(field, scene.CurrentArea);

            activator.Bounds = _AwayFromDoorway;
            scene.Tick();

            activator.Bounds = _InsideDoorway;
            scene.Tick();

            Assert.Same(cave, scene.CurrentArea);
        });

    [Fact]
    public void UpdateGameplay_SpawnedOnReturnDoorway_NoImmediateReturn()
        => RunTest((game, device) =>
        {
            Area field = CreateArea(device, "Field");
            Area cave = CreateArea(device, "Cave");

            field.AddTransitionPoint(new TransitionPoint("Cave", _Doorway) { Name = "NorthDoor", TargetPointName = "SouthDoor" });
            cave.AddTransitionPoint(new TransitionPoint("Field", _Doorway) { Name = "SouthDoor", TargetPointName = "NorthDoor" });

            var activator = new SpatialStub(_AwayFromDoorway);
            using TestGameplayScene scene = LoadScene(game, activator, field, cave);

            scene.Tick();

            activator.Bounds = _InsideDoorway;
            scene.Tick();

            Assert.Same(cave, scene.CurrentArea);

            // The activator now stands on the doorway leading straight back, which must not fire until it steps off.
            scene.Tick();
            scene.Tick();

            Assert.Same(cave, scene.CurrentArea);
            Assert.Equal(1, scene.TransitionedCount);

            activator.Bounds = _AwayFromDoorway;
            scene.Tick();

            activator.Bounds = _InsideDoorway;
            scene.Tick();

            Assert.Same(field, scene.CurrentArea);
            Assert.Equal(2, scene.TransitionedCount);
        });

    [Fact]
    public void CompleteAreaTransition_UnknownAreaName_ThrowsException()
        => RunTest((game, device) =>
        {
            using TestGameplayScene scene = LoadScene(game, null, CreateArea(device, "Field"));

            Assert.Throws<ArgumentException>(() => scene.Complete("Atlantis"));
        });

    [Fact]
    public void CompleteAreaTransition_DeferredLoader_LoadsAndSwitches()
        => RunTest((game, device) =>
        {
            Area cave = CreateArea(device, "Cave");
            using TestGameplayScene scene = LoadScene(game, null, CreateArea(device, "Field"));

            scene.DeferredAreaLoader = name => "Cave".Equals(name, StringComparison.Ordinal) ? cave : null;

            scene.Complete("Cave");

            Assert.Same(cave, scene.CurrentArea);
            Assert.Equal(1, scene.TransitionedCount);
        });

    [Fact]
    public void CompleteAreaTransition_LoadedArea_HandsOverBackgroundBuiltArea()
        => RunTest((game, device) =>
        {
            Area cave = CreateArea(device, "Cave");
            using TestGameplayScene scene = LoadScene(game, null, CreateArea(device, "Field"));

            scene.Complete(cave);

            Assert.Same(cave, scene.CurrentArea);

            // Handing the very same instance over a second time is not a duplicate.
            scene.Complete(cave);

            Assert.Equal(2, scene.TransitionedCount);
        });

    [Fact]
    public void CompleteAreaTransition_LoadedArea_DuplicateNameThrowsException()
        => RunTest((game, device) =>
        {
            using TestGameplayScene scene = LoadScene(game, null, CreateArea(device, "Field"));

            Assert.Throws<InvalidOperationException>(() => scene.Complete(CreateArea(device, "FIELD")));
        });

    [Fact]
    public void CompleteAreaTransition_WithoutBegin_PassesNoDestination()
        => RunTest((game, device) =>
        {
            Area cave = CreateArea(device, "Cave");

            cave.AddTransitionPoint(new TransitionPoint("Field", _Doorway) { Name = "SouthDoor" });

            using TestGameplayScene scene = LoadScene(game, null, CreateArea(device, "Field"), cave);

            scene.Complete("Cave");

            Assert.Null(scene.DestinationPoint);
            Assert.Null(scene.PointActiveWhileTransitioned);
        });

    [Fact]
    public void OnAreaTransitioned_WiredDestination_ResolvesIgnoringCase()
        => RunTest((game, device) =>
        {
            Area cave = CreateArea(device, "Cave");
            var southDoor = new TransitionPoint("Field", new RectangleF(32, 32, 16, 16)) { Name = "SouthDoor" };

            cave.AddTransitionPoint(southDoor);

            using TestGameplayScene scene = TransitionThroughDoor(game, device, cave, "southdoor");

            Assert.Same(southDoor, scene.DestinationPoint);
            Assert.Equal(new PointF(40, 40), scene.DestinationPoint?.SpawnPosition);
        });

    [Fact]
    public void OnAreaTransitioned_DisabledDestination_StillResolves()
        => RunTest((game, device) =>
        {
            Area cave = CreateArea(device, "Cave");
            var southDoor = new TransitionPoint("Field", new RectangleF(32, 32, 16, 16))
                            {
                                Name = "SouthDoor", IsEnabled = false
                            };

            cave.AddTransitionPoint(southDoor);

            using TestGameplayScene scene = TransitionThroughDoor(game, device, cave, "SouthDoor");

            Assert.Same(southDoor, scene.DestinationPoint);
        });

    [Fact]
    public void OnAreaTransitioned_MissingDestination_PassesNull()
        => RunTest((game, device) =>
        {
            Area cave = CreateArea(device, "Cave");

            cave.AddTransitionPoint(new TransitionPoint("Field", _Doorway) { Name = "SouthDoor" });

            using TestGameplayScene scene = TransitionThroughDoor(game, device, cave, "WestDoor");

            Assert.Equal(1, scene.TransitionedCount);
            Assert.Null(scene.DestinationPoint);
        });

    [Fact]
    public void OnAreaTransitioned_NoWiredDestination_PassesNull()
        => RunTest((game, device) =>
        {
            Area cave = CreateArea(device, "Cave");

            cave.AddTransitionPoint(new TransitionPoint("Field", _Doorway) { Name = "SouthDoor" });

            using TestGameplayScene scene = TransitionThroughDoor(game, device, cave, string.Empty);

            Assert.Null(scene.DestinationPoint);
        });

    [Fact]
    public void OnAreaTransitioned_InitiatingPoint_ReadableThenCleared()
        => RunTest((game, device) =>
        {
            using TestGameplayScene scene = TransitionThroughDoor(game, device, CreateArea(device, "Cave"), "SouthDoor");

            Assert.Equal("NorthDoor", scene.PointActiveWhileTransitioned?.Name);
            Assert.Null(scene.ObservedActiveTransitionPoint);
        });

    [Fact]
    public void OnAreaTransitioning_Deferred_StaysTransitioningUntilCompleted()
        => RunTest((game, device) =>
        {
            Area field = CreateArea(device, "Field");
            Area cave = CreateArea(device, "Cave");

            field.AddTransitionPoint(new TransitionPoint("Cave", _Doorway) { Name = "NorthDoor", TargetPointName = "SouthDoor" });
            cave.AddTransitionPoint(new TransitionPoint("Field", _Doorway) { Name = "SouthDoor" });

            var activator = new SpatialStub(_AwayFromDoorway);
            using TestGameplayScene scene = LoadScene(game, activator, field);

            scene.DefersTransitions = true;

            scene.Tick();

            activator.Bounds = _InsideDoorway;
            scene.Tick();

            Assert.True(scene.IsTransitioningAreas);
            Assert.Same(field, scene.CurrentArea);
            Assert.Equal(0, scene.TransitionedCount);

            // Detection stays quiet while a transition is in flight, no matter how many updates elapse.
            scene.Tick();

            Assert.Equal(0, scene.TransitionedCount);

            // The background-built area is handed over directly, as a loading screen's worker would do.
            scene.Complete(cave);

            Assert.False(scene.IsTransitioningAreas);
            Assert.Same(cave, scene.CurrentArea);
            Assert.Equal("SouthDoor", scene.DestinationPoint?.Name);
        });

    private static TestGameplayScene TransitionThroughDoor(TestGame game,
                                                           GraphicsDevice device,
                                                           Area cave,
                                                           string targetPointName)
    {
        Area field = CreateArea(device, "Field");

        field.AddTransitionPoint(new TransitionPoint("Cave", _Doorway)
                                 {
                                     Name = "NorthDoor", TargetPointName = targetPointName
                                 });

        var activator = new SpatialStub(_AwayFromDoorway);
        TestGameplayScene scene = LoadScene(game, activator, field, cave);

        scene.Tick();

        activator.Bounds = _InsideDoorway;
        scene.Tick();

        return scene;
    }

    private static TestGameplayScene LoadScene(TestGame game, ISpatial? activator, params Area[] areas)
    {
        var scene = new TestGameplayScene(game, areas) { Activator = activator };

        scene.Load(new SceneManager(game));
        scene.MakeCurrent(areas[0]);

        return scene;
    }

    private static Area CreateArea(GraphicsDevice device, string name)
    {
        var tileMap = new TileMap(device, name, new Size(2, 2), new Size(16, 16), new CustomProperties());

        return new Area(device, tileMap) { Name = name };
    }

    private static void RunTest(Action<TestGame, GraphicsDevice> test)
    {
        using var game = new TestGame();

        ExceptionDispatchInfo? failure = null;

        game.Initialized += (_, _) =>
        {
            try
            {
                test(game, game.GraphicsDevice);
            }
            catch (Exception e)
            {
                failure = ExceptionDispatchInfo.Capture(e);
            }
        };

        game.Run();

        failure?.Throw();
    }

    private sealed class SpatialStub(IShape bounds) : ISpatial
    {
        public IShape Bounds
        { get; set; } = bounds;
    }
}
