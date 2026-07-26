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

using BadEcho.Game.Pipeline.Areas;
using System.Runtime.CompilerServices;
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

    private static string GetAssetPath(string assetName, [CallerFilePath] string rootPath = "")
        => $"{Path.GetDirectoryName(rootPath)}\\Content\\Areas\\{assetName}";
}
