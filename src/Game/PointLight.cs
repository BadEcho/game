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

namespace BadEcho.Game;

/// <summary>
/// Provides a light source that emits light from a point. 
/// </summary>
public sealed class PointLight
{
    /// <summary>
    /// Gets or sets the position of the point light.
    /// </summary>
    public Vector2 Position
    { get; set; }

    /// <summary>
    /// Gets or sets the color of the light.
    /// </summary>
    public Color Color
    { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the radius of the light.
    /// </summary>
    public int Radius
    { get; set; } = 250;

    /// <summary>
    /// Draws the light using the provided active sprite batch against the provided normal buffer.
    /// </summary>
    /// <param name="spriteBatch">An active sprite batch.</param>
    /// <param name="normalBuffer">A buffer containing normals to draw the light against.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D normalBuffer)
    {
        int diameter = Radius * 2;

        var destination = new Rectangle((int) (Position.X - Radius), (int) (Position.Y - Radius), diameter, diameter);

        spriteBatch.Draw(normalBuffer, destination, Color);
    }
}
