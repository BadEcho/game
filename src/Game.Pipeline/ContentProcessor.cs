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
/// Provides a game content processor that comes with some additional helper methods that aid with processing-related concerns.
/// </summary>
/// <typeparam name="T">The type of content processed by this processor.</typeparam>
public abstract class ContentProcessor<T> : ContentProcessor<T,T>
{
    /// <summary>
    /// Ensures that the path to an asset's dependency doesn't point to a directory, as this would lead to a confusing error
    /// during the content build phase.
    /// </summary>
    /// <param name="dependencyPath">The path to the asset's dependency.</param>
    /// <param name="dependencyName">
    /// The name of the asset's dependency; defaults to the expression <c>dependencyPath</c> was passed as, reduced to
    /// the authored property name by <see cref="DependencyName.FromExpression"/>.
    /// </param>
    /// <exception cref="PipelineException"><c>dependencyPath</c> points to a directory instead of a file.</exception>
    protected static void ValidateDependencyPath(string dependencyPath, [CallerArgumentExpression(nameof(dependencyPath))]string? dependencyName = null)
    {
        if (Directory.Exists(dependencyPath) || Path.EndsInDirectorySeparator(dependencyPath))
        {
            throw new PipelineException(
                Strings.DependencyFileIsADirectory.InvariantFormat(DependencyName.FromExpression(dependencyName),
                                                                  dependencyPath));
        }
    }
}
