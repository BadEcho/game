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

        string tileSetPath = Path.GetFullPath(
            Path.Combine(Path.GetDirectoryName(assetPath) ?? string.Empty, "../Grasslands.tsx"));

        Assert.Collection(content.Asset.TileSets, t => Assert.Equal(tileSetPath, t.Source));
        Assert.Equal([tileSetPath], _importerContext.Dependencies);
    }

    [Fact]
    public void Import_EmptyImageLayerSource_ThrowsPipelineException()
        => Assert.Throws<PipelineException>(
            () => _importer.Import(GetAssetPath("EmptyImageLayerSource.tmx"), _importerContext));

    [Fact]
    public void Import_GrassTransitionPoints_ParsesObjects()
    {
        ObjectLayerAsset objectLayer = ImportObjectLayer("GrassTransitionPoints.tmx");

        Assert.Equal("Transitions", objectLayer.Name);
        Assert.Equal(8, objectLayer.OffsetX);
        Assert.Equal(4, objectLayer.OffsetY);
        Assert.Equal(6, objectLayer.Objects.Count);

        MapObjectAsset northDoor = objectLayer.Objects.First();

        Assert.Equal(1, northDoor.Id);
        Assert.Equal("NorthDoor", northDoor.Name);
        Assert.Equal("TransitionPoint", northDoor.Type);
        Assert.Equal(16, northDoor.X);
        Assert.Equal(0, northDoor.Y);
        Assert.Equal(16, northDoor.Width);
        Assert.Equal(8, northDoor.Height);
        Assert.False(northDoor.IsPoint);
        Assert.True(northDoor.IsSupportedShape);
        Assert.Equal("Cave", northDoor.CustomStringProperties["TargetAreaName"]);
        Assert.Equal("SouthDoor", northDoor.CustomStringProperties["TargetPointName"]);
    }

    [Fact]
    public void Import_GrassTransitionPoints_ParsesPointObject()
    {
        MapObjectAsset shrinePortal = FindObject("GrassTransitionPoints.tmx", "ShrinePortal");

        Assert.True(shrinePortal.IsPoint);
        Assert.True(shrinePortal.IsSupportedShape);
        Assert.Equal(8, shrinePortal.X);
        Assert.Equal(24, shrinePortal.Y);
        Assert.Equal("ShrineExit", shrinePortal.CustomStringProperties["TargetPointName"]);
    }

    [Fact]
    public void Import_GrassTransitionPoints_ParsesDisabledObject()
    {
        MapObjectAsset sealedDoor = FindObject("GrassTransitionPoints.tmx", "SealedDoor");

        Assert.False(sealedDoor.CustomBoolProperties["Enabled"]);
    }

    [Fact]
    public void Import_GrassTransitionPoints_ClassOverridesType()
    {   // Tiled 1.9 renamed the object 'type' attribute to 'class'; a map carrying both is read as the newer format.
        MapObjectAsset legacyDoor = FindObject("GrassTransitionPoints.tmx", "LegacyDoor");

        Assert.Equal("TransitionPoint", legacyDoor.Type);
    }

    [Fact]
    public void Import_GrassTransitionPoints_ParsesRotationOfUnclassedObject()
    {   // Rotation is only rejected for transition points; every other object carries it through.
        MapObjectAsset signpost = FindObject("GrassTransitionPoints.tmx", "Signpost");

        Assert.Equal(90, signpost.Rotation);
    }

    [Fact]
    public void Import_GrassTransitionPoints_EllipseIsUnsupportedShape()
    {
        MapObjectAsset pond = FindObject("GrassTransitionPoints.tmx", "Pond");

        Assert.False(pond.IsSupportedShape);
    }

    [Fact]
    public void Process_GrassTransitionPoints_DropsUnsupportedShapes()
    {
        TileMapContent content = _importer.Import(GetAssetPath("GrassTransitionPoints.tmx"), _importerContext);

        content = _processor.Process(content, _processorContext);

        var objectLayer = content.Asset.Layers.OfType<ObjectLayerAsset>().Single();

        Assert.Equal(5, objectLayer.Objects.Count);
        Assert.DoesNotContain(objectLayer.Objects, o => "Pond".Equals(o.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Process_TransitionObjectNoTargetArea_ThrowsPipelineException()
        => AssertProcessThrows("TransitionObjectNoTargetArea.tmx");

    [Fact]
    public void Process_TransitionObjectNoTargetPoint_ThrowsPipelineException()
    {
        TileMapContent content = _importer.Import(GetAssetPath("TransitionObjectNoTargetPoint.tmx"), _importerContext);

        PipelineException exception
            = Assert.Throws<PipelineException>(() => _processor.Process(content, _processorContext));

        // The object is otherwise valid, so the absent wiring is the only thing left to fail on.
        Assert.Contains("TargetPointName", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Process_TransitionObjectZeroSize_ThrowsPipelineException()
        => AssertProcessThrows("TransitionObjectZeroSize.tmx");

    [Fact]
    public void Import_GrassTransitionPoints_UnrotatedObjectHasNoRotation()
    {
        MapObjectAsset northDoor = FindObject("GrassTransitionPoints.tmx", "NorthDoor");

        Assert.Equal(0, northDoor.Rotation);
    }

    [Fact]
    public void Import_TransitionObjectRotated_ParsesRotation()
    {
        MapObjectAsset tiltedDoor = FindObject("TransitionObjectRotated.tmx", "TiltedDoor");

        // The shape itself is supported; it is the rotation that the processor goes on to reject.
        Assert.Equal(45, tiltedDoor.Rotation);
        Assert.True(tiltedDoor.IsSupportedShape);
    }

    [Fact]
    public void Process_TransitionObjectRotated_ThrowsPipelineException()
    {
        TileMapContent content = _importer.Import(GetAssetPath("TransitionObjectRotated.tmx"), _importerContext);

        PipelineException exception
            = Assert.Throws<PipelineException>(() => _processor.Process(content, _processorContext));

        // The object is otherwise valid, so the rotation is the only thing left to fail on.
        Assert.Contains("rotated", exception.Message, StringComparison.Ordinal);
        Assert.Contains("45", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("GrassFourTiles.tmx")]
    [InlineData("GrassAndCollidable.tmx")]
    public void ImportProcess_NoObjectLayers_ReturnsNoObjectLayers(string assetName)
    {
        TileMapContent content = _importer.Import(GetAssetPath(assetName), _importerContext);

        content = _processor.Process(content, _processorContext);

        Assert.Empty(content.Asset.Layers.OfType<ObjectLayerAsset>());
    }

    [Fact]
    public void Process_DirectoryTileSetSource_ThrowsPipelineException()
        => AssertProcessThrows("DirectoryTileSetSource.tmx");

    [Fact]
    public void Process_DirectoryEmbeddedImage_ThrowsPipelineException()
        => AssertProcessThrows("DirectoryEmbeddedImage.tmx");

    [Fact]
    public void Process_DirectoryImageLayerSource_ThrowsPipelineException()
        => AssertProcessThrows("DirectoryImageLayerSource.tmx");

    private ObjectLayerAsset ImportObjectLayer(string assetName)
    {
        TileMapContent content = _importer.Import(GetAssetPath(assetName), _importerContext);

        return content.Asset.Layers.OfType<ObjectLayerAsset>().Single();
    }

    private MapObjectAsset FindObject(string assetName, string objectName)
        => ImportObjectLayer(assetName)
            .Objects
            .Single(o => objectName.Equals(o.Name, StringComparison.Ordinal));

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
