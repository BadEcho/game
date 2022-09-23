﻿//-----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2022 Bad Echo LLC. All rights reserved.
//
//		Bad Echo Technologies are licensed under a
//		GNU Affero General Public License v3.0.
//
//		See accompanying file LICENSE.md or a copy at:
//		https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
//-----------------------------------------------------------------------

using BadEcho.Game.Tiles;
using Xunit;

namespace BadEcho.Game.Tests;


public class TileSetTests : IClassFixture<ContentManagerFixture>
{
    private readonly Microsoft.Xna.Framework.Content.ContentManager _content;

    public TileSetTests(ContentManagerFixture contentFixture)
        => _content = contentFixture.Content;

    [Fact]
    public void Load_Grasslands_NotNull()
    {
        TileSet tileSet = _content.Load<TileSet>("Tiles\\Grasslands");
        
        Assert.NotNull(tileSet);
    }

    [Fact]
    public void Load_Grasslands_SizeValid()
    {
        TileSet tileSet = _content.Load<TileSet>("Tiles\\Grasslands");

        Assert.Equal(16, tileSet.TileSize.X);
        Assert.Equal(16, tileSet.TileSize.Y);
    }

    [Fact]
    public void Load_Grasslands_CountValid()
    {
        TileSet tileSet = _content.Load<TileSet>("Tiles\\Grasslands");

        Assert.Equal(10, tileSet.TileCount);
    }
}
