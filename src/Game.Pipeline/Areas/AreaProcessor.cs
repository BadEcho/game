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

using BadEcho.Extensions;
using BadEcho.Game.Pipeline.Properties;
using BadEcho.Game.Pipeline.SpriteSheets;
using BadEcho.Game.Pipeline.Tiles;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace BadEcho.Game.Pipeline.Areas;

/// <summary>
/// Provides a processor of area asset data for the content pipeline.
/// </summary>
[ContentProcessor(DisplayName = "Area Processor - Bad Echo")]
public sealed class AreaProcessor : ContentProcessor<AreaContent>
{
    /// <inheritdoc/>
    public override AreaContent Process(AreaContent input, ContentProcessorContext context)
    {
        Require.NotNull(input, nameof(input));
        Require.NotNull(context, nameof(context));

        context.Log(Strings.ProcessingArea.InvariantFormat(input.Identity.SourceFilename));

        ValidateDependencyPath(input.Asset.TileMapPath);

        input.AddReference<TileMapContent>(context, input.Asset.TileMapPath, []);

        foreach (AreaActorAsset actor in input.Asset.Actors)
        {
            ValidateDependencyPath(actor.SpriteSheetPath);

            input.AddReference<SpriteSheetContent>(context, actor.SpriteSheetPath, []);
        }

        context.Log(Strings.ProcessingFinished.InvariantFormat(input.Identity.SourceFilename));

        return input;
    }
}
