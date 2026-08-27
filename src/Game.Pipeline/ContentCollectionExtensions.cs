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
using BadEcho.Game.Pipeline.Atlases;
using BadEcho.Game.Pipeline.BoundedTextures;
using BadEcho.Game.Pipeline.Fonts;
using BadEcho.Game.Pipeline.SpriteSheets;
using BadEcho.Game.Pipeline.Tiles;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace BadEcho.Game.Pipeline;

/// <summary>
/// Provides a set of static methods intended to aid in defining content building rules.
/// </summary>
public static class ContentCollectionExtensions
{
    extension(ContentCollection contentCollection)
    {
        /// <summary>
        /// Marks the sprite sheets falling under the specified directory to be built.
        /// </summary>
        /// <param name="contentDirectory">Directory containing the sprite sheets to build.</param>
        public void IncludeSpriteSheets(string contentDirectory)
            => contentCollection.Include<WildcardRule>(FilterContent(contentDirectory, "spritesheet"),
                                                       // It's not entirely imperative that we provide the importer
                                                       // and processor instances here, as the framework will adhere
                                                       // to what's defined in the ContentImporterAttribute.
                                                       // But, might as well for the sake of clarity.
                                                       new SpriteSheetImporter(),
                                                       new SpriteSheetProcessor());
        /// <summary>
        /// Marks the tile maps falling under the specified directory to be built.
        /// </summary>
        /// <param name="contentDirectory">Directory containing the tile maps to build.</param>
        public void IncludeTileMaps(string contentDirectory)
            => contentCollection.Include<WildcardRule>(FilterContent(contentDirectory, "tmx"),
                                                       new TileMapImporter(),
                                                       new TileMapProcessor());
        /// <summary>
        /// Marks the areas falling under the specified directory to be built.
        /// </summary>
        /// <param name="contentDirectory">Directory containing the areas to build.</param>
        public void IncludeAreas(string contentDirectory)
            => contentCollection.Include<WildcardRule>(FilterContent(contentDirectory, "area"),
                                                       new AreaImporter(),
                                                       new AreaProcessor());
        /// <summary>
        /// Marks the distance field fonts falling under the specified directory to be built.
        /// </summary>
        /// <param name="contentDirectory">Directory containing the distance field fonts to build.</param>
        public void IncludeDistanceFieldFonts(string contentDirectory)
            => contentCollection.Include<WildcardRule>(FilterContent(contentDirectory, "sdfont"),
                                                       new DistanceFieldFontImporter(),
                                                       new DistanceFieldFontProcessor());
        /// <summary>
        /// Marks the texture atlases falling under the specified directory to be built.
        /// </summary>
        /// <param name="contentDirectory">Directory containing the texture atlases to build.</param>
        public void IncludeTextureAtlases(string contentDirectory)
            => contentCollection.Include<WildcardRule>(FilterContent(contentDirectory, "atlas"),
                                                       new TextureAtlasImporter(),
                                                       new TextureAtlasProcessor());
        /// <summary>
        /// Marks the bounded textures that match the specified pattern to be built.
        /// </summary>
        /// <param name="includePattern">A pattern to use for marking the bounded textures to build.</param>
        public void IncludeBoundedTextures(string includePattern)
            => contentCollection.Include<WildcardRule>(includePattern,
                                                       new TextureImporter(),
                                                       new BoundedTextureProcessor());
    }

    private static string FilterContent(string contentDirectory, string contentExtension) 
        => $"{Path.TrimEndingDirectorySeparator(contentDirectory)}/*.{contentExtension}";
}
