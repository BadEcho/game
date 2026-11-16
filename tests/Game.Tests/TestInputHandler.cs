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
using Microsoft.Xna.Framework.Input;

namespace BadEcho.Game.Tests;

/// <summary>
/// Provides an input handler whose state is set directly by unit tests.
/// </summary>
internal sealed class TestInputHandler : IInputHandler
{
    /// <inheritdoc />
    public Point MousePosition
    { get; set; }

    /// <summary>
    /// Gets the mouse buttons the handler will report as being pressed.
    /// </summary>
    public List<MouseButton> PressedButtons
    { get; } = [];

    /// <summary>
    /// Gets the keyboard keys the handler will report as being pressed.
    /// </summary>
    public List<Keys> PressedKeys
    { get; } = [];

    /// <inheritdoc />
    public IInputElement? FocusedElement
    { get; set; }

    /// <inheritdoc />
    IEnumerable<MouseButton> IInputHandler.PressedButtons
        => PressedButtons;

    /// <inheritdoc />
    IEnumerable<Keys> IInputHandler.PressedKeys
        => PressedKeys;

    /// <inheritdoc />
    public void ClearFocus()
        => FocusedElement = null;
}