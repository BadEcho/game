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

using System.Runtime.InteropServices.Marshalling;
using BadEcho.Extensions;
using BadEcho.Game.Interop;
using BadEcho.Game.Properties;
using BadEcho.Interop;
using Microsoft.Xna.Framework;
using MonoGame.Framework.Utilities;

namespace BadEcho.Game;

/// <summary>
/// Provides a set of static extension methods that aid in matters related to a game's window.
/// </summary>
public static class GameWindowExtensions
{
    /// <summary>
    /// Gets the handle to the underlying native window for this <see cref="GameWindow"/> instance.
    /// </summary>
    /// <param name="window">The game window to get the underlying window's handle for.</param>
    /// <returns>The handle to the underlying native window for <c>window</c>.</returns>
    public static WindowHandle GetNativeWindowHandle(this GameWindow window)
    {
        Require.NotNull(window, nameof(window));

        return PlatformInfo.MonoGamePlatform switch
        {
            MonoGamePlatform.DesktopGL => GetSdlWindowHandle(window),
            MonoGamePlatform.Windows => new WindowHandle(window.Handle, false),
            MonoGamePlatform.WindowsDX12 => FindWindowHandle(),
            _ => throw new InvalidOperationException()
        };
    }

    private static WindowHandle FindWindowHandle()
    {   // The native MGRuntime.dll does not export the internal SDL window handle it creates. We have the handle to the MGP_WINDOW
        // but that does us no good here. Without a clean way to extract the actual window handle, we're left with simply searching for it.
        IEnumerable<NativeWindow> processWindows = NativeWindow.FromProcessId(Environment.ProcessId);

        foreach (NativeWindow processWindow in processWindows)
        {   // There tends to be more than one window associated with the process. Checking the class name will tell us for sure if it is the SDL window.
            if (processWindow.ClassName == "SDL_app")
                return processWindow.Handle;
        }

        return WindowHandle.InvalidHandle;
    }

    private static unsafe WindowHandle GetSdlWindowHandle(GameWindow window)
    {
        SdlWindowManagerInfo wmInfo = default;

        // The version affects which members in the info union get written by SDL.
        Sdl2.GetVersion(out wmInfo.version);

        bool success = Sdl2.GetWindowManagerInfo(window.Handle, ref wmInfo);

        if (!success)
        {
            byte* pError = Sdl2.GetError();
            string? error = Utf8StringMarshaller.ConvertToManaged(pError);

            Sdl2.ClearError();
            
            throw new InvalidOperationException(Strings.SdlGetWindowManagerFailed.InvariantFormat(error));
        }

        // The info union is essentially a sequence of pointers (or pointer-sized members at least).
        nint* info = (nint*) wmInfo.info;

        nint handle = wmInfo.subsystem switch
        {
            SdlWindowManagerType.X11 or SdlWindowManagerType.Wayland or SdlWindowManagerType.Mir => info[1],
            SdlWindowManagerType.Unknown => nint.Zero,
            _ => info[0]
        };

        return handle == nint.Zero ? WindowHandle.InvalidHandle : new WindowHandle(handle, false);
    }
}
