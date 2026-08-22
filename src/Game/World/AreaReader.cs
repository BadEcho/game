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

using BadEcho.Game.Tiles;
using Microsoft.Xna.Framework.Content;

namespace BadEcho.Game.World;

/// <summary>
/// Provides a reader of raw area content from the content pipeline.
/// </summary>
public sealed class AreaReader : ContentTypeReader<Area>
{
    /// <inheritdoc/>
    protected override Area Read(ContentReader input, Area existingInstance)
    {
        Require.NotNull(input, nameof(input));

        var name = input.ReadString();
        var tileMap = input.ReadExternalReference<TileMap>();

        var actorsToRead = input.ReadInt32();
        var area = new Area(tileMap)
                   {
                       Name = name
                   };

        while (actorsToRead > 0)
        {
            var spriteSheet = input.ReadExternalReference<SpriteSheet>();
            var position = input.ReadVector2();
            var isShadowless = input.ReadBoolean();

            var actor = new AnimatedSprite(spriteSheet) { Position = position };

            area.AddActor(actor, isShadowless);

            actorsToRead--;
        }

        return area;
    }
}
