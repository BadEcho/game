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

namespace BadEcho.Game.Pipeline;

/// <summary>
/// Provides a file format importer for use with game assets that comes with some additional helper methods that aid
/// with import-related concerns.
/// </summary>
/// <typeparam name="T">The type of <see cref="IContentItem"/> imported by the importer.</typeparam>
public abstract class ContentImporter<T> : Microsoft.Xna.Framework.Content.Pipeline.ContentImporter<T>
    where T : IContentItem
{
    /// <summary>
    /// Normalizes the path to a dependency referenced by an asset so that it can be resolved relative to the
    /// correct base directory.
    /// </summary>
    /// <param name="assetPath">The path to the asset that references the dependency.</param>
    /// <param name="dependencyPath">The path to the dependency referenced by the asset.</param>
    /// <returns>
    /// A normalized path to the dependency located at <c>dependencyPath</c>; if <c>dependencyPath</c> includes a
    /// directory it is assumed to be relative to the MGCB file and returned as-is, otherwise it is resolved relative
    /// to <c>assetPath</c>.
    /// </returns>
    protected static string NormalizeDependencyPath(string assetPath, string dependencyPath)
    {
        return string.IsNullOrEmpty(Path.GetDirectoryName(dependencyPath))
            ? Path.Combine(Path.GetDirectoryName(assetPath) ?? string.Empty, dependencyPath)
            : dependencyPath;
    }
}
