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
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Lighting;

/// <summary>
/// Provides a light source that emits light from a point. 
/// </summary>
public sealed class PointLight : ILight
{
    /// <inheritdoc/>
    public Vector2 Position
    { get; set; }

    /// <inheritdoc/>
    public Color Color
    { get; set; } = Color.White;

    /// <inheritdoc/>
    public int Radius
    { get; set; }

    /// <summary>
    /// Draws the light using the provided active sprite batch against the provided normal buffer.
    /// </summary>
    /// <param name="spriteBatch">An active sprite batch.</param>
    /// <param name="normalBuffer">A buffer containing normals to draw the light against.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D normalBuffer)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));

        int diameter = Radius * 2;

        var destination = new Rectangle((int) (Position.X - Radius), (int) (Position.Y - Radius), diameter, diameter);

        spriteBatch.Draw(normalBuffer, destination, Color);
    }
}
