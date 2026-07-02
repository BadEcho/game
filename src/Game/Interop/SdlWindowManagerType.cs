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

namespace BadEcho.Game.Interop;

/// <summary>
/// Specifies a type of windowing subsystem.
/// </summary>
internal enum SdlWindowManagerType
{
    /// <summary>
    /// Who knows?
    /// </summary>
    Unknown,
    /// <summary>
    /// Windows...that Bill Gates guy, you know.
    /// </summary>
    Windows,
    /// <summary>
    /// Leet Linux window.
    /// </summary>
    X11,
    /// <summary>
    /// Leet Linux window without 
    /// </summary>
    Directfb,
    /// <summary>
    /// Evil Apple Cocoa window.
    /// </summary>
    Cocoa,
    /// <summary>
    /// Evil Apple window on your phone.
    /// </summary>
    UiKit,
    /// <summary>
    /// Another Linux thing.
    /// </summary>
    Wayland,
    /// <summary>
    /// More Linux.
    /// </summary>
    Mir,
    /// <summary>
    /// Pretty sure this is no longer supported by SDL.
    /// </summary>
    WinRt,
    /// <summary>
    /// Bee bop boop.
    /// </summary>
    Android
}
