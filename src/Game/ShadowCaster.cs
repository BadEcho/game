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

using Microsoft.Xna.Framework;

namespace BadEcho.Game;

/// <summary>
/// Provides an entity that casts a shadow.
/// </summary>
public sealed class ShadowCaster
{
    public Vector2 Position
    { get; set; }

    public IList<Vector2> Points
    { get; } = [];

    public static ShadowCaster SimplePolygon(Vector2 position, float radius, int sides)
    {
        var anglePerSide = MathHelper.TwoPi / sides;
        var caster = new ShadowCaster
                     {
                         Position = position
                     };

        for (var angle = 0f; angle < MathHelper.TwoPi; angle += anglePerSide)
        {
            var pt = radius * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            caster.Points.Add(pt);
        }

        return caster;
    }
}
