// -----------------------------------------------------------------------
// <copyright>
//		Created by Matt Weber <matt@badecho.com>
//		Copyright @ 2024 Bad Echo LLC. All rights reserved.
//
//		Bad Echo Technologies are licensed under a
//		GNU Affero General Public License v3.0.
//
//		See accompanying file LICENSE.md or a copy at:
//		https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using BadEcho.Game.Pipeline.Tiles;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Content.Pipeline;
using Xunit;

namespace BadEcho.Game.Tests;

public class TileMapPipelineTests
{
    private readonly TileMapImporter _importer = new();
    private readonly TileMapProcessor _processor = new();
    private readonly TestContentImporterContext _importerContext = new();
    private readonly TestContentProcessorContext _processorContext = new();

    [Fact]
    public void ImportProcess_Csv_ReturnsValid() 
        => ValidateTileMap("GrassCsvFormat.tmx");

    [Fact]
    public void ImportProcess_Zlib_ReturnsValid() 
        => ValidateTileMap("GrassZlibFormat.tmx");

    [Fact]
    public void ImportProcess_Gzip_ReturnsValid()
        => ValidateTileMap("GrassGzipFormat.tmx");

    [Fact]
    public void ImportProcess_UncompressedBase64_ReturnsValid()
        => ValidateTileMap("GrassUncompressedBase64Format.tmx");

    [Fact]
    public void Import_NestedGrass_ResolvesTileSetRelativeToMap()
    {
        string assetPath = GetAssetPath("Nested\\NestedGrass.tmx");

        TileMapContent content = _importer.Import(assetPath, _importerContext);

        // Tiled always writes dependency paths relative to the file they appear in, regardless of whether
        // the path includes a directory.
        string tileSetPath = Path.Combine(Path.GetDirectoryName(assetPath) ?? string.Empty, "..\\Grasslands.tsx");

        Assert.Collection(content.Asset.TileSets, t => Assert.Equal(tileSetPath, t.Source));
        Assert.Equal([tileSetPath], _importerContext.Dependencies);
    }

    [Fact]
    public void Import_EmptyImageLayerSource_ThrowsPipelineException()
        => Assert.Throws<PipelineException>(
            () => _importer.Import(GetAssetPath("EmptyImageLayerSource.tmx"), _importerContext));

    [Fact]
    public void Process_DirectoryTileSetSource_ThrowsPipelineException()
        => AssertProcessThrows("DirectoryTileSetSource.tmx");

    [Fact]
    public void Process_DirectoryEmbeddedImage_ThrowsPipelineException()
        => AssertProcessThrows("DirectoryEmbeddedImage.tmx");

    [Fact]
    public void Process_DirectoryImageLayerSource_ThrowsPipelineException()
        => AssertProcessThrows("DirectoryImageLayerSource.tmx");

    private void AssertProcessThrows(string assetName)
    {
        TileMapContent content = _importer.Import(GetAssetPath(assetName), _importerContext);

        Assert.Throws<PipelineException>(() => _processor.Process(content, _processorContext));
    }

    private void ValidateTileMap(string assetName)
    {
        TileMapContent content = _importer.Import(GetAssetPath(assetName), _importerContext);
        
        Assert.NotNull(content);
        Assert.NotNull(content.Asset);
        Assert.NotEmpty(content.Asset.Layers);
        
        content = _processor.Process(content, _processorContext);

        var tileLayer = content.Asset.Layers.First() as TileLayerAsset;

        Assert.NotNull(tileLayer);
        Assert.NotEmpty(tileLayer.Tiles);
    }

    private static string GetAssetPath(string assetName, [CallerFilePath] string rootPath = "")
        => $"{Path.GetDirectoryName(rootPath)}\\Content\\Tiles\\{assetName}";
}
