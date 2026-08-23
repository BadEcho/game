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

using System.Text.Json;

namespace BadEcho.Game.Pipeline;

/// <summary>
/// Provides a means of naming an asset's dependency in a way that a content author will recognize.
/// </summary>
internal static class DependencyName
{
    /// <summary>
    /// Converts the expression a dependency's path was passed as into the name of the property declaring it, as that
    /// property appears in an authored asset file.
    /// </summary>
    /// <param name="expression">
    /// The argument expression a dependency's path was passed as, typically captured by
    /// <see cref="System.Runtime.CompilerServices.CallerArgumentExpressionAttribute"/>.
    /// </param>
    /// <returns>The authored name of the dependency described by <c>expression</c>.</returns>
    /// <remarks>
    /// <para>
    /// Capturing the argument expression gets a name for free at every call site, but the raw expression is written in
    /// terms of the pipeline's own object model: a content author reading that a problem lies with
    /// <c>input.Asset.TileMapPath</c> has no way to connect it to anything they wrote. Only the last segment of the
    /// expression names the property itself, so that is all this keeps.
    /// </para>
    /// <para>
    /// The segment is then camel cased by the same policy the asset deserializer applies, which is what makes the
    /// result match the authored file. Tiled's formats need no such treatment, their attribute names already being
    /// lowercase.
    /// </para>
    /// </remarks>
    internal static string? FromExpression(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return expression;

        string name = expression[(expression.LastIndexOf('.') + 1)..]
                      .Trim()
                      .TrimEnd('!');

        return name.Length == 0
            ? expression
            : JsonNamingPolicy.CamelCase.ConvertName(name);
    }
}
