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
    public static nint GetNativeWindowHandle(this GameWindow window)
    {
        Require.NotNull(window, nameof(window));

        return PlatformInfo.MonoGamePlatform switch
        {
            MonoGamePlatform.DesktopGL => GetSdlWindowHandle(window),
            MonoGamePlatform.Windows => window.Handle,
            _ => throw new InvalidOperationException()
        };
    }

    private static unsafe nint GetSdlWindowHandle(GameWindow window)
    {
        SdlWindowManagerInfo wmInfo = default;
        bool success = Sdl2.GetWindowManagerInfo(window.Handle, ref wmInfo);

        if (!success)
        {
            byte* pError = Sdl2.GetError();
            string? error = Utf8StringMarshaller.ConvertToManaged(pError);

            Sdl2.ClearError();
            
            throw new InvalidOperationException(Strings.SdlGetWindowManagerFailed.InvariantFormat(error));
        }

        return wmInfo.window;
    }
}
