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

namespace BadEcho.Game;

/// <summary>
/// Provides contextual information for a game.
/// </summary>
/// <param name="GraphicsDevice">The graphics devices used by the game.</param>
/// <param name="ServiceProvider">The service provider attached to the game.</param>
public sealed record GameContext(GraphicsDevice GraphicsDevice, IServiceProvider ServiceProvider);
