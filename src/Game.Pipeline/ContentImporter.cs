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
using BadEcho.Extensions;
using BadEcho.Game.Pipeline.Properties;
using Microsoft.Xna.Framework.Content.Pipeline;

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
    /// <param name="dependencyName">
    /// The name of the dependency referenced by the asset; defaults to the expression <c>dependencyPath</c> was passed
    /// as, reduced to the authored property name by <see cref="DependencyName.FromExpression"/>.
    /// </param>
    /// <returns>
    /// A normalized path to the dependency located at <c>dependencyPath</c>; if <c>dependencyPath</c> includes a
    /// directory it is assumed to be relative to the MGCB file and returned as-is, otherwise it is resolved relative
    /// to <c>assetPath</c>.
    /// </returns>
    /// <remarks>
    /// This convention applies only to all of Bad Echo's own asset formats. Dependencies declared by externally authored
    /// formats must be resolved with <see cref="ResolveAssetRelativePath"/> instead.
    /// </remarks>
    protected static string NormalizeDependencyPath(string assetPath, 
                                                    string dependencyPath, 
                                                    [CallerArgumentExpression(nameof(dependencyPath))] string? dependencyName = null)
    {
        return string.IsNullOrEmpty(Path.GetDirectoryName(dependencyPath))
            ? ResolveAssetRelativePath(assetPath, dependencyPath, dependencyName)
            : dependencyPath;
    }

    /// <summary>
    /// Resolves the path to a dependency that is always expressed relative to the asset file referencing it, validating
    /// that a path was actually provided for the dependency.
    /// </summary>
    /// <param name="assetPath">The path to the asset that references the dependency.</param>
    /// <param name="dependencyPath">The path to the dependency referenced by the asset.</param>
    /// <param name="dependencyName">
    /// The name of the dependency referenced by the asset; defaults to the expression <c>dependencyPath</c> was passed
    /// as, reduced to the authored property name by <see cref="DependencyName.FromExpression"/>.
    /// </param>
    /// <returns>A path to the dependency located at <c>dependencyPath</c>, resolved relative to <c>assetPath</c>.</returns>
    /// <exception cref="PipelineException">
    /// <c>dependencyPath</c> is empty, meaning the asset omitted a path required to reference the dependency.
    /// </exception>
    /// <remarks>
    /// Used if an asset references a dependency that is always relative to itself, never to the MGCB file. Because a
    /// dependency resolved through this method is a required one, an omitted path is reported here, where the name of
    /// the missing dependency is still known, instead of during the content build phase.
    /// </remarks>
    protected static string ResolveAssetRelativePath(string assetPath, 
                                                     string dependencyPath, 
                                                     [CallerArgumentExpression(nameof(dependencyPath))]string? dependencyName = null)
    {
        return string.IsNullOrWhiteSpace(dependencyPath)
            ? throw new PipelineException(
                Strings.DependencyPathIsEmpty.InvariantFormat(DependencyName.FromExpression(dependencyName)))
            : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assetPath) ?? string.Empty, dependencyPath));
    }
}
