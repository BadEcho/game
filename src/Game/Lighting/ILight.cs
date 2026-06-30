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
/// Defines a light source.
/// </summary>
public interface ILight
{
    /// <summary>
    /// Gets or sets the position of the point light.
    /// </summary>
    Vector2 Position { get; set; }

    /// <summary>
    /// Gets or sets the color of the light.
    /// </summary>
    Color Color { get; set; }

    /// <summary>
    /// Gets or sets the radius of the light.
    /// </summary>
    int Radius { get; set; }
    
    /// <summary>
    /// Draws the light using the provided active sprite batch against the provided normal buffer.
    /// </summary>
    /// <param name="spriteBatch">An active sprite batch.</param>
    /// <param name="normalBuffer">A buffer containing normals to draw the light against.</param>
    void Draw(SpriteBatch spriteBatch, Texture2D normalBuffer);
}
