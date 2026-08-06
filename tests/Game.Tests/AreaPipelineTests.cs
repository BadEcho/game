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

using System.Runtime.CompilerServices;
using BadEcho.Game.Pipeline.Areas;
using BadEcho.Game.Pipeline.SpriteSheets;
using BadEcho.Game.Pipeline.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Xunit;

namespace BadEcho.Game.Tests;

public class AreaPipelineTests
{
    private readonly AreaImporter _importer = new();
    private readonly AreaProcessor _processor = new();
    private readonly TestContentImporterContext _importerContext = new();
    private readonly TestContentProcessorContext _processorContext = new();

    [Fact]
    public void ImportProcess_Simple_ReturnsValid()
    {
        AreaContent content = _importer.Import(GetAssetPath("Simple.area"), _importerContext);

        Assert.NotNull(content);
        Assert.NotNull(content.Asset);

        content = _processor.Process(content, _processorContext);

        Assert.Equal(1, content.Asset.Actors.Count);
    }

    [Fact]
    public void Import_Simple_ReturnsValidAsset()
    {
        AreaContent content = _importer.Import(GetAssetPath("Simple.area"), _importerContext);

        Assert.NotNull(content);
        Assert.Equal("Simple Area", content.Asset.Name);
        Assert.Equal("Tiles/GrassFourTiles.tmx", content.Asset.TileMapPath);
        Assert.Collection(content.Asset.Actors, Inspect);

        static void Inspect(AreaActorAsset actor)
        {
            Assert.Equal("Images/StickMan.spritesheet", actor.SpriteSheetPath);
            Assert.Equal(new Vector2(16, 16), actor.Position);
        }
    }

    [Fact]
    public void Import_Simple_ReturnsSourceIdentity()
    {
        string assetPath = GetAssetPath("Simple.area");

        AreaContent content = _importer.Import(assetPath, _importerContext);

        Assert.NotNull(content.Identity);
        Assert.Equal(assetPath, content.Identity.SourceFilename);
    }

    [Fact]
    public void Import_MultipleActors_ReturnsAllActors()
    {
        AreaContent content = _importer.Import(GetAssetPath("MultipleActors.area"), _importerContext);

        Assert.Equal(2, content.Asset.Actors.Count);
        Assert.Collection(content.Asset.Actors, InspectFirst, InspectSecond);

        static void InspectFirst(AreaActorAsset actor)
        {
            Assert.Equal("Images/StickMan.spritesheet", actor.SpriteSheetPath);
            Assert.Equal(new Vector2(16, 32), actor.Position);
        }

        static void InspectSecond(AreaActorAsset actor)
        {
            Assert.Equal("Images/SecondStickMan.spritesheet", actor.SpriteSheetPath);
            Assert.Equal(new Vector2(64, 128), actor.Position);
        }
    }

    [Fact]
    public void Import_NoActors_ReturnsNoActors()
    {
        AreaContent content = _importer.Import(GetAssetPath("NoActors.area"), _importerContext);

        Assert.Equal("No Actors Area", content.Asset.Name);
        Assert.Equal("Tiles/GrassFourTiles.tmx", content.Asset.TileMapPath);
        Assert.Empty(content.Asset.Actors);
    }

    [Fact]
    public void Import_RelativePaths_NormalizesToAssetDirectory()
    {
        string assetPath = GetAssetPath("RelativePaths.area");
        string assetDirectory = Path.GetDirectoryName(assetPath) ?? string.Empty;

        AreaContent content = _importer.Import(assetPath, _importerContext);

        // Bare filenames are relative to the asset, whereas paths with a directory are relative to
        // the MGCB file and are left alone.
        string tileMapPath = Path.Combine(assetDirectory, "GrassFourTiles.tmx");
        string spriteSheetPath = Path.Combine(assetDirectory, "StickMan.spritesheet");

        Assert.Equal(tileMapPath, content.Asset.TileMapPath);
        Assert.Collection(content.Asset.Actors, a => Assert.Equal(spriteSheetPath, a.SpriteSheetPath));
        Assert.Equal(new[] { tileMapPath, spriteSheetPath }, _importerContext.Dependencies);
    }

    [Fact]
    public void Import_NullArea_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(
            () => _importer.Import(GetAssetPath("NullArea.area"), _importerContext));

    [Fact]
    public void Import_NullFilename_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _importer.Import(null!, _importerContext));

    [Fact]
    public void Import_NullContext_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _importer.Import(GetAssetPath("Simple.area"), null!));

    [Fact]
    public void Process_Simple_ReturnsSameContent()
    {
        AreaContent content = _importer.Import(GetAssetPath("Simple.area"), _importerContext);

        Assert.Same(content, _processor.Process(content, _processorContext));
    }

    [Fact]
    public void Process_Simple_AddsTileMapReference()
    {
        AreaContent content = _importer.Import(GetAssetPath("Simple.area"), _importerContext);

        content = _processor.Process(content, _processorContext);

        ExternalReference<TileMapContent> tileMapReference
            = content.GetReference<TileMapContent>(content.Asset.TileMapPath);

        Assert.NotNull(tileMapReference);
    }

    [Fact]
    public void Process_MultipleActors_AddsSpriteSheetReferencePerActor()
    {
        AreaContent content = _importer.Import(GetAssetPath("MultipleActors.area"), _importerContext);

        content = _processor.Process(content, _processorContext);

        foreach (AreaActorAsset actor in content.Asset.Actors)
        {
            ExternalReference<SpriteSheetContent> spriteSheetReference
                = content.GetReference<SpriteSheetContent>(actor.SpriteSheetPath);

            Assert.NotNull(spriteSheetReference);
        }
    }

    [Fact]
    public void Process_MultipleActors_BuildsEveryDependency()
    {
        AreaContent content = _importer.Import(GetAssetPath("MultipleActors.area"), _importerContext);

        var assetsBuilt = 0;
        _processorContext.AssetBuilt += (_, _) => assetsBuilt++;

        _processor.Process(content, _processorContext);

        Assert.Equal(3, assetsBuilt);
    }

    [Fact]
    public void Process_NoActors_BuildsOnlyTileMap()
    {
        AreaContent content = _importer.Import(GetAssetPath("NoActors.area"), _importerContext);

        var assetsBuilt = 0;
        _processorContext.AssetBuilt += (_, _) => assetsBuilt++;

        content = _processor.Process(content, _processorContext);

        Assert.Equal(1, assetsBuilt);
        Assert.NotNull(content.GetReference<TileMapContent>(content.Asset.TileMapPath));
    }

    [Fact]
    public void Process_SharedSpriteSheet_ActorsShareSingleReference()
    {
        AreaContent content = _importer.Import(GetAssetPath("SharedSpriteSheet.area"), _importerContext);

        var assetsBuilt = 0;
        _processorContext.AssetBuilt += (_, _) => assetsBuilt++;

        content = _processor.Process(content, _processorContext);

        // Both actors use the same sprite sheet, which is only built once and then shared between them.
        Assert.Equal(2, assetsBuilt);
        Assert.Collection(content.Asset.Actors,
                          a1 => Assert.Same(content.GetReference<SpriteSheetContent>("Images/StickMan.spritesheet"),
                                            content.GetReference<SpriteSheetContent>(a1.SpriteSheetPath)),
                          a2 => Assert.Same(content.GetReference<SpriteSheetContent>("Images/StickMan.spritesheet"),
                                            content.GetReference<SpriteSheetContent>(a2.SpriteSheetPath)));
    }

    [Fact]
    public void Process_RelativePaths_ReferencesKeyedByNormalizedPath()
    {
        AreaContent content = _importer.Import(GetAssetPath("RelativePaths.area"), _importerContext);

        content = _processor.Process(content, _processorContext);

        Assert.NotNull(content.GetReference<TileMapContent>(content.Asset.TileMapPath));

        // The path as authored in the asset file is not a valid key; the writer looks references up
        // with the normalized path the importer wrote back onto the asset.
        Assert.Throws<ArgumentException>(() => content.GetReference<TileMapContent>("GrassFourTiles.tmx"));
    }

    [Fact]
    public void Process_NullInput_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _processor.Process(null!, _processorContext));

    [Fact]
    public void Process_NullContext_ThrowsArgumentNullException()
    {
        AreaContent content = _importer.Import(GetAssetPath("Simple.area"), _importerContext);

        Assert.Throws<ArgumentNullException>(() => _processor.Process(content, null!));
    }

    private static string GetAssetPath(string assetName, [CallerFilePath] string rootPath = "")
        => $"{Path.GetDirectoryName(rootPath)}\\Content\\Areas\\{assetName}";
}
