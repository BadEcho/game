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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BadEcho.Game.Interop;

/// <summary>
/// Provides interoperability with the Simple DirectMedia Layer API.
/// </summary>
internal static unsafe partial class Sdl2
{
    private const string LIBRARY_NAME = "SDL2";
    
    /// <summary>
    /// Gets driver-specific information about a window.
    /// </summary>
    /// <param name="handle">The handle to the SDL window about which information is being requested.</param>
    /// <param name="info">
    /// Pointer to a <see cref="SdlWindowManagerInfo"/> structure that will be filled with the info.
    /// </param>
    /// <returns>True if successful; otherwise, false.</returns>
    [LibraryImport(LIBRARY_NAME, EntryPoint = "SDL_GetWindowWMInfo")]
    [return: MarshalAs(UnmanagedType.U1)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static partial bool GetWindowManagerInfo(nint handle, ref SdlWindowManagerInfo info);

    /// <summary>
    /// Retrieve a message about the last error that occurred on the current thread.
    /// </summary>
    /// <returns>The last error (if any) that occurred on the current thread.</returns>
    [LibraryImport(LIBRARY_NAME, EntryPoint = "SDL_GetError")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static partial byte* GetError();

    /// <summary>
    /// Clear any previous error message for this thread.
    /// </summary>
    [LibraryImport(LIBRARY_NAME, EntryPoint = "SDL_ClearError")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static partial void ClearError();
}
