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

using System.Globalization;
using BadEcho.Game.Properties;
using Microsoft.Xna.Framework;

namespace BadEcho.Game;

/// <summary>
/// Provides parsing methods for converting various representations of a color into their <see cref="Color"/> equivalent.
/// </summary>
public static class Coloring
{
    /// <summary>
    /// Converts a color hex code string into its equivalent <see cref="Color"/> representation.
    /// </summary>
    /// <param name="colorHex">A color hex code string, with or without a leading '#' character.</param>
    /// <returns>A <see cref="Color"/> representation of <c>colorHex</c>.</returns>
    /// <exception cref="FormatException"><c>colorHex</c> is not a valid string representation of a color.</exception>
    public static Color Parse(string? colorHex)
        => TryParse(colorHex, out Color result) ? result : throw new FormatException(Strings.ColorBadHexString);

    /// <summary>
    /// Attempts to convert a color hex code string into its equivalent <see cref="Color"/> representation.
    /// </summary>
    /// <param name="colorHex">A color hex code string, with or without a leading '#' character.</param>
    /// <param name="result">
    /// When this method returns, a <see cref="Color"/> representation of <c>colorHex</c>, or a default value
    /// if the conversion failed.
    /// </param>
    /// <returns>
    /// True if <c>colorHex</c> was converted successfully; otherwise, false.
    /// </returns>
    public static bool TryParse(string? colorHex, out Color result)
        => TryParse(colorHex.AsSpan(), out result);

    /// <summary>
    /// Attempts to convert a color hex code string into its equivalent <see cref="Color"/> representation.
    /// </summary>
    /// <param name="colorHex">
    /// A span containing the characters of a hex code string, with or without a leading '#' character.
    /// </param>
    /// <param name="result">
    /// When this method returns, a <see cref="Color"/> representation of <c>colorHex</c>, or a default value
    /// if the conversion failed.
    /// </param>
    /// <returns>
    /// True if <c>colorHex</c> was converted successfully; otherwise, false.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> colorHex, out Color result)
    {
        result = new Color();
        colorHex = colorHex.Trim('#');

        if (colorHex.Length != 6 && colorHex.Length != 8)
            return false;
        
        if (!TryParseScalar(colorHex[..2], out int r))
            return false;

        if (!TryParseScalar(colorHex[2..4], out int g))
            return false;

        if (!TryParseScalar(colorHex[4..6], out int b))
            return false;

        int a = 255;

        if (colorHex.Length == 8 && !TryParseScalar(colorHex[6..8], out a))
            return false;

        result = new Color(r, g, b, a);

        return true;

        static bool TryParseScalar(ReadOnlySpan<char> scalar, out int value)
        {
            return int.TryParse(scalar, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
        }
    }
}