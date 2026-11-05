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

using BadEcho.Game.Atlases;
using BadEcho.Game.UI;
using Microsoft.Xna.Framework;
using Xunit;

namespace BadEcho.Game.Tests;

public class ImageTests : IClassFixture<ContentManagerFixture>
{
    private readonly Rectangle _screenBounds = new(0, 0, 1920, 1080);

    private readonly TextureAtlas _atlas;
    private readonly Image _image;

    public ImageTests(ContentManagerFixture contentFixture)
    {
        _atlas = contentFixture.Content.Load<TextureAtlas>("Atlases\\BlackShuttleGrass");

        _image = new Image
                 {
                     Visual = _atlas["black"],
                     HorizontalAlignment = HorizontalAlignment.Center,
                     VerticalAlignment = VerticalAlignment.Center
                 };
    }

    [Fact]
    public void Measure_Default_SourceWidthHeight()
    {
        _image.Measure(_screenBounds.Size);

        Assert.Equal(16, _image.DesiredSize.Width);
        Assert.Equal(16, _image.DesiredSize.Height);
    }

    [Fact]
    public void Measure_WidthSet_AspectRatioObeyed()
    {
        _image.Width = 32;
        _image.Measure(_screenBounds.Size);

        Assert.Equal(32, _image.DesiredSize.Width);
        Assert.Equal(32, _image.DesiredSize.Height);
    }

    [Fact]
    public void Measure_HeightSet_AspectRatioObeyed()
    {
        _image.Height = 32;
        _image.Measure(_screenBounds.Size);

        Assert.Equal(32, _image.DesiredSize.Width);
        Assert.Equal(32, _image.DesiredSize.Height);
    }

    [Fact]
    public void Measure_NonSquareWidthSet_AspectRatioObeyed()
    {   // The shuttle region is 58x90.
        _image.Visual = _atlas["shuttle"];
        _image.Width = 116;
        _image.Measure(_screenBounds.Size);

        Assert.Equal(new Size(116, 180), _image.DesiredSize);
    }

    [Fact]
    public void Measure_NonSquareHeightSet_AspectRatioObeyed()
    {
        _image.Visual = _atlas["shuttle"];
        _image.Height = 45;
        _image.Measure(_screenBounds.Size);

        Assert.Equal(new Size(29, 45), _image.DesiredSize);
    }
}
