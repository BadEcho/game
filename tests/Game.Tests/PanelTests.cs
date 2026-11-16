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

using BadEcho.Game.UI;
using Xunit;

namespace BadEcho.Game.Tests;

public class PanelTests
{
    [Fact]
    public void AddChild_AfterInputHandlerAssigned_ChildReceivesHandler()
    {
        var handler = new TestInputHandler();
        var panel = new StackPanel { InputHandler = handler };
        var button = new Button();

        panel.AddChild(button);

        Assert.Same(handler, button.InputHandler);

        panel.UpdateInput();
    }

    [Fact]
    public void AddItem_AfterInputHandlerAssigned_MenuItemReceivesHandler()
    {
        var handler = new TestInputHandler();
        var menu = new Menu { InputHandler = handler };

        MenuItem item = menu.AddItem("Quit");

        Assert.Same(handler, item.InputHandler);

        menu.UpdateInput();
    }

    [Fact]
    public void RemoveChild_Attached_HandlerCleared()
    {
        var handler = new TestInputHandler();
        var panel = new StackPanel { InputHandler = handler };
        var button = new Button();

        panel.AddChild(button);
        panel.RemoveChild(button);

        Assert.Null(button.InputHandler);
        Assert.Null(button.Parent);
    }

    [Fact]
    public void RemoveChild_DescendantFocused_FocusCleared()
    {
        var handler = new TestInputHandler();
        var panel = new StackPanel { InputHandler = handler };
        var menu = new Menu();

        panel.AddChild(menu);

        Assert.True(menu.Focus());
        Assert.Same(menu.Content, handler.FocusedElement);

        panel.RemoveChild(menu);

        Assert.Null(handler.FocusedElement);
    }
}
