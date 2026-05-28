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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Scenes;

/// <summary>
/// Provides a game scene that acts as a backdrop behind any other active scenes. 
/// </summary>
/// <remarks>
/// Unlike how other scenes behave, it will not begin to deactivate if not at the top of the z-order, as it is meant to serve
/// as a background (something it cannot do if it deactivates and becomes hidden).
/// </remarks>
public sealed class BackgroundScene : GameScene
{
    private readonly Texture2D _texture;

    private bool _disposed;
      
    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundScene"/> class.
    /// </summary>
    /// <param name="game">The game this scene is for.</param>
    /// <param name="backgroundAssetPath">The relative content path to the asset that will be loaded as the background's texture.</param>
    public BackgroundScene(Microsoft.Xna.Framework.Game game, string backgroundAssetPath)
        : base(game)
    {
        TransitionTime = TimeSpan.FromSeconds(1.0);

        _texture = Content.Load<Texture2D>(backgroundAssetPath);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// We'll always be displayed on screen regardless of z-order position.
    /// This class is named <see cref="BackgroundScene"/>, after all.
    /// </remarks>
    protected override bool AlwaysDisplay 
        => true;

    /// <inheritdoc/>
    protected override void UpdateCore(GameUpdateTime time, bool isActive)
    { }

    /// <inheritdoc/>
    protected override void DrawCore(ConfiguredSpriteBatch spriteBatch)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));

        Viewport viewport = spriteBatch.GraphicsDevice.Viewport;

        spriteBatch.Draw(_texture,
                         new Rectangle(0, 0, viewport.Width, viewport.Height),
                         Color.White);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _texture.Dispose();

            _disposed = true;
        }

        base.Dispose(disposing);
    }
}
