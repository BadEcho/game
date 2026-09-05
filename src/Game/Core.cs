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
using BadEcho.Game.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game;

/// <summary>
/// Provides a game entry point with globally accessible state.
/// </summary>
/// <remarks>
/// Only a single <see cref="Core"/> instance may exist at a time. The instance is made available through
/// <see cref="Instance"/> as soon as it is constructed and remains available until it is disposed.
/// </remarks>
public class Core : Microsoft.Xna.Framework.Game
{
    private static Core? _Instance;

    private readonly SceneManager _sceneManager;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    /// <param name="windowWidth">The initial width, in pixels, of the game window.</param>
    /// <param name="windowHeight">The initial height, in pixels, of the game window.</param>
    public Core(int windowWidth, int windowHeight)
         : this()
    {
        Graphics.IsFullScreen = false;
        Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        Graphics.ApplyChanges();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    /// <param name="exclusiveFullScreen">True to enable exclusive fullscreen; false </param>
    public Core(bool exclusiveFullScreen)
        : this()
    {
        Graphics.IsFullScreen = true;
        Graphics.HardwareModeSwitch = exclusiveFullScreen;
        Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        Graphics.ApplyChanges();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Core"/> class.
    /// </summary>
    private Core()
    {
        if (_Instance != null)
            throw new InvalidOperationException(Strings.CoreAlreadyExists);

        _Instance = this;

        Graphics = new GraphicsDeviceManager(this)
                   {
                       SynchronizeWithVerticalRetrace = true,
                       GraphicsProfile = GraphicsProfile.HiDef,
                       HardwareModeSwitch = false
                   };

        IsFixedTimeStep = false;
        Content.RootDirectory = "Content";

        _sceneManager = new SceneManager(this);

        Components.Add(_sceneManager);
        GraphicsDevice.Clear(Color.Black);
    }

    /// <summary>
    /// Gets the game's graphics device manager.
    /// </summary>
    public GraphicsDeviceManager Graphics
    { get; }

    /// <summary>
    /// Gets the currently running game.
    /// </summary>
    public static Core Instance
        => _Instance ?? throw new InvalidOperationException(Strings.CoreNotCreated);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing && ReferenceEquals(_Instance, this))
        {
            _Instance = null;
            _disposed = true;
        }

        base.Dispose(disposing);
    }
}
