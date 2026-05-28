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

using BadEcho.Game.Scenes;

namespace BadEcho.Game.UI;

/// <summary>
/// Provides a self-contained user interface game scene that acts a loading screen, used to distract the player from 
/// the fact that the game takes so damn long to load.
/// </summary>
public abstract class LoadingScreenScene : ScreenScene
{
    private bool _otherScenesUnloaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingScreenScene"/> class.
    /// </summary>
    /// <param name="game">The game this scene is for.</param>
    protected LoadingScreenScene(Microsoft.Xna.Framework.Game game)
        : base(game)
    {
        TransitionTime = TimeSpan.FromSeconds(0.5);
    }

    /// <inheritdoc/>
    protected override void UpdateCore(GameUpdateTime time, bool isActive)
    {
        base.UpdateCore(time, isActive);

        if (_otherScenesUnloaded)
        {
            foreach (GameScene sceneToLoad in GetScenesToLoad())
            {
                Manager?.AddScene(sceneToLoad);
            }

            Manager?.Game.ResetElapsedTime();
            Manager?.RemoveScene(this);
        }

        if (TransitionStatus == TransitionStatus.Entered && Manager?.Scenes.Count == 1)
            _otherScenesUnloaded = true;
    }

    /// <inheritdoc/>
    protected override void OnLoad(SceneManager manager)
    {
        Require.NotNull(manager, nameof(manager));

        foreach (GameScene scene in manager.Scenes)
        {
            scene.Close();
        }       
        
        base.OnLoad(manager);
    }

    /// <summary>
    /// Retrieves the game scenes to load with this user interface.
    /// </summary>
    /// <returns>The sequence of <see cref="GameScene"/> instances to load.</returns>
    protected abstract IEnumerable<GameScene> GetScenesToLoad();
}
