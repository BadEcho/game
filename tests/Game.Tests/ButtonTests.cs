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

using BadEcho.Game.Fonts;
using BadEcho.Game.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace BadEcho.Game.Tests;

public class ButtonTests : IClassFixture<ContentManagerFixture>
{
    private readonly Rectangle _screenBounds = new(0, 0, 1920, 1080);

    private readonly ContentManagerFixture _contentFixture;
    private readonly DistanceFieldFont _font;

    public ButtonTests(ContentManagerFixture contentFixture)
    {
        _contentFixture = contentFixture;
        _font = contentFixture.Content.Load<DistanceFieldFont>("Fonts\\Lato");
    }

    [Fact]
    public void UpdateInput_VisibleClicked_ClickedRaised()
    {
        var handler = new TestInputHandler { MousePosition = new Point(10, 10) };
        Button button = CreateArrangedButton(handler);
        bool clicked = false;

        button.Clicked += (_, _) => clicked = true;

        Click(handler, button);

        Assert.True(clicked);
    }

    [Fact]
    public void UpdateInput_HiddenClicked_ClickedNotRaised()
    {
        var handler = new TestInputHandler { MousePosition = new Point(10, 10) };
        Button button = CreateArrangedButton(handler);
        bool clicked = false;

        button.Clicked += (_, _) => clicked = true;
        button.IsVisible = false;

        Click(handler, button);

        Assert.False(clicked);
        Assert.False(button.IsMouseOver);
    }

    [Fact]
    public void UpdateInput_HiddenWhilePressed_ClickedNotRaised()
    {
        var handler = new TestInputHandler { MousePosition = new Point(10, 10) };
        Button button = CreateArrangedButton(handler);
        bool clicked = false;

        button.Clicked += (_, _) => clicked = true;

        handler.PressedButtons.Add(MouseButton.Left);
        button.UpdateInput();

        button.IsVisible = false;

        handler.PressedButtons.Clear();
        button.UpdateInput();

        Assert.False(clicked);
    }

    [Fact]
    public void UpdateInput_ParentHidden_ClickedNotRaised()
    {
        var handler = new TestInputHandler { MousePosition = new Point(10, 10) };
        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            InputHandler = handler
        };
        var button = new Button { Width = 100, Height = 40 };
        bool clicked = false;

        button.Clicked += (_, _) => clicked = true;
        panel.AddChild(button);
        panel.Measure(_screenBounds.Size);
        panel.Arrange(_screenBounds);

        panel.IsVisible = false;

        handler.PressedButtons.Add(MouseButton.Left);
        panel.UpdateInput();
        handler.PressedButtons.Clear();
        panel.UpdateInput();

        Assert.False(clicked);
    }

    private static void Click(TestInputHandler handler, Button button)
    {
        handler.PressedButtons.Add(MouseButton.Left);
        button.UpdateInput();
        handler.PressedButtons.Clear();
        button.UpdateInput();
    }

    private Button CreateArrangedButton(TestInputHandler handler)
    {
        var button = new Button
        {
            Width = 100,
            Height = 40,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            InputHandler = handler
        };

        button.Measure(_screenBounds.Size);
        button.Arrange(_screenBounds);

        return button;
    }

    [Fact]
    public void DrawPrimitives_TextSet_RendersText()
    {
        GraphicsDevice device = _contentFixture.Device;
        var bounds = new Rectangle(0, 0, 200, 100);

        var button = new Button
        {
            Font = _font,
            FontSize = 24,
            FontColor = Color.White,
            Text = "OK",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        button.Measure(bounds.Size);
        button.Arrange(bounds);

        using var target = new RenderTarget2D(device, bounds.Width, bounds.Height);

        device.SetRenderTarget(target);
        device.Clear(Color.Transparent);
        device.BlendState = BlendState.AlphaBlend;
        device.DepthStencilState = DepthStencilState.None;
        device.RasterizerState = RasterizerState.CullNone;

        button.DrawPrimitives(null, 1.0f);

        device.SetRenderTarget(null);

        var pixels = new Color[bounds.Width * bounds.Height];
        target.GetData(pixels);

        Assert.Contains(pixels, p => p.A > 0);
    }
}
