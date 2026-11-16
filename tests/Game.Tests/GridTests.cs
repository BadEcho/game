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
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace BadEcho.Game.Tests;

public class GridTests : IClassFixture<ContentManagerFixture>
{
    private readonly Rectangle _screenBounds = new(0, 0, 1920, 1080);

    private readonly ContentManagerFixture _contentFixture;
    private readonly DistanceFieldFont _font;

    public GridTests(ContentManagerFixture contentFixture)
    {
        _contentFixture = contentFixture;
        _font = contentFixture.Content.Load<DistanceFieldFont>("Fonts\\Lato");
    }

    [Fact]
    public void Measure_ChildHidden_HiddenChildIgnored()
    {
        var wide = CreateLabel("Hello there", 0);
        var narrow = CreateLabel("Hi", 1);
        var grid = new Grid();

        grid.AddChild(wide);
        grid.AddChild(narrow);
        grid.Measure(_screenBounds.Size);

        wide.IsVisible = false;
        grid.Measure(_screenBounds.Size);

        Assert.Equal(narrow.DesiredSize.Width, grid.DesiredSize.Width);
    }

    [Fact]
    public void Measure_RepeatedPasses_CellHoldsSingleChild()
    {
        var handler = new TestInputHandler { MousePosition = new Point(-1, -1) };
        var label = CreateLabel("Hello there", 0);
        var grid = new Grid { IsSelectable = true, InputHandler = handler };

        grid.AddChild(label);

        // Varying the available size forces a full re-measure on every pass.
        for (int i = 0; i < 3; i++)
        {
            var bounds = new Rectangle(0, 0, _screenBounds.Width - i, _screenBounds.Height);

            grid.Measure(bounds.Size);
            grid.Arrange(bounds);
        }

        List<IControl>? selected = null;
        grid.SelectionChanged += (_, e) => selected = e.Data.ToList();

        Assert.True(grid.Focus());

        handler.PressedKeys.Add(Keys.Enter);
        grid.UpdateInput();

        Assert.NotNull(selected);
        Assert.Same(label, Assert.Single(selected));
    }

    [Fact]
    public void Measure_AllChildrenHidden_EmptySize()
    {
        var handler = new TestInputHandler { MousePosition = new Point(-1, -1) };
        var label = CreateLabel("Hello there", 0);
        var grid = new Grid { IsSelectable = true, InputHandler = handler };

        grid.AddChild(label);
        label.IsVisible = false;

        grid.Measure(_screenBounds.Size);
        grid.Arrange(_screenBounds);

        Assert.Equal(Size.Empty, grid.DesiredSize);

        // Keyboard navigation over a grid with no visible cells must be a no-op.
        grid.Focus();
        handler.PressedKeys.Add(Keys.Down);
        grid.UpdateInput();
    }

    [Fact]
    public void Draw_HoveredCellRemoved_NoException()
    {
        var handler = new TestInputHandler { MousePosition = new Point(-1, -1) };
        var labels = new[] { CreateLabel("One", 0), CreateLabel("Two", 1), CreateLabel("Three", 2) };
        var grid = new Grid
        {
            IsSelectable = true,
            IsHoverPersistent = true,
            HoveredItemBackground = new Brush(Color.Red),
            InputHandler = handler
        };

        foreach (Label label in labels)
        {
            grid.AddChild(label);
        }

        grid.Measure(_screenBounds.Size);
        grid.Arrange(_screenBounds);
        grid.Focus();

        // Hover the last column via the keyboard: (0,0) -> (1,0) -> (2,0).
        for (int i = 0; i < 2; i++)
        {
            handler.PressedKeys.Add(Keys.Right);
            grid.UpdateInput();
            handler.PressedKeys.Clear();
            grid.UpdateInput();
        }

        labels[1].IsVisible = false;
        labels[2].IsVisible = false;

        grid.Measure(_screenBounds.Size);
        grid.Arrange(_screenBounds);

        using var spriteBatch = new SpriteBatch(_contentFixture.Device);

        spriteBatch.Begin();
        grid.Draw(spriteBatch);
        spriteBatch.End();
    }

    private Label CreateLabel(string text, int column)
        => new()
        {
            Font = _font,
            FontSize = 24,
            Text = text,
            Column = column
        };
}
