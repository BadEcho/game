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

using System.Runtime.InteropServices;

namespace BadEcho.Game.Interop;

/// <summary>
/// Represents custom window manager information.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct SdlWindowManagerInfo
{
    /// <summary>
    /// The version of SDL.
    /// </summary>
    public SdlVersion version;
    /// <summary>
    /// The subsystem type for the window.
    /// </summary>
    public SdlWindowManagerType subsystem;
    /// <summary>
    /// The handle to the underlying native window.
    /// </summary>
    public nint window;
}
