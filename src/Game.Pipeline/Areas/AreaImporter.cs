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

using System.Text.Json;
using BadEcho.Extensions;
using BadEcho.Game.Pipeline.Properties;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace BadEcho.Game.Pipeline.Areas;

/// <summary>
/// Provides an importer of area asset data for the content pipeline.
/// </summary>
[ContentImporter(".area", DisplayName = "Area Importer - Bad Echo", DefaultProcessor = nameof(AreaProcessor))]
public sealed class AreaImporter : ContentImporter<AreaContent>
{
    private static readonly JsonSerializerOptions _AssetFileOptions
        = new()
          {
              PropertyNameCaseInsensitive = true,
              IncludeFields = true
          };

    /// <inheritdoc/>
    public override AreaContent Import(string filename, ContentImporterContext context)
    {
        Require.NotNull(filename, nameof(filename));
        Require.NotNull(context, nameof(context));

        context.Log(Strings.ImportingArea.InvariantFormat(filename));

        var fileContents = File.ReadAllText(filename);
        var asset = JsonSerializer.Deserialize<AreaAsset>(fileContents, _AssetFileOptions)
                    ?? throw new ArgumentException(Strings.AreaIsNull.InvariantFormat(filename), nameof(filename));

        context.Log(Strings.ImportingDependency.InvariantFormat(asset.TileMapPath));

        asset.TileMapPath = NormalizeDependencyPath(filename, asset.TileMapPath);

        context.AddDependency(asset.TileMapPath);

        foreach (AreaActorAsset actor in asset.Actors)
        {
            context.Log(Strings.ImportingDependency.InvariantFormat(actor.SpriteSheetPath));

            actor.SpriteSheetPath = NormalizeDependencyPath(filename, actor.SpriteSheetPath);

            context.AddDependency(actor.SpriteSheetPath);
        }

        context.Log(Strings.ImportingFinished.InvariantFormat(filename));

        return new AreaContent(asset) { Identity = new ContentIdentity(filename) };
    }
}
