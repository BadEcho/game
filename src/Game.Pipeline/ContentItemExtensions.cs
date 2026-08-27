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

using Microsoft.Xna.Framework.Content.Pipeline;
using System.Reflection;
using BadEcho.Extensions;
using BadEcho.Game.Pipeline.Properties;

namespace BadEcho.Game.Pipeline;

/// <summary>
/// Provides a set of static methods intended to aid in matters related to content items.
/// </summary>
/// <remarks>
/// Ideally, I'd prefer to maintain file extenstion to importer type associations within the content item class
/// itself; however, static fields are not shared between closed generic types, so it is a poor place to hold that
/// kind of info.
/// </remarks>
internal static class ContentItemExtensions
{
    private static readonly Dictionary<string, Type> _FileExtensionImporterTypeMap = 
        InitializeImporterMap();

    public static IContentImporter LoadImporter(this ContentItem _, string fileExtension)
    {
        if (!_FileExtensionImporterTypeMap.TryGetValue(fileExtension, out Type? importerType))
        {
            throw new ArgumentException(Strings.NoImporterForFileExtension.InvariantFormat(fileExtension),
                                        nameof(fileExtension));

        }

        var ctor = importerType.FindConstructor()
                   ?? throw new InvalidOperationException();
        
        return (IContentImporter) ctor.Invoke(null);
    }

    private static Dictionary<string, Type> InitializeImporterMap()
    {
        List<Assembly> importerAssemblies = [typeof(TextureImporter).Assembly, typeof(IContentItem).Assembly];
        
        var importerMap = new Dictionary<string, Type>();

        var importers =
            importerAssemblies.SelectMany(asm => asm.ExportedTypes
                                                    .Where(type => !type.IsAbstract && type.IsA<IContentImporter>()))
                              .Select(importerType => (Type: importerType, 
                                                       Attribute: importerType.GetAttribute<ContentImporterAttribute>()));
        foreach (var importer in importers)
        {
            if (importer.Attribute == null)
                continue;

            foreach (var fileExtension in importer.Attribute.FileExtensions)
            {
                importerMap[fileExtension] = importer.Type;
            }
        }

        return importerMap;
    }
}
