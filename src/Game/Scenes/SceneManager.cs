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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Scenes;

/// <summary>
/// Provides a game component that manages the active scenes by performing tasks such as controlling their z-order
/// and visibility on the screen, as well as directing input from the user to the appropriate screen.
/// </summary>
public sealed class SceneManager : DrawableGameComponent
{
    private readonly List<GameScene> _scenes = [];

    private SpriteBatch? _spriteBatch;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneManager"/> class.
    /// </summary>
    /// <param name="game">The game that the component should be attached to.</param>
    public SceneManager(Microsoft.Xna.Framework.Game game)
        : base(game)
    { }

    /// <summary>
    /// Gets the scenes loaded into this manager.
    /// </summary>
    public IReadOnlyCollection<GameScene> Scenes
        => _scenes;

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        Require.NotNull(gameTime, nameof(gameTime));

        base.Update(gameTime);

        var scenes = new Stack<GameScene>(_scenes);
        var updateTime = new GameUpdateTime(Game, gameTime);
        bool isActive = true;
        
        while (scenes.Count > 0)
        {   // We iterate through the scenes, from the top in the z-order to the bottom.
            var scene = scenes.Pop();

            scene.Update(updateTime, isActive);

            if (scene.HasClosed)
                RemoveScene(scene); // This is the typical path for a gracefully exiting scene's removal from the manager.
            else if (scene.TransitionStatus is TransitionStatus.Entered or TransitionStatus.Entering && !scene.IsModal)
            {
                // All others below the first scene not marked as a modal popup will be considered to not be active.
                isActive = false;
            }
        }
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        Require.NotNull(gameTime, nameof(gameTime));

        base.Draw(gameTime);

        if (_spriteBatch == null)
            throw new InvalidOperationException(Strings.UninitializedGraphicsDevice);

        IEnumerable<GameScene> visibleScenes 
            = _scenes.Where(s => s.TransitionStatus != TransitionStatus.Exited);
        
        foreach (var visibleScene in visibleScenes)
        {
            visibleScene.Draw(_spriteBatch);
        }
    }

    /// <summary>
    /// Adds the provided game scene to the collection of loaded scenes, which will prepare it for being drawn to the screen.
    /// </summary>
    /// <param name="scene">The game scene to load into this manager.</param>
    public void AddScene(GameScene scene)
    {
        Require.NotNull(scene, nameof(scene));
        
        _scenes.Add(scene);

        // Associate this manager with the scene.
        scene.Load(this);
    }
    
    /// <summary>
    /// Removes the specified game scene from the collection of loaded scenes, immediately disconnecting it from the screen.
    /// </summary>
    /// <param name="scene">The game scene to unload from this manager.</param>
    public void RemoveScene(GameScene scene)
    {
        Require.NotNull(scene, nameof(scene));
        
        scene.Dispose();

        _scenes.Remove(scene);
    }

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        base.LoadContent();

        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _spriteBatch?.Dispose();
            
            foreach (GameScene scene in _scenes)
            {   
                scene.Dispose();
            }

            _disposed = true;
        }

        base.Dispose(disposing);
    }
}
