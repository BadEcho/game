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

namespace BadEcho.Game.Scenes;

/// <summary>
/// Specifies the transition phase of a game scene's scene.
/// </summary>
public enum TransitionStatus
{
    /// <summary>
    /// The game scene is transitioning onto the screen.
    /// </summary>
    Entering,
    /// <summary>
    /// The game scene has fully transitioned onto the screen and is fully visible.
    /// </summary>
    Entered,
    /// <summary>
    /// The game scene is transitioning off the screen.
    /// </summary>
    Exiting,
    /// <summary>
    /// The game scene has fully transitioned off the screen and is no longer visible.
    /// </summary>
    Exited
}
