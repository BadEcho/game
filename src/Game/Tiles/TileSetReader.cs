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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Tiles;

/// <summary>
/// Provides a reader of raw tile set content from the content pipeline.
/// </summary>
public sealed class TileSetReader : ContentTypeReader<TileSet>
{
    /// <inheritdoc />
    protected override TileSet Read(ContentReader input, TileSet existingInstance)
    {
        Require.NotNull(input, nameof(input));

        var texture = input.ReadExternalReference<Texture2D>();
        var tileWidth = input.ReadInt32();
        var tileHeight = input.ReadInt32();
        var tileCount = input.ReadInt32();
        var columns = input.ReadInt32();
        var spacing = input.ReadInt32();
        var margin = input.ReadInt32();

        return new TileSet(texture, new Point(tileWidth, tileHeight), tileCount, columns)
               {
                   Spacing = spacing,
                   Margin = margin
               };
    }
}
