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

using BadEcho.Game.Pipeline.SpriteSheets;
using BadEcho.Game.Pipeline.Tiles;
using BadEcho.Game.World;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace BadEcho.Game.Pipeline.Areas;

/// <summary>
/// Provides a writer of raw area content to the content pipeline.
/// </summary>
[ContentTypeWriter]
public sealed class AreaWriter : ContentTypeWriter<AreaContent>
{
    /// <inheritdoc/>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
        => typeof(AreaReader).AssemblyQualifiedName ?? string.Empty;

    /// <inheritdoc/>
    protected override void Write(ContentWriter output, AreaContent value)
    {
        Require.NotNull(output, nameof(output));
        Require.NotNull(value, nameof(value));

        AreaAsset asset = value.Asset;

        output.Write(asset.Name);
        output.WriteExternalReference(value.GetReference<TileMapContent>(asset.TileMapPath));
        output.Write(asset.DefaultSpawnPosition);

        output.Write(asset.Actors.Count);

        foreach (AreaActorAsset actor in asset.Actors)
        {
            output.WriteExternalReference(value.GetReference<SpriteSheetContent>(actor.SpriteSheetPath));
            output.Write(actor.Position);
            output.Write(actor.IsShadowless);
        }
    }
}
