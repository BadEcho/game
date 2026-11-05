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
    private readonly ContentManagerFixture _contentFixture;
    private readonly DistanceFieldFont _font;

    public ButtonTests(ContentManagerFixture contentFixture)
    {
        _contentFixture = contentFixture;
        _font = contentFixture.Content.Load<DistanceFieldFont>("Fonts\\Lato");
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
